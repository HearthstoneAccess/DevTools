using CommonUtils;
using System;
using System.IO;

namespace EnvironmentSetup
{
	class Setup
	{
		private static string s_baselinePatch = "baseline.patch";

		static void Main(string[] args)
		{
			if (Directory.Exists(Constants.DecompiledDir))
			{
				ProgramUtils.ExitProgramWith("Your environment has already been setup.",
					$"If you're sure you want to set it up again, please delete your {Constants.DecompiledDir} directory and run the setup again.");

			}

			SetupDecompiledDirectory();

			DecompileBaseline();

			var hsVersion = HearthstoneUtils.ExtractHearthstoneVersion(Constants.BaselineDir);

			PatchUtils.PatchDecompiledHearthstone(hsVersion, Path.Combine($"..{Path.DirectorySeparatorChar}", s_baselinePatch));

			GitUtils.CommitAll(Constants.DecompiledDir, "Patched");

			PatchUtils.BuildDecompiledHearthstone();

			Console.WriteLine("Setup finished successfully.");
		}

		private static void DecompileBaseline()
		{
			new Decompiler.Decompiler(Constants.BaselineDir, Constants.DecompiledDir).Run();
		}

		private static void SetupDecompiledDirectory()
		{
			Directory.CreateDirectory(Constants.DecompiledDir);

			Console.WriteLine($"Initializing a new local (unversioned) git repository in {Constants.DecompiledDir}");

			ProcessUtils.ExecuteProcess("git", $"init", Constants.DecompiledDir);
		}
	}
}
