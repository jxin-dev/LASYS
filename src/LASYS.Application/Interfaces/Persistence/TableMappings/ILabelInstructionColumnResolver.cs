using LASYS.Application.Common.Enums;

namespace LASYS.Application.Interfaces.Persistence.TableMappings
{
    public interface ILabelInstructionColumnResolver
    {
        string GetInstructionColumnName(BoxType boxType);
    }
}
