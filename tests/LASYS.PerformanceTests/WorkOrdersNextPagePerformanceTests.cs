using LASYS.Application.Common.Messaging;
using Xunit;

namespace LASYS.PerformanceTests
{
    public class WorkOrdersNextPagePerformanceTests
    {
        [Fact]
        public void TriggerNextPageNavigation_ShouldStayUnderBudget()
        {
            var averageMilliseconds = WorkOrdersPerformanceHelpers.MeasureNextPageNavigationAverageMilliseconds(250);
            Assert.True(averageMilliseconds < 5.0,
                $"Expected next-page navigation to average under 5.0 ms, but measured {averageMilliseconds:F4} ms.");
        }
    }
}
