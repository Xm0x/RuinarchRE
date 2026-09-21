using System;
using System.Collections.Generic;
using Inner_Maps;
using Traits;
using UtilityScripts;

public class Cinder : TileObject
{
	private string _expiryScheduleKey;

	private Action<ITraitable> _traitableCallback;

	public int level { get; private set; }

	public GameDate expiryDate { get; private set; }

	public string expiryScheduleKey => _expiryScheduleKey;

	public override Type serializedData => typeof(SaveDataCinder);

	public Cinder()
	{
		Initialize(TILE_OBJECT_TYPE.CINDER, shouldAddCommonAdvertisements: false);
		AddAdvertisedAction(INTERACTION_TYPE.ASSAULT);
		AddAdvertisedAction(INTERACTION_TYPE.RESOLVE_COMBAT);
		_traitableCallback = ApplyExplosionEffect;
	}

	public Cinder(SaveDataCinder data)
		: base(data)
	{
		level = data.level;
		_traitableCallback = ApplyExplosionEffect;
	}

	public void SetLevel(int p_level)
	{
		level = p_level;
	}

	public override void OnRemoveTileObject(Character removedBy, LocationGridTile removedFrom, bool removeTraits = true, bool destroyTileSlots = true)
	{
		if (!string.IsNullOrEmpty(_expiryScheduleKey))
		{
			SchedulingManager.Instance.RemoveSpecificEntry(_expiryScheduleKey);
		}
		base.OnRemoveTileObject(removedBy, removedFrom, removeTraits, destroyTileSlots);
	}

	public override void OnPlacePOI()
	{
		base.OnPlacePOI();
		if (!base.traitContainer.HasTrait("Burning"))
		{
			base.traitContainer.AddTrait(this, "Burning", null, bypassElementalChance: false, 0);
		}
	}

	public override void OnLoadPlacePOI()
	{
		base.OnLoadPlacePOI();
		if (!base.traitContainer.HasTrait("Burning"))
		{
			base.traitContainer.AddTrait(this, "Burning", null, bypassElementalChance: false, 0);
		}
	}

	public void OnHitByIce()
	{
		LocationGridTile locationGridTile = gridTileLocation;
		if (locationGridTile != null)
		{
			SkillData spellData = PlayerSkillManager.Instance.GetSpellData(PLAYER_SKILL_TYPE.BRIMSTONES);
			if (spellData.HasAddedEffectForCurrentLevel(POWER_ADDED_EFFECT.Cinder_Explode))
			{
				Explode();
			}
			else if (!spellData.HasAddedEffectForCurrentLevel(POWER_ADDED_EFFECT.Cinder_Water_Ice_No_Die))
			{
				locationGridTile.structure.RemovePOI(this);
			}
		}
	}

	public void OnHitByWater()
	{
		LocationGridTile locationGridTile = gridTileLocation;
		if (locationGridTile != null)
		{
			SkillData spellData = PlayerSkillManager.Instance.GetSpellData(PLAYER_SKILL_TYPE.BRIMSTONES);
			if (spellData.HasAddedEffectForCurrentLevel(POWER_ADDED_EFFECT.Cinder_Explode))
			{
				Explode();
			}
			else if (!spellData.HasAddedEffectForCurrentLevel(POWER_ADDED_EFFECT.Cinder_Water_No_Die) && !spellData.HasAddedEffectForCurrentLevel(POWER_ADDED_EFFECT.Cinder_Water_Ice_No_Die))
			{
				locationGridTile.structure.RemovePOI(this);
			}
		}
	}

	public void Explode()
	{
		LocationGridTile locationGridTile = gridTileLocation;
		if (locationGridTile != null)
		{
			locationGridTile.structure.RemovePOI(this);
			GameManager.Instance.CreateParticleEffectAt(locationGridTile, PARTICLE_EFFECT.Fire_Explosion);
			List<ITraitable> list = RuinarchListPool<ITraitable>.Claim();
			locationGridTile.PopulateAliveTraitablesOnTile(list);
			for (int i = 0; i < locationGridTile.neighbourList.Count; i++)
			{
				locationGridTile.neighbourList[i].PopulateAliveTraitablesOnTile(list);
			}
			TraitManager.Instance.PerformActionOnTraitables(list, _traitableCallback);
			RuinarchListPool<ITraitable>.Release(list);
		}
	}

	private void ApplyExplosionEffect(ITraitable traitable)
	{
		traitable.AdjustHP(-100, ELEMENTAL_TYPE.Fire, triggerDeath: true, null, null, showHPBar: true);
	}

	public void SetExpiry(GameDate expiry)
	{
		expiryDate = expiry;
		_expiryScheduleKey = SchedulingManager.Instance.AddEntry(expiryDate, Expire, this);
	}

	private void Expire()
	{
		if (gridTileLocation != null)
		{
			gridTileLocation.structure.RemovePOI(this);
		}
	}
}
