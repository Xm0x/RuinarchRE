using System.Collections;
using System.Collections.Generic;
using Inner_Maps;
using Locations.Settlements;
using Player_Input;
using Quests.Alerts;
using Ruinarch;
using Tutorial;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Serialization;
using UtilityScripts;

public class PlayerManager : BaseMonoBehaviour, LocalizationManagerEventDispatcher.ILocaleChangeListener
{
	public static PlayerManager Instance;

	public Player player;

	[Header("Job Action Icons")]
	[FormerlySerializedAs("jobActionIcons")]
	[SerializeField]
	private StringSpriteDictionary spellIcons;

	[Header("Combat Ability Icons")]
	[SerializeField]
	private StringSpriteDictionary combatAbilityIcons;

	[Header("Intervention Ability Tiers")]
	[FormerlySerializedAs("interventionAbilityTiers")]
	[SerializeField]
	private InterventionAbilityTierDictionary spellTiers;

	[Header("Mana Orbs")]
	[SerializeField]
	private GameObject chaosOrbPrefab;

	[Header("Spirit Energy")]
	[SerializeField]
	private GameObject spiritnEnergyPrefab;

	[Header("Base Building")]
	[SerializeField]
	private PlayerStructurePlacementVisual _structurePlacementVisual;

	private List<PlayerInputModule> _playerInputModules;

	public static SeizeInputModule seizeInputModule;

	public static SpellInputModule spellInputModule;

	public static IntelInputModule intelInputModule;

	public static PickPortalInputModule pickPortalInputModule;

	public List<ChaosOrb> availableChaosOrbs;

	public List<SpiritEnergy> availableSpiritEnergy;

	private WeightedDictionary<PLAYER_SKILL_TYPE> _resonancePowerWeights;

	public ShareIntelContextMenuItem shareIntelContextMenuItem { get; private set; }

	private void Awake()
	{
		Instance = this;
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		_ = LocalizationManager.Instance != null;
		Instance = null;
		seizeInputModule = null;
		spellInputModule = null;
		intelInputModule = null;
		pickPortalInputModule = null;
		shareIntelContextMenuItem?.Cleanup();
		shareIntelContextMenuItem = null;
		InputManager.RemoveOnUpdateEvent(ProcessPlayerInputModules);
	}

	public void Initialize()
	{
		availableChaosOrbs = new List<ChaosOrb>();
		_playerInputModules = new List<PlayerInputModule>();
		Messenger.AddListener<Vector3, int, InnerTileMap>(PlayerSignals.CREATE_CHAOS_ORBS, CreateChaosOrbsAt);
		Messenger.AddListener<Vector3, int, InnerTileMap>(PlayerSignals.CREATE_SPIRIT_ENERGY, CreateSpiritEnergyAt);
		Messenger.AddListener<string>(PlayerSignals.WIN_GAME, WinGame);
		Messenger.AddListener<Faction>(FactionSignals.FACTION_BECAME_AWARE_OF_PLAYER, OnFactionBecameAwareOfPlayer);
		Messenger.AddListener<Character>(PlayerSignals.MUMMIFIED_RELEASED_BY_PLAYER, MummifiedReleasedByPlayer);
		seizeInputModule = new SeizeInputModule();
		spellInputModule = new SpellInputModule();
		intelInputModule = new IntelInputModule();
		pickPortalInputModule = new PickPortalInputModule();
		shareIntelContextMenuItem = new ShareIntelContextMenuItem();
		InputManager.AddOnUpdateEvent(ProcessPlayerInputModules);
		_structurePlacementVisual.Initialize(InnerMapCameraMove.Instance.camera);
		ConstructResonancePowerWeights();
	}

	public void InitializePlayer(Area portal, PlayerSettlement p_settlement)
	{
		player = new Player();
		player.CreatePlayerFaction();
		player.SetPortalTile(portal);
		if (!portal.HasSettlementOnArea(p_settlement))
		{
			p_settlement.AddPortalAreaToPlayerSettlement(portal);
		}
		player.SetPlayerArea(p_settlement);
		LandmarkManager.Instance.OwnSettlement(player.playerFaction, p_settlement);
		player.storedTargetsComponent.UpdateRaidTargetsList();
		PlayerSkillManager.Instance.GetSelectedLoadout().ResetDataBeforeGameStart();
		PlayerUI.Instance.UpdateUI();
	}

	public void InitializePlayer(SaveDataCurrentProgress data)
	{
		player = data.LoadPlayer();
	}

