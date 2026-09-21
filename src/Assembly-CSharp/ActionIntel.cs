using System;
using System.Collections.Generic;
using Crime_System;
using Maccima_Games.Util;
using UtilityScripts;

public class ActionIntel : IIntel, IDisposable
{
	public ActualGoapNode node { get; private set; }

	public IReactable reactable => node;

	public Log log
	{
		get
		{
			if (!node.isAssumption)
			{
				return node.descriptionLog;
			}
			return node.assumption.assumptionLog;
		}
	}

	public Character actor => node.actor;

	public IPointOfInterest target => node.target;

	public ActionIntel(ActualGoapNode node)
	{
		this.node = node;
		DatabaseManager.Instance.mainSQLDatabase.SetLogIntelState(log.persistentID, isIntel: true);
		node.SetIsIntel(p_state: true);
	}

	public ActionIntel(SaveDataActionIntel data)
	{
		node = DatabaseManager.Instance.actionDatabase.GetActionByPersistentID(data.node);
	}

	public string GetIntelInfoRelationshipText()
	{
		string text = string.Empty;
		Character character = actor;
		Character character2 = null;
		Character character3 = null;
		Character character4 = null;
		bool flag = false;
		bool flag2 = false;
		bool flag3 = false;
		if (node.action.goapType == INTERACTION_TYPE.MAKE_LOVE)
		{
			Character character5 = target as Character;
			bool flag4 = false;
			if (character5 != null && character5.traitComponent.obsessedCharacterIDs.Count > 0)
			{
				for (int i = 0; i < character5.traitComponent.obsessedCharacterIDs.Count; i++)
				{
					Character characterByPersistentID = CharacterManager.Instance.GetCharacterByPersistentID(character5.traitComponent.obsessedCharacterIDs[i]);
					if (characterByPersistentID != null && characterByPersistentID != character5 && characterByPersistentID != character)
					{
						if (character2 == null)
						{
							character2 = characterByPersistentID;
							flag = true;
							if (!string.IsNullOrEmpty(text))
							{
								text += "\n";
							}
							text += GetObsessedMakeLoveFeelingText(character2, character);
						}
						else if (character3 == null)
						{
							character3 = characterByPersistentID;
							flag2 = true;
							if (!string.IsNullOrEmpty(text))
							{
								text += "\n";
							}
							text += GetObsessedMakeLoveFeelingText(character2, character);
						}
						else if (character4 == null)
						{
							character4 = characterByPersistentID;
							flag3 = true;
							if (!string.IsNullOrEmpty(text))
							{
								text += "\n";
							}
							text += GetObsessedMakeLoveFeelingText(character2, character);
						}
					}
					if (character2 != null && character3 != null && character4 != null)
					{
						flag4 = true;
						break;
					}
				}
			}
			if (character != null && !flag4 && character.traitComponent.obsessedCharacterIDs.Count > 0)
			{
				for (int j = 0; j < character.traitComponent.obsessedCharacterIDs.Count; j++)
				{
					Character characterByPersistentID2 = CharacterManager.Instance.GetCharacterByPersistentID(character.traitComponent.obsessedCharacterIDs[j]);
					if (characterByPersistentID2 != null && characterByPersistentID2 != character5 && characterByPersistentID2 != character)
					{
						if (character2 == null)
						{
							character2 = characterByPersistentID2;
							flag = true;
							if (!string.IsNullOrEmpty(text))
							{
								text += "\n";
							}
							text += GetObsessedMakeLoveFeelingText(character2, character5);
						}
						else if (character3 == null)
						{
							character3 = characterByPersistentID2;
							flag2 = true;
							if (!string.IsNullOrEmpty(text))
							{
								text += "\n";
							}
							text += GetObsessedMakeLoveFeelingText(character2, character5);
						}
						else if (character4 == null)
						{
							character4 = characterByPersistentID2;
							flag3 = true;
							if (!string.IsNullOrEmpty(text))
							{
								text += "\n";
							}
							text += GetObsessedMakeLoveFeelingText(character2, character5);
						}
					}
					if (character2 != null && character3 != null && character4 != null)
					{
						flag4 = true;
						break;
					}
				}
			}
			if (flag4)
			{
				return text;
			}
		}
		Character character6 = null;
		Character character7 = null;
		Character character8 = null;
		Character character9 = null;
		Character character10 = null;
		for (int k = 0; k < character.relationshipContainer.charactersWithOpinion.Count; k++)
		{
			Character character11 = character.relationshipContainer.charactersWithOpinion[k];
			string opinionLabel = character.relationshipContainer.GetOpinionLabel(character11);
			if (character.relationshipContainer.HasRelationshipWith(character11, RELATIONSHIP_TYPE.LOVER))
			{
				if (CanBeConsideredWitnessInIntelInfoRelationshipText(character, character2, character3, character4, character11) && character2 == null)
				{
					character2 = character11;
				}
			}
			else if (character.relationshipContainer.HasRelationshipWith(character11, RELATIONSHIP_TYPE.AFFAIR))
			{
				if (CanBeConsideredWitnessInIntelInfoRelationshipText(character, character2, character3, character4, character11) && character6 == null)
				{
					character6 = character11;
				}
			}
			else if (character.relationshipContainer.IsFamilyMember(character11))
			{
				if (CanBeConsideredWitnessInIntelInfoRelationshipText(character, character2, character3, character4, character11) && character7 == null)
				{
					character7 = character11;
				}
			}
			else if (opinionLabel == "Close Friend")
			{
				if (CanBeConsideredWitnessInIntelInfoRelationshipText(character, character2, character3, character4, character11) && character8 == null)
				{
					character8 = character11;
				}
			}
			else if (opinionLabel == "Friend")
			{
				if (CanBeConsideredWitnessInIntelInfoRelationshipText(character, character2, character3, character4, character11) && character9 == null)
				{
					character9 = character11;
				}
			}
			else if (CanBeConsideredWitnessInIntelInfoRelationshipText(character, character2, character3, character4, character11) && character10 == null)
			{
				character10 = character11;
			}
			if (character3 == null && opinionLabel == "Acquaintance" && CanBeConsideredWitnessInIntelInfoRelationshipText(character, character2, character3, character4, character11) && character11 != character6 && character11 != character7 && character11 != character8 && character11 != character9 && character11 != character10)
			{
				character3 = character11;
			}
			if (character4 == null && CanBeConsideredWitnessInIntelInfoRelationshipText(character, character2, character3, character4, character11) && character11 != character6 && character11 != character7 && character11 != character8 && character11 != character9 && character11 != character10)
			{
				character4 = character11;
			}
		}
		if (character2 == null)
		{
			if (character6 != null)
			{
				character2 = character6;
			}
			else if (character7 != null)
			{
				character2 = character7;
			}
			else if (character8 != null)
			{
				character2 = character8;
			}
			else if (character9 != null)
			{
				character2 = character9;
			}
			else if (character10 != null)
			{
				character2 = character10;
			}
		}
		List<EMOTION> list = RuinarchListPool<EMOTION>.Claim(5);
		if (character2 != null && !flag)
		{
			list.Clear();
			if ((node.action.goapType == INTERACTION_TYPE.IS_IMPRISONED || node.action.goapType == INTERACTION_TYPE.IS_CAPTIVE) && !node.target.traitContainer.HasTrait("Restrained", "Prisoner"))
			{
				if (!string.IsNullOrEmpty(text))
				{
					text += "\n";
				}
				text += GetCaptiveImprisonedFeelingText(character2, node.target as Character);
			}
			else
			{
				if (character2 == target)
				{
					node.PopulateReactionsOfTarget(list, character, target, REACTION_STATUS.INFORMED);
				}
				else
				{
					node.PopulateReactionsToActor(list, character, target, character2, REACTION_STATUS.INFORMED);
				}
				string feelingTextInIntelInfoRelationshipText = GetFeelingTextInIntelInfoRelationshipText(list, character2, character);
				if (!string.IsNullOrEmpty(feelingTextInIntelInfoRelationshipText))
				{
					if (!string.IsNullOrEmpty(text))
					{
						text += "\n";
					}
					text += feelingTextInIntelInfoRelationshipText;
				}
			}
		}
		if (character3 != null && !flag2)
		{
			list.Clear();
			if ((node.action.goapType == INTERACTION_TYPE.IS_IMPRISONED || node.action.goapType == INTERACTION_TYPE.IS_CAPTIVE) && !node.target.traitContainer.HasTrait("Restrained", "Prisoner"))
			{
				if (!string.IsNullOrEmpty(text))
				{
					text += "\n";
				}
				text += GetCaptiveImprisonedFeelingText(character2, node.target as Character);
			}
			else
			{
				if (character3 == target)
				{
					node.PopulateReactionsOfTarget(list, character, target, REACTION_STATUS.INFORMED);
				}
				else
				{
					node.PopulateReactionsToActor(list, character, target, character3, REACTION_STATUS.INFORMED);
				}
				string feelingTextInIntelInfoRelationshipText2 = GetFeelingTextInIntelInfoRelationshipText(list, character3, character);
				if (!string.IsNullOrEmpty(feelingTextInIntelInfoRelationshipText2))
				{
					if (!string.IsNullOrEmpty(text))
					{
						text += "\n";
					}
					text += feelingTextInIntelInfoRelationshipText2;
				}
			}
		}
		if (character4 != null && !flag3)
		{
			list.Clear();
			if ((node.action.goapType == INTERACTION_TYPE.IS_IMPRISONED || node.action.goapType == INTERACTION_TYPE.IS_CAPTIVE) && !node.target.traitContainer.HasTrait("Restrained", "Prisoner"))
			{
				if (!string.IsNullOrEmpty(text))
				{
					text += "\n";
				}
				text += GetCaptiveImprisonedFeelingText(character2, node.target as Character);
			}
			else
			{
				if (character4 == target)
				{
					node.PopulateReactionsOfTarget(list, character, target, REACTION_STATUS.INFORMED);
				}
				else
				{
					node.PopulateReactionsToActor(list, character, target, character4, REACTION_STATUS.INFORMED);
				}
				string feelingTextInIntelInfoRelationshipText3 = GetFeelingTextInIntelInfoRelationshipText(list, character4, character);
				if (!string.IsNullOrEmpty(feelingTextInIntelInfoRelationshipText3))
				{
					if (!string.IsNullOrEmpty(text))
					{
						text += "\n";
					}
					text += feelingTextInIntelInfoRelationshipText3;
				}
			}
		}
		RuinarchListPool<EMOTION>.Release(list);
		return text;
	}

