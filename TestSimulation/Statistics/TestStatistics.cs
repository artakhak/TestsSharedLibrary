// Copyright (c) TestsSharedLibrary Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the solution root for license information.

using System.Collections.Generic;
using JetBrains.Annotations;

namespace TestsSharedLibrary.TestSimulation.Statistics;

public class TestStatistics<TStatisticSource> : ITestStatistics<TStatisticSource>
{
    [NotNull] [ItemNotNull] private readonly List<ITestStatistic<TStatisticSource>> _testStatistics;

    public TestStatistics([NotNull] string statisticName, [NotNull] [ItemNotNull] IEnumerable<ITestStatistic<TStatisticSource>> testStatistics)
    {
        StatisticName = statisticName;
        _testStatistics = new List<ITestStatistic<TStatisticSource>>(testStatistics);
    }

    /// <inheritdoc />
    public IEnumerable<ITestStatistic<TStatisticSource>> TestStatisticsCollection => _testStatistics;

    /// <inheritdoc />
    public void UpdateStatistics(TStatisticSource statisticSource)
    {
        ++Counter;

        foreach (var testStatistic in _testStatistics)
            testStatistic.UpdateStatistic(statisticSource);
    }

    /// <inheritdoc />
    public string StatisticName { get; }

    /// <inheritdoc />
    public int Counter { get; private set; }
}