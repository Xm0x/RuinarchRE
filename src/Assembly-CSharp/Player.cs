using System;
using System.Collections.Generic;
using Crime_System;
using Inner_Maps;
using Inner_Maps.Location_Structures;
using Interrupts;
using Locations.Settlements;
using Maccima_Games.Util;
using Object_Pools;
using Ruinarch;
using Traits;
using UnityEngine;
using UtilityScripts;

public class Player : ILeader, ISavable, IObjectManipulator
{
	public Faction playerFaction { get; private set; }

	public PlayerSettlement playerSettlement { get; private set; }

	public List<IIntel> allIntel { get; private set; }

	public IIntel currentActiveIntel { get; private set; }

	public Area portalArea { get; private set; }

	public TILE_OBJECT_TYPE currentActiveItem { get; private set; }

	public SUMMON_TYPE currentActiveMonsterType { get; private set; }

	public bool isCurrentlyBuildingDemonicStructure { get; private set; }

	public IPlayerActionTarget currentlySelectedPlayerActionTarget { get; private set; }

	public List<string> charactersThatHaveReportedDemonicStructure { get; private set; }

	public SeizeComponent seizeComponent { get; }

	public ThreatComponent threatComponent { get; }

	public PlayerSkillComponent playerSkillComponent { get; }

	public CurrenciesComponent currenciesComponent { get; }

	public PlayerUnderlingsComponent underlingsComponent { get; private set; }

	public PlayerTileObjectComponent tileObjectComponent { get; private set; }

	public StoredTargetsComponent storedTargetsComponent { get; }

	public BookmarkComponent bookmarkComponent { get; }

	public SummonMeterComponent summonMeterComponent { get; private set; }

	public ManaRegenComponent manaRegenComponent { get; set; }

	public PlayerDamageAccumulator damageAccumulator { get; private set; }

	public PlayerRetaliationComponent retaliationComponent { get; private set; }

	public PlayerDevastationComponent devastationComponent { get; private set; }

	public GoalComponent goalComponent { get; private set; }

	public PrimordialPoolDataHandler primordialPoolDataHandler { get; private set; }

	public PartyStructureDataHandler partyStructureDataHandler { get; private set; }

	public bool hasAlreadyWon { get; set; }

	public int id => -645;

	public string name => "Player";

	public RACE race => RACE.HUMANS;

	public GENDER gender => GENDER.MALE;

	public Region currentRegion => null;

	public Region homeRegion => null;

	public string persistentID => string.Empty;

	public OBJECT_TYPE objectType => OBJECT_TYPE.Player;

	public Type serializedData => typeof(SaveDataPlayer);

	public int chaoticEnergy => currenciesComponent.chaoticEnergy;

	public SkillData currentActivePlayerSpell { get; private set; }

	public ARTIFACT_TYPE currentActiveArtifact { get; private set; }

	public Player()
	{
		allIntel = new List<IIntel>();
		charactersThatHaveReportedDemonicStructure = new List<string>();
		currentActiveItem = TILE_OBJECT_TYPE.NONE;
		primordialPoolDataHandler = new PrimordialPoolDataHandler();
		partyStructureDataHandler = new PartyStructureDataHandler();
		seizeComponent = new SeizeComponent();
		threatComponent = new ThreatComponent(this);
		playerSkillComponent = new PlayerSkillComponent();
		currenciesComponent = new CurrenciesComponent();
		underlingsComponent = new PlayerUnderlingsComponent();
		storedTargetsComponent = new StoredTargetsComponent();
		manaRegenComponent = new ManaRegenComponent(this);
		tileObjectComponent = new PlayerTileObjectComponent();
		summonMeterComponent = new SummonMeterComponent();
		bookmarkComponent = new BookmarkComponent();
		damageAccumulator = new PlayerDamageAccumulator();
		retaliationComponent = new PlayerRetaliationComponent();
		devastationComponent = new PlayerDevastationComponent();
		goalComponent = new GoalComponent();
		goalComponent.Initialize();
		summonMeterComponent.Initialize();
		hasAlreadyWon = false;
		if (WorldSettings.Instance.worldSettingsData.IsRetaliationAllowed())
		{
			bookmarkComponent.AddBookmark(retaliationComponent.retaliationProgress, BOOKMARK_CATEGORY.Major_Events);
		}
		SubscribeListeners();
	}

	public Player(SaveDataPlayerGame data)
	{
		allIntel = new List<IIntel>();
		seizeComponent = data.seizeComponent.Load();
		threatComponent = data.threatComponent.Load();
		playerSkillComponent = data.playerSkillComponent.Load();
		underlingsComponent = data.underlingsComponent.Load();
		tileObjectComponent = data.tileObjectComponent.Load();
		summonMeterComponent = data.summonMeterComponent.Load();
		damageAccumulator = data.damageAccumulator.Load();
		retaliationComponent = data.retaliationComponent.Load();
		devastationComponent = data.devastationComponent.Load();
		primordialPoolDataHandler = data.primordialPoolDataHandler.Load();
		partyStructureDataHandler = data.partyStructureDataHandler.Load();
		bookmarkComponent = new BookmarkComponent();
		currenciesComponent = new CurrenciesComponent(data.currenciesComponent);
		threatComponent.SetPlayer(this);
		currentActiveItem = TILE_OBJECT_TYPE.NONE;
		storedTargetsComponent = new StoredTargetsComponent();
		manaRegenComponent = new ManaRegenComponent(this, data.manaRegenComponent);
		summonMeterComponent.Initialize();
		goalComponent = data.goalComponent.Load();
		hasAlreadyWon = data.hasAlreadyWon;
		charactersThatHaveReportedDemonicStructure = new List<string>(data.charactersThatHaveReportedDemonicStructure);
	}

