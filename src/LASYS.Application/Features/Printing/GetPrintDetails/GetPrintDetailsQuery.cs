using LASYS.Application.Common.Enums;
using LASYS.Application.Common.Models;
using LASYS.Application.Common.Results;
using MediatR;

namespace LASYS.Application.Features.Printing.GetPrintDetails
{
    public sealed record GetPrintDetailsQuery(string ItemCode, string LotNo, string PrintType, BoxType BoxType) : IRequest<Result<PrintDetails>>;
}
