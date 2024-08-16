using HarmonyLib;
using System;

namespace DredgeVR.DockNavigation;

[HarmonyPatch]
public class ActionDestination : BaseDestination
{
    private Action _action;

    public void SetUp(Action action, string name)
    {
        this._action = action;
        this.id = name;

        this.alwaysShow = true;
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(DestinationButton), nameof(DestinationButton.OnButtonClicked))]
    private static bool DestinationButton_OnButtonClicked(DestinationButton __instance)
    {
        if (__instance.destination is ActionDestination actionDestination)
        {
            actionDestination._action?.Invoke();
            return false;
        }
        else
        {
            return true;
        }
    }
}

