// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Mono.Linker.Tests.Benchmarks
{
	public class TestProject
	{
		public TestDirectory TestDirectory { get; }
		public string AppDll { get; }
		public string AppExe { get; }

		public TestProject (string testName)
		{
			var testAsset = new TestAsset (testName);
			TestDirectory = testAsset.Build ();

			var dirInfo = new DirectoryInfo (TestDirectory.Path);
			AppDll = dirInfo.GetFiles ($"{testName}.dll", SearchOption.AllDirectories)
				.SingleOrDefault ()?.FullName;
			AppExe = dirInfo.GetFiles ($"{testName}{ExecutableExtension ()}", SearchOption.AllDirectories)
				.SingleOrDefault ()?.FullName;

			string ExecutableExtension () => RuntimeInformation.IsOSPlatform (OSPlatform.Windows) ? ".exe" : string.Empty;
		}
	}
}
