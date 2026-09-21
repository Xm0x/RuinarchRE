using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class TileObjectSlotDictionary : SerializableDictionary<Sprite, List<TileObjectSlotSetting>, TileObjectSlotListStorage>
{
}
