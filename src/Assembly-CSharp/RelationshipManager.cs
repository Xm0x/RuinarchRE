using System;
using System.Collections.Generic;
using Traits;
using UnityEngine;

public class RelationshipManager : BaseMonoBehaviour
{
	public static RelationshipManager Instance;

	private IRelationshipValidator _characterRelationshipValidator;

	private IRelationshipProcessor _characterRelationshipProcessor;

	public const string Close_Friend = "Close Friend";

	public const string Friend = "Friend";

	public const string Acquaintance = "Acquaintance";

	public const string Enemy = "Enemy";

	public const string Rival = "Rival";

	public const int MaxCompatibility = 5;

	public const int MinCompatibility = 0;

	private void Awake()
	{
		Instance = this;
		_characterRelationshipValidator = new CharacterRelationshipValidator();
		_characterRelationshipProcessor = new CharacterRelationshipProcessor();
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		Instance = null;
	}

	public IRelationshipContainer CreateRelationshipContainer(Relatable relatable)
	{
		if (relatable is IPointOfInterest)
		{
			return new BaseRelationshipContainer();
		}
		return null;
	}

	public IRelationshipValidator GetValidator(Relatable obj)
	{
		if (obj is Character)
		{
			return _characterRelationshipValidator;
		}
		throw new Exception("There is no relationship validator for " + obj.relatableName);
	}

	public bool CanHaveRelationship(Relatable rel1, Relatable rel2, RELATIONSHIP_TYPE rel)
	{
		return GetValidator(rel1)?.CanHaveRelationship(rel1, rel2, rel) ?? false;
	}

	private RELATIONSHIP_TYPE GetPairedRelationship(RELATIONSHIP_TYPE rel)
	{
		return rel switch
		{
			RELATIONSHIP_TYPE.RELATIVE => RELATIONSHIP_TYPE.RELATIVE, 
			RELATIONSHIP_TYPE.LOVER => RELATIONSHIP_TYPE.LOVER, 
			RELATIONSHIP_TYPE.AFFAIR => RELATIONSHIP_TYPE.AFFAIR, 
			RELATIONSHIP_TYPE.EX_LOVER => RELATIONSHIP_TYPE.EX_LOVER, 
			RELATIONSHIP_TYPE.MASTER => RELATIONSHIP_TYPE.PET, 
			RELATIONSHIP_TYPE.PET => RELATIONSHIP_TYPE.MASTER, 
			_ => RELATIONSHIP_TYPE.NONE, 
		};
	}

	public bool IsCompatibleBasedOnSexualityAndOpinion(Character p_character1, Character p_character2)
	{
		return p_character1.traitContainer.GetTraitOrStatus<Unfaithful>("Unfaithful")?.IsCompatibleBasedOnSexualityAndOpinions(p_character1, p_character2) ?? IsSexuallyCompatible(p_character1, p_character2);
	}

	public bool CanHaveRelationship(Character p_character1, Character p_character2, RELATIONSHIP_TYPE p_rel)
	{
		if (GetValidator(p_character1).CanHaveRelationship(p_character2, p_character1, p_rel) && GetValidator(p_character2).CanHaveRelationship(p_character1, p_character2, p_rel))
		{
			return true;
		}
		return false;
	}

	public static bool IsSexuallyCompatible(Character character1, Character character2)
	{
		if (IsSexuallyCompatibleOneSided(character1, character2))
		{
			return IsSexuallyCompatibleOneSided(character2, character1);
		}
		return false;
	}

	public static bool IsSexuallyCompatibleOneSided(Character character1, Character character2)
	{
		if (IsSexuallyCompatibleOneSided(character1.sexuality, character2.sexuality, character1.gender, character2.gender))
		{
			if (character1.traitContainer.HasTrait("Hemophobic"))
			{
				Vampire traitOrStatus = character2.traitContainer.GetTraitOrStatus<Vampire>("Vampire");
				if (traitOrStatus != null && traitOrStatus.DoesCharacterKnowThisVampire(character1))
				{
					return false;
				}
			}
			if (character1.traitContainer.HasTrait("Lycanphobic") && character2.isLycanthrope && character2.lycanData.DoesCharacterKnowThisLycan(character1))
			{
				return false;
			}
			return true;
		}
		return false;
	}

