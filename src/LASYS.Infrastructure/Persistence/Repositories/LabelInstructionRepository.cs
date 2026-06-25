using Dapper;
using DirectShowLib.BDA;
using LASYS.Application.Common.Enums;
using LASYS.Application.Common.Models;
using LASYS.Application.Interfaces.Persistence;
using LASYS.Application.Interfaces.Persistence.Repositories;
using LASYS.Application.Interfaces.Persistence.TableMappings;

namespace LASYS.Infrastructure.Persistence.Repositories
{
    public sealed class LabelInstructionRepository : ILabelInstructionRepository
    {
        private readonly IDbConnectionFactory _factory;
        private readonly ILabelInstructionColumnResolver _labelInstructionColumnResolver;
        public LabelInstructionRepository(IDbConnectionFactory factory, ILabelInstructionColumnResolver labelInstructionColumnResolver)
        {
            _factory = factory;
            _labelInstructionColumnResolver = labelInstructionColumnResolver;
        }

        public async Task<LabelInstructionDetails> GetDetailsAsync(string itemCode, string lotNo, uint masterRevision, BoxType boxType)
        {
            var (instrunctionCode, printType, approvedByUserCode, approvedBySectionId, approvedByIpAddress, approvedByDateTime) = _labelInstructionColumnResolver.GetInstructionColumnNames(boxType);

            var sql = $@"
            SELECT 
                {instrunctionCode} AS InstructionCode,
                LINE_CODE AS LineCode,
	            ITEM_CODE AS ItemCode,
                LOT_NO AS LotNo,
                LABEL_INS_REV_NUMBER AS LabelInsRevNumber,
                MASTER_LABEL_REVISION_NUMBER AS MasterLabelRevNumber,
                MANUFACTURE_DATE AS ManufactureDate,
                EXPIRY_DATE AS ExpirationDate,
                PRODUCTION_DATE AS ProductionDate,
                STERILIZATION_DATE AS SterilizationDate,
                TARGET_PRODUCTION_QUANTITY AS TargetProductionQuantity,
                {printType} AS PrintType,
                {approvedByUserCode} AS ApprovedByUserCode,
                {approvedBySectionId} AS ApprovedBySectionId,
                {approvedByIpAddress} AS ApprovedByIpAddress,
                {approvedByDateTime} AS ApprovedByDateTime
            FROM ppt_lbl_instructn_plns_hst
            WHERE 
            ITEM_CODE = @itemCode AND
            LOT_NO = @lotNo AND 
            MASTER_LABEL_REVISION_NUMBER = @masterRevision
            GROUP BY ITEM_CODE, LOT_NO, LABEL_INS_REV_NUMBER, MASTER_LABEL_REVISION_NUMBER 
            ORDER BY history_datetime DESC";

            try
            {
                using var connection = await _factory.CreateConnectionAsync();
                var result = await connection.QueryFirstOrDefaultAsync<LabelInstructionDetails>(sql, new { itemCode, lotNo, masterRevision });
                return result ?? throw new Exception("Label instruction details not found.");
            }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving label instruction details: {ex.Message}", ex);
            }
        }

        public async Task<LabelInstructionDetails> GetDetailsAsync(string itemCode, uint masterRevision, BoxType boxType)
        {
            var (instrunctionCode, printType, approvedByUserCode, approvedBySectionId, approvedByIpAddress, approvedByDateTime) = _labelInstructionColumnResolver.GetInstructionColumnNames(boxType);

            var sql = $@"
            SELECT 
                {instrunctionCode} AS InstructionCode,
                LINE_CODE AS LineCode,
	            ITEM_CODE AS ItemCode,
                LOT_NO AS LotNo,
                LABEL_INS_REV_NUMBER AS LabelInsRevNumber,
                MASTER_LABEL_REVISION_NUMBER AS MasterLabelRevNumber,
                MANUFACTURE_DATE AS ManufactureDate,
                EXPIRY_DATE AS ExpirationDate,
                PRODUCTION_DATE AS ProductionDate,
                STERILIZATION_DATE AS SterilizationDate,
                TARGET_PRODUCTION_QUANTITY AS TargetProductionQuantity,
                {printType} AS PrintType,
                {approvedByUserCode} AS ApprovedByUserCode,
                {approvedBySectionId} AS ApprovedBySectionId,
                {approvedByIpAddress} AS ApprovedByIpAddress,
                {approvedByDateTime} AS ApprovedByDateTime
            FROM ppt_lbl_instructn_plns_hst
            WHERE 
            ITEM_CODE = @itemCode AND
            MASTER_LABEL_REVISION_NUMBER = @masterRevision
            GROUP BY ITEM_CODE, LABEL_INS_REV_NUMBER, MASTER_LABEL_REVISION_NUMBER 
            ORDER BY history_datetime DESC LIMIT 1";

            try
            {
                using var connection = await _factory.CreateConnectionAsync();
                var result = await connection.QueryFirstOrDefaultAsync<LabelInstructionDetails>(sql, new { itemCode, masterRevision });
                return result ?? throw new Exception("Label instruction details not found.");
            }
            catch (Exception ex)
            {
                throw;
                //throw new Exception($"Error retrieving label instruction details: {ex.Message}", ex);
            }
        }
    }
}
