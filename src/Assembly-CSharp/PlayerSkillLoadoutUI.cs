using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerSkillLoadoutUI : MonoBehaviour
{
	public PlayerSkillLoadout loadout;

	public GameObject skillSlotItemPrefab;

	public PlayerSkillLoadoutObjectPicker objectPicker;

	public PlayerSkillDetailsTooltip skillDetailsTooltip;

	public ScrollRect spellsScrollRect;

	public ScrollRect afflictionsScrollRect;

	public ScrollRect minionsScrollRect;

	public ScrollRect structuresScrollRect;

	public ScrollRect miscsScrollRect;

	public Toggle spellsTab;

	public Toggle afflictionsTab;

	public Toggle minionsTab;

	public Toggle structuresTab;

	public Toggle miscsTab;

	private SkillSlotItem pickedSlotItem;

	private List<PLAYER_SKILL_TYPE> loadoutChoices;

	public GroupedSkillSlotItems spellsSkillSlotItems { get; private set; }

	public GroupedSkillSlotItems afflictionsSkillSlotItems { get; private set; }

	public GroupedSkillSlotItems minionsSkillSlotItems { get; private set; }

	public GroupedSkillSlotItems structuresSkillSlotItems { get; private set; }

	public GroupedSkillSlotItems miscsSkillSlotItems { get; private set; }

	public bool moreLoadoutOptions { get; set; }

	public void Initialize()
	{
		spellsSkillSlotItems = new GroupedSkillSlotItems();
		afflictionsSkillSlotItems = new GroupedSkillSlotItems();
		minionsSkillSlotItems = new GroupedSkillSlotItems();
		structuresSkillSlotItems = new GroupedSkillSlotItems();
		miscsSkillSlotItems = new GroupedSkillSlotItems();
		loadoutChoices = new List<PLAYER_SKILL_TYPE>();
		LoadSkillDataToUI(loadout.spells.fixedSkills, loadout.spells.extraSlots, spellsSkillSlotItems, SaveManager.Instance.currentSaveDataPlayer.GetLoadoutExtraSpells(loadout.archetype), spellsScrollRect.content);
		LoadSkillDataToUI(loadout.afflictions.fixedSkills, loadout.afflictions.extraSlots, afflictionsSkillSlotItems, SaveManager.Instance.currentSaveDataPlayer.GetLoadoutExtraAfflictions(loadout.archetype), afflictionsScrollRect.content);
		LoadSkillDataToUI(loadout.minions.fixedSkills, loadout.minions.extraSlots, minionsSkillSlotItems, SaveManager.Instance.currentSaveDataPlayer.GetLoadoutExtraMinions(loadout.archetype), minionsScrollRect.content);
		LoadSkillDataToUI(loadout.structures.fixedSkills, loadout.structures.extraSlots, structuresSkillSlotItems, SaveManager.Instance.currentSaveDataPlayer.GetLoadoutExtraStructures(loadout.archetype), structuresScrollRect.content);
		LoadSkillDataToUI(loadout.miscs.fixedSkills, loadout.miscs.extraSlots, miscsSkillSlotItems, SaveManager.Instance.currentSaveDataPlayer.GetLoadoutExtraMiscs(loadout.archetype), miscsScrollRect.content);
		Messenger.AddListener<SkillSlotItem, PLAYER_ARCHETYPE>(UISignals.SKILL_SLOT_ITEM_CLICKED, OnClickSkillSlotItem);
		Messenger.AddListener(UISignals.SAVE_LOADOUTS, SaveLoadoutSettings);
		spellsTab.isOn = true;
	}

	public void OnDestroy()
	{
		Messenger.RemoveListener<SkillSlotItem, PLAYER_ARCHETYPE>(UISignals.SKILL_SLOT_ITEM_CLICKED, OnClickSkillSlotItem);
		Messenger.RemoveListener(UISignals.SAVE_LOADOUTS, SaveLoadoutSettings);
	}

	private void SaveLoadoutSettings()
	{
		if (!SaveManager.Instance.useSaveData)
		{
			SaveManager.Instance.currentSaveDataPlayer.ClearLoadoutSaveData(loadout.archetype);
			SaveManager.Instance.currentSaveDataPlayer.SaveLoadoutExtraSpells(loadout.archetype, spellsSkillSlotItems.extraSkillSlotItems);
			SaveManager.Instance.currentSaveDataPlayer.SaveLoadoutExtraAfflictions(loadout.archetype, afflictionsSkillSlotItems.extraSkillSlotItems);
			SaveManager.Instance.currentSaveDataPlayer.SaveLoadoutExtraMinions(loadout.archetype, minionsSkillSlotItems.extraSkillSlotItems);
			SaveManager.Instance.currentSaveDataPlayer.SaveLoadoutExtraStructures(loadout.archetype, structuresSkillSlotItems.extraSkillSlotItems);
			SaveManager.Instance.currentSaveDataPlayer.SaveLoadoutExtraMiscs(loadout.archetype, miscsSkillSlotItems.extraSkillSlotItems);
		}
	}

	public void OnClickSpellsTab(bool state)
	{
		if (state && spellsSkillSlotItems.fixedSkillSlotItems.Count <= 0 && spellsSkillSlotItems.extraSkillSlotItems.Count <= 0)
		{
			LoadSkillDataToUI(loadout.spells.fixedSkills, loadout.spells.extraSlots, spellsSkillSlotItems, SaveManager.Instance.currentSaveDataPlayer.GetLoadoutExtraSpells(loadout.archetype), spellsScrollRect.content);
		}
	}

	public void OnClickAfflictionsTab(bool state)
	{
		if (state && afflictionsSkillSlotItems.fixedSkillSlotItems.Count <= 0 && afflictionsSkillSlotItems.extraSkillSlotItems.Count <= 0)
		{
			LoadSkillDataToUI(loadout.afflictions.fixedSkills, loadout.afflictions.extraSlots, afflictionsSkillSlotItems, SaveManager.Instance.currentSaveDataPlayer.GetLoadoutExtraAfflictions(loadout.archetype), afflictionsScrollRect.content);
		}
	}

	public void OnClickMinionsTab(bool state)
	{
		if (state && minionsSkillSlotItems.fixedSkillSlotItems.Count <= 0 && minionsSkillSlotItems.extraSkillSlotItems.Count <= 0)
		{
			LoadSkillDataToUI(loadout.minions.fixedSkills, loadout.minions.extraSlots, minionsSkillSlotItems, SaveManager.Instance.currentSaveDataPlayer.GetLoadoutExtraMinions(loadout.archetype), minionsScrollRect.content);
		}
	}

	public void OnClickStructuresTab(bool state)
	{
		if (state && structuresSkillSlotItems.fixedSkillSlotItems.Count <= 0 && structuresSkillSlotItems.extraSkillSlotItems.Count <= 0)
		{
			LoadSkillDataToUI(loadout.structures.fixedSkills, loadout.structures.extraSlots, structuresSkillSlotItems, SaveManager.Instance.currentSaveDataPlayer.GetLoadoutExtraStructures(loadout.archetype), structuresScrollRect.content);
		}
	}

	public void OnClickMiscsTab(bool state)
	{
		if (state && miscsSkillSlotItems.fixedSkillSlotItems.Count <= 0 && miscsSkillSlotItems.extraSkillSlotItems.Count <= 0)
		{
			LoadSkillDataToUI(loadout.miscs.fixedSkills, loadout.miscs.extraSlots, miscsSkillSlotItems, SaveManager.Instance.currentSaveDataPlayer.GetLoadoutExtraMiscs(loadout.archetype), miscsScrollRect.content);
		}
	}

	private void LoadSkillDataToUI(List<PLAYER_SKILL_TYPE> fixedSkills, int extraSlots, GroupedSkillSlotItems groupedSkillSlotItems, List<PLAYER_SKILL_TYPE> extraSkills, Transform parent)
	{
		if (fixedSkills != null)
		{
			for (int i = 0; i < fixedSkills.Count; i++)
			{
				PLAYER_SKILL_TYPE skillType = fixedSkills[i];
				SkillSlotItem skillSlotItem = CreateNewSkillSlotItem(parent);
				skillSlotItem.SetSkillSlotItem(loadout.archetype, skillType, isFixed: true);
				skillSlotItem.SetOnHoverEnterAction(OnHoverEnterSkillSlotItem);
				skillSlotItem.SetOnHoverExitAction(OnHoverExitSkillSlotItem);
				groupedSkillSlotItems.AddFixedSkillSlotItem(skillSlotItem);
			}
		}
		for (int j = 0; j < extraSlots; j++)
		{
			PlayerSkillData playerSkillData = null;
			if (extraSkills != null && extraSkills.Count > 0 && j < extraSkills.Count)
			{
				playerSkillData = PlayerSkillManager.Instance.GetScriptableObjPlayerSkillData<PlayerSkillData>(extraSkills[j]);
			}
			if (!(playerSkillData != null) || PlayerSkillManager.Instance.GetSkillData(playerSkillData.skill) != null)
			{
				SkillSlotItem skillSlotItem2 = CreateNewSkillSlotItem(parent);
				skillSlotItem2.SetSkillSlotItem(loadout.archetype, playerSkillData, isFixed: false);
				skillSlotItem2.SetOnHoverEnterAction(OnHoverEnterSkillSlotItem);
				skillSlotItem2.SetOnHoverExitAction(OnHoverExitSkillSlotItem);
				groupedSkillSlotItems.AddExtraSkillSlotItem(skillSlotItem2);
			}
		}
	}

	private SkillSlotItem CreateNewSkillSlotItem(Transform parent)
	{
		GameObject obj = Object.Instantiate(skillSlotItemPrefab, parent);
		obj.transform.localPosition = Vector3.zero;
		return obj.GetComponent<SkillSlotItem>();
	}

	private void OnClickSkillSlotItem(SkillSlotItem slotItem, PLAYER_ARCHETYPE archetype)
	{
		if (archetype != loadout.archetype)
		{
			return;
		}
		pickedSlotItem = slotItem;
		loadoutChoices.Clear();
		PLAYER_SKILL_TYPE[] array = null;
		List<PLAYER_SKILL_TYPE> list = null;
		GroupedSkillSlotItems groupedSkillSlotItems = null;
		if (spellsTab.isOn)
		{
			array = loadout.availableSpells;
			if (moreLoadoutOptions)
			{
				array = PlayerSkillManager.Instance.allSpells;
			}
			list = loadout.spells.fixedSkills;
			groupedSkillSlotItems = spellsSkillSlotItems;
		}
		else if (afflictionsTab.isOn)
		{
			array = loadout.availableAfflictions;
			if (moreLoadoutOptions)
			{
				array = PlayerSkillManager.Instance.allAfflictions;
			}
			list = loadout.afflictions.fixedSkills;
			groupedSkillSlotItems = afflictionsSkillSlotItems;
		}
		else if (minionsTab.isOn)
		{
			array = loadout.availableMinions;
			if (moreLoadoutOptions)
			{
				array = PlayerSkillManager.Instance.allMinionPlayerSkills;
			}
			list = loadout.minions.fixedSkills;
			groupedSkillSlotItems = minionsSkillSlotItems;
		}
		else if (structuresTab.isOn)
		{
			array = loadout.availableStructures;
			if (moreLoadoutOptions)
			{
				array = PlayerSkillManager.Instance.allDemonicStructureSkills;
			}
			list = loadout.structures.fixedSkills;
			groupedSkillSlotItems = structuresSkillSlotItems;
		}
		else if (miscsTab.isOn)
		{
			array = loadout.availableMiscs;
			if (moreLoadoutOptions)
			{
				array = PlayerSkillManager.Instance.allPlayerActions;
			}
			list = loadout.miscs.fixedSkills;
			groupedSkillSlotItems = miscsSkillSlotItems;
		}
		if (array != null)
		{
			foreach (PLAYER_SKILL_TYPE pLAYER_SKILL_TYPE in array)
			{
				if (PlayerSkillManager.Instance.playerSkillDataDictionary.ContainsKey(pLAYER_SKILL_TYPE) && !PlayerSkillManager.Instance.constantSkills.Contains(pLAYER_SKILL_TYPE) && !list.Contains(pLAYER_SKILL_TYPE) && !groupedSkillSlotItems.HasExtraSkill(pLAYER_SKILL_TYPE))
				{
					loadoutChoices.Add(pLAYER_SKILL_TYPE);
				}
			}
		}
		if (loadoutChoices.Count > 0)
		{
			objectPicker.ShowLoadoutPicker(loadoutChoices, OnConfirmSkill, OnHoverEnterSkill, OnHoverExitSkill);
		}
	}

	private void OnConfirmSkill(PlayerSkillData skillData)
	{
		if ((bool)pickedSlotItem)
		{
			pickedSlotItem.SetSkillSlotItem(loadout.archetype, skillData, isFixed: false);
		}
	}

	private void OnHoverEnterSkill(PlayerSkillData skillData)
	{
		skillDetailsTooltip.ShowPlayerSkillDetails(skillData, 0, objectPicker.hoverPos);
	}

	private void OnHoverExitSkill(PlayerSkillData skillData)
	{
		skillDetailsTooltip.HidePlayerSkillDetails();
	}

	private void OnHoverEnterSkillSlotItem(PlayerSkillData skillData)
	{
		if (skillData != null)
		{
			skillDetailsTooltip.ShowPlayerSkillDetails(skillData);
		}
	}

	private void OnHoverExitSkillSlotItem(PlayerSkillData skillData)
	{
		skillDetailsTooltip.HidePlayerSkillDetails();
	}

	public void ClearExtraSlotItems()
	{
		for (int i = 0; i < spellsSkillSlotItems.extraSkillSlotItems.Count; i++)
		{
			spellsSkillSlotItems.extraSkillSlotItems[i].ClearData();
		}
		for (int j = 0; j < afflictionsSkillSlotItems.extraSkillSlotItems.Count; j++)
		{
			afflictionsSkillSlotItems.extraSkillSlotItems[j].ClearData();
		}
		for (int k = 0; k < minionsSkillSlotItems.extraSkillSlotItems.Count; k++)
		{
			minionsSkillSlotItems.extraSkillSlotItems[k].ClearData();
		}
		for (int l = 0; l < structuresSkillSlotItems.extraSkillSlotItems.Count; l++)
		{
			structuresSkillSlotItems.extraSkillSlotItems[l].ClearData();
		}
		for (int m = 0; m < miscsSkillSlotItems.extraSkillSlotItems.Count; m++)
		{
			miscsSkillSlotItems.extraSkillSlotItems[m].ClearData();
		}
	}

	public void SetMoreLoadoutOptions(bool state, bool doEffect)
	{
		if (moreLoadoutOptions != state)
		{
			moreLoadoutOptions = state;
			if (doEffect && !moreLoadoutOptions)
			{
				ClearExtraSlotItems();
			}
		}
	}
}
