using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using Ruinarch.Custom_UI;
using TMPro;
using Tutorial;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;
using UnityEngine.UI;
using UtilityScripts;

public class PlayerUI : BaseMonoBehaviour
{
	public static PlayerUI Instance;

	[Header("Spirit Energy")]
	public TextMeshProUGUI spiritEnergyLabel;

	[SerializeField]
	private RectTransform spiritEnergyContainer;

	[Header("Mana")]
	public TextMeshProUGUI manaLbl;

	[SerializeField]
	private RectTransform manaContainer;

	[SerializeField]
	private UIHoverPosition manaTooltipPos;

	[Header("Intel")]
	[SerializeField]
	private IntelListUI _intelListUI;

	public GameObject intelContainer;

	[SerializeField]
	private IntelItem[] intelItems;

	public Toggle intelToggle;

	[Header("General Confirmation")]
	[SerializeField]
	private GeneralConfirmation _generalConfirmation;

	[Header("Demolish Confirmation")]
	[SerializeField]
	private DemolishConfirmation _demolishConfirmation;

	[Header("Saving/Loading")]
	public Button saveGameButton;

	[Header("End Game Mechanics")]
	[SerializeField]
	private WinGameOverItem winGameOver;

	[SerializeField]
	private LoseGameOverItem loseGameOver;

	[Header("Villagers")]
	[SerializeField]
	private Toggle villagerTab;

	[Header("Top Menu")]
	[SerializeField]
	private RuinarchToggle[] topMenuButtons;

	[SerializeField]
	private SpellListUI spellList;

	[SerializeField]
	private CustomDropdownList customDropdownList;

	[SerializeField]
	private CultistsListUI cultistsList;

	[SerializeField]
	private TargetsListUI targetsList;

	public Toggle monsterToggle;

	public Toggle targetsToggle;

	[Header("Minion List")]
	[SerializeField]
	private MinionListUI minionList;

	public UIHoverPosition minionListHoverPosition;

	private readonly List<string> factionActionsList = new List<string> { "Manage Cult", "Meddle" };

	[Header("Spells")]
	public GridLayoutGroup spellsGridLayout;

	public CanvasGroup spellsCanvasGroup;

	public RectTransform spellsRectTransform;

	public GameObject spellsContainerGO;

	public GameObject spellItemPrefab;

	public PlayerSkillDetailsTooltip skillDetailsTooltip;

	public UIHoverPosition spellListHoverPosition;

	public CustomVerticalScrollSnap spellsScrollSnap;

	private List<SpellItem> _spellItems;

	[Header("Summons")]
	public SummonListUI summonList;

	[Header("Items")]
	public Toggle itemsToggle;

	public ScrollRect itemsScrollRect;

	public GameObject itemsContainerGO;

	public GameObject itemItemPrefab;

	[Header("Artifacts")]
	public Toggle artifactsToggle;

	public ScrollRect artifactsScrollRect;

	public GameObject artifactsContainerGO;

	public GameObject artifactItemPrefab;

	[Header("Monster Spawner")]
	public Toggle monsterSpawnerToggle;

	public ScrollRect monsterSpawnerScrollRect;

	public GameObject monsterSpawnerContainerGO;

	public GameObject monsterSpawnerItemPrefab;

	[Header("Threat")]
	[SerializeField]
	private TextMeshProUGUI threatLbl;

	[SerializeField]
	private UIHoverPosition threatHoverPos;

	[SerializeField]
	private RectTransform threatContainer;

	[Header("Building")]
	[SerializeField]
	private BuildListUI _buildListUI;

	[Header("Plague Points")]
	[SerializeField]
	public TextMeshProUGUI plaguePointLbl;

	[SerializeField]
	private RectTransform plaguePointsContainer;

	[Header("Accumulated Damage")]
	public TextMeshProUGUI accumulatedDamageLbl;

	public GameObject accumulatedDamageGO;

	[Header("Skill Pop Up Notif Position")]
	public RectTransform popUpDisplayPoint;

	private Tweener _currentThreatPunchTween;

	private Tweener _currentSpiritEnergyPunchTween;

	private Tweener _currentManaPunchTween;

	private Tweener _currentMonsterTabTween;

	private Tweener _currentIntelPunchEffect;

	private Tweener _currentPlaguePointPunchTween;

	private Tweener _currentTargetPunchEffect;

	[FormerlySerializedAs("_tutorialUIController")]
	[Header("Tutorial")]
	public TutorialUIController tutorialUIController;

	[SerializeField]
	private Toggle _tutorialToggle;

	[FormerlySerializedAs("_goalsUIController")]
	public GoalsUIController goalsUIController;

	[SerializeField]
	private Toggle goalsToggle;

	public SubGoalsUIController subGoalsUIController;

	[SerializeField]
	private Toggle subGoalsToggle;

	private List<Action> pendingUIToShow { get; set; }

	public DecorationListUI decorationsUI => _buildListUI.decorationsUI;

	public BuildListUI buildListUI => _buildListUI;

	public SpellListUI spellListUI => spellList;

	private void Awake()
	{
		Instance = this;
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		Instance = null;
	}

	public void UpdateUI()
	{
		if (PlayerManager.Instance.player != null)
		{
			UpdateMana();
			UpdateSpiritEnergy();
		}
	}