	public void LoadPlayerData(SaveDataPlayer save)
	{
		if (save != null)
		{
			playerSkillComponent.LoadPlayerSkillTreeOrLoadout(save);
		}
	}

	public void SetPortalTile(Area tile)
	{
		portalArea = tile;
	}

	private void SubscribeListeners()
	{
		Messenger.AddListener<Character, Faction>(FactionSignals.CHARACTER_ADDED_TO_FACTION, OnCharacterAddedToFaction);
		Messenger.AddListener<Character, Faction>(FactionSignals.CHARACTER_REMOVED_FROM_FACTION, OnCharacterRemovedFromFaction);
		Messenger.AddListener<Character>(CharacterSignals.CHARACTER_DEATH, OnCharacterDied);
		Messenger.AddListener(Signals.TICK_ENDED, OnTickEnded);
		Messenger.AddListener<Character>(CharacterSignals.CHARACTER_CHANGED_NAME, OnCharacterChangedName);
		Messenger.AddListener<Character>(CharacterSignals.DISCONNECT_FROM_CHARACTER, DisconnectFromCharacter);
		underlingsComponent.SubscribeListeners();
		bookmarkComponent.SubscribeListeners();
		storedTargetsComponent.SubscribeListeners();
		currenciesComponent.SubscribeListeners();
	}

	private void OnCharacterChangedName(Character p_character)
	{
		for (int i = 0; i < allIntel.Count; i++)
		{
			IIntel intel = allIntel[i];
			if (intel is ActionIntel actionIntel)
			{
				if (actionIntel.node.isAssumption)
				{
					if (actionIntel.node.assumption.assumptionLog.TryUpdateLogAfterRename(p_character))
					{
						Messenger.Broadcast(UISignals.INTEL_LOG_UPDATED, intel);
					}
				}
				else if (actionIntel.node.descriptionLog.TryUpdateLogAfterRename(p_character))
				{
					Messenger.Broadcast(UISignals.INTEL_LOG_UPDATED, intel);
				}
			}
			else if (intel is InterruptIntel interruptIntel && interruptIntel.interruptHolder.effectLog.TryUpdateLogAfterRename(p_character))
			{
				Messenger.Broadcast(UISignals.INTEL_LOG_UPDATED, intel);
			}
		}
	}

	private void DisconnectFromCharacter(Character p_character)
	{
		if (allIntel.Count <= 0)
		{
			return;
		}
		List<IIntel> list = RuinarchListPool<IIntel>.Claim(allIntel.Count);
		list.AddRange(allIntel);
		bool flag = false;
		for (int i = 0; i < list.Count; i++)
		{
			IIntel intel = list[i];
			if (intel is ActionIntel actionIntel)
			{
				bool flag2 = false;
				actionIntel.node.DisconnectFromCharacter(p_character);
				if (actionIntel.node.IsNodeObjectInvalid() || actionIntel.node.IsCharacterReferenced(p_character))
				{
					flag2 = true;
				}
				if (flag2)
				{
					flag = true;
					RemoveIntel(intel);
				}
			}
			else if (intel is InterruptIntel interruptIntel)
			{
				interruptIntel.interruptHolder.DisconnectFromCharacter(p_character);
				if (interruptIntel.interruptHolder.IsImportantDataNull() || interruptIntel.interruptHolder.IsCharacterReferenced(p_character))
				{
					flag = true;
					RemoveIntel(intel);
				}
			}
		}
		RuinarchListPool<IIntel>.Release(list);
		if (flag)
		{
			Dictionary<string, string> dictionary = MaccimaDictionaryPool<string, string>.Claim();
			dictionary.Add("name", p_character.name);
			PopUpNotificationUI.Instance.ShowPlayerPoppingTextNotif(Utilities.YellowDotIcon() + LocalizationManager.Instance.GetLocalizedValue("OtherUI_Table", "Lost_Intel", dictionary), 5, PlayerUI.Instance.popUpDisplayPoint);
			MaccimaDictionaryPool<string, string>.Release(dictionary);
		}
	}

	public void SetPlayerArea(PlayerSettlement npcSettlement)
	{
		playerSettlement = npcSettlement;
	}

	public void CreatePlayerFaction()
	{
		Faction faction = FactionManager.Instance.CreateNewFaction(FACTION_TYPE.Demons, LocalizationManager.Instance.GetLocalizedValue("Faction_Table", "Demons"));
		faction.SetLeader(this);
		SetPlayerFaction(faction);
	}

	private void SetPlayerFaction(Faction faction)
	{
		playerFaction = faction;
	}

	public int GetNumberOfAliveMonstersInPlayerFaction(SUMMON_TYPE p_type)
	{
		int num = 0;
		for (int i = 0; i < PlayerManager.Instance.player.playerFaction.characters.Count; i++)
		{
			Character character = PlayerManager.Instance.player.playerFaction.characters[i];
			if (!character.isDead && character is Summon summon && summon.summonType == p_type)
			{
				num++;
			}
		}
		return num;
	}

	public int GetNumberOfAliveMonstersInPlayerFaction(MINION_TYPE p_type)
	{
		int num = 0;
		for (int i = 0; i < PlayerManager.Instance.player.playerFaction.characters.Count; i++)
		{
			Character character = PlayerManager.Instance.player.playerFaction.characters[i];
			if (!character.isDead && character.minion != null && character.minion.minionType == p_type)
			{
				num++;
			}
		}
		return num;
	}

