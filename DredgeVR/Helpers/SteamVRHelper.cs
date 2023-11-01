using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Valve.VR;

namespace DredgeVR.Helpers;

public static class SteamVRHelper
{
	public static T GetSubSystem<T>() where T : ISubsystem
	{
		var subsystems = new List<T>();
		SubsystemManager.GetSubsystems(subsystems);
		return subsystems.FirstOrDefault();
	}

	public static Matrix4x4 ToMatrix4x4(this HmdMatrix44_t hmdMatrix)
	{
		var m = Matrix4x4.identity;
		m.m00 = hmdMatrix.m0;
		m.m01 = hmdMatrix.m1;
		m.m02 = hmdMatrix.m2;
		m.m03 = hmdMatrix.m3;
		m.m10 = hmdMatrix.m4;
		m.m11 = hmdMatrix.m5;
		m.m12 = hmdMatrix.m6;
		m.m13 = hmdMatrix.m7;
		m.m20 = hmdMatrix.m8;
		m.m21 = hmdMatrix.m9;
		m.m22 = hmdMatrix.m10;
		m.m23 = hmdMatrix.m11;
		m.m30 = hmdMatrix.m12;
		m.m31 = hmdMatrix.m13;
		m.m32 = hmdMatrix.m14;
		m.m33 = hmdMatrix.m15;

		return m;
	}
}
