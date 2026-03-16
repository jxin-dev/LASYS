using LASYS.Application.Common.Results;
using MediatR;

namespace LASYS.Application.Features.LabelProcessing.GetLabelTemplate
{
    public record GetLabelTemplateQuery(int WorkOrderId) : IRequest<Result<string>>;
}
