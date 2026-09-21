using System;
using UnityEngine.Serialization;

[Serializable]
public class PlayerSkillSettings
{
	public int startingPortalLevel;

	public SKILL_COOLDOWN_SPEED cooldownSpeed;

	public SKILL_COST_AMOUNT costAmount;

	public SKILL_CHARGE_AMOUNT chargeAmount;

	public CORRUPTION_CHARGE_AMOUNT corruptionChargeAmount;

	[FormerlySerializedAs("threatAmount")]
	public RETALIATION retaliation;

	public PLAYER_ARCHETYPE[] forcedArchetypes;

	public OMNIPOTENT_MODE omnipotentMode;

	public PlayerSkillSettings()
	{
		startingPortalLevel = 1;
		cooldownSpeed = SKILL_COOLDOWN_SPEED.Normal;
		costAmount = SKILL_COST_AMOUNT.Normal;
		chargeAmount = SKILL_CHARGE_AMOUNT.Normal;
		corruptionChargeAmount = CORRUPTION_CHARGE_AMOUNT.Normal;
		retaliation = RETALIATION.Normal;
		forcedArchetypes = null;
		omnipotentMode = OMNIPOTENT_MODE.Disabled;
	}

	public void SetCooldownSpeed(SKILL_COOLDOWN_SPEED p_value)
	{
		cooldownSpeed = p_value;
	}

	public float GetCooldownSpeedModification()
	{
		return cooldownSpeed switch
		{
			SKILL_COOLDOWN_SPEED.None => 0f, 
			SKILL_COOLDOWN_SPEED.Half => 0.5f, 
			SKILL_COOLDOWN_SPEED.Normal => 1f, 
			SKILL_COOLDOWN_SPEED.Double => 2f, 
			_ => throw new ArgumentOutOfRangeException(), 
		};
	}

	public void SetManaCostAmount(SKILL_COST_AMOUNT p_value)
	{
		costAmount = p_value;
	}

	public float GetCostsModification()
	{
		return costAmount switch
		{
			SKILL_COST_AMOUNT.None => 0f, 
			SKILL_COST_AMOUNT.Half => 0.5f, 
			SKILL_COST_AMOUNT.Normal => 1f, 
			_ => throw new ArgumentOutOfRangeException(), 
		};
	}

	public void SetChargeAmount(SKILL_CHARGE_AMOUNT p_value)
	{
		chargeAmount = p_value;
	}

	private float GetChargeCostsModification()
	{
		switch (chargeAmount)
		{
		case SKILL_CHARGE_AMOUNT.Unlimited:
		case SKILL_CHARGE_AMOUNT.Normal:
			return 1f;
		case SKILL_CHARGE_AMOUNT.Half:
			return 0.5f;
		case SKILL_CHARGE_AMOUNT.Double:
			return 2f;
		default:
			throw new ArgumentOutOfRangeException();
		}
	}

	public void SetRetaliationState(RETALIATION p_value)
	{
		retaliation = p_value;
	}

	public void SetCorruptionChargeAmount(CORRUPTION_CHARGE_AMOUNT p_value)
	{
		corruptionChargeAmount = p_value;
	}

	private float GetCorruptionChargeModification()
	{
		switch (corruptionChargeAmount)
		{
		case CORRUPTION_CHARGE_AMOUNT.Unlimited:
		case CORRUPTION_CHARGE_AMOUNT.Normal:
			return 1f;
		case CORRUPTION_CHARGE_AMOUNT.Double:
			return 2f;
		default:
			throw new ArgumentOutOfRangeException();
		}
	}

	public void SetForcedArchetype(params PLAYER_ARCHETYPE[] p_archetype)
	{
		forcedArchetypes = p_archetype;
	}

	public void SetOmnipotentMode(OMNIPOTENT_MODE p_omnipotentMode)
	{
		omnipotentMode = p_omnipotentMode;
	}

	public bool PowerHasUnlimitedCharges(PLAYER_SKILL_TYPE p_type)
	{
		if (p_type == PLAYER_SKILL_TYPE.CORRUPT_TILE || p_type == PLAYER_SKILL_TYPE.DEMONIC_WALL || p_type == PLAYER_SKILL_TYPE.DECORATIONS)
		{
			return corruptionChargeAmount == CORRUPTION_CHARGE_AMOUNT.Unlimited;
		}
		return chargeAmount == SKILL_CHARGE_AMOUNT.Unlimited;
	}

	public float GetChargeModificationBasedOnSkillType(PLAYER_SKILL_TYPE p_type)
	{
		if (p_type == PLAYER_SKILL_TYPE.CORRUPT_TILE || p_type == PLAYER_SKILL_TYPE.DEMONIC_WALL || p_type == PLAYER_SKILL_TYPE.DECORATIONS)
		{
			return GetCorruptionChargeModification();
		}
		return GetChargeCostsModification();
	}

	public void SetStartingPortalLevel(int p_value)
	{
		startingPortalLevel = p_value;
	}
}
