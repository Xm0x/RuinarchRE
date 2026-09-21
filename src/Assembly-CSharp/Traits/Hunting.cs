using System;

namespace Traits;

public class Hunting : Status
{
	public Area targetArea { get; private set; }

	public override Type serializedData => typeof(SaveDataHunting);

	public Hunting()
	{
		name = "Hunting";
		description = "This is Dousing fires.";
		type = TRAIT_TYPE.STATUS;
		effect = TRAIT_EFFECT.NEUTRAL;
		ticksDuration = GameManager.Instance.GetTicksBasedOnHour(5);
		isHidden = true;
	}

	public override void LoadFirstWaveInstancedTrait(SaveDataTrait saveDataTrait)
	{
		base.LoadFirstWaveInstancedTrait(saveDataTrait);
		SaveDataHunting saveDataHunting = saveDataTrait as SaveDataHunting;
		targetArea = DatabaseManager.Instance.areaDatabase.GetAreaByPersistentID(saveDataHunting.targetAreaID);
	}

	public override void LoadTraitOnLoadTraitContainer(ITraitable addTo)
	{
		base.LoadTraitOnLoadTraitContainer(addTo);
		_ = addTo is Character;
	}

	public override void OnAddTrait(ITraitable addedTo)
	{
		base.OnAddTrait(addedTo);
		if (addedTo is Character character)
		{
			character.behaviourComponent.AddBehaviourComponent(typeof(HuntPreyBehaviour));
		}
	}

	public override void OnRemoveTrait(ITraitable removedFrom, Character removedBy)
	{
		base.OnRemoveTrait(removedFrom, removedBy);
		if (removedFrom is Character character)
		{
			character.behaviourComponent.RemoveBehaviourComponent(typeof(HuntPreyBehaviour));
		}
	}

	public override void OnCopyStatus(Status statusToCopy, ITraitable from, ITraitable to)
	{
		base.OnCopyStatus(statusToCopy, from, to);
		if (statusToCopy is Hunting hunting)
		{
			targetArea = hunting.targetArea;
		}
	}

	public void SetTargetArea(Area p_area)
	{
		targetArea = p_area;
	}
}
