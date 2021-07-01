// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mono.Linker.Tests.Benchmarks
{
	public class Command
	{
		private StringWriter _stdOutCapture;
		private StringWriter _stdErrCapture;

		private Command (string executable, string args)
		{
			var psi = new ProcessStartInfo () {
				FileName = executable,
				Arguments = args,
			};

			Process = new Process () {
				StartInfo = psi,
			};
		}

		public Process Process { get; }

		public static Command Create (string executable, string args)
		{
			return new Command (executable, args);
		}

		public Command CaptureStdErr ()
		{
			Process.StartInfo.RedirectStandardError = true;
			_stdErrCapture = new StringWriter ();
			return this;
		}

		public Command CaptureStdOut ()
		{
			Process.StartInfo.RedirectStandardOutput = true;
			_stdOutCapture = new StringWriter ();
			return this;
		}

		public Command EnvironmentVariable (string key, string value)
		{
			if (value == null)
				value = string.Empty;

			Process.StartInfo.Environment[key] = value;
			return this;
		}

		public CommandResult Execute ()
		{
			if (Process.StartInfo.RedirectStandardOutput)
				Process.OutputDataReceived += (sender, args) => _stdOutCapture.WriteLine (args.Data);

			if (Process.StartInfo.RedirectStandardError)
				Process.ErrorDataReceived += (sender, args) => _stdErrCapture.WriteLine (args.Data);

			if (string.IsNullOrEmpty (Process.StartInfo.WorkingDirectory))
				Process.StartInfo.WorkingDirectory = @"D:\linker\test\Assets\TestProjects\ConsoleApp";

			Process.Start ();
			Process.WaitForExit ();
			return new CommandResult (Process.StartInfo,
				Process.ExitCode,
				_stdOutCapture?.GetStringBuilder ()?.ToString (),
				_stdErrCapture?.GetStringBuilder ()?.ToString ());
		}

		public Command WorkingDirectory (string projectDirectory)
		{
			Process.StartInfo.WorkingDirectory = projectDirectory;
			return this;
		}
	}
}
