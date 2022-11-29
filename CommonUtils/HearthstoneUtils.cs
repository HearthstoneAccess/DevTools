using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace CommonUtils
{
	public class HearthstoneUtils
	{
		// HS files
		private static string s_hearthstoneDirName = "Hearthstone";
		private static string s_hearthstoneDataDirName = "Hearthstone_Data";
		private static string s_hearthstoneAssembliesRelativePath = Path.Combine(s_hearthstoneDataDirName, "Managed");
		private static string s_hearthstoneAssemblyCSharpDllRelativePath = Path.Combine(s_hearthstoneAssembliesRelativePath, "Assembly-CSharp.dll");
		private static string s_hearthstoneProductDbFileName = ".product.db";

		public static string HearthstoneDirTypicalLocation = Path.Combine(GetProgramFilesX86Path(), s_hearthstoneDirName);

		private static int s_findHearthstoneDirSearchDepth = 3; // Should be more than enough for most and complexity grows steeply beyond this

		private static string GetProgramFilesX86Path()
		{
			if (Environment.Is64BitOperatingSystem)
			{
				return Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86);
			}
			else
			{
				return Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
			}
		}

		public static bool IsHearthstoneDirectory(string path)
		{
			if (!Directory.Exists(path))
			{
				return false;
			}

			var assemblyCSharpDllPath = GetAssemblyCSharpDllPath(path);
			var productDbPath = GetProductDbPath(path);

			return File.Exists(assemblyCSharpDllPath) && File.Exists(productDbPath);
		}

		public static string GetAssembliesPath(string hsDir)
		{
			return Path.Combine(hsDir, s_hearthstoneAssembliesRelativePath);
		}

		public static string GetAssemblyCSharpDllPath(string hsDir)
		{
			return Path.Combine(hsDir, s_hearthstoneAssemblyCSharpDllRelativePath);
		}

		public static string GetProductDbPath(string hsDir)
		{
			return Path.Combine(hsDir, s_hearthstoneProductDbFileName);
		}

		public static string ExtractHearthstoneVersion(string hsDir)
		{
			string productDbPath = Path.Combine(hsDir, s_hearthstoneProductDbFileName);
			if (!File.Exists(productDbPath))
			{
				throw new SystemException($"Could not find {s_hearthstoneProductDbFileName} file at {productDbPath}");
			}

			string productInfo = File.ReadAllText(productDbPath, Encoding.ASCII);

			Regex regex = new Regex("([0-9]+\\.[0-9]+\\.[0-9]+\\.[0-9]+)");
			var versions = regex.Matches(productInfo);
			if (versions.Count != 1)
			{
				throw new SystemException($"Could not extract version from {s_hearthstoneProductDbFileName}");
			}

			return versions[0].Value;
		}

        public static List<string> FindHearthstoneCandidateDirectories()
        {
            var ret = new List<string>();

            var drives = DriveInfo.GetDrives();

            var candidateDirs = new List<string>();

            // Find all dirs named Hearthstone in all drives
            foreach (var drive in drives)
            {
                candidateDirs.AddRange(FindSubdirectories(drive.RootDirectory, s_hearthstoneDirName, s_findHearthstoneDirSearchDepth));
            }

            // Filter out the Hearthstone dirs that appear to be real HS dirs
            foreach (var dir in candidateDirs)
            {
                if (IsHearthstoneDirectory(dir))
                {
                    ret.Add(dir);
                }
            }

            return ret;
        }

		private static IEnumerable<string> FindSubdirectories(DirectoryInfo dir, string name, int searchDepth)
        {
            if (searchDepth <= 0)
            {
                yield break;
            }

            DirectoryInfo[] childDirs = new DirectoryInfo[0];

            try
            {
                childDirs = dir.GetDirectories();
            }
            catch
            {
                // Protect against permissions etc
            }

            foreach (var childDir in childDirs)
            {
                if (childDir.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase))
                {
                    yield return childDir.FullName;
                }

                if (!childDir.Attributes.HasFlag(FileAttributes.Hidden)) // TODO: Find a nice way to handle potential symlinks and blacklist Windows dirs etc
                {
                    // Do both as we may have e.g. Hearthstone/Hearthstone/etc. in secondary drives
                    foreach (var grandchildDir in FindSubdirectories(childDir, name, searchDepth - 1))
                    {
                        yield return grandchildDir;
                    }
                }
            }
        }
	}
}
