using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

[Serializable]
public class BiomeTileObjectTileSetting
{
	public Sprite[] activeTile;

	public Sprite[] inactiveTile;

	public List<TileBase> tileBase;

	public BiomeTileObjectTileSetting()
	{
		tileBase = new List<TileBase>();
	}
}