	public static bool IsSexuallyCompatible(SEXUALITY sexuality1, SEXUALITY sexuality2, GENDER gender1, GENDER gender2)
	{
		if (!IsSexuallyCompatibleOneSided(sexuality1, sexuality2, gender1, gender2))
		{
			return false;
		}
		return IsSexuallyCompatibleOneSided(sexuality2, sexuality1, gender1, gender2);
	}

	private static bool IsSexuallyCompatibleOneSided(SEXUALITY sexuality1, SEXUALITY sexuality2, GENDER gender1, GENDER gender2)
	{
		return sexuality1 switch
		{
			SEXUALITY.STRAIGHT => gender1 != gender2, 
			SEXUALITY.BISEXUAL => true, 
			SEXUALITY.GAY => gender1 == gender2, 
			_ => false, 
		};
	}

	public void ApplyPreGeneratedRelationships(PreCharacterData characterData, Character character)
	{
		foreach (KeyValuePair<int, PreCharacterRelationship> relationship in characterData.relationships)
		{
			PreCharacterData characterWithID = DatabaseManager.Instance.familyTreeDatabase.GetCharacterWithID(relationship.Key);
			IRelationshipData orCreateRelationshipDataWith = character.relationshipContainer.GetOrCreateRelationshipDataWith(character, characterWithID.id, characterWithID.firstName, characterWithID.gender);
			character.relationshipContainer.SetOpinion(character, characterWithID.id, characterWithID.firstName, characterWithID.gender, "Base", relationship.Value.baseOpinion, isInitial: true);
			orCreateRelationshipDataWith.opinions.SetCompatibilityValue(relationship.Value.compatibility);
			for (int i = 0; i < relationship.Value.relationships.Count; i++)
			{
				RELATIONSHIP_TYPE relType = relationship.Value.relationships[i];
				orCreateRelationshipDataWith.AddRelationship(relType);
			}
		}
	}

	public IRelationshipData CreateNewRelationshipBetween(Relatable rel1, Relatable rel2, RELATIONSHIP_TYPE rel)
	{
		RELATIONSHIP_TYPE pairedRelationship = GetPairedRelationship(rel);
		if (CanHaveRelationship(rel1, rel2, rel) && CanHaveRelationship(rel2, rel1, rel))
		{
			switch (rel)
			{
			case RELATIONSHIP_TYPE.AFFAIR:
			{
				Log log2 = GameManager.CreateNewLogUsingNewLocalization(GameManager.Instance.Today(), "Character", "CharacterAlerts_Table", "Affair", LOG_TAG.Social, LOG_TAG.Life_Changes);
				log2.AddToFillers(rel1 as Character, rel1.relatableName, LOG_IDENTIFIER.ACTIVE_CHARACTER);
				log2.AddToFillers(rel2 as Character, rel2.relatableName, LOG_IDENTIFIER.TARGET_CHARACTER);
				log2.AddLogToDatabase(releaseLogAfter: true);
				break;
			}
			case RELATIONSHIP_TYPE.LOVER:
			{
				Log log = GameManager.CreateNewLogUsingNewLocalization(GameManager.Instance.Today(), "Character", "CharacterAlerts_Table", "Lover", LOG_TAG.Social, LOG_TAG.Life_Changes);
				log.AddToFillers(rel1 as Character, rel1.relatableName, LOG_IDENTIFIER.ACTIVE_CHARACTER);
				log.AddToFillers(rel2 as Character, rel2.relatableName, LOG_IDENTIFIER.TARGET_CHARACTER);
				log.AddLogToDatabase(releaseLogAfter: true);
				break;
			}
			}
			rel1.relationshipContainer.AddRelationship(rel1, rel2, rel);
			rel1.relationshipProcessor?.OnRelationshipAdded(rel1, rel2, rel);
			rel2.relationshipContainer.AddRelationship(rel2, rel1, pairedRelationship);
			rel2.relationshipProcessor?.OnRelationshipAdded(rel2, rel1, pairedRelationship);
		}
		return rel1.relationshipContainer.GetRelationshipDataWith(rel2);
	}