	public string GetIntelInfoBlackmailText()
	{
		string result = string.Empty;
		Character character = actor;
		if (CanBeUsedToBlackmailCharacter(character) && node.crimeType != CRIME_TYPE.None && node.crimeType != CRIME_TYPE.Unset)
		{
			CrimeType crimeType = CrimeManager.Instance.GetCrimeType(node.crimeType);
			if (GetBlackMailTypeConsideringTarget(character) != BLACKMAIL_TYPE.None)
			{
				Dictionary<string, string> dictionary = MaccimaDictionaryPool<string, string>.Claim();
				dictionary.Add("actorName", character.visuals.GetCharacterNameWithIconAndColor());
				dictionary.Add("crimeName", crimeType.localizedName);
				string localizedValue = LocalizationManager.Instance.GetLocalizedValue("UIStrings_Table", "Blackmail_Evidence", dictionary);
				MaccimaDictionaryPool<string, string>.Release(dictionary);
				result = localizedValue;
			}
		}
		return result;
	}

	private string GetFeelingTextInIntelInfoRelationshipText(List<EMOTION> emotions, Character witness, Character actor)
	{
		string text = string.Empty;
		if (emotions != null)
		{
			for (int i = 0; i < emotions.Count; i++)
			{
				if (i > 0)
				{
					text += "|";
				}
				text += CharacterManager.Instance.GetLocalizedEmotionText(emotions[i]);
			}
		}
		if (text != string.Empty)
		{
			Dictionary<string, string> dictionary = MaccimaDictionaryPool<string, string>.Claim();
			dictionary.Add("actorName", actor.visuals.GetCharacterNameWithIconAndColor());
			dictionary.Add("witnessName", witness.visuals.GetCharacterNameWithIconAndColor());
			dictionary.Add("emotions", Utilities.GetFirstFewEmotionsAndComafy(text, 2));
			string localizedValue = LocalizationManager.Instance.GetLocalizedValue("UIStrings_Table", "Intel_Emotion", dictionary);
			MaccimaDictionaryPool<string, string>.Release(dictionary);
			return localizedValue;
		}
		return string.Empty;
	}

