using UnityEngine;

namespace DredgeVR.Tutorial;

/// <summary>
/// In the future will tutorialize VR elements
/// For now just disables tutorials that aren't working
/// </summary>
public class VRTutorialManager : MonoBehaviour
{
	public void Awake()
	{
		DredgeVRCore.GameSceneStart += OnGameSceneStart;
	}

	public void OnDestroy()
	{
		DredgeVRCore.GameSceneStart -= OnGameSceneStart;
	}

	private void OnGameSceneStart()
	{
		// Marks the movement and mouse moving tutorials as completed
		var stepIDs = new TutorialStepEnum[] { TutorialStepEnum.TUTORIAL_010, TutorialStepEnum.TUTORIAL_020 };
		foreach (var stepID in stepIDs)
		{
			GameManager.Instance.SaveData.SetBoolVariable($"tutorial-step-complete-{stepID}", true);
		}
	}
}
