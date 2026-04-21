using LASYS.Application.Common.Messaging;
using Xunit;

namespace LASYS.PerformanceTests
{
    public class WorkOrdersNextPageBaselineComparisonTests
    {
        private const double BaselineMilliseconds = 0.5;
        private const double AllowedRegressionFactor = 2.0;

        [Fact]
        public void TriggerNextPageNavigation_ShouldNotRegressAgainstBaseline()
        {
            var averageMilliseconds = WorkOrdersPerformanceHelpers.MeasureNextPageNavigationAverageMilliseconds(250);
            var allowedMilliseconds = BaselineMilliseconds * AllowedRegressionFactor;

            Assert.True(
                averageMilliseconds <= allowedMilliseconds,
                $"Expected next-page navigation to stay within {allowedMilliseconds:F3} ms of the {BaselineMilliseconds:F3} ms baseline, but measured {averageMilliseconds:F4} ms.");
        }
    }
}