	public void SetCurrentlyActivePlayerSpell(SkillData action)
	{
		if (currentActivePlayerSpell == action)
		{
			return;
		}
		if (action == null && currentActivePlayerSpell is DecorationsData { chosenTileObjectType: not TILE_OBJECT_TYPE.NONE })
		{
			TileObjectTypeItem currentlyChosenNameplate = PlayerUI.Instance.decorationsUI.currentlyChosenNameplate;
			if (currentlyChosenNameplate != null)
			{
				currentlyChosenNameplate.toggle.isOn = false;
			}
			UIManager.Instance.SetTempDisableShowInfoUI(state: false);
			return;
		}
		SkillData skillData = currentActivePlayerSpell;
		currentActivePlayerSpell = action;
		if (currentActivePlayerSpell == null)
		{
			skillData.OnNoLongerCurrentActiveSpell();
			PlayerManager.Instance.RemovePlayerInputModule(PlayerManager.spellInputModule);
			UIManager.Instance.SetTempDisableShowInfoUI(state: false);
			Messenger.RemoveListener<SHORTCUT_ACTION>(ControlsSignals.PLAYER_INPUT_ACTION, OnReceivePlayerInputActionForSpell);
			InputManager.Instance.SetCursorTo(Cursor_Type.Default);
			skillData.UnhighlightAffectedTiles();
			UIManager.Instance.HideSmallInfo();
			Messenger.Broadcast(PlayerSkillSignals.PLAYER_NO_ACTIVE_SPELL, skillData);
		}
		else
		{
			action.OnSetAsCurrentActiveSpell();
			PlayerManager.Instance.AddPlayerInputModule(PlayerManager.spellInputModule);
			InputManager.Instance.SetCursorTo(Cursor_Type.Cross);
			Messenger.AddListener<SHORTCUT_ACTION>(ControlsSignals.PLAYER_INPUT_ACTION, OnReceivePlayerInputActionForSpell);
			Messenger.Broadcast(PlayerSkillSignals.PLAYER_SET_ACTIVE_SPELL, currentActivePlayerSpell);
		}
	}

	private void OnReceivePlayerInputActionForSpell(SHORTCUT_ACTION p_action)
	{
		if (p_action == SHORTCUT_ACTION.Left_Click)
		{
			TryExecuteCurrentActiveSpell();
		}
	}

	private void TryExecuteCurrentActiveSpell()
	{
		if (UIManager.Instance.IsMouseOnUI() || !InnerMapManager.Instance.isAnInnerMapShowing)
		{
			return;
		}
		bool flag = false;
		if (currentActivePlayerSpell is DecorationsData { chosenTileObjectType: TILE_OBJECT_TYPE.NONE })
		{
			return;
		}
		for (int i = 0; i < currentActivePlayerSpell.targetTypes.Length; i++)
		{
			LocationGridTile locationGridTile = null;
			switch (currentActivePlayerSpell.targetTypes[i])
			{
			case SPELL_TARGET.BASE_BUILDING:
				locationGridTile = InnerMapManager.Instance.GetTileFromMousePosition();
				if (locationGridTile != null)
				{
					UIManager.Instance.SetTempDisableShowInfoUI(state: true);
				}
				break;
			case SPELL_TARGET.CHARACTER:
				if (InnerMapManager.Instance.currentlyShowingMap != null && InnerMapManager.Instance.currentlyHoveredPoi is Character)
				{
					if (currentActivePlayerSpell.CanPerformAbilityTowards(InnerMapManager.Instance.currentlyHoveredPoi))
					{
						currentActivePlayerSpell.ActivateAbility(InnerMapManager.Instance.currentlyHoveredPoi);
						flag = true;
					}
					UIManager.Instance.SetTempDisableShowInfoUI(state: true);
				}
				break;
			case SPELL_TARGET.TILE_OBJECT:
				if (InnerMapManager.Instance.currentlyHoveredPoi is TileObject)
				{
					if (currentActivePlayerSpell.CanPerformAbilityTowards(InnerMapManager.Instance.currentlyHoveredPoi))
					{
						currentActivePlayerSpell.ActivateAbility(InnerMapManager.Instance.currentlyHoveredPoi);
						flag = true;
					}
					UIManager.Instance.SetTempDisableShowInfoUI(state: true);
				}
				break;
			case SPELL_TARGET.TILE:
				locationGridTile = InnerMapManager.Instance.GetTileFromMousePosition();
				if (locationGridTile != null)
				{
					if (currentActivePlayerSpell.CanPerformAbilityTowards(locationGridTile, out var o_cannotPerformReason))
					{
						currentActivePlayerSpell.ActivateAbility(locationGridTile);
						flag = true;
					}
					else if (!string.IsNullOrEmpty(o_cannotPerformReason))
					{
						InnerMapManager.Instance.ShowAreaMapTextPopup(o_cannotPerformReason, locationGridTile.centeredWorldLocation, Color.white);
					}
					UIManager.Instance.SetTempDisableShowInfoUI(state: true);
				}
				break;
			case SPELL_TARGET.AREA:
				locationGridTile = InnerMapManager.Instance.GetTileFromMousePosition();
				if (locationGridTile != null)
				{
					if (currentActivePlayerSpell.CanPerformAbilityTowards(locationGridTile.area))
					{
						currentActivePlayerSpell.ActivateAbility(locationGridTile.area);
						flag = true;
					}
					UIManager.Instance.SetTempDisableShowInfoUI(state: true);
				}
				break;
			case SPELL_TARGET.SETTLEMENT:
			{
				locationGridTile = InnerMapManager.Instance.GetTileFromMousePosition();
				BaseSettlement settlement = null;
				if (locationGridTile != null && locationGridTile.IsPartOfSettlement(out settlement))
				{
					if (currentActivePlayerSpell.CanPerformAbilityTowards(settlement))
					{
						currentActivePlayerSpell.ActivateAbility(settlement);
						flag = true;
					}
					UIManager.Instance.SetTempDisableShowInfoUI(state: true);
				}
				break;
			}
			}
			if (flag)
			{
				break;
			}
		}
		InputManager.Instance.SetCursorTo(Cursor_Type.Default);
		if (currentActivePlayerSpell == null)
		{
			return;
		}
		if (currentActivePlayerSpell is SummonPlayerSkill summonPlayerSkill)
		{
			if (!underlingsComponent.HasMonsterUnderlingCharge(summonPlayerSkill.summonType) || !underlingsComponent.CanStillSpawnPlayerDefenders())
			{
				SetCurrentlyActivePlayerSpell(null);
			}
		}
		else if (!currentActivePlayerSpell.CanPerformAbility() || !currentActivePlayerSpell.IsValid() || (currentActivePlayerSpell is DemonicStructurePlayerSkill && flag))
		{
			SetCurrentlyActivePlayerSpell(null);
		}
	}

