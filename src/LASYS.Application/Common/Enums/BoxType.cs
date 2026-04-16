namespace LASYS.Application.Common.Enums
{
    /// <summary>Packing / box type codes embedded in the GS1-128 barcode.</summary>
    public enum BoxType
    {
        NotSet = -1,
        UnitBox = 1,    // UB
        CartonBox = 2,    // CB
        OuterUnitBox = 3,    // OUB
        AdditionalUnitBox = 4,  // AUB
        AdditionalCartonBox = 5,  // ACB
        OuterCartonBox = 6,    // OCB
        McKessonUB = 7,
        CaseLabel = 8,
        QC = 9,
        AubQC = 10,
        OubQC = 11,
        COC = 12,
    }
}
