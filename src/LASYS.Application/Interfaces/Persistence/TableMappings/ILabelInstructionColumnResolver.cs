using LASYS.Application.Common.Enums;

namespace LASYS.Application.Interfaces.Persistence.TableMappings
{
    public interface ILabelInstructionColumnResolver
    {
        (string InstrunctionCode, string PrintType, string ApprovedByUserCode, string ApprovedBySectionId, string ApprovedByIpAddress, string ApprovedByDateTime) GetInstructionColumnNames(BoxType boxType);
    }
}
