// Copyright (c) TestsSharedLibrary Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the solution root for license information.

using OROptimizer.Diagnostics.Log;
using TestsSharedLibrary.Diagnostics.Log;

namespace TestsSharedLibrary;

public class TestsBase
{
    public TestsBase()
    {
        if (!LogHelper.IsContextInitialized)
            LogHelper.RegisterContext(new LogHelper4TestsContext());

        Log4Tests.LogLevel = LogLevel.Debug;
    }
}