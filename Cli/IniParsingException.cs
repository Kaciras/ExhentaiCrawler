using System;
using System.Collections.Generic;
using System.Text;

namespace Cli
{
	public class IniParsingException : Exception
	{
		internal IniParsingException(string message) : base(message) { }
	}
}
