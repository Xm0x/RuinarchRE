using System;
using System.Collections.Generic;
using Inner_Maps;
using Traits;
using UtilityScripts;

public class Disable : GoapAction
{
	private Action<ITraitable, Character> _traitableCallback;

	public Disable()
		: base(INTERACTION_TYPE.DISABLE)
	{
		base.actionLocationType = ACTION_LOCATION_TYPE.IN_PLACE;
		base.actionIconString = GoapActionStateDB.Magic_Icon;
		base.logTags = new LOG_TAG[2]
		{
			LOG_TAG.Life_Changes,
			LOG_TAG.Player
		};
		_traitableCallback = DisableEffect;
	}

	public override void Perform(ActualGoapNode goapNode)
	{
		base.Perform(goapNode);
		SetState("Disable Success", goapNode);
	}

	protected override int GetBaseCost(Character actor, IPointOfInterest target, JobQueueItem job, OtherData[] otherData)
	{
		return 10;
	}

	public override bool IsInvalidOnVision(ActualGoapNode node, out string reason)
	{
		reason = string.Empty;
		return false;
	}

	public void PreDisableSuccess(ActualGoapNode goapNode)
	{
		GameManager.Instance.CreateParticleEffectAt(goapNode.actor.gridTileLocation, PARTICLE_EFFECT.Disabler);
	}

	public void AfterDisableSuccess(ActualGoapNode goapNode)
	{
		List<LocationGridTile> list = RuinarchListPool<LocationGridTile>.Claim();
		List<ITraitable> list2 = RuinarchListPool<ITraitable>.Claim();
		Character actor = goapNode.actor;
		actor.gridTileLocation.PopulateTilesInRadius(list, 3, 0, includeCenterTile: true, includeTilesInDifferentStructure: true);
		for (int i = 0; i < list.Count; i++)
		{
			list[i].PopulateAliveTraitablesOnTile(list2);
		}
		TraitManager.Instance.PerformActionOnTraitables(list2, actor, _traitableCallback);
		RuinarchListPool<ITraitable>.Release(list2);
		RuinarchListPool<LocationGridTile>.Release(list);
		actor.AdjustHP(-actor.maxHP, ELEMENTAL_TYPE.Normal, triggerDeath: true);
	}

	private void DisableEffect(ITraitable traitable, Character actor)
	{
		if (traitable is Character character && actor.IsHostileWith(character))
		{
			character.traitContainer.AddTrait(traitable, "Ensnared", actor);
			Log log = GameManager.CreateNewLogUsingNewLocalization(GameManager.Instance.Today(), "GoapAction", "GoapActionsStrings_Table", "Disable effect", base.logTags);
			log.AddToFillers(actor, actor.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
			log.AddToFillers(character, character.name, LOG_IDENTIFIER.TARGET_CHARACTER);
			log.AddLogToDatabase(releaseLogAfter: true);
		}
	}
}
