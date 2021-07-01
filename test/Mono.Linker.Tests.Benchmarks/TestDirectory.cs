// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.IO;

namespace Mono.Linker.Tests.Benchmarks
{
	public class TestDirectory
	{
		public string Configuration { get; }
		public string TargetFrameworkMoniker { get; }
		public string TestName { get; }
		public string Path { get; }

		public TestDirectory (string testDirPath)
		{
			Path = testDirPath;
			if (string.IsNullOrEmpty (testDirPath))
				throw new ArgumentException (testDirPath);

			if (!Directory.Exists (testDirPath))
				throw new DirectoryNotFoundException (testDirPath);
		}
	}
}
