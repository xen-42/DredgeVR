using System;
using System.Collections;
using UnityEngine;

namespace DredgeVR.Helpers;

/// <summary>
/// Taken from New Horizons.
/// It's not stealing when you steal from yourself.
/// 
/// These coroutines will not persist between scenes.
/// </summary>
internal static class Delay
{
	public static void FireOnNextUpdate(Action action) => DredgeVRCore.Instance.StartCoroutine(FireOnNextUpdateEnumerator(action));

	private static IEnumerator FireOnNextUpdateEnumerator(Action action)
	{
		yield return null;
		action?.Invoke();
	}

	public static void RunWhen(Func<bool> predicate, Action action) => DredgeVRCore.Instance.StartCoroutine(RunWhenEnumerator(predicate, action));

	private static IEnumerator RunWhenEnumerator(Func<bool> prediate, Action action)
	{
		yield return new WaitUntil(prediate);
		action?.Invoke();
	}

	public static void FireInNUpdates(int n, Action action) => DredgeVRCore.Instance.StartCoroutine(FireInNUpdatesEnumerator(n, action));

	private static IEnumerator FireInNUpdatesEnumerator(int n, Action action)
	{
		for (int i = 0; i < n; i++)
		{
			yield return null;
		}
		action?.Invoke();
	}
}