	public void Initialize()
	{
		pendingUIToShow = new List<Action>();
		_spellItems = new List<SpellItem>();
		goalsToggle.onValueChanged.AddListener(OnToggleGoalsTab);
		subGoalsToggle.onValueChanged.AddListener(OnToggleSubGoalsTab);
		minionList.Initialize();
		summonList.Initialize();
		Messenger.AddListener(PlayerSignals.UPDATED_CURRENCIES, UpdateUI);
		Messenger.AddListener<IIntel>(PlayerSignals.PLAYER_OBTAINED_INTEL, OnIntelObtained);
		Messenger.AddListener<IIntel>(PlayerSignals.PLAYER_REMOVED_INTEL, OnIntelRemoved);
		Messenger.AddListener(UISignals.ON_OPEN_CONVERSATION_MENU, OnOpenConversationMenu);
		Messenger.AddListener(UISignals.ON_CLOSE_CONVERSATION_MENU, OnCloseConversationMenu);
		Messenger.AddListener<PLAYER_SKILL_TYPE>(PlayerSkillSignals.PLAYER_GAINED_SPELL, OnGainSpell);
		Messenger.AddListener<PLAYER_SKILL_TYPE>(PlayerSkillSignals.PLAYER_LOST_SPELL, OnLostSpell);
		Messenger.AddListener<SkillData>(PlayerSkillSignals.SPELL_COOLDOWN_FINISHED, OnSpellCooldownFinished);
		Messenger.AddListener<MonsterAndDemonUnderlingCharges>(PlayerSkillSignals.ON_FINISH_UNDERLING_COOLDOWN, OnFinishUnderlingCoolDown);
		Messenger.AddListener<int>(PlayerSignals.PLAYER_FINISHED_PORTAL_UPGRADE, OnPortalUpgraded);
		Messenger.AddListener(UISignals.TOP_UI_ENABLED, OnSpellsOrBuildTabEnabled);
		Messenger.AddListener(UISignals.TOP_UI_DISABLED, OnSpellsOrBuildTabDisabled);
		Messenger.AddListener<SHORTCUT_ACTION>(ControlsSignals.PLAYER_INPUT_ACTION, OnReceivePlayerInputAction);
		AdjustUIDisplayBaseOnGameMode();
	}

	private void OnReceivePlayerInputAction(SHORTCUT_ACTION p_action)
	{
		if (p_action != SHORTCUT_ACTION.Cycle_Top_Menus)
		{
			return;
		}
		int num = -1;
		List<RuinarchToggle> list = RuinarchListPool<RuinarchToggle>.Claim(topMenuButtons.Length);
		for (int i = 0; i < topMenuButtons.Length; i++)
		{
			RuinarchToggle ruinarchToggle = topMenuButtons[i];
			if (ruinarchToggle.gameObject.activeSelf)
			{
				list.Add(ruinarchToggle);
			}
		}
		for (int j = 0; j < list.Count; j++)
		{
			if (list[j].isOn)
			{
				num = j;
				break;
			}
		}
		if (num == -1)
		{
			list[0].OnReceiveHotKeyClick();
		}
		else
		{
			CollectionUtilities.GetNextElementCyclic(list, num).OnReceiveHotKeyClick();
		}
		RuinarchListPool<RuinarchToggle>.Release(list);
	}

	private void OnSpellsOrBuildTabEnabled()
	{
		Vector3 vector = popUpDisplayPoint.anchoredPosition;
		vector.y = 300f;
		popUpDisplayPoint.anchoredPosition = vector;
	}

	private void OnSpellsOrBuildTabDisabled()
	{
		if (!buildListUI.isShowing && !spellListUI.isShowing && !cultistsList.isShowing && !targetsList.isShowing && !summonList.isShowing)
		{
			Vector3 vector = popUpDisplayPoint.anchoredPosition;
			vector.y = 430f;
			popUpDisplayPoint.anchoredPosition = vector;
		}
	}

	private void OnPortalUpgraded(int p_newLevel)
	{
		summonList.OnPortalUpgrade();
		minionList.OnPortalUpgrade();
	}

	private void OnFinishUnderlingCoolDown(MonsterAndDemonUnderlingCharges p_playerUnderlingComponent)
	{
		string displayName = CharacterManager.Instance.GetCharacterClass(p_playerUnderlingComponent.characterClassName).displayName;
		PopUpNotificationUI.Instance.ShowPlayerPoppingTextNotif(Utilities.YellowDotIcon() + Utilities.ColorizeName(displayName) + " " + LocalizationManager.Instance.GetLocalizedValue("UIStrings_Table", "Now_Available"), popUpDisplayPoint);
	}

	private void OnSpellCooldownFinished(SkillData p_skillData)
	{
		if (p_skillData.category != PLAYER_SKILL_CATEGORY.SUMMON)
		{
			PopUpNotificationUI.Instance.ShowPlayerPoppingTextNotif(Utilities.YellowDotIcon() + Utilities.ColorizeName(p_skillData.localizedName) + " " + LocalizationManager.Instance.GetLocalizedValue("UIStrings_Table", "Charge_Replenished"), popUpDisplayPoint);
		}
	}

	public void AdjustUIDisplayBaseOnGameMode()
	{
		UpdatePlaguePointsAmount(EditableValuesManager.Instance.GetInitialChaoticEnergyBaseOnGameMode());
		manaLbl.text = EditableValuesManager.Instance.startingMana.ToString();
	}

