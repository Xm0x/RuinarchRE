using System;
using System.Collections.Generic;
using UtilityScripts;

[Serializable]
public class SaveDataCombatComponent : SaveData<CombatComponent>
{
	public int attack;

	public int strengthModification;

	public float strengthPercentModification;

	public int intelligenceModification;

	public float intelligencePercentModification;

	public int maxHP;

	public int maxHPModification;

	public float maxHPPercentModification;

	public int attackSpeed;

	public float attackSpeedPercentModification;

	public int numOfKilledCharacters;

	public COMBAT_MODE combatMode;

	public COMBAT_MODE previousCombatMode;

	public List<string> hostileCharactersInRange;

	public List<string> hostileTileObjectsInRange;

	public List<string> avoidCharactersInRange;

	public List<string> avoidTileObjectsInRange;

	public List<string> bannedFromHostileList;

	public List<string> unkillableCharacters;

	public Dictionary<string, SaveDataCombatData> characterCombatData;

	public Dictionary<string, SaveDataCombatData> tileObjectCombatData;

	public ELEMENTAL_TYPE elementalDamageType;

	public List<ELEMENTAL_TYPE> elementalStatusWaitingList;

	public SaveDataCharacterCombatBehaviourParent combatBehaviourParent;

	public SaveDataCombatSpecialSkillWrapper specialSkillParent;

	public bool willProcessCombat;

	public int critRate;

	public int clearUnkillableListTicks;

	public bool shouldIncreaseClearUnkillableListTicks;

	public int hpRecoveryPerTickOutsideCombat;

	public override void Save(CombatComponent data)
	{
		attack = data.attack;
		strengthModification = data.strengthModification;
		critRate = data.critRate;
		strengthPercentModification = data.strengthPercentModification;
		intelligenceModification = data.intelligenceModification;
		intelligencePercentModification = data.intelligencePercentModification;
		maxHP = data.maxHP;
		maxHPModification = data.maxHPModification;
		maxHPPercentModification = data.maxHPPercentModification;
		attackSpeed = data.attackSpeed;
		attackSpeedPercentModification = data.attackSpeedPercentModification;
		combatMode = data.combatMode;
		previousCombatMode = data.combatMode;
		elementalStatusWaitingList = RuinarchListPool<ELEMENTAL_TYPE>.Claim(10);
		if (data.elementalStatusWaitingList.Count > 0)
		{
			elementalStatusWaitingList.AddRange(data.elementalStatusWaitingList);
		}
		elementalDamageType = data.currentElement.type;
		willProcessCombat = data.willProcessCombat;
		numOfKilledCharacters = data.numOfKilledCharacters;
		clearUnkillableListTicks = data.clearUnkillableListTicks;
		shouldIncreaseClearUnkillableListTicks = data.shouldIncreaseClearUnkillableListTicks;
		hpRecoveryPerTickOutsideCombat = data.hpRecoveryPerTickOutsideCombat;
		hostileCharactersInRange = RuinarchListPool<string>.Claim();
		hostileTileObjectsInRange = RuinarchListPool<string>.Claim();
		for (int i = 0; i < data.hostilesInRange.Count; i++)
		{
			IPointOfInterest pointOfInterest = data.hostilesInRange[i];
			if (pointOfInterest.poiType == POINT_OF_INTEREST_TYPE.CHARACTER)
			{
				hostileCharactersInRange.Add(pointOfInterest.persistentID);
			}
			else if (pointOfInterest.poiType == POINT_OF_INTEREST_TYPE.TILE_OBJECT)
			{
				hostileTileObjectsInRange.Add(pointOfInterest.persistentID);
			}
		}
		avoidCharactersInRange = RuinarchListPool<string>.Claim();
		avoidTileObjectsInRange = RuinarchListPool<string>.Claim();
		for (int j = 0; j < data.avoidInRange.Count; j++)
		{
			IPointOfInterest pointOfInterest2 = data.avoidInRange[j];
			if (pointOfInterest2.poiType == POINT_OF_INTEREST_TYPE.CHARACTER)
			{
				avoidCharactersInRange.Add(pointOfInterest2.persistentID);
			}
			else if (pointOfInterest2.poiType == POINT_OF_INTEREST_TYPE.TILE_OBJECT)
			{
				avoidTileObjectsInRange.Add(pointOfInterest2.persistentID);
			}
		}
		characterCombatData = new Dictionary<string, SaveDataCombatData>();
		tileObjectCombatData = new Dictionary<string, SaveDataCombatData>();
		foreach (KeyValuePair<IPointOfInterest, CombatData> item in data.combatDataDictionary)
		{
			SaveDataCombatData saveDataCombatData = new SaveDataCombatData();
			saveDataCombatData.Save(item.Value);
			if (item.Key.poiType == POINT_OF_INTEREST_TYPE.CHARACTER)
			{
				characterCombatData.Add(item.Key.persistentID, saveDataCombatData);
			}
			else if (item.Key.poiType == POINT_OF_INTEREST_TYPE.TILE_OBJECT)
			{
				tileObjectCombatData.Add(item.Key.persistentID, saveDataCombatData);
			}
		}
		if (data.bannedFromHostileList.Count > 0)
		{
			bannedFromHostileList = RuinarchListPool<string>.Claim();
			for (int k = 0; k < data.bannedFromHostileList.Count; k++)
			{
				bannedFromHostileList.Add(data.bannedFromHostileList[k].persistentID);
			}
		}
		if (data.unkillableCharacters.Count > 0)
		{
			unkillableCharacters = RuinarchListPool<string>.Claim();
			for (int l = 0; l < data.unkillableCharacters.Count; l++)
			{
				unkillableCharacters.Add(data.unkillableCharacters[l].persistentID);
			}
		}
		combatBehaviourParent = new SaveDataCharacterCombatBehaviourParent();
		combatBehaviourParent.Save(data.combatBehaviourParent);
		specialSkillParent = new SaveDataCombatSpecialSkillWrapper();
		specialSkillParent.Save(data.specialSkillParent);
	}

	public override CombatComponent Load()
	{
		return new CombatComponent(this);
	}

	public override void CleanUp()
	{
		if (hostileCharactersInRange != null)
		{
			RuinarchListPool<string>.Release(hostileCharactersInRange);
			hostileCharactersInRange = null;
		}
		if (hostileTileObjectsInRange != null)
		{
			RuinarchListPool<string>.Release(hostileTileObjectsInRange);
			hostileTileObjectsInRange = null;
		}
		if (avoidCharactersInRange != null)
		{
			RuinarchListPool<string>.Release(avoidCharactersInRange);
			avoidCharactersInRange = null;
		}
		if (avoidTileObjectsInRange != null)
		{
			RuinarchListPool<string>.Release(avoidTileObjectsInRange);
			avoidTileObjectsInRange = null;
		}
		if (bannedFromHostileList != null)
		{
			RuinarchListPool<string>.Release(bannedFromHostileList);
			bannedFromHostileList = null;
		}
		if (unkillableCharacters != null)
		{
			RuinarchListPool<string>.Release(unkillableCharacters);
			unkillableCharacters = null;
		}
		if (elementalStatusWaitingList != null)
		{
			RuinarchListPool<ELEMENTAL_TYPE>.Release(elementalStatusWaitingList);
			elementalStatusWaitingList = null;
		}
		characterCombatData?.Clear();
		characterCombatData = null;
		tileObjectCombatData?.Clear();
		tileObjectCombatData = null;
	}
}
