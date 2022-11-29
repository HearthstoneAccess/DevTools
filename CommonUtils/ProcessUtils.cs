using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace CommonUtils
{
	public class ProcessUtils
	{
		public static int ExecuteProcessWithReturnCode(string filename, string args, string workDir)
		{
			List<string> stdout = new List<string>();

			return ExecuteProcessWithReturnCode(filename, args, workDir, stdout);
		}

		public static int ExecuteProcessWithReturnCode(string filename, string args, string workDir, List<string> stdout)
		{
			var workDirBeforeExec = Environment.CurrentDirectory;
			try
			{
				Environment.CurrentDirectory = workDir;

				var proc = new Process
				{
					StartInfo = new ProcessStartInfo
					{
						FileName = filename,
						Arguments = args,
						UseShellExecute = false,
						RedirectStandardOutput = true,
						RedirectStandardError = true,
						CreateNoWindow = true
					}
				};

				Console.WriteLine($"WorkDir: {workDir}");
				Console.WriteLine($"> {filename} {args}");

				proc.Start();
				while (!proc.StandardOutput.EndOfStream)
				{
					string line = proc.StandardOutput.ReadLine();
					stdout.Add(line);
					Console.WriteLine($"[STDOUT]: {line}");
				}

				while (!proc.StandardError.EndOfStream)
				{
					string line = proc.StandardError.ReadLine();
					Console.WriteLine($"[STDERR] {line}");
				}

				proc.WaitForExit();

				return proc.ExitCode;
			}
			finally
			{
				Environment.CurrentDirectory = workDirBeforeExec;
			}
		}

		public static void ExecuteProcess(string filename, string args, string workDir)
		{
			int exitCode = ExecuteProcessWithReturnCode(filename, args, workDir);
			if (exitCode != 0)
			{
				throw new SystemException($"Process ended with exit code {exitCode}");
			}
		}

		public static void MkLinkDir(string link, string targetDir)
		{
			ExecuteProcess("cmd.exe", $"/c mklink /D {link} \"{targetDir}\"", Environment.CurrentDirectory);
		}
	}
}