	public bool IsCurrentActiveSpell(PLAYER_SKILL_TYPE p_skillType)
	{
		if (currentActivePlayerSpell != null)
		{
			return currentActivePlayerSpell.type == p_skillType;
		}
		return false;
	}

	public void AddIntel(IIntel newIntel)
	{
		if (!allIntel.Contains(newIntel))
		{
			allIntel.Add(newIntel);
			if (allIntel.Count > 5)
			{
				RemoveIntel(allIntel[0]);
			}
			Messenger.Broadcast(PlayerSignals.PLAYER_OBTAINED_INTEL, newIntel);
		}
	}

	public void RemoveIntel(IIntel intel)
	{
		if (allIntel.Remove(intel))
		{
			Messenger.Broadcast(PlayerSignals.PLAYER_REMOVED_INTEL, intel);
			intel.OnIntelRemoved();
		}
	}

	public void SetCurrentActiveIntel(IIntel intel)
	{
		if (currentActiveIntel != intel)
		{
			IIntel intel2 = currentActiveIntel;
			currentActiveIntel = intel;
			if (intel2 != null)
			{
				PlayerUI.Instance.GetIntelItemWithIntel(intel2)?.SetClickedState(isClicked: false);
				Messenger.RemoveListener<SHORTCUT_ACTION>(ControlsSignals.PLAYER_INPUT_ACTION, OnReceivePlayerInputActionForIntel);
				InputManager.Instance.SetCursorTo(Cursor_Type.Default);
			}
			if (currentActiveIntel != null)
			{
				PlayerManager.Instance.AddPlayerInputModule(PlayerManager.intelInputModule);
				Messenger.Broadcast(PlayerSignals.ACTIVE_INTEL_SET, currentActiveIntel);
				PlayerUI.Instance.GetIntelItemWithIntel(currentActiveIntel)?.SetClickedState(isClicked: true);
				InputManager.Instance.SetCursorTo(Cursor_Type.Cross);
				Messenger.AddListener<SHORTCUT_ACTION>(ControlsSignals.PLAYER_INPUT_ACTION, OnReceivePlayerInputActionForIntel);
			}
			else
			{
				PlayerManager.Instance.RemovePlayerInputModule(PlayerManager.intelInputModule);
				Messenger.Broadcast(PlayerSignals.ACTIVE_INTEL_REMOVED);
			}
		}
	}

	private void OnReceivePlayerInputActionForIntel(SHORTCUT_ACTION p_action)
	{
		if (p_action == SHORTCUT_ACTION.Left_Click)
		{
			TryExecuteShareIntel(InnerMapManager.Instance.currentlyHoveredPoi, currentActiveIntel);
		}
	}

