using HarmonyLib;
using InControl;

namespace DredgeVR.VRInput.Patches;

[HarmonyPatch(typeof(KeyBindingSource))]
public static class KeyBindingSourcePatches
{
	[HarmonyPostfix]
	[HarmonyPatch(nameof(KeyBindingSource.GetState))]
	public static void KeyBindingSource_GetState(KeyBindingSource __instance, ref bool __result)
	{
		if (VRInputManager.KeyMapping.TryGetValue(__instance.Control.GetInclude(0), out var vrBinding))
		{
			__result = VRInputManager.State[vrBinding] || __result;
		}
	}
}
