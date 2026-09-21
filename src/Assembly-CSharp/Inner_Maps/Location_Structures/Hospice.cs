using System;
using System.Collections.Generic;
using Traits;
using UtilityScripts;

namespace Inner_Maps.Location_Structures;

public class Hospice : ManMadeStructure, TileObjectEventDispatcher.IDestroyedListener
{
	private const int DefaultBedCount = 5;

	public List<BedClinic> beds { get; private set; }

	public List<Character> bannedCharacters { get; private set; }

	public override Type serializedData => typeof(SaveDataHospice);

	public Hospice(Region location)
		: base(STRUCTURE_TYPE.HOSPICE, location)
	{
		SetMaxHPAndReset(8000);
		beds = new List<BedClinic>();
		bannedCharacters = new List<Character>();
	}

	public Hospice(Region location, SaveDataManMadeStructure data)
		: base(location, data)
	{
		SetMaxHP(8000);
		beds = new List<BedClinic>();
		bannedCharacters = new List<Character>();
	}

	public override void LoadReferences(SaveDataLocationStructure saveDataLocationStructure)
	{
		base.LoadReferences(saveDataLocationStructure);
		SaveDataHospice saveDataHospice = saveDataLocationStructure as SaveDataHospice;
		if (saveDataHospice.beds != null)
		{
			for (int i = 0; i < saveDataHospice.beds.Length; i++)
			{
				string text = saveDataHospice.beds[i];
				if (DatabaseManager.Instance.tileObjectDatabase.GetTileObjectByPersistentID(text) is BedClinic p_bed)
				{
					AddBed(p_bed);
				}
			}
		}
		if (saveDataHospice.bannedCharacters != null)
		{
			bannedCharacters.AddRange(SaveUtilities.ConvertIDListToCharacters(saveDataHospice.bannedCharacters));
		}
	}

	public void AddBed(BedClinic p_bed)
	{
		if (!beds.Contains(p_bed))
		{
			beds.Add(p_bed);
			p_bed.eventDispatcher.SubscribeToTileObjectDestroyed(this);
		}
	}

	private void RemoveBed(BedClinic p_bed)
	{
		if (beds.Remove(p_bed))
		{
			p_bed.eventDispatcher.UnsubscribeToTileObjectDestroyed(this);
		}
	}

	public bool TryGetMissingDefaultBedPosition(out StructureTemplateObjectData p_objectTemplate)
	{
		List<StructureTemplateObjectData> list = RuinarchListPool<StructureTemplateObjectData>.Claim();
		base.structureObj.PopulateMissingPreplacedObjectsOfTypeThatIsOnUnoccupiedTile(list, TILE_OBJECT_TYPE.BED_CLINIC, base.region.innerMap);
		if (list.Count > 0)
		{
			p_objectTemplate = CollectionUtilities.GetRandomElement(list);
			return true;
		}
		p_objectTemplate = null;
		return false;
	}

	public void OnTileObjectDestroyed(TileObject p_tileObject)
	{
		if (p_tileObject is BedClinic p_bed)
		{
			RemoveBed(p_bed);
		}
	}

	protected override void SubscribeListeners(bool shouldLock)
	{
		base.SubscribeListeners(shouldLock);
		Messenger.AddListener<TileObject, Character, LocationGridTile>(GridTileSignals.TILE_OBJECT_REMOVED, OnTileObjectRemoved, shouldLock);
		Messenger.AddListener<TileObject, LocationGridTile>(GridTileSignals.TILE_OBJECT_PLACED, OnTileObjectPlaced, shouldLock);
	}

	protected override void UnsubscribeListeners()
	{
		base.UnsubscribeListeners();
		Messenger.RemoveListener<TileObject, Character, LocationGridTile>(GridTileSignals.TILE_OBJECT_REMOVED, OnTileObjectRemoved);
		Messenger.RemoveListener<TileObject, LocationGridTile>(GridTileSignals.TILE_OBJECT_PLACED, OnTileObjectPlaced);
	}

	private void OnTileObjectPlaced(TileObject p_tileObject, LocationGridTile p_tile)
	{
		if (p_tile.structure == this && p_tileObject is BedClinic p_bed)
		{
			AddBed(p_bed);
		}
	}

	private void OnTileObjectRemoved(TileObject p_tileObject, Character p_removedBy, LocationGridTile p_removedFrom)
	{
		if (p_tileObject is BedClinic p_bed)
		{
			RemoveBed(p_bed);
		}
	}

	public override string GetTestingInfo()
	{
		return string.Concat(base.GetTestingInfo() + "\nBeds: " + beds.ComafyList(), "\nBanned Characters: ", bannedCharacters.ComafyList());
	}

	public override bool CanHireAWorker()
	{
		return !HasAssignedWorker();
	}

