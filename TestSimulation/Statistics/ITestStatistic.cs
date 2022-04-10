// Copyright (c) TestsSharedLibrary Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the solution root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;

namespace TestsSharedLibrary.TestSimulation.Statistics;

public interface ITestStatistic<TStatisticSource> : IStatisticCounter
{
    [CanBeNull] ITestStatisticGroup<TStatisticSource> ParentStatisticGroup { get; set; }

    /// <summary>
    ///     Updates the counter if <paramref name="statisticSource" /> is matched by this statistic.
    /// </summary>
    /// <param name="statisticSource">Statistic source to match.</param>
    /// <returns>Returns true, if <paramref name="statisticSource" /> matched the statistic criteria.</returns>
    bool UpdateStatistic([NotNull] TStatisticSource statisticSource);
}

public static class TestStatisticExtensions
{
    [NotNull]
    public static string GetPath<TStatisticSource>(this ITestStatistic<TStatisticSource> testStatistic)
    {
        var testStatisticsNamesInPath = new LinkedList<string>();

        while (testStatistic != null)
        {
            testStatisticsNamesInPath.AddFirst(testStatistic.StatisticName);
            testStatistic = testStatistic.ParentStatisticGroup;
        }

        return string.Join("=>", testStatisticsNamesInPath.Select(x => $"[{x}]"));
    }

    public static bool IsStatisticsPathAMatch<TStatisticSource>(this ITestStatistic<TStatisticSource> testStatistic,
        bool useRegularExpressionMatching,
        bool isStartsWithPathMatching,
        params string[] statisticsNamesInPath)
    {
        if (statisticsNamesInPath == null || statisticsNamesInPath.Length == 0)
            return false;

        var testStatisticsInPath = new LinkedList<ITestStatistic<TStatisticSource>>();

        while (testStatistic != null)
        {
            testStatisticsInPath.AddFirst(testStatistic);
            testStatistic = testStatistic.ParentStatisticGroup;
        }

        if (testStatisticsInPath.Count != statisticsNamesInPath.Length)
        {
            if (!isStartsWithPathMatching)
                return false;

            if (testStatisticsInPath.Count < statisticsNamesInPath.Length)
                return false;
        }

        var currentStatisticsNodeInPath = testStatisticsInPath.First;

        foreach (var statisticsName in statisticsNamesInPath)
        {
            if (currentStatisticsNodeInPath == null)
                return true;

            if (useRegularExpressionMatching)
                // TODO: implement
                throw new NotImplementedException();
            if (currentStatisticsNodeInPath.Value.StatisticName != statisticsName) return false;

            currentStatisticsNodeInPath = currentStatisticsNodeInPath.Next;
        }

        return true;
    }

    public static ITestStatisticGroup<TStatisticSource> TryGetParentStatisticsGroup<TStatisticSource>(
        this ITestStatistic<TStatisticSource> testStatistic, Func<ITestStatisticGroup<TStatisticSource>, bool> isMatch)
    {
        var parentTestStatisticGroup = testStatistic.ParentStatisticGroup;

        while (parentTestStatisticGroup != null)
            if (isMatch(parentTestStatisticGroup))
                return parentTestStatisticGroup;

        return null;
    }
}