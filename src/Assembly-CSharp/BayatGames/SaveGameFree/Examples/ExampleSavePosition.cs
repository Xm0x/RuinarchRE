using BayatGames.SaveGameFree.Types;
using UnityEngine;

namespace BayatGames.SaveGameFree.Examples;

public class ExampleSavePosition : MonoBehaviour
{
	public Transform target;

	public bool loadOnStart = true;

	public string identifier = "exampleSavePosition.dat";

	private void Start()
	{
		if (loadOnStart)
		{
			Load();
		}
	}

	private void Update()
	{
		Vector3 position = target.position;
		position.x += Input.GetAxis("Horizontal");
		position.y += Input.GetAxis("Vertical");
		target.position = position;
	}

	private void OnApplicationQuit()
	{
		Save();
	}

	public void Save()
	{
		SaveGame.Save(identifier, (Vector3Save)target.position, SerializerDropdown.Singleton.ActiveSerializer);
	}

	public void Load()
	{
		target.position = SaveGame.Load(identifier, (Vector3Save)Vector3.zero, SerializerDropdown.Singleton.ActiveSerializer);
	}
}
