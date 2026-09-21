using System;
using System.Collections.Generic;
using System.Linq;
using Inner_Maps.Location_Structures;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
using UtilityScripts;

public class ObjectPicker : PopupMenuBase
{
	[Header("Object Picker")]
	[SerializeField]
	private ScrollRect objectPickerScrollView;

	[SerializeField]
	private GameObject objectPickerCharacterItemPrefab;

	[SerializeField]
	private GameObject objectPickerRegionItemPrefab;

	[SerializeField]
	private GameObject objectPickerStringItemPrefab;

	[SerializeField]
	private GameObject objectPickerRaceClassItemPrefab;

	[SerializeField]
	private GameObject objectPickerEnumItemPrefab;

	[SerializeField]
	private GameObject objectPickerSummonSlotItemPrefab;

	[SerializeField]
	private GameObject objectPickerSpellItemPrefab;

	[FormerlySerializedAs("objectPickerArtifactSlotItemPrefab")]
	[SerializeField]
	private GameObject objectPickerArtifactItemPrefab;

	[SerializeField]
	private GameObject objectPickerStructureItemPrefab;

	[SerializeField]
	private GameObject objectPickerFactionItemPrefab;

	[SerializeField]
	private TextMeshProUGUI titleLbl;

	[SerializeField]
	private GameObject cover;

	[SerializeField]
	private Button closeBtn;

	[SerializeField]
	private Button confirmBtn;

	[SerializeField]
	private ToggleGroup toggleGroup;

	[Header("Misc")]
	[SerializeField]
	private UIHoverPosition minionCardPos;

	private bool _isGamePausedBeforeOpeningPicker;

	private object _pickedObj;

	private Action<object> onConfirmAction;

	private bool _shouldShowConfirmationWindowOnPick;

	public object pickedObj
	{
		get
		{
			return _pickedObj;
		}
		set
		{
			_pickedObj = value;
			UpdateConfirmBtnState();
		}
	}

	public void ShowClickable<T>(T[] items, Action<object> onConfirmAction, IComparer<T> comparer = null, Func<T, bool> validityChecker = null, string title = "", Action<T> onHoverItemAction = null, Action<T> onHoverExitItemAction = null, string identifier = "", bool showCover = false, int layer = 9, Func<string, Sprite> portraitGetter = null, bool asButton = false, bool shouldShowConfirmationWindowOnPick = false)
	{
		OrganizeList(items, out var validItems, out var invalidItems, comparer, validityChecker);
		UpdateClickable(validItems, invalidItems, onConfirmAction, comparer, validityChecker, title, onHoverItemAction, onHoverExitItemAction, identifier, showCover, layer, portraitGetter, asButton, shouldShowConfirmationWindowOnPick);
	}

	public void ShowClickable<T>(List<T> items, Action<object> onConfirmAction, IComparer<T> comparer = null, Func<T, bool> validityChecker = null, string title = "", Action<T> onHoverItemAction = null, Action<T> onHoverExitItemAction = null, string identifier = "", bool showCover = false, int layer = 9, Func<string, Sprite> portraitGetter = null, bool asButton = false, bool shouldShowConfirmationWindowOnPick = false)
	{
		OrganizeList(items, out var validItems, out var invalidItems, comparer, validityChecker);
		UpdateClickable(validItems, invalidItems, onConfirmAction, comparer, validityChecker, title, onHoverItemAction, onHoverExitItemAction, identifier, showCover, layer, portraitGetter, asButton, shouldShowConfirmationWindowOnPick);
	}

