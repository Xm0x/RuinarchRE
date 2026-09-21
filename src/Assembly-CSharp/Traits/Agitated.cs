using System;

namespace Traits;

public class Agitated : Status
{
	public float addedAttackPercent { get; private set; }

	public float addedHPPercent { get; private set; }

	public override Type serializedData => typeof(SaveDataAgitated);

	public Agitated()
	{
		name = "Agitated";
		description = "In a frenzied state.";
		type = TRAIT_TYPE.STATUS;
		effect = TRAIT_EFFECT.NEGATIVE;
		ticksDuration = 0;
		AddTraitOverrideFunctionIdentifier("Initiate_Map_Visual_Trait");
		AddTraitOverrideFunctionIdentifier("Destroy_Map_Visual_Trait");
	}

	public override void LoadFirstWaveInstancedTrait(SaveDataTrait saveDataTrait)
	{
		base.LoadFirstWaveInstancedTrait(saveDataTrait);
		SaveDataAgitated saveDataAgitated = saveDataTrait as SaveDataAgitated;
		addedAttackPercent = saveDataAgitated.addedAttackPercent;
		addedHPPercent = saveDataAgitated.addedHPPercent;
	}

	public override void OnAddTrait(ITraitable addedTo)
	{
		base.OnAddTrait(addedTo);
		if (addedTo is Character character)
		{
			if (character is Ent ent)
			{
				ent.EntAgitatedHandling();
			}
			else if (character is Mimic mimic)
			{
				mimic.MimicAgitatedHandling();
			}
			if ((bool)character.marker)
			{
				character.marker.BerserkedMarker();
			}
			addedAttackPercent = PlayerSkillManager.Instance.GetAdditionalAttackPercentagePerLevelBaseOnLevel(PLAYER_SKILL_TYPE.AGITATE);
			character.combatComponent.AdjustAttackPercentModifier(addedAttackPercent);
			addedHPPercent = PlayerSkillManager.Instance.GetAdditionalMaxHpPercentagePerLevelBaseOnLevel(PLAYER_SKILL_TYPE.AGITATE);
			character.combatComponent.AdjustMaxHPPercentModifier(addedHPPercent);
			character.movementComponent.SetEnableDigging(state: true);
		}
	}

	public override void LoadTraitOnLoadTraitContainer(ITraitable addTo)
	{
		base.LoadTraitOnLoadTraitContainer(addTo);
		if (addTo is Character character && (bool)character.marker)
		{
			character.marker.BerserkedMarker();
		}
	}

	public override void OnRemoveTrait(ITraitable removedFrom, Character removedBy)
	{
		base.OnRemoveTrait(removedFrom, removedBy);
		if (removedFrom is Character character)
		{
			if ((bool)character.marker && !character.traitContainer.HasTrait("Berserked"))
			{
				character.marker.UnberserkedMarker();
			}
			character.combatComponent.AdjustAttackPercentModifier(0f - addedAttackPercent);
			character.combatComponent.AdjustMaxHPPercentModifier(0f - addedHPPercent);
			character.movementComponent.SetEnableDigging(state: false);
		}
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
		if (traitable is Character character && (bool)character.marker && !character.traitContainer.HasTrait("Berserked"))
		{
			character.marker.UnberserkedMarker();
		}
	}
}
