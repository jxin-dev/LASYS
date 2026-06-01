using LASYS.Application.Common.Enums;
using LASYS.Application.Common.Results;
using MediatR;

namespace LASYS.Application.Features.NiceLabels.GetNiceLabelDetails
{
    public sealed record GetNiceLabelDetailsQuery(string ItemCode, uint MasterRevision, BoxType BoxType) : IRequest<Result<NiceLabel>>;
    
}
