using LASYS.Application.Common.Enums;
using LASYS.Application.Common.Results;
using LASYS.Application.Features.LabelInstructions.GetLabelInstructionContext;
using LASYS.Application.Interfaces.Persistence.Repositories;
using MediatR;

namespace LASYS.Application.Features.OCRCalibration.PrintLabel
{
    public class PrintSampleLabelHandler : IRequestHandler<PrintSampleLabelCommand, Result<LabelPrintingContext>>
    {
        //private readonly IDbConnectionFactory _factory;
        //private readonly IPrinterService _printerService;
        //private readonly INiceLabelTemplateService _niceLabelTemplateService;

        //public PrintSampleLabelHandler(IDbConnectionFactory factory, IPrinterService printerService, INiceLabelTemplateService niceLabelTemplateService)
        //{
        //    _factory = factory;
        //    _printerService = printerService;
        //    _niceLabelTemplateService = niceLabelTemplateService;
        //}

        private readonly IProductRepository _productRepository;
        private readonly IMasterLabelRepository _masterLabelRepository;
        private readonly ILabelInstructionRepository _labelInstructionRepository;

        public PrintSampleLabelHandler(IProductRepository productRepository, IMasterLabelRepository masterLabelRepository, ILabelInstructionRepository labelInstructionRepository)
        {
            _productRepository = productRepository;
            _masterLabelRepository = masterLabelRepository;
            _labelInstructionRepository = labelInstructionRepository;
        }

        public async Task<Result<LabelPrintingContext>> Handle(PrintSampleLabelCommand request, CancellationToken cancellationToken)
        {
            try
            {
                BoxType boxType = request.BoxType switch
                {
                    "UB" => BoxType.UnitBox,
                    "CB" => BoxType.CartonBox,
                    "OUB" => BoxType.OuterUnitBox,
                    "OCB" => BoxType.OuterCartonBox,
                    "AUB" => BoxType.AdditionalUnitBox,
                    "ACB" => BoxType.AdditionalCartonBox,
                    _ => throw new ArgumentException($"Invalid box type: {request.BoxType}")
                };

                var labelInstruction = await _labelInstructionRepository.GetDetailsAsync(request.ItemCode, request.MasterRevision, boxType);
                var productTask = _productRepository.GetDetailsAsync(request.ItemCode, request.MasterRevision, boxType);
                var masterLabelTask = _masterLabelRepository.GetDetailsAsync(request.ItemCode, request.MasterRevision, boxType);
                await Task.WhenAll(productTask, masterLabelTask);

                var context = new LabelPrintingContext
                {
                    LabelInstructionDetails = labelInstruction,
                    ProductDetails = productTask.Result,
                    MasterLabelDetails = masterLabelTask.Result
                };
                return Result.Success(context);
            }
            catch (Exception ex)
            {
                return Result.Failure<LabelPrintingContext>($"An error occurred while retrieving label context: {ex.Message}");
            }
        }


        //public async Task<Result<Unit>> Handle(PrintSampleLabelCommand request, CancellationToken cancellationToken)
        //{
        //    var itemCode = request.ItemCode;
        //    var revisionNumber = request.RevisionNumber;
        //    var boxType = request.BoxType;
        //    var filePath = request.FilePath;

        //    //await _printerService.InitializeAsync();

        //    //if (string.IsNullOrWhiteSpace(request.FilePath) || !File.Exists(request.FilePath))
        //    //{
        //    //    return Result.Failure<Unit>("Label template file not found.");
        //    //}
        //    //_printerService.LoadLabelTemplate(request.FilePath);

        //    var sampleTemplatePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources","Sample Label Templates", "CB sample W MANUFACTURE Vbscript.lbl");

        //    if (!File.Exists(sampleTemplatePath))
        //    {
        //        return Result.Failure<Unit>("Sample label template file not found.");
        //    }
        //    _niceLabelTemplateService.LoadTemplate(sampleTemplatePath);

        //    int sequenceNo = 1;
        //    var formattedSequence = SequenceFormatter.Format(sequenceNo, 6);

        //    var labelData = new Dictionary<string, string>
        //    {

        //        { "LOT_NO","250530X" },
        //        { "GAUGE", "20G x 1 1/4\"" },
        //        { "MANUFACTURE_DATE","2025-05-30" },
        //        { "BARCODE_MANUFACTURE_DATE", "250530" },
        //        { "EXPIRY_DATE","2030-05-31" },
        //        { "BARCODE_EXPIRY_DATE", "300531" },
        //        { "BARCODE_NO", "3498735074357" },
        //        { "BOX_NO",$"{formattedSequence}" },
        //    };

        //    _niceLabelTemplateService.SetVariables(labelData);

        //    string baseDir = AppDomain.CurrentDomain.BaseDirectory;
        //    string dirPrn = Path.Combine(baseDir, "prns");
        //    string dirImage = Path.Combine(baseDir, "images");

        //    bool generated = _niceLabelTemplateService.GeneratePreview(dirImage, $"LBL_{formattedSequence}");
        //    if (!generated)
        //    {
        //        return Result.Failure<Unit>("Failed to generate label preview.");
        //    }

        //    bool isPrnGenerated = _niceLabelTemplateService.GeneratePrn(dirPrn, $"LBL_{formattedSequence}", out var prnPath);
        //    var templateVariables = _niceLabelTemplateService.GetTemplateVariables();
        //    await _printerService.IsPrinted(prnPath);
        //    return Result.Success(Unit.Value);
        //}
    }
}
