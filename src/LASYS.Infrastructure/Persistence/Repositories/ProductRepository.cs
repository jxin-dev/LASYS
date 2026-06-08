using Dapper;
using LASYS.Application.Common.Enums;
using LASYS.Application.Features.Products;
using LASYS.Application.Interfaces.Persistence;
using LASYS.Application.Interfaces.Persistence.Repositories;
using LASYS.Application.Interfaces.Persistence.TableMappings;

namespace LASYS.Infrastructure.Persistence.Repositories
{
    public sealed class ProductRepository : IProductRepository
    {
        private readonly IDbConnectionFactory _factory;
        private readonly IProductColumnResolver _productColumnResolver;

        public ProductRepository(IDbConnectionFactory factory, IProductColumnResolver productColumnResolver)
        {
            _factory = factory;
            _productColumnResolver = productColumnResolver;
        }

        public async Task<ProductDetails> GetDetailsAsync(string itemCode, uint masterRevision, BoxType boxType)
        {
            var (reportTypeCodeColumn, barcodeNumberColumn, quantityColumn) = _productColumnResolver.GetColumnNames(boxType);
            var sql = $@"
            SELECT
                ITEM_NAME AS ItemName,
                ITEM_GROUP_TYPE_CODE AS ItemGroupTypeCode,
                MARKET_CODE AS MarketCode,
                SECTION_ASSIGNMENTS AS SectionAssignments,
                ITEM_TYPE AS ItemType,
                CALENDAR_TYPE AS CalendarType,
                DESCRIPTION AS Description,
                TIP_TYPE AS TipType,
                WITH_COC AS WithCoc,
                CASE WHEN PAIRED_CB_BOX_TYPE IS NOT NULL THEN 1 ELSE 0 END AS IsPairedBoxType,
                WITH_STERILIZATION AS WithSterilization,
                BARCODE_TYPE AS BarcodeType,
                BARCODE_CATEGORY AS BarcodeCategory,
                UB_PER_CB_QUANTITY AS UbPerCbQuantity,
                CB_PER_PALLETE AS CbPerPallete,
                QC_SAMPLE_QUANTITY AS QcSampleQuantity,
                SB_QUANTITY AS SbQuantity,
                {reportTypeCodeColumn} AS ReportTypeCode,
                {barcodeNumberColumn} AS BarcodeNumber,
                {quantityColumn} AS Quantity,
                LABEL_TYPE_EUMDR_FLAG AS IsEumdr,
                PQE_CONTROL_NUMBER_CODE AS PqeControlNumberCode,
                DVR_NUMBER AS DvrNumber,
                DEPKES_NUMBER AS DepkesNumber,
                CE_MARK AS CeMark,
                SR_TYPE AS SrType,
                GAUGE AS Gauge,
                NEEDLE_GAUGE AS NeedleGauge,
                NEEDLE_SIZE AS NeedleSize,
                TRANSFER_INSTRUCTION AS TransferInstruction,
                PSS_ID_NO AS PssIdNo,
                FLOW_RATE AS FlowRate,
                DESCRIPTION_1 AS Description1,
                DESCRIPTION_2 AS Description2,
                Custom_1 AS Custom1,
                Custom_2 AS Custom2,
                Custom_3 AS Custom3,
                Custom_4 AS Custom4,
                Custom_5 AS Custom5,
                Custom_6 AS Custom6,
                Custom_7 AS Custom7,
                Custom_8 AS Custom8,
                Custom_9 AS Custom9,
                Custom_10 AS Custom10 
            FROM
                pre_tpc_products_tcl
            WHERE
                ITEM_CODE = @itemCode AND MASTERLABEL_REVISION_NUMBER = @masterRevision";

            try
            {
                using var connection = await _factory.CreateConnectionAsync();
                var result = await connection.QueryFirstOrDefaultAsync<ProductDetails>(sql, new { itemCode, masterRevision });
                return result ?? throw new Exception("Product not found.");
            }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving product details: {ex.Message}", ex);
            }

        }
    }
}