	private string GetObsessedMakeLoveFeelingText(Character p_sourceOfObsession, Character p_thirdPartyCharacter)
	{
		Dictionary<string, string> dictionary = MaccimaDictionaryPool<string, string>.Claim();
		dictionary.Add("sourceOfObsession", p_sourceOfObsession.visuals.GetCharacterNameWithIconAndColor());
		dictionary.Add("thirdPartyCharacter", p_thirdPartyCharacter.visuals.GetCharacterNameWithIconAndColor());
		string localizedValue = LocalizationManager.Instance.GetLocalizedValue("UIStrings_Table", "Intel_Make_Love_Emotion", dictionary);
		MaccimaDictionaryPool<string, string>.Release(dictionary);
		return localizedValue;
	}

	private string GetCaptiveImprisonedFeelingText(Character p_sourceCharacter, Character p_targetCharacter)
	{
		Dictionary<string, string> dictionary = MaccimaDictionaryPool<string, string>.Claim();
		dictionary.Add("sourceCharacter", p_sourceCharacter.visuals.GetCharacterNameWithIconAndColor());
		dictionary.Add("targetCharacter", p_targetCharacter.visuals.GetCharacterNameWithIconAndColor());
		string localizedValue = LocalizationManager.Instance.GetLocalizedValue("ShareIntel_Table", "Intel_Hover_Captive_No_Care", dictionary);
		MaccimaDictionaryPool<string, string>.Release(dictionary);
		return localizedValue;
	}

