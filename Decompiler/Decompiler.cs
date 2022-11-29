using System;
using System.IO;
using System.Reflection;
using System.Security.Cryptography;
using CommonUtils;
using ICSharpCode.Decompiler;
using ICSharpCode.Decompiler.CSharp;
using ICSharpCode.Decompiler.CSharp.ProjectDecompiler;
using ICSharpCode.Decompiler.Metadata;

namespace Decompiler
{
	public class Decompiler
	{
		private readonly string _hearthstoneRoot;
		private readonly string _hearthstoneAssembliesPath;
		private readonly string _hearthstoneAssemblyCSharpPath;

		private readonly string _decompiledDir;
		private readonly string _decompiledAssembliesPath;
		private readonly string _decompiledAssemblyCSharpPath;

		private readonly string _decompiledAssemblyCSharpVSProjectDirectory;

		private const LanguageVersion HearthstoneLanguageVersion = LanguageVersion.CSharp7_3;

		private class DecompilationProgressIndicator : IProgress<DecompilationProgress>
		{
			private int done = 0;

			public void Report(DecompilationProgress value)
			{
				Console.WriteLine($"({++done}/{value.TotalNumberOfFiles}) {value.Status}");
			}
		}


		public Decompiler(string hearthstoneRoot, string decompiledDir)
		{
			_hearthstoneRoot = hearthstoneRoot;
			_hearthstoneAssembliesPath = HearthstoneUtils.GetAssembliesPath(hearthstoneRoot);
			_hearthstoneAssemblyCSharpPath = HearthstoneUtils.GetAssemblyCSharpDllPath(_hearthstoneRoot);

			_decompiledDir = decompiledDir;
			_decompiledAssembliesPath = Path.Combine(_decompiledDir, "Hearthstone_Data", "Managed");
			_decompiledAssemblyCSharpPath = Path.Combine(_decompiledAssembliesPath, Constants.AssemblyCSharpDll);
			_decompiledAssemblyCSharpVSProjectDirectory = Path.Combine(decompiledDir, Constants.AssemblyCSharpVSProjectName);
		}

		public void Run()
		{
			// Make sure we're not attempting to decompile Hearthstone Access by mistake
			if (DoesHearthstoneDirectoryNeedRepair())
			{
				ProgramUtils.ExitProgramWith("Your local version of Hearthstone is currently patched with Hearthstone Access.",
					"Please use the Blizzard app to repair your local Hearthstone installation first.");
			}

			Console.WriteLine("Setting up your development environment for Hearthstone Access...");

			// Remove any dependencies we may have added (e.g. Tolk.dll etc) to HS Managed dir during development
			RestoreHearthstone();

			// Extract HS version and checksum
			PrintHearthstoneAssemblyCSharpChecksum();

			Console.WriteLine("Extracting Hearthstone version");
			var hsVersion = HearthstoneUtils.ExtractHearthstoneVersion(_hearthstoneRoot);

			// Create a new branch for the new Hearthstone version in the local Decompiled directory
			PrepareDecompiledDirectory(hsVersion);

			// Extract relevant files and dependencies from the Hearthstone directory into our Decompiled directory
			ExtractHearthstoneFiles();

			DecompileHearthstone();

			DecompilationUtils.PreffityDecompiledProject(_decompiledAssemblyCSharpVSProjectDirectory);

			CommitLocalDecompiledDirectory();

			Console.WriteLine($"Successfully decompiled Hearthstone version {hsVersion} into {_decompiledDir}");
		}

		private void CommitLocalDecompiledDirectory()
		{
			Console.WriteLine("Commiting local project...");

			GitUtils.CommitAll(_decompiledDir, "Decompiled");
		}

		private void PrepareDecompiledDirectory(string hsVersion)
		{
			ValidateDecompiledDirectoryBranch(hsVersion);

			Console.WriteLine($"Creating new branch in the decompiled directory for HS version {hsVersion}");

			GitUtils.CheckoutNewBranch(_decompiledDir, hsVersion);

			CleanupDecompiledDirectory();
		}

		private void ValidateDecompiledDirectoryBranch(string hsVersion)
		{
			CheckIfUserForgotToSwitchBackToHSBranchBeforeDecompiling();
			CheckIfThisVersionOfHearthstoneHasAlreadyBeenDecompiled(hsVersion);
		}

		private void CheckIfUserForgotToSwitchBackToHSBranchBeforeDecompiling()
		{
			var curBranch = GitUtils.GetCurrentBranchName(_decompiledDir);

			if (curBranch.Contains("-"))
			{
				ProgramUtils.ExitProgramWith("Your decompiled directory doesn't seem to be in a valid Hearthstone branch.",
					"Aborting in case you forgot to commit any local changes.",
					"Make sure you're in a directory named after a Hearthstone version and not in your local HSA directory.");
			}
		}

