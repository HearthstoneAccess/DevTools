using System;
using System.IO;

namespace CommonUtils
{
	public class ProgramUtils
	{
		public static void ExitProgramWith(params string[] errorMessages)
		{
			foreach (var msg in errorMessages)
			{
				Console.WriteLine(msg);
			}
			Environment.Exit(1);
		}

		public static void EnsureSetupHasBeenRun()
		{
			if (!Directory.Exists(Constants.DecompiledDir))
			{
				ExitProgramWith("You need to set up your environment first.",
					"Please run: dotnet run --project EnvironmentSetup");
			}
		}
	}
}