	private void UpdateClickable<T>(List<T> validItems, List<T> invalidItems, Action<object> onConfirmAction, IComparer<T> comparer = null, Func<T, bool> validityChecker = null, string title = "", Action<T> onHoverItemAction = null, Action<T> onHoverExitItemAction = null, string identifier = "", bool showCover = false, int layer = 9, Func<string, Sprite> portraitGetter = null, bool asButton = false, bool shouldShowConfirmationWindowOnPick = false)
	{
		Utilities.DestroyChildren(objectPickerScrollView.content);
		_shouldShowConfirmationWindowOnPick = shouldShowConfirmationWindowOnPick;
		pickedObj = null;
		this.onConfirmAction = onConfirmAction;
		Type typeFromHandle = typeof(T);
		if (typeFromHandle == typeof(Character))
		{
			ShowCharacterItems(validItems.Cast<Character>().ToList(), invalidItems.Cast<Character>().ToList(), onHoverItemAction, onHoverExitItemAction, identifier, asButton);
		}
		else if (typeFromHandle == typeof(Region))
		{
			ShowRegionItems(validItems.Cast<Region>().ToList(), invalidItems.Cast<Region>().ToList(), onHoverItemAction, onHoverExitItemAction, asButton);
		}
		else if (typeFromHandle == typeof(string))
		{
			ShowStringItems(validItems.Cast<string>().ToList(), invalidItems.Cast<string>().ToList(), onHoverItemAction, onHoverExitItemAction, identifier, asButton);
		}
		else if (typeFromHandle == typeof(SkillData))
		{
			ShowSpellItems(validItems.Cast<SkillData>().ToList(), invalidItems.Cast<SkillData>().ToList(), onHoverItemAction, onHoverExitItemAction, identifier, portraitGetter, asButton);
		}
		else if (typeFromHandle.IsEnum)
		{
			ShowEnumItems(validItems.Cast<Enum>().ToList(), invalidItems.Cast<Enum>().ToList(), onHoverItemAction, onHoverExitItemAction, identifier, portraitGetter, asButton);
		}
		else if (typeFromHandle == typeof(RaceClass))
		{
			ShowRaceClassItems(validItems.Cast<RaceClass>().ToList(), invalidItems.Cast<RaceClass>().ToList(), onHoverItemAction, onHoverExitItemAction, identifier, asButton);
		}
		else if (typeFromHandle == typeof(LocationStructure))
		{
			ShowStructureItems(validItems.Cast<LocationStructure>().ToList(), invalidItems.Cast<LocationStructure>().ToList(), onHoverItemAction, onHoverExitItemAction, asButton);
		}
		else if (typeFromHandle == typeof(Faction))
		{
			ShowFactionItems(validItems.Cast<Faction>().ToList(), invalidItems.Cast<Faction>().ToList(), onHoverItemAction, onHoverExitItemAction, asButton);
		}
		titleLbl.text = title;
		base.Open();
		UIManager.Instance.Pause();
		UIManager.Instance.SetSpeedTogglesState(state: false);
		cover.SetActive(showCover);
		base.gameObject.transform.SetSiblingIndex(layer);
		if (_shouldShowConfirmationWindowOnPick)
		{
			confirmBtn.gameObject.SetActive(value: false);
		}
		else
		{
			confirmBtn.gameObject.SetActive(value: true);
		}
	}

	public override void Close()
	{
		if (base.gameObject.activeSelf)
		{
			base.Close();
			if (!PlayerUI.Instance.TryShowPendingUI() && !UIManager.Instance.IsObjectPickerOpen())
			{
				UIManager.Instance.ResumeLastProgressionSpeed();
			}
		}
	}

	private void OrganizeList<T>(List<T> items, out List<T> validItems, out List<T> invalidItems, IComparer<T> comparer = null, Func<T, bool> validityChecker = null)
	{
		validItems = new List<T>();
		invalidItems = new List<T>();
		if (validityChecker != null)
		{
			for (int i = 0; i < items.Count; i++)
			{
				T val = items[i];
				if (validityChecker(val))
				{
					validItems.Add(val);
				}
				else
				{
					invalidItems.Add(val);
				}
			}
		}
		else
		{
			validItems.AddRange(items);
		}
		if (comparer != null)
		{
			validItems.Sort(comparer);
			invalidItems.Sort(comparer);
		}
	}

