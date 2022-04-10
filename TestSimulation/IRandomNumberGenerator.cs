// This software is part of the TestsSharedLibrary library
// Copyright © 2018 TestsSharedLibrary Contributors
// http://oroptimizer.com

// Permission is hereby granted, free of charge, to any person
// obtaining a copy of this software and associated documentation
// files (the "Software"), to deal in the Software without
// restriction, including without limitation the rights to use,
// copy, modify, merge, publish, distribute, sublicense, and/or sell
// Copyright (c) TestsSharedLibrary Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the solution root for license information.

namespace TestsSharedLibrary.TestSimulation;

public interface IRandomNumberGenerator
{
    int? RandomNumberSeed { get; }
    int Next(int maxValue);
    int Next(int minValue, int maxValue);
}