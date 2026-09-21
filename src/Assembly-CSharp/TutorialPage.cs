using System;
using UnityEngine;
using UnityEngine.Serialization;

[Serializable]
public class TutorialPage
{
	public Sprite imgTutorial;

	[FormerlySerializedAs("description")]
	public string descriptionKey;

	public string controllerDescriptionKey;

	public KeybindFillerDictionary keybindFillers;
}
