using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UtilityScripts;

public class ItemPanelUI : MonoBehaviour
{
	public static ItemPanelUI Instance;

	public Dropdown itemTypeOptions;

	public Dropdown weaponTypeOptions;

	public Dropdown armorTypeOptions;

	public Dropdown weaponPrefixOptions;

	public Dropdown weaponSuffixOptions;

	public Dropdown armorPrefixOptions;

	public Dropdown armorSuffixOptions;

	public Dropdown elementOptions;

	public Dropdown attributeOptions;

	public Dropdown iconOptions;

	public InputField nameInput;

	public InputField descriptionInput;

	public InputField interactionInput;

	public InputField goldCostInput;

	public InputField powerInput;

	public InputField defInput;

	public Toggle stackableToggle;

	public Image iconImg;

	public GameObject weaponFieldsGO;

	public GameObject armorFieldsGO;

	public GameObject attributeBtnPrefab;

	public Transform attributeContentTransform;

	private List<string> _attributes;

	private List<string> _allItems;

	private List<string> _allWeapons;

	private List<string> _allArmors;

	private Dictionary<string, Sprite> _iconSprites;

	private AttributeBtn _currentSelectedButton;

	public List<string> allItems => _allItems;

	public List<string> allWeapons => _allWeapons;

	public List<string> allArmors => _allArmors;

	private void Awake()
	{
		Instance = this;
	}

	private void UpdateAllItems()
	{
		_allItems.Clear();
		_allWeapons.Clear();
		_allArmors.Clear();
		_allArmors.Add("None");
		string[] directories = Directory.GetDirectories(Utilities.coreStreamingDataPath + "/Items/");
		for (int i = 0; i < directories.Length; i++)
		{
			string text = new DirectoryInfo(directories[i]).Name;
			string[] files = Directory.GetFiles(directories[i], "*.json");
			for (int j = 0; j < files.Length; j++)
			{
				string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(files[j]);
				if (text == "WEAPON")
				{
					_allWeapons.Add(fileNameWithoutExtension);
				}
				else if (text == "ARMOR")
				{
					_allArmors.Add(fileNameWithoutExtension);
				}
				_allItems.Add(fileNameWithoutExtension);
			}
		}
		MonsterPanelUI.Instance.UpdateItemDropOptions();
		CharacterPanelUI.Instance.UpdateItemOptions();
	}

	public void LoadAllData()
	{
		_attributes = new List<string>();
		_allItems = new List<string>();
		_allWeapons = new List<string>();
		_allArmors = new List<string>();
		itemTypeOptions.ClearOptions();
		weaponTypeOptions.ClearOptions();
		armorTypeOptions.ClearOptions();
		weaponPrefixOptions.ClearOptions();
		weaponSuffixOptions.ClearOptions();
		armorPrefixOptions.ClearOptions();
		armorSuffixOptions.ClearOptions();
		elementOptions.ClearOptions();
		iconOptions.ClearOptions();
		Sprite[] array = Resources.LoadAll<Sprite>("Textures/ItemIcons");
		_iconSprites = new Dictionary<string, Sprite>();
		_iconSprites.Add("None", null);
		for (int i = 0; i < array.Length; i++)
		{
			_iconSprites.Add(array[i].name, array[i]);
		}
		iconOptions.AddOptions(_iconSprites.Keys.ToList());
		UpdateAllItems();
	}

	private void ClearData()
	{
		itemTypeOptions.value = 0;
		weaponTypeOptions.value = 0;
		armorTypeOptions.value = 0;
		weaponPrefixOptions.value = 0;
		weaponSuffixOptions.value = 0;
		armorPrefixOptions.value = 0;
		armorSuffixOptions.value = 0;
		elementOptions.value = 0;
		attributeOptions.value = 0;
		iconOptions.value = 0;
		nameInput.text = string.Empty;
		descriptionInput.text = string.Empty;
		interactionInput.text = string.Empty;
		goldCostInput.text = "0";
		powerInput.text = "0";
		defInput.text = "0";
		stackableToggle.isOn = false;
		iconImg.sprite = null;
		armorFieldsGO.SetActive(value: false);
		_attributes.Clear();
		foreach (Transform item in attributeContentTransform)
		{
			Object.Destroy(item.gameObject);
		}
	}

