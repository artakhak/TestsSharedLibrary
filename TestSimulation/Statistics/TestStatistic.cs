// Copyright (c) TestsSharedLibrary Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the solution root for license information.

namespace TestsSharedLibrary.TestSimulation.Statistics;

public abstract class TestStatistic<TStatisticSource> : TestStatisticBase<TStatisticSource>, INonGroupTestStatistic<TStatisticSource>
{
    public abstract bool IsStatisticSourceAMatch(TStatisticSource statisticSource);

    protected override bool OnUpdateStatisticCheck(TStatisticSource statisticSource)
    {
        return IsStatisticSourceAMatch(statisticSource);
    }
}