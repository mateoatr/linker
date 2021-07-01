// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mono.Linker.Tests.Benchmarks
{
	public readonly struct CommandResult
	{
		public ProcessStartInfo StartInfo { get; }
		public int ExitCode { get; }
		public string StdOut { get; }
		public string StdErr { get; }


		public CommandResult (ProcessStartInfo startInfo, int exitCode, string stdOut, string stdErr)
		{
			StartInfo = startInfo;
			ExitCode = exitCode;
			StdOut = stdOut;
			StdErr = stdErr;
		}
	}
}
