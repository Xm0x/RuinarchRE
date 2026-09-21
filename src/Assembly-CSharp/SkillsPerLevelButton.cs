using System;
using UnityEngine;
using UnityEngine.UI;

public class SkillsPerLevelButton : MonoBehaviour
{
	public Text buttonText;

	[NonSerialized]
	public LevelCollapseUI collapseUI;

	public void SetCurrentlySelectedButton()
	{
		collapseUI.currentSelectedButton = this;
	}
}