	public int GetManaCostForSpell(int tier)
	{
		return tier switch
		{
			1 => 150, 
			2 => 100, 
			_ => 50, 
		};
	}

	private void ProcessPlayerInputModules()
	{
		if (_playerInputModules != null)
		{
			for (int i = 0; i < _playerInputModules.Count; i++)
			{
				_playerInputModules[i].OnUpdate();
			}
		}
	}

	public void AddPlayerInputModule(PlayerInputModule p_module)
	{
		if (!_playerInputModules.Contains(p_module))
		{
			_playerInputModules.Add(p_module);
			p_module.OnModuleAdded();
		}
	}

	public void RemovePlayerInputModule(PlayerInputModule p_module)
	{
		if (_playerInputModules.Remove(p_module))
		{
			p_module.OnModuleRemoved();
		}
	}

	public Sprite GetJobActionSprite(string actionName)
	{
		if (spellIcons.ContainsKey(actionName))
		{
			return spellIcons[actionName];
		}
		return null;
	}

	public Sprite GetCombatAbilitySprite(string abilityName)
	{
		if (combatAbilityIcons.ContainsKey(abilityName))
		{
			return combatAbilityIcons[abilityName];
		}
		return null;
	}

	public int GetSpellTier(PLAYER_SKILL_TYPE abilityType)
	{
		if (spellTiers.ContainsKey(abilityType))
		{
			return spellTiers[abilityType];
		}
		return 3;
	}

	public void CreateChaosOrbFromSave(Vector3 worldPos, Region region)
	{
		worldPos.z = -1f;
		GameObject obj = ObjectPoolManager.Instance.InstantiateObjectFromPool(chaosOrbPrefab.name, Vector3.zero, Quaternion.identity, region.innerMap.objectsParent);
		obj.transform.position = worldPos;
		ChaosOrb component = obj.GetComponent<ChaosOrb>();
		component.Initialize(worldPos, region);
		availableChaosOrbs.Add(component);
	}

	private void CreateChaosOrbsAt(Vector3 worldPos, int amount, InnerTileMap mapLocation)
	{
		StartCoroutine(ChaosOrbCreationCoroutine(worldPos, amount, mapLocation));
	}

	private IEnumerator ChaosOrbCreationCoroutine(Vector3 worldPos, int amount, InnerTileMap mapLocation)
	{
		worldPos.z = -1f;
		for (int i = 0; i < amount; i++)
		{
			GameObject obj = ObjectPoolManager.Instance.InstantiateObjectFromPool(chaosOrbPrefab.name, Vector3.zero, Quaternion.identity, mapLocation.objectsParent);
			obj.transform.position = worldPos;
			ChaosOrb component = obj.GetComponent<ChaosOrb>();
			component.Initialize(mapLocation.region);
			AddAvailableChaosOrb(component);
			yield return null;
		}
	}

	private void AddAvailableChaosOrb(ChaosOrb chaosOrb)
	{
		availableChaosOrbs.Add(chaosOrb);
		Messenger.Broadcast(PlayerSignals.CHAOS_ORB_SPAWNED);
	}

	public void RemoveChaosOrbFromAvailability(ChaosOrb chaosOrb)
	{
		availableChaosOrbs.Remove(chaosOrb);
		Messenger.Broadcast(PlayerSignals.CHAOS_ORB_DESPAWNED);
	}

	public void CreateSpiritEnergyFromSave(Vector3 worldPos, Region region)
	{
		worldPos.z = -1f;
		GameObject obj = ObjectPoolManager.Instance.InstantiateObjectFromPool(spiritnEnergyPrefab.name, Vector3.zero, Quaternion.identity, region.innerMap.objectsParent);
		obj.transform.position = worldPos;
		SpiritEnergy component = obj.GetComponent<SpiritEnergy>();
		component.Initialize(worldPos, region);
		availableSpiritEnergy.Add(component);
	}

	private void CreateSpiritEnergyAt(Vector3 worldPos, int amount, InnerTileMap mapLocation)
	{
		StartCoroutine(SpiritEnergyCreationCoroutine(worldPos, amount, mapLocation));
	}

	private IEnumerator SpiritEnergyCreationCoroutine(Vector3 worldPos, int amount, InnerTileMap mapLocation)
	{
		worldPos.z = -1f;
		for (int i = 0; i < amount; i++)
		{
			GameObject obj = ObjectPoolManager.Instance.InstantiateObjectFromPool(spiritnEnergyPrefab.name, Vector3.zero, Quaternion.identity, mapLocation.objectsParent);
			obj.transform.position = worldPos;
			SpiritEnergy component = obj.GetComponent<SpiritEnergy>();
			component.Initialize(mapLocation.region, amount);
			AddAvailableSpritiEnergy(component);
			yield return null;
		}
	}

