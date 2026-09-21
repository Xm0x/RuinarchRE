using System;
using System.Collections.Generic;

namespace Traits;

public class Criminal : Status
{
	public Character owner { get; private set; }

	public List<Character> charactersThatAreAlreadyWorried { get; private set; }

	public bool isImprisoned { get; private set; }

	public override Type serializedData => typeof(SaveDataCriminal);

	public Criminal()
	{
		name = "Criminal";
		description = "Has been witnessed or accused of doing something illegal.";
		type = TRAIT_TYPE.STATUS;
		effect = TRAIT_EFFECT.NEGATIVE;
		ticksDuration = 0;
		charactersThatAreAlreadyWorried = new List<Character>();
	}

	public override void LoadSecondWaveInstancedTrait(SaveDataTrait p_saveDataTrait)
	{
		base.LoadSecondWaveInstancedTrait(p_saveDataTrait);
		SaveDataCriminal saveDataCriminal = p_saveDataTrait as SaveDataCriminal;
		charactersThatAreAlreadyWorried = SaveUtilities.ConvertIDListToCharacters(saveDataCriminal.alreadyWorriedCharacterIDs);
		isImprisoned = saveDataCriminal.isImprisoned;
	}

	public override void LoadTraitOnLoadTraitContainer(ITraitable addTo)
	{
		base.LoadTraitOnLoadTraitContainer(addTo);
		if (addTo is Character)
		{
			owner = addTo as Character;
		}
	}

	public override void OnAddTrait(ITraitable sourcePOI)
	{
		base.OnAddTrait(sourcePOI);
		if (sourcePOI is Character)
		{
			owner = sourcePOI as Character;
			owner.CancelOrUnassignRemoveTraitRelatedJobs();
		}
	}

	public override void OnRemoveTrait(ITraitable sourcePOI, Character removedBy)
	{
		owner.ForceCancelAllJobsTargetingThisCharacter(JOB_TYPE.APPREHEND);
		owner.ForceCancelAllJobsTargetingThisCharacter(JOB_TYPE.APPREHEND_RESTRAINED);
		owner.crimeComponent.RemoveAllActiveCrimes(removeTrait: false);
		owner = null;
		base.OnRemoveTrait(sourcePOI, removedBy);
	}

	protected override string GetDescriptionInUI()
	{
		string text = base.GetDescriptionInUI();
		if (owner.crimeComponent.activeCrimes.Count > 0)
		{
			text = text + "\n\n" + LocalizationManager.Instance.GetLocalizedValue("Traits_Table", "Active_Crimes");
			for (int i = 0; i < owner.crimeComponent.activeCrimes.Count; i++)
			{
				CrimeData crimeData = owner.crimeComponent.activeCrimes[i];
				text = text + "\n" + crimeData.GetCrimeDataDescription();
			}
		}
		if (owner.crimeComponent.previousCrimes.Count > 0)
		{
			text = text + "\n\n" + LocalizationManager.Instance.GetLocalizedValue("Traits_Table", "Previous_Crimes");
			for (int j = 0; j < owner.crimeComponent.previousCrimes.Count; j++)
			{
				CrimeData crimeData2 = owner.crimeComponent.previousCrimes[j];
				text = text + "\n" + crimeData2.GetCrimeDataDescription();
			}
		}
		return text;
	}

	public override void OnCopyStatus(Status statusToCopy, ITraitable from, ITraitable to)
	{
		base.OnCopyStatus(statusToCopy, from, to);
		if (statusToCopy is Criminal criminal)
		{
			isImprisoned = criminal.isImprisoned;
			charactersThatAreAlreadyWorried.AddRange(criminal.charactersThatAreAlreadyWorried);
		}
	}

	public override void DisconnectFromCharacter(IPointOfInterest p_owner, Character p_character)
	{
		base.DisconnectFromCharacter(p_owner, p_character);
		charactersThatAreAlreadyWorried.Remove(p_character);
	}

	public void AddCharacterThatIsAlreadyWorried(Character character)
	{
		charactersThatAreAlreadyWorried.Add(character);
	}

	public bool HasCharacterThatIsAlreadyWorried(Character character)
	{
		return charactersThatAreAlreadyWorried.Contains(character);
	}

	public void SetIsImprisoned(bool state)
	{
		if (isImprisoned != state)
		{
			isImprisoned = state;
		}
	}

	public override void CheckIfCharacterIsStillReferenced(Character p_character)
	{
		base.CheckIfCharacterIsStillReferenced(p_character);
		_ = owner;
		charactersThatAreAlreadyWorried.Contains(p_character);
	}
}
