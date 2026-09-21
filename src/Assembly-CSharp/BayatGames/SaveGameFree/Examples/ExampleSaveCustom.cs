using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace BayatGames.SaveGameFree.Examples;

public class ExampleSaveCustom : MonoBehaviour
{
	[Serializable]
	public struct Level
	{
		public bool unlocked;

		public bool completed;

		public Level(bool unlocked, bool completed)
		{
			this.unlocked = unlocked;
			this.completed = completed;
		}
	}

	[Serializable]
	public class CustomData
	{
		public int score;

		public int highScore;

		public List<Level> levels;

		public CustomData()
		{
			score = 0;
			highScore = 0;
			levels = new List<Level>
			{
				new Level(unlocked: true, completed: false),
				new Level(unlocked: false, completed: false),
				new Level(unlocked: false, completed: true),
				new Level(unlocked: true, completed: false)
			};
		}
	}

	public CustomData customData;

	public bool loadOnStart = true;

	public InputField scoreInputField;

	public InputField highScoreInputField;

	public string identifier = "exampleSaveCustom";

	private void Start()
	{
		if (loadOnStart)
		{
			Load();
		}
	}

	public void SetScore(string score)
	{
		customData.score = int.Parse(score);
	}

	public void SetHighScore(string highScore)
	{
		customData.highScore = int.Parse(highScore);
	}

	public void Save()
	{
		SaveGame.Save(identifier, customData, SerializerDropdown.Singleton.ActiveSerializer);
	}

	public void Load()
	{
		customData = SaveGame.Load(identifier, new CustomData(), SerializerDropdown.Singleton.ActiveSerializer);
		scoreInputField.text = customData.score.ToString();
		highScoreInputField.text = customData.highScore.ToString();
	}
}
