using Inner_Maps;
using Inner_Maps.Location_Structures;
using Locations.Settlements;
using Object_Pools;
using UtilityScripts;

public class DragonBehaviour : BaseMonsterBehaviour
{
	public DragonBehaviour()
	{
		base.priority = 8;
	}

	protected override bool WildBehaviour(Character character, ref string log, out JobQueueItem producedJob)
	{
		producedJob = null;
		if (character is Dragon dragon)
		{
			if (dragon.willLeaveWorld)
			{
				if (dragon.gridTileLocation.IsAtEdgeOfWalkableMap())
				{
					Region currentRegion = dragon.currentRegion;
					dragon.SetDestroyMarkerOnDeath(state: true);
					dragon.SetShowNotificationOnDeath(showNotificationOnDeath: false);
					Log log2 = GameManager.CreateNewLogUsingNewLocalization(GameManager.Instance.Today(), "Summon", "CharacterAlerts_Table", "dragon_left", LOG_TAG.Life_Changes);
					log2.AddToFillers(dragon, dragon.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
					log2.AddLogToDatabase();
					dragon.Death("normal", null, null, log2);
					LogPool.Release(log2);
					if (UIManager.Instance.monsterInfoUI.isShowing && UIManager.Instance.monsterInfoUI.activeMonster == dragon)
					{
						UIManager.Instance.monsterInfoUI.CloseMenu();
					}
					Messenger.Broadcast(MonsterSignals.DRAGON_LEFT_WORLD, character, currentRegion);
				}
				else
				{
					dragon.jobComponent.CreateGoToSpecificTileJob(dragon.gridTileLocation.GetNearestEdgeTileFromThis(), out producedJob);
				}
				return true;
			}
			if (dragon.isAttackingPlayer)
			{
				if (dragon.targetStructure == null || !(dragon.targetStructure is DemonicStructure) || dragon.targetStructure.hasBeenDestroyed)
				{
					dragon.SetPlayerTargetStructure();
				}
				if (dragon.targetStructure != null)
				{
					if (dragon.currentStructure == dragon.targetStructure)
					{
						LocationStructure currentStructure = dragon.currentStructure;
						if (currentStructure.objectsThatContributeToDamage.Count > 0)
						{
							TileObject tileObject = null;
							IDamageable nearestDamageableThatContributeToHP = currentStructure.GetNearestDamageableThatContributeToHP(dragon.gridTileLocation);
							if (nearestDamageableThatContributeToHP != null && nearestDamageableThatContributeToHP is TileObject tileObject2)
							{
								tileObject = tileObject2;
							}
							if (tileObject != null)
							{
								dragon.combatComponent.Fight(tileObject, "Hostility");
								return true;
							}
							dragon.ResetTargetStructure();
							return true;
						}
						dragon.ResetTargetStructure();
						return true;
					}
					LocationGridTile randomElement = CollectionUtilities.GetRandomElement(dragon.targetStructure.passableTiles);
					character.jobComponent.CreateGoToJob(randomElement, out producedJob);
					return true;
				}
				character.jobComponent.TriggerRoamAroundTile(out producedJob);
				return true;
			}
			if (dragon.targetStructure == null)
			{
				dragon.SetVillageTargetStructure();
			}
			if (dragon.targetStructure != null)
			{
				BaseSettlement settlementLocation = dragon.targetStructure.settlementLocation;
				if (settlementLocation != null && character.gridTileLocation != null)
				{
					if (character.gridTileLocation.IsPartOfSettlement(settlementLocation))
					{
						Character randomResidentForInvasionTargetThatIsInsideSettlement = settlementLocation.GetRandomResidentForInvasionTargetThatIsInsideSettlement(settlementLocation, character);
						if (randomResidentForInvasionTargetThatIsInsideSettlement != null)
						{
							character.combatComponent.Fight(randomResidentForInvasionTargetThatIsInsideSettlement, "Hostility");
							return true;
						}
						LocationGridTile randomTile = settlementLocation.GetRandomStructure().GetRandomTile();
						character.combatComponent.Fight(randomTile.tileObjectComponent.genericTileObject, "Hostility");
						return true;
					}
					LocationStructure randomStructure = settlementLocation.GetRandomStructure();
					if (randomStructure != null)
					{
						LocationGridTile randomElement2 = CollectionUtilities.GetRandomElement(randomStructure.passableTiles);
						if (character.jobComponent.CreateGoToJob(randomElement2, out producedJob))
						{
							return true;
						}
					}
				}
				character.jobComponent.TriggerRoamAroundStructure(out producedJob);
				return true;
			}
			character.jobComponent.TriggerRoamAroundTile(out producedJob);
			return true;
		}
		return false;
	}
}
