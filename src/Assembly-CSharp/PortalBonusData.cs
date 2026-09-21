using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Portal Bonus Data", menuName = "Scriptable Objects/Portal Bonus Data")]
public class PortalBonusData : ScriptableObject
{
	[Serializable]
	public class MigrationBonus
	{
		public int bonusTalentPoints;

		public float weaponEquipmentChance;

		public float armorEquipmentChance;

		public float accessoryEquipmentChance;
	}

	[Header("Initial Bonus")]
	public List<MigrationBonus> migrationBonusPerLevel = new List<MigrationBonus>();
}
