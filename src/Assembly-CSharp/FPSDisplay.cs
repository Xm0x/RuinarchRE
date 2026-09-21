using Steamworks;
using UnityEngine;

public class FPSDisplay : MonoBehaviour
{
	private float deltaTime;

	private void Update()
	{
		deltaTime += (Time.unscaledDeltaTime - deltaTime) * 0.1f;
	}

	private void OnGUI()
	{
		int width = Screen.width;
		int height = Screen.height;
		GUIStyle gUIStyle = new GUIStyle();
		Rect position = new Rect(0f, 0f, width, height * 2 / 100);
		gUIStyle.alignment = TextAnchor.UpperLeft;
		gUIStyle.fontSize = height * 2 / 100;
		gUIStyle.normal.textColor = Color.white;
		float num = deltaTime * 1000f;
		string text = string.Format(arg1: 1f / deltaTime, format: "{0:0.0} ms ({1:0.} fps)", arg0: num);
		text = text + "\nAgents: " + PathfindingManager.Instance.allAgents.Count;
		text = text + "\nCharacters: " + CharacterManager.Instance.allCharacters.Count;
		if (SteamManager.Initialized)
		{
			string personaName = SteamFriends.GetPersonaName();
			text = text + "\nSteam Name: " + personaName;
		}
		GUI.Label(position, text, gUIStyle);
	}
}
