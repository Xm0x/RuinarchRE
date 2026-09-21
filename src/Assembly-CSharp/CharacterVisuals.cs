using Object_Pools;
using Settings;
using Traits;
using UnityEngine;
using UtilityScripts;

public class CharacterVisuals
{
	private static readonly int HairHue = Shader.PropertyToID("_Hue");

	private static readonly int HairSaturation = Shader.PropertyToID("_Saturation");

	private static readonly int HairValue = Shader.PropertyToID("_Value");

	private static readonly int HsvaAdjust = Shader.PropertyToID("_HSVAAdjust");

	public static string ClassToUseForArachnophobia = "Earthen Wisp";

	public static Color ColorToUseForArachnophobia = Color.green;

	private Character _owner;

	private bool _hasBlood;

	private bool _usePreviousClassAsset;

	public PortraitSettings portraitSettings { get; private set; }

	public CharacterSpritesPerAnimation markerAnimations { get; private set; }

	public Sprite defaultSprite { get; private set; }

	public Vector2 selectableSize { get; private set; }

	public Color baseMarkerTint { get; private set; }

	public string classToUseForVisuals
	{
		get
		{
			if (_owner.race == RACE.RATMAN)
			{
				return "Ratman";
			}
			if (_owner.minion != null && _owner.minion.minionPlayerSkillType != PLAYER_SKILL_TYPE.NONE)
			{
				return _owner.minion.GetMinionClassName(_owner.minion.minionPlayerSkillType);
			}
			if (_usePreviousClassAsset)
			{
				if (!string.IsNullOrEmpty(_owner.classComponent.previousClassName))
				{
					return _owner.classComponent.previousClassName;
				}
			}
			else if (_owner.race == RACE.SPIDER && SettingsManager.Instance.settings.arachnophobiaToggle)
			{
				return ClassToUseForArachnophobia;
			}
			return _owner.characterClass.className;
		}
	}

	public Vector2 markerVisualScale
	{
		get
		{
			if (_owner.race == RACE.SPIDER && SettingsManager.Instance.settings.arachnophobiaToggle)
			{
				switch (_owner.characterClass.className)
				{
				case "Tarantula":
				case "Broodmother":
					return new Vector2(1.5f, 1.5f);
				case "Small Spider":
					return new Vector2(0.8f, 0.8f);
				}
			}
			return Vector2.one;
		}
	}

	public Color markerVisualTint
	{
		get
		{
			if (_owner.race == RACE.SPIDER && SettingsManager.Instance.settings.arachnophobiaToggle)
			{
				return ColorToUseForArachnophobia;
			}
			return baseMarkerTint;
		}
	}

	public CharacterVisuals(Character character)
	{
		_owner = character;
		_hasBlood = true;
		baseMarkerTint = Color.white;
	}

	public CharacterVisuals(Character character, SaveDataCharacter data)
	{
		_owner = character;
		_hasBlood = data.hasBlood;
		portraitSettings = data.portraitSettings;
		baseMarkerTint = ((data.baseMarkerTint == default(Color)) ? Color.white : data.baseMarkerTint);
	}

	public void Initialize()
	{
		portraitSettings = CharacterManager.Instance.GeneratePortraitSettings(_owner);
		UpdateMarkerAnimations(_owner);
		UpdateMountMarkerAnimations(_owner);
	}

	public bool HasHeadHair()
	{
		if (!_owner.race.HasHeadHair())
		{
			return false;
		}
		if (_owner.traitContainer.HasTrait("Polymorphed"))
		{
			return false;
		}
		if (_owner.isInVampireBatForm || _owner.isInWerewolfForm)
		{
			return false;
		}
		if (_owner.characterClass.IsZombie() && !_usePreviousClassAsset)
		{
			return false;
		}
		return true;
	}

	public void UpdateAllVisuals(Character character, bool regeneratePortrait = false)
	{
		UpdateMarkerAnimations(character);
		UpdateMountMarkerAnimations(character);
		UpdateMarkerVisualSize();
		UpdateMarkerVisualTint();
		if (regeneratePortrait)
		{
			RegeneratePortraitSettings(character);
		}
		else
		{
			UpdatePortraitSettings(character);
		}
		if (character.hasMarker)
		{
			character.marker.UpdateMarkerVisuals();
		}
		Messenger.Broadcast(CharacterSignals.UPDATE_CHARACTER_PORTRAITS, _owner);
	}

