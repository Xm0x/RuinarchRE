using System;
using UnityEngine;
using UnityEngine.UI;
using UtilityScripts;

[Serializable]
public class DeployedMonsterItemUI : MonoBehaviour
{
	public Action<DeployedMonsterItemUI> onDelete;

	public Action<DeployedMonsterItemUI> onUnlockClicked;

	public Action onAddSummonClicked;

	public Button btnDelete;

	public Button btnItemClick;

	public Button btnUnlockSlot;

	public Button btnAddSummon;

	public RuinarchText txtName;

	public RuinarchText txtSummonCost;

	public RuinarchText txtUnlockCost;

	public GameObject lockCover;

	public GameObject emptyCover;

	public GameObject deadIcon;

	public GameObject addSummonCover;

	public bool isReadyForDeploy;

	public bool isDeployed;

	public bool isMinion;

	public int summonCost;

	public int unlockCost;

	public HoverText hoverText;

	public GameObject manaIconAndPrice;

	public Character deployedCharacter;

	private MonsterAndDemonUnderlingCharges _monsterOrMinion;

	public MonsterAndDemonUnderlingCharges obj => _monsterOrMinion;

	private void OnEnable()
	{
		btnDelete.onClick.AddListener(OnDeleteClicked);
		btnItemClick.onClick.AddListener(OnItemClicked);
		btnUnlockSlot.onClick.AddListener(OnUnlockClicked);
		btnAddSummon.onClick.AddListener(OnAddSummonClicked);
	}

	private void OnDisable()
	{
		btnDelete.onClick.RemoveListener(OnDeleteClicked);
		btnItemClick.onClick.RemoveListener(OnItemClicked);
		btnUnlockSlot.onClick.RemoveListener(OnUnlockClicked);
		btnAddSummon.onClick.RemoveListener(OnAddSummonClicked);
	}

	public void InitializeItem(MonsterAndDemonUnderlingCharges p_underling, bool p_isDeployed = false, bool p_hideRemoveButton = false)
	{
		CharacterClass characterClass = CharacterManager.Instance.GetCharacterClass(p_underling.characterClassName);
		_monsterOrMinion = p_underling;
		isMinion = p_underling.isDemon;
		txtName.text = (isMinion ? (Utilities.DemonIcon() + characterClass.displayName) : (Utilities.MonsterIcon() + characterClass.displayName));
		summonCost = characterClass.GetSummonCost();
		txtSummonCost.text = summonCost.ToString();
		if (!p_isDeployed)
		{
			isReadyForDeploy = true;
			isDeployed = false;
		}
		else
		{
			isReadyForDeploy = false;
			isDeployed = true;
		}
		if (p_hideRemoveButton)
		{
			HideRemoveButton();
		}
		else
		{
			ShowRemoveButton();
		}
		if (isReadyForDeploy || p_isDeployed)
		{
			addSummonCover.SetActive(value: false);
		}
		else
		{
			addSummonCover.SetActive(value: true);
		}
		lockCover.SetActive(value: false);
		emptyCover.SetActive(value: false);
		HideDeadIcon();
	}

	public void MakeSlotEmpty(bool p_hideAddSummon = false)
	{
		lockCover.SetActive(value: false);
		isDeployed = false;
		isReadyForDeploy = false;
		HideDeadIcon();
		emptyCover.SetActive(value: true);
		addSummonCover.SetActive(!p_hideAddSummon);
		btnAddSummon.interactable = false;
	}

	public void ResetButton()
	{
		deployedCharacter = null;
		isDeployed = false;
		isReadyForDeploy = false;
		emptyCover.SetActive(value: true);
	}

	public void MakeSlotLocked(bool p_isAbleToBuy)
	{
		isDeployed = false;
		isReadyForDeploy = false;
		emptyCover.SetActive(value: false);
		lockCover.SetActive(value: true);
		addSummonCover.SetActive(value: false);
		txtUnlockCost.text = GetUnlockCost().ToString();
		btnUnlockSlot.gameObject.SetActive(value: true);
		HideDeadIcon();
		if (p_isAbleToBuy)
		{
			btnUnlockSlot.interactable = true;
			hoverText.SetText("Expand Capacity");
		}
		else
		{
			btnUnlockSlot.interactable = false;
			hoverText.SetText("Not_Enough_Chaotic_Energy");
		}
	}

	public void MakeSlotLockedNoButton()
	{
		MakeSlotLocked(p_isAbleToBuy: false);
		btnUnlockSlot.gameObject.SetActive(value: false);
	}

	public void EnableButton()
	{
		btnDelete.interactable = true;
		lockCover.SetActive(value: false);
	}

	private void OnDeleteClicked()
	{
		onDelete?.Invoke(this);
	}

	private void OnUnlockClicked()
	{
		onUnlockClicked?.Invoke(this);
	}

	private void OnAddSummonClicked()
	{
		onAddSummonClicked?.Invoke();
	}

	private void OnItemClicked()
	{
		if (deployedCharacter != null)
		{
			UIManager.Instance.OpenObjectUI(deployedCharacter);
		}
	}

	public void UndeployCharacter()
	{
		deployedCharacter = null;
	}

	public void HideManaCost()
	{
		manaIconAndPrice.gameObject.SetActive(value: false);
	}

	public void ShowManaCost()
	{
		manaIconAndPrice.gameObject.SetActive(value: true);
	}

	public void Deploy(Character p_createdCharacter = null, bool p_dontHideremoveButton = false)
	{
		deployedCharacter = p_createdCharacter;
		isDeployed = true;
		isReadyForDeploy = false;
		if (p_dontHideremoveButton)
		{
			ShowRemoveButton();
		}
		else
		{
			HideRemoveButton();
		}
	}

	public void HideRemoveButton()
	{
		btnDelete.gameObject.SetActive(value: false);
	}

	public void ShowRemoveButton()
	{
		btnDelete.gameObject.SetActive(value: true);
	}

	public void ShowDeadIcon()
	{
		deployedCharacter = null;
		isDeployed = true;
		deadIcon.SetActive(value: true);
	}

	public void HideDeadIcon()
	{
		deadIcon.SetActive(value: false);
	}

	public void DisplayAddSummon()
	{
		emptyCover.SetActive(value: false);
		lockCover.SetActive(value: false);
		btnUnlockSlot.gameObject.SetActive(value: false);
		addSummonCover.SetActive(value: true);
		HideDeadIcon();
		btnAddSummon.interactable = true;
	}

	public int GetUnlockCost()
	{
		return SpellUtilities.GetModifiedSpellCost(unlockCost, WorldSettings.Instance.worldSettingsData.playerSkillSettings.GetCostsModification(), CharacterManager.Instance.GetChaoticEnergyCostIncrease());
	}
}
