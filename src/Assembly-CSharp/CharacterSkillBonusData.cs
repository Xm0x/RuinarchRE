using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Villager Skill Bonus Data", menuName = "Scriptable Objects/VIllager Skills/VillagerSkillBonusData")]
public class CharacterSkillBonusData : ScriptableObject
{
	public List<SkillBonus> bonusPerLevel = new List<SkillBonus>();
}
