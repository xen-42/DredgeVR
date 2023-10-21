using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
	public bool limitLOD;

	[JsonProperty]
	public bool leftHand;
}
