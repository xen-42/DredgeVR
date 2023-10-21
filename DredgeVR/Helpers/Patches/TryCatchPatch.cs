using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Winch.Core;

namespace DredgeVR.Helpers;

/// <summary>
/// Not sure why, but Winch does not log when exceptions are thrown by methods
/// This patch doesn't report proper line numbers unfortunately, but it will record which method is throwing the exception at least
/// 
/// This patching will decrease performance, so do not include it in release builds
/// </summary>
# if DEBUG
[HarmonyPatch]
internal static class TryCatchPatch
{
	[HarmonyTargetMethods]
	public static IEnumerable<MethodBase> TargetMethods()
	{
		return Assembly.GetExecutingAssembly()
			.GetTypes()
			.Where(x => x.GetCustomAttribute(typeof(HarmonyPatch)) == null && !x.IsAbstract)
			.SelectMany(AccessTools.GetDeclaredMethods)
			.Where(x => !x.IsGenericMethod)
			.Distinct();
	}

	[HarmonyFinalizer]
	public static Exception HandleException(Exception __exception)
	{
		if (__exception != null)
		{
			DredgeVRLogger.Error(__exception);
		}
		return __exception;
	}
}
#endif
