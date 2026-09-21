using System;

public class PrismEventRequirement
{
	private string _requirementText;

	private Func<bool> _isRequirementSatisfied;

	public string requirementText => _requirementText;

	public PrismEventRequirement(string p_localizationKey, Func<bool> p_requirement)
	{
		_requirementText = LocalizationManager.Instance.GetLocalizedValue("PrismEvents_Table", p_localizationKey);
		_isRequirementSatisfied = p_requirement;
	}

	public bool IsRequirementSatisfied()
	{
		return _isRequirementSatisfied();
	}
}
