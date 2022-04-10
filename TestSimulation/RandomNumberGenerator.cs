// Copyright (c) TestsSharedLibrary Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the solution root for license information.

using System;
using JetBrains.Annotations;

namespace TestsSharedLibrary.TestSimulation;

public class RandomNumberGenerator : IRandomNumberGenerator
{
    [NotNull] private readonly Random _randomNumberGenerator;

    protected RandomNumberGenerator([NotNull] Random randomNumberGenerator, int? randomNumberSeed)
    {
        _randomNumberGenerator = randomNumberGenerator;
        RandomNumberSeed = randomNumberSeed;
    }

    public int Next(int maxValue)
    {
        return Next(0, maxValue);
    }

    /// <inheritdoc />
    public virtual int Next(int minValue, int maxValue)
    {
        if (minValue < 0 || maxValue < minValue)
            throw new ArgumentException(nameof(minValue));

        return _randomNumberGenerator.Next(minValue, maxValue + 1);
    }

    public int? RandomNumberSeed { get; }

    [NotNull]
    public static RandomNumberGenerator CreateWithNullSeed()
    {
        return new RandomNumberGenerator(new Random(), null);
    }

    [NotNull]
    public static RandomNumberGenerator CreateWithSeed(int randomNumberSeed)
    {
        return new RandomNumberGenerator(new Random(randomNumberSeed), randomNumberSeed);
    }

    [NotNull]
    public static RandomNumberGenerator CreateWithRandomlyGeneratedSeed()
    {
        var random = new Random();
        var randomNumberSeed = random.Next();
        return new RandomNumberGenerator(new Random(randomNumberSeed), randomNumberSeed);
    }
}