// Copyright (c) TestsSharedLibrary Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the solution root for license information.

using JetBrains.Annotations;

namespace TestsSharedLibrary.TestSimulation.Statistics;

public interface IStatisticCounter
{
    [NotNull] string StatisticName { get; }

    int Counter { get; }
}