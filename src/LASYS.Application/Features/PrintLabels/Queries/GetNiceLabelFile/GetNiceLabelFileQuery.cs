using LASYS.Application.Common.Enums;
using LASYS.Application.Common.Results;
using MediatR;

namespace LASYS.Application.Features.PrintLabels.Queries.GetNiceLabelFile
{
    public sealed record GetNiceLabelFileQuery(string ItemCode, uint MasterLabelRevNumber, BoxType BoxType) :IRequest<Result<byte[]?>>;

}
