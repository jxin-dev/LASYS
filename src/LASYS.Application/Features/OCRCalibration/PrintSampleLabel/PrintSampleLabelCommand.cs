using LASYS.Application.Common.Results;
using LASYS.Application.Features.LabelInstructions.GetLabelInstructionContext;
using MediatR;

namespace LASYS.Application.Features.OCRCalibration.PrintLabel
{
    public record PrintSampleLabelCommand(string ItemCode, uint MasterRevision, string BoxType, string FilePath) : IRequest<Result<LabelPrintingContext>>;
}
