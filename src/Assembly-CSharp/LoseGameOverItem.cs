using TMPro;
using UnityEngine;

public class LoseGameOverItem : MonoBehaviour
{
	public GameObject go;

	public TextMeshProUGUI experienceText;

	public void Open()
	{
		UpdateData();
		go.SetActive(value: true);
	}

	public void Close()
	{
		go.SetActive(value: false);
	}

	private void UpdateData()
	{
	}

	public void BackToMainMenu()
	{
		LevelLoaderManager.Instance.LoadLevel("MainMenu");
	}
}
