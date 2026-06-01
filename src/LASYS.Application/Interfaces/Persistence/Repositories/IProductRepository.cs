using LASYS.Application.Common.Enums;
using LASYS.Application.Features.Products;

namespace LASYS.Application.Interfaces.Persistence.Repositories
{
    public interface IProductRepository
    {
        Task<ProductDetails> GetDetailsAsync(string itemCode, uint masterRevision, BoxType boxType);
    }

}
