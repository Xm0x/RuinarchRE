using System;
using Inner_Maps.Location_Structures;

namespace Traits;

public class Prisoner : Status
{
	public Character owner { get; private set; }

	public Faction prisonerOfFaction { get; private set; }

	public Character prisonerOfCharacter { get; private set; }

	public override Type serializedData => typeof(SaveDataPrisoner);

	public bool isFactionPrisoner => prisonerOfFaction != null;

	public bool isPersonalPrisoner => prisonerOfCharacter != null;

	public bool isPrisoner
	{
		get
		{
			if (!isFactionPrisoner)
			{
				return isPersonalPrisoner;
			}
			return true;
		}
	}

	public Prisoner()
	{
		name = "Prisoner";
		description = "Imprisoned";
		type = TRAIT_TYPE.STATUS;
		effect = TRAIT_EFFECT.NEGATIVE;
		ticksDuration = 0;
	}

	public override void LoadTraitOnLoadTraitContainer(ITraitable addTo)
	{
		base.LoadTraitOnLoadTraitContainer(addTo);
		if (addTo is Character)
		{
			owner = addTo as Character;
		}
	}

	public override void LoadSecondWaveInstancedTrait(SaveDataTrait p_saveDataTrait)
	{
		base.LoadSecondWaveInstancedTrait(p_saveDataTrait);
		SaveDataPrisoner saveDataPrisoner = p_saveDataTrait as SaveDataPrisoner;
		if (!string.IsNullOrEmpty(saveDataPrisoner.prisonerOfFaction))
		{
			prisonerOfFaction = FactionManager.Instance.GetFactionByPersistentID(saveDataPrisoner.prisonerOfFaction);
		}
		if (!string.IsNullOrEmpty(saveDataPrisoner.prisonerOfCharacter))
		{
			prisonerOfCharacter = CharacterManager.Instance.GetCharacterByPersistentID(saveDataPrisoner.prisonerOfCharacter);
		}
	}

	public override void OnAddTrait(ITraitable sourceCharacter)
	{
		base.OnAddTrait(sourceCharacter);
		if (sourceCharacter is Character)
		{
			owner = sourceCharacter as Character;
		}
	}

	public override void OnRemoveTrait(ITraitable removedFrom, Character removedBy)
	{
		base.OnRemoveTrait(removedFrom, removedBy);
		owner = null;
	}

	public override string GetTestingData(ITraitable traitable = null)
	{
		return string.Concat(base.GetTestingData(traitable) + "Prisoner of faction: " + prisonerOfFaction?.name, "\nPrisoner of character: ", prisonerOfCharacter?.name);
	}

	public override void OnCopyStatus(Status statusToCopy, ITraitable from, ITraitable to)
	{
		base.OnCopyStatus(statusToCopy, from, to);
		if (statusToCopy is Prisoner prisoner)
		{
			prisonerOfFaction = prisoner.prisonerOfFaction;
			prisonerOfCharacter = prisoner.prisonerOfCharacter;
		}
	}

	public override void DisconnectFromCharacter(IPointOfInterest p_owner, Character p_character)
	{
		base.DisconnectFromCharacter(p_owner, p_character);
		if (IsPersonalPrisonerOf(p_character))
		{
			owner.traitContainer.RemoveRestrainAndImprison(p_character);
		}
	}

	public bool IsFactionPrisonerOf(Faction faction)
	{
		if (isFactionPrisoner)
		{
			return prisonerOfFaction == faction;
		}
		return false;
	}

	public bool IsPersonalPrisonerOf(Character character)
	{
		if (isPersonalPrisoner)
		{
			return prisonerOfCharacter == character;
		}
		return false;
	}

	public bool IsConsideredPrisonerOf(Character character)
	{
		if (IsPersonalPrisonerOf(character))
		{
			return true;
		}
		if (character.faction != null && IsFactionPrisonerOf(character.faction))
		{
			return true;
		}
		return false;
	}

	public Faction GetFactionThatImprisoned()
	{
		Faction result = null;
		if (prisonerOfCharacter != null)
		{
			result = prisonerOfCharacter.faction;
		}
		else if (prisonerOfFaction != null)
		{
			result = prisonerOfFaction;
		}
		return result;
	}

	public void SetPrisonerOfFaction(Faction faction)
	{
		prisonerOfFaction = faction;
		if (prisonerOfFaction != null)
		{
			Messenger.Broadcast(TraitSignals.HAS_BECOME_PRISONER, this);
		}
	}

	public void SetPrisonerOfCharacter(Character character)
	{
		prisonerOfCharacter = character;
		if (prisonerOfCharacter != null)
		{
			Messenger.Broadcast(TraitSignals.HAS_BECOME_PRISONER, this);
		}
	}

	public LocationStructure GetIntendedPrisonAccordingTo(Character character)
	{
		if (prisonerOfCharacter != null && prisonerOfCharacter == character)
		{
			return character.homeStructure;
		}
		if (prisonerOfFaction != null && prisonerOfFaction == character.faction)
		{
			return character.GetSettlementPrisonFor(character);
		}
		return null;
	}

	public bool IsInIntendedPrisonAccordingTo(Character character)
	{
		LocationStructure intendedPrisonAccordingTo = GetIntendedPrisonAccordingTo(character);
		if (owner.currentStructure != null)
		{
			return owner.currentStructure == intendedPrisonAccordingTo;
		}
		return false;
	}

	public override void CheckIfCharacterIsStillReferenced(Character p_character)
	{
		base.CheckIfCharacterIsStillReferenced(p_character);
		_ = owner;
		_ = prisonerOfCharacter;
	}
}
