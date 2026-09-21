using UnityEngine;
using UnityEngine.UI;

public class PrismEventRequirementItem : MonoBehaviour
{
	[SerializeField]
	private Toggle _toggle;

	[SerializeField]
	private RuinarchText _requirementNameLbl;

	private PrismEventRequirement _requirement;

	public void Initialize(PrismEventRequirement p_requirement)
	{
		_requirement = p_requirement;
		_requirementNameLbl.text = _requirement.requirementText;
		SetIsRequirementMet(_requirement.IsRequirementSatisfied());
	}

	public void UpdateRequirementToggle()
	{
		SetIsRequirementMet(_requirement.IsRequirementSatisfied());
	}

	private void SetIsRequirementMet(bool p_state)
	{
		_toggle.isOn = p_state;
	}

	public bool IsUsed()
	{
		return _requirement != null;
	}
}
