using LASYS.Application.Common.Results;
using MediatR;

namespace LASYS.Application.Features.LabelProcessing.LoadLabelTemplate
{
    public record LoadLabelTemplateCommand(int WorkOrderId) : IRequest<Result>;
}
