using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UtilityScripts;

public class BuildListUI : PopupMenuBase
{
	[Header("Build")]
	[SerializeField]
	private Toggle buildToggle;

	[SerializeField]
	private Toggle structuresToggle;

	[SerializeField]
	private CanvasGroup structuresCanvasGroup;

	[SerializeField]
	private GridLayoutGroup structuresLayoutGroup;

	[SerializeField]
	private GameObject buildItemPrefab;

	[SerializeField]
	private UIHoverPosition buildTooltipPosition;

	[Header("Decorations")]
	[SerializeField]
	private CanvasGroup decorationsCanvasGroup;

	[SerializeField]
	private DecorationNameplateToggle _decorationsNameplateToggle;

	[SerializeField]
	private DecorationListUI _decorationsUI;

	private List<SpellItem> buildItems = new List<SpellItem>(14);

	public DecorationListUI decorationsUI => _decorationsUI;

	public override void Open()
	{
		base.Open();
		UpdateBuildList();
		SubscribeListeners();
		buildToggle.SetIsOnWithoutNotify(value: true);
	}

	public override void Close()
	{
		buildToggle.SetIsOnWithoutNotify(value: false);
		UnsubscribeListeners();
		base.Close();
	}

	protected override void OnGameObjectEnabled()
	{
		base.OnGameObjectEnabled();
		Messenger.Broadcast(UISignals.TOP_UI_ENABLED);
	}

	protected override void OnGameObjectDisabled()
	{
		base.OnGameObjectDisabled();
		Messenger.Broadcast(UISignals.TOP_UI_DISABLED);
	}

	private void SubscribeListeners()
	{
		Messenger.AddListener<int, int>(PlayerSignals.PLAYER_ADJUSTED_SPIRIT_ENERGY, OnPlayerAdjustedSpiritEnergy);
	}

	private void UnsubscribeListeners()
	{
		Messenger.RemoveListener<int, int>(PlayerSignals.PLAYER_ADJUSTED_SPIRIT_ENERGY, OnPlayerAdjustedSpiritEnergy);
	}

	private void OnPlayerAdjustedSpiritEnergy(int p_adjustedAmount, int p_totalSpiritEnergy)
	{
		UpdateDecorationsInteractability();
	}

	public void Initialize()
	{
		Close();
		PopulateBuildingList();
		Messenger.AddListener(UISignals.UPDATE_BUILD_LIST, UpdateBuildList);
		Messenger.AddListener<PLAYER_SKILL_TYPE>(PlayerSkillSignals.PLAYER_GAINED_DEMONIC_STRUCTURE, OnPlayerGainedDemonicStructure);
		Messenger.AddListener<PLAYER_SKILL_TYPE>(PlayerSkillSignals.PLAYER_LOST_DEMONIC_STRUCTURE, OnPlayerLostDemonicStructure);
		InitializeDecorations();
	}

	private void OnPlayerGainedDemonicStructure(PLAYER_SKILL_TYPE p_structureType)
	{
		DemonicStructurePlayerSkill demonicStructureSkillData = PlayerSkillManager.Instance.GetDemonicStructureSkillData(p_structureType);
		CreateStructureItem(demonicStructureSkillData);
	}

	private void OnPlayerLostDemonicStructure(PLAYER_SKILL_TYPE p_structureType)
	{
		DeleteStructureItem(p_structureType);
	}

	private void PopulateBuildingList()
	{
		Utilities.DestroyChildrenObjectPool(structuresLayoutGroup.transform);
		for (int i = 0; i < PlayerManager.Instance.player.playerSkillComponent.buildSkills.Count; i++)
		{
			PLAYER_SKILL_TYPE pLAYER_SKILL_TYPE = PlayerManager.Instance.player.playerSkillComponent.buildSkills[i];
			if (pLAYER_SKILL_TYPE != PLAYER_SKILL_TYPE.DECORATIONS)
			{
				BuildPlayerSkill buildSkillData = PlayerSkillManager.Instance.GetBuildSkillData(pLAYER_SKILL_TYPE);
				CreateStructureItem(buildSkillData);
			}
		}
		for (int j = 0; j < PlayerManager.Instance.player.playerSkillComponent.demonicStructuresSkills.Count; j++)
		{
			PLAYER_SKILL_TYPE type = PlayerManager.Instance.player.playerSkillComponent.demonicStructuresSkills[j];
			DemonicStructurePlayerSkill demonicStructureSkillData = PlayerSkillManager.Instance.GetDemonicStructureSkillData(type);
			CreateStructureItem(demonicStructureSkillData);
		}
	}

