using LASYS.Application.Common.Enums;
using LASYS.Application.Common.Models;

namespace LASYS.Application.Interfaces.Persistence.Repositories
{
    public interface IPrintLabelRepository
    {
        Task<PrintDetails> GetDetailsAsync(string itemCode, string lotNo, BoxType boxType);
    }

}