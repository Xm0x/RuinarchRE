public class Mantra : WeaponItem
{
	public Mantra()
	{
		Initialize(TILE_OBJECT_TYPE.MANTRA, shouldAddCommonAdvertisements: false);
		equipmentData = EquipmentDataHandler.Instance.GetEquipmentDataBaseOnName(base.internalName);
		base.traitContainer.AddTrait(this, "Indestructible");
	}

	public Mantra(SaveDataEquipmentItem data)
		: base(data)
	{
	}

	public override void ProcessEffectsOnEquip(Character p_character)
	{
		base.ProcessEffectsOnEquip(p_character);
		if (p_character != null)
		{
			p_character.limiterComponent.VoteToHinderAfflictions();
			p_character.traitContainer.RemoveAllTraitsByType(p_character, TRAIT_TYPE.FLAW);
		}
	}

	public override void ProcessEffectsOnUnEquip(Character p_character)
	{
		base.ProcessEffectsOnUnEquip(p_character);
		p_character?.limiterComponent.VoteToAllowAfflictions();
	}
}