	private void OrganizeList<T>(T[] items, out List<T> validItems, out List<T> invalidItems, IComparer<T> comparer = null, Func<T, bool> validityChecker = null)
	{
		validItems = new List<T>();
		invalidItems = new List<T>();
		if (validityChecker != null)
		{
			foreach (T val in items)
			{
				if (validityChecker(val))
				{
					validItems.Add(val);
				}
				else
				{
					invalidItems.Add(val);
				}
			}
		}
		else
		{
			validItems.AddRange(items);
		}
		if (comparer != null)
		{
			validItems.Sort(comparer);
			invalidItems.Sort(comparer);
		}
	}

	private void ShowCharacterItems<T>(List<Character> validItems, List<Character> invalidItems, Action<T> onHoverItemAction, Action<T> onHoverExitItemAction, string identifier, bool asButton)
	{
		Action<Character> action = null;
		if (onHoverItemAction != null)
		{
			action = Convert(onHoverItemAction);
		}
		Action<Character> action2 = null;
		if (onHoverExitItemAction != null)
		{
			action2 = Convert(onHoverExitItemAction);
		}
		for (int i = 0; i < validItems.Count; i++)
		{
			Character character = validItems[i];
			CharacterNameplateItem component = UIManager.Instance.InstantiateUIObject(objectPickerCharacterItemPrefab.name, objectPickerScrollView.content).GetComponent<CharacterNameplateItem>();
			component.SetObject(character);
			component.ClearAllOnClickActions();
			component.ClearAllHoverEnterActions();
			if (action != null)
			{
				component.AddHoverEnterAction(action.Invoke);
			}
			component.ClearAllHoverExitActions();
			if (action2 != null)
			{
				component.AddHoverExitAction(action2.Invoke);
			}
			if (asButton)
			{
				component.AddOnClickAction(OnPickObject);
				component.SetAsButton();
			}
			else
			{
				component.AddOnToggleAction(OnPickObject);
				component.SetAsToggle();
				component.SetToggleGroup(toggleGroup);
			}
			component.SetPortraitInteractableState(state: false);
		}
		for (int j = 0; j < invalidItems.Count; j++)
		{
			Character character2 = invalidItems[j];
			CharacterNameplateItem component2 = UIManager.Instance.InstantiateUIObject(objectPickerCharacterItemPrefab.name, objectPickerScrollView.content).GetComponent<CharacterNameplateItem>();
			component2.SetObject(character2);
			component2.ClearAllOnClickActions();
			component2.ClearAllHoverEnterActions();
			if (action != null)
			{
				component2.AddHoverEnterAction(action.Invoke);
			}
			component2.ClearAllHoverExitActions();
			if (action2 != null)
			{
				component2.AddHoverExitAction(action2.Invoke);
			}
			if (asButton)
			{
				component2.SetAsButton();
			}
			else
			{
				component2.SetAsToggle();
			}
			component2.SetInteractableState(state: false);
			component2.SetPortraitInteractableState(state: true);
		}
	}

