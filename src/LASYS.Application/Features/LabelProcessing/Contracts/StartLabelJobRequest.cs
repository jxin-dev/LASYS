namespace LASYS.Application.Features.LabelProcessing.Contracts
{
    public class StartLabelJobRequest
    {
        public string ItemCode { get; set; } = string.Empty;

        public int StartSequence { get; set; }

        public int Quantity { get; set; } = 1;

        // Fully dynamic label variables
        public Dictionary<string, string> LabelData { get; set; } = new();

        // Tell system which key is sequence variable
        public string SequenceVariableName { get; set; } = "SEQUENCE";
        // Tell system which key is barcode variable
        public string BarcodeVariableName { get; set; } = "BARCODE";
    }
}
