// Copyright (c) TestsSharedLibrary Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the solution root for license information.

using JetBrains.Annotations;

namespace TestsSharedLibrary.TestSimulation.Statistics;

public interface INonGroupTestStatistic<TStatisticSource> : ITestStatistic<TStatisticSource>
{
    /// <summary>
    ///     Returns true, if <paramref name="statisticSource" /> is matched by this statistic.
    /// </summary>
    /// <param name="statisticSource">Statistic source to match.</param>
    /// <returns>Returns true, if <paramref name="statisticSource" /> matched the statistic criteria.</returns>
    bool IsStatisticSourceAMatch([NotNull] TStatisticSource statisticSource);
}