	private void ShowRegionItems<T>(List<Region> validItems, List<Region> invalidItems, Action<T> onHoverItemAction, Action<T> onHoverExitItemAction, bool asButton)
	{
		Action<Region> action = null;
		if (onHoverItemAction != null)
		{
			action = ConvertToRegion(onHoverItemAction);
		}
		Action<Region> action2 = null;
		if (onHoverExitItemAction != null)
		{
			action2 = ConvertToRegion(onHoverExitItemAction);
		}
		for (int i = 0; i < validItems.Count; i++)
		{
			Region region = validItems[i];
			RegionNameplateItem component = UIManager.Instance.InstantiateUIObject(objectPickerRegionItemPrefab.name, objectPickerScrollView.content).GetComponent<RegionNameplateItem>();
			component.SetObject(region);
			component.ClearAllOnClickActions();
			component.ClearAllHoverEnterActions();
			if (action != null)
			{
				component.AddHoverEnterAction(action.Invoke);
			}
			component.ClearAllHoverExitActions();
			if (action2 != null)
			{
				component.AddHoverExitAction(action2.Invoke);
			}
			if (asButton)
			{
				component.AddOnClickAction(OnPickObject);
				component.SetAsButton();
			}
			else
			{
				component.AddOnToggleAction(OnPickObject);
				component.SetAsToggle();
				component.SetToggleGroup(toggleGroup);
			}
		}
		for (int j = 0; j < invalidItems.Count; j++)
		{
			Region region2 = invalidItems[j];
			RegionNameplateItem component2 = UIManager.Instance.InstantiateUIObject(objectPickerRegionItemPrefab.name, objectPickerScrollView.content).GetComponent<RegionNameplateItem>();
			component2.SetObject(region2);
			component2.ClearAllOnClickActions();
			component2.ClearAllHoverEnterActions();
			if (action != null)
			{
				component2.AddHoverEnterAction(action.Invoke);
			}
			component2.ClearAllHoverExitActions();
			if (action2 != null)
			{
				component2.AddHoverExitAction(action2.Invoke);
			}
			if (asButton)
			{
				component2.SetAsButton();
			}
			else
			{
				component2.SetAsToggle();
			}
			component2.SetInteractableState(state: false);
		}
	}

	private void ShowStringItems<T>(List<string> validItems, List<string> invalidItems, Action<T> onHoverItemAction, Action<T> onHoverExitItemAction, string identifier, bool asButton)
	{
		Action<string> action = null;
		if (onHoverItemAction != null)
		{
			action = ConvertToString(onHoverItemAction);
		}
		Action<string> action2 = null;
		if (onHoverExitItemAction != null)
		{
			action2 = ConvertToString(onHoverExitItemAction);
		}
		for (int i = 0; i < validItems.Count; i++)
		{
			string text = validItems[i];
			StringNameplateItem component = UIManager.Instance.InstantiateUIObject(objectPickerStringItemPrefab.name, objectPickerScrollView.content).GetComponent<StringNameplateItem>();
			component.SetObject(text);
			component.SetIdentifier(identifier);
			component.ClearAllOnClickActions();
			component.ClearAllHoverEnterActions();
			if (action != null)
			{
				component.AddHoverEnterAction(action.Invoke);
			}
			component.ClearAllHoverExitActions();
			if (action2 != null)
			{
				component.AddHoverExitAction(action2.Invoke);
			}
			if (asButton)
			{
				component.AddOnClickAction(OnPickObject);
				component.SetAsButton();
			}
			else
			{
				component.AddOnToggleAction(OnPickObject);
				component.SetAsToggle();
				component.SetToggleGroup(toggleGroup);
			}
		}
		for (int j = 0; j < invalidItems.Count; j++)
		{
			string text2 = invalidItems[j];
			StringNameplateItem component2 = UIManager.Instance.InstantiateUIObject(objectPickerStringItemPrefab.name, objectPickerScrollView.content).GetComponent<StringNameplateItem>();
			component2.SetObject(text2);
			component2.SetIdentifier(identifier);
			component2.ClearAllOnClickActions();
			component2.ClearAllHoverEnterActions();
			if (action != null)
			{
				component2.AddHoverEnterAction(action.Invoke);
			}
			component2.ClearAllHoverExitActions();
			if (action2 != null)
			{
				component2.AddHoverExitAction(action2.Invoke);
			}
			if (asButton)
			{
				component2.SetAsButton();
			}
			else
			{
				component2.SetAsToggle();
			}
			component2.SetInteractableState(state: false);
		}
	}

