using LASYS.Application.Features.LabelProcessing.GetWorkOrders;
using LASYS.DesktopApp.Core.Interfaces;
using LASYS.DesktopApp.Events;
using LASYS.DesktopApp.Views.Interfaces;
using LASYS.DesktopApp.Views.UserControls;
using LASYS.DesktopApp.Views.Forms;
using LASYS.Infrastructure.Services.WorkOrder;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using LASYS.Application.Common.Enums;

namespace LASYS.DesktopApp.Presenters
{
    public class WorkOrdersPresenter
    {
        private readonly IWorkOrdersView _view;
        private readonly IMainView _mainView;
        private readonly IServiceProvider _serviceProvider;
        private readonly IMediator _mediator;
        private readonly WorkOrderService _workOrderService;
        public UserControl View { get; }
        private bool _isLoading;
        private int _currentPage = 1;
        private string _searchTerm = string.Empty;

        public WorkOrdersPresenter(IWorkOrdersView view,
                                   IMainView mainView,
                                   IServiceProvider serviceProvider,
                                   IMediator mediator,
                                   WorkOrderService workOrderService)
        {
            View = (UserControl)view;
            _view = view;
            _mainView = mainView;
            _serviceProvider = serviceProvider;
            _mediator = mediator;
            _workOrderService = workOrderService;

            _view.LabelPrintingRequested += OnLabelPrintingRequested;
            _view.PageNoChanged += OnPageNoChanged;
            _view.SearchTermChanged += OnSearchTermChanged;

            _ = LoadWorkOrdersAsync();
        }

        private async void OnSearchTermChanged(object? sender, string e)
        {
            if (_isLoading) return;

            _searchTerm = e?.Trim() ?? string.Empty;
            _currentPage = 1;
            _isLoading = true;
            _view.SetLoading(true);

            try
            {
                var result = await _mediator.Send(new GetWorkOrdersQuery(_searchTerm, 50, _currentPage));

                if (result.IsSuccess && result.Value?.Items == null) return;

                if (result.Value?.Items != null && result.Value.Items.Count > 0)
                {
                    _view.SetWorkOrders(result.Value.Items, result.Value.TotalPages);
                }
            }
            finally
            {
                _isLoading = false;
                _view.SetLoading(false);
            }
        }

        private async void OnLabelPrintingRequested(object? sender, LabelPrintingRequestedEventArgs e)
        {
            var labelBoxTypePresenter = _serviceProvider.GetRequiredService<LabelBoxTypePresenter>();

            if (string.IsNullOrWhiteSpace(e.ItemCode) || string.IsNullOrWhiteSpace(e.LotNo))
            {
                _view.ShowNotification("Item Code or Lot Number is missing.", "Error", MessageBoxIcon.Error);
                return;
            }

            var result = labelBoxTypePresenter.Show(
                hasCaseLabel: HasInstructionCode(e.UbLiCode),
                hasUnitBox: HasInstructionCode(e.UbLiCode),
                hasAdditionalUnitBox: HasInstructionCode(e.AubLiCode),
                hasOuterUnitBox: HasInstructionCode(e.OubLiCode),
                hasCartonBox: HasInstructionCode(e.CbLiCode),
                hasOuterCartonBox: HasInstructionCode(e.OcbLiCode),
                hasAdditionalCartonBox: HasInstructionCode(e.AcbLiCode));

            if (result == null)
            {
                _view.ShowNotification("No printable label instruction code is available for this work order.",
                                       "No Printable Type",
                                       MessageBoxIcon.Warning);
                return;
            }

            var labelType = MapToDomainBoxType(result.Value);
            var instructionCode = ResolveInstructionCode(result.Value, e);

            if (string.IsNullOrWhiteSpace(instructionCode))
            {
                _view.ShowNotification("The selected box type has no instruction code.",
                                       "Missing Instruction",
                                       MessageBoxIcon.Warning);
                return;
            }

            var labelStatus = ResolveLabelStatus(e.PrintType);
            var printData = await _workOrderService.BuildPrintDataAsync(
                e.ItemCode,
                e.LotNo,
                instructionCode,
                labelType,
                labelStatus);

            var labelPrintingPresenter = _serviceProvider.GetRequiredService<LabelPrintingPresenter>();
            labelPrintingPresenter.InitializePrintData(printData);
            _mainView?.LoadView(labelPrintingPresenter.View, false); // always new

            // Load the label template to enable print button
            // TODO: Update GetLabelTemplateHandler to use ItemCode/LotNo/InstructionCode instead of WorkOrderId
            await labelPrintingPresenter.InitializeTemplateAsync(0);
        }

