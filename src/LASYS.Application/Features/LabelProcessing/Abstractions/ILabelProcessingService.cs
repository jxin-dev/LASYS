using System.Drawing;
using LASYS.Application.Common.Enums;
using LASYS.Application.Features.LabelProcessing.Contracts;

namespace LASYS.Application.Features.LabelProcessing.Abstractions
{
    public interface ILabelProcessingService
    {
        Task StartJobAsync(Size viewerSize, StartLabelJobRequest request, CancellationToken token);
        void SetUserDecision(LabelProcessingAction action);

        event Action<LabelProcessingStatus> StatusChanged;
    }
}
