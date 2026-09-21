public class StructureBlockerTileObject : TileObject
{
	public StructureBlockerTileObject()
	{
		Initialize(TILE_OBJECT_TYPE.STRUCTURE_BLOCKER_TILE_OBJECT);
		RemoveAdvertisedAction(INTERACTION_TYPE.STEAL_ANYTHING);
		RemoveAdvertisedAction(INTERACTION_TYPE.DEMON_STEAL);
		base.traitContainer.AddTrait(this, "Immovable");
	}

	public StructureBlockerTileObject(SaveDataTileObject data)
		: base(data)
	{
	}

	public override bool CanBeSelected()
	{
		return false;
	}

	public override bool IsUnpassable()
	{
		return true;
	}

	public override void AdjustHP(int amount, ELEMENTAL_TYPE elementalDamageType, bool triggerDeath = false, object source = null, CombatManager.ElementalTraitProcessor elementalTraitProcessor = null, bool showHPBar = false, float piercingPower = 0f, bool isPlayerSource = false, bool isTrueDamage = false)
	{
		showHPBar = false;
		base.AdjustHP(amount, elementalDamageType, triggerDeath, source, elementalTraitProcessor, showHPBar, piercingPower, isPlayerSource, isTrueDamage);
	}
}
