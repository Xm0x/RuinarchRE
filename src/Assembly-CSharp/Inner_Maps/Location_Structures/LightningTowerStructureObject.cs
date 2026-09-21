using UnityEngine;

namespace Inner_Maps.Location_Structures;

public class LightningTowerStructureObject : DefenseTowerStructureObject
{
	[SerializeField]
	private GameObject lightningParticleParent;

	protected override void Awake()
	{
		base.Awake();
		lightningParticleParent.gameObject.SetActive(value: false);
	}

	public void SetLightningParticleParentState(bool p_state)
	{
		lightningParticleParent.SetActive(p_state);
	}

	public override void Reset()
	{
		base.Reset();
		lightningParticleParent.SetActive(value: false);
	}
}
