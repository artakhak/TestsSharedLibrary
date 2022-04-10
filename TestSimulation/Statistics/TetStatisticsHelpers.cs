// Copyright (c) TestsSharedLibrary Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the solution root for license information.

using JetBrains.Annotations;

namespace TestsSharedLibrary.TestSimulation.Statistics;

public delegate void ProcessTestStatisticsDelegate<TStatisticSource>([NotNull] ITestStatistic<TStatisticSource> testStatistic,
    ref bool stopProcessing);

public static class TetStatisticsHelpers
{
    public static void ProcessTestStatistics<TStatisticSource>([NotNull] ITestStatistics<TStatisticSource> testStatistics,
        [NotNull] ProcessTestStatisticsDelegate<TStatisticSource> processTestStatisticsDelegate)
    {
        var stopProcessing = false;

        foreach (var testStatistic in testStatistics.TestStatisticsCollection)
        {
            ProcessTestStatistics(testStatistic, processTestStatisticsDelegate, ref stopProcessing);

            if (stopProcessing)
                return;
        }
    }

    private static void ProcessTestStatistics<TStatisticSource>([NotNull] ITestStatistic<TStatisticSource> testStatistic,
        [NotNull] ProcessTestStatisticsDelegate<TStatisticSource> processTestStatisticsDelegate,
        ref bool stopProcessing)
    {
        processTestStatisticsDelegate(testStatistic, ref stopProcessing);

        if (stopProcessing)
            return;

        if (testStatistic is ITestStatisticGroup<TStatisticSource> testStatisticGroup)
            foreach (var testStatisticGroupChild in testStatisticGroup.ChildStatisticsCollection)
            {
                ProcessTestStatistics(testStatisticGroupChild, processTestStatisticsDelegate, ref stopProcessing);

                if (stopProcessing)
                    break;
            }
    }
}