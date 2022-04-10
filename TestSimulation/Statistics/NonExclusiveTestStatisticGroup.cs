// Copyright (c) TestsSharedLibrary Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the solution root for license information.

using JetBrains.Annotations;

namespace TestsSharedLibrary.TestSimulation.Statistics;

/// <summary>
///     Represents statistics group when more than one child group or statistic can succeed.
/// </summary>
/// <typeparam name="TStatisticSource"></typeparam>
public abstract class NonExclusiveTestStatisticGroup<TStatisticSource> : TestStatisticGroupBase<TStatisticSource>
{
    /// <inheritdoc />
    protected NonExclusiveTestStatisticGroup([CanBeNull] TextItemStatisticsIsFilteredOut<TStatisticSource> textItemStatisticsIsFilteredOut) : base(textItemStatisticsIsFilteredOut)
    {
    }

    /// <inheritdoc />
    protected override bool OnUpdateStatisticCheck(TStatisticSource statisticSource)
    {
        var success = false;

        foreach (var childStatistic in ChildStatisticsCollection)
            if (childStatistic.UpdateStatistic(statisticSource))
                success = true;

        return success;
    }
}