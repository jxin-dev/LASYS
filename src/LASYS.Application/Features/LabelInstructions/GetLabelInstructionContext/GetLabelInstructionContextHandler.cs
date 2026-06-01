using LASYS.Application.Common.Results;
using LASYS.Application.Interfaces.Persistence.Repositories;
using LASYS.Application.Interfaces.Services;
using MediatR;

namespace LASYS.Application.Features.LabelInstructions.GetLabelInstructionContext
{
    public sealed class GetLabelInstructionContextHandler : IRequestHandler<GetLabelInstructionContextQuery, Result<LabelPrintingContext>>
    {
        private readonly IProductRepository _productRepository;
        private readonly IMasterLabelRepository _masterLabelRepository;
        private readonly ILabelInstructionRepository _labelInstructionRepository;
        private readonly IPrintLabelRepository _printLabelRepository;

        //private readonly INiceLabelTemplateService _niceLabelTemplateService;

        public GetLabelInstructionContextHandler(IProductRepository productRepository, IMasterLabelRepository masterLabelRepository, ILabelInstructionRepository labelInstructionRepository, IPrintLabelRepository printLabelRepository)
        {
            _productRepository = productRepository;
            _masterLabelRepository = masterLabelRepository;
            _labelInstructionRepository = labelInstructionRepository;
            _printLabelRepository = printLabelRepository;
        }

        public async Task<Result<LabelPrintingContext>> Handle(GetLabelInstructionContextQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var labelInstructionTask = _labelInstructionRepository.GetDetailsAsync(request.ItemCode, request.LotNo, request.MasterRevision, request.BoxType);
                var productTask = _productRepository.GetDetailsAsync(request.ItemCode, request.MasterRevision, request.BoxType);
                var masterLabelTask = _masterLabelRepository.GetDetailsAsync(request.ItemCode, request.MasterRevision, request.BoxType);
                var printDetailsTask = _printLabelRepository.GetDetailsAsync(request.ItemCode, request.LotNo, request.BoxType);


                await Task.WhenAll(labelInstructionTask, productTask, masterLabelTask, printDetailsTask);

                var context = new LabelPrintingContext
                {
                    LabelInstructionDetails = await labelInstructionTask,
                    ProductDetails = await productTask,
                    MasterLabelDetails = await masterLabelTask,
                    PrintDetails = await printDetailsTask
                };
                return Result.Success(context);
            }
            catch (Exception ex)
            {
                return Result.Failure<LabelPrintingContext>($"An error occurred while retrieving label instruction context: {ex.Message}");
            }

        }
    }
}
