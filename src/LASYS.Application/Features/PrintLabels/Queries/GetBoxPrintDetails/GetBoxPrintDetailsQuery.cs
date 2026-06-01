using LASYS.Application.Common.Enums;
using LASYS.Application.Common.Results;
using MediatR;

namespace LASYS.Application.Features.PrintLabels.Queries.GetBoxPrintDetails
{
    public sealed record GetBoxPrintDetailsQuery(string ItemCode, string LotNo, BoxType BoxType) : IRequest<Result<PrintDetailsDto?>>;
}
