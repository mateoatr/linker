using ILLink.Tasks.IntegrationTests;
using Xunit;
using Xunit.Abstractions;

namespace ILLink.Tests
{
	public class HelloWorldFixture : ProjectFixture
	{
		public HelloWorldFixture (IMessageSink diagnosticMessageSink) :
			base (diagnosticMessageSink)
		{
			ProjectName = "HelloWorld";
			ProjectTemplate = Template.Console;
			SetupProject ();
		}
	}

	public class ConsoleAppTests : IntegrationTestBase, IClassFixture<HelloWorldFixture>
	{
		protected readonly HelloWorldFixture fixture;

		public ConsoleAppTests (HelloWorldFixture fixture, ITestOutputHelper helper) :
			base (helper)
		{
			this.fixture = fixture;
		}

		[Fact]
		public void RunHelloWorldStandalone ()
		{
			bool selfContained = true;
			string executablePath = BuildAndLink (fixture, selfContained);

			RunApp (executablePath, out string output, selfContained);
			Assert.Contains ("Hello World!", output);
		}
	}
}
