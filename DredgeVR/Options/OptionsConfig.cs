using Newtonsoft.Json;

namespace DredgeVR.Options;

[JsonObject]
public class OptionsConfig
{
	[JsonProperty]
	public bool removeWaves = true;

	[JsonProperty]
	public bool lockViewToHorizon = true;

	[JsonProperty]
	public bool lockCameraYPosition = true;

	[JsonProperty]
	public bool lowerTerrainLOD = true;

	[JsonProperty]
	public bool leftHanded;

	[JsonProperty]
	public bool useFlatUI;
}
