namespace LASYS.Application.Features.WorkOrders.GetWorkOrders
{
    public sealed record WorkOrderItemResponse(
        int Id,
        string ItemCode,
        string LotNo,
        string ExpDate,
        string PrintType,
        string Verdict,
        string DateApproved,
        int ProductQty,
        int MasterLabelRevNo,
        int LabelInsRevNo,
        string UnitBoxQty, string UnitBoxStatus,
        string AddtnlUnitBoxQty, string AddtnlUnitBoxStatus,
        string OuterUnitBoxQty, string OuterUnitBoxStatus);
}