	public void InitializeAfterGameLoaded()
	{
		Messenger.AddListener(PlayerSignals.THREAT_UPDATED, OnThreatUpdated);
		Messenger.AddListener<int>(PlayerSignals.THREAT_INCREASED, OnThreatIncreased);
		Messenger.AddListener(PlayerSignals.THREAT_RESET, OnThreatReset);
		Messenger.AddListener<IPointOfInterest>(CharacterSignals.ON_SEIZE_POI, OnSeizePOI);
		Messenger.AddListener<IPointOfInterest>(CharacterSignals.ON_UNSEIZE_POI, OnUnseizePOI);
		Messenger.AddListener<int>(PlayerSignals.UPDATED_CHAOTIC_ENERGY, UpdatePlaguePointsAmount);
		Messenger.AddListener<int, int>(PlayerSignals.PLAYER_ADJUSTED_MANA, OnManaAdjusted);
		Messenger.AddListener<int, int>(PlayerSignals.PLAYER_ADJUSTED_SPIRIT_ENERGY, OnSpiritEnergyAdjusted);
		Messenger.AddListener<int, int>(PlayerSignals.CHAOTIC_ENERGY_ADJUSTED, OnPlaguePointsAdjusted);
		InitializeIntel();
		goalsToggle.gameObject.SetActive(WorldSettings.Instance.worldSettingsData.victoryCondition == VICTORY_CONDITION.Attainment);
		itemsToggle.gameObject.SetActive(value: false);
		artifactsToggle.gameObject.SetActive(value: false);
		monsterSpawnerToggle.gameObject.SetActive(value: false);
		summonList.UpdateSummonsList();
	}

	public void InitializeAfterLoadOutPicked()
	{
		UpdateIntel();
		_buildListUI.Initialize();
		cultistsList.Initialize();
		targetsList.Initialize();
		cultistsList.UpdateList();
		minionList.UpdateList();
		summonList.UpdateList();
		if (PlayerManager.Instance.player.goalComponent.activeGoals != null)
		{
			goalsUIController.Initialize(PlayerManager.Instance.player.goalComponent.activeGoals);
		}
		subGoalsUIController.InitializeSubGoals(PlayerManager.Instance.player.goalComponent.subGoals);
		OnThreatUpdated();
		UpdatePlaguePointsAmount(PlayerManager.Instance.player.currenciesComponent.chaoticEnergy);
		spiritEnergyContainer.gameObject.SetActive(WorldSettings.Instance.worldSettingsData.IsSpiritEnergyEnabledBasedOnVictoryCondition());
	}

	private void OnThreatUpdated()
	{
		threatLbl.text = PlayerManager.Instance.player.threatComponent.threat.ToString();
	}

	private void OnThreatIncreased(int amount)
	{
		string text = "<color=\"red\">+" + amount + "</color>";
		ObjectPoolManager.Instance.InstantiateObjectFromPool("AdjustmentEffectLbl", threatLbl.transform.position, Quaternion.identity, base.transform, isWorldPosition: true).GetComponent<AdjustmentEffectLabel>().PlayEffect(text, new Vector2(UnityEngine.Random.Range(-25, 25), -70f));
		DoThreatPunchEffect();
	}

	public void DoThreatPunchEffect()
	{
		if (_currentThreatPunchTween == null)
		{
			_currentThreatPunchTween = threatContainer.DOPunchScale(new Vector3(0.8f, 0.8f, 0.8f), 0.5f).OnComplete(delegate
			{
				_currentThreatPunchTween = null;
			});
		}
	}

	private void OnThreatReset()
	{
		string text = "<color=\"green\">-" + 100 + "</color>";
		ObjectPoolManager.Instance.InstantiateObjectFromPool("AdjustmentEffectLbl", threatLbl.transform.position, Quaternion.identity, base.transform, isWorldPosition: true).GetComponent<AdjustmentEffectLabel>().PlayEffect(text, new Vector2(UnityEngine.Random.Range(-25, 25), -70f));
		DoThreatPunchEffect();
	}

	private void OnSpiritEnergyAdjusted(int adjustedAmount, int spiritEnergy)
	{
		if (adjustedAmount != 0)
		{
			UpdateSpiritEnergy();
			ShowSpiritEnergyAdjustEffect(adjustedAmount);
			DoSpiritEnergyPunchEffect();
		}
	}

	private void UpdateSpiritEnergy()
	{
		spiritEnergyLabel.text = PlayerManager.Instance.player.currenciesComponent.spiritEnergy.ToString();
	}

	private void DoSpiritEnergyPunchEffect()
	{
		if (_currentSpiritEnergyPunchTween == null)
		{
			_currentSpiritEnergyPunchTween = spiritEnergyContainer.DOPunchScale(new Vector3(0.8f, 0.8f, 0.8f), 0.5f).OnComplete(delegate
			{
				_currentSpiritEnergyPunchTween = null;
			});
		}
	}

	private void ShowSpiritEnergyAdjustEffect(int adjustmentAmount)
	{
		string text = ((adjustmentAmount > 0) ? ("<color=\"green\">+" + adjustmentAmount + "</color>") : ("<color=\"red\">" + adjustmentAmount + "</color>"));
		ObjectPoolManager.Instance.InstantiateObjectFromPool("AdjustmentEffectLbl", spiritEnergyLabel.transform.position, Quaternion.identity, base.transform, isWorldPosition: true).GetComponent<AdjustmentEffectLabel>().PlayEffect(text, new Vector2(UnityEngine.Random.Range(-25, 25), -70f));
	}

	private void OnManaAdjusted(int adjustedAmount, int mana)
	{
		if (adjustedAmount != 0)
		{
			UpdateMana();
			ShowManaAdjustEffect(adjustedAmount);
			DoManaPunchEffect();
		}
	}

	private void UpdateMana()
	{
		manaLbl.text = PlayerManager.Instance.player.currenciesComponent.mana.ToString();
	}

	private void DoManaPunchEffect()
	{
		if (_currentManaPunchTween == null)
		{
			_currentManaPunchTween = manaContainer.DOPunchScale(new Vector3(0.8f, 0.8f, 0.8f), 0.5f).OnComplete(delegate
			{
				_currentManaPunchTween = null;
			});
		}
	}

