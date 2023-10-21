using DredgeVR.Helpers;
using Newtonsoft.Json;
using System;
using System.IO;

namespace DredgeVR.Options;

public static class OptionsManager
{
	public static OptionsConfig Options { get; private set; }

	public static void Load()
	{
		var optionsPath = Path.Combine(DredgeVRCore.ModPath, "options.json");

		try
		{
			Options = new();
			if (File.Exists(optionsPath))
			{
				JsonConvert.PopulateObject(File.ReadAllText(optionsPath), Options);
				DredgeVRLogger.Info("Loaded options file");
			}
			else
			{
				File.WriteAllText(optionsPath, JsonConvert.SerializeObject(Options, Formatting.Indented));
				DredgeVRLogger.Info("Created default options file");
			}
		}
		catch (Exception e)
		{
			DredgeVRLogger.Error($"Couldn't load options file: {e}");
		}
	}
}
