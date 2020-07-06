using System;
using System.Runtime.InteropServices;
using Xunit;
using Xunit.Abstractions;
using ILLink.Tasks.IntegrationTests;

namespace ILLink.Tests
{
	public class WebApiFixture : ProjectFixture
	{
		public WebApiFixture (IMessageSink diagnosticMessageSink) :
			base (diagnosticMessageSink)
		{
			ProjectName = "WebApi";
			ProjectTemplate = Template.WebApi;
			SetupProject ("--no-https");
		}
	}

	public class WebApiTests : IntegrationTestBase, IClassFixture<WebApiFixture>
	{
		private readonly WebApiFixture fixture;

		public WebApiTests (WebApiFixture fixture, ITestOutputHelper output) : base (output)
		{
			this.fixture = fixture;
		}

		[Fact]
		public void RunWebApiStandalone ()
		{
			// CI has issues with the HTTPS dev cert
			if (RuntimeInformation.IsOSPlatform (OSPlatform.OSX) && !string.IsNullOrEmpty (Environment.GetEnvironmentVariable ("TF_BUILD")))
				return;

			bool selfContained = true;
			string executablePath = BuildAndLink (fixture, selfContained);
			string terminatingOutput = "Application started. Press Ctrl+C to shut down.";
			
			RunApp (executablePath, out string output, selfContained, 10000, terminatingOutput);
			Assert.Contains ("Now listening on: http://localhost:5000", output);
			Assert.Contains (terminatingOutput, output);
		}
	}
}
