using System.IO;
using System.Collections.Generic;
using System.Linq;
using Xunit;
using Xunit.Abstractions;
using System.Text;

namespace ILLink.Tests
{
	/// <summary>
	///	Contains logic shared by multiple test classes.
	/// </summary>
	public class IntegrationTestBase
	{
		private readonly TestLogger logger;
		protected readonly CommandHelper CommandHelper;

		public IntegrationTestBase (ITestOutputHelper output)
		{
			logger = new TestLogger (output);
			CommandHelper = new CommandHelper (logger);
		}

		private void LogMessage (string message)
		{
			logger.LogMessage (message);
		}

		/// <summary>
		/// Run the linker on the specified project. This assumes
		/// that the project already contains a reference to the
		/// linker task package.
		/// Optionally takes a list of root descriptor files.
		/// Returns the path to the built app, either the renamed
		/// host for self-contained publish, or the dll containing
		/// the entry point.
		/// </summary>
		public string BuildAndLink (ProjectFixture fixture, bool selfContained = false, List<string> rootFiles = null, params (string arg, string value)[] publishArgs)
		{
			StringBuilder publishCommand = new StringBuilder ("publish -c ");
			publishCommand.Append (TestContext.Configuration);
			publishCommand.Append (" /v:n");
			publishCommand.Append (" /p:PublishTrimmed=true");
			if (selfContained)
				publishCommand.Append (" -r ").Append (TestContext.RuntimeIdentifier);

			if (rootFiles != null && rootFiles.Any ())
				publishCommand.Append (" /p:LinkerRootDescriptors=").Append (string.Join (';', rootFiles));

			if (publishArgs.Length != 0) {
				foreach (var arg in publishArgs)
					publishCommand.Append (" /p:").Append (arg.arg).Append ('=').Append (arg.value);
			}

			Assert.True (CommandHelper.Dotnet (publishCommand.ToString (), Path.GetDirectoryName (fixture.Csproj)), "Publish failed.");
			// Detect the target framework for which the app was published
			string tfmDir = Path.Combine (Path.GetDirectoryName (fixture.Csproj), "bin", TestContext.Configuration);
			string tfm = Directory.GetDirectories (tfmDir).Select (p => Path.GetFileName (p)).Single ();
			string builtApp = Path.Combine (tfmDir, tfm,
				selfContained ? TestContext.RuntimeIdentifier : string.Empty,
				"publish",
				$"{fixture.ProjectName}.dll");

			Assert.True (File.Exists (builtApp), $"File {builtApp} was expected to exist.");
			return builtApp;
		}

		public int RunApp (string target, out string processOutput, bool selfContained = false,
			int timeout = int.MaxValue, string terminatingOutput = null)
		{
			Assert.True (File.Exists (target), $"Could not run {target}. The target does not exist.");
			return CommandHelper.RunCommand (
				Path.GetFullPath (TestContext.DotnetToolPath),
				Path.GetFullPath (target),
				Directory.GetParent (target).FullName, null,
				out processOutput, timeout, terminatingOutput);
		}
	}
}
