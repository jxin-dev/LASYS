using LASYS.Application.Features.Barcodes.Common.Constants;
using LASYS.Application.Features.Barcodes.Common.Interfaces;
using LASYS.Application.Features.Barcodes.Common.Models;

namespace LASYS.Application.Features.Barcodes.ParseInstructionBarcode
{
    public class InstructionBarcodeParser : IInstructionBarcodeParser
    {
        public InstructionBarcodeResult Parse(string barcode)
        {
            if (string.IsNullOrWhiteSpace(barcode))
                throw new ArgumentException("Barcode is required.");

            if (barcode.Length != 32)
                throw new ArgumentException("Invalid barcode.");


            InstructionType instructionType = InstructionType.NotSet;
            InstructionStatus instructionStatus = InstructionStatus.NotSet;
            int? revisionNumber = null;
            bool isInstructionFormIndicator = barcode.Substring(
                InstructionBarcodeIndexes.InstructionFormIndicatorIndex,
                InstructionBarcodeIndexes.InstructionFormIndicatorLength) == GS1ApplicationIdentifiers.InstructionFormIndicator;
            if (isInstructionFormIndicator)
            {
                // Instruction Type
                var typeValue = Convert.ToInt32(barcode.Substring(
                    InstructionBarcodeIndexes.InstructionTypeIndex, 
                    InstructionBarcodeIndexes.InstructionTypeLength));
                if (Enum.IsDefined(typeof(InstructionType), typeValue))
                {
                    instructionType = (InstructionType)typeValue;
                }
                // Instruction Status
                var statusValue = Convert.ToInt32(barcode.Substring(
                    InstructionBarcodeIndexes.InstructionStatusIndex,
                    InstructionBarcodeIndexes.InstructionStatusLength));
                if (Enum.IsDefined(typeof(InstructionStatus), statusValue))
                {
                    instructionStatus = (InstructionStatus)statusValue;
                }
                // Revision Number
                revisionNumber = Convert.ToInt32(barcode.Substring(
                    InstructionBarcodeIndexes.InstructionRevisionIndex,
                    InstructionBarcodeIndexes.InstructionRevisionLength));
            }

            //SR*FS2032 mfg_products_mst 10.243.2.89
            string? lotNumber = null;
            string? lineCode = null;

            bool isLotNumberIndicator = barcode.Substring(InstructionBarcodeIndexes.LotNumberIndicatorIndex, InstructionBarcodeIndexes.LotNumberIndicatorLength) == GS1ApplicationIdentifiers.LotNumberIndicator;
            if (isLotNumberIndicator)
            {
                // Lot Number
                lotNumber = barcode.Substring(
                    InstructionBarcodeIndexes.LotNumberIndex, 
                    InstructionBarcodeIndexes.LotNumberLength);
                // Line Code
                lineCode = barcode.Length > InstructionBarcodeIndexes.LineCodeIndex 
                    ? barcode.Substring(InstructionBarcodeIndexes.LineCodeIndex) 
                    : null; //get everything from that index until the end.
            }



            return new InstructionBarcodeResult
            {
                InstructionBarcodeScanned = barcode,
                InstructionType = instructionType,
                InstructionStatus = instructionStatus,
                InstructionRevision = revisionNumber,
                InstructionLotNumber = lotNumber,
                InstructionLineCode = lineCode,
            };
        }
    }
}
