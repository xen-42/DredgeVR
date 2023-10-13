using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Winch.Core;

namespace DredgeVR.Helpers;

//[HarmonyPatch]
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
	public static void HandleException(Exception __exception)
	{
		if (__exception == null)
		{
			WinchCore.Log.Error($"\nException is somehow null. StackTrace: {Environment.StackTrace}\n");
		}
		else
		{
			WinchCore.Log.Error(__exception);
		}
	}
}
