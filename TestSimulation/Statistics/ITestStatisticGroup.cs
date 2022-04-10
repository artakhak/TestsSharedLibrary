// Copyright (c) TestsSharedLibrary Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the solution root for license information.

using System.Collections.Generic;
using JetBrains.Annotations;

namespace TestsSharedLibrary.TestSimulation.Statistics;

public interface ITestStatisticGroup<TStatisticSource> : ITestStatistic<TStatisticSource>
{
    [NotNull] [ItemNotNull] IEnumerable<ITestStatistic<TStatisticSource>> ChildStatisticsCollection { get; }

    void AddChildStatistic([NotNull] ITestStatistic<TStatisticSource> childTestStatistic);
}

public static class TestStatisticGroupExtensions
{
    public static void AddChildStatistics<TStatisticSource>([NotNull] this ITestStatisticGroup<TStatisticSource> testStatisticGroup, [NotNull] [ItemNotNull] IEnumerable<ITestStatistic<TStatisticSource>> childTestStatistics)
    {
        foreach (var testStatistic in childTestStatistics)
            testStatisticGroup.AddChildStatistic(testStatistic);
    }
}