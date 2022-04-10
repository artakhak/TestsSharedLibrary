// Copyright (c) TestsSharedLibrary Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the solution root for license information.

using System;
using JetBrains.Annotations;

namespace TestsSharedLibrary.TestSimulation;

public interface ISimulationRandomNumberGenerator : IRandomNumberGenerator
{
    /// <summary>
    ///     Initializes the random number generator for a new simulator for simulation with identifier
    ///     <paramref name="simulationIterationIdentifier" />.
    /// </summary>
    /// <param name="simulationIdentifier">
    ///     An identifier for the simulation. This can be a concatenation of the test class and test method.
    ///     For example if test class is TestUniversalExpression and the test method is TestExpressionParsing, the identifier
    ///     might be "TestUniversalExpression_TestUniversalExpression".
    /// </param>
    /// <param name="simulationIterationIdentifier">
    ///     Simulation iteration identifier. This can be simulation iteration index, or any other unique identifier.
    /// </param>
    /// <param name="reuseSavedRandomNumbers">
    ///     If true, a previously saved list for simulation with identifier <paramref name="reuseSavedRandomNumbers" />
    ///     will be used when the calls to get random number are made (e.g., <see cref="IRandomNumberGenerator.Next(int)" />
    ///     and <see cref="IRandomNumberGenerator.Next(int, int)" />).
    ///     Otherwise, new random numbers will be generated.
    /// </param>
    /// <exception cref="Exception">
    ///     Throws this exception if random numbers if <paramref name="reuseSavedRandomNumbers" /> is true, and
    ///     no random numbers were saved for simulation identifier <paramref name="simulationIterationIdentifier" />
    ///     before.
    /// </exception>
    void OnSimulationIterationStarting([NotNull] string simulationIdentifier, [NotNull] string simulationIterationIdentifier, bool reuseSavedRandomNumbers);

    /// <summary>
    ///     Saves all random numbers to a file.
    /// </summary>
    void SaveRandomNumbers();
}