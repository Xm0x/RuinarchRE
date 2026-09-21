using UnityEngine.Localization.Settings;
using UtilityScripts;

public abstract class ResourcePile : TileObject
{
	public RESOURCE providedResource { get; private set; }

	public abstract CONCRETE_RESOURCES specificProvidedResource { get; }

	public int resourceInPile => base.resourceStorageComponent.storedResources[providedResource];

	public override string nameplateName => GetNameplateName();

	public string strResourcesInPile => GetResourceQuantityString(resourceInPile);

	public ResourcePile(RESOURCE providedResource)
	{
		AddAdvertisedAction(INTERACTION_TYPE.TAKE_RESOURCE);
		AddAdvertisedAction(INTERACTION_TYPE.PICK_UP);
		AddAdvertisedAction(INTERACTION_TYPE.DEPOSIT_RESOURCE_PILE);
		AddAdvertisedAction(INTERACTION_TYPE.ASSAULT);
		AddAdvertisedAction(INTERACTION_TYPE.RESOLVE_COMBAT);
		AddAdvertisedAction(INTERACTION_TYPE.BOOBY_TRAP);
		AddAdvertisedAction(INTERACTION_TYPE.DROP_ITEM);
		AddAdvertisedAction(INTERACTION_TYPE.DROP_RESOURCE_TO_WORK_STRUCTURE);
		AddAdvertisedAction(INTERACTION_TYPE.STEAL_ANYTHING);
		AddAdvertisedAction(INTERACTION_TYPE.DEMON_STEAL);
		this.providedResource = providedResource;
	}

	public ResourcePile(SaveDataTileObject data, RESOURCE providedResource)
		: base(data)
	{
		this.providedResource = providedResource;
	}

	public virtual void SetResourceInPile(int amount)
	{
		base.resourceStorageComponent.SetResource(specificProvidedResource, amount);
		if (resourceInPile <= 0 && gridTileLocation != null && base.isBeingCarriedBy == null)
		{
			gridTileLocation.structure.RemovePOI(this);
		}
	}

	public virtual void AdjustResourceInPile(int adjustment, bool shouldBeDestroyed = true)
	{
		base.resourceStorageComponent.AdjustResource(specificProvidedResource, adjustment);
		Messenger.Broadcast(TileObjectSignals.RESOURCE_IN_PILE_CHANGED, this);
		if (resourceInPile <= 0 && shouldBeDestroyed)
		{
			DestroyResourcePile();
		}
	}

	public void DestroyResourcePile()
	{
		if (gridTileLocation != null && base.isBeingCarriedBy == null)
		{
			gridTileLocation.structure.RemovePOI(this);
		}
		else if (base.isBeingCarriedBy != null)
		{
			base.isBeingCarriedBy.UncarryPOI(this, bringBackToInventory: false, addToLocation: false);
			base.eventDispatcher.ExecuteTileObjectDestroyed(this);
			Messenger.Broadcast(TileObjectSignals.DESTROY_TILE_OBJECT, (TileObject)this);
		}
	}

	public void OnPileCombinedToOtherPile()
	{
		base.eventDispatcher.ExecuteTileObjectDestroyed(this);
		Messenger.Broadcast(TileObjectSignals.DESTROY_TILE_OBJECT, (TileObject)this);
	}

	private string GetNameplateName()
	{
		return base.name + " (x" + resourceInPile + ")";
	}

	public override void OnDestroyPOI(Character p_destroyer = null)
	{
		base.OnDestroyPOI(p_destroyer);
		Messenger.Broadcast(JobSignals.CHECK_JOB_APPLICABILITY, JOB_TYPE.DESTROY, (IPointOfInterest)this);
		Messenger.Broadcast(JobSignals.CHECK_JOB_APPLICABILITY, JOB_TYPE.ANGRY_DESTROY, (IPointOfInterest)this);
	}

	protected override void OnSetObjectAsUnbuilt()
	{
		base.OnSetObjectAsUnbuilt();
		AddAdvertisedAction(INTERACTION_TYPE.DEPOSIT_RESOURCE_PILE);
	}

	public override string GetAdditionalTestingData()
	{
		return base.GetAdditionalTestingData() + "\n\tResource in Pile: " + resourceInPile;
	}

	public override void VillagerReactionToTileObject(Character actor, ref string debugLog)
	{
		base.VillagerReactionToTileObject(actor, ref debugLog);
		if (actor.partyComponent.hasParty && actor.partyComponent.currentParty.isActive && actor.partyComponent.currentParty.partyState == PARTY_STATE.Working && base.mapObjectState == MAP_OBJECT_STATE.BUILT && actor.partyComponent.currentParty.currentQuest is RaidPartyQuest raidPartyQuest && gridTileLocation != null && gridTileLocation.IsPartOfSettlement(raidPartyQuest.targetSettlement) && GameUtilities.RollChance(35) && actor.jobComponent.TriggerStealRaidJob(this))
		{
			raidPartyQuest.SetIsSuccessful(state: true);
		}
	}

	protected override void OnSetGridTileLocation()
	{
		base.OnSetGridTileLocation();
	}

	public static string GetResourceQuantityString(int p_quantity)
	{
		if (LocalizationSettings.SelectedLocale.LocaleName.Equals("Polish (pl)"))
		{
			return p_quantity + "x";
		}
		return p_quantity.ToString();
	}
}
