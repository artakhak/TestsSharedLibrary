// Copyright (c) TestsSharedLibrary Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the solution root for license information.

using System;
using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace TestsSharedLibrary.TestSimulation;

public class RandomNumbersRange : IEnumerable<int>
{
    [NotNull] private readonly List<int> _randomNumbers;

    public RandomNumbersRange(int minValue, int maxValue)
    {
        if (minValue < 0)
            throw new ArgumentException($"The value of '{nameof(minValue)}' can be negative.", nameof(minValue));

        if (maxValue < minValue)
            throw new ArgumentException($"The value of '{nameof(maxValue)}' cannot be les than the value of '{nameof(maxValue)}'.", nameof(maxValue));

        _randomNumbers = new List<int>(maxValue - minValue + 1);

        for (var value = minValue; value <= maxValue; ++value)
            _randomNumbers.Add(value);
    }

    /// <inheritdoc />
    public IEnumerator<int> GetEnumerator()
    {
        return _randomNumbers.GetEnumerator();
    }

    /// <inheritdoc />
    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}