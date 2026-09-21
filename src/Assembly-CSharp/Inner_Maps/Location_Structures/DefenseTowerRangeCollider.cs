using UnityEngine;

namespace Inner_Maps.Location_Structures;

public class DefenseTowerRangeCollider : MonoBehaviour
{
	[SerializeField]
	private DefenseTowerStructureObject _structureObject;

	protected void OnTriggerEnter2D(Collider2D collision)
	{
		BaseVisionTrigger andAddPOIVisionTriggerFromCache = CharacterManager.Instance.GetAndAddPOIVisionTriggerFromCache(collision);
		if (andAddPOIVisionTriggerFromCache != null && andAddPOIVisionTriggerFromCache.damageable is Character p_character)
		{
			_structureObject.connectedTower?.TryAddHostileInRange(p_character);
		}
	}

	protected void OnTriggerExit2D(Collider2D collision)
	{
		BaseVisionTrigger andAddPOIVisionTriggerFromCache = CharacterManager.Instance.GetAndAddPOIVisionTriggerFromCache(collision);
		if (andAddPOIVisionTriggerFromCache != null && andAddPOIVisionTriggerFromCache.damageable is Character p_character)
		{
			_structureObject.connectedTower?.RemoveHostileInRange(p_character);
		}
	}
}
