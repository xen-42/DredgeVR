using Newtonsoft.Json;

namespace DredgeVR.Options;

[JsonObject]
public class OptionsConfig
{
	/// <summary>
	/// Removes all waves from the Game scene to avoid sea-sickness
	/// </summary>
	[JsonProperty]
	public bool removeWaves = true;

	/// <summary>
	/// Locks the rotation of the player camera so that it doesn't move relative to the horizon. Helps with motion sickness.
	/// </summary>
	[JsonProperty]
	public bool lockViewToHorizon = true;

	/// <summary>
	/// Locks the y position of the player camera so that it doesn't move up and down with the boat. Helps with motion sickness.
	/// </summary>
	[JsonProperty]
	public bool lockCameraYPosition = true;

	/// <summary>
	/// Keep this on unless you have a beast of a PC, not sure why but the terrain LOD just obliterates my frames.
	/// </summary>
	[JsonProperty]
	public bool lowerTerrainLOD = true;

	/// <summary>
	/// True to use left hand for pointer, false for right. Also mirrors controls.
	/// </summary>
	[JsonProperty]
	public bool leftHanded;

	/// <summary>
	/// Keeps UI flat in front of the player but interactable with the laser pointer. Useful if your controllers do not have enough buttons for all the inputs (i.e., Vive)
	/// </summary>
	[JsonProperty]
	public bool useFlatUI;

	/// <summary>
	/// Improves frames for me
	/// </summary>
	[JsonProperty]
	public bool lockPhysicsUpdateRateToRenderFrequency = true;
}
