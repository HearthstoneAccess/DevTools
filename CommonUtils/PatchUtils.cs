using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace CommonUtils
{
	public class PatchUtils
	{
		private static string s_hsaBranchSuffix = "-HSA";

		public static void PatchDecompiledHearthstone(string hsVersion, string patchFile)
		{
			PrepareBranchForPatching(hsVersion);

			GitUtils.ApplyPatch(Constants.DecompiledDir, patchFile);
		}

		private static void PrepareBranchForPatching(string hsVersion)
		{
			// Allow decompiled on top of the HSA branch as long as it looks exactly the same as HS to ease things
			var currentBranch = GitUtils.GetCurrentBranchName(Constants.DecompiledDir);
			var hsaBranchName = GetHSABranchName(hsVersion);

			if (!currentBranch.Equals(hsVersion) && !currentBranch.Equals(hsaBranchName))
			{
				ProgramUtils.ExitProgramWith($"Your {Constants.DecompiledDir} directory is in the wrong branch.",
					$"Expected: {hsVersion}",
					$"Actual: {currentBranch}");
			}

			if (GitUtils.BranchExists(Constants.DecompiledDir, hsaBranchName))
			{
				if (!GitUtils.AreBranchContentsEqual(Constants.DecompiledDir, hsVersion, hsaBranchName))
				{
					ProgramUtils.ExitProgramWith($"The {hsaBranchName} branch in your {Constants.DecompiledDir} directory is not clean.",
						$"If you're sure you want to try to patch {hsVersion} again, please delete the {hsaBranchName} branch first",
						$"{GetDeleteHsaBranchExampleMessage(hsVersion)}");
				}
			}

			GitUtils.CheckoutAndCreateBranchIfNeeded(Constants.DecompiledDir, hsaBranchName);
		}

		public static void BuildDecompiledHearthstone()
		{
			var vsProjectPath = Path.Combine(Constants.DecompiledDir, Constants.AssemblyCSharpVSProjectName);
			ProcessUtils.ExecuteProcess("dotnet", $"build {vsProjectPath}", Environment.CurrentDirectory);
		}

		public static string GetHSABranchName(string hsVersion)
		{
			if (IsHSABranchName(hsVersion))
			{
				return hsVersion;
			}

			return $"{hsVersion}{s_hsaBranchSuffix}";
		}

		public static bool IsHSABranchName(string branchName)
		{
			return branchName.EndsWith(s_hsaBranchSuffix);
		}

		public static string GetDeleteHsaBranchExampleMessage(string hsVersion)
		{
			var hsaBranchName = GetHSABranchName(hsVersion);

			return $"Example: cd {Constants.DecompiledDir} && git checkout {hsVersion} && git checkout . && git branch -D {hsaBranchName} && git clean -fd && cd ..";
		}
	}
}
