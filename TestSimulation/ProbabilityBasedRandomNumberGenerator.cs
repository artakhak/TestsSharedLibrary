// Copyright (c) TestsSharedLibrary Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the solution root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;

namespace TestsSharedLibrary.TestSimulation;

public class ProbabilityBasedRandomNumberGenerator : IProbabilityBasedRandomNumberGenerator
{
    private readonly List<ProbabilityData> _probabilityData = new();

    private readonly int _probabilityValueFor100Percent;

    [NotNull] private readonly IRandomNumberGenerator _randomNumberGenerator;

    /// <summary>
    ///     Constructor.
    /// </summary>
    /// <param name="randomNumberGenerator">
    ///     An instance of <see cref="IRandomNumberGenerator" /> used to generate random
    ///     numbers.
    /// </param>
    /// <param name="probabilityValueFor100Percent">
    ///     If the value for this parameter is 1000, than 1 is equivalent to (1/1000)*100=0.1%.
    ///     On the other hand if <paramref name="probabilityValueFor100Percent" /> is 100, than 1 is (1/100)*100=1%.
    ///     Bigger values for this parameter allow more refined random number generation.
    /// </param>
    public ProbabilityBasedRandomNumberGenerator([NotNull] IRandomNumberGenerator randomNumberGenerator,
        int probabilityValueFor100Percent)
    {
        _randomNumberGenerator = randomNumberGenerator;
        _probabilityValueFor100Percent = probabilityValueFor100Percent;
    }

    /// <inheritdoc />
    public int GetRandomNumber()
    {
        if (CurrentCumulativeProbability != _probabilityValueFor100Percent)
            throw new Exception($"Total cumulative probability is less than {_probabilityValueFor100Percent}. Make sure to call the methods '{nameof(AddRandomNumbersForProbability)}' to setup probabilities.");

        var randomNumber = _randomNumberGenerator.Next(_probabilityValueFor100Percent);

        ProbabilityData probabilityData = null;

        if (_probabilityData.Count >= 1)
        {
            var currentCumulativeProbability = 0;

            for (var i = 0; i < _probabilityData.Count - 1; ++i)
            {
                var currentProbabilityData = _probabilityData[i];

                var nextCumulativeProbability = currentCumulativeProbability + currentProbabilityData.Probability;

                if (randomNumber >= currentCumulativeProbability && randomNumber < nextCumulativeProbability)
                {
                    probabilityData = currentProbabilityData;
                    break;
                }

                currentCumulativeProbability = nextCumulativeProbability;
            }
        }

        if (probabilityData == null)
            probabilityData = _probabilityData[_probabilityData.Count - 1];

        return probabilityData.CandidateValues[_randomNumberGenerator.Next(probabilityData.CandidateValues.Count - 1)];
    }

    /// <inheritdoc />
    public void AddRandomNumbersForProbability(int probability, IEnumerable<int> candidateRandomValuesSelectedForProbability)
    {
        var newCurrentTotalProbability = CurrentCumulativeProbability + probability;

        if (newCurrentTotalProbability > _probabilityValueFor100Percent)
            throw new ArgumentException($"Sum of values of parameter {nameof(probability)} cannot be bigger than {_probabilityValueFor100Percent}.");

        var randomNumbersList = candidateRandomValuesSelectedForProbability.ToList();

        if (randomNumbersList.Count == 0)
            throw new ArgumentException($"The collection '{nameof(candidateRandomValuesSelectedForProbability)}' cannot be empty.");

        CurrentCumulativeProbability = newCurrentTotalProbability;

        _probabilityData.Add(new ProbabilityData(probability, randomNumbersList));
    }

    /// <inheritdoc />
    public void AddRandomNumbers(IEnumerable<int> candidateRandomValues)
    {
        if (CurrentCumulativeProbability == _probabilityValueFor100Percent)
            throw new ArgumentException($"The parameter-less method '{nameof(AddRandomNumbers)}' cannot be called if cumulative probability is {_probabilityValueFor100Percent}.");

        AddRandomNumbersForProbability(_probabilityValueFor100Percent - CurrentCumulativeProbability, candidateRandomValues);
    }

    /// <inheritdoc />
    public int CurrentCumulativeProbability { get; private set; }

    private class ProbabilityData
    {
        public ProbabilityData(int probability, [NotNull] [ItemNotNull] IReadOnlyList<int> candidateValues)
        {
            Probability = probability;
            CandidateValues = candidateValues;
        }

        public int Probability { get; }

        public IReadOnlyList<int> CandidateValues { get; }
    }
}