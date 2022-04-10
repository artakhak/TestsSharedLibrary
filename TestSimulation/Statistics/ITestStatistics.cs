// Copyright (c) TestsSharedLibrary Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the solution root for license information.

using System.Collections.Generic;
using JetBrains.Annotations;

namespace TestsSharedLibrary.TestSimulation.Statistics;

public interface ITestStatistics<TStatisticSource> : IStatisticCounter
{
    [NotNull] [ItemNotNull] IEnumerable<ITestStatistic<TStatisticSource>> TestStatisticsCollection { get; }

    void UpdateStatistics([NotNull] TStatisticSource statisticSource);
}