using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class TileSpriteCorruptionListDictionary : SerializableDictionary<Sprite, List<GameObject>, CorruptionObjectsListStorage>
{
}
