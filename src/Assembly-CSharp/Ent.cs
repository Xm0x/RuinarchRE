using System;
using Inner_Maps;
using UtilityScripts;

public abstract class Ent : Summon
{
	private Action<Ent> _awakenEntEvent;

	public override Type serializedData => typeof(SaveDataEnt);

	public bool isTree { get; private set; }

	protected Ent(SUMMON_TYPE summonType, string className)
		: base(summonType, className, RACE.ENT, Utilities.GetRandomGender())
	{
	}

	protected Ent(SaveDataEnt data)
		: base(data)
	{
		isTree = data.isTree;
	}

	public override void Initialize()
	{
		base.Initialize();
		SetDestroyMarkerOnDeath(state: true);
	}

	protected override void AfterAdjustHP(int amount, ELEMENTAL_TYPE elementalDamageType)
	{
		base.AfterAdjustHP(amount, elementalDamageType);
		if (amount >= 0 || base.isDead)
		{
			return;
		}
		if (!base.faction.isPlayerFaction)
		{
			if (elementalDamageType == ELEMENTAL_TYPE.Fire)
			{
				base.combatComponent.SetCombatMode(COMBAT_MODE.Aggressive);
			}
			else
			{
				base.combatComponent.SetCombatMode(COMBAT_MODE.Defend);
			}
			base.jobQueue.GetJob(JOB_TYPE.STAND_STILL)?.ForceCancelJob();
		}
		if (isTree)
		{
			ExecuteAwakenEntEvent();
		}
	}

	protected override void AfterDeath(LocationGridTile deathTileLocation)
	{
		base.AfterDeath(deathTileLocation);
		if (deathTileLocation != null && deathTileLocation.structure.structureType != STRUCTURE_TYPE.KENNEL && deathTileLocation.structure.structureType != STRUCTURE_TYPE.TORTURE_CHAMBERS)
		{
			LocationGridTile locationGridTile = deathTileLocation;
			if (deathTileLocation.tileObjectComponent.objHere != null)
			{
				locationGridTile = deathTileLocation.GetFirstNearestTileFromThisWithNoObject();
			}
			int resourceInPile = 300;
			WoodPile woodPile = InnerMapManager.Instance.CreateNewTileObject<WoodPile>(TILE_OBJECT_TYPE.WOOD_PILE);
			woodPile.SetResourceInPile(resourceInPile);
			locationGridTile.structure.AddPOI(woodPile, locationGridTile);
		}
	}

	protected override void OnTickEnded()
	{
		if (!isTree)
		{
			base.OnTickEnded();
		}
	}

	protected override void OnTickStarted()
	{
		if (!isTree)
		{
			base.OnTickStarted();
		}
	}

	public override void OnSeizePOI(bool wasUnseizedFromCharacter)
	{
		if (isTree)
		{
			ExecuteAwakenEntEvent();
		}
		base.OnSeizePOI(wasUnseizedFromCharacter);
	}

	public void EntAgitatedHandling()
	{
		if (isTree)
		{
			ExecuteAwakenEntEvent();
		}
	}

	public void SetIsTree(bool state)
	{
		isTree = state;
		if (isTree)
		{
			base.traitContainer.AddTrait(this, "Hidden");
		}
		else
		{
			base.traitContainer.RemoveTrait(this, "Hidden");
		}
	}

	public void SubscribeToAwakenEntEvent(TreeObject p_tree)
	{
		_awakenEntEvent = (Action<Ent>)Delegate.Combine(_awakenEntEvent, new Action<Ent>(p_tree.TryAwakenEnt));
	}

	public void UnsubscribeToAwakenEntEvent(TreeObject p_tree)
	{
		_awakenEntEvent = (Action<Ent>)Delegate.Remove(_awakenEntEvent, new Action<Ent>(p_tree.TryAwakenEnt));
	}

	private void ExecuteAwakenEntEvent()
	{
		_awakenEntEvent?.Invoke(this);
	}

	public override bool Agitate(ref JobQueueItem p_agitateJob)
	{
		return AgitateAttackNearbyVillager(ref p_agitateJob);
	}

	protected override string GetAgitateTooltipKey()
	{
		return AGITATE_MESSAGE_TYPE.Attack_Villager_Tooltip.ToStringEnum();
	}

	public override void CleanUp()
	{
		base.CleanUp();
		_awakenEntEvent = null;
	}
}
