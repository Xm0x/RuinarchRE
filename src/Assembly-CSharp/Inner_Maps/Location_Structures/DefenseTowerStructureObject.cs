using UnityEngine;

namespace Inner_Maps.Location_Structures;

public class DefenseTowerStructureObject : LocationStructureObject
{
	[Header("Defense Tower Specific")]
	public GameObject rangeHighlight;

	public SpriteRenderer rangeHighlightSpriteRenderer;

	public DefenseTower connectedTower { get; private set; }

	public void SetConnectedTower(DefenseTower p_tower)
	{
		connectedTower = p_tower;
	}

	public void SetRangeHighlightState(bool p_state)
	{
		if (rangeHighlight.activeSelf != p_state)
		{
			rangeHighlight.SetActive(p_state);
		}
	}

	protected override void UpdateSortingOrders()
	{
		base.UpdateSortingOrders();
		rangeHighlightSpriteRenderer.sortingOrder = 910;
		rangeHighlightSpriteRenderer.sortingLayerName = "Area Maps";
	}

	public override void Reset()
	{
		base.Reset();
		connectedTower = null;
		rangeHighlight.gameObject.SetActive(value: false);
	}
}
