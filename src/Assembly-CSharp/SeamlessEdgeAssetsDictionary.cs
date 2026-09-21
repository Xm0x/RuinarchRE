using System;
using System.Collections.Generic;
using Inner_Maps;
using UnityEngine.Tilemaps;

[Serializable]
public class SeamlessEdgeAssetsDictionary : SerializableDictionary<LocationGridTile.Ground_Type, List<TileBase>, TileBaseListStorage>
{
}
