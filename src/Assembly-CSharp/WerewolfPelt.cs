public class WerewolfPelt : TileObject
{
	public WerewolfPelt()
	{
		Initialize(TILE_OBJECT_TYPE.WEREWOLF_PELT, shouldAddCommonAdvertisements: false);
		AddAdvertisedAction(INTERACTION_TYPE.ASSAULT);
		AddAdvertisedAction(INTERACTION_TYPE.RESOLVE_COMBAT);
		AddAdvertisedAction(INTERACTION_TYPE.DROP_ITEM);
		AddAdvertisedAction(INTERACTION_TYPE.PICK_UP);
		AddAdvertisedAction(INTERACTION_TYPE.BOOBY_TRAP);
		AddAdvertisedAction(INTERACTION_TYPE.STEAL_ANYTHING);
		AddAdvertisedAction(INTERACTION_TYPE.DEMON_STEAL);
		base.traitContainer.AddTrait(this, "Interesting");
	}

	public WerewolfPelt(SaveDataTileObject data)
		: base(data)
	{
	}

	public override void SetInventoryOwner(Character p_newOwner)
	{
		base.SetInventoryOwner(p_newOwner);
		if (p_newOwner == null || p_newOwner.traitContainer.HasTrait("Lycanthrope"))
		{
			return;
		}
		if (p_newOwner.HasItem(TILE_OBJECT_TYPE.PHYLACTERY))
		{
			p_newOwner.UnobtainItem(TILE_OBJECT_TYPE.PHYLACTERY);
			p_newOwner.UnobtainItem(this);
			PlayerManager.Instance.player.storedTargetsComponent.Remove(this);
			Log log = GameManager.CreateNewLogUsingNewLocalization(GameManager.Instance.Today(), "Tile Object", "TileObjectAlerts_Table", "Werewolf Pelt activated_phylactery", LOG_TAG.Life_Changes);
			log.AddToFillers(p_newOwner, p_newOwner.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
			log.AddToFillers(this, base.name, LOG_IDENTIFIER.TARGET_CHARACTER);
			log.AddLogToDatabase(releaseLogAfter: true);
		}
		else
		{
			p_newOwner.UnobtainItem(this);
			PlayerManager.Instance.player.storedTargetsComponent.Remove(this);
			if (!p_newOwner.classComponent.IsStalkerCannotBeTurned())
			{
				p_newOwner.interruptComponent.TriggerInterrupt(INTERRUPT.Become_Lycanthrope, this);
			}
		}
	}
}
