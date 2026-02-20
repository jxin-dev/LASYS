using Dapper;
using LASYS.Application.Interfaces;
using LASYS.Domain.OCR;
using LASYS.Infrastructure.Data;

namespace LASYS.Infrastructure.Repositories;

public class LabelRepository: ILabelRepository
{
    private readonly ApplicationDbContext _context;

    public LabelRepository(string connectionString)
    {
        _context = new ApplicationDbContext(connectionString);
    }

    public async Task<LabelProduct?> GetByItemCodeAsync(string itemCode)
    {
        const string sql = @"
            SELECT *,
                (ACTIVE_FLAG IS NOT NULL) AS IsActive,
                (LABEL_TYPE_UB_FLAG IS NOT NULL) AS HasUbLabel,
                (LABEL_TYPE_CB_FLAG IS NOT NULL) AS HasCbLabel,
                (LABEL_TYPE_OCB_FLAG IS NOT NULL) AS HasOcbLabel,
                (LABEL_TYPE_OUB_FLAG IS NOT NULL) AS HasOubLabel,
                (LABEL_TYPE_AUB_FLAG IS NOT NULL) AS HasAubLabel,
                (LABEL_TYPE_ACB_FLAG IS NOT NULL) AS HasAcbLabel,
                (LABEL_TYPE_CASE_FLAG IS NOT NULL) AS HasCaseLabel,
                INET_NTOA(CREATED_IP_ADDRESS) AS CreatedIpAddress
            FROM pre_tpc_products_mst 
            WHERE ITEM_CODE = @itemCode";

        using var db = _context.CreateConnection();
        return await db.QueryFirstOrDefaultAsync<LabelProduct>(sql, new { itemCode });
    }

    public async Task<bool> UpsertAsync(LabelProduct product)
    {
        const string sql = @"
            INSERT INTO pre_tpc_products_mst (
                ITEM_CODE, ITEM_NAME, ITEM_GROUP_TYPE_CODE, MARKET_CODE, SECTION_ASSIGNMENTS,
                ITEM_TYPE, CALENDAR_TYPE, DESCRIPTION, DESCRIPTION_1, DESCRIPTION_2,
                WITH_COC, DAYS_BEFORE_EXPIRATION, DAYS_BFR_XPRTN_FR_BRCD, DAYS_BEFORE_STERILIZATION,
                BARCODE_TYPE, BARCODE_CATEGORY, CB_PER_PALLETE, QC_SAMPLE_QUANTITY, SB_QUANTITY,
                TRANSFER_INSTRUCTION, LABEL_WIDTH, LABEL_HEIGHT,
                LABEL_TYPE_UB_FLAG, LABEL_TYPE_CB_FLAG, LABEL_TYPE_OCB_FLAG, 
                LABEL_TYPE_OUB_FLAG, LABEL_TYPE_AUB_FLAG, LABEL_TYPE_ACB_FLAG,
                ACTIVE_FLAG, CUSTOM_1, CUSTOM_2, CUSTOM_3, CUSTOM_4, CUSTOM_5,
                CREATED_USER_CODE, CREATED_SECTION_ID, CREATED_IP_ADDRESS, CREATED_DATETIME,
                LASTUPDATE_USER_CODE, LASTUPDATE_SECTION_ID, LASTUPDATE_IP_ADDRESS, LASTUPDATE_DATETIME
            ) VALUES (
                @ItemCode, @ItemName, '1', '1', '0',
                @ItemType, @CalendarType, @Description, @Description1, @Description2,
                @WithCoc, 0, 0, 0,
                @BarcodeType, @BarcodeCategory, @CbPerPallet, 0, 0,
                @TransferInstruction, @LabelWidth, @LabelHeight,
                IF(@HasUbLabel, '', NULL), IF(@HasCbLabel, '', NULL), IF(@HasOcbLabel, '', NULL),
                IF(@HasOubLabel, '', NULL), IF(@HasAubLabel, '', NULL), IF(@HasAcbLabel, '', NULL),
                IF(@IsActive, '', NULL), @Custom1, @Custom2, @Custom3, @Custom4, @Custom5,
                @CreatedUserCode, @CreatedSectionId, INET_ATON(@CreatedIpAddress), CAST(DATE_FORMAT(NOW(), '%Y%m%d%H%i%s') AS UNSIGNED),
                @CreatedUserCode, @CreatedSectionId, INET_ATON(@CreatedIpAddress), CAST(DATE_FORMAT(NOW(), '%Y%m%d%H%i%s') AS UNSIGNED)
            ) 
            ON DUPLICATE KEY UPDATE 
                ITEM_NAME = VALUES(ITEM_NAME),
                LABEL_WIDTH = VALUES(LABEL_WIDTH),
                LABEL_HEIGHT = VALUES(LABEL_HEIGHT),
                ACTIVE_FLAG = VALUES(ACTIVE_FLAG),
                LASTUPDATE_DATETIME = VALUES(LASTUPDATE_DATETIME);";

        using var db = _context.CreateConnection();
        return await db.ExecuteAsync(sql, product) > 0;
    }

    public async Task<bool> SetActiveStatusAsync(string itemCode, bool isActive)
    {
        const string sql = "UPDATE pre_tpc_products_mst SET ACTIVE_FLAG = @flag WHERE ITEM_CODE = @itemCode";
        using var db = _context.CreateConnection();
        return await db.ExecuteAsync(sql, new { itemCode, flag = isActive ? "" : null }) > 0;
    }

    public async Task<bool> DeleteAsync(string itemCode)
    {
        using var db = _context.CreateConnection();
        return await db.ExecuteAsync("DELETE FROM pre_tpc_products_mst WHERE ITEM_CODE = @itemCode", new { itemCode }) > 0;
    }

    public async Task<IEnumerable<LabelProduct>> GetActiveLabelsAsync()
    {
        const string sql = "SELECT ITEM_CODE as ItemCode, ITEM_NAME as ItemName FROM pre_tpc_products_mst WHERE ACTIVE_FLAG IS NOT NULL";
        using var db = _context.CreateConnection();
        return await db.QueryAsync<LabelProduct>(sql);
    }
}