	public bool HasWorkerWithLevel5HealingMagic()
	{
		for (int i = 0; i < base.assignedWorkerIDs.Count; i++)
		{
			string text = base.assignedWorkerIDs[i];
			if (DatabaseManager.Instance.characterDatabase.GetCharacterByPersistentID(text).TryGetTalentLevel(CHARACTER_TALENT.Healing_Magic) >= 5)
			{
				return true;
			}
		}
		return false;
	}

	public override bool AddAssignedWorker(Character p_worker)
	{
		if (base.AddAssignedWorker(p_worker))
		{
			ClearBannedCharacters();
			return true;
		}
		return false;
	}

	public override bool RemoveAssignedWorker(Character p_worker)
	{
		if (base.RemoveAssignedWorker(p_worker))
		{
			ClearBannedCharacters();
			return true;
		}
		return false;
	}

	public override bool CanPurchaseFromHere(Character p_buyer, out bool needsToPay, out int buyerOpinionOfWorker)
	{
		return DefaultCanPurchaseFromHereForSingleWorkerStructures(p_buyer, out needsToPay, out buyerOpinionOfWorker);
	}

	protected override void ProcessWorkStructureJobsByWorker(Character p_worker, out JobQueueItem producedJob)
	{
		producedJob = null;
		for (int i = 0; i < beds.Count; i++)
		{
			BedClinic bedClinic = beds[i];
			for (int j = 0; j < bedClinic.users.Length; j++)
			{
				Character character = bedClinic.users[j];
				if (character != null && p_worker.relationshipContainer.HasRelationshipWith(character) && p_worker.relationshipContainer.IsEnemiesWith(character) && !p_worker.traitContainer.HasTrait("Diplomatic"))
				{
					p_worker.jobComponent.TriggerKickOutOfHospiceCharacter(character, out producedJob);
					if (producedJob != null)
					{
						return;
					}
				}
			}
		}
		Character villagerToBeCured = GetVillagerToBeCured(p_worker);
		if (villagerToBeCured != null)
		{
			p_worker.jobComponent.TriggerHealerCureCharacter(villagerToBeCured, out producedJob);
			if (producedJob != null)
			{
				return;
			}
		}
		villagerToBeCured = GetVillagerToBeFed(p_worker);
		if (villagerToBeCured != null)
		{
			p_worker.jobComponent.TryTriggerFeed(villagerToBeCured, out producedJob);
			if (producedJob != null)
			{
				return;
			}
		}
		villagerToBeCured = GetVillagerToBeDispelledForVampirism(p_worker);
		if (villagerToBeCured != null)
		{
			p_worker.jobComponent.TriggerCureMagicalAffliction(villagerToBeCured, "Vampire", out producedJob);
			if (producedJob != null)
			{
				return;
			}
		}
		villagerToBeCured = GetVillagerToBeDispelledForLycan(p_worker);
		if (villagerToBeCured != null)
		{
			p_worker.jobComponent.TriggerCureMagicalAffliction(villagerToBeCured, "Lycanthrope", out producedJob);
			if (producedJob != null)
			{
				return;
			}
		}
		List<TileObject> tileObjectsOfType = GetTileObjectsOfType(TILE_OBJECT_TYPE.HERB_PLANT);
		if (tileObjectsOfType == null || tileObjectsOfType.Count < 3)
		{
			HerbPlant firstAvailableHerbPlant = p_worker.homeSettlement.SettlementResources.GetFirstAvailableHerbPlant(p_worker.homeSettlement, p_worker);
			if (firstAvailableHerbPlant != null)
			{
				p_worker.jobComponent.TriggerGatherHerb(firstAvailableHerbPlant, out producedJob);
				if (producedJob != null)
				{
					return;
				}
			}
		}
		List<TileObject> tileObjectsOfType2 = GetTileObjectsOfType(TILE_OBJECT_TYPE.HEALING_POTION);
		if ((tileObjectsOfType2 == null || tileObjectsOfType2.Count <= 0) && tileObjectsOfType != null && tileObjectsOfType.Count >= 1)
		{
			p_worker.jobComponent.TriggerCraftWorkplacePotion(tileObjectsOfType[0], out producedJob);
			if (producedJob != null)
			{
				return;
			}
		}
		List<TileObject> tileObjectsOfType3 = GetTileObjectsOfType(TILE_OBJECT_TYPE.ANTIDOTE);
		if ((tileObjectsOfType3 == null || tileObjectsOfType3.Count <= 0) && tileObjectsOfType != null && tileObjectsOfType.Count > 1)
		{
			p_worker.jobComponent.TriggerCraftHospiceAntidote(tileObjectsOfType[0], out producedJob);
			if (producedJob != null)
			{
				return;
			}
		}
		if (GetNumberOfTileObjects(TILE_OBJECT_TYPE.BED_CLINIC) < 5)
		{
			LocationStructure preferredBasicResourceStructure = p_worker.structureComponent.GetPreferredBasicResourceStructure(p_worker);
			if (preferredBasicResourceStructure != null && p_worker.jobComponent.CreateCraftHospiceBed(this, preferredBasicResourceStructure, out producedJob))
			{
				return;
			}
		}
		TryCreateCleanJob(p_worker, out producedJob);
	}

