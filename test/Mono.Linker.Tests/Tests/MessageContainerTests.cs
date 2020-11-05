using NUnit.Framework;

namespace Mono.Linker.Tests
{
	[TestFixture]
	public class MessageContainerTests
	{
		[Test]
		public void MSBuildFormat ()
		{
			LinkContext context = new LinkContext (new Pipeline ());

			var msg = MessageContainer.CreateErrorMessage ("text", 6001);
			Assert.AreEqual ("ILLink: error IL6001: text", msg.ToMSBuildString ());

			msg = MessageContainer.CreateWarningMessage (context, "message", 6002, new MessageOrigin ("logtest", 1, 1), WarnVersion.Latest);
			Assert.AreEqual ("logtest(1,1): warning IL6002: message", msg.ToMSBuildString ());

			msg = MessageContainer.CreateInfoMessage ("log test");
			Assert.AreEqual ("ILLink: log test", msg.ToMSBuildString ());
		}
	}
}
