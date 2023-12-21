using Newtonsoft.Json;
using UnityEngine;

namespace DredgeVR.Options;

[JsonObject(MemberSerialization.OptIn)]
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
	/// True to use left hand for pointer, false for right.
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

	/// <summary>
	/// Keep this on unless you have a beast of a PC, not sure why but the terrain LOD just obliterates my frames.
	/// </summary>
	[JsonProperty]
	public bool lowerTerrainLOD = true;

	/// <summary>
	/// Gets rid of rocks and other details on the sea floor. Improves performance.
	/// </summary>
	[JsonProperty]
	public bool disableUnderseaDetails = true;

	/// <summary>
	/// Dredge has a custom LOD implementation that causes a lot of lag. Greatly improves performance but might use more memory? 
	/// </summary>
	[JsonProperty]
	public bool disableCullingBrain = true;

	/// <summary>
	/// Disables particles that aren't essential to gameplay. Improves performance
	/// </summary>
	[JsonProperty]
	public bool disableExtraParticleEffects = false;

	/// <summary>
	/// Disables particles effects over 100m away. Improves performance
	/// </summary>
	[JsonProperty]
	public bool disableDistantParticleEffects = true;

	/// <summary>
	/// Not ideal for now because it makes some rocks literally invisible, but helps with performance.
	/// </summary>
	[JsonProperty]
	public bool decreaseLOD = false;

	/// <summary>
	/// Improves perforemance
	/// </summary>
	[JsonProperty]
	public bool removeTrees = false;

	/// <summary>
	/// Do not mirror the VR view to the monitor
	/// </summary>
	[JsonProperty]
	public bool disableMonitor = false;

	/// <summary>
	/// Enable smooth rotation with the right thumbstick instead of snapping
	/// </summary>
	[JsonProperty]
	public bool smoothRotation = false;

	/// <summary>
	/// Changes the size of the player in the game world
	/// </summary>
	[JsonProperty]
	public float playerScale = 1f;

	[JsonProperty]
	public float[] playerPosition = new float[] { 0, 1.13f, -1.5f };

	public Vector3 PlayerPosition => new(playerPosition[0], playerPosition[1], playerPosition[2]);
}
