using LASYS.Application.Common.Results;
using LASYS.Application.Interfaces.Persistence.Repositories;
using MediatR;

namespace LASYS.Application.Features.Products.GetProductDetails
{
    public sealed class GetProductDetailsHandler : IRequestHandler<GetProductDetailsQuery, Result<ProductDetails>>
    {
        private readonly IProductRepository _productRepository;

        public GetProductDetailsHandler(IProductRepository productRepository)
        {
            _productRepository = productRepository;
        }

        public async Task<Result<ProductDetails>> Handle(GetProductDetailsQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var productDetails = await _productRepository.GetDetailsAsync(request.ItemCode, request.MasterRevision, request.BoxType);
                if (productDetails is null)
                {
                    return Result.Failure<ProductDetails>("Product not found");
                }
                return Result.Success(productDetails);
            }
            catch (Exception ex)
            {
                return Result.Failure<ProductDetails>($"An error occurred while retrieving product details: {ex.Message}");
            }

        }
    }
}
