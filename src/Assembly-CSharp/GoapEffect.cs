using System;

[Serializable]
public class GoapEffect
{
	public GOAP_EFFECT_CONDITION conditionType;

	public string conditionKey;

	public bool isKeyANumber;

	public GOAP_EFFECT_TARGET target;

	public GoapEffect(GOAP_EFFECT_CONDITION conditionType, string conditionKey, bool isKeyANumber, GOAP_EFFECT_TARGET target)
	{
		this.conditionType = conditionType;
		this.conditionKey = conditionKey;
		this.isKeyANumber = isKeyANumber;
		this.target = target;
	}

	public bool IsSameGoal(GoapEffect p_goapEffect)
	{
		if (conditionType == p_goapEffect.conditionType)
		{
			return conditionKey == p_goapEffect.conditionKey;
		}
		return false;
	}

	public override string ToString()
	{
		return $"{conditionType} - {conditionKey} - {target}";
	}
}