	private void ShowEnumItems<T>(List<Enum> validItems, List<Enum> invalidItems, Action<T> onHoverItemAction, Action<T> onHoverExitItemAction, string identifier, Func<string, Sprite> portraitGetter, bool asButton)
	{
		Action<Enum> action = null;
		if (onHoverItemAction != null)
		{
			action = ConvertToEnum(onHoverItemAction);
		}
		Action<Enum> action2 = null;
		if (onHoverExitItemAction != null)
		{
			action2 = ConvertToEnum(onHoverExitItemAction);
		}
		for (int i = 0; i < invalidItems.Count; i++)
		{
			Enum obj = invalidItems[i];
			EnumNameplateItem component = UIManager.Instance.InstantiateUIObject(objectPickerEnumItemPrefab.name, objectPickerScrollView.content).GetComponent<EnumNameplateItem>();
			component.SetObject(obj);
			component.ClearAllOnClickActions();
			component.SetPortrait(portraitGetter?.Invoke(obj.ToString()));
			component.ClearAllHoverEnterActions();
			if (action != null)
			{
				component.AddHoverEnterAction(action.Invoke);
			}
			component.ClearAllHoverExitActions();
			if (action2 != null)
			{
				component.AddHoverExitAction(action2.Invoke);
			}
			if (asButton)
			{
				component.SetAsButton();
			}
			else
			{
				component.SetAsToggle();
			}
			component.SetInteractableState(state: false);
		}
		for (int j = 0; j < validItems.Count; j++)
		{
			Enum obj2 = validItems[j];
			EnumNameplateItem component2 = UIManager.Instance.InstantiateUIObject(objectPickerEnumItemPrefab.name, objectPickerScrollView.content).GetComponent<EnumNameplateItem>();
			component2.SetObject(obj2);
			component2.ClearAllOnClickActions();
			component2.SetPortrait(portraitGetter?.Invoke(obj2.ToString()));
			component2.ClearAllHoverEnterActions();
			if (action != null)
			{
				component2.AddHoverEnterAction(action.Invoke);
			}
			component2.ClearAllHoverExitActions();
			if (action2 != null)
			{
				component2.AddHoverExitAction(action2.Invoke);
			}
			if (asButton)
			{
				component2.AddOnClickAction(OnPickObject);
				component2.SetAsButton();
			}
			else
			{
				component2.AddOnToggleAction(OnPickObject);
				component2.SetAsToggle();
				component2.SetToggleGroup(toggleGroup);
			}
			component2.transform.SetAsFirstSibling();
		}
	}

	private void ShowRaceClassItems<T>(List<RaceClass> validItems, List<RaceClass> invalidItems, Action<T> onHoverItemAction, Action<T> onHoverExitItemAction, string identifier, bool asButton)
	{
		Action<RaceClass> action = null;
		if (onHoverItemAction != null)
		{
			action = ConvertToRaceClass(onHoverItemAction);
		}
		Action<RaceClass> action2 = null;
		if (onHoverExitItemAction != null)
		{
			action2 = ConvertToRaceClass(onHoverExitItemAction);
		}
		for (int i = 0; i < validItems.Count; i++)
		{
			RaceClass raceClass = validItems[i];
			RaceClassNameplate component = UIManager.Instance.InstantiateUIObject(objectPickerRaceClassItemPrefab.name, objectPickerScrollView.content).GetComponent<RaceClassNameplate>();
			component.SetObject(raceClass);
			component.ClearAllOnClickActions();
			component.ClearAllHoverEnterActions();
			if (action != null)
			{
				component.AddHoverEnterAction(action.Invoke);
			}
			component.ClearAllHoverExitActions();
			if (action2 != null)
			{
				component.AddHoverExitAction(action2.Invoke);
			}
			if (asButton)
			{
				component.AddOnClickAction(OnPickObject);
				component.SetAsButton();
			}
			else
			{
				component.AddOnToggleAction(OnPickObject);
				component.SetAsToggle();
				component.SetToggleGroup(toggleGroup);
			}
		}
		for (int j = 0; j < invalidItems.Count; j++)
		{
			RaceClass raceClass2 = invalidItems[j];
			RaceClassNameplate component2 = UIManager.Instance.InstantiateUIObject(objectPickerStringItemPrefab.name, objectPickerScrollView.content).GetComponent<RaceClassNameplate>();
			component2.SetObject(raceClass2);
			component2.ClearAllOnClickActions();
			component2.ClearAllHoverEnterActions();
			if (action != null)
			{
				component2.AddHoverEnterAction(action.Invoke);
			}
			component2.ClearAllHoverExitActions();
			if (action2 != null)
			{
				component2.AddHoverExitAction(action2.Invoke);
			}
			if (asButton)
			{
				component2.SetAsButton();
			}
			else
			{
				component2.SetAsToggle();
			}
			component2.SetInteractableState(state: false);
		}
	}

