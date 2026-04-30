using System.IO;
using LASYS.Application.Common.Results;
using LASYS.Application.Features.LabelProcessing.Utilities;
using LASYS.Application.Interfaces.Persistence;
using LASYS.Application.Interfaces.Services;
using MediatR;

namespace LASYS.Application.Features.OCRCalibration.PrintLabel
{
    public class PrintLabelHandler : IRequestHandler<PrintLabelCommand, Result<Unit>>
    {
        private readonly IDbConnectionFactory _factory;
        private readonly IPrinterService _printerService;

        public PrintLabelHandler(IDbConnectionFactory factory, IPrinterService printerService)
        {
            _factory = factory;
            _printerService = printerService;
        }

        public async Task<Result<Unit>> Handle(PrintLabelCommand request, CancellationToken cancellationToken)
        {
            var itemCode = request.ItemCode;
            var revisionNumber = request.RevisionNumber;
            var boxType = request.BoxType;
            var filePath = request.FilePath;

            //await _printerService.InitializeAsync();

            //if (string.IsNullOrWhiteSpace(request.FilePath) || !File.Exists(request.FilePath))
            //{
            //    return Result.Failure<Unit>("Label template file not found.");
            //}
            //_printerService.LoadLabelTemplate(request.FilePath);

            var sampleTemplatePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources","Sample Label Templates", "CB sample W MANUFACTURE Vbscript.lbl");

            if (!File.Exists(sampleTemplatePath))
            {
                return Result.Failure<Unit>("Sample label template file not found.");
            }
            _printerService.LoadLabelTemplate(sampleTemplatePath);

        
            int sequenceNo = 1;
            var formattedSequence = SequenceFormatter.Format(sequenceNo, 6);

            var labelData = new Dictionary<string, string>
            {

                { "LOT_NO","250530X" },
                { "GAUGE", "20G x 1 1/4\"" },
                { "MANUFACTURE_DATE","2025-05-30" },
                { "BARCODE_MANUFACTURE_DATE", "250530" },
                { "EXPIRY_DATE","2030-05-31" },
                { "BARCODE_EXPIRY_DATE", "300531" },
                { "BARCODE_NO", "3498735074357" },
                { "BOX_NO",$"{formattedSequence}" },
            };


            _printerService.SetLabelVariables(labelData);

            bool generated = _printerService.PrintLabelWithPreview($"LBL_{formattedSequence}");
            if (!generated)
            {
                return Result.Failure<Unit>("Failed to generate label preview.");
            }

            _printerService.Print();
            return Result.Success(Unit.Value);
        }
    }
}
