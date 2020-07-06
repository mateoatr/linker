using System;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace ILLink.Tests
{

	public class CommandHelper
	{
		private readonly ILogger logger;

		public CommandHelper (ILogger logger)
		{
			this.logger = logger;
		}

		public bool Dotnet (string args, string workingDir, string additionalPath = null)
		{
			return RunCommand (Path.GetFullPath (TestContext.DotnetToolPath), args,
				workingDir, additionalPath, out _) == 0;
		}

		public int RunCommand (string command, string args, string workingDir, string additionalPath,
			out string commandOutput, int timeout = Int32.MaxValue, string terminatingOutput = null)
		{
			return (new CommandRunner (command, logger))
				.WithArguments (args)
				.WithWorkingDir (workingDir)
				.WithAdditionalPath (additionalPath)
				.WithTimeout (timeout)
				.WithTerminatingOutput (terminatingOutput)
				.Run (out commandOutput);
		}
	}

	public class CommandRunner
	{
		private readonly ILogger logger;

		private readonly string command;
		private string args;
		private string workingDir;
		private string additionalPath;
		private int timeout = Int32.MaxValue;
		private string terminatingOutput;

		private void LogMessage (string message)
		{
			logger.LogMessage (message);
		}

		public CommandRunner (string command, ILogger logger)
		{
			this.command = command;
			this.logger = logger;
		}

		public CommandRunner WithArguments (string args)
		{
			this.args = args;
			return this;
		}

		public CommandRunner WithWorkingDir (string workingDir)
		{
			this.workingDir = workingDir;
			return this;
		}

		public CommandRunner WithAdditionalPath (string additionalPath)
		{
			this.additionalPath = additionalPath;
			return this;
		}

		public CommandRunner WithTimeout (int timeout)
		{
			this.timeout = timeout;
			return this;
		}

		public CommandRunner WithTerminatingOutput (string terminatingOutput)
		{
			this.terminatingOutput = terminatingOutput;
			return this;
		}

		public int Run ()
		{
			return Run (out _);
		}

		public int Run (out string commandOutput)
		{
			if (string.IsNullOrEmpty (command))
				throw new Exception ("No command was specified specified.");

			if (logger == null)
				throw new Exception ("No logger present.");

			var psi = new ProcessStartInfo {
				FileName = command,
				UseShellExecute = false,
				RedirectStandardOutput = true,
				RedirectStandardError = true,
			};

			LogMessage ($"Caller working directory: {Environment.CurrentDirectory}");
			if (!string.IsNullOrEmpty (args))
				psi.Arguments = args;

			LogMessage ($"{command} {args}");
			if (!string.IsNullOrEmpty (workingDir)) {
				psi.WorkingDirectory = workingDir;
				LogMessage ($"Working directory: {workingDir}");
			}

			if (!string.IsNullOrEmpty (additionalPath))
				psi.Environment["PATH"] += $";{additionalPath}";

			// dotnet sets some environment variables that
			// may cause problems in the child process.
			psi.Environment.Remove ("MSBuildExtensionsPath");
			psi.Environment.Remove ("MSBuildLoadMicrosoftTargetsReadOnly");
			psi.Environment.Remove ("MSBuildSDKsPath");
			psi.Environment.Remove ("VbcToolExe");
			psi.Environment.Remove ("CscToolExe");
			psi.Environment.Remove ("MSBUILD_EXE_PATH");

			LogMessage ("Environment:");
			foreach (var item in psi.Environment)
				LogMessage ($"\t{item.Key}={item.Value}");

			// Set process
			var process = new Process {
				StartInfo = psi
			};

			StringBuilder processOutput = new StringBuilder ();
			void handler (object sender, DataReceivedEventArgs e)
			{
				processOutput.Append (e.Data);
				processOutput.AppendLine ();
			}

			StringBuilder processError = new StringBuilder ();
			void ehandler (object sender, DataReceivedEventArgs e)
			{
				processError.Append (e.Data);
				processError.AppendLine ();
			}

			process.OutputDataReceived += handler;
			process.ErrorDataReceived += ehandler;
			if (!string.IsNullOrEmpty (terminatingOutput)) {
				void terminatingOutputHandler (object sender, DataReceivedEventArgs e)
				{
					if (!string.IsNullOrEmpty (e.Data) && e.Data.Contains (terminatingOutput))
						process.Kill ();
				}

				process.OutputDataReceived += terminatingOutputHandler;
			}

			process.Start ();
			process.BeginOutputReadLine ();
			process.BeginErrorReadLine ();
			if (!process.WaitForExit (timeout)) {
				LogMessage ($"Killing process after {timeout} ms");
				process.Kill ();
			}

			// WaitForExit with timeout doesn't guarantee
			// that the async output handlers have been
			// called, so WaitForExit needs to be called
			// afterwards.
			process.WaitForExit ();
			commandOutput = processOutput.ToString ();
			LogMessage (commandOutput);
			LogMessage (processError.ToString ());
			
			return process.ExitCode;
		}
	}
}
