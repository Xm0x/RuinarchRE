using System;
using System.Linq;
using Inner_Maps;
using Inner_Maps.Location_Structures;
using Ruinarch.Custom_UI;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UI.Extensions;
using UtilityScripts;

public class SkillTreeSelector : MonoBehaviour
{
	[SerializeField]
	private HorizontalScrollSnap _horizontalScrollSnap;

	[SerializeField]
	private Button continueBtn;

	[SerializeField]
	private RuinarchToggle moreLoadoutOptionsToggle;

	[SerializeField]
	private Toggle[] archetypeToggles;

	[SerializeField]
	private PlayerSkillLoadoutUI[] playerLoadoutUI;

	public void Initialize()
	{
		base.gameObject.SetActive(value: true);
		_horizontalScrollSnap.Awake();
		for (int i = 0; i < playerLoadoutUI.Length; i++)
		{
			playerLoadoutUI[i].Initialize();
			playerLoadoutUI[i].SetMoreLoadoutOptions(state: false, doEffect: false);
		}
		for (int j = 0; j < archetypeToggles.Length; j++)
		{
			Toggle toggle = archetypeToggles[j];
			PlayerSkillLoadoutUI playerSkillLoadoutUI = playerLoadoutUI[j];
			if (!WorldSettings.Instance.worldSettingsData.playerSkillSettings.forcedArchetypes.Contains(playerSkillLoadoutUI.loadout.archetype))
			{
				toggle.gameObject.SetActive(value: false);
				playerSkillLoadoutUI.gameObject.SetActive(value: false);
				_horizontalScrollSnap.RemoveChild(playerSkillLoadoutUI.transform.GetSiblingIndex(), out var _);
				toggle.SetIsOnWithoutNotify(value: false);
			}
		}
		moreLoadoutOptionsToggle.SetIsOnWithoutNotify(value: false);
		base.gameObject.SetActive(value: false);
	}

	public void Show()
	{
		base.gameObject.SetActive(value: true);
		for (int i = 0; i < archetypeToggles.Length; i++)
		{
			Toggle toggle = archetypeToggles[i];
			if (toggle.gameObject.activeInHierarchy)
			{
				toggle.isOn = true;
				break;
			}
		}
	}

	public void Hide()
	{
		base.gameObject.SetActive(value: false);
	}

	public void OnToggleMoreLoadoutOptions(bool state)
	{
		MoreLoadoutOptions(state);
	}

	private void MoreLoadoutOptions(bool state)
	{
		for (int i = 0; i < playerLoadoutUI.Length; i++)
		{
			playerLoadoutUI[i].SetMoreLoadoutOptions(state, doEffect: true);
		}
	}

	public void OnClickContinue()
	{
		continueBtn.interactable = false;
		PLAYER_ARCHETYPE selectedArchetype = GetSelectedArchetype();
		PlayerSkillManager.Instance.SetSelectedArchetype(selectedArchetype);
		if (selectedArchetype.IsLichLoadout())
		{
			PlayerManager.Instance.player.playerFaction.SetRelationshipFor(FactionManager.Instance.undeadFaction, FACTION_RELATIONSHIP_STATUS.Friendly);
		}
		SaveManager.Instance.currentSaveDataPlayer.SetMoreLoadoutOptions(moreLoadoutOptionsToggle.isOn);
		BroadcastLoadoutSelectedSignals();
		UIManager.Instance.initialWorldSetupMenu.Hide();
		InnerMapManager.Instance.TryShowLocationMap(GridMap.Instance.mainRegion);
		(PlayerManager.Instance.player.playerSettlement.GetRandomStructureOfType(STRUCTURE_TYPE.THE_PORTAL) as ThePortal).CenterOnStructure();
		if (WorldConfigManager.Instance.mapGenerationData.isGeneratingTileObjects)
		{
			UIManager.Instance.ShowWaitForTileObjectGenerationToFinishWindow();
		}
		else
		{
			GameManager.Instance.StartProgression();
		}
	}

	public void LoadLoadout(PLAYER_ARCHETYPE archetype)
	{
		PlayerSkillManager.Instance.SetSelectedArchetype(archetype);
		BroadcastLoadoutSelectedSignals();
		GameManager.Instance.LoadProgression();
		UIManager.Instance.initialWorldSetupMenu.Hide();
		InnerMapManager.Instance.TryShowLocationMap(GridMap.Instance.mainRegion);
		(PlayerManager.Instance.player.playerSettlement.GetRandomStructureOfType(STRUCTURE_TYPE.THE_PORTAL) as ThePortal).CenterOnStructure();
	}

	private void BroadcastLoadoutSelectedSignals()
	{
		Messenger.Broadcast(UISignals.SAVE_LOADOUTS);
		Messenger.Broadcast(UISignals.START_GAME_AFTER_LOADOUT_SELECT);
	}

	private PLAYER_ARCHETYPE GetSelectedArchetype()
	{
		if (WorldSettings.Instance.worldSettingsData.playerSkillSettings.omnipotentMode == OMNIPOTENT_MODE.Enabled)
		{
			return PLAYER_ARCHETYPE.Attainment_Ravager;
		}
		for (int i = 0; i < archetypeToggles.Length; i++)
		{
			Toggle toggle = archetypeToggles[i];
			if (toggle.gameObject.activeInHierarchy && toggle.isOn)
			{
				return (PLAYER_ARCHETYPE)Enum.Parse(typeof(PLAYER_ARCHETYPE), Utilities.NotNormalizedConversionStringToEnum(toggle.gameObject.name));
			}
		}
		return PLAYER_ARCHETYPE.Normal;
	}

	private void OnScreenResolutionChanged()
	{
		StartCoroutine(_horizontalScrollSnap.UpdateLayoutCoroutine());
	}

	public void OnHoverMoreLoadoutOptions()
	{
		string text = "Unlock all available optional abilities regardless of Archetype.";
		text += "\n<color=red>Warning: Toggling this off will reset all optional loadouts.</color>";
		UIManager.Instance.ShowSmallInfo(text);
	}

	public void OnHoverOutMoreLoadoutOptions()
	{
		UIManager.Instance.HideSmallInfo();
	}
}
