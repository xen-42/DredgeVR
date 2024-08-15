namespace DredgeVR.Helpers;

public static class DLCHelper
{
	public static bool OwnsThePaleReach() => GameManager.Instance.EntitlementManager.GetHasEntitlement(Entitlement.DLC_1);
	public static bool OwnsTheIronRig() => GameManager.Instance.EntitlementManager.GetHasEntitlement(Entitlement.DLC_2);
}