		private void CheckIfThisVersionOfHearthstoneHasAlreadyBeenDecompiled(string hsVersion)
		{
			if (GitUtils.BranchExists(_decompiledDir, hsVersion))
			{
				ProgramUtils.ExitProgramWith("You've already decompiled your locally installed Hearthstone version.",
					$"If you want to decompile again, please delete the {hsVersion} branch from your decompiled directory and run this program again.");
			}
		}

		private void RestoreHearthstone()
		{
			Console.WriteLine("Restoring Hearthstone...");

			File.Delete(Path.Combine(_hearthstoneAssembliesPath, "TolkDotNet.dll"));
			File.Delete(Path.Combine(_hearthstoneAssembliesPath, "dolapi32.dll"));
			File.Delete(Path.Combine(_hearthstoneAssembliesPath, "nvdaControllerClient32.dll"));
			File.Delete(Path.Combine(_hearthstoneAssembliesPath, "SAAPI32.dll"));
			File.Delete(Path.Combine(_hearthstoneAssembliesPath, "Tolk.dll"));

			string accessibilityDirPath = Path.Combine(_hearthstoneAssembliesPath, "Accessibility");
			if (Directory.Exists(accessibilityDirPath))
			{
				Directory.Delete(accessibilityDirPath, true);
			}
		}

		private void PrintHearthstoneAssemblyCSharpChecksum()
		{
			Console.WriteLine("Extracting Assembly-CSharp.dll checksum...");

			SHA1 sha = new SHA1CryptoServiceProvider();

			byte[] hash = sha.ComputeHash(File.ReadAllBytes(_hearthstoneAssemblyCSharpPath));

			string hex = BitConverter.ToString(hash);
			string hearthstoneAssemblyCSharpChecksum = hex.Replace("-", string.Empty);

			Console.WriteLine($"Hearthstone Assembly-CSharp.dll checksum: {hearthstoneAssemblyCSharpChecksum}");
		}

		private void CleanupDecompiledDirectory()
		{
			Console.WriteLine("Cleaning up your Decompilation directory");

			DirectoryInfo decompiledDirectory = new DirectoryInfo(_decompiledDir);

			GitUtils.CleanupDirectory(decompiledDirectory);

			// Create VS project dir
			var projectDir = new DirectoryInfo(_decompiledAssemblyCSharpVSProjectDirectory);
			projectDir.Create();
		}

		private void ExtractHearthstoneFiles()
		{
			Console.WriteLine($"Extracting Hearthstone files into the {Constants.DecompiledDir} directory to compile against...");

			DirectoryInfo srcManagedDataDir = new DirectoryInfo(_hearthstoneAssembliesPath);
			DirectoryInfo targetManagedDataDir = new DirectoryInfo(_decompiledAssembliesPath);

			if (!srcManagedDataDir.Exists)
			{
				// Should never happen since we validate the directory at the start
				throw new SystemException("Hearthstone Managed Data folder does not exist");
			}
			else if (!targetManagedDataDir.Exists)
			{
				targetManagedDataDir.Create();
			}

			GitUtils.CleanupDirectory(targetManagedDataDir); // Unlikely to be an actual git directory, but someone might find it useful

			// Copy all files
			FileInfo[] srcFiles = srcManagedDataDir.GetFiles();
			foreach (FileInfo file in srcFiles)
			{
				file.CopyTo(Path.Combine(_decompiledAssembliesPath, file.Name));
			}

			// Copy .product.db
			File.Copy(HearthstoneUtils.GetProductDbPath(_hearthstoneRoot), HearthstoneUtils.GetProductDbPath(_decompiledDir), true);
		}

		private void DecompileHearthstone()
		{
			Console.WriteLine("Decompiling Hearthstone...");

			PEFile module = new PEFile(_decompiledAssemblyCSharpPath);
			var resolver = new UniversalAssemblyResolver(_decompiledAssemblyCSharpPath, false, module.Reader.DetectTargetFrameworkId());
			resolver.AddSearchDirectory(_decompiledAssembliesPath);
			DecompilerSettings decompilerSettings = new DecompilerSettings(HearthstoneLanguageVersion)
			{
				UseSdkStyleProjectFormat = WholeProjectDecompiler.CanUseSdkStyleProjectFormat(module)
			};

			var decompiler = new WholeProjectDecompiler(decompilerSettings, resolver, resolver, null);
			IProgress<DecompilationProgress> progressIndicator = new DecompilationProgressIndicator();
			decompiler.ProgressIndicator = progressIndicator;
			decompiler.DecompileProject(module, _decompiledAssemblyCSharpVSProjectDirectory);

			Console.WriteLine($"Successfully decompiled Hearthstone into {_decompiledAssemblyCSharpVSProjectDirectory}");
		}

		private bool DoesHearthstoneDirectoryNeedRepair()
		{
			return Assembly.LoadFrom(_hearthstoneAssemblyCSharpPath).GetType("Accessibility.HearthstoneAccessConstants") != null;
		}
	}
}

