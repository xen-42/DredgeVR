using HarmonyLib;
using InControl;
using UnityEngine;

namespace DredgeVR.VRInput.Patches;

[HarmonyPatch]
public static class TwoAxisInputControlPatches
{
	[HarmonyPostfix]
	[HarmonyPatch(typeof(TwoAxisInputControl), nameof(TwoAxisInputControl.Value), MethodType.Getter)]
	public static void TwoAxisInputControl_Value(TwoAxisInputControl __instance, ref Vector2 __result)
	{
		if (GetVector(__instance) is Vector2 vector2)
		{
			__result = vector2;
		}
	}

	[HarmonyPostfix]
	[HarmonyPatch(typeof(TwoAxisInputControl), nameof(TwoAxisInputControl.Vector), MethodType.Getter)]
	public static void TwoAxisInputControl_Vector(TwoAxisInputControl __instance, ref Vector2 __result)
	{
		if (GetVector(__instance) is Vector2 vector2)
		{
			__result = vector2;
		}
	}

	[HarmonyPostfix]
	[HarmonyPatch(typeof(TwoAxisInputControl), nameof(TwoAxisInputControl.Angle), MethodType.Getter)]
	public static void TwoAxisInputControl_Angle(TwoAxisInputControl __instance, ref float __result)
	{
		if (GetVector(__instance) is Vector2 vector2)
		{
			__result = Utility.VectorToAngle(vector2);
		}
	}

	private static Vector2? GetVector(TwoAxisInputControl action)
	{
		var controls = GameManager.Instance?.Input?.Controls;
		if ((action == controls.Move || action == controls.MoveMap) && VRInputManager.LeftThumbStick != Vector2.zero)
		{
			return ApplyDeadZone(action, VRInputManager.LeftThumbStick);
		}
		if ((action == controls.RadialSelect || action == controls.CameraMove) && VRInputManager.RightThumbStick != Vector2.zero)
		{
			return ApplyDeadZone(action, VRInputManager.RightThumbStick);
		}
		return null;
	}

	private static Vector2 ApplyDeadZone(TwoAxisInputControl action, Vector2 vector)
	{
		return action.DeadZoneFunc(vector.x, vector.y, action.LowerDeadZone, action.UpperDeadZone);
	}
}
