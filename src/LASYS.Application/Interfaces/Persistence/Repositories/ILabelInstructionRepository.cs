using LASYS.Application.Common.Enums;
using LASYS.Application.Common.Models;

namespace LASYS.Application.Interfaces.Persistence.Repositories
{
    public interface ILabelInstructionRepository
    {
        Task<LabelInstructionDetails> GetDetailsAsync(string itemCode, string lotNo, uint masterRevision, BoxType boxType);
        Task<LabelInstructionDetails> GetDetailsAsync(string itemCode, uint masterRevision, BoxType boxType);
    }
}
