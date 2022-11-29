using CommonUtils;
using System;

namespace DiffGenerator
{
	class Program
	{
		static void Main(string[] args)
		{
			ProgramUtils.EnsureSetupHasBeenRun();

			// Make sure we're attempting to apply a patch on top of the correct branch
			var curBranchName = GitUtils.GetCurrentBranchName(Constants.DecompiledDir);

			if (!curBranchName.EndsWith("-HSA"))
			{
				ProgramUtils.ExitProgramWith($"Your {Constants.DecompiledDir} directory is not in a HSA branch",
					$"Current branch is: {curBranchName}",
					$"Example of an HSA branch: 24.6.2.155409-HSA");
			}

			var baseBranchName = curBranchName.Replace("-HSA", "");

			GitUtils.CreateDiff(Constants.DecompiledDir, baseBranchName, curBranchName, $"../{Constants.PatchFileName}"); // TODO: this properly

			Console.WriteLine($"Patch saved to {Constants.PatchFileName}");
		}
	}
}
