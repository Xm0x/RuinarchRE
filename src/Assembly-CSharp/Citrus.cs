public class Citrus : WeaponItem
{
	public Citrus()
	{
		Initialize(TILE_OBJECT_TYPE.CITRUS, shouldAddCommonAdvertisements: false);
		equipmentData = EquipmentDataHandler.Instance.GetEquipmentDataBaseOnName(base.internalName);
		base.traitContainer.AddTrait(this, "Indestructible");
	}

	public Citrus(SaveDataEquipmentItem data)
		: base(data)
	{
	}

	public override void ApplyWeaponEffectsOnHit(IPointOfInterest p_targetPOI)
	{
		base.ApplyWeaponEffectsOnHit(p_targetPOI);
		Character character = base.isBeingCarriedBy;
		if (ChanceData.RollChance(CHANCE_TYPE.Citrus_Tame) && character != null && p_targetPOI is Character character2 && character2.IsUndead() && !character.petComponent.ownedPetsData.IsAtMaxCapacity() && !character2.isDead && !character2.traitContainer.HasTrait("Hibernating") && !character2.traitContainer.HasTrait("Temporal") && !character2.petComponent.HasPetOwner() && !character2.movementComponent.isStationary)
		{
			character.interruptComponent.TriggerInterrupt(INTERRUPT.Tame_Beast, character2);
		}
	}
}
