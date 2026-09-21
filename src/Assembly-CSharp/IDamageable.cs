using Inner_Maps;
using UnityEngine;

public interface IDamageable
{
	string name { get; }

	int currentHP { get; }

	int maxHP { get; }

	ProjectileReceiver projectileReceiver { get; }

	LocationGridTile gridTileLocation { get; }

	BaseMapObjectVisual mapObjectVisual { get; }

	void AdjustHP(int amount, ELEMENTAL_TYPE elementalDamageType, bool triggerDeath = false, object source = null, CombatManager.ElementalTraitProcessor elementalTraitProcessor = null, bool showHPBar = false, float piercingPower = 0f, bool isPlayerSource = false, bool isTrueDamage = false);

	void OnHitByAttackFrom(Character characterThatAttacked, CombatState combatStateOfAttacker, ref string attackSummary);

	void OnHitByAttackFrom(Character characterThatAttacked, CombatState combatStateOfAttacker, int p_attackPower, ELEMENTAL_TYPE p_elementType, ref string attackSummary);

	bool CanBeDamaged();

	Vector3 GetProjectileTargetPosition();
}
