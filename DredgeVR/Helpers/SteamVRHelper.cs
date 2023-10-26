using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace DredgeVR.Helpers;

public static class SteamVRHelper
{
	public static T GetSubSystem<T>() where T : ISubsystem
	{
		var subsystems = new List<T>();
		SubsystemManager.GetSubsystems(subsystems);
		return subsystems.FirstOrDefault();
	}
}
