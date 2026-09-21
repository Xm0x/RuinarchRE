using System;
using UnityEngine;

[Serializable]
public class CharacterProgressionBonusData
{
	[Space]
	[Header("Elemental Resistance")]
	[Space]
	[SerializeField]
	private float m_fireResistanceBonus;

	[SerializeField]
	private float m_earthResistanceBonus;

	[SerializeField]
	private float m_windResistanceBonus;

	[SerializeField]
	private float m_waterResistanceBonus;

	[Space]
	[SerializeField]
	private float m_allElementalresistanceBonus;

	[Space]
	[Header("Secondary Resistance")]
	[Space]
	[SerializeField]
	private float m_poisonResistanceBonus;

	[SerializeField]
	private float m_iceResistanceBonus;

	[SerializeField]
	private float m_electricResistanceBonus;

	[Space]
	[SerializeField]
	private float m_allSecondaryresistanceBonus;

	[Space]
	[Space]
	[Header("Other")]
	[SerializeField]
	private float m_piercingBonus;

	[SerializeField]
	private float m_mentalResistanceBonus;

	[SerializeField]
	private float m_physicalResistanceBonus;

	public float GetBonusBaseOnElement(RESISTANCE p_bonusForThisElement)
	{
		return p_bonusForThisElement switch
		{
			RESISTANCE.Fire => m_fireResistanceBonus, 
			RESISTANCE.Poison => m_poisonResistanceBonus, 
			RESISTANCE.Water => m_waterResistanceBonus, 
			RESISTANCE.Ice => m_iceResistanceBonus, 
			RESISTANCE.Electric => m_electricResistanceBonus, 
			RESISTANCE.Earth => m_earthResistanceBonus, 
			RESISTANCE.Wind => m_windResistanceBonus, 
			RESISTANCE.Physical => m_physicalResistanceBonus, 
			RESISTANCE.Mental => m_mentalResistanceBonus, 
			_ => 0f, 
		};
	}

	public float GetPiercingBonus()
	{
		return m_piercingBonus;
	}

	public float GetPhysicalResistanceBonus()
	{
		return m_physicalResistanceBonus;
	}

	public float GetMentalResistanceBonus()
	{
		return m_mentalResistanceBonus;
	}

	public float GetAllElementalResistanceBonus()
	{
		return m_allElementalresistanceBonus;
	}

	public float GetAllSecondaryResistanceBonus()
	{
		return m_allSecondaryresistanceBonus;
	}
}