	public void CreateNewRelationshipDataBetween(Relatable rel1, Relatable rel2)
	{
		IRelationshipData orCreateRelationshipDataWith = rel1.relationshipContainer.GetOrCreateRelationshipDataWith(rel1, rel2);
		IRelationshipData orCreateRelationshipDataWith2 = rel2.relationshipContainer.GetOrCreateRelationshipDataWith(rel2, rel1);
		int compatibilityValue = UnityEngine.Random.Range(0, 5);
		orCreateRelationshipDataWith.opinions.SetCompatibilityValue(compatibilityValue);
		orCreateRelationshipDataWith2.opinions.SetCompatibilityValue(compatibilityValue);
		orCreateRelationshipDataWith.opinions.RandomizeBaseOpinionBasedOnCompatibility();
		orCreateRelationshipDataWith2.opinions.RandomizeBaseOpinionBasedOnCompatibility();
	}

	public void RemoveRelationshipBetween(Relatable rel1, Relatable rel2, RELATIONSHIP_TYPE rel)
	{
		if (rel1.relationshipContainer.relationships.ContainsKey(rel2.id) && rel2.relationshipContainer.relationships.ContainsKey(rel1.id))
		{
			RELATIONSHIP_TYPE pairedRelationship = GetPairedRelationship(rel);
			if (rel1.relationshipContainer.relationships[rel2.id].HasRelationship(rel) && rel2.relationshipContainer.relationships[rel1.id].HasRelationship(pairedRelationship))
			{
				rel1.relationshipContainer.RemoveRelationship(rel2, rel);
				rel1.relationshipProcessor?.OnRelationshipRemoved(rel1, rel2, rel);
				rel2.relationshipContainer.RemoveRelationship(rel1, pairedRelationship);
				rel2.relationshipProcessor?.OnRelationshipRemoved(rel2, rel1, pairedRelationship);
				Messenger.Broadcast(CharacterSignals.RELATIONSHIP_REMOVED, rel1, rel, rel2);
			}
		}
	}

	public bool RelationshipImprovement(Character actor, Character target, GoapAction cause = null)
	{
		if (actor.race == RACE.DEMON || target.race == RACE.DEMON || actor is Summon || target is Summon)
		{
			return false;
		}
		if (actor.hasBeenRaisedFromDead || target.hasBeenRaisedFromDead)
		{
			return false;
		}
		_ = "Relationship improvement between " + actor.name + " and " + target.name;
		return false;
	}

	public IRelationshipProcessor GetProcessor(Relatable relatable)
	{
		if (relatable is Character)
		{
			return _characterRelationshipProcessor;
		}
		return null;
	}

	public int GetCompatibilityBetween(Character character1, Character character2)
	{
		return character1.relationshipContainer.GetCompatibility(character2);
	}

	public int GetCompatibilityBetween(Character character1, int target)
	{
		return character1.relationshipContainer.GetCompatibility(target);
	}

	public FactionCriminalOpinionModifier CreateNewFactionCriminalOpinionModifier(CRIME_TYPE p_crimeType, CRIME_SEVERITY p_crimeSeverity, Character p_criminal)
	{
		return new FactionCriminalOpinionModifier(p_crimeType, p_crimeSeverity, p_criminal);
	}

	public NewcomerOpinionModifier CreateNewComerOpinionModifier(Character p_newComer)
	{
		return new NewcomerOpinionModifier(p_newComer);
	}
}
