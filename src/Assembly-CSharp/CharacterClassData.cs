using System.Collections.Generic;
using AK.Wwise;
using UnityEngine;

[CreateAssetMenu(fileName = "New Character Class Data", menuName = "Scriptable Objects/Character Class Data")]
public class CharacterClassData : ScriptableObject
{
	[Header("Combat")]
	public CHARACTER_COMBAT_BEHAVIOUR combatBehaviourType;

	public COMBAT_SPECIAL_SKILL combatSpecialSkillType;

	[Header("Structure")]
	public STRUCTURE_TYPE workStructureType;

	[Header("Misc")]
	public int summonCost;

	public string identifier;

	public string[] traitNames;

	public string[] interestedItemNames;

	public JOB_TYPE[] ableJobs;

	[Header("Game Start Values")]
	public float initialVillagerPiercing;

	public float[] initialVillagerPhysicalResistances;

	public float[] initialVillagerMentalResistances;

	public float[] initialVillagerElementalResistances;

	public float[] initialVillagerSecondaryResistances;

	[Header("Upgrade bonus per skill level up")]
	public CharacterProgressionBonusData characterSkillUpdateData;

	[Header("Craftable Equipments")]
	public List<TILE_OBJECT_TYPE> craftableWeapons = new List<TILE_OBJECT_TYPE>();

	public List<TILE_OBJECT_TYPE> craftableArmors = new List<TILE_OBJECT_TYPE>();

	public List<TILE_OBJECT_TYPE> craftableAccessories = new List<TILE_OBJECT_TYPE>();

	[Header("Character Category")]
	public bool isVillagerType;

	[Header("Agitate Data")]
	public bool cantBeAgitated;

	[Header("Monster Spawner Data")]
	public int monsterSpawnerDamageOnSpawn;

	[Header("SFX")]
	public AK.Wwise.Event uiSFX;

	[ContextMenu("Set Knight Type Items")]
	public void SetKnightTypeItems()
	{
		craftableWeapons.Clear();
		craftableArmors.Clear();
		craftableAccessories.Clear();
		craftableWeapons.Add(TILE_OBJECT_TYPE.BASIC_SWORD);
		craftableWeapons.Add(TILE_OBJECT_TYPE.COPPER_SWORD);
		craftableWeapons.Add(TILE_OBJECT_TYPE.IRON_SWORD);
		craftableWeapons.Add(TILE_OBJECT_TYPE.MITHRIL_SWORD);
		craftableWeapons.Add(TILE_OBJECT_TYPE.ORICHALCUM_SWORD);
		craftableArmors.Add(TILE_OBJECT_TYPE.BASIC_SHIRT);
		craftableArmors.Add(TILE_OBJECT_TYPE.RABBIT_SHIRT);
		craftableArmors.Add(TILE_OBJECT_TYPE.MINK_SHIRT);
		craftableArmors.Add(TILE_OBJECT_TYPE.WOOL_SHIRT);
		craftableArmors.Add(TILE_OBJECT_TYPE.MOONWALKER_SHIRT);
		craftableArmors.Add(TILE_OBJECT_TYPE.COPPER_ARMOR);
		craftableArmors.Add(TILE_OBJECT_TYPE.IRON_ARMOR);
		craftableArmors.Add(TILE_OBJECT_TYPE.MITHRIL_ARMOR);
		craftableArmors.Add(TILE_OBJECT_TYPE.ORICHALCUM_ARMOR);
		craftableAccessories.Add(TILE_OBJECT_TYPE.BRACER);
	}

	[ContextMenu("Set Archer Type Items")]
	public void SetArcherTypeItems()
	{
		craftableWeapons.Clear();
		craftableArmors.Clear();
		craftableAccessories.Clear();
		craftableWeapons.Add(TILE_OBJECT_TYPE.BASIC_BOW);
		craftableWeapons.Add(TILE_OBJECT_TYPE.COPPER_BOW);
		craftableWeapons.Add(TILE_OBJECT_TYPE.IRON_BOW);
		craftableWeapons.Add(TILE_OBJECT_TYPE.MITHRIL_BOW);
		craftableWeapons.Add(TILE_OBJECT_TYPE.ORICHALCUM_BOW);
		SetCultLeaderArmorSets();
	}

	[ContextMenu("Set Non Combatant Items")]
	public void SetNonCombatantTypeItems()
	{
		craftableWeapons.Clear();
		craftableArmors.Clear();
		craftableAccessories.Clear();
		craftableWeapons.Add(TILE_OBJECT_TYPE.BASIC_DAGGER);
		craftableWeapons.Add(TILE_OBJECT_TYPE.COPPER_DAGGER);
		craftableWeapons.Add(TILE_OBJECT_TYPE.IRON_DAGGER);
		craftableWeapons.Add(TILE_OBJECT_TYPE.MITHRIL_DAGGER);
		craftableWeapons.Add(TILE_OBJECT_TYPE.ORICHALCUM_DAGGER);
		craftableArmors.Add(TILE_OBJECT_TYPE.BASIC_SHIRT);
		craftableArmors.Add(TILE_OBJECT_TYPE.RABBIT_SHIRT);
		craftableArmors.Add(TILE_OBJECT_TYPE.MINK_SHIRT);
		craftableArmors.Add(TILE_OBJECT_TYPE.WOOL_SHIRT);
		craftableArmors.Add(TILE_OBJECT_TYPE.MOONWALKER_SHIRT);
		craftableAccessories.Add(TILE_OBJECT_TYPE.NECKLACE);
	}

