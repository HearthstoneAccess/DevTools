using CommonUtils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Decompiler
{
	class Program
	{
		static void Main(string[] args)
		{
			ProgramUtils.EnsureSetupHasBeenRun();

			string exampleMklinkMessage = $"Example: mklink /D {Constants.HearthstoneLinkDir} \"{HearthstoneUtils.HearthstoneDirTypicalLocation}\"";
			if (!Directory.Exists(Constants.HearthstoneLinkDir))
			{
				ProgramUtils.ExitProgramWith("Please create a symlink for your Hearthstone directory first.",
					exampleMklinkMessage);
			}

			if (!HearthstoneUtils.IsHearthstoneDirectory(Constants.HearthstoneLinkDir))
			{
				ProgramUtils.ExitProgramWith("Your local symlink for your Hearthstone directory id not valid.",
					$"Please make sure you've correctly linked your Hearthstone directory.",
					exampleMklinkMessage);
			}

			new Decompiler(Constants.HearthstoneLinkDir, Constants.DecompiledDir).Run();
		}
	}
}
