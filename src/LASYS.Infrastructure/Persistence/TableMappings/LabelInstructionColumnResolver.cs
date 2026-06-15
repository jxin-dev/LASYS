using DirectShowLib.BDA;
using LASYS.Application.Common.Enums;
using LASYS.Application.Interfaces.Persistence.TableMappings;

namespace LASYS.Infrastructure.Persistence.TableMappings
{
    public sealed class LabelInstructionColumnResolver : ILabelInstructionColumnResolver
    {
        public (string InstrunctionCode, string PrintType, string ApprovedByUserCode, string ApprovedBySectionId, string ApprovedByIpAddress, string ApprovedByDateTime) GetInstructionColumnNames(BoxType boxType)
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
            return ($"{prefix}_LBL_INS_CODE", $"{prefix}_LBL_INS_PRNT_TYPE", $"{prefix}_LBL_INS_APPRVD_USR_CD", $"{prefix}_LBL_INS_APPRVD_SCTN_ID", $"{prefix}_LBL_INS_APPRVD_IP_ADDR", $"{prefix}_LBL_INS_APPRVD_DATETIME");
        }
    }
}