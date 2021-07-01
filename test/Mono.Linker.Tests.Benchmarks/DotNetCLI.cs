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
	public static class DotNetCLI
	{
		static readonly string ExecutablePath;

		static DotNetCLI ()
		{
			ExecutablePath = @"D:\linker\.dotnet\dotnet";
		}

		static Command SetupEnvironmentVariables (Command cmd)
		{
			return cmd
				.EnvironmentVariable ("DOTNET_ROOT", Path.GetDirectoryName (ExecutablePath))
				.EnvironmentVariable ("PATH", "%PATH%;%DOTNET_ROOT%");
		}

		static Command CreateCommand (string command, params string[] args)
		{
			var _args = args.ToList ();
			_args.Insert (0, command);
			return SetupEnvironmentVariables (Command.Create (ExecutablePath, string.Join (' ', _args)));
		}

		public static Command Build (params string[] args) => CreateCommand ("build", args);
		public static Command Exec (params string[] args) => CreateCommand ("exec", args);
	}
}
