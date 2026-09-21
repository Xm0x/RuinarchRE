using UnityEngine;

public class EditorPanelUI : MonoBehaviour
{
	public static EditorPanelUI Instance;

	public GameObject page1GO;

	public GameObject page2GO;

	private void Awake()
	{
		Instance = this;
	}

	private void Start()
	{
		LoadAllDataOfAllEditors();
	}

	public void LoadAllDataOfAllEditors()
	{
		OnClickPage2();
		OnClickPage1();
		CharacterPanelUI.Instance.LoadAllData();
		MonsterPanelUI.Instance.LoadAllData();
		ClassPanelUI.Instance.LoadAllData();
		RacePanelUI.Instance.LoadAllData();
		TraitPanelUI.Instance.LoadAllData();
	}

	public void OnClickPage1()
	{
		page1GO.SetActive(value: true);
		page2GO.SetActive(value: false);
	}

	public void OnClickPage2()
	{
		page1GO.SetActive(value: false);
		page2GO.SetActive(value: true);
	}
}
