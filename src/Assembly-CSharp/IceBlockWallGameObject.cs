using Inner_Maps;
using UnityEngine;

public class IceBlockWallGameObject : TileObjectGameObject
{
	public GameObject leftBotImpassable;

	public GameObject leftTopImpassable;

	public GameObject topLeftImpassable;

	public GameObject topRightImpassable;

	public GameObject rightTopImpassable;

	public GameObject rightBotImpassable;

	public GameObject botRightImpassable;

	public GameObject botLeftImpassable;

	public void EvaluateImpassables(IceBlockWall blockWall)
	{
		LocationGridTile locationGridTile = base.obj.gridTileLocation;
		if (locationGridTile == null)
		{
			locationGridTile = base.obj.previousTile;
		}
		if (locationGridTile != null)
		{
			LocationGridTile neighbourAtDirection = locationGridTile.GetNeighbourAtDirection(GridNeighbourDirection.North_West);
			LocationGridTile neighbourAtDirection2 = locationGridTile.GetNeighbourAtDirection(GridNeighbourDirection.North_East);
			LocationGridTile neighbourAtDirection3 = locationGridTile.GetNeighbourAtDirection(GridNeighbourDirection.South_West);
			LocationGridTile neighbourAtDirection4 = locationGridTile.GetNeighbourAtDirection(GridNeighbourDirection.South_East);
			LocationGridTile neighbourAtDirection5 = locationGridTile.GetNeighbourAtDirection(GridNeighbourDirection.North);
			LocationGridTile neighbourAtDirection6 = locationGridTile.GetNeighbourAtDirection(GridNeighbourDirection.West);
			LocationGridTile neighbourAtDirection7 = locationGridTile.GetNeighbourAtDirection(GridNeighbourDirection.East);
			LocationGridTile neighbourAtDirection8 = locationGridTile.GetNeighbourAtDirection(GridNeighbourDirection.South);
			ActivateDeactivateImpassables(neighbourAtDirection2, neighbourAtDirection5, topRightImpassable, neighbourAtDirection7, rightTopImpassable);
			ActivateDeactivateImpassables(neighbourAtDirection, neighbourAtDirection5, topLeftImpassable, neighbourAtDirection6, leftTopImpassable);
			ActivateDeactivateImpassables(neighbourAtDirection4, neighbourAtDirection8, botRightImpassable, neighbourAtDirection7, rightBotImpassable);
			ActivateDeactivateImpassables(neighbourAtDirection3, neighbourAtDirection8, botLeftImpassable, neighbourAtDirection6, leftBotImpassable);
			blockWall.SetImpassables(leftBotImpassable.activeSelf, leftTopImpassable.activeSelf, topLeftImpassable.activeSelf, topRightImpassable.activeSelf, rightTopImpassable.activeSelf, rightBotImpassable.activeSelf, botRightImpassable.activeSelf, botLeftImpassable.activeSelf);
			ApplyGraphUpdate();
		}
	}

	private void ActivateDeactivateImpassables(LocationGridTile p_diagonalNeighbourToCheck, LocationGridTile p_firstNeighbour, GameObject p_firstNeighbourImpassable, LocationGridTile p_secondNeighbour, GameObject p_secondNeighbourImpassable)
	{
		if (p_diagonalNeighbourToCheck == null)
		{
			return;
		}
		if (!p_diagonalNeighbourToCheck.IsPassable())
		{
			if (p_firstNeighbour != null)
			{
				bool active = p_secondNeighbour?.IsPassable() ?? false;
				p_firstNeighbourImpassable.SetActive(active);
			}
			if (p_secondNeighbour != null)
			{
				bool active2 = p_firstNeighbour?.IsPassable() ?? false;
				p_secondNeighbourImpassable.SetActive(active2);
			}
		}
		else
		{
			if (p_firstNeighbour != null)
			{
				p_firstNeighbourImpassable.SetActive(value: false);
			}
			if (p_secondNeighbour != null)
			{
				p_secondNeighbourImpassable.SetActive(value: false);
			}
		}
	}

	public override void Reset()
	{
		base.Reset();
		leftBotImpassable.SetActive(value: false);
		leftTopImpassable.SetActive(value: false);
		topLeftImpassable.SetActive(value: false);
		topRightImpassable.SetActive(value: false);
		rightTopImpassable.SetActive(value: false);
		rightBotImpassable.SetActive(value: false);
		botRightImpassable.SetActive(value: false);
		botLeftImpassable.SetActive(value: false);
	}
}