	private void ShowManaAdjustEffect(int adjustmentAmount)
	{
		string text = ((adjustmentAmount > 0) ? ("<color=\"green\">+" + adjustmentAmount + "</color>") : ("<color=\"red\">" + adjustmentAmount + "</color>"));
		ObjectPoolManager.Instance.InstantiateObjectFromPool("AdjustmentEffectLbl", manaLbl.transform.position, Quaternion.identity, base.transform, isWorldPosition: true).GetComponent<AdjustmentEffectLabel>().PlayEffect(text, new Vector2(UnityEngine.Random.Range(-25, 25), -70f));
	}

	public void OnHoverSpiritEnergy()
	{
		string text = Utilities.SpiritEnergyIcon() + LocalizationManager.Instance.GetLocalizedValue("UIStrings_Table", "Spirit Energy");
		if (PlayerManager.Instance.player != null)
		{
			text = text ?? "";
		}
		UIManager.Instance.ShowSmallInfo(LocalizationManager.Instance.GetLocalizedValue("UIStrings_Table", "Spirit_Energy_Tooltip"), manaTooltipPos, text, autoReplaceText: false);
	}

	public void OnHoverOverMana()
	{
		string text = LocalizationManager.Instance.GetLocalizedValue("UIStrings_Table", "Mana") ?? "";
		if (PlayerManager.Instance.player != null)
		{
			text = text + " - " + PlayerManager.Instance.player.currenciesComponent.mana + "/" + EditableValuesManager.Instance.maximumMana + " (+" + PlayerManager.Instance.player.manaRegenComponent.GetManaRegenPerHour() + "/" + LocalizationManager.Hour + ")";
		}
		UIManager.Instance.ShowSmallInfo(LocalizationManager.Instance.GetLocalizedValue("UIStrings_Table", "Mana_Tooltip"), manaTooltipPos, text, autoReplaceText: false);
	}

	public void OnHoverOutMana()
	{
		UIManager.Instance.HideSmallInfo();
	}

	public void DoMonsterTabPunchEffect()
	{
		if (_currentMonsterTabTween == null)
		{
			_currentMonsterTabTween = monsterToggle.transform.DOPunchScale(new Vector3(2f, 2f, 1f), 0.2f).OnComplete(delegate
			{
				_currentMonsterTabTween = null;
			});
		}
	}

	public void AddPendingUI(Action pendingUIAction)
	{
		pendingUIToShow.Add(pendingUIAction);
	}

	public bool TryShowPendingUI()
	{
		if (pendingUIToShow.Count > 0)
		{
			Action action = pendingUIToShow[0];
			pendingUIToShow.RemoveAt(0);
			action();
			return true;
		}
		return false;
	}

	public bool IsMajorUIShowing()
	{
		if (!_generalConfirmation.isShowing && !UIManager.Instance.generalConfirmationWithVisual.isShowing)
		{
			return UIManager.Instance.yesNoConfirmation.yesNoGO.activeInHierarchy;
		}
		return true;
	}

	private void OnIntelObtained(IIntel intel)
	{
		UpdateIntel();
	}

	private void OnIntelRemoved(IIntel intel)
	{
		UpdateIntel();
	}

	private void UpdateIntel()
	{
		for (int i = 0; i < intelItems.Length; i++)
		{
			IntelItem intelItem = intelItems[i];
			IIntel intel = PlayerManager.Instance.player.allIntel.ElementAtOrDefault(i);
			intelItem.SetIntel(intel);
			if (intel != null)
			{
				intelItem.SetClickAction(PlayerManager.Instance.player.SetCurrentActiveIntel);
				intelItem.SetOnHoverEnterAction(delegate
				{
					OnHoverEnterStoredIntel(intel);
				});
				intelItem.SetOnHoverExitAction(OnHoverExitStoredIntel);
			}
		}
	}

	private void OnHoverEnterStoredIntel(IIntel intel)
	{
		string fullIntelTooltip = intel.GetFullIntelTooltip();
		UIManager.Instance.ShowSmallInfo(fullIntelTooltip, "", autoReplaceText: false);
	}

	private void OnHoverExitStoredIntel()
	{
		UIManager.Instance.HideSmallInfo();
	}

	private void InitializeIntel()
	{
		for (int i = 0; i < intelItems.Length; i++)
		{
			intelItems[i].SetIntel(null);
		}
	}

	public void SetIntelMenuState(bool state)
	{
		if (intelToggle.isOn != state)
		{
			intelToggle.isOn = state;
			if (!intelToggle.isOn)
			{
				OnCloseIntelMenu();
			}
		}
	}

	private void OnCloseIntelMenu()
	{
		for (int i = 0; i < intelItems.Length; i++)
		{
			intelItems[i].ClearClickActions();
		}
	}

	public void SetIntelItemClickActions(IntelItem.OnClickAction clickAction)
	{
		for (int i = 0; i < intelItems.Length; i++)
		{
			intelItems[i].SetClickAction(clickAction);
		}
	}

	public void AddIntelItemOtherClickActions(Action clickAction)
	{
		for (int i = 0; i < intelItems.Length; i++)
		{
			intelItems[i].AddOtherClickAction(clickAction);
		}
	}

	private void OnOpenConversationMenu()
	{
		intelToggle.isOn = false;
		intelToggle.interactable = false;
	}

	private void OnCloseConversationMenu()
	{
		intelToggle.interactable = true;
	}

	public void OnToggleIntel(bool isOn)
	{
		if (isOn)
		{
			_intelListUI.Open();
		}
		else
		{
			_intelListUI.Close();
		}
	}

	public IntelItem GetIntelItemWithIntel(IIntel intel)
	{
		for (int i = 0; i < intelItems.Length; i++)
		{
			if (intelItems[i].intel != null && intelItems[i].intel == intel)
			{
				return intelItems[i];
			}
		}
		return null;
	}