	public void TryExecuteShareIntel(IPointOfInterest p_target, IIntel p_intel)
	{
		string hoverText = string.Empty;
		if (!CanShareIntelTo(p_target, ref hoverText, p_intel))
		{
			return;
		}
		Character character = p_target as Character;
		List<ConversationData> list = RuinarchListPool<ConversationData>.Claim(3);
		string text = (character.traitContainer.HasTrait("Demon Cultist") ? LocalizationManager.Instance.GetLocalizedValue("UIStrings_Table", "Cultist_Conversation") : LocalizationManager.Instance.GetLocalizedValue("UIStrings_Table", "Default_Conversation"));
		ConversationData conversationData = ObjectPoolManager.Instance.CreateNewConversationData(text, character, DialogItem.Position.Left);
		ConversationData conversationData2 = ObjectPoolManager.Instance.CreateNewConversationData(p_intel.log.logText, null, DialogItem.Position.Right);
		string text2 = character.reactionComponent.ReactToIntel(p_intel);
		string text3 = string.Empty;
		if (p_intel.actor == character)
		{
			text3 = LocalizationManager.Instance.GetLocalizedValue("UIStrings_Table", "Known_Action");
			goto IL_0319;
		}
		if (p_intel is ActionIntel actionIntel)
		{
			ActualGoapNode node = actionIntel.node;
			if ((object)node != null && node.action.goapType == INTERACTION_TYPE.MAKE_LOVE)
			{
				Obsessed traitOrStatus = character.traitContainer.GetTraitOrStatus<Obsessed>("Obsessed");
				if (traitOrStatus != null)
				{
					if (traitOrStatus.targetCharacter == actionIntel.node.actor)
					{
						text3 = FormulateMakeLoveObsessedText(character, traitOrStatus.targetCharacter, actionIntel.node.target as Character);
					}
					else if (traitOrStatus.targetCharacter == actionIntel.node.target)
					{
						text3 = FormulateMakeLoveObsessedText(character, traitOrStatus.targetCharacter, actionIntel.node.actor);
					}
				}
				goto IL_022a;
			}
		}
		if (p_intel is InterruptIntel interruptIntel)
		{
			InterruptHolder interruptHolder = interruptIntel.interruptHolder;
			if (interruptHolder != null && interruptHolder.interrupt.type == INTERRUPT.Flirt)
			{
				Obsessed traitOrStatus2 = character.traitContainer.GetTraitOrStatus<Obsessed>("Obsessed");
				if (traitOrStatus2 != null)
				{
					if (traitOrStatus2.targetCharacter == interruptIntel.interruptHolder.actor)
					{
						text3 = FormulateFlirtObsessedText(character, traitOrStatus2.targetCharacter, interruptIntel.interruptHolder.target as Character);
					}
					else if (traitOrStatus2.targetCharacter == interruptIntel.interruptHolder.target)
					{
						text3 = FormulateFlirtObsessedText(character, traitOrStatus2.targetCharacter, interruptIntel.interruptHolder.actor);
					}
				}
			}
		}
		goto IL_022a;
		IL_022a:
		if (string.IsNullOrEmpty(text3))
		{
			if (text2.HasEmotion())
			{
				text3 = Utilities.FormulateTextFromEmotions(text2, p_intel.actor, p_intel.target, character);
			}
			else
			{
				Dictionary<string, string> dictionary = MaccimaDictionaryPool<string, string>.Claim();
				dictionary.Add("criminalName", p_intel.actor.name);
				CRIME_SEVERITY crimeSeverity = CrimeManager.Instance.GetCrimeSeverity(character, p_intel.actor, p_intel.target, p_intel.reactable.crimeType);
				CrimeSeverity crimeSeverity2 = CrimeManager.Instance.GetCrimeSeverity(crimeSeverity);
				if (crimeSeverity2 != null)
				{
					dictionary.Add("crimeType", crimeSeverity2.localizedName);
				}
				dictionary.Add("criminalObjectivePronoun", Utilities.GetPronounString(p_intel.actor.gender, PRONOUN_TYPE.OBJECTIVE, isUppercaseFirstLetter: false));
				dictionary.Add("criminalSubjectivePronoun", Utilities.GetPronounString(p_intel.actor.gender, PRONOUN_TYPE.SUBJECTIVE, isUppercaseFirstLetter: false));
				text3 = LocalizationManager.Instance.GetLocalizedValue("ShareIntel_Table", text2, dictionary);
				MaccimaDictionaryPool<string, string>.Release(dictionary);
			}
		}
		goto IL_0319;
		IL_0319:
		ConversationData conversationData3 = ObjectPoolManager.Instance.CreateNewConversationData(text3, character, DialogItem.Position.Left);
		list.Add(conversationData);
		list.Add(conversationData2);
		list.Add(conversationData3);
		UIManager.Instance.OpenConversationMenu(list, LocalizationManager.Instance.GetLocalizedValue("UIStrings_Table", "Share_Intel_Title") + " " + character.name);
		AudioManager.Instance.PlayConversationMenuOpenedSFX();
		if (p_intel.IsIntelConsideredACrimeByTarget(character))
		{
			PlayerManager.Instance.player.goalComponent.CompleteSubGoal(SUB_GOAL.GOAL_SHARE_CRIME_INTEL);
		}
		Messenger.Broadcast(UISignals.ON_SHARE_INTEL);
		RemoveIntel(p_intel);
		if (p_intel == currentActiveIntel)
		{
			SetCurrentActiveIntel(null);
		}
		ObjectPoolManager.Instance.ReturnConversationDataToPool(conversationData);
		ObjectPoolManager.Instance.ReturnConversationDataToPool(conversationData2);
		ObjectPoolManager.Instance.ReturnConversationDataToPool(conversationData3);
		RuinarchListPool<ConversationData>.Release(list);
	}

	public bool CanShareIntelTo(IPointOfInterest poi, ref string hoverText, IIntel p_intel)
	{
		if (poi is Character character)
		{
			if (!character.isNormalCharacter)
			{
				return false;
			}
			hoverText = string.Empty;
			if (character.traitContainer.HasTrait("Catatonic"))
			{
				hoverText = LocalizationManager.Instance.GetLocalizedValue("UIStrings_Table", "Catatonic_Share_Intel");
				return false;
			}
			if (character.traitContainer.HasTrait("Resting"))
			{
				hoverText = LocalizationManager.Instance.GetLocalizedValue("UIStrings_Table", "Sleeping_Share_Intel");
				return false;
			}
			if (!character.limiterComponent.canWitness)
			{
				return false;
			}
			if (!p_intel.CanShareIntelTo(character))
			{
				return false;
			}
			if (!character.faction.isPlayerFaction && !GameUtilities.IsRaceBeast(character.race))
			{
				return true;
			}
		}
		return false;
	}

	public bool CanShareIntelTo(IPointOfInterest poi, IIntel p_intel)
	{
		if (poi is Character character)
		{
			if (!character.isNormalCharacter)
			{
				return false;
			}
			if (character.traitContainer.HasTrait("Catatonic"))
			{
				return false;
			}
			if (character.traitContainer.HasTrait("Resting"))
			{
				return false;
			}
			if (!character.limiterComponent.canWitness)
			{
				return false;
			}
			if (!p_intel.CanShareIntelTo(character))
			{
				return false;
			}
			if (!character.faction.isPlayerFaction && !GameUtilities.IsRaceBeast(character.race))
			{
				return true;
			}
		}
		return false;
	}

	public bool HasIsImprisonedIntel(Character p_hostage)
	{
		for (int i = 0; i < allIntel.Count; i++)
		{
			if (allIntel[i] is ActionIntel { reactable: ActualGoapNode reactable } && reactable.actor == p_hostage && reactable.goapType == INTERACTION_TYPE.IS_IMPRISONED)
			{
				return true;
			}
		}
		return false;
	}

	public bool HasIsCaptiveIntel(Character p_hostage)
	{
		for (int i = 0; i < allIntel.Count; i++)
		{
			if (allIntel[i] is ActionIntel { reactable: ActualGoapNode reactable } && reactable.actor == p_hostage && reactable.goapType == INTERACTION_TYPE.IS_CAPTIVE)
			{
				return true;
			}
		}
		return false;
	}

