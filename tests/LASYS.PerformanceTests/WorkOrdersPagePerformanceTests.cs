using LASYS.Application.Features.LabelProcessing.GetWorkOrders;
using LASYS.DesktopApp.Views.UserControls;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;
using Xunit;

namespace LASYS.PerformanceTests
{
    public class WorkOrdersPagePerformanceTests
    {
        [Fact]
        public void SetWorkOrders_250Rows_ShouldStayUnderBudget()
        {
            Exception? threadError = null;

            var thread = new Thread(() =>
            {
                try
                {
                    RunOnStaThread();
                }
                catch (Exception ex)
                {
                    threadError = ex;
                }
            });

            thread.IsBackground = true;
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
            thread.Join();

            if (threadError != null)
                throw new Xunit.Sdk.XunitException(threadError.ToString());
        }

        private static void RunOnStaThread()
        {
            using var control = new WorkOrdersControl();
            var rows = CreateRows(250);

            // Warm-up
            for (int i = 0; i < 10; i++)
            {
                control.SetWorkOrders(rows, 10);
            }

            const int iterations = 50;
            long totalTicks = 0;

            for (int i = 0; i < iterations; i++)
            {
                var start = Stopwatch.GetTimestamp();
                control.SetWorkOrders(rows, 10);
                totalTicks += Stopwatch.GetTimestamp() - start;
            }

            var averageMilliseconds = totalTicks * 1000d / Stopwatch.Frequency / iterations;
            Assert.True(averageMilliseconds < 1.0,
                $"Expected SetWorkOrders to average under 1.0 ms, but measured {averageMilliseconds:F4} ms.");
        }

        private static List<GetWorkOrdersResult> CreateRows(int count)
        {
            var rows = new List<GetWorkOrdersResult>(count);

            for (int i = 0; i < count; i++)
            {
                rows.Add(new GetWorkOrdersResult
                {
                    ItemCode = $"ITEM-{i:0000}",
                    LotNo = $"LOT-{i:0000}",
                    ExpDate = "2026-12-31",
                    PrintType = i % 2 == 0 ? "Original" : "Additional",
                    Verdict = i % 3 == 0 ? "OK" : "Pending",
                    DateApproved = "04/20/2026",
                    ProdQty = 1200,
                    MasterLabelRevisionNo = 3,
                    LabelInsRevisionNo = 7,
                    UB_Qty = 100,
                    UB_LI_Status = "Printed",
                    AUB_Qty = 25,
                    AUB_LI_Status = "Pending",
                    OUB_Qty = 10,
                    OUB_LI_Status = "Pending"
                });
            }

            return rows;
        }
    }
}
