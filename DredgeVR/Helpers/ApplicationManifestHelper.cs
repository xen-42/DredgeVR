using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Valve.VR;

namespace DredgeVR.Helpers;

/// <summary>
/// From NomaiVR, thanks Rai! Unless Artum wrote it
/// The application manifest tells SteamVR about our mod
/// </summary>
internal class ApplicationManifestHelper
{
	public static void UpdateManifest(string manifestPath, string appKey, string imagePath, string name, string description, int steamAppId = 0, bool steamBuild = false)
	{
		try
		{
			var launchType = steamBuild ? GetSteamLaunchString(steamAppId) : GetBinaryLaunchString();
			var appManifestContent = $@"{{
                                            ""source"": ""builtin"",
                                            ""applications"": [{{
                                                ""app_key"": {JsonConvert.ToString(appKey)},
                                                ""image_path"": {JsonConvert.ToString(imagePath)},
                                                {launchType}
                                                ""last_played_time"":""{CurrentUnixTimestamp()}"",
                                                ""strings"": {{
                                                    ""en_us"": {{
                                                        ""name"": {JsonConvert.ToString(name)}
                                                    }}
                                                }}
                                            }}]
                                        }}";

			// Appearances matter to me okay
			var dictContent = JsonConvert.DeserializeObject<Dictionary<string, string>>(appManifestContent);
			var prettyString = JsonConvert.SerializeObject(dictContent, Formatting.Indented);

			File.WriteAllText(manifestPath, prettyString);

			var error = OpenVR.Applications.AddApplicationManifest(manifestPath, false);
			if (error != EVRApplicationError.None)
			{
				DredgeVRLogger.Error("Failed to set AppManifest " + error);
			}

			var processId = System.Diagnostics.Process.GetCurrentProcess().Id;
			var applicationIdentifyErr = OpenVR.Applications.IdentifyApplication((uint)processId, appKey);
			if (applicationIdentifyErr != EVRApplicationError.None)
			{
				DredgeVRLogger.Error("Error identifying application: " + applicationIdentifyErr.ToString());
			}
		}
		catch (Exception exception)
		{
			DredgeVRLogger.Error("Error updating AppManifest: " + exception);
		}
	}

	private static string GetSteamLaunchString(int steamAppId)
	{
		return $@"""launch_type"": ""url"",
                      ""url"": ""steam://launch/{steamAppId}/VR"",";
	}

	private static string GetBinaryLaunchString()
	{
		var workingDir = Directory.GetCurrentDirectory();
		var executablePath = Assembly.GetExecutingAssembly().Location;
		return $@"""launch_type"": ""binary"",
                      ""binary_path_windows"": {JsonConvert.ToString(executablePath)},
                      ""working_directory"": {JsonConvert.ToString(workingDir)},";
	}

	private static long CurrentUnixTimestamp()
	{
		var foo = DateTime.Now;
		return ((DateTimeOffset)foo).ToUnixTimeSeconds();
	}
}
