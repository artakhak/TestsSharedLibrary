// Copyright (c) TestsSharedLibrary Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the solution root for license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using JetBrains.Annotations;
using OROptimizer.Diagnostics.Log;

namespace TestsSharedLibrary.TestSimulation;

public class SimulationRandomNumberGenerator : RandomNumberGenerator, ISimulationRandomNumberGenerator
{
    private readonly LinkedList<int> _generatedNumbers = new();

    [NotNull] private readonly Func<string> _getSerializedRandomNumbersFileDirectoryPath;

    [NotNull] private readonly object _lockObject = new();

    private LinkedListNode<int> _currentReusedRandomValueNode;

    private string _lastSimulationIdentifier;
    private string _lastSimulationIterationIdentifier;
    private bool _reuseSavedRandomNumbers;

    private SimulationRandomNumberGenerator([NotNull] Random randomNumberGenerator, int? randomNumberSeed,
        [NotNull] Func<string> getSerializedRandomNumbersFileDirectoryPath) : base(randomNumberGenerator, randomNumberSeed)
    {
        _getSerializedRandomNumbersFileDirectoryPath = getSerializedRandomNumbersFileDirectoryPath;
    }

    /// <inheritdoc />
    public override int Next(int minValue, int maxValue)
    {
        lock (_lockObject)
        {
            int number;
            if (_reuseSavedRandomNumbers)
            {
                if (_currentReusedRandomValueNode == null)
                    throw new Exception($"Too many calls to '{nameof(Next)}()' in a mode when random numbers are picked from saved list.");

                number = _currentReusedRandomValueNode.Value;

                if (number < minValue || number > maxValue)
                    throw new Exception($"The random number selected from re-used list is not between {minValue} and {maxValue}. The value is {number}.");

                _currentReusedRandomValueNode = _currentReusedRandomValueNode.Next;
                return number;
            }

            number = base.Next(minValue, maxValue);
            _generatedNumbers.AddLast(number);
            return number;
        }
    }

    /// <inheritdoc />
    public void OnSimulationIterationStarting(string simulationIdentifier, string simulationIterationIdentifier, bool reuseSavedRandomNumbers)
    {
        if (string.IsNullOrWhiteSpace(simulationIdentifier))
            throw new ArgumentNullException(nameof(simulationIdentifier));

        if (string.IsNullOrWhiteSpace(simulationIterationIdentifier))
            throw new ArgumentNullException(nameof(simulationIterationIdentifier));

        lock (_lockObject)
        {
            _lastSimulationIdentifier = simulationIdentifier.Trim();
            _lastSimulationIterationIdentifier = simulationIterationIdentifier.Trim();
            _reuseSavedRandomNumbers = reuseSavedRandomNumbers;

            _generatedNumbers.Clear();

            if (reuseSavedRandomNumbers)
            {
                var simulationFilePath = GetSimulationFilePath(_lastSimulationIdentifier);

                if (!File.Exists(simulationFilePath))
                    throw new Exception($"File '{simulationFilePath}' does not exist.");

                try
                {
                    using (var streamReader = new StreamReader(simulationFilePath))
                    {
                        var simulationFile = streamReader.ReadToEnd();

                        var xmlDocument = new XmlDocument();
                        xmlDocument.LoadXml(simulationFile);

                        var simulationNodes = xmlDocument.SelectNodes("//Simulation");

                        if (simulationNodes != null)
                            for (var simulationIndex = 0; simulationIndex < simulationNodes.Count; ++simulationIndex)
                            {
                                if (!(simulationNodes[simulationIndex] is XmlElement simulationElement))
                                    break;

                                if (!string.Equals(_lastSimulationIdentifier, simulationElement.GetAttribute("SimulationIdentifier"), StringComparison.OrdinalIgnoreCase))
                                    continue;

                                var simulationIterations = simulationElement.SelectNodes("//SimulationIteration");

                                if (simulationIterations == null || simulationIterations.Count == 0)
                                    break;

                                for (var simulationIterationIndex = 0; simulationIterationIndex < simulationIterations.Count; ++simulationIterationIndex)
                                {
                                    if (!(simulationIterations[simulationIterationIndex] is XmlElement simulationIterationElement))
                                        break;

                                    if (!string.Equals(_lastSimulationIterationIdentifier, simulationIterationElement.GetAttribute("SimulationIterationIdentifier"), StringComparison.OrdinalIgnoreCase))
                                        continue;

                                    var randomNumbersNode = simulationIterationElement.SelectSingleNode("//RandomNumbers");

                                    if (randomNumbersNode == null || !(randomNumbersNode is XmlElement randomNumbersElement) ||
                                        !randomNumbersElement.HasAttribute("Values"))
                                        break;

                                    var randomNumbersElementValue = randomNumbersElement.GetAttribute("Values");

                                    var randomNumbers = randomNumbersElementValue.Split(',');

                                    foreach (var randomNumber in randomNumbers)
                                    {
                                        if (!int.TryParse(randomNumber, out var randomNumberInt))
                                            throw new Exception($"Could not parse '{randomNumber}' to type '{typeof(int).FullName}'.");

                                        _generatedNumbers.AddLast(randomNumberInt);
                                    }
                                }
                            }

                        if (_generatedNumbers.Count == 0)
                            throw new Exception($"Failed to find simulation random numbers in any of elements with path '//Simulation/SimulationIteration/RandomNumbers' for simulation '{_lastSimulationIdentifier}' and iteration '{_lastSimulationIterationIdentifier}'.");

                        _currentReusedRandomValueNode = _generatedNumbers.First;
                    }
                }
                catch (Exception e)
                {
                    LogHelper.Context.Log.Error(e);

                    throw new Exception($"Failed to load simulation data for simulation '{_lastSimulationIdentifier}' and simulation iteration '{_lastSimulationIterationIdentifier}' from file '{simulationFilePath}'.");
                }
            }
        }
    }

