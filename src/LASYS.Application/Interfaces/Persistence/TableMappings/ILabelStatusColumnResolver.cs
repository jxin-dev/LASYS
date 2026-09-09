using LASYS.Application.Common.Enums;

namespace LASYS.Application.Interfaces.Persistence.TableMappings
{
    public interface ILabelStatusColumnResolver
    {
        string GetLabelStatusColumnName(BoxType boxType);
    }
}
