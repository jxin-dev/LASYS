using LASYS.Application.Common.Enums;

namespace LASYS.Application.Interfaces.Persistence.TableMappings
{
    public interface IPrintTableResolver
    {
        string GetTableName(BoxType boxType);
    }
}
