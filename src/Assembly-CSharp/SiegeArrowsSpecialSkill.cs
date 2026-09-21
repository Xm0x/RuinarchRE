using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Inner_Maps;
using Traits;
using UnityEngine;
using UtilityScripts;

public class SiegeArrowsSpecialSkill : CombatSpecialSkill
{
	public SiegeArrowsSpecialSkill()
		: base(COMBAT_SPECIAL_SKILL.Siege_Arrows, COMBAT_SPECIAL_SKILL_TARGET.Multiple, COMBAT_SPECIAL_SKILL_CATEGORY.Physical, 40, 3)
	{
	}

	public override bool TryActivateSkill(Character p_character)
	{
		bool result = false;
		if (p_character.gridTileLocation != null && p_character.stateComponent.currentState is CombatState combatState && p_character.combatComponent.isInActualCombat && combatState.currentClosestHostile is TileObject { gridTileLocation: not null } tileObject && (tileObject.isDamageContributorToStructure || tileObject is BlockWall || tileObject is IceBlockWall || tileObject is OreVein || tileObject is ThinWall))
		{
			List<ITraitable> list = RuinarchListPool<ITraitable>.Claim();
			List<LocationGridTile> list2 = RuinarchListPool<LocationGridTile>.Claim();
			LocationGridTile locationGridTile = tileObject.gridTileLocation;
			if (tileObject.isDamageContributorToStructure)
			{
				LocationGridTile centerTile = tileObject.gridTileLocation.structure.GetCenterTile();
				if (centerTile != null)
				{
					locationGridTile = centerTile;
				}
			}
			locationGridTile.PopulateTilesInRadius(list2, 2, 0, includeCenterTile: true, includeTilesInDifferentStructure: true);
			for (int i = 0; i < list2.Count; i++)
			{
				list2[i].PopulateTraitablesOnTile(list);
			}
			int num = GameUtilities.RandomBetweenTwoNumbers(4, list2.Count);
			RuinarchListPool<LocationGridTile>.Release(list2);
			if (list.Count((ITraitable t) => t is TileObject) > 0)
			{
				result = true;
				if (p_character.hasMarker)
				{
					InnerMapManager.Instance.ShowTextEffect("Siege Arrows!", Color.yellow, p_character.worldPosition);
				}
				int damage = 50 + p_character.TryGetTalentLevel(CHARACTER_TALENT.Martial_Arts) * 10;
				list.Shuffle();
				for (int num2 = 0; num2 < list.Count; num2++)
				{
					if (list[num2] is TileObject tileObject2)
					{
						if (num2 <= num)
						{
							GameManager.Instance.StartCoroutine(DelayedDamage(tileObject2, p_character, damage));
						}
						else
						{
							DealDamage(tileObject2, p_character, damage, combatState);
						}
					}
				}
			}
			RuinarchListPool<ITraitable>.Release(list);
		}
		return result;
	}

	public override bool CanBeLearnedByCharacter(Character p_character)
	{
		if (p_character.characterClass.attackType == ATTACK_TYPE.PHYSICAL)
		{
			return p_character.combatComponent.rangeType == RANGE_TYPE.RANGED;
		}
		return false;
	}

	private IEnumerator DelayedDamage(TileObject obj, Character p_character, int damage)
	{
		yield return GameUtilities.waitForHalfSecond;
		GameManager.Instance.CreateParticleEffectAt(obj, PARTICLE_EFFECT.Siege_Arrow);
		yield return GameUtilities.waitFor1Second;
		if (obj.gridTileLocation != null)
		{
			CombatState combatState = p_character.stateComponent.currentState as CombatState;
			DealDamage(obj, p_character, damage, combatState);
		}
	}

	private void DealDamage(TileObject p_target, Character p_character, int damage, CombatState combatState)
	{
		string attackSummary = string.Empty;
		p_target.OnHitByAttackFrom(p_character, combatState, damage, ELEMENTAL_TYPE.Fire, ref attackSummary);
	}
}