	private Character GetVillagerToBeCured(Character p_worker)
	{
		if (p_worker.TryGetTalentLevel(CHARACTER_TALENT.Healing_Magic) <= 1)
		{
			return null;
		}
		for (int i = 0; i < beds.Count; i++)
		{
			if (beds[i].users.Length == 0)
			{
				continue;
			}
			Character character = beds[i].users[0];
			if (character == null)
			{
				continue;
			}
			if (p_worker.TryGetTalentLevel(CHARACTER_TALENT.Healing_Magic) >= 3)
			{
				if (character.traitContainer.HasTrait("Injured", "Poisoned", "Plagued", "Burnt"))
				{
					return character;
				}
			}
			else if (p_worker.TryGetTalentLevel(CHARACTER_TALENT.Healing_Magic) >= 2 && character.traitContainer.HasTrait("Injured", "Poisoned", "Burnt"))
			{
				return character;
			}
		}
		return null;
	}

	private Character GetVillagerToBeFed(Character p_worker)
	{
		for (int i = 0; i < beds.Count; i++)
		{
			if (beds[i].users.Length != 0)
			{
				Character character = beds[i].users[0];
				if (character != null && character.needsComponent.isStarving && !character.HasJobTargetingThis(JOB_TYPE.FEED))
				{
					return character;
				}
			}
		}
		return null;
	}

	private Character GetVillagerToBeDispelledForLycan(Character p_worker)
	{
		if (p_worker.TryGetTalentLevel(CHARACTER_TALENT.Healing_Magic) < 5)
		{
			return null;
		}
		for (int i = 0; i < beds.Count; i++)
		{
			if (beds[i].users.Length != 0)
			{
				Character character = beds[i].users[0];
				if (character != null && character.isLycanthrope && character.lycanData.dislikesBeingLycan)
				{
					return character;
				}
			}
		}
		for (int j = 0; j < base.charactersHere.Count; j++)
		{
			Character character2 = base.charactersHere[j];
			if (character2 != null && character2.isLycanthrope && character2.lycanData.dislikesBeingLycan)
			{
				return character2;
			}
		}
		return null;
	}

	private Character GetVillagerToBeDispelledForVampirism(Character p_worker)
	{
		if (p_worker.TryGetTalentLevel(CHARACTER_TALENT.Healing_Magic) < 5)
		{
			return null;
		}
		for (int i = 0; i < beds.Count; i++)
		{
			if (beds[i].users.Length != 0)
			{
				Character character = beds[i].users[0];
				if (character != null && character.traitContainer.HasTrait("Vampire") && character.traitContainer.GetTraitOrStatus<Vampire>("Vampire").dislikedBeingVampire)
				{
					return character;
				}
			}
		}
		for (int j = 0; j < base.charactersHere.Count; j++)
		{
			Character character2 = base.charactersHere[j];
			if (character2 != null && character2.traitContainer.HasTrait("Vampire") && character2.traitContainer.GetTraitOrStatus<Vampire>("Vampire").dislikedBeingVampire)
			{
				return character2;
			}
		}
		return null;
	}

	public BedClinic GetFirstUnoccupiedBed()
	{
		for (int i = 0; i < beds.Count; i++)
		{
			BedClinic bedClinic = beds[i];
			if (bedClinic.GetUserCount() <= 0)
			{
				return bedClinic;
			}
		}
		return null;
	}

	public BedClinic GetFirstBedToRecuperate()
	{
		for (int i = 0; i < beds.Count; i++)
		{
			BedClinic bedClinic = beds[i];
			if (bedClinic.GetUserCount() <= 0 && !bedClinic.HasJobTargetingThis(JOB_TYPE.RECUPERATE))
			{
				return bedClinic;
			}
		}
		return null;
	}

	public void BanCharacter(Character p_character)
	{
		if (!bannedCharacters.Contains(p_character))
		{
			bannedCharacters.Add(p_character);
		}
	}

	private void ClearBannedCharacters()
	{
		bannedCharacters.Clear();
	}

	public bool IsBanned(Character p_character)
	{
		return bannedCharacters.Contains(p_character);
	}

	public override void CleanUp()
	{
		if (!DatabaseManager.Instance.structureDatabase.HasStructure(base.persistentID))
		{
			return;
		}
		ClearBannedCharacters();
		if (beds != null)
		{
			while (beds.Count > 0)
			{
				RemoveBed(beds[0]);
			}
		}
		base.CleanUp();
	}
}
