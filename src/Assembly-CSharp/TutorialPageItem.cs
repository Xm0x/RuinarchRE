using System.Collections.Generic;
using EZObjectPools;
using Maccima_Games.Util;
using Ruinarch;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class TutorialPageItem : PooledObject
{
	[SerializeField]
	private Image imgMain;

	[SerializeField]
	private TextMeshProUGUI lblDescription;

	public void Initialize(TutorialPage p_page)
	{
		imgMain.sprite = p_page.imgTutorial;
		UpdateTutorialTexts(p_page);
	}

	public void UpdateTutorialTexts(TutorialPage p_page)
	{
		if (p_page.keybindFillers.Count > 0)
		{
			Dictionary<string, string> dictionary = MaccimaDictionaryPool<string, string>.Claim(p_page.keybindFillers.Count);
			foreach (KeyValuePair<string, InputActionReference> keybindFiller in p_page.keybindFillers)
			{
				string text = (InputManager.Instance.isUsingGamepad ? "Gamepad" : "Keyboard");
				string bindingDisplayString = keybindFiller.Value.action.GetBindingDisplayString(InputBinding.DisplayStringOptions.DontIncludeInteractions, text);
				dictionary.Add(keybindFiller.Key, bindingDisplayString);
			}
			string key = ((!InputManager.Instance.isUsingGamepad || string.IsNullOrEmpty(p_page.controllerDescriptionKey)) ? p_page.descriptionKey : p_page.controllerDescriptionKey);
			lblDescription.text = LocalizationManager.Instance.GetLocalizedValue("Tutorials_Table", key, dictionary);
			MaccimaDictionaryPool<string, string>.Release(dictionary);
		}
		else
		{
			lblDescription.text = LocalizationManager.Instance.GetLocalizedValue("Tutorials_Table", p_page.descriptionKey);
		}
	}

	public override void Reset()
	{
		base.Reset();
		imgMain.sprite = null;
	}
}
