using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Decompiler
{
	class FilePrettier
	{
		private FileInfo _file;
		private string _content;

		internal FilePrettier(FileInfo file)
		{
			_file = file;
		}

		internal void Prettify()
		{
			_content = File.ReadAllText(_file.FullName);
			var contentBeforePrettify = _content;

			DeleteImplicitBaseConstructors();
			FixBrokenSymbols();

			if (!_content.Equals(contentBeforePrettify))
			{
				File.WriteAllText(_file.FullName, _content);
			}
		}

		private void FixBrokenSymbols()
		{
			_content = _content.Replace("_003C", "<")
				.Replace("_003E", ">");
		}

		private void DeleteImplicitBaseConstructors()
		{
			_content = _content.Replace("base._002Ector();", "");
		}
	}
}
