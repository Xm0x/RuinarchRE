using System;
using System.Collections.Generic;
using Inner_Maps.Location_Structures;
using UnityEngine;

[Serializable]
public class LocationStructurePrefabDictionary : SerializableDictionary<StructureSetting, List<GameObject>, GameObjectListStorage>
{
}
