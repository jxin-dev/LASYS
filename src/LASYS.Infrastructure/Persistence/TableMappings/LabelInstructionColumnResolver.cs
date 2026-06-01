using LASYS.Application.Common.Enums;
using LASYS.Application.Interfaces.Persistence.TableMappings;

namespace LASYS.Infrastructure.Persistence.TableMappings
{
    public sealed class LabelInstructionColumnResolver : ILabelInstructionColumnResolver
    {
        public string GetInstructionColumnName(BoxType boxType)
        {
            var prefix = boxType switch
            {
                BoxType.UnitBox => "UB",
                BoxType.AdditionalUnitBox => "AUB",
                BoxType.OuterUnitBox => "OUB",
                BoxType.CartonBox => "CB",
                BoxType.AdditionalCartonBox => "ACB",
                BoxType.OuterCartonBox => "OCB",
                _ => throw new ArgumentOutOfRangeException(nameof(boxType), boxType, "Unsupported BoxType")
            };
            return $"{prefix}_LBL_INS_CODE";
        }
    }
}
