namespace LASYS.Domain.Instruction
{
    /// <summary>
    /// DTO representing a single instruction print snapshot row returned by
    /// the GetInstructionPrintSnapshot SQL template.
    /// </summary>
    public class InstructionPrintSnapshot
    {
        /// <summary>
        /// Maps to i.item_code
        /// </summary>
        public string? ItemCode { get; set; }

        /// <summary>
        /// Maps to i.lot_no
        /// </summary>
        public string? LotNo { get; set; }

        /// <summary>
        /// Maps to i.expiry_date
        /// </summary>
        public DateTime? ExpiryDate { get; set; }

        /// <summary>
        /// Maps to i.{0} AS ins_code (instruction code column)
        /// </summary>
        public string? InsCode { get; set; }

        /// <summary>
        /// Maps to i.{1} AS targetqty (target print quantity for the instruction)
        /// </summary>
        public int? TargetQty { get; set; }

        /// <summary>
        /// Count of labels with status in ('Original','Additional','Replacement','Returned')
        /// </summary>
        public int TotalPassed { get; set; }

        /// <summary>
        /// Count of labels with status = 'Failed During Printing'
        /// </summary>
        public int TotalFailed { get; set; }

        /// <summary>
        /// Count of labels with status in ('First','Last')
        /// </summary>
        public int TotalSample { get; set; }

        /// <summary>
        /// IFNULL(MAX(sequence_number), 0)
        /// </summary>
        public long LastSequence { get; set; }
    }
}
