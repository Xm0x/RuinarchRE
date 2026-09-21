using System;
using System.Collections.Generic;

[Serializable]
public class BiomeMonsterDictionary : SerializableDictionary<BIOMES, List<MonsterSetting>, MonsterSettingListStorage>
{
}