	private void UpdatePortraitSettings(Character character)
	{
		portraitSettings = CharacterManager.Instance.UpdatePortraitSettings(character.race, character.gender, character.hairColorType, classToUseForVisuals, portraitSettings.portraitIndex);
	}

	private void RegeneratePortraitSettings(Character character)
	{
		portraitSettings = CharacterManager.Instance.GeneratePortraitSettings(character);
	}

	public void UsePreviousClassAsset(bool p_state)
	{
		_usePreviousClassAsset = p_state;
	}

	private void UpdateMarkerVisualSize()
	{
		if (_owner.hasMarker)
		{
			_owner.marker.SetMainVisualScale(markerVisualScale);
		}
	}

	private void UpdateMarkerVisualTint()
	{
		if (_owner.hasMarker)
		{
			_owner.marker.SetMainVisualTint(markerVisualTint);
		}
	}

	public void SetBaseMarkerTint(Color p_color)
	{
		baseMarkerTint = p_color;
	}

	private void UpdateMarkerAnimations(Character character)
	{
		CharacterSpritesPerAnimation characterSpritesPerAnimation = CharacterManager.Instance.GetCharacterAnimationSprites(character.race, character.visuals.classToUseForVisuals);
		Polymorphed traitOrStatus = character.traitContainer.GetTraitOrStatus<Polymorphed>("Polymorphed");
		if (traitOrStatus != null)
		{
			SummonPlayerSkill summonPlayerSkillData = PlayerSkillManager.Instance.GetSummonPlayerSkillData(traitOrStatus.animalType);
			characterSpritesPerAnimation = CharacterManager.Instance.GetCharacterAnimationSprites(summonPlayerSkillData.race, summonPlayerSkillData.className);
		}
		else if (!character.mountComponent.IsMounting())
		{
			bool flag = character.isInVampireBatForm;
			bool flag2 = character.isInWerewolfForm;
			if (character.reactionComponent.disguisedCharacter != null && character.reactionComponent.disguisedCharacter.visuals != null)
			{
				flag = false;
				flag2 = false;
				characterSpritesPerAnimation = CharacterManager.Instance.GetCharacterAnimationSprites(character.reactionComponent.disguisedCharacter.race, character.reactionComponent.disguisedCharacter.visuals.classToUseForVisuals);
			}
			if (flag)
			{
				characterSpritesPerAnimation = CharacterManager.Instance.GetSpecialCharacterAnimationSprites("Vampire Bat");
			}
			else if (flag2)
			{
				characterSpritesPerAnimation = CharacterManager.Instance.GetCharacterAnimationSprites(RACE.NONE, "Werewolf");
			}
		}
		defaultSprite = characterSpritesPerAnimation.GetSprite("Idle", "idle_1");
		if (defaultSprite == null)
		{
			Debug.LogError("No default sprite for " + character.characterClass.className);
		}
		float num = defaultSprite.rect.width / 100f;
		if (character is Troll)
		{
			num = 0.8f;
		}
		else if (character is Dragon)
		{
			num = 2.56f;
		}
		else if (character is Wyvern)
		{
			num = 1.28f;
		}
		selectableSize = new Vector2(num, num);
		markerAnimations = characterSpritesPerAnimation;
		if (character.hasMarker)
		{
			character.marker.UpdateName();
		}
	}

	private void UpdateMountMarkerAnimations(Character character)
	{
		if (character != null)
		{
			Character mountedCharacter = character.mountComponent.mountedCharacter;
			mountedCharacter?.visuals?.UpdateMarkerAnimations(mountedCharacter);
		}
	}

	public bool HasBlood()
	{
		return _hasBlood;
	}

	public void SetHasBlood(bool state)
	{
		_hasBlood = state;
	}

