using System;
using System.Collections.Generic;
using Inner_Maps;
using UnityEngine;

public class BallLightning : MovingTileObject
{
	private BallLightningMapObjectVisual _ballLightningMapVisual;

	public override string neutralizer => "Thunder Master";

	public GameDate expiryDate { get; set; }

	public Vector3 targetDirection { get; set; }

	public override Type serializedData => typeof(SaveDataBallLightning);

	public BallLightning()
	{
		Initialize(TILE_OBJECT_TYPE.BALL_LIGHTNING, shouldAddCommonAdvertisements: false);
		base.traitContainer.RemoveTrait(this, "Flammable");
		AddAdvertisedAction(INTERACTION_TYPE.ASSAULT);
		AddAdvertisedAction(INTERACTION_TYPE.RESOLVE_COMBAT);
	}

	public BallLightning(SaveDataBallLightning data)
		: base(data)
	{
		expiryDate = data.expiryDate;
		base.hasExpired = data.hasExpired;
	}

	protected override void CreateMapObjectVisual()
	{
		base.CreateMapObjectVisual();
		_ballLightningMapVisual = mapVisual as BallLightningMapObjectVisual;
	}

	public override void Neutralize()
	{
		_ballLightningMapVisual.Expire();
	}

	public override void OnPlacePOI()
	{
		base.OnPlacePOI();
		base.traitContainer.AddTrait(this, "Dangerous");
		base.traitContainer.RemoveTrait(this, "Flammable");
	}

	public override string ToString()
	{
		return "Ball Lightning";
	}

	public override void AdjustHP(int amount, ELEMENTAL_TYPE elementalDamageType, bool triggerDeath = false, object source = null, CombatManager.ElementalTraitProcessor elementalTraitProcessor = null, bool showHPBar = false, float piercingPower = 0f, bool isPlayerSource = false, bool isTrueDamage = false)
	{
		if (base.currentHP == 0 && amount < 0)
		{
			return;
		}
		LocationGridTile locationGridTile = gridTileLocation;
		if (!isTrueDamage)
		{
			CombatManager.Instance.ModifyDamage(ref amount, elementalDamageType, piercingPower, this);
		}
		base.currentHP += amount;
		base.currentHP = Mathf.Clamp(base.currentHP, 0, base.maxHP);
		if (amount < 0)
		{
			Character characterResponsible = null;
			if (source is Character character)
			{
				characterResponsible = character;
			}
			CombatManager.Instance.ApplyElementalDamage(amount, elementalDamageType, this, characterResponsible, elementalTraitProcessor, createHitEffect: true, isPlayerSource, piercingPower);
		}
		if (amount < 0 && elementalDamageType == ELEMENTAL_TYPE.Ice)
		{
			List<AOESpellTileObject> activeSpellsOnTile = locationGridTile.parentMap.region.regionSpellsComponent.GetActiveSpellsOnTile(locationGridTile);
			ElectricStormTileObject electricStormTileObject = null;
			if (activeSpellsOnTile != null)
			{
				for (int i = 0; i < activeSpellsOnTile.Count; i++)
				{
					if (activeSpellsOnTile[i] is ElectricStormTileObject electricStormTileObject2)
					{
						electricStormTileObject = electricStormTileObject2;
						break;
					}
				}
			}
			if (electricStormTileObject != null)
			{
				electricStormTileObject.ResetElectricStormDuration();
			}
			else
			{
				ElectricStormTileObject poi = InnerMapManager.Instance.CreateNewTileObject<ElectricStormTileObject>(TILE_OBJECT_TYPE.ELECTRIC_STORM_TILE_OBJECT);
				locationGridTile.structure.AddPOI(poi, locationGridTile);
			}
			_ballLightningMapVisual.Expire();
		}
		else if (base.currentHP == 0)
		{
			_ballLightningMapVisual.Expire();
		}
	}

	protected override bool TryGetGridTileLocation(out LocationGridTile tile)
	{
		if (_ballLightningMapVisual != null && _ballLightningMapVisual.isSpawned)
		{
			tile = _ballLightningMapVisual.gridTileLocation;
			return true;
		}
		tile = null;
		return false;
	}

	public override void GeneralReactionToTileObject(Character actor, ref string debugLog)
	{
		base.GeneralReactionToTileObject(actor, ref debugLog);
		if (actor is Troll && base.traitContainer.HasTrait("Lightning Remnant"))
		{
			actor.combatComponent.Flight(this, "Saw_Frightening");
		}
	}
}
