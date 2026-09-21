using UnityEngine;

namespace Inner_Maps.Location_Structures;

[RequireComponent(typeof(BoxCollider2D))]
public class LocationStructureObjectClickCollider : MonoBehaviour
{
	[SerializeField]
	private BoxCollider2D clickCollider;

	public LocationStructureObject structureObject;

	private void Awake()
	{
		base.gameObject.tag = "Location Structure Object";
	}

	public void Enable()
	{
		base.enabled = true;
		clickCollider.enabled = true;
	}

	public void Disable()
	{
		base.enabled = false;
		clickCollider.enabled = false;
	}
}
