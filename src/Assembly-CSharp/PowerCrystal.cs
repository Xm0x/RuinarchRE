using System;
using System.Collections.Generic;
using UtilityScripts;

public class PowerCrystal : TileObject
{
	public List<RESISTANCE> resistanceBonuses = new List<RESISTANCE>();

	public float amountBonusResistance;

	public float amountBonusPiercing;

	public bool hasScheduleDestruction { get; private set; }

	public GameDate destroyDate { get; private set; }

	public override Type serializedData => typeof(SaveDataPowerCrystal);

	public PowerCrystal()
	{
		Initialize(TILE_OBJECT_TYPE.POWER_CRYSTAL);
		base.maxHP = 1000;
		base.currentHP = base.maxHP;
		AddAdvertisedAction(INTERACTION_TYPE.ABSORB_POWER_CRYSTAL);
		if (GameUtilities.RandomBetweenTwoNumbers(1, 100) <= 30)
		{
			amountBonusPiercing = 5f;
			return;
		}
		amountBonusResistance = 10f;
		EquipmentBonusProcessor.SetBonusResistanceOnPowerCrystal(this);
	}

	public PowerCrystal(SaveDataPowerCrystal data)
		: base(data)
	{
		hasScheduleDestruction = data.hasScheduleDestruction;
		destroyDate = data.destroyDate;
		data.resistanceBonuses.ForEach(delegate(RESISTANCE eachResistance)
		{
			resistanceBonuses.Add(eachResistance);
		});
		amountBonusPiercing = data.bonusPiercing;
		amountBonusResistance = data.bonusResistance;
		if (hasScheduleDestruction)
		{
			SchedulingManager.Instance.AddEntry(destroyDate, TryExpire, this);
		}
	}

	public override void OnPlacePOI()
	{
		base.OnPlacePOI();
		ScheduleExpiry();
	}

	public void ScheduleExpiry()
	{
		if (!hasScheduleDestruction)
		{
			hasScheduleDestruction = true;
			destroyDate = GameManager.Instance.Today().AddTicks(GameManager.Instance.GetTicksBasedOnHour(2));
			SchedulingManager.Instance.AddEntry(destroyDate, TryExpire, this);
		}
	}

	private void TryExpire()
	{
		bool flag = true;
		if (base.isBeingSeized)
		{
			flag = false;
		}
		if (flag)
		{
			Expire();
			return;
		}
		hasScheduleDestruction = false;
		ScheduleExpiry();
	}

	private void Expire()
	{
		if (base.isBeingCarriedBy != null)
		{
			base.isBeingCarriedBy.DropItem(this);
		}
		if (gridTileLocation != null)
		{
			gridTileLocation.structure.RemovePOI(this);
		}
		hasScheduleDestruction = false;
	}

	public override void GeneralReactionToTileObject(Character actor, ref string debugLog)
	{
		base.GeneralReactionToTileObject(actor, ref debugLog);
		if (actor.race == RACE.ELVES && !HasJobTargetingThis(JOB_TYPE.ABSORB_CRYSTAL) && !actor.jobComponent.HasHigherPriorityJobThan(JOB_TYPE.ABSORB_CRYSTAL))
		{
			actor.jobComponent.TriggerAbsorbPowerCrystal(this);
		}
	}
}