	private void CreateStructureItem(SkillData p_skill)
	{
		SpellItem component = ObjectPoolManager.Instance.InstantiateObjectFromPool(buildItemPrefab.name, Vector3.zero, Quaternion.identity, structuresLayoutGroup.transform).GetComponent<SpellItem>();
		component.SetObject(p_skill);
		component.SetInteractableChecker(CanChooseLandmark);
		component.ClearAllHoverEnterActions();
		component.ClearAllHoverExitActions();
		component.AddHoverEnterAction(OnHoverEnterBuildItem);
		component.AddHoverExitAction(OnHoverExitBuildItem);
		component.ForceUpdateInteractableState();
		buildItems.Add(component);
	}

	private void DeleteStructureItem(PLAYER_SKILL_TYPE structureSpell)
	{
		SpellItem buildItem = GetBuildItem(structureSpell);
		if (buildItem != null)
		{
			ObjectPoolManager.Instance.DestroyObject(buildItem);
			buildItems.Remove(buildItem);
		}
	}

	private SpellItem GetBuildItem(PLAYER_SKILL_TYPE structureSpell)
	{
		for (int i = 0; i < buildItems.Count; i++)
		{
			SpellItem spellItem = buildItems[i];
			if (spellItem.spellData.type == structureSpell)
			{
				return spellItem;
			}
		}
		return null;
	}

	private void OnHoverEnterBuildItem(SkillData spellData)
	{
		Vector2 p_spriteSize = new Vector2(150f, 150f);
		if (spellData.type == PLAYER_SKILL_TYPE.SPIRE)
		{
			p_spriteSize = new Vector2(150f, 300f);
		}
		PlayerUI.Instance.OnHoverSpell(spellData, buildTooltipPosition, null, p_showImage: true, p_spriteSize);
	}

	private void OnHoverExitBuildItem(SkillData spellData)
	{
		PlayerUI.Instance.OnHoverOutSpell(spellData);
	}

	private void UpdateBuildList()
	{
		for (int i = 0; i < buildItems.Count; i++)
		{
			buildItems[i].ForceUpdateInteractableState();
		}
		UpdateDecorations();
		if (!_decorationsNameplateToggle.toggle.interactable)
		{
			structuresToggle.isOn = true;
		}
	}

	private bool CanChooseLandmark(SkillData p_spellData)
	{
		return p_spellData.CanPerformAbility();
	}

	public void OnToggleStructuresTab(bool p_isOn)
	{
		structuresCanvasGroup.gameObject.SetActive(p_isOn);
	}

	public void OnToggleDecorationsTab(bool p_isOn)
	{
		if (p_isOn)
		{
			ShowDecorations();
		}
		else
		{
			HideDecorations();
		}
	}

	private void InitializeDecorations()
	{
		_decorationsNameplateToggle.SetDecorationSkill(PlayerSkillManager.Instance.GetBuildSkillData(PLAYER_SKILL_TYPE.DECORATIONS) as DecorationsData);
		_decorationsNameplateToggle.SetHoverEnterAction(OnHoverEnterBuildItem);
		_decorationsNameplateToggle.SetHoverExitAction(OnHoverExitBuildItem);
		_decorationsUI.Initialize();
	}

	private void ShowDecorations()
	{
		_decorationsUI.ShowDecorationsUI();
	}

	private void HideDecorations()
	{
		_decorationsUI.HideDecorationsUI();
	}

	public void UpdateDecorations()
	{
		_decorationsNameplateToggle.UpdateData();
		UpdateDecorationsInteractability();
	}

	public void UpdateDecorationsInteractability()
	{
		_decorationsNameplateToggle.UpdateInteractability();
		_decorationsUI.UpdateDecorationItemsInteractableState(_decorationsNameplateToggle.toggle.interactable);
	}
}
