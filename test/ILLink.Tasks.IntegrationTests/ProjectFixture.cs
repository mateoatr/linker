using System.IO;
using System.Text;
using System.Xml.Linq;
using ILLink.Tasks.IntegrationTests;
using Xunit;
using Xunit.Abstractions;

namespace ILLink.Tests
{
	/// <summary>
	/// Represents a project. Each fixture contains setup code run
	/// once before all tests in the same test class. ProjectFixture
	/// is the base type for different specific project fixtures.
	/// </summary>
	public class ProjectFixture
	{
		public string ProjectRoot { get; protected set; }
		public string ProjectName { get; protected set; }
		public Template? ProjectTemplate { get; protected set; }
		public string Csproj { get; protected set; }

		protected CommandHelper CommandHelper { get; private set; }
		protected FixtureLogger Logger { get; }

		public ProjectFixture (IMessageSink diagnosticMessageSink)
		{
			Logger = new FixtureLogger (diagnosticMessageSink);
			CommandHelper = new CommandHelper (Logger);
		}

		protected virtual void SetupProject (params string[] args)
		{
			Assert.True (ProjectName != null, "The project name was not set.");
			Assert.True (ProjectTemplate != null, "The project template was not set.");

			CreateTestFolder (ProjectName);
			Csproj = Path.Combine (ProjectRoot, $"{ProjectName}.Csproj");
			if (Directory.Exists (ProjectRoot))
				Directory.Delete (ProjectRoot, true);

			Directory.CreateDirectory (ProjectRoot);
			StringBuilder dotnetNew = new StringBuilder ("new ");
			dotnetNew.Append (ProjectTemplate.ToString ().ToLower ());
			foreach (var arg in args)
				dotnetNew.Append (' ').Append (arg);

			Assert.True (CommandHelper.Dotnet (dotnetNew.ToString (), ProjectRoot), "Failed creating a new project.");
			AddLinkerReference ();
			AddNuGetConfig ();
		}

		void CreateTestFolder (string ProjectName)
		{
			var rootFolder = Path.GetFullPath (Path.Combine ("tests-temp", ProjectName));
			Directory.CreateDirectory (rootFolder);

			// Write empty Directory.Build.props and Directory.Build.targets to disable accidental import of arcade from repo root
			File.WriteAllText (Path.Combine (rootFolder, "Directory.Build.props"), "<Project></Project>");
			File.WriteAllText (Path.Combine (rootFolder, "Directory.Build.targets"), "<Project></Project>");

			ProjectRoot = Path.Combine (rootFolder, ProjectName);
		}

		void AddLinkerReference ()
		{
			var xdoc = XDocument.Load (Csproj);
			var ns = xdoc.Root.GetDefaultNamespace ();
			foreach (var element in xdoc.Root.Elements (ns + "PropertyGroup"))
				element.Add (new XElement (ns + "ILLinkTasksAssembly", TestContext.TasksAssemblyPath));

			using (var fs = new FileStream (Csproj, FileMode.Create))
				xdoc.Save (fs);
		}

		void AddNuGetConfig ()
		{
			var nugetConfig = Path.Combine (ProjectRoot, "NuGet.config");
			var xdoc = new XDocument ();
			var configuration = new XElement ("configuration");
			var packageSources = new XElement ("packageSources");
			packageSources.Add (new XElement ("add",
						new XAttribute ("key", "dotnet-core"),
						new XAttribute ("value", "https://pkgs.dev.azure.com/dnceng/public/_packaging/dotnet5/nuget/v3/index.json")));

			configuration.Add (packageSources);
			xdoc.Add (configuration);

			using (var fs = new FileStream (nugetConfig, FileMode.Create))
				xdoc.Save (fs);
		}
	}
}
