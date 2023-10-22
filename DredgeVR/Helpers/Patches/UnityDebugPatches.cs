using HarmonyLib;
using System;
using UnityEngine;

namespace DredgeVR.Helpers;

#if DEBUG
[HarmonyPatch(typeof(Debug))]
internal static class DebugLogPatches
{
	[HarmonyPostfix]
	[HarmonyPatch(nameof(Debug.Log), new Type[] { typeof(object) })]
	public static void Debug_Log(object message) => DredgeVRLogger.Info($"[UnityEngine.Debug.Log] {message}");

	[HarmonyPostfix]
	[HarmonyPatch(nameof(Debug.LogWarning), new Type[] { typeof(object) })]
	public static void Debug_LogWarning(object message) => DredgeVRLogger.Warn($"[UnityEngine.Debug.Log] {message}");

	[HarmonyPostfix]
	[HarmonyPatch(nameof(Debug.LogError), new Type[] { typeof(object) })]
	public static void Debug_LogError(object message) => DredgeVRLogger.Error($"[UnityEngine.Debug.Log] {message}");

	[HarmonyPostfix]
	[HarmonyPatch(nameof(Debug.LogException), new Type[] { typeof(Exception) })]
	public static void Debug_LogException(Exception exception) => DredgeVRLogger.Error($"[UnityEngine.Debug.Log] {exception}");
}
#endif
