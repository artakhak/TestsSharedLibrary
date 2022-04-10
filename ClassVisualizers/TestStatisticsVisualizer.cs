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
using System.Collections.Generic;
using System.Linq;
using ClassVisualizer;
using JetBrains.Annotations;
using TestsSharedLibrary.TestSimulation.Statistics;

namespace TestsSharedLibrary.ClassVisualizers;

public class TestStatisticsVisualizer<TStatisticSource> : InterfaceVisualizer
{
    [NotNull] private readonly ITestStatistics<TStatisticSource> _testStatistics;

    /// <inheritdoc />
    public TestStatisticsVisualizer([NotNull] IValueVisualizerDependencyObjects valueVisualizerDependencyObjects,
        [NotNull] IObjectVisualizationContext objectVisualizationContext,
        [NotNull] ITestStatistics<TStatisticSource> testStatistics,
        [NotNull] string visualizedElementName,
        [NotNull] Type mainInterfaceType, bool addChildren) :
        base(valueVisualizerDependencyObjects, objectVisualizationContext, testStatistics, visualizedElementName, mainInterfaceType, addChildren)
    {
        _testStatistics = testStatistics;
    }

    protected override bool PropertyShouldBeIgnoredVirtual(string propertyName)
    {
        if (propertyName == SpecialVisualizedPropertyNames.ObjectId)
            return true;

        return base.PropertyShouldBeIgnoredVirtual(propertyName);
    }

    protected override (IList<IInterfacePropertyInfo> interfacePropertiesWithNoCategory, IList<IPropertyCategory> propertyCategories) GetVisualizedProperties()
    {
        var interfacePropertiesWithNoCategory = new List<IInterfacePropertyInfo>();

        var baseVisualizedProperties = base.GetVisualizedProperties();
        interfacePropertiesWithNoCategory.AddRange(baseVisualizedProperties.interfacePropertiesWithNoCategory.Where(
            x => x.Name != nameof(ITestStatistics<TStatisticSource>.TestStatisticsCollection)));

        foreach (var testStatistic in _testStatistics.TestStatisticsCollection)
            if (testStatistic is ITestStatisticGroup<TStatisticSource>)
                interfacePropertiesWithNoCategory.Add(new InterfacePropertyInfo(VisualizedElementNames.StatisticGroup,
                    typeof(ITestStatisticGroup<TStatisticSource>), PropertyVisualizationType.VisualizePropertyAndChildren, testStatistic));
            else
                interfacePropertiesWithNoCategory.Add(new InterfacePropertyInfo(VisualizedElementNames.Statistic,
                    typeof(ITestStatistic<TStatisticSource>), PropertyVisualizationType.VisualizePropertyAndChildren, testStatistic));

        return (interfacePropertiesWithNoCategory, new List<IPropertyCategory>(0));
    }
}