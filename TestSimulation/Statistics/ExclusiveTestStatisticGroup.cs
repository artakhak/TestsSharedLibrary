// Copyright (c) TestsSharedLibrary Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the solution root for license information.

using System;
using JetBrains.Annotations;

namespace TestsSharedLibrary.TestSimulation.Statistics;

/// <summary>
///     Represents statistics group when at most one child group or statistic can succeed.
/// </summary>
/// <typeparam name="TStatisticSource"></typeparam>
public abstract class ExclusiveTestStatisticGroup<TStatisticSource> : TestStatisticGroupBase<TStatisticSource>
{
    /// <inheritdoc />
    protected ExclusiveTestStatisticGroup([CanBeNull] TextItemStatisticsIsFilteredOut<TStatisticSource> textItemStatisticsIsFilteredOut) : base(textItemStatisticsIsFilteredOut)
    {
    }

    /// <inheritdoc />
    protected override bool OnUpdateStatisticCheck(TStatisticSource statisticSource)
    {
        ITestStatistic<TStatisticSource> matchedStatistic = null;
        foreach (var childStatistic in ChildStatisticsCollection)
            if (childStatistic.UpdateStatistic(statisticSource))
            {
                if (matchedStatistic != null)
                    throw new Exception($"Group '{StatisticName}' matched test statistics source '{statisticSource.GetType().FullName}' using both statistics '{matchedStatistic.StatisticName}' and '{childStatistic.StatisticName}'. At most one child statistic should succeed.");

                matchedStatistic = childStatistic;
            }

        return matchedStatistic != null;
    }
}