	public void DoIntelTabPunchEffect()
	{
		if (_currentIntelPunchEffect == null)
		{
			_currentIntelPunchEffect = intelToggle.transform.DOPunchScale(new Vector3(2f, 2f, 1f), 0.2f).OnComplete(delegate
			{
				_currentIntelPunchEffect = null;
			});
		}
	}

	public void OnHoverEnterThreat()
	{
		UIManager.Instance.ShowSmallInfo(LocalizationManager.Instance.GetLocalizedValue("UIStrings_Table", "Threat_Tooltip"), threatHoverPos, LocalizationManager.Instance.GetLocalizedValue("UIStrings_Table", "Threat"));
	}

	public void OnHoverExitThreat()
	{
		UIManager.Instance.HideSmallInfo();
	}

	public void WinGameOver(string winMessage)
	{
		if (!PlayerManager.Instance.player.hasAlreadyWon)
		{
			PlayerManager.Instance.player.hasAlreadyWon = true;
			UIManager.Instance.ShowEndDemoScreen(winMessage, p_isGameWin: true);
		}
	}

	public void LoseGameOver(string p_gameOverMessage)
	{
		UIManager.Instance.ShowEndDemoScreen(p_gameOverMessage, p_isGameWin: false);
	}

	public void ToggleVillagersTab(bool isOn)
	{
		if (isOn)
		{
			FactionInfoHubUI.Instance.Open();
			FactionInfoHubUI.Instance.ShowMembers();
		}
		else
		{
			FactionInfoHubUI.Instance.Close();
		}
	}

	public void SetVillagerTabIsOn(bool state)
	{
		villagerTab.isOn = state;
	}

	public void ShowGeneralConfirmation(string header, string body, string buttonText = "OK", Action onClickOK = null, Action onClickCenter = null, bool autoReplaceBodyText = true)
	{
		_generalConfirmation.ShowGeneralConfirmation(header, body, buttonText, onClickOK, onClickCenter, autoReplaceBodyText);
	}

	public void ShowDemolishConfirmation(UnityAction<bool, bool, bool, bool> p_onClickOK)
	{
		_demolishConfirmation.ShowDemolishConfirmation(p_onClickOK);
	}

	private void OnSeizePOI(IPointOfInterest poi)
	{
		DisableTopMenuButtons();
	}

	private void OnUnseizePOI(IPointOfInterest poi)
	{
		EnableTopMenuButtons();
	}

	public void OnToggleSpells(bool isOn)
	{
		if (isOn)
		{
			ShowSpells();
		}
		else
		{
			HideSpells();
		}
	}

	private void ShowSpells()
	{
		spellListUI.Open();
		spellsScrollSnap.ForceUpdateButtonsInteractability();
		Messenger.Broadcast(UISignals.SPELLS_MENU_SHOWN);
		StartCoroutine(UpdateSpellItemNames());
	}

	private void HideSpells()
	{
		spellListUI.Close();
	}

	private IEnumerator UpdateSpellItemNames()
	{
		yield return null;
		for (int i = 0; i < _spellItems.Count; i++)
		{
			_spellItems[i].UpdateNameText();
		}
	}

	private void StartShowSpells()
	{
		spellsCanvasGroup.DOKill();
		spellsRectTransform.DOKill();
		spellsCanvasGroup.interactable = false;
		spellsCanvasGroup.alpha = 0f;
		spellsRectTransform.anchoredPosition = new Vector3(0f, 300f, 0f);
		Sequence sequence = DOTween.Sequence();
		sequence.Append(spellsRectTransform.DOAnchorPosY(0f, 0.3f).SetEase(Ease.OutBack));
		sequence.Join(DOTween.To(() => spellsCanvasGroup.alpha, delegate(float x)
		{
			spellsCanvasGroup.alpha = x;
		}, 1f, 0.3f).SetEase(Ease.InSine));
		sequence.OnComplete(OnCompleteShowSpells);
		sequence.Play();
	}

	private void StartHideSpells()
	{
		spellsCanvasGroup.DOFade(0f, 0.4f).SetEase(Ease.OutSine).OnComplete(OnCompleteHideSpells);
	}

	private void OnCompleteShowSpells()
	{
		spellsCanvasGroup.interactable = true;
	}

	private void OnCompleteHideSpells()
	{
		spellsCanvasGroup.interactable = false;
		spellsCanvasGroup.alpha = 0f;
		spellsRectTransform.localPosition = new Vector3(0f, 300f, 0f);
	}

	private void CreateInitialSpells()
	{
		for (int i = 0; i < PlayerManager.Instance.player.playerSkillComponent.spells.Count; i++)
		{
			PLAYER_SKILL_TYPE spell = PlayerManager.Instance.player.playerSkillComponent.spells[i];
			CreateNewSpellItem(spell);
		}
	}

	private void OnGainSpell(PLAYER_SKILL_TYPE spell)
	{
		CreateNewSpellItem(spell);
	}

	private void OnLostSpell(PLAYER_SKILL_TYPE spell)
	{
		DeleteSpellItem(spell);
	}

	private void CreateNewSpellItem(PLAYER_SKILL_TYPE spell)
	{
		GameObject obj = ObjectPoolManager.Instance.InstantiateObjectFromPool(spellItemPrefab.name, Vector3.zero, Quaternion.identity, spellsGridLayout.transform);
		SpellItem component = obj.GetComponent<SpellItem>();
		obj.SetActive(value: false);
		SkillData spellData = PlayerSkillManager.Instance.GetSpellData(spell);
		if (spellData != null)
		{
			component.SetObject(spellData);
		}
		else
		{
			spellData = PlayerSkillManager.Instance.GetAfflictionData(spell);
			if (spellData != null)
			{
				component.SetObject(spellData);
			}
			else
			{
				spellData = PlayerSkillManager.Instance.GetPlayerActionData(spell);
				if (spellData != null)
				{
					component.SetObject(spellData);
				}
			}
		}
		obj.SetActive(value: true);
		_spellItems.Add(component);
	}