	private void AddAvailableSpritiEnergy(SpiritEnergy p_spiritEnergy)
	{
		availableSpiritEnergy.Add(p_spiritEnergy);
		Messenger.Broadcast(PlayerSignals.SPIRIT_ENERGY_SPAWNED);
	}

	public void RemoveSpiritEnergyFromAvailability(SpiritEnergy p_spiritEnergy)
	{
		availableSpiritEnergy.Remove(p_spiritEnergy);
		Messenger.Broadcast(PlayerSignals.SPIRIT_ENERGY_DESPAWNED);
	}

	private void WinGame(string winMessage)
	{
		StartCoroutine(DelayedWinGame(winMessage));
	}

	private IEnumerator DelayedWinGame(string winMessage)
	{
		UIManager.Instance.Pause();
		UIManager.Instance.SetSpeedTogglesState(state: false);
		yield return GameUtilities.waitFor2Seconds;
		AkSoundEngine.PostEvent("Play_Victory", InnerMapCameraMove.Instance.gameObject);
		PlayerUI.Instance.WinGameOver(winMessage);
	}

	public void ShowStructurePlacementVisual(STRUCTURE_TYPE p_structureType)
	{
		_structurePlacementVisual.Show(p_structureType);
	}

	public void HideStructurePlacementVisual()
	{
		_structurePlacementVisual.Hide();
	}

	public void SetStructurePlacementVisualFollowMouseState(bool p_state)
	{
		_structurePlacementVisual.SetFollowMouseState(p_state);
	}

	public void SetStructurePlacementVisualHighlightColor(Color p_color)
	{
		_structurePlacementVisual.SetHighlightColor(p_color);
	}

	private void ConstructResonancePowerWeights()
	{
		_resonancePowerWeights = new WeightedDictionary<PLAYER_SKILL_TYPE>();
		foreach (KeyValuePair<PLAYER_SKILL_TYPE, SkillData> allPlayerSkillsDatum in PlayerSkillManager.Instance.allPlayerSkillsData)
		{
			if (allPlayerSkillsDatum.Value.category != PLAYER_SKILL_CATEGORY.AFFLICTION && allPlayerSkillsDatum.Value.category != PLAYER_SKILL_CATEGORY.PLAYER_ACTION && allPlayerSkillsDatum.Value.category != PLAYER_SKILL_CATEGORY.SPELL && allPlayerSkillsDatum.Value.category != PLAYER_SKILL_CATEGORY.DEMONIC_STRUCTURE)
			{
				continue;
			}
			PlayerSkillData scriptableObjPlayerSkillData = PlayerSkillManager.Instance.GetScriptableObjPlayerSkillData<PlayerSkillData>(allPlayerSkillsDatum.Value.type);
			if (scriptableObjPlayerSkillData != null)
			{
				int baseLoadoutWeight = scriptableObjPlayerSkillData.baseLoadoutWeight;
				if (baseLoadoutWeight > 0)
				{
					_resonancePowerWeights.AddElement(allPlayerSkillsDatum.Key, baseLoadoutWeight);
				}
			}
		}
	}

	public PLAYER_SKILL_TYPE GetRandomResonancePower(Character p_character)
	{
		return _resonancePowerWeights.PickRandomElementGivenWeights();
	}

	private void OnFactionBecameAwareOfPlayer(Faction p_faction)
	{
		FactionAwareAlert factionAwareAlert = TutorialManager.Instance.CreateGameAlert<FactionAwareAlert>(Game_Alert.Faction_Aware_Alert);
		factionAwareAlert.SetFaction(p_faction);
		factionAwareAlert.SetAsActive();
	}

	private void MummifiedReleasedByPlayer(Character p_character)
	{
		MummifiedReleaseAlert mummifiedReleaseAlert = TutorialManager.Instance.CreateGameAlert<MummifiedReleaseAlert>(Game_Alert.Mummified_Release_Alert);
		mummifiedReleaseAlert.SetCharacter(p_character);
		mummifiedReleaseAlert.SetAsActive();
	}

	public void OnLocaleChanged(Locale locale)
	{
		if (player != null && player.playerFaction != null)
		{
			player.playerFaction.SetName(LocalizationManager.Instance.GetLocalizedValue("Faction_Table", "Demons"));
		}
	}
}