	private void ShowSpellItems<T>(List<SkillData> validItems, List<SkillData> invalidItems, Action<T> onHoverItemAction, Action<T> onHoverExitItemAction, string identifier, Func<string, Sprite> portraitGetter, bool asButton)
	{
		Action<SkillData> action = null;
		if (onHoverItemAction != null)
		{
			action = ConvertToSpellData(onHoverItemAction);
		}
		Action<SkillData> action2 = null;
		if (onHoverExitItemAction != null)
		{
			action2 = ConvertToSpellData(onHoverExitItemAction);
		}
		for (int i = 0; i < invalidItems.Count; i++)
		{
			SkillData skillData = invalidItems[i];
			SpellItem component = UIManager.Instance.InstantiateUIObject(objectPickerSpellItemPrefab.name, objectPickerScrollView.content).GetComponent<SpellItem>();
			component.SetObject(skillData);
			component.ClearAllOnClickActions();
			component.ClearAllHoverEnterActions();
			if (action != null)
			{
				component.AddHoverEnterAction(action.Invoke);
			}
			component.ClearAllHoverExitActions();
			if (action2 != null)
			{
				component.AddHoverExitAction(action2.Invoke);
			}
			if (asButton)
			{
				component.SetAsButton();
			}
			else
			{
				component.SetAsToggle();
			}
			component.SetInteractableState(state: false);
			component.UpdateCooldownFromLastState();
			component.transform.SetAsLastSibling();
		}
		for (int j = 0; j < validItems.Count; j++)
		{
			SkillData skillData2 = validItems[j];
			SpellItem component2 = UIManager.Instance.InstantiateUIObject(objectPickerSpellItemPrefab.name, objectPickerScrollView.content).GetComponent<SpellItem>();
			component2.SetObject(skillData2);
			component2.ClearAllOnClickActions();
			component2.ClearAllHoverEnterActions();
			if (action != null)
			{
				component2.AddHoverEnterAction(action.Invoke);
			}
			component2.ClearAllHoverExitActions();
			if (action2 != null)
			{
				component2.AddHoverExitAction(action2.Invoke);
			}
			if (asButton)
			{
				component2.AddOnClickAction(OnPickObject);
				component2.SetAsButton();
			}
			else
			{
				component2.AddOnToggleAction(OnPickObject);
				component2.SetAsToggle();
				component2.SetToggleGroup(toggleGroup);
			}
			component2.UpdateCooldownFromLastState();
			component2.transform.SetAsFirstSibling();
		}
	}

