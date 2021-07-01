// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mono.Linker.Tests.Benchmarks
{
	public class TestAsset
	{
		// Working dir is linker/artifacts/bin/Mono.Linker.Tests.Benchmarks/config/tfm
		static readonly string TestDir = Path.GetFullPath (Path.Combine (Directory.GetCurrentDirectory (), "..", "..", "..", "..", "..", "test"));
		static readonly string TestProjectsDir = Path.Combine (TestDir, "Assets", "TestProjects");
		static readonly string TestArtifactsDir = Path.Combine (TestDir, "..", "artifacts", "bin");

		public readonly string TestAssetName;
		public readonly string TestAssetRoot;
		public readonly string TestProjectFile;
		
		public TestDirectory TestDirectory { get; private set; }

		public TestAsset (string assetName)
		{
			TestAssetName = assetName;
			if (string.IsNullOrEmpty (assetName))
				throw new ArgumentException (nameof (assetName));

			TestAssetRoot = Path.Combine (TestProjectsDir, assetName);
			TestProjectFile = Path.Combine (TestAssetRoot, $"{assetName}.csproj");
			if (!File.Exists (TestProjectFile))
				throw new FileNotFoundException (TestProjectFile);
		}

		public TestDirectory Build ()
		{
			var build = DotNetCLI.Build (TestProjectFile).Execute ();
			if (build.ExitCode != 0)
				throw new Exception ($"Building '{TestProjectFile}' failed with error code '{build.ExitCode}': {build.StdErr}");

			return new TestDirectory (Path.Combine (TestArtifactsDir, TestAssetName));
		}
	}
}
