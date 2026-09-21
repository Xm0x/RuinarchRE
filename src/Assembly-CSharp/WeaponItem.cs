using System;

public class WeaponItem : EquipmentItem
{
	public override Type serializedData => typeof(SaveDataWeaponItem);

	public WeaponItem()
	{
	}

	public WeaponItem(SaveDataEquipmentItem data)
		: base(data)
	{
	}

	public virtual void ApplyWeaponEffectsOnHit(IPointOfInterest p_targetPOI)
	{
	}

	public virtual void ApplyAttackPowerChanges(IPointOfInterest p_targetPOI, ref int attackPower)
	{
	}
}
