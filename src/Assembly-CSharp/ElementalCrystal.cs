using System;
using System.Collections.Generic;
using Inner_Maps;
using Traits;
using UtilityScripts;

public abstract class ElementalCrystal : TileObject
{
	protected ELEMENTAL_TYPE elementalType;

	private Action<ITraitable> _traitableCallback;

	protected ElementalCrystal(ELEMENTAL_TYPE _elementalType)
	{
		elementalType = _elementalType;
		_traitableCallback = DealElementalDamage;
	}

	public ElementalCrystal(SaveDataTileObject data, ELEMENTAL_TYPE _elementalType)
		: base(data)
	{
		elementalType = _elementalType;
		_traitableCallback = DealElementalDamage;
	}

	public override void OnDestroyPOI(Character p_destroyer = null)
	{
		List<ITraitable> list = RuinarchListPool<ITraitable>.Claim();
		List<LocationGridTile> list2 = RuinarchListPool<LocationGridTile>.Claim();
		if (base.previousTile != null)
		{
			list2.Add(base.previousTile);
		}
		list2.AddRange(base.previousTile.neighbourList);
		for (int i = 0; i < list2.Count; i++)
		{
			list2[i].PopulateAliveTraitablesOnTile(list);
		}
		RuinarchListPool<LocationGridTile>.Release(list2);
		TraitManager.Instance.PerformActionOnTraitables(list, _traitableCallback);
		RuinarchListPool<ITraitable>.Release(list);
		base.OnDestroyPOI(p_destroyer);
	}

	private void DealElementalDamage(ITraitable traitable)
	{
		traitable.AdjustHP(-50, elementalType, triggerDeath: true, this, null, showHPBar: true);
	}
}