	private void SaveItem()
	{
	}

	private void SaveItemJson(string path)
	{
	}

	private void SaveWeapon(string path)
	{
	}

	private void SaveArmor(string path)
	{
	}

	private void Save(string path)
	{
	}

	private int GetItemTypeIndex(string name)
	{
		for (int i = 0; i < itemTypeOptions.options.Count; i++)
		{
			if (itemTypeOptions.options[i].text == name)
			{
				return i;
			}
		}
		return 0;
	}

	private int GetWeaponTypeIndex(string name)
	{
		for (int i = 0; i < weaponTypeOptions.options.Count; i++)
		{
			if (weaponTypeOptions.options[i].text == name)
			{
				return i;
			}
		}
		return 0;
	}

	private int GetWeaponPrefixIndex(string name)
	{
		for (int i = 0; i < weaponPrefixOptions.options.Count; i++)
		{
			if (weaponPrefixOptions.options[i].text == name)
			{
				return i;
			}
		}
		return 0;
	}

	private int GetWeaponSuffixIndex(string name)
	{
		for (int i = 0; i < weaponSuffixOptions.options.Count; i++)
		{
			if (weaponSuffixOptions.options[i].text == name)
			{
				return i;
			}
		}
		return 0;
	}

	private int GetElementIndex(string name)
	{
		for (int i = 0; i < elementOptions.options.Count; i++)
		{
			if (elementOptions.options[i].text == name)
			{
				return i;
			}
		}
		return 0;
	}

	private int GetArmorTypeIndex(string name)
	{
		for (int i = 0; i < armorTypeOptions.options.Count; i++)
		{
			if (armorTypeOptions.options[i].text == name)
			{
				return i;
			}
		}
		return 0;
	}

	private int GetArmorPrefixIndex(string name)
	{
		for (int i = 0; i < armorPrefixOptions.options.Count; i++)
		{
			if (armorPrefixOptions.options[i].text == name)
			{
				return i;
			}
		}
		return 0;
	}

	private int GetArmorSuffixIndex(string name)
	{
		for (int i = 0; i < armorSuffixOptions.options.Count; i++)
		{
			if (armorSuffixOptions.options[i].text == name)
			{
				return i;
			}
		}
		return 0;
	}

	private int GetIconIndex(string name)
	{
		for (int i = 0; i < iconOptions.options.Count; i++)
		{
			if (iconOptions.options[i].text == name)
			{
				return i;
			}
		}
		return 0;
	}

	public void SetCurrentlySelectedButton(AttributeBtn btn)
	{
		_currentSelectedButton = btn;
	}

	public void UpdateAttributeOptions()
	{
		attributeOptions.ClearOptions();
		attributeOptions.AddOptions(TraitPanelUI.Instance.allTraits);
	}

	public void OnItemTypeChange(int index)
	{
		weaponFieldsGO.SetActive(value: false);
		armorFieldsGO.SetActive(value: false);
	}

	public void OnIconChange(int index)
	{
		string text = iconOptions.options[index].text;
		Sprite sprite = _iconSprites[text];
		iconImg.sprite = sprite;
	}

	public void OnClickAddNewItem()
	{
		ClearData();
	}

	public void OnClickEditItem()
	{
	}

	public void OnClickSaveItem()
	{
		SaveItem();
	}

	public void OnAddAttribute()
	{
		string text = attributeOptions.options[attributeOptions.value].text;
		if (!_attributes.Contains(text))
		{
			_attributes.Add(text);
			Object.Instantiate(attributeBtnPrefab, attributeContentTransform).GetComponent<AttributeBtn>().buttonText.text = text;
		}
	}

	public void OnRemoveAttribute()
	{
		if (_currentSelectedButton != null)
		{
			string text = _currentSelectedButton.buttonText.text;
			if (_attributes.Remove(text))
			{
				Object.Destroy(_currentSelectedButton.gameObject);
				_currentSelectedButton = null;
			}
		}
	}
}