	private string FormulateFlirtObsessedText(Character p_sourceOfObsession, Character p_targetOfObsession, Character p_thirdPartyCharacter)
	{
		Dictionary<string, string> dictionary = MaccimaDictionaryPool<string, string>.Claim();
		dictionary.Add("thirdPartyCharacter", p_thirdPartyCharacter.visuals.GetCharacterNameWithIconAndColor());
		dictionary.Add("targetOfObsession", p_targetOfObsession.visuals.GetCharacterNameWithIconAndColor());
		dictionary.Add("thirdPartyPronoun", Utilities.GetPronounString(p_thirdPartyCharacter.gender, PRONOUN_TYPE.SUBJECTIVE, isUppercaseFirstLetter: true));
		string localizedValue = LocalizationManager.Instance.GetLocalizedValue("UIStrings_Table", "Intel_Flirt_Obsessed_Conversation", dictionary);
		MaccimaDictionaryPool<string, string>.Release(dictionary);
		return localizedValue;
	}

	private string FormulateMakeLoveObsessedText(Character p_sourceOfObsession, Character p_targetOfObsession, Character p_thirdPartyCharacter)
	{
		Dictionary<string, string> dictionary = MaccimaDictionaryPool<string, string>.Claim();
		dictionary.Add("thirdPartyCharacter", p_thirdPartyCharacter.visuals.GetCharacterNameWithIconAndColor());
		string localizedValue = LocalizationManager.Instance.GetLocalizedValue("UIStrings_Table", "Intel_Make_Love_Obsessed_Conversation", dictionary);
		MaccimaDictionaryPool<string, string>.Release(dictionary);
		return localizedValue;
	}

	private bool ShouldShowNotificationFrom(LocationGridTile location)
	{
		if (!ConsoleBase.alwaysShowNotifications)
		{
			return location?.tileObjectComponent.isSeenByEyeWard ?? false;
		}
		return true;
	}

	public bool ShouldShowNotificationFrom(Character character)
	{
		return ShouldShowNotificationFrom(character.gridTileLocation);
	}

	private bool ShouldShowNotificationFrom(Character character, in Log log)
	{
		if (ShouldShowNotificationFrom(character))
		{
			return true;
		}
		for (int i = 0; i < log.fillers.Count; i++)
		{
			object objectForFiller = log.fillers[i].GetObjectForFiller();
			if (objectForFiller is Character character2)
			{
				if (ShouldShowNotificationFrom(character2))
				{
					return true;
				}
			}
			else if (objectForFiller is LocationGridTile location && ShouldShowNotificationFrom(location))
			{
				return true;
			}
		}
		return false;
	}

	private bool ShouldShowNotificationFrom(LocationGridTile location, in Log log)
	{
		if (ShouldShowNotificationFrom(location))
		{
			return true;
		}
		for (int i = 0; i < log.fillers.Count; i++)
		{
			object objectForFiller = log.fillers[i].GetObjectForFiller();
			if (objectForFiller is Character character)
			{
				if (ShouldShowNotificationFrom(character))
				{
					return true;
				}
			}
			else if (objectForFiller is LocationGridTile location2 && ShouldShowNotificationFrom(location2))
			{
				return true;
			}
		}
		return false;
	}

	public bool ShowNotificationFrom(LocationGridTile location, Log log, bool releaseLogAfter = false)
	{
		if (ShouldShowNotificationFrom(location, in log))
		{
			ShowNotification(log, releaseLogAfter);
			return true;
		}
		return false;
	}

	public bool ShowNotificationFrom(Character character, Log log, bool releaseLogAfter = false)
	{
		if (ShouldShowNotificationFrom(character, in log))
		{
			ShowNotification(log, releaseLogAfter);
			return true;
		}
		return false;
	}

	public void ShowNotificationFrom(Character character, IIntel intel)
	{
		ShowNotification(intel);
	}

	public void ShowNotificationFromPlayer(Log log, bool releaseLogAfter = false)
	{
		ShowNotification(log, releaseLogAfter);
	}

	public void ShowNotificationFromPlayer(IIntel intel)
	{
		ShowNotification(intel);
	}

	private void ShowNotification(Log log, bool releaseLogAfter = false)
	{
		Messenger.Broadcast(UISignals.SHOW_PLAYER_NOTIFICATION, log);
		if (releaseLogAfter)
		{
			LogPool.Release(log);
		}
	}

	private void ShowNotification(IIntel intel)
	{
		Messenger.Broadcast(UISignals.SHOW_INTEL_NOTIFICATION, intel);
	}

	public void SetCurrentlyActiveArtifact(ARTIFACT_TYPE artifact)
	{
		if (currentActiveArtifact != artifact)
		{
			ARTIFACT_TYPE arg = currentActiveArtifact;
			currentActiveArtifact = artifact;
			if (currentActiveArtifact == ARTIFACT_TYPE.None)
			{
				Messenger.RemoveListener<SHORTCUT_ACTION>(ControlsSignals.PLAYER_INPUT_ACTION, OnReceivePlayerInputActionForArtifact);
				InputManager.Instance.SetCursorTo(Cursor_Type.Default);
				Messenger.Broadcast(PlayerSignals.PLAYER_NO_ACTIVE_ARTIFACT, arg);
			}
			else
			{
				InputManager.Instance.SetCursorTo(Cursor_Type.Check);
				Messenger.AddListener<SHORTCUT_ACTION>(ControlsSignals.PLAYER_INPUT_ACTION, OnReceivePlayerInputActionForArtifact);
			}
		}
	}

	private void OnReceivePlayerInputActionForArtifact(SHORTCUT_ACTION p_action)
	{
		if (p_action == SHORTCUT_ACTION.Left_Click)
		{
			TrySpawnArtifact();
		}
	}

