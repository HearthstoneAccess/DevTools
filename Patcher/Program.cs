using CommonUtils;
using System;

namespace Patcher
{
	class Program
	{
		static void Main(string[] args)
		{
			ProgramUtils.EnsureSetupHasBeenRun();

			// Make sure we're attempting to apply a patch on top of the correct branch
			var decompiledHearthstoneVersion = HearthstoneUtils.ExtractHearthstoneVersion(Constants.DecompiledDir);
			var curBranchName = GitUtils.GetCurrentBranchName(Constants.DecompiledDir);

			ValidateBranch(decompiledHearthstoneVersion, curBranchName);

			var hsaBranchName = PatchUtils.GetHSABranchName(curBranchName);
			GitUtils.CheckoutAndCreateBranchIfNeeded(Constants.DecompiledDir, hsaBranchName);

			GitUtils.ApplyPatch(Constants.DecompiledDir, $"../{Constants.PatchFileName}"); // TODO: this properly
		}

		private static void ValidateBranch(string decompiledHearthstoneVersion, string curBranchName)
		{
			var hsaBranchName = PatchUtils.GetHSABranchName(curBranchName);
			var expectedHsaBranchName = PatchUtils.GetHSABranchName(decompiledHearthstoneVersion);

			if (!expectedHsaBranchName.Equals(hsaBranchName))
			{
				ProgramUtils.ExitProgramWith($"Your {Constants.DecompiledDir} directory is in the incorrect branch.",
					$"Current branch is: {curBranchName}",
					$"Expected: {decompiledHearthstoneVersion}");
			}
			else if (GitUtils.BranchExists(Constants.DecompiledDir, hsaBranchName) &&
				!GitUtils.AreBranchContentsEqual(Constants.DecompiledDir, decompiledHearthstoneVersion, expectedHsaBranchName))
			{
				ProgramUtils.ExitProgramWith($"Your {hsaBranchName} branch in the {Constants.DecompiledDir} directory is not clean.",
					$"Please delete it before trying again",
					PatchUtils.GetDeleteHsaBranchExampleMessage(decompiledHearthstoneVersion));
			}
		}
	}
}
