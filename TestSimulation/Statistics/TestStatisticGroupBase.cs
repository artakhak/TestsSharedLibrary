// Copyright (c) TestsSharedLibrary Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the solution root for license information.

using System.Collections.Generic;
using JetBrains.Annotations;

namespace TestsSharedLibrary.TestSimulation.Statistics;

public delegate bool TextItemStatisticsIsFilteredOut<TStatisticSource>(ITestStatistic<TStatisticSource> childTestStatistic);

public abstract class TestStatisticGroupBase<TStatisticSource> : TestStatisticBase<TStatisticSource>, ITestStatisticGroup<TStatisticSource>
{
    [NotNull] [ItemNotNull] private readonly List<ITestStatistic<TStatisticSource>> _childStatisticsCollection = new();

    [CanBeNull] private readonly TextItemStatisticsIsFilteredOut<TStatisticSource> _textItemStatisticsIsFilteredOut;

    protected TestStatisticGroupBase([CanBeNull] TextItemStatisticsIsFilteredOut<TStatisticSource> textItemStatisticsIsFilteredOut)
    {
        _textItemStatisticsIsFilteredOut = textItemStatisticsIsFilteredOut;
    }

    /// <inheritdoc />
    public void AddChildStatistic(ITestStatistic<TStatisticSource> childTestStatistic)
    {
        if (_textItemStatisticsIsFilteredOut?.Invoke(childTestStatistic) ?? false)
            return;

        childTestStatistic.ParentStatisticGroup = this;
        _childStatisticsCollection.Add(childTestStatistic);
    }

    public IEnumerable<ITestStatistic<TStatisticSource>> ChildStatisticsCollection => _childStatisticsCollection;
}