	[ContextMenu("Set Mage Type Items")]
	public void SetMageTypeItems()
	{
		craftableWeapons.Clear();
		craftableArmors.Clear();
		craftableAccessories.Clear();
		craftableWeapons.Add(TILE_OBJECT_TYPE.BASIC_STAFF);
		craftableWeapons.Add(TILE_OBJECT_TYPE.COPPER_STAFF);
		craftableWeapons.Add(TILE_OBJECT_TYPE.IRON_STAFF);
		craftableWeapons.Add(TILE_OBJECT_TYPE.MITHRIL_STAFF);
		craftableWeapons.Add(TILE_OBJECT_TYPE.ORICHALCUM_STAFF);
		craftableArmors.Add(TILE_OBJECT_TYPE.BASIC_SHIRT);
		craftableArmors.Add(TILE_OBJECT_TYPE.RABBIT_SHIRT);
		craftableArmors.Add(TILE_OBJECT_TYPE.MINK_SHIRT);
		craftableArmors.Add(TILE_OBJECT_TYPE.WOOL_SHIRT);
		craftableArmors.Add(TILE_OBJECT_TYPE.MOONWALKER_SHIRT);
		craftableAccessories.Add(TILE_OBJECT_TYPE.RING);
		craftableAccessories.Add(TILE_OBJECT_TYPE.SCROLL);
	}

	[ContextMenu("Set Barbarian Type Items")]
	public void SetBarbarianTypeItems()
	{
		craftableWeapons.Clear();
		craftableArmors.Clear();
		craftableAccessories.Clear();
		craftableWeapons.Add(TILE_OBJECT_TYPE.BASIC_AXE);
		craftableWeapons.Add(TILE_OBJECT_TYPE.COPPER_AXE);
		craftableWeapons.Add(TILE_OBJECT_TYPE.IRON_AXE);
		craftableWeapons.Add(TILE_OBJECT_TYPE.MITHRIL_AXE);
		craftableWeapons.Add(TILE_OBJECT_TYPE.ORICHALCUM_AXE);
		craftableArmors.Add(TILE_OBJECT_TYPE.BASIC_SHIRT);
		craftableArmors.Add(TILE_OBJECT_TYPE.RABBIT_SHIRT);
		craftableArmors.Add(TILE_OBJECT_TYPE.MINK_SHIRT);
		craftableArmors.Add(TILE_OBJECT_TYPE.WOOL_SHIRT);
		craftableArmors.Add(TILE_OBJECT_TYPE.MOONWALKER_SHIRT);
		craftableArmors.Add(TILE_OBJECT_TYPE.COPPER_ARMOR);
		craftableArmors.Add(TILE_OBJECT_TYPE.IRON_ARMOR);
		craftableArmors.Add(TILE_OBJECT_TYPE.MITHRIL_ARMOR);
		craftableArmors.Add(TILE_OBJECT_TYPE.ORICHALCUM_ARMOR);
		craftableAccessories.Add(TILE_OBJECT_TYPE.BRACER);
	}

	[ContextMenu("Set Cultleader Armors Set")]
	public void SetCultLeaderArmorSets()
	{
		craftableArmors.Clear();
		craftableArmors.Add(TILE_OBJECT_TYPE.BASIC_SHIRT);
		craftableArmors.Add(TILE_OBJECT_TYPE.RABBIT_SHIRT);
		craftableArmors.Add(TILE_OBJECT_TYPE.MINK_SHIRT);
		craftableArmors.Add(TILE_OBJECT_TYPE.WOOL_SHIRT);
		craftableArmors.Add(TILE_OBJECT_TYPE.MOONWALKER_SHIRT);
		craftableArmors.Add(TILE_OBJECT_TYPE.BOAR_HIDE_ARMOR);
		craftableArmors.Add(TILE_OBJECT_TYPE.BEAR_HIDE_ARMOR);
		craftableArmors.Add(TILE_OBJECT_TYPE.WOLF_HIDE_ARMOR);
		craftableArmors.Add(TILE_OBJECT_TYPE.SCALE_ARMOR);
		craftableArmors.Add(TILE_OBJECT_TYPE.DRAGON_ARMOR);
		craftableAccessories.Add(TILE_OBJECT_TYPE.BELT);
	}

	[ContextMenu("Clear Armors Craftable")]
	public void ClearArmorCraftables()
	{
		craftableArmors.Clear();
	}

	[ContextMenu("Clear Accessories Craftable")]
	public void ClearAccessoriesCraftables()
	{
		craftableAccessories.Clear();
	}
}
