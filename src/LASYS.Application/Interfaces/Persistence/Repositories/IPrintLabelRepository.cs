using LASYS.Application.Common.Enums;
using LASYS.Application.Common.Models;
using LASYS.Application.Features.BatchPrinting.Models;

namespace LASYS.Application.Interfaces.Persistence.Repositories
{
    public interface IPrintLabelRepository
    {
        Task<PrintDetails> GetDetailsAsync(string itemCode, string lotNo, string printType, BoxType boxType);
        Task<string?> GetLatestSpecialLabelStatusAsync(string itemCode, string lotNo, BoxType boxType);
        Task<bool> SavePrintedLabelAsync(SequenceData sequenceData);
    }

}