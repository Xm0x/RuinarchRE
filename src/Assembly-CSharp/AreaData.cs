using System;
using UnityEngine;

[Serializable]
public class AreaData
{
	public int id;

	public string persistentID;

	public int xCoordinate;

	public int yCoordinate;

	public string areaName;

	public Vector2 position => new Vector2(xCoordinate, yCoordinate);
}
