using System;
using System.Collections.Generic;
using System.IO;

namespace CommonUtils
{
	public class GitUtils
	{
		public static string GetCurrentBranchName(string gitDir)
		{
			List<string> stdout = new List<string>();
			ProcessUtils.ExecuteProcessWithReturnCode("git", $"symbolic-ref --short HEAD", gitDir, stdout);

			if (stdout.Count != 1)
			{
				throw new SystemException($"Unexpected response when getting branch name");
			}

			return stdout[0];
		}

		public static bool BranchExists(string gitDir, string branchName)
		{
			int exitCode = ProcessUtils.ExecuteProcessWithReturnCode("git", $"show-ref --verify --quiet refs/heads/{branchName}", gitDir);

			return exitCode == 0;
		}

		// Used to cleanup git directories while preserving the directory itself (i.e. .git/)
		public static void CleanupDirectory(DirectoryInfo gitDir)
		{
			FileInfo[] files = gitDir.GetFiles();
			DirectoryInfo[] directories = gitDir.GetDirectories();
			foreach (FileInfo file in files)
			{
				file.Delete();
			}

			foreach (DirectoryInfo childDir in directories)
			{
				if (!childDir.Name.Equals(".git"))
				{
					childDir.Delete(true);
				}
			}
		}

		internal static bool AreBranchesDifferent(string gitDir, string branchA, string branchB)
		{
			int rc = ProcessUtils.ExecuteProcessWithReturnCode("git", $"diff --quiet {branchA} {branchB}", gitDir);
			return rc != 0;
		}

		public static void CheckoutAndCreateBranchIfNeeded(string gitDir, string branchName)
		{
			if (BranchExists(gitDir, branchName))
			{
				CheckoutExistingBranch(gitDir, branchName);
			}
			else
			{
				CheckoutNewBranch(gitDir, branchName);
			}
		}

		internal static bool IsBranchClean(string gitDir, string branchName)
		{
			int rc = ProcessUtils.ExecuteProcessWithReturnCode("git", $"diff --quiet {branchName}", gitDir);
			return rc == 0;
		}

		public static void CheckoutNewBranch(string gitDir, string branchName)
		{
			ProcessUtils.ExecuteProcess("git", $"checkout -b {branchName}", gitDir);
		}

		public static void CheckoutExistingBranch(string gitDir, string branchName)
		{
			ProcessUtils.ExecuteProcess("git", $"checkout {branchName}", gitDir);
		}

		public static void ApplyPatch(string gitDir, string patchFile)
		{
			ProcessUtils.ExecuteProcess("git", $"apply --whitespace=fix {patchFile}", gitDir);
		}

		public static void CreateDiff(string gitDir, string fromBranch, string toBranch, string outputFile)
		{
			ProcessUtils.ExecuteProcessWithReturnCode("git", $"diff --output={outputFile} {fromBranch} {toBranch}", gitDir);
		}

		public static void CommitAll(string gitDir, string commitMessage)
		{
			ProcessUtils.ExecuteProcess("git", "add -A --verbose", gitDir);
			ProcessUtils.ExecuteProcess("git", $"commit -m \"{commitMessage}\"", gitDir);
		}

		public static bool AreBranchContentsEqual(string gitDir, string branchA, string branchB)
		{
			return !AreBranchesDifferent(gitDir, branchA, branchB) && IsBranchClean(gitDir, branchB);
		}
	}
}
