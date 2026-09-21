using System.Collections;
using BayatGames.SaveGameFree.Types;
using UnityEngine;

namespace BayatGames.SaveGameFree.Examples;

public class ExampleSaveWeb : MonoBehaviour
{
	public Transform target;

	public bool loadOnStart = true;

	public string identifier = "exampleSaveWeb";

	public string username = "savegamefree";

	public string password = "$@ve#game%free";

	public string url = "http://www.example.com/savegamefree.php";

	public bool encode = true;

	public string encodePassword = "h@e#ll$o%^";

	private void Start()
	{
		Load();
	}

	private void Update()
	{
		Vector3 position = target.position;
		position.x += Input.GetAxis("Horizontal");
		position.y += Input.GetAxis("Vertical");
		target.position = position;
	}

	public void Load()
	{
		StartCoroutine(LoadEnumerator());
	}

	public void Save()
	{
		StartCoroutine(SaveEnumerator());
	}

	private IEnumerator LoadEnumerator()
	{
		Debug.Log("Downloading...");
		SaveGameWeb web = new SaveGameWeb(username, password, url, encode, encodePassword, SerializerDropdown.Singleton.ActiveSerializer);
		yield return StartCoroutine(web.Download(identifier));
		target.position = web.Load(identifier, (Vector3Save)Vector3.zero);
		Debug.Log("Download Done.");
	}

	private IEnumerator SaveEnumerator()
	{
		Debug.Log("Uploading...");
		SaveGameWeb saveGameWeb = new SaveGameWeb(username, password, url, encode, encodePassword, SerializerDropdown.Singleton.ActiveSerializer);
		yield return StartCoroutine(saveGameWeb.Save(identifier, (Vector3Save)target.position));
		Debug.Log("Upload Done.");
	}
}
