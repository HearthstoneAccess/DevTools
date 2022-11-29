using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Decompiler
{
	class DecompilationUtils
	{
		internal static void PreffityDecompiledProject(string projectDir)
		{
			PrettifyCsFiles(projectDir);

			NormalizeW8TouchIfNeeded(projectDir);
		}

		private static void PrettifyCsFiles(string projectDir)
		{
			foreach (var file in GetCsFilesInProject(projectDir))
			{
				PrettifyCsFile(file);
			}
		}

		private static void PrettifyCsFile(FileInfo file)
		{
			new FilePrettier(file).Prettify();
		}

		private static List<FileInfo> GetCsFilesInProject(string projectDir)
		{
			DirectoryInfo dir = new DirectoryInfo(projectDir);
			List<FileInfo> files = GetFilesInDir(dir);

			files.RemoveAll(f => !f.Extension.Equals(".cs"));
			return files;
		}

		private static List<FileInfo> GetFilesInDir(DirectoryInfo dir)
		{
			List<FileInfo> files = new List<FileInfo>();

			files.AddRange(dir.GetFiles());

			foreach (var childDir in dir.GetDirectories())
			{
				files.AddRange(GetFilesInDir(childDir));
			}

			return files;
		}

		private static void NormalizeW8TouchIfNeeded(string projectDir)
		{
			var w8Path = Path.Combine(projectDir, "W8Touch.cs");

			if (!File.Exists(w8Path))
			{
				return;
			}

			var content = File.ReadAllText(w8Path);

			var normalizedLayoutKindStr = "[StructLayout(LayoutKind.Sequential,";

			var normalizedContent = content.Replace("[StructLayout(0,", normalizedLayoutKindStr)
				.Replace("[StructLayout((short)0,", normalizedLayoutKindStr);

			if (content.Equals(normalizedContent))
			{
				File.WriteAllText(w8Path, normalizedContent);
			}
		}
	}
}
