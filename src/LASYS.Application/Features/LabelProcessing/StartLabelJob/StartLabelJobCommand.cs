using System.Drawing;
using MediatR;

namespace LASYS.Application.Features.LabelProcessing.StartLabelJob
{
    public class StartLabelJobCommand : IRequest
    {
        public Size ViewerSize { get; init; }
        public string ItemCode { get; init; } = string.Empty;
        public int StartSequence { get; init; }
        public int Quantity { get; init; }
        public Dictionary<string, string> LabelData { get; init; } = new();
        public string SequenceVariableName { get; init; } = "SEQUENCE";
        public string BarcodeVariableName { get; init; } = "BARCODE";
    }
}
