using Newtonsoft.Json;
using System;
using System.IO;
using Winch.Core;

namespace DredgeVR.Options;

public static class OptionsManager
{
	public static OptionsConfig Options { get; private set; }

	public static void Load()
	{
		try
		{
			Options = new();
			JsonConvert.PopulateObject(File.ReadAllText(Path.Combine(DredgeVRCore.ModPath, "options.json")), Options);
		}
		catch (Exception e)
		{
			WinchCore.Log.Error($"Couldn't load options file: {e}");
		}
	}
}