        private static bool HasInstructionCode(string? code)
        {
            return !string.IsNullOrWhiteSpace(code);
        }

        private static BoxType MapToDomainBoxType(LabelBoxType boxType)
        {
            return boxType switch
            {
                LabelBoxType.CaseLabel => BoxType.CaseLabel,
                LabelBoxType.UnitBox => BoxType.UnitBox,
                LabelBoxType.AdditionalUnitBox => BoxType.AdditionalUnitBox,
                LabelBoxType.OuterUnitBox => BoxType.OuterUnitBox,
                LabelBoxType.CartonBox => BoxType.CartonBox,
                LabelBoxType.OuterCartonBox => BoxType.OuterCartonBox,
                LabelBoxType.AdditionalCartonBox => BoxType.AdditionalCartonBox,
                _ => BoxType.NotSet,
            };
        }

        private static string ResolveInstructionCode(LabelBoxType boxType, LabelPrintingRequestedEventArgs args)
        {
            return boxType switch
            {
                LabelBoxType.CaseLabel => args.UbLiCode ?? string.Empty,
                LabelBoxType.UnitBox => args.UbLiCode ?? string.Empty,
                LabelBoxType.AdditionalUnitBox => args.AubLiCode ?? string.Empty,
                LabelBoxType.OuterUnitBox => args.OubLiCode ?? string.Empty,
                LabelBoxType.CartonBox => args.CbLiCode ?? string.Empty,
                LabelBoxType.OuterCartonBox => args.OcbLiCode ?? string.Empty,
                LabelBoxType.AdditionalCartonBox => args.AcbLiCode ?? string.Empty,
                _ => string.Empty,
            };
        }

        private static LabelPrintType ResolveLabelStatus(string? printType)
        {
            if (string.IsNullOrWhiteSpace(printType))
                return LabelPrintType.Original;

            return printType.Trim().ToLowerInvariant() switch
            {
                "original" => LabelPrintType.Original,
                "additional" => LabelPrintType.Additional,
                "replacement" => LabelPrintType.Replacement,
                "return" or "returned" => LabelPrintType.Returned,
                "excess" => LabelPrintType.Excess,
                "qc" => LabelPrintType.QC,
                "coc" => LabelPrintType.COC,
                _ => LabelPrintType.Original,
            };
        }

        private async void OnPageNoChanged(object? sender, int pageNo)
        {
            if (_isLoading) return;

            _isLoading = true;
            _view.SetLoading(true);
            _currentPage = pageNo;

            try
            {
                var result = await _mediator.Send(new GetWorkOrdersQuery(_searchTerm, 50, pageNo));

                if (result.IsSuccess && result.Value?.Items == null) return;

                if (result.Value?.Items != null && result.Value.Items.Count > 0)
                {
                    _view.SetWorkOrders(result.Value.Items, result.Value.TotalPages);
                }
            }
            finally
            {
                _isLoading = false;
                _view.SetLoading(false);
            }
        }

        private async Task LoadWorkOrdersAsync()
        {
            if (_isLoading)
                return;

            _isLoading = true;
            _view.SetLoading(true);

            try
            {
                var result = await _mediator.Send(new GetWorkOrdersQuery(_searchTerm, 50, 1));

                if (result.Value?.Items != null && result.Value.Items.Count > 0)
                {
                    _view.SetWorkOrders(result.Value.Items, result.Value.TotalPages);
                }
            }
            finally
            {
                _isLoading = false;
                _view.SetLoading(false);
            }
        }
    }
}