	public string GetThoughtBubble()
	{
		if (_owner.isDead)
		{
			if (_owner.deathLog != null)
			{
				return _owner.deathLog.logText;
			}
			Log log = GameManager.CreateNewLogUsingNewLocalization(GameManager.Instance.Today(), "Character", "CharacterGeneric_Table", "Thought_Bubble_Death_Default");
			log.AddToFillers(_owner, _owner.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
			string logText = log.logText;
			LogPool.Release(log);
			return logText;
		}
		if (_owner is Wyvern wyvern)
		{
			Character rider = wyvern.mountComponent.GetRider();
			if (rider != null)
			{
				Log log2 = GameManager.CreateNewLogUsingNewLocalization(GameManager.Instance.Today(), "Character", "CharacterGeneric_Table", "Thought_Bubble_Mounted");
				log2.AddToFillers(_owner, _owner.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
				log2.AddToFillers(rider, rider.name, LOG_IDENTIFIER.TARGET_CHARACTER);
				string logText2 = log2.logText;
				LogPool.Release(log2);
				return logText2;
			}
		}
		if (_owner.interruptComponent.isInterrupted && _owner.interruptComponent.thoughtBubbleLog != null)
		{
			return _owner.interruptComponent.thoughtBubbleLog.logText;
		}
		if (_owner.currentActionNode != null)
		{
			return _owner.currentActionNode.GetCurrentLog().logText;
		}
		if (_owner.stateComponent.currentState != null)
		{
			if (_owner.stateComponent.currentState.thoughtBubbleLog != null)
			{
				return _owner.stateComponent.currentState.thoughtBubbleLog.logText;
			}
			Debug.LogWarning("Thought Bubble Log of " + _owner.name + "'s state " + _owner.stateComponent.currentState.stateName + " is null!");
		}
		if ((bool)_owner.marker && _owner.marker.hasFleePath)
		{
			Log log3 = GameManager.CreateNewLogUsingNewLocalization(GameManager.Instance.Today(), "Character", "CharacterGeneric_Table", "Thought_Bubble_Fleeing");
			log3.AddToFillers(_owner, _owner.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
			string logText3 = log3.logText;
			LogPool.Release(log3);
			return logText3;
		}
		Character masterCharacter = _owner.carryComponent.masterCharacter;
		if ((bool)masterCharacter.marker && masterCharacter.marker.destinationTile != null && masterCharacter.marker.isMoving && masterCharacter.marker.pathfindingAI.currentPath != null)
		{
			Log log4 = GameManager.CreateNewLogUsingNewLocalization(GameManager.Instance.Today(), "Character", "CharacterGeneric_Table", "Thought_Bubble_Travelling");
			log4.AddToFillers(_owner, _owner.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
			log4.AddToFillers(_owner.carryComponent.masterCharacter.marker.destinationTile.structure, _owner.carryComponent.masterCharacter.marker.destinationTile.structure.GetNameRelativeTo(_owner), LOG_IDENTIFIER.LANDMARK_1, replaceExisting: true, overrideStringValue: true);
			string logText4 = log4.logText;
			LogPool.Release(log4);
			return logText4;
		}
		if (_owner.traitContainer.HasTrait("Quarantined"))
		{
			Log log5 = GameManager.CreateNewLogUsingNewLocalization(GameManager.Instance.Today(), "Character", "CharacterGeneric_Table", "Thought_Bubble_Quarantined");
			log5.AddToFillers(_owner, _owner.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
			string logText5 = log5.logText;
			LogPool.Release(log5);
			return logText5;
		}
		if (_owner.traitContainer.HasTrait("Being Drained"))
		{
			Log log6 = GameManager.CreateNewLogUsingNewLocalization(GameManager.Instance.Today(), "Character", "CharacterGeneric_Table", "Thought_Bubble_Being_Drained");
			log6.AddToFillers(_owner, _owner.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
			string logText6 = log6.logText;
			LogPool.Release(log6);
			return logText6;
		}
		if (_owner.currentStructure != null)
		{
			Log log7 = GameManager.CreateNewLogUsingNewLocalization(GameManager.Instance.Today(), "Character", "CharacterGeneric_Table", "Thought_Bubble_Default");
			log7.AddToFillers(_owner, _owner.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
			log7.AddToFillers(_owner.currentStructure, _owner.currentStructure.GetNameRelativeTo(_owner), LOG_IDENTIFIER.LANDMARK_1, replaceExisting: true, overrideStringValue: true);
			string logText7 = log7.logText;
			LogPool.Release(log7);
			return logText7;
		}
		if (_owner.minion != null && !_owner.minion.isSummoned)
		{
			Log log8 = GameManager.CreateNewLogUsingNewLocalization(GameManager.Instance.Today(), "Character", "CharacterGeneric_Table", "Thought_Bubble_Unsummoned");
			log8.AddToFillers(_owner, _owner.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
			string logText8 = log8.logText;
			LogPool.Release(log8);
			return logText8;
		}
		Log log9 = GameManager.CreateNewLogUsingNewLocalization(GameManager.Instance.Today(), "Character", "CharacterGeneric_Table", "Thought_Bubble_Default");
		log9.AddToFillers(_owner, _owner.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
		log9.AddToFillers(null, _owner.currentRegion?.name, LOG_IDENTIFIER.LANDMARK_1);
		string logText9 = log9.logText;
		LogPool.Release(log9);
		return logText9;
	}

	public string GetCharacterStringIcon()
	{
		if (_owner.characterClass.className == "Necromancer")
		{
			return Utilities.UndeadIcon();
		}
		if (!_owner.isNormalCharacter)
		{
			RaceData raceData = RaceManager.Instance.GetRaceData(_owner.race);
			if (_owner.minion != null || (_owner.faction != null && _owner.faction.isPlayerFaction))
			{
				return Utilities.DemonIcon();
			}
			Faction faction = _owner.faction;
			if (faction != null && faction.factionType.type == FACTION_TYPE.Undead)
			{
				return Utilities.UndeadIcon();
			}
			if (raceData.category == CHARACTER_CATEGORY.Undead)
			{
				return Utilities.UndeadIcon();
			}
			return Utilities.MonsterIcon();
		}
		if (_owner.traitContainer.HasTrait("Demon Cultist"))
		{
			return Utilities.CultistIcon();
		}
		if (_owner.race == RACE.RATMAN)
		{
			return Utilities.RatmanIcon();
		}
		return Utilities.VillagerIcon();
	}

	public string GetCharacterNameWithIconAndColor()
	{
		string characterStringIcon = GetCharacterStringIcon();
		string firstNameWithColor = _owner.firstNameWithColor;
		return characterStringIcon + firstNameWithColor;
	}

	public string GetRelationshipSummary(Character character)
	{
		if (_owner.relationshipContainer.HasRelationshipWith(character))
		{
			string localizedRelationshipNameWith = _owner.relationshipContainer.GetLocalizedRelationshipNameWith(character);
			int totalOpinion = _owner.relationshipContainer.GetTotalOpinion(character);
			int totalOpinion2 = character.relationshipContainer.GetTotalOpinion(_owner);
			string text = "<color=" + BaseRelationshipContainer.OpinionColor(totalOpinion) + ">" + GetOpinionText(totalOpinion);
			string text2 = "<color=" + BaseRelationshipContainer.OpinionColor(totalOpinion2) + ">" + GetOpinionText(totalOpinion2);
			return localizedRelationshipNameWith + "  " + character.visuals.GetCharacterNameWithIconAndColor() + "  " + text + "(" + text2 + ")";
		}
		return _owner.visuals.GetCharacterNameWithIconAndColor() + " doesn't have a relationship with " + character.visuals.GetCharacterNameWithIconAndColor() + "\n";
	}

	private string GetOpinionText(int number)
	{
		if (number < 0)
		{
			return number.ToString() ?? "";
		}
		return "+" + number;
	}

	public string GetBothWayRelationshipSummary(Character otherCharacter)
	{
		string relationshipSummary = GetRelationshipSummary(otherCharacter);
		string relationshipSummary2 = otherCharacter.visuals.GetRelationshipSummary(_owner);
		string text = string.Empty;
		if (!string.IsNullOrEmpty(relationshipSummary))
		{
			text = text + relationshipSummary + "\n";
		}
		if (!string.IsNullOrEmpty(relationshipSummary2))
		{
			text = text + relationshipSummary2 + "\n";
		}
		return text;
	}

	public void CleanUp()
	{
		markerAnimations = null;
		defaultSprite = null;
	}
}
