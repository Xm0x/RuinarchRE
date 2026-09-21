using System;

[Serializable]
public class SaveDataCharacterNeedsComponent : SaveData<CharacterNeedsComponent>
{
	public int doNotGetHungry;

	public int doNotGetTired;

	public int doNotGetBored;

	public int doNotGetDrained;

	public float tiredness;

	public float tirednessDecreaseRate;

	public float fullness;

	public float fullnessDecreaseRate;

	public float happiness;

	public float happinessDecreaseRate;

	public float happinessDecreaseRateDivisor;

	public float happinessDecreaseRateMultiplier;

	public float stamina;

	public float staminaDecreaseRate;

	public float baseStaminaDecreaseRate;

	public float hope;

	public bool hasForcedFullness;

	public bool hasForcedTiredness;

	public bool hasForcedSecondHappiness;

	public override void Save(CharacterNeedsComponent data)
	{
		doNotGetHungry = data.doNotGetHungry;
		doNotGetTired = data.doNotGetTired;
		doNotGetBored = data.doNotGetBored;
		doNotGetDrained = data.doNotGetDrained;
		tiredness = data.tiredness;
		tirednessDecreaseRate = data.tirednessDecreaseRate;
		fullness = data.fullness;
		fullnessDecreaseRate = data.fullnessDecreaseRate;
		happiness = data.happiness;
		happinessDecreaseRate = data.happinessDecreaseRate;
		happinessDecreaseRateDivisor = data.happinessDecreaseRateDivisor;
		happinessDecreaseRateMultiplier = data.happinessDecreaseRateMultiplier;
		stamina = data.stamina;
		staminaDecreaseRate = data.staminaDecreaseRate;
		baseStaminaDecreaseRate = data.baseStaminaDecreaseRate;
		hope = data.hope;
		hasForcedFullness = data.hasForcedFullness;
		hasForcedTiredness = data.hasForcedTiredness;
		hasForcedSecondHappiness = data.hasForcedSecondHappiness;
	}

	public override CharacterNeedsComponent Load()
	{
		return new CharacterNeedsComponent(this);
	}
}
