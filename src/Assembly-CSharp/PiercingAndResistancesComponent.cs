using System.Collections.Generic;
using Inner_Maps.Location_Structures;
using UnityEngine;
using UtilityScripts;

public class PiercingAndResistancesComponent : CharacterComponent
{
	public float piercingPower { get; private set; }

	public Dictionary<RESISTANCE, float> resistances { get; private set; }

	public Dictionary<RESISTANCE, float> resistancesMultipliers { get; private set; }

	public float piercingMultiplier { get; private set; }

	public float basePiercing { get; private set; }

	public PiercingAndResistancesComponent()
	{
		resistances = new Dictionary<RESISTANCE, float>();
		resistancesMultipliers = new Dictionary<RESISTANCE, float>();
	}

	public PiercingAndResistancesComponent(SaveDataPiercingAndResistancesComponent data)
	{
		piercingPower = data.piercingPower;
		if (data.resistances != null && data.resistances.Count > 0)
		{
			resistances = new Dictionary<RESISTANCE, float>(data.resistances);
		}
		else
		{
			resistances = new Dictionary<RESISTANCE, float>();
		}
		if (data.resistancesMultipliers != null && data.resistancesMultipliers.Count > 0)
		{
			resistancesMultipliers = new Dictionary<RESISTANCE, float>(data.resistancesMultipliers);
		}
		else
		{
			resistancesMultipliers = new Dictionary<RESISTANCE, float>();
		}
		piercingMultiplier = data.piercingMultiplier;
		basePiercing = data.basePiercing;
	}

	public void AdjustBasePiercing(float p_amount)
	{
		basePiercing += p_amount;
		UpdatePiercing();
	}

	public void SetBasePiercing(float p_amount)
	{
		basePiercing = p_amount;
		UpdatePiercing();
	}

	public void AdjustPiercingMultiplier(float p_amount)
	{
		piercingMultiplier += p_amount;
		UpdatePiercing();
	}

	private void UpdatePiercing()
	{
		piercingPower = basePiercing + basePiercing * (piercingMultiplier / 100f);
		piercingPower = Mathf.Round(piercingPower);
		Messenger.Broadcast(UISignals.UPDATE_PIERCING_AND_RESISTANCE_INFO, base.owner);
	}

	public void AdjustResistance(RESISTANCE p_resistance, float p_value, bool shouldBroadcastSignal = true)
	{
		if (!resistances.ContainsKey(p_resistance))
		{
			resistances.Add(p_resistance, 0f);
		}
		resistances[p_resistance] += p_value;
		if (shouldBroadcastSignal)
		{
			Messenger.Broadcast(UISignals.UPDATE_PIERCING_AND_RESISTANCE_INFO, base.owner);
		}
	}

	public void AdjustAllResistances(float p_value)
	{
		RESISTANCE[] enumValues = CollectionUtilities.GetEnumValues<RESISTANCE>();
		foreach (RESISTANCE rESISTANCE in enumValues)
		{
			if (rESISTANCE != RESISTANCE.None)
			{
				AdjustResistance(rESISTANCE, p_value, shouldBroadcastSignal: false);
			}
		}
		Messenger.Broadcast(UISignals.UPDATE_PIERCING_AND_RESISTANCE_INFO, base.owner);
	}

	public void AdjustElementalResistances(float p_value)
	{
		RESISTANCE[] enumValues = CollectionUtilities.GetEnumValues<RESISTANCE>();
		foreach (RESISTANCE rESISTANCE in enumValues)
		{
			if (rESISTANCE != RESISTANCE.None && rESISTANCE.IsElemental())
			{
				AdjustResistance(rESISTANCE, p_value, shouldBroadcastSignal: false);
			}
		}
		Messenger.Broadcast(UISignals.UPDATE_PIERCING_AND_RESISTANCE_INFO, base.owner);
	}

	public void AdjustSecondaryResistances(float p_value)
	{
		RESISTANCE[] enumValues = CollectionUtilities.GetEnumValues<RESISTANCE>();
		foreach (RESISTANCE rESISTANCE in enumValues)
		{
			if (rESISTANCE != RESISTANCE.None && rESISTANCE.IsSecondary())
			{
				AdjustResistance(rESISTANCE, p_value, shouldBroadcastSignal: false);
			}
		}
		Messenger.Broadcast(UISignals.UPDATE_PIERCING_AND_RESISTANCE_INFO, base.owner);
	}

	public void AdjustRandomResistance(float p_value)
	{
		RESISTANCE[] enumValues = CollectionUtilities.GetEnumValues<RESISTANCE>();
		int num = GameUtilities.RandomBetweenTwoNumbers(1, enumValues.Length - 1);
		RESISTANCE p_resistance = enumValues[num];
		AdjustResistance(p_resistance, p_value, shouldBroadcastSignal: false);
		Messenger.Broadcast(UISignals.UPDATE_PIERCING_AND_RESISTANCE_INFO, base.owner);
	}

	public void SetResistance(RESISTANCE p_resistance, float p_value)
	{
		if (!resistances.ContainsKey(p_resistance))
		{
			resistances.Add(p_resistance, 0f);
		}
		resistances[p_resistance] = p_value;
		Messenger.Broadcast(UISignals.UPDATE_PIERCING_AND_RESISTANCE_INFO, base.owner);
	}

	public float GetResistanceValue(RESISTANCE p_resistance)
	{
		if (resistances.ContainsKey(p_resistance))
		{
			float num = resistances[p_resistance];
			float num2 = 1f;
			if (resistancesMultipliers.ContainsKey(p_resistance))
			{
				num2 = resistancesMultipliers[p_resistance];
				num2 = Mathf.Max(num2, 1f);
			}
			return num * num2;
		}
		return 0f;
	}

	public float GetResistanceValue(ELEMENTAL_TYPE p_element)
	{
		RESISTANCE resistance = p_element.GetResistance();
		return GetResistanceValue(resistance);
	}

	public void ModifyValueByResistance(ref int p_value, ELEMENTAL_TYPE p_element, float piercingPower)
	{
		float resistanceValue = GetResistanceValue(p_element);
		CombatManager.ModifyValueByPiercingAndResistance(ref p_value, piercingPower, resistanceValue);
	}

	public void AdjustResistanceMultiplier(RESISTANCE p_resistance, float p_value)
	{
		if (!resistancesMultipliers.ContainsKey(p_resistance))
		{
			resistancesMultipliers.Add(p_resistance, 0f);
		}
		resistancesMultipliers[p_resistance] += p_value;
		Messenger.Broadcast(UISignals.UPDATE_PIERCING_AND_RESISTANCE_INFO, base.owner);
	}

	public void CheckIfStructureIsStillReferenced(LocationStructure p_structure)
	{
	}
}
