using UnityEngine;

public class PortalBonusDataHandler : MonoBehaviour
{
	public static PortalBonusDataHandler Instance;

	public PortalBonusData portalBonus;

	private void OnEnable()
	{
		if (Instance == null)
		{
			Instance = this;
		}
	}

	private void OnDisable()
	{
		if (Instance == this)
		{
			Instance = null;
		}
	}
}