	private void ShowStructureItems<T>(List<LocationStructure> validItems, List<LocationStructure> invalidItems, Action<T> onHoverItemAction, Action<T> onHoverExitItemAction, bool asButton)
	{
		Action<LocationStructure> action = null;
		if (onHoverItemAction != null)
		{
			action = ConvertToStructure(onHoverItemAction);
		}
		Action<LocationStructure> action2 = null;
		if (onHoverExitItemAction != null)
		{
			action2 = ConvertToStructure(onHoverExitItemAction);
		}
		for (int i = 0; i < validItems.Count; i++)
		{
			LocationStructure locationStructure = validItems[i];
			StructureNameplateItem component = UIManager.Instance.InstantiateUIObject(objectPickerStructureItemPrefab.name, objectPickerScrollView.content).GetComponent<StructureNameplateItem>();
			component.SetObject(locationStructure);
			component.ClearAllOnClickActions();
			component.ClearAllHoverEnterActions();
			if (action != null)
			{
				component.AddHoverEnterAction(action.Invoke);
			}
			component.ClearAllHoverExitActions();
			if (action2 != null)
			{
				component.AddHoverExitAction(action2.Invoke);
			}
			if (asButton)
			{
				component.AddOnClickAction(OnPickObject);
				component.SetAsButton();
			}
			else
			{
				component.AddOnToggleAction(OnPickObject);
				component.SetAsToggle();
				component.SetToggleGroup(toggleGroup);
			}
		}
		for (int j = 0; j < invalidItems.Count; j++)
		{
			LocationStructure locationStructure2 = invalidItems[j];
			StructureNameplateItem component2 = UIManager.Instance.InstantiateUIObject(objectPickerStructureItemPrefab.name, objectPickerScrollView.content).GetComponent<StructureNameplateItem>();
			component2.SetObject(locationStructure2);
			component2.ClearAllOnClickActions();
			component2.ClearAllHoverEnterActions();
			if (action != null)
			{
				component2.AddHoverEnterAction(action.Invoke);
			}
			component2.ClearAllHoverExitActions();
			if (action2 != null)
			{
				component2.AddHoverExitAction(action2.Invoke);
			}
			if (asButton)
			{
				component2.SetAsButton();
			}
			else
			{
				component2.SetAsToggle();
			}
			component2.SetInteractableState(state: false);
		}
	}

	private void ShowFactionItems<T>(List<Faction> validItems, List<Faction> invalidItems, Action<T> onHoverItemAction, Action<T> onHoverExitItemAction, bool asButton)
	{
		Action<Faction> action = null;
		if (onHoverItemAction != null)
		{
			action = ConvertToFaction(onHoverItemAction);
		}
		Action<Faction> action2 = null;
		if (onHoverExitItemAction != null)
		{
			action2 = ConvertToFaction(onHoverExitItemAction);
		}
		for (int i = 0; i < validItems.Count; i++)
		{
			Faction faction = validItems[i];
			FactionNameplateItem component = UIManager.Instance.InstantiateUIObject(objectPickerFactionItemPrefab.name, objectPickerScrollView.content).GetComponent<FactionNameplateItem>();
			component.SetObject(faction);
			component.ClearAllOnClickActions();
			component.ClearAllHoverEnterActions();
			if (action != null)
			{
				component.AddHoverEnterAction(action.Invoke);
			}
			component.ClearAllHoverExitActions();
			if (action2 != null)
			{
				component.AddHoverExitAction(action2.Invoke);
			}
			if (asButton)
			{
				component.AddOnClickAction(OnPickObject);
				component.SetAsButton();
			}
			else
			{
				component.AddOnToggleAction(OnPickObject);
				component.SetAsToggle();
				component.SetToggleGroup(toggleGroup);
			}
		}
		for (int j = 0; j < invalidItems.Count; j++)
		{
			Faction faction2 = invalidItems[j];
			FactionNameplateItem component2 = UIManager.Instance.InstantiateUIObject(objectPickerFactionItemPrefab.name, objectPickerScrollView.content).GetComponent<FactionNameplateItem>();
			component2.SetObject(faction2);
			component2.ClearAllOnClickActions();
			component2.ClearAllHoverEnterActions();
			if (action != null)
			{
				component2.AddHoverEnterAction(action.Invoke);
			}
			component2.ClearAllHoverExitActions();
			if (action2 != null)
			{
				component2.AddHoverExitAction(action2.Invoke);
			}
			if (asButton)
			{
				component2.SetAsButton();
			}
			else
			{
				component2.SetAsToggle();
			}
			component2.SetInteractableState(state: false);
		}
	}

