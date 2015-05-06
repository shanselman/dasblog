using System;

namespace DasBlog.Import
{
	/// <summary>
	/// An enumeration of the various blog sources that are supported.
	/// </summary>
	public enum BlogSourceType
	{
		// Lower case used below because it appears in the help text.
		// This is used so that we can default to none and error out.
		// Without this the default is Radio even if an invalid
		// source is specified on the command line.
		none,
		Radio
	}
}
