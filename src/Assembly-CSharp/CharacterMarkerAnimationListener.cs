using UnityEngine;

[ExecuteInEditMode]
public class CharacterMarkerAnimationListener : MonoBehaviour
{
	[SerializeField]
	private CharacterMarker parentMarker;

	private float _timeElapsed;

	private bool isExecutingAttack;

	private const float AttackTime = 0.16f;

	private IPointOfInterest _dummyAttackTarget;

	private void OnAttackExecuted()
	{
		if (parentMarker.character.stateComponent.currentState is CombatState { isExecutingAttack: not false } combatState)
		{
			if (parentMarker.character.combatComponent.rangeType == RANGE_TYPE.RANGED)
			{
				CombatManager.Instance.CreateProjectile(parentMarker, combatState.currentClosestHostile, combatState);
			}
			else
			{
				combatState.OnAttackHit(combatState.currentClosestHostile);
				if (parentMarker.character == null)
				{
					return;
				}
				CombatManager.Instance.PlayAudioForMeleeAttack(parentMarker.character, combatState.currentClosestHostile);
			}
			MountAttack(combatState.currentClosestHostile);
			combatState.isExecutingAttack = false;
		}
		else if (_dummyAttackTarget != null)
		{
			if (parentMarker.character.combatComponent.rangeType == RANGE_TYPE.RANGED)
			{
				CombatManager.Instance.CreateDummyProjectile(parentMarker, _dummyAttackTarget);
			}
			else
			{
				CombatManager.Instance.CreateHitEffectAt(_dummyAttackTarget, parentMarker.character.combatComponent.currentElement.type);
				CombatManager.Instance.PlayAudioForMeleeAttack(parentMarker.character, _dummyAttackTarget);
			}
			_dummyAttackTarget = null;
		}
	}

	public void SetDummyAttackTarget(IPointOfInterest p_target)
	{
		_dummyAttackTarget = p_target;
	}

	public void StartAttackExecution()
	{
		isExecutingAttack = true;
	}

	private void Update()
	{
		if (isExecutingAttack)
		{
			_timeElapsed += Time.deltaTime;
			if (_timeElapsed >= 0.16f)
			{
				_timeElapsed = 0f;
				isExecutingAttack = false;
				OnAttackExecuted();
			}
		}
	}

	private void MountAttack(IDamageable p_target)
	{
		if (parentMarker.character.mountComponent.IsMounting())
		{
			parentMarker.character.mountComponent.TryAttack(p_target);
		}
	}

	public void Reset()
	{
		isExecutingAttack = false;
	}
}
