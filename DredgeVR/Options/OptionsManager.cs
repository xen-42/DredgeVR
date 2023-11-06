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
				try
				{
					JsonConvert.PopulateObject(File.ReadAllText(optionsPath), Options);
					DredgeVRLogger.Info("Loaded options file");
				}
				catch (Exception e)
				{
					// Will go on to replace it with the defaults
					DredgeVRLogger.Error($"Couldn't load options file: {e}");
				}
			}
			else
			{
				DredgeVRLogger.Info("Created default options file");
			}
			// Always write the contents so that if there's been an update they'll get the new settings in their config
			File.WriteAllText(optionsPath, JsonConvert.SerializeObject(Options, Formatting.Indented));
		}
		catch (Exception e)
		{
			DredgeVRLogger.Error($"Something went wrong when loading/creating options file: {e}");
		}
	}
}
