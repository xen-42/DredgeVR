using System;
using System.Collections.Generic;
using UnityEngine;

namespace DredgeVR.Helpers;

public class DebugCommands : MonoBehaviour
{
	private List<(KeyCode, Action)> commands = new()
	{
		(KeyCode.Keypad1, TryInfect)
	};

	public void Update()
	{
		foreach (var (key, action) in commands) 
		{
			if (Input.GetKeyDown(key))
			{
				NotificationHelper.ShowNotificationWithColour(NotificationType.NONE, $"Invoked debug action {action.Method.Name}", DredgeColorTypeEnum.POSITIVE);
				try
				{
					action.Invoke();
				}
				catch (Exception ex)
				{
					NotificationHelper.ShowNotificationWithColour(NotificationType.NONE, $"Failed {ex}", DredgeColorTypeEnum.NEGATIVE);
				}
			}
		}
	}

	private static void TryInfect()
	{
		GameManager.Instance.GridManager.InfectRandomItemInInventory();
	}
}
