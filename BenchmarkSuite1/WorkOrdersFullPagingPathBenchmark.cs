using BenchmarkDotNet.Attributes;
using MediatR;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace BenchmarkSuite1.Benchmarks
{
    [MemoryDiagnoser]
    public class WorkOrdersFullPagingPathBenchmark
    {
        private object _presenter = null!;
        private MethodInfo _handlePageNoChangedAsync = null!;
        private FakeMediator _mediator = null!;

        [GlobalSetup]
        public void GlobalSetup()
        {
            var desktopAppAssembly = LoadAssembly("LASYS.DesktopApp");
            var infrastructureAssembly = LoadAssembly("LASYS.Infrastructure");
            var applicationAssembly = LoadAssembly("LASYS.Application");

            var presenterType = desktopAppAssembly.GetType("LASYS.DesktopApp.Presenters.WorkOrdersPresenter", throwOnError: true)!;
            var viewType = desktopAppAssembly.GetType("LASYS.DesktopApp.Views.UserControls.WorkOrdersControl", throwOnError: true)!;
            var mainViewInterfaceType = desktopAppAssembly.GetType("LASYS.DesktopApp.Views.Interfaces.IMainView", throwOnError: true)!;
            var workOrderServiceType = infrastructureAssembly.GetType("LASYS.Infrastructure.Services.WorkOrder.WorkOrderService", throwOnError: true)!;
            var workOrderRepositoryInterfaceType = applicationAssembly.GetType("LASYS.Application.Interfaces.Persistence.Repositories.IWorkOrderRepository", throwOnError: true)!;
            var logServiceInterfaceType = applicationAssembly.GetType("LASYS.Application.Interfaces.Services.ILogService", throwOnError: true)!;

            var view = Activator.CreateInstance(viewType)!;
            var mainView = CreateNoOpProxy(mainViewInterfaceType);
            var logService = CreateNoOpProxy(logServiceInterfaceType);
            var workOrderRepository = CreateNoOpProxy(workOrderRepositoryInterfaceType);
            var workOrderService = Activator.CreateInstance(workOrderServiceType, new object[] { workOrderRepository })!;

            _mediator = new FakeMediator(applicationAssembly);

            var ctor = presenterType.GetConstructors().Single();
            _presenter = ctor.Invoke(new object?[]
            {
                view,
                mainView,
                new NullServiceProvider(),
                _mediator,
                workOrderService,
                logService
            });

            _handlePageNoChangedAsync = presenterType.GetMethod("HandlePageNoChangedAsync", BindingFlags.Instance | BindingFlags.NonPublic)
                ?? throw new InvalidOperationException("HandlePageNoChangedAsync not found.");

            SpinWait.SpinUntil(() => _mediator.CallCount >= 1, 1000);
        }

        [Benchmark]
        public void TriggerNextPage()
        {
            var task = (Task)_handlePageNoChangedAsync.Invoke(_presenter, new object[] { 2 })!
                ?? throw new InvalidOperationException("Task retrieval failed.");
            task.GetAwaiter().GetResult();
        }

        private static object CreateNoOpProxy(Type interfaceType)
        {
            var createMethod = typeof(DispatchProxy)
                .GetMethods(BindingFlags.Public | BindingFlags.Static)
                .Single(m => m.Name == "Create" && m.IsGenericMethodDefinition && m.GetGenericArguments().Length == 2);

            return createMethod.MakeGenericMethod(interfaceType, typeof(NoOpProxy)).Invoke(null, null)!;
        }

        private class NoOpProxy : DispatchProxy
        {
            protected override object? Invoke(MethodInfo? targetMethod, object?[]? args)
            {
                if (targetMethod == null)
                    return null;

                if (targetMethod.ReturnType == typeof(void))
                    return null;

                return targetMethod.ReturnType.IsValueType ? Activator.CreateInstance(targetMethod.ReturnType) : null;
            }
        }

        private static Assembly LoadAssembly(string assemblyName)
        {
            try
            {
                return Assembly.Load(assemblyName);
            }
            catch
            {
                var path = Path.Combine(AppContext.BaseDirectory, assemblyName + ".dll");
                return Assembly.LoadFrom(path);
            }
        }

        private sealed class FakeMediator : IMediator
        {
            private readonly object _result;
            public int CallCount { get; private set; }

            public FakeMediator(Assembly applicationAssembly)
            {
                var resultType = applicationAssembly.GetType("LASYS.Application.Common.Results.Result", throwOnError: true)!;
                var paginatedListOpenType = applicationAssembly.GetType("LASYS.Application.Common.Pagination.PaginatedList`1", throwOnError: true)!;
                var getWorkOrdersResultType = applicationAssembly.GetType("LASYS.Application.Features.LabelProcessing.GetWorkOrders.GetWorkOrdersResult", throwOnError: true)!;

                var items = CreateItems(getWorkOrdersResultType);
                var paginatedListType = paginatedListOpenType.MakeGenericType(getWorkOrdersResultType);
                var paginatedList = Activator.CreateInstance(paginatedListType, new object[] { items, 2709, 1, 50 })!;

                var successMethod = resultType.GetMethods(BindingFlags.Public | BindingFlags.Static)
                    .Single(m => m.Name == "Success" && m.IsGenericMethodDefinition && m.GetParameters().Length == 1);

                _result = successMethod.MakeGenericMethod(paginatedListType).Invoke(null, new[] { paginatedList })!;
            }

            public Task Send<TRequest>(TRequest request, CancellationToken cancellationToken = default) where TRequest : IRequest
            {
                CallCount++;
                return Task.CompletedTask;
            }

            public Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
            {
                CallCount++;
                return Task.FromResult((TResponse)_result);
            }

            public Task<object?> Send(object request, CancellationToken cancellationToken = default)
            {
                CallCount++;
                return Task.FromResult<object?>(_result);
            }

            public IAsyncEnumerable<TResponse> CreateStream<TResponse>(IStreamRequest<TResponse> request, CancellationToken cancellationToken = default)
                => Empty<TResponse>();

            public IAsyncEnumerable<object?> CreateStream(object request, CancellationToken cancellationToken = default)
                => Empty<object?>();

            public Task Publish(object notification, CancellationToken cancellationToken = default) => Task.CompletedTask;
            public Task Publish<TNotification>(TNotification notification, CancellationToken cancellationToken = default) where TNotification : INotification => Task.CompletedTask;

            private static object CreateItems(Type getWorkOrdersResultType)
            {
                var listType = typeof(List<>).MakeGenericType(getWorkOrdersResultType);
                var list = Activator.CreateInstance(listType)!;
                var addMethod = listType.GetMethod("Add") ?? throw new InvalidOperationException("Add method not found.");

                for (int i = 0; i < 50; i++)
                {
                    var item = Activator.CreateInstance(getWorkOrdersResultType)!;
                    SetProperty(item, "ItemCode", $"ITEM-{i:0000}");
                    SetProperty(item, "LotNo", $"LOT-{i:0000}");
                    SetProperty(item, "ExpDate", "2026-12-31");
                    SetProperty(item, "PrintType", "Original");
                    SetProperty(item, "Verdict", "OK");
                    SetProperty(item, "DateApproved", "04/20/2026");
                    SetProperty(item, "ProdQty", 1200);
                    SetProperty(item, "MasterLabelRevisionNo", 3);
                    SetProperty(item, "LabelInsRevisionNo", 7);
                    addMethod.Invoke(list, new[] { item });
                }

                return list;
            }

            private static void SetProperty(object target, string propertyName, object value)
            {
                target.GetType().GetProperty(propertyName)?.SetValue(target, value);
            }

            private static async IAsyncEnumerable<T> Empty<T>()
            {
                yield break;
            }
        }

        private sealed class NullServiceProvider : IServiceProvider
        {
            public object? GetService(Type serviceType) => null;
        }
    }
}