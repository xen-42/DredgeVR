using Newtonsoft.Json;

namespace DredgeVR.Options;

[JsonObject]
public class OptionsConfig
{
	[JsonProperty]
	public bool removeWaves;

	[JsonProperty]
	public bool lockViewToHorizon;

	[JsonProperty]
	public bool lockCameraYPosition;

	[JsonProperty]
	public bool lowerTerrainLOD;

	[JsonProperty]
	public bool leftHand;
}
