using System;
using System.Collections.Generic;
using System.Text;

namespace CommonUtils
{
	public class Constants
	{
		public static string HearthstoneLinkDir = "Hearthstone";

		public static string DecompiledDir = "Decompiled";

		public static string BaselineDir = "SetupBaseline";

		public static string PatchFileName = "diff.patch";

		public static string AssemblyCSharpVSProjectName = "Assembly-CSharp";

		public static string AssemblyCSharpDll = $"{AssemblyCSharpVSProjectName}.dll";

		public static string ExampleMkLinkMessage = $"Example: mklink /D {HearthstoneLinkDir} \"{HearthstoneUtils.HearthstoneDirTypicalLocation}\"";
	}
}
