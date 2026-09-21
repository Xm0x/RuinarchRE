using System.Collections.Generic;
using Inner_Maps;
using Locations.Settlements;
using UtilityScripts;

public class ArsonistBehaviour : CharacterBehaviour
{
	public ArsonistBehaviour()
	{
		base.priority = 30;
	}

	public override bool TryDoBehaviour(Character character, ref string log, out JobQueueItem producedJob)
	{
		if (character.behaviourComponent.canArson)
		{
			if (character.behaviourComponent.arsonVillageTarget.Count <= 0)
			{
				character.behaviourComponent.PopulateVillageTargetsByPriority(character.behaviourComponent.arsonVillageTarget);
			}
			else
			{
				bool flag = false;
				Area area = character.behaviourComponent.arsonVillageTarget[0];
				for (int i = 0; i < area.settlementsOnArea.Count; i++)
				{
					BaseSettlement baseSettlement = area.settlementsOnArea[i];
					if (baseSettlement.locationType == LOCATION_TYPE.VILLAGE && baseSettlement.owner != null && !baseSettlement.owner.IsHostileWith(PlayerManager.Instance.player.playerFaction))
					{
						flag = true;
						break;
					}
				}
				if (flag)
				{
					character.behaviourComponent.ResetArsonistVillageTarget();
					character.behaviourComponent.PopulateVillageTargetsByPriority(character.behaviourComponent.arsonVillageTarget);
				}
			}
			if (character.behaviourComponent.arsonVillageTarget.Count > 0)
			{
				Area areaLocation = character.areaLocation;
				if (areaLocation != null && character.behaviourComponent.arsonVillageTarget.Contains(areaLocation))
				{
					List<TileObject> arsonTargetChoices = GetArsonTargetChoices(character);
					if (arsonTargetChoices != null)
					{
						TileObject randomElement = CollectionUtilities.GetRandomElement(arsonTargetChoices);
						return character.jobComponent.TriggerArson(randomElement, out producedJob);
					}
					LocationGridTile randomElement2 = CollectionUtilities.GetRandomElement(CollectionUtilities.GetRandomElement(character.behaviourComponent.arsonVillageTarget).gridTileComponent.borderTiles);
					return character.jobComponent.CreateGoToJob(randomElement2, out producedJob);
				}
				LocationGridTile randomElement3 = CollectionUtilities.GetRandomElement(CollectionUtilities.GetRandomElement(character.behaviourComponent.arsonVillageTarget).gridTileComponent.borderTiles);
				return character.jobComponent.CreateGoToJob(randomElement3, out producedJob);
			}
			return character.jobComponent.TriggerRoamAroundTerritory(out producedJob);
		}
		return character.jobComponent.TriggerRoamAroundTerritory(out producedJob);
	}

	public override void OnAddBehaviourToCharacter(Character character)
	{
		base.OnAddBehaviourToCharacter(character);
		character.behaviourComponent.OnBecomeArsonist();
		character.behaviourComponent.ResetArsonistVillageTarget();
		character.behaviourComponent.PopulateVillageTargetsByPriority(character.behaviourComponent.arsonVillageTarget);
	}

	public override void OnRemoveBehaviourFromCharacter(Character character)
	{
		base.OnRemoveBehaviourFromCharacter(character);
		character.behaviourComponent.OnNoLongerArsonist();
	}

	public override void OnLoadBehaviourToCharacter(Character character)
	{
		base.OnLoadBehaviourToCharacter(character);
		character.behaviourComponent.OnBecomeArsonist();
	}

	private List<TileObject> GetArsonTargetChoices(Character arson)
	{
		List<TileObject> list = null;
		List<LocationGridTile> list2 = RuinarchListPool<LocationGridTile>.Claim();
		arson.gridTileLocation.PopulateTilesInRadius(list2, 2, 0, includeCenterTile: true, includeTilesInDifferentStructure: true);
		for (int i = 0; i < list2.Count; i++)
		{
			TileObject objHere = list2[i].tileObjectComponent.objHere;
			if (objHere != null && !objHere.traitContainer.HasTrait("Burning", "Fire Resistant") && objHere.traitContainer.HasTrait("Flammable"))
			{
				if (list == null)
				{
					list = new List<TileObject>();
				}
				list.Add(objHere);
			}
		}
		RuinarchListPool<LocationGridTile>.Release(list2);
		return list;
	}
}
