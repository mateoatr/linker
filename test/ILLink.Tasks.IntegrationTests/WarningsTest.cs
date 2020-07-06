//using ILLink.Tests;
//using Xunit;
//using Xunit.Abstractions;

//namespace ILLink.Tasks
//{
//	public class WarningsFixture : ProjectFixture
//	{
//		public WarningsFixture (IMessageSink diagnosticMessageSink) : base (diagnosticMessageSink)
//		{
//			csproj = SetupProject ();
//		}

//		string SetupProject ()
//		{
//			return string.Empty;
//		}
//	}

//	public class WarningsTest : IntegrationTestBase, IClassFixture<WarningsFixture>
//	{
//		readonly WarningsFixture fixture;

//		public WarningsTest (WarningsFixture fixture, ITestOutputHelper output) : base (output)
//		{
//			this.fixture = fixture;
//		}
//	}
//}