	private void DeleteSpellItem(PLAYER_SKILL_TYPE spell)
	{
		SpellItem spellItem = GetSpellItem(spell);
		if (spellItem != null)
		{
			ObjectPoolManager.Instance.DestroyObject(spellItem.gameObject);
			_spellItems.Remove(spellItem);
		}
	}

	public SpellItem GetSpellItem(PLAYER_SKILL_TYPE spell)
	{
		for (int i = 0; i < _spellItems.Count; i++)
		{
			SpellItem spellItem = _spellItems[i];
			if (spellItem.spellData.type == spell)
			{
				return spellItem;
			}
		}
		return null;
	}

	public void OnHoverSpell(SkillData skillData, UIHoverPosition position = null, IPlayerActionTarget p_target = null, bool p_showImage = false, Vector2 p_spriteSize = default(Vector2))
	{
		skillDetailsTooltip.ShowPlayerSkillDetails(skillData, position, p_dontShowAdditionalText: false, p_target, p_showImage, p_spriteSize);
	}

	public string OnHoverSpellChargeRemaining(SkillData skillData, MonsterAndDemonUnderlingCharges p_monsterUnderling)
	{
		string result = string.Empty;
		if (skillData.isInCooldown)
		{
			string text = GameManager.Instance.Today().AddTicks(skillData.cooldown - skillData.currentCooldownTick).ToString();
			result = LocalizationManager.Instance.GetLocalizedValue("UIStrings_Table", "New_Charge") + " " + Utilities.ColorizeName(skillData.localizedName) + " " + LocalizationManager.Instance.GetLocalizedValue("UIStrings_Table", "Small_At") + " " + Utilities.ColorizeName(text);
		}
		return result;
	}

	public string OnHoverSpellChargeRemainingForSummon(CharacterClass cData, MonsterAndDemonUnderlingCharges p_monsterUnderling)
	{
		string result = string.Empty;
		if (p_monsterUnderling.isReplenishing)
		{
			string text = p_monsterUnderling.replenishDate.ToString();
			result = LocalizationManager.Instance.GetLocalizedValue("UIStrings_Table", "New_Charge") + " " + Utilities.ColorizeName(cData.displayName) + " " + LocalizationManager.Instance.GetLocalizedValue("UIStrings_Table", "Small_At") + " " + Utilities.ColorizeName(text);
		}
		return result;
	}

	public void OnHoverOutSpell(SkillData skillData)
	{
		Tooltip.Instance.HideSmallInfo();
		skillDetailsTooltip.HidePlayerSkillDetails();
	}

	public void OnToggleItems(bool isOn)
	{
		if (isOn)
		{
			ShowItems();
		}
		else
		{
			HideItems();
		}
	}

	private void ShowItems()
	{
		itemsContainerGO.SetActive(value: true);
	}

	private void HideItems()
	{
		itemsContainerGO.SetActive(value: false);
	}

