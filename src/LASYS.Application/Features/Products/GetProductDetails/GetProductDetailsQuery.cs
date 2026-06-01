using LASYS.Application.Common.Enums;
using LASYS.Application.Common.Results;
using MediatR;

namespace LASYS.Application.Features.Products.GetProductDetails
{
    public sealed record GetProductDetailsQuery(string ItemCode, uint MasterRevision, BoxType BoxType) : IRequest<Result<ProductDetails>>;
}
