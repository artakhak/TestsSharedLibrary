// Copyright (c) TestsSharedLibrary Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the solution root for license information.

using System;
using JetBrains.Annotations;

namespace TestsSharedLibrary.TestSimulation.Statistics;

public abstract class TestStatisticBase<TStatisticSource> : ITestStatistic<TStatisticSource>
{
    [CanBeNull] private ITestStatisticGroup<TStatisticSource> _parentStatisticGroup;

    public abstract string StatisticName { get; }

    public int Counter { get; private set; }

    public bool UpdateStatistic(TStatisticSource statisticSource)
    {
        var statisticShouldBeUpdated = OnUpdateStatisticCheck(statisticSource);

        if (statisticShouldBeUpdated)
            ++Counter;

        OnUpdateStatisticCompleted(statisticSource, statisticShouldBeUpdated);
        return statisticShouldBeUpdated;
    }

    public ITestStatisticGroup<TStatisticSource> ParentStatisticGroup
    {
        get => _parentStatisticGroup;
        set
        {
            if (_parentStatisticGroup != null)
                throw new ArgumentException($"The value of '{nameof(ITestStatistic<TStatisticSource>.ParentStatisticGroup)}' was already set and cannot be changed.");

            _parentStatisticGroup = value;
        }
    }

    protected virtual void OnUpdateStatisticCompleted(TStatisticSource statisticSource, bool isStatitisticUpdated)
    {
    }

    /// <summary>
    ///     Returns true, if statistic should be updated, normally when <paramref name="statisticSource" /> is matched by this
    ///     statistic, or
    ///     any child statistics match. Returns false otherwise.
    ///     Propagates calls to <see cref="UpdateStatistic(TStatisticSource)" /> to child statistics.
    /// </summary>
    /// <param name="statisticSource">Statistic source to match.</param>
    protected abstract bool OnUpdateStatisticCheck(TStatisticSource statisticSource);
}