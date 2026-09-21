using BayatGames.SaveGameFree.Types;
using UnityEngine;

namespace BayatGames.SaveGameFree.Examples;

public class ExampleSaveRotation : MonoBehaviour
{
	public Transform target;

	public bool loadOnStart = true;

	public string identifier = "exampleSaveRotation.dat";

	private void Start()
	{
		if (loadOnStart)
		{
			Load();
		}
	}

	private void Update()
	{
		Vector3 eulerAngles = target.rotation.eulerAngles;
		eulerAngles.z += Input.GetAxis("Horizontal");
		target.rotation = Quaternion.Euler(eulerAngles);
	}

	private void OnApplicationQuit()
	{
		Save();
	}

	public void Save()
	{
		SaveGame.Save(identifier, (QuaternionSave)target.rotation, SerializerDropdown.Singleton.ActiveSerializer);
	}

	public void Load()
	{
		target.rotation = SaveGame.Load(identifier, (QuaternionSave)Quaternion.identity, SerializerDropdown.Singleton.ActiveSerializer);
	}
}
