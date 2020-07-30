namespace Mono.Linker.Steps
{
	public class OutlineRewriterStep : BaseStep
	{
		protected override void Process ()
		{
			var suffixArray = new SuffixArray<string> (Context.OutlineInstructionsAsStrings);
			var lrsNonOverlapping = suffixArray.GetLongestNonOverlappingSubsequence ();
		}
	}
}