	private void UpdateConfirmBtnState()
	{
		confirmBtn.interactable = pickedObj != null;
	}

	private void OnPickObject(object obj, bool isOn)
	{
		if (isOn)
		{
			pickedObj = obj;
			if (_shouldShowConfirmationWindowOnPick)
			{
				OnClickConfirm();
			}
		}
		else if (pickedObj == obj)
		{
			pickedObj = null;
		}
	}

	private void OnPickObject(object obj)
	{
		pickedObj = obj;
		if (pickedObj != null && _shouldShowConfirmationWindowOnPick)
		{
			OnClickConfirm();
		}
	}

	private void OnPickObject(RaceClass obj)
	{
		pickedObj = obj;
		if (pickedObj != null && _shouldShowConfirmationWindowOnPick)
		{
			OnClickConfirm();
		}
	}

	private void OnPickObject(RaceClass obj, bool isOn)
	{
		if (isOn)
		{
			pickedObj = obj;
			if (_shouldShowConfirmationWindowOnPick)
			{
				OnClickConfirm();
			}
		}
		else if (obj.Equals(pickedObj))
		{
			pickedObj = null;
		}
	}

	public void OnClickConfirm()
	{
		onConfirmAction(pickedObj);
	}

	private Action<Character> Convert<T>(Action<T> myActionT)
	{
		if (myActionT == null)
		{
			return null;
		}
		return delegate(Character o)
		{
			myActionT((T)(object)o);
		};
	}

	private Action<Artifact> ConvertToArtifact<T>(Action<T> myActionT)
	{
		if (myActionT == null)
		{
			return null;
		}
		return delegate(Artifact o)
		{
			myActionT((T)(object)o);
		};
	}

	public Action<Minion> ConvertToMinion<T>(Action<T> myActionT)
	{
		if (myActionT == null)
		{
			return null;
		}
		return delegate(Minion o)
		{
			myActionT((T)(object)o);
		};
	}

	public Action<NPCSettlement> ConvertToArea<T>(Action<T> myActionT)
	{
		if (myActionT == null)
		{
			return null;
		}
		return delegate(NPCSettlement o)
		{
			myActionT((T)(object)o);
		};
	}

	private Action<Region> ConvertToRegion<T>(Action<T> myActionT)
	{
		if (myActionT == null)
		{
			return null;
		}
		return delegate(Region o)
		{
			myActionT((T)(object)o);
		};
	}

	private Action<string> ConvertToString<T>(Action<T> myActionT)
	{
		if (myActionT == null)
		{
			return null;
		}
		return delegate(string o)
		{
			myActionT((T)(object)o);
		};
	}

	private Action<Enum> ConvertToEnum<T>(Action<T> myActionT)
	{
		if (myActionT == null)
		{
			return null;
		}
		return delegate(Enum o)
		{
			myActionT((T)(object)o);
		};
	}

	private Action<RaceClass> ConvertToRaceClass<T>(Action<T> myActionT)
	{
		if (myActionT == null)
		{
			return null;
		}
		return delegate(RaceClass o)
		{
			myActionT((T)(object)o);
		};
	}

	private Action<SkillData> ConvertToSpellData<T>(Action<T> myActionT)
	{
		if (myActionT == null)
		{
			return null;
		}
		return delegate(SkillData o)
		{
			myActionT((T)(object)o);
		};
	}

	private Action<LocationStructure> ConvertToStructure<T>(Action<T> myActionT)
	{
		if (myActionT == null)
		{
			return null;
		}
		return delegate(LocationStructure o)
		{
			myActionT((T)(object)o);
		};
	}

	private Action<Faction> ConvertToFaction<T>(Action<T> myActionT)
	{
		if (myActionT == null)
		{
			return null;
		}
		return delegate(Faction o)
		{
			myActionT((T)(object)o);
		};
	}
}