    /// <inheritdoc />
    public void SaveRandomNumbers()
    {
        if (string.IsNullOrWhiteSpace(_lastSimulationIdentifier) ||
            string.IsNullOrWhiteSpace(_lastSimulationIterationIdentifier))
            throw new Exception($"Method '{nameof(SaveRandomNumbers)}()' can be called only if method '{nameof(OnSimulationIterationStarting)}' was called.");

        SaveRandomNumbers(GetSimulationFilePath(_lastSimulationIdentifier));
    }

    /// <inheritdoc />
    public void SaveRandomNumbers(string savedFilePath)
    {
        lock (_lockObject)
        {
            try
            {
                var stringBuilder = new StringBuilder();

                stringBuilder.AppendLine(@"<?xml version=""1.0"" encoding=""utf-8""?>");
                stringBuilder.AppendLine($"<Simulation SimulationIdentifier='{_lastSimulationIdentifier}' >");
                stringBuilder.AppendLine($"    <SimulationIteration SimulationIterationIdentifier='{_lastSimulationIterationIdentifier}' {nameof(RandomNumberSeed)}='{RandomNumberSeed}'>");
                stringBuilder.AppendLine($"        <RandomNumbers Values='{string.Join(",", _generatedNumbers)}' />");
                stringBuilder.AppendLine( "    </SimulationIteration>");
                stringBuilder.AppendLine( "</Simulation>");

                using (var fileStream = new StreamWriter(savedFilePath, false))
                {
                    fileStream.Write(stringBuilder.ToString());
                }
            }
            catch (Exception e)
            {
                LogHelper.Context.Log.Error(e);
                throw new Exception($"Failed to save the simulation data to file '{savedFilePath}' for simulation '{_lastSimulationIdentifier}' and iteration '{_lastSimulationIterationIdentifier}'.");
            }
        }
    }

    [NotNull]
    public static SimulationRandomNumberGenerator CreateWithNullSeed([NotNull] Func<string> getSerializedRandomNumbersFileDirectoryPath)
    {
        return new SimulationRandomNumberGenerator(new Random(), null, getSerializedRandomNumbersFileDirectoryPath);
    }

    [NotNull]
    public static SimulationRandomNumberGenerator CreateWithSeed([NotNull] Func<string> getSerializedRandomNumbersFileDirectoryPath, int randomNumberSeed)
    {
        return new SimulationRandomNumberGenerator(new Random(randomNumberSeed), randomNumberSeed, getSerializedRandomNumbersFileDirectoryPath);
    }

    [NotNull]
    public static SimulationRandomNumberGenerator CreateWithRandomlyGeneratedSeed([NotNull] Func<string> getSerializedRandomNumbersFileDirectoryPath)
    {
        var random = new Random();
        var randomNumberSeed = random.Next();
        return new SimulationRandomNumberGenerator(new Random(randomNumberSeed), randomNumberSeed, getSerializedRandomNumbersFileDirectoryPath);
    }

    private string GetSimulationFilePath(string simulationIterationIdentifier)
    {
        return Path.Combine(_getSerializedRandomNumbersFileDirectoryPath(), $"SimulationData_{simulationIterationIdentifier}.xml");
    }
}