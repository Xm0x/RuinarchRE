using System.Collections.Generic;
using UnityEngine;

namespace Traits;

public class Berserked : Status
{
	private Character _owner;

	public Berserked()
	{
		name = "Berserked";
		description = "Mentally absent and is just rampaging like crazy.";
		type = TRAIT_TYPE.STATUS;
		effect = TRAIT_EFFECT.NEGATIVE;
		ticksDuration = GameManager.Instance.GetTicksBasedOnHour(6);
		hindersWitness = true;
		hindersSocials = true;
		AddTraitOverrideFunctionIdentifier("Initiate_Map_Visual_Trait");
		AddTraitOverrideFunctionIdentifier("Destroy_Map_Visual_Trait");
	}

	public override void OnAddTrait(ITraitable addedTo)
	{
		base.OnAddTrait(addedTo);
		if (addedTo is Character character)
		{
			_owner = character;
			if ((bool)character.marker)
			{
				character.marker.BerserkedMarker();
				character.marker.visionColliderComponent.VoteToUnFilterVision();
			}
			character.jobQueue.CancelAllJobs();
			character.behaviourComponent.AddBehaviourComponent(typeof(BerserkBehaviour));
		}
	}

	public override void LoadTraitOnLoadTraitContainer(ITraitable addTo)
	{
		base.LoadTraitOnLoadTraitContainer(addTo);
		if (addTo is Character character)
		{
			_owner = character;
			if ((bool)character.marker)
			{
				character.marker.BerserkedMarker();
				character.marker.visionColliderComponent.VoteToUnFilterVision();
			}
		}
	}

	public override void OnRemoveTrait(ITraitable removedFrom, Character removedBy)
	{
		base.OnRemoveTrait(removedFrom, removedBy);
		if (!(removedFrom is Character character))
		{
			return;
		}
		if ((bool)character.marker)
		{
			if (!character.traitContainer.HasTrait("Agitated"))
			{
				character.marker.UnberserkedMarker();
			}
			character.marker.visionColliderComponent.VoteToFilterVision();
		}
		List<IPointOfInterest> list = new List<IPointOfInterest>();
		for (int i = 0; i < character.combatComponent.hostilesInRange.Count; i++)
		{
			IPointOfInterest pointOfInterest = character.combatComponent.hostilesInRange[i];
			if (pointOfInterest is Character)
			{
				Character character2 = pointOfInterest as Character;
				if (!character.IsHostileWith(character2) || character2.isDead)
				{
					list.Add(character2);
				}
			}
			else
			{
				list.Add(pointOfInterest);
			}
		}
		for (int j = 0; j < list.Count; j++)
		{
			IPointOfInterest poi = list[j];
			character.combatComponent.RemoveHostileInRange(poi);
		}
		character.behaviourComponent.RemoveBehaviourComponent(typeof(BerserkBehaviour));
		character.needsComponent.CheckExtremeNeeds();
		_owner = null;
	}

	public override void OnInitiateMapObjectVisual(ITraitable traitable)
	{
		if (traitable is Character character && (bool)character.marker)
		{
			character.marker.BerserkedMarker();
		}
	}

	public override void OnDestroyMapObjectVisual(ITraitable traitable)
	{
		if (_owner != null && (bool)_owner.marker)
		{
			if (!_owner.traitContainer.HasTrait("Agitated"))
			{
				_owner.marker.UnberserkedMarker();
			}
			_owner.marker.visionColliderComponent.VoteToFilterVision();
		}
	}

	public void BerserkCombat(IPointOfInterest targetPOI, Character character)
	{
		if (targetPOI is Character)
		{
			Character character2 = targetPOI as Character;
			if (!character2.isDead)
			{
				if (character.faction.isPlayerFaction)
				{
					character.combatComponent.Fight(character2, "Berserked");
				}
				else if (!character2.traitContainer.HasTrait("Unconscious"))
				{
					character.combatComponent.Fight(character2, "Berserked", null, isLethal: false);
				}
			}
		}
		else if (targetPOI is TileObject && Random.Range(0, 100) < 35)
		{
			character.combatComponent.Fight(targetPOI, "Berserked", null, isLethal: false);
		}
	}

	public override void CheckIfCharacterIsStillReferenced(Character p_character)
	{
		base.CheckIfCharacterIsStillReferenced(p_character);
		_ = _owner;
	}
}
