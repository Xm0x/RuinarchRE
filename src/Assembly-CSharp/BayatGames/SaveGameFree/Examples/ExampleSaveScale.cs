using BayatGames.SaveGameFree.Types;
using UnityEngine;

namespace BayatGames.SaveGameFree.Examples;

public class ExampleSaveScale : MonoBehaviour
{
	public Transform target;

	public bool loadOnStart = true;

	public string identifier = "exampleSaveScale.dat";

	private void Start()
	{
		if (loadOnStart)
		{
			Load();
		}
	}

	private void Update()
	{
		Vector3 localScale = target.localScale;
		localScale.x += Input.GetAxis("Horizontal");
		localScale.y += Input.GetAxis("Vertical");
		target.localScale = localScale;
	}

	private void OnApplicationQuit()
	{
		Save();
	}

	public void Save()
	{
		SaveGame.Save(identifier, (Vector3Save)target.localScale, SerializerDropdown.Singleton.ActiveSerializer);
	}

	public void Load()
	{
		target.localScale = SaveGame.Load(identifier, new Vector3Save(1f, 1f, 1f), SerializerDropdown.Singleton.ActiveSerializer);
	}
}
