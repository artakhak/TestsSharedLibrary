// This software is part of the TestsSharedLibrary library
// Copyright © 2018 TestsSharedLibrary Contributors
// http://oroptimizer.com

// Permission is hereby granted, free of charge, to any person
// obtaining a copy of this software and associated documentation
// files (the "Software"), to deal in the Software without
// restriction, including without limitation the rights to use,
// copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the
// Software is furnished to do so, subject to the following
// conditions:

// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.

// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
// OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
// HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
// OTHER DEALINGS IN THE SOFTWARE.


using System;
using System.Globalization;
using System.Text;
using ClassVisualizer;
using JetBrains.Annotations;
using TestsSharedLibrary.TestSimulation.Statistics;

namespace TestsSharedLibrary.ClassVisualizers;

public class TestStatisticGroupVisualizer<TStatisticSource> : NonNullValueInitializer
{
    [NotNull] private readonly ITestStatisticGroup<TStatisticSource> _testStatisticGroup;

    /// <inheritdoc />
    public TestStatisticGroupVisualizer([NotNull] IValueVisualizerDependencyObjects valueVisualizerDependencyObjects,
        [NotNull] IObjectVisualizationContext objectVisualizationContext,
        [NotNull] ITestStatisticGroup<TStatisticSource> testStatisticGroup, [NotNull] string visualizedElementName) :
        base(valueVisualizerDependencyObjects, objectVisualizationContext, testStatisticGroup, visualizedElementName)
    {
        _testStatisticGroup = testStatisticGroup;
    }

    /// <inheritdoc />
    public override void Visualize(StringBuilder visualizedText, int level)
    {
        AddIndentedLineBreak(visualizedText, level);
        visualizedText.Append($"<{VisualizedElementName} StatisticGroupName='{_testStatisticGroup.StatisticName}' {nameof(ITestStatistic<TStatisticSource>.Counter)}='{_testStatisticGroup.Counter}'");

        if (_testStatisticGroup.ParentStatisticGroup != null)
        {
            visualizedText.Append(" Percentage='");

            if (_testStatisticGroup.ParentStatisticGroup.Counter == 0)
                visualizedText.Append("0");
            else
                visualizedText.Append(Math.Round((double) _testStatisticGroup.Counter / _testStatisticGroup.ParentStatisticGroup.Counter * 100, 4).ToString(CultureInfo.InvariantCulture));

            visualizedText.Append("'");
        }

        visualizedText.Append(">");

        foreach (var childStatistic in _testStatisticGroup.ChildStatisticsCollection)
        {
            var statisticVisualizer = ValueVisualizerDependencyObjects.ValueVisualizerFactory.CreateInterfaceVisualizer(
                ObjectVisualizationContext, childStatistic, childStatistic is ITestStatisticGroup<TStatisticSource> ? VisualizedElementNames.StatisticGroup : VisualizedElementNames.Statistic,
                typeof(ITestStatistics<TStatisticSource>), true);

            statisticVisualizer.Visualize(visualizedText, level + 1);
        }

        AddIndentedLineBreak(visualizedText, level);
        visualizedText.Append($"</{VisualizedElementName}>");
    }
}