	private void CreateItemsForTesting()
	{
		TILE_OBJECT_TYPE[] array = new TILE_OBJECT_TYPE[86]
		{
			TILE_OBJECT_TYPE.ELECTRIC_CRYSTAL,
			TILE_OBJECT_TYPE.FIRE_CRYSTAL,
			TILE_OBJECT_TYPE.ICE_CRYSTAL,
			TILE_OBJECT_TYPE.POISON_CRYSTAL,
			TILE_OBJECT_TYPE.WATER_CRYSTAL,
			TILE_OBJECT_TYPE.SNOW_MOUND,
			TILE_OBJECT_TYPE.WINTER_ROSE,
			TILE_OBJECT_TYPE.DESERT_ROSE,
			TILE_OBJECT_TYPE.CULTIST_KIT,
			TILE_OBJECT_TYPE.TREASURE_CHEST,
			TILE_OBJECT_TYPE.ICE,
			TILE_OBJECT_TYPE.HERB_PLANT,
			TILE_OBJECT_TYPE.ANIMAL_MEAT,
			TILE_OBJECT_TYPE.PROFESSION_PEDESTAL,
			TILE_OBJECT_TYPE.TOOL,
			TILE_OBJECT_TYPE.EXCALIBUR,
			TILE_OBJECT_TYPE.PHYLACTERY,
			TILE_OBJECT_TYPE.BIG_TREE_OBJECT,
			TILE_OBJECT_TYPE.COPPER_SWORD,
			TILE_OBJECT_TYPE.IRON_SWORD,
			TILE_OBJECT_TYPE.ORICHALCUM_SWORD,
			TILE_OBJECT_TYPE.MITHRIL_SWORD,
			TILE_OBJECT_TYPE.MINK_SHIRT,
			TILE_OBJECT_TYPE.SPIDER_SILK_SHIRT,
			TILE_OBJECT_TYPE.SCROLL,
			TILE_OBJECT_TYPE.RING,
			TILE_OBJECT_TYPE.BELT,
			TILE_OBJECT_TYPE.BRACER,
			TILE_OBJECT_TYPE.COPPER_ARMOR,
			TILE_OBJECT_TYPE.IRON_ARMOR,
			TILE_OBJECT_TYPE.POWER_CRYSTAL,
			TILE_OBJECT_TYPE.BASIC_SWORD,
			TILE_OBJECT_TYPE.BASIC_AXE,
			TILE_OBJECT_TYPE.BASIC_BOW,
			TILE_OBJECT_TYPE.BASIC_DAGGER,
			TILE_OBJECT_TYPE.BASIC_STAFF,
			TILE_OBJECT_TYPE.BASIC_SHIRT,
			TILE_OBJECT_TYPE.CORN,
			TILE_OBJECT_TYPE.POTATO,
			TILE_OBJECT_TYPE.PINEAPPLE,
			TILE_OBJECT_TYPE.ICEBERRY,
			TILE_OBJECT_TYPE.HYPNO_HERB,
			TILE_OBJECT_TYPE.COPPER,
			TILE_OBJECT_TYPE.IRON,
			TILE_OBJECT_TYPE.MITHRIL,
			TILE_OBJECT_TYPE.ORICHALCUM,
			TILE_OBJECT_TYPE.RABBIT_CLOTH,
			TILE_OBJECT_TYPE.MINK_CLOTH,
			TILE_OBJECT_TYPE.WOOL,
			TILE_OBJECT_TYPE.BOAR_HIDE,
			TILE_OBJECT_TYPE.WOLF_HIDE,
			TILE_OBJECT_TYPE.BEAR_HIDE,
			TILE_OBJECT_TYPE.SCALE_HIDE,
			TILE_OBJECT_TYPE.DRAGON_HIDE,
			TILE_OBJECT_TYPE.COPPER,
			TILE_OBJECT_TYPE.STONE_PILE,
			TILE_OBJECT_TYPE.MITHRIL,
			TILE_OBJECT_TYPE.MINK_CLOTH,
			TILE_OBJECT_TYPE.WOOD_PILE,
			TILE_OBJECT_TYPE.ANIMAL_MEAT,
			TILE_OBJECT_TYPE.ELF_MEAT,
			TILE_OBJECT_TYPE.HUMAN_MEAT,
			TILE_OBJECT_TYPE.STONE_PILE,
			TILE_OBJECT_TYPE.WOOD_PILE,
			TILE_OBJECT_TYPE.FISH_PILE,
			TILE_OBJECT_TYPE.TABLE,
			TILE_OBJECT_TYPE.BOAR_HIDE_ARMOR,
			TILE_OBJECT_TYPE.BEAR_HIDE_ARMOR,
			TILE_OBJECT_TYPE.DRAGON_ARMOR,
			TILE_OBJECT_TYPE.WOLF_HIDE_ARMOR,
			TILE_OBJECT_TYPE.SCALE_ARMOR,
			TILE_OBJECT_TYPE.RABBIT_SHIRT,
			TILE_OBJECT_TYPE.WOOL_SHIRT,
			TILE_OBJECT_TYPE.MOONWALKER_SHIRT,
			TILE_OBJECT_TYPE.ICEBERRY_CROP,
			TILE_OBJECT_TYPE.ICEBERRY,
			TILE_OBJECT_TYPE.LUNCH_PACK,
			TILE_OBJECT_TYPE.WYVERN_EGG,
			TILE_OBJECT_TYPE.MAGIC_CIRCLE,
			TILE_OBJECT_TYPE.MANTRA,
			TILE_OBJECT_TYPE.FANG,
			TILE_OBJECT_TYPE.SAVAGE,
			TILE_OBJECT_TYPE.ECLIPSE,
			TILE_OBJECT_TYPE.CITRUS,
			TILE_OBJECT_TYPE.WEREWOLF_PELT,
			TILE_OBJECT_TYPE.POISON_VENT
		};
		for (int i = 0; i < array.Length; i++)
		{
			CreateNewItemItem(array[i]);
		}
	}

	private void CreateNewItemItem(TILE_OBJECT_TYPE item)
	{
		ObjectPoolManager.Instance.InstantiateObjectFromPool(itemItemPrefab.name, Vector3.zero, Quaternion.identity, itemsScrollRect.content).GetComponent<ItemItem>().SetItem(item);
	}

	public void OnToggleArtifacts(bool isOn)
	{
		if (isOn)
		{
			ShowArtifacts();
		}
		else
		{
			HideArtifacts();
		}
	}

	private void ShowArtifacts()
	{
		artifactsContainerGO.SetActive(value: true);
	}

	private void HideArtifacts()
	{
		artifactsContainerGO.SetActive(value: false);
	}

	public void CreateArtifactsForTesting()
	{
		ARTIFACT_TYPE[] array = (ARTIFACT_TYPE[])Enum.GetValues(typeof(ARTIFACT_TYPE));
		for (int i = 0; i < array.Length; i++)
		{
			if (array[i] != ARTIFACT_TYPE.None)
			{
				CreateNewArtifactItem(array[i]);
			}
		}
	}

	private void CreateNewArtifactItem(ARTIFACT_TYPE artifact)
	{
		ObjectPoolManager.Instance.InstantiateObjectFromPool(artifactItemPrefab.name, Vector3.zero, Quaternion.identity, artifactsScrollRect.content).GetComponent<ArtifactItem>().SetArtifact(artifact);
	}

	public void OnToggleMonsterSpawner(bool isOn)
	{
		if (isOn)
		{
			ShowMonsterSpawner();
		}
		else
		{
			HideMonsterSpawner();
		}
	}

	private void ShowMonsterSpawner()
	{
		monsterSpawnerContainerGO.SetActive(value: true);
	}

	private void HideMonsterSpawner()
	{
		monsterSpawnerContainerGO.SetActive(value: false);
	}

	private void CreateMonstersForTesting()
	{
		SUMMON_TYPE[] enumValues = CollectionUtilities.GetEnumValues<SUMMON_TYPE>();
		foreach (SUMMON_TYPE sUMMON_TYPE in enumValues)
		{
			if (sUMMON_TYPE != SUMMON_TYPE.None)
			{
				CreateNewMonsterSpawnerItem(sUMMON_TYPE);
			}
		}
	}

