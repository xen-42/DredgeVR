﻿using DredgeVR.Helpers;
using Newtonsoft.Json;
using System;
using System.IO;
using UnityEngine.XR;
using Valve.VR;

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
				DredgeVRLogger.Info("Created default options file");
			}
			// Always write the contents so that if there's been an update they'll get the new settings in their config
			File.WriteAllText(optionsPath, JsonConvert.SerializeObject(Options, Formatting.Indented));
		}
		catch (Exception e)
		{
			DredgeVRLogger.Error($"Couldn't load options file: {e}");
		}
	}
}
