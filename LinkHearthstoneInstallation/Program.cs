using CommonUtils;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace LinkHearthstoneInstallation
{
	class Program
	{
		static void Main(string[] args)
		{
			if (Directory.Exists(Constants.HearthstoneLinkDir))
			{
				ProgramUtils.ExitProgramWith("You already have a Hearthstone installation in this directory",
					$"If you want to link a different one, please delete or unlink your local {Constants.HearthstoneLinkDir} directory",
					"If this is a symlink, be careful! Don't delete your entire Hearthstone installation directory by mistake.");
			}

			Console.WriteLine("Scanning your local drives looking for a Hearthstone installation...");

			var hsDirs = HearthstoneUtils.FindHearthstoneCandidateDirectories();

			// Remove our own baseline HR dir from this if we happen to be near a drive root
			hsDirs.Remove(Path.Combine(Environment.CurrentDirectory, Constants.BaselineDir));

			if (hsDirs.Count == 0)
			{
				ProgramUtils.ExitProgramWith("Could not find your local Hearthstone installation.",
					"Please create the symlink yourself.",
					Constants.ExampleMkLinkMessage);
			}
			else if (hsDirs.Count > 1)
			{
				var messageHeader = new string[] {
				"Found multiple local Hearthstone installations",
					"Please create the symlink yourself with the correct one.",
					Constants.ExampleMkLinkMessage,
					"Hearthstone installations:" };

				var messageBody = hsDirs.ToArray();

				var message = Enumerable.Concat(messageHeader, messageBody).ToArray();

				ProgramUtils.ExitProgramWith(message);
			}

			var hsDir = hsDirs[0];

			Console.WriteLine($"Found a local Hearthstone installation in {hsDir}");

			//ProcessUtils.ExecuteProcess("mklink", $"/D {Constants.HearthstoneLinkDir} {hsDir}", Environment.CurrentDirectory);
			ProcessUtils.MkLinkDir(Constants.HearthstoneLinkDir, hsDir);
			Console.WriteLine("Symlink created");
		}
	}
}
