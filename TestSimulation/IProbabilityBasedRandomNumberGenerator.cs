// Copyright (c) TestsSharedLibrary Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the solution root for license information.

using System.Collections.Generic;
using JetBrains.Annotations;

namespace TestsSharedLibrary.TestSimulation;

public interface IProbabilityBasedRandomNumberGenerator
{
    int CurrentCumulativeProbability { get; }
    int GetRandomNumber();
    void AddRandomNumbersForProbability(int probability, [NotNull] IEnumerable<int> candidateRandomValuesSelectedForProbability);
    void AddRandomNumbers([NotNull] IEnumerable<int> candidateRandomValues);
}