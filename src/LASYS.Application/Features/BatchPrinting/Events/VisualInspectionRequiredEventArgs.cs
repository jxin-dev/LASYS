using LASYS.Application.Features.BatchPrinting.Enums;

namespace LASYS.Application.Features.BatchPrinting.Events
{
    public sealed class VisualInspectionRequiredEventArgs : EventArgs
    {
        public VisualInspectionSampleType SampleType { get; }

        public string SequenceNo { get; }

        public VisualInspectionRequiredEventArgs(VisualInspectionSampleType sampleType, string sequenceNo)
        {
            SampleType = sampleType;
            SequenceNo = sequenceNo;
        }
    }
}
