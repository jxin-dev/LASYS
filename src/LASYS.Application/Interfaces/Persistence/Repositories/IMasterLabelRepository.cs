using LASYS.Application.Common.Enums;
using LASYS.Application.Common.Models;
using LASYS.Application.Features.NiceLabels;

namespace LASYS.Application.Interfaces.Persistence.Repositories
{
    public interface IMasterLabelRepository
    {
        Task<NiceLabel> GetNiceLabelDetailsAsync(string itemCode, uint masterRevision, BoxType boxType);
        Task<MasterLabelDetails> GetDetailsAsync(string itemCode, uint masterRevision, BoxType boxType);

    }
}
