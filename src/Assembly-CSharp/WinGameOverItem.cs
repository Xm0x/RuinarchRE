using TMPro;
using UnityEngine;

public class WinGameOverItem : MonoBehaviour
{
	public GameObject go;

	public TextMeshProUGUI killsText;

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
