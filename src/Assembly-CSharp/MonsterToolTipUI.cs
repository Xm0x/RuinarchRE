using UnityEngine;

public class MonsterToolTipUI : MonoBehaviour
{
	public GameObject toolTipParent;

	public GameObject tooltipWithChargeParent;

	public GameObject tooltipWithoutChargeParent;

	public RuinarchText txtToolTipDisplayName;

	public RuinarchText txtToolTipDisplayDescription;

	public RuinarchText txtToolTipChargingTime;

	public void DisplayToolTipWithCharge(string p_name, string p_description, string p_chargeDisplay)
	{
		toolTipParent.SetActive(value: true);
		tooltipWithoutChargeParent.SetActive(value: false);
		tooltipWithChargeParent.SetActive(value: true);
		txtToolTipDisplayName.text = p_name;
		txtToolTipDisplayDescription.text = p_description;
		txtToolTipChargingTime.text = p_chargeDisplay;
	}

	public void DisplayToolTipWithoutCharge(string p_name, string p_description)
	{
		toolTipParent.SetActive(value: true);
		tooltipWithoutChargeParent.SetActive(value: true);
		tooltipWithChargeParent.SetActive(value: false);
		txtToolTipDisplayName.text = p_name;
		txtToolTipDisplayDescription.text = p_description;
	}

	public void HideToolTip()
	{
		toolTipParent.SetActive(value: false);
		tooltipWithoutChargeParent.SetActive(value: false);
		tooltipWithChargeParent.SetActive(value: false);
	}
}