	private bool CanBeConsideredWitnessInIntelInfoRelationshipText(Character actor, Character witness1, Character witness2, Character witness3, Character target)
	{
		if (target != null && !target.isDead && target != actor && target != witness1 && target != witness2)
		{
			return target != witness3;
		}
		return false;
	}

	public void OnIntelRemoved()
	{
		DatabaseManager.Instance.mainSQLDatabase.SetLogIntelState(log.persistentID, isIntel: false);
		node.SetIsIntel(p_state: false);
		if (node.isSupposedToBeInPool)
		{
			node.ProcessReturnToPool();
		}
	}

	public bool CanBeUsedToBlackmailCharacter(Character p_target)
	{
		if (node.isAssumption)
		{
			return false;
		}
		return GetBlackMailTypeConsideringTarget(p_target) != BLACKMAIL_TYPE.None;
	}

	public bool IsIntelConsideredACrimeByTarget(Character p_target)
	{
		CRIME_TYPE crimeType = node.crimeType;
		if (crimeType != CRIME_TYPE.None && crimeType != CRIME_TYPE.Unset && p_target.faction != null && p_target.faction.GetCrimeSeverity(actor, target, crimeType).IsConsideredACrime())
		{
			return true;
		}
		return false;
	}

	public BLACKMAIL_TYPE GetBlackMailTypeConsideringTarget(Character p_target)
	{
		if (p_target == actor)
		{
			CRIME_TYPE crimeType = node.crimeType;
			if (crimeType != CRIME_TYPE.None && crimeType != CRIME_TYPE.Unset && p_target.faction != null)
			{
				switch (p_target.faction.GetCrimeSeverity(actor, target, crimeType))
				{
				case CRIME_SEVERITY.Heinous:
					return BLACKMAIL_TYPE.Strong;
				case CRIME_SEVERITY.Serious:
					return BLACKMAIL_TYPE.Normal;
				case CRIME_SEVERITY.Infraction:
				case CRIME_SEVERITY.Misdemeanor:
					return BLACKMAIL_TYPE.Weak;
				}
			}
		}
		return node.action.GetBlackMailTypeConsideringTarget(node, p_target);
	}

	public string GetFullIntelTooltip()
	{
		string intelInfoBlackmailText = GetIntelInfoBlackmailText();
		string intelInfoRelationshipText = GetIntelInfoRelationshipText();
		string empty = string.Empty;
		empty += intelInfoBlackmailText;
		if (!string.IsNullOrEmpty(empty))
		{
			empty += "\n";
		}
		empty += intelInfoRelationshipText;
		if (!string.IsNullOrEmpty(empty))
		{
			return empty;
		}
		return LocalizationManager.Instance.GetLocalizedValue("UIStrings_Table", "Intel_Not_Useful");
	}

	public bool CanShareIntelTo(Character p_target)
	{
		if (node.action.goapType == INTERACTION_TYPE.IS_IMPRISONED || node.action.goapType == INTERACTION_TYPE.IS_CAPTIVE)
		{
			return p_target != target;
		}
		return true;
	}

	~ActionIntel()
	{
		ReleaseUnmanagedResources();
	}

	private void ReleaseUnmanagedResources()
	{
		node = null;
	}

	public void Dispose()
	{
		ReleaseUnmanagedResources();
		GC.SuppressFinalize(this);
	}
}
