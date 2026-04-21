using BenchmarkDotNet.Attributes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace BenchmarkSuite1.Benchmarks
{
    [MemoryDiagnoser]
    public class WorkOrdersSetWorkOrdersBenchmark
    {
        private object _control = null!;
        private MethodInfo _setWorkOrders = null!;
        private object _rows = null!;
        private Type _getWorkOrdersResultType = null!;

        [GlobalSetup]
        public void GlobalSetup()
        {
            var desktopAppAssembly = LoadAssembly("LASYS.DesktopApp");
            var applicationAssembly = LoadAssembly("LASYS.Application");

            _getWorkOrdersResultType = applicationAssembly.GetType("LASYS.Application.Features.LabelProcessing.GetWorkOrders.GetWorkOrdersResult", throwOnError: true)!;
            var controlType = desktopAppAssembly.GetType("LASYS.DesktopApp.Views.UserControls.WorkOrdersControl", throwOnError: true)!;

            _control = Activator.CreateInstance(controlType)!;
            _setWorkOrders = controlType.GetMethod("SetWorkOrders") ?? throw new InvalidOperationException("SetWorkOrders not found.");

            var listType = typeof(List<>).MakeGenericType(_getWorkOrdersResultType);
            _rows = Activator.CreateInstance(listType)!;
            var addMethod = listType.GetMethod("Add") ?? throw new InvalidOperationException("Add method not found.");

            for (int i = 0; i < 250; i++)
            {
                var row = Activator.CreateInstance(_getWorkOrdersResultType)!;
                SetProperty(row, "ItemCode", $"ITEM-{i:0000}");
                SetProperty(row, "LotNo", $"LOT-{i:0000}");
                SetProperty(row, "ExpDate", "2026-12-31");
                SetProperty(row, "PrintType", i % 2 == 0 ? "Original" : "Additional");
                SetProperty(row, "Verdict", i % 3 == 0 ? "OK" : "Pending");
                SetProperty(row, "DateApproved", "04/20/2026");
                SetProperty(row, "ProdQty", 1200);
                SetProperty(row, "MasterLabelRevisionNo", 3);
                SetProperty(row, "LabelInsRevisionNo", 7);
                SetProperty(row, "UB_Qty", 100);
                SetProperty(row, "UB_LI_Status", "Printed");
                SetProperty(row, "AUB_Qty", 25);
                SetProperty(row, "AUB_LI_Status", "Pending");
                SetProperty(row, "OUB_Qty", 10);
                SetProperty(row, "OUB_LI_Status", "Pending");
                addMethod.Invoke(_rows, new[] { row });
            }

            controlType.GetMethod("CreateControl", Type.EmptyTypes)?.Invoke(_control, null);
        }

        [Benchmark]
        public void SetWorkOrders250()
        {
            _setWorkOrders.Invoke(_control, new object[] { _rows, 10 });
        }

        private static void SetProperty(object target, string propertyName, object value)
        {
            target.GetType().GetProperty(propertyName)?.SetValue(target, value);
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
    }
}