	private void TrySpawnArtifact()
	{
		if (!UIManager.Instance.IsMouseOnUI() && InnerMapManager.Instance.isAnInnerMapShowing)
		{
			LocationGridTile tileFromMousePosition = InnerMapManager.Instance.GetTileFromMousePosition();
			if (tileFromMousePosition != null && tileFromMousePosition.tileObjectComponent.objHere == null)
			{
				Artifact poi = InnerMapManager.Instance.CreateNewArtifact(currentActiveArtifact);
				tileFromMousePosition.structure.AddPOI(poi, tileFromMousePosition);
			}
		}
	}

	public bool IsPerformingPlayerAction()
	{
		SkillData skillData = PlayerManager.Instance.player.currentActivePlayerSpell;
		bool flag = skillData != null;
		if (flag && skillData is DecorationsData decorationsData)
		{
			flag = decorationsData.chosenTileObjectType != TILE_OBJECT_TYPE.NONE;
		}
		if (!flag && !PlayerManager.Instance.player.seizeComponent.hasSeizedPOI && PlayerManager.Instance.player.currentActiveIntel == null && PlayerManager.Instance.player.currentActiveItem == TILE_OBJECT_TYPE.NONE)
		{
			return PlayerManager.Instance.player.currentActiveArtifact != ARTIFACT_TYPE.None;
		}
		return true;
	}

	public void SetCurrentPlayerActionTarget(IPlayerActionTarget p_target)
	{
		currentlySelectedPlayerActionTarget = p_target;
	}

	private void OnTickEnded()
	{
		devastationComponent.OnTickEnded();
	}

	public void InitializeAfterLoadoutPicked()
	{
		goalComponent.InitializeAfterLoadoutPicked();
		if (!SaveManager.Instance.useSaveData)
		{
			ThePortal thePortal = playerSettlement.GetFirstStructureOfType(STRUCTURE_TYPE.THE_PORTAL) as ThePortal;
			if (WorldSettings.Instance.worldSettingsData.playerSkillSettings.omnipotentMode == OMNIPOTENT_MODE.Enabled)
			{
				playerSkillComponent.UpdateBuildSkillChargesBasedOnCurrentPortalLevel(thePortal.level);
				playerSkillComponent.UpdateDefenderSlotsBasedOnCurrentPortalLevel(thePortal.level);
			}
			else if (WorldSettings.Instance.worldSettingsData.victoryCondition == VICTORY_CONDITION.Eradication)
			{
				playerSkillComponent.UpdateBuildSkillChargesBasedOnCurrentPortalLevel(7);
				playerSkillComponent.UpdateDefenderSlotsBasedOnCurrentPortalLevel(7);
			}
			thePortal.GainPowersFromStartingLevel();
		}
		else
		{
			PlayerManager.Instance.player.playerSkillComponent.OnLoadSaveData();
		}
	}

	private void OnCharacterAddedToFaction(Character character, Faction faction)
	{
		if (faction == playerFaction)
		{
			underlingsComponent.OnCharacterAddedToPlayerFaction(character);
		}
	}

	private void OnCharacterRemovedFromFaction(Character character, Faction faction)
	{
		if (faction == playerFaction)
		{
			underlingsComponent.OnCharacterRemovedFromPlayerFaction(character);
		}
	}

	private void OnCharacterDied(Character p_character)
	{
		if (p_character.faction == playerFaction)
		{
			underlingsComponent.OnFactionMemberDied(p_character);
		}
		retaliationComponent.OnCharacterDeath(p_character);
	}

	public void SetCurrentlyActiveItem(TILE_OBJECT_TYPE item)
	{
		if (currentActiveItem != item)
		{
			TILE_OBJECT_TYPE arg = currentActiveItem;
			currentActiveItem = item;
			if (currentActiveItem == TILE_OBJECT_TYPE.NONE)
			{
				Messenger.RemoveListener<SHORTCUT_ACTION>(ControlsSignals.PLAYER_INPUT_ACTION, OnReceivePlayerInputActionForItem);
				InputManager.Instance.SetCursorTo(Cursor_Type.Default);
				Messenger.Broadcast(PlayerSignals.PLAYER_NO_ACTIVE_ITEM, arg);
			}
			else
			{
				InputManager.Instance.SetCursorTo(Cursor_Type.Check);
				Messenger.AddListener<SHORTCUT_ACTION>(ControlsSignals.PLAYER_INPUT_ACTION, OnReceivePlayerInputActionForItem);
			}
		}
	}

	private void OnReceivePlayerInputActionForItem(SHORTCUT_ACTION p_action)
	{
		if (p_action == SHORTCUT_ACTION.Left_Click)
		{
			TrySpawnItem();
		}
	}

	private void TrySpawnItem()
	{
		if (UIManager.Instance.IsMouseOnUI() || !InnerMapManager.Instance.isAnInnerMapShowing)
		{
			return;
		}
		LocationGridTile tileFromMousePosition = InnerMapManager.Instance.GetTileFromMousePosition();
		if (tileFromMousePosition != null && tileFromMousePosition.tileObjectComponent.objHere == null)
		{
			TileObject tileObject = InnerMapManager.Instance.CreateNewTileObject<TileObject>(currentActiveItem);
			tileFromMousePosition.structure.AddPOI(tileObject, tileFromMousePosition);
			if (tileObject is EquipmentItem equipmentItem)
			{
				equipmentItem.TryAddRandomPrefix();
			}
		}
	}

	public void SetCurrentlyActiveMonster(SUMMON_TYPE p_monsterType)
	{
		if (currentActiveMonsterType != p_monsterType)
		{
			SUMMON_TYPE arg = currentActiveMonsterType;
			currentActiveMonsterType = p_monsterType;
			if (currentActiveMonsterType == SUMMON_TYPE.None)
			{
				Messenger.RemoveListener<SHORTCUT_ACTION>(ControlsSignals.PLAYER_INPUT_ACTION, OnReceivePlayerInputActionForMonster);
				InputManager.Instance.SetCursorTo(Cursor_Type.Default);
				Messenger.Broadcast(PlayerSignals.PLAYER_NO_ACTIVE_MONSTER, arg);
			}
			else
			{
				InputManager.Instance.SetCursorTo(Cursor_Type.Check);
				Messenger.AddListener<SHORTCUT_ACTION>(ControlsSignals.PLAYER_INPUT_ACTION, OnReceivePlayerInputActionForMonster);
			}
		}
	}

