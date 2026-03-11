using LASYS.Application.Common.Results;
using LASYS.Application.Features.LabelProcessing.Abstractions;
using LASYS.Application.Interfaces;
using MediatR;

namespace LASYS.Application.Features.LabelProcessing.LoadLabelTemplate
{
    public class LoadLabelTemplateCommandHandler : IRequestHandler<LoadLabelTemplateCommand, Result>
    {
        private readonly ILabelProcessingService _labelProcessingService;
        private readonly IWorkOrderRepository _workOrderRepository;
        public LoadLabelTemplateCommandHandler(ILabelProcessingService labelProcessingService,
                                               IWorkOrderRepository workOrderRepository)
        {
            _labelProcessingService = labelProcessingService;
            _workOrderRepository = workOrderRepository;
        }

        public async Task<Result> Handle(LoadLabelTemplateCommand request, CancellationToken cancellationToken)
        {
            var templatePath = await _workOrderRepository.GetTemplatePathAsync(request.WorkOrderId);
            if (string.IsNullOrEmpty(templatePath))
            {
                return Result.Failure($"No template path found for WorkOrderId: {request.WorkOrderId}");
            }

             _labelProcessingService.LoadLabelTemplateAsync(templatePath);
            return Result.Success();
        }
    }
}