	private void CreateNewMonsterSpawnerItem(SUMMON_TYPE p_monsterType)
	{
		ObjectPoolManager.Instance.InstantiateObjectFromPool(monsterSpawnerItemPrefab.name, Vector3.zero, Quaternion.identity, monsterSpawnerScrollRect.content).GetComponent<MonsterSpawnerItem>().SetMonster(p_monsterType);
	}

	public void EnableTopMenuButtons()
	{
		for (int i = 0; i < topMenuButtons.Length; i++)
		{
			topMenuButtons[i].interactable = true;
		}
	}

	public void DisableTopMenuButtons()
	{
		for (int i = 0; i < topMenuButtons.Length; i++)
		{
			topMenuButtons[i].interactable = false;
		}
	}

	public void CloseAllTopMenus()
	{
		for (int i = 0; i < topMenuButtons.Length; i++)
		{
			topMenuButtons[i].isOn = false;
		}
	}

	public bool IsTopMenuToggleOn(string toggleName)
	{
		for (int i = 0; i < topMenuButtons.Length; i++)
		{
			Toggle toggle = topMenuButtons[i];
			if (toggle.name == toggleName && toggle.isOn)
			{
				return true;
			}
		}
		return false;
	}

	public void OnToggleBuildList(bool isOn)
	{
		if (isOn)
		{
			_buildListUI.Open();
		}
		else
		{
			_buildListUI.Close();
		}
	}

	private void OnPlaguePointsAdjusted(int p_adjustedAmount, int p_totalAmount)
	{
		if (p_adjustedAmount != 0)
		{
			UpdatePlaguePointsAmount(p_totalAmount);
			ShowPlaguePointsGainedEffect(p_adjustedAmount);
			DoPlaguePointPunchEffect();
		}
	}

	private void DoPlaguePointPunchEffect()
	{
		if (_currentPlaguePointPunchTween == null)
		{
			_currentPlaguePointPunchTween = plaguePointsContainer.DOPunchScale(new Vector3(0.8f, 0.8f, 0.8f), 0.5f).OnComplete(delegate
			{
				_currentPlaguePointPunchTween = null;
			});
		}
	}

	private void ShowPlaguePointsGainedEffect(int adjustmentAmount)
	{
		if (plaguePointsContainer.gameObject.activeSelf)
		{
			string text = ((adjustmentAmount > 0) ? ("<color=\"green\">+" + adjustmentAmount + "</color>") : ("<color=\"red\">" + adjustmentAmount + "</color>"));
			ObjectPoolManager.Instance.InstantiateObjectFromPool("AdjustmentEffectLbl", plaguePointLbl.transform.position, Quaternion.identity, base.transform, isWorldPosition: true).GetComponent<AdjustmentEffectLabel>().PlayEffect(text, new Vector2(UnityEngine.Random.Range(-25, 25), -70f));
		}
	}

	private void UpdatePlaguePointsAmount(int p_amount)
	{
		plaguePointLbl.text = p_amount.ToString();
	}

	public void OnHoverEnterPlaguePoints()
	{
		string text = Utilities.ChaoticEnergyIcon() + LocalizationManager.Instance.GetLocalizedValue("UIStrings_Table", "Chaotic Energy");
		if (PlayerManager.Instance.player != null)
		{
			text = text + " - " + PlayerManager.Instance.player.currenciesComponent.chaoticEnergy + "/" + PlayerManager.Instance.player.currenciesComponent.maxChaoticEnergy;
		}
		UIManager.Instance.ShowSmallInfo(LocalizationManager.Instance.GetLocalizedValue("UIStrings_Table", "Chaotic_Energy_Tooltip"), threatHoverPos, text, autoReplaceText: false);
	}

	public void OnHoverExitPlaguePoints()
	{
		UIManager.Instance.HideSmallInfo();
	}

	public void OnToggleTargetsTab(bool p_state)
	{
		if (p_state)
		{
			targetsList.Open();
		}
		else
		{
			targetsList.Close();
		}
	}

	public void DoTargetTabPunchEffect()
	{
		if (_currentTargetPunchEffect == null)
		{
			_currentTargetPunchEffect = targetsToggle.transform.DOPunchScale(new Vector3(2f, 2f, 1f), 0.2f).OnComplete(delegate
			{
				_currentTargetPunchEffect = null;
			});
		}
	}

	public void UpdateAccumulatedDamageText(int amount)
	{
		accumulatedDamageLbl.text = amount.ToString();
	}

	public void OnToggleTutorialTab(bool p_isOn)
	{
		if (p_isOn)
		{
			tutorialUIController.ShowUI();
		}
		else
		{
			tutorialUIController.HideUI();
		}
	}

	public void ShowSpecificTutorial(TutorialManager.Tutorial_Type p_type)
	{
		tutorialUIController.ShowUI();
		tutorialUIController.JumpToSpecificTutorial(p_type);
	}

	public void OnCloseTutorialUI()
	{
		_tutorialToggle.SetIsOnWithoutNotify(value: false);
	}

	private void OnToggleGoalsTab(bool p_isOn)
	{
		if (p_isOn)
		{
			goalsUIController.ShowUI();
		}
		else
		{
			goalsUIController.HideUI();
		}
	}

	public void OnCloseGoalsUI()
	{
		goalsToggle.SetIsOnWithoutNotify(value: false);
	}

	private void OnToggleSubGoalsTab(bool p_isOn)
	{
		if (p_isOn)
		{
			subGoalsUIController.ShowUI();
		}
		else
		{
			subGoalsUIController.HideUI();
		}
	}

	public void OnCloseSubGoalsUI()
	{
		subGoalsToggle.SetIsOnWithoutNotify(value: false);
	}
}