	private void OnReceivePlayerInputActionForMonster(SHORTCUT_ACTION p_action)
	{
		if (p_action == SHORTCUT_ACTION.Left_Click)
		{
			TrySpawnMonster();
		}
	}

	private void TrySpawnMonster()
	{
		if (UIManager.Instance.IsMouseOnUI() || !InnerMapManager.Instance.isAnInnerMapShowing)
		{
			return;
		}
		LocationGridTile tileFromMousePosition = InnerMapManager.Instance.GetTileFromMousePosition();
		if (tileFromMousePosition != null)
		{
			Summon summon = CharacterManager.Instance.CreateNewSummon(currentActiveMonsterType, FactionManager.Instance.GetDefaultFactionForMonster(currentActiveMonsterType), null, tileFromMousePosition.parentMap.region);
			CharacterManager.Instance.PlaceSummonInitially(summon, tileFromMousePosition);
			if (tileFromMousePosition.structure != null && tileFromMousePosition.structure.structureType != STRUCTURE_TYPE.WILDERNESS && tileFromMousePosition.structure.structureType != STRUCTURE_TYPE.OCEAN)
			{
				summon.MigrateHomeStructureTo(tileFromMousePosition.structure);
			}
			else
			{
				summon.SetTerritory(tileFromMousePosition.area, returnHome: false);
			}
		}
	}

	public void LoadReferences(SaveDataPlayerGame data)
	{
		SetPortalTile(GridMap.Instance.map[data.portalTileXCoordinate, data.portalTileYCoordinate]);
		Faction factionByPersistentID = FactionManager.Instance.GetFactionByPersistentID(data.factionID);
		SetPlayerFaction(factionByPersistentID);
		playerSettlement = DatabaseManager.Instance.settlementDatabase.GetSettlementByPersistentID(data.settlementID) as PlayerSettlement;
		for (int i = 0; i < data.actionIntels.Count; i++)
		{
			SaveDataActionIntel saveDataActionIntel = data.actionIntels[i];
			ActionIntel item = new ActionIntel(DatabaseManager.Instance.actionDatabase.GetActionByPersistentID(saveDataActionIntel.node));
			allIntel.Add(item);
		}
		for (int j = 0; j < data.interruptIntels.Count; j++)
		{
			SaveDataInterruptIntel saveDataInterruptIntel = data.interruptIntels[j];
			InterruptIntel item2 = new InterruptIntel(DatabaseManager.Instance.interruptDatabase.GetInterruptByPersistentID(saveDataInterruptIntel.interruptHolder));
			allIntel.Add(item2);
		}
		summonMeterComponent.LoadReferences(data.summonMeterComponent);
		retaliationComponent.LoadReferences(data.retaliationComponent);
		currenciesComponent.LoadReferences(data.currenciesComponent);
		playerSettlement.LoadOtherReferencesUponLoadingSecondWavePlayer();
	}

	public void LoadReferencesMainThread(SaveDataPlayerGame data)
	{
		for (int i = 0; i < data.allNotifs.Count; i++)
		{
			data.allNotifs[i].Load();
		}
		for (int j = 0; j < data.allChaosOrbs.Count; j++)
		{
			data.allChaosOrbs[j].Load();
		}
		SubscribeListeners();
		playerSkillComponent.LoadReferencesInMainThread(data.playerSkillComponent);
		storedTargetsComponent.LoadReferences(data.storedTargetsComponent);
		underlingsComponent.LoadReferences(data.underlingsComponent);
		goalComponent.LoadReferences(data.goalComponent);
		manaRegenComponent.LoadReferencesMainThread(data.manaRegenComponent);
		if (WorldSettings.Instance.worldSettingsData.IsRetaliationAllowed())
		{
			bookmarkComponent.AddBookmark(retaliationComponent.retaliationProgress, BOOKMARK_CATEGORY.Major_Events);
		}
		PlayerUI.Instance.UpdateUI();
		for (int k = 0; k < GridMap.Instance.mainRegion.allStructures.Count; k++)
		{
			LocationStructure locationStructure = GridMap.Instance.mainRegion.allStructures[k];
			if (locationStructure.partyStructureComponent != null && locationStructure.partyStructureComponent.party != null && locationStructure.partyStructureComponent.party.currentQuest != null && locationStructure.partyStructureComponent.party.currentQuest.isDemonicQuest)
			{
				PlayerManager.Instance.player.bookmarkComponent.AddBookmark(locationStructure.partyStructureComponent.party, BOOKMARK_CATEGORY.Player_Parties);
				locationStructure.partyStructureComponent.ListenToParty();
			}
		}
	}

	public void SetIsCurrentlyBuildingDemonicStructure(bool state)
	{
		isCurrentlyBuildingDemonicStructure = state;
	}

	public void AddCharacterThatHasReported(Character p_character)
	{
		if (!charactersThatHaveReportedDemonicStructure.Contains(p_character.persistentID))
		{
			charactersThatHaveReportedDemonicStructure.Add(p_character.persistentID);
		}
	}

	public void ClearCharactersThatHaveReported()
	{
		charactersThatHaveReportedDemonicStructure.Clear();
	}

	public bool HasAlreadyReportedADemonicStructure(Character p_character)
	{
		return charactersThatHaveReportedDemonicStructure.Contains(p_character.persistentID);
	}
}
