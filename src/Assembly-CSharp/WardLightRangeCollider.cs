using UnityEngine;

public class WardLightRangeCollider : MonoBehaviour
{
	[SerializeField]
	private WardLightGameObject wardLightGameObject;

	protected void OnTriggerEnter2D(Collider2D collision)
	{
		BaseVisionTrigger andAddPOIVisionTriggerFromCache = CharacterManager.Instance.GetAndAddPOIVisionTriggerFromCache(collision);
		if (andAddPOIVisionTriggerFromCache != null && andAddPOIVisionTriggerFromCache.damageable is Character p_character)
		{
			wardLightGameObject.ExecuteOnAddAction(p_character);
		}
	}

	protected void OnTriggerExit2D(Collider2D collision)
	{
		BaseVisionTrigger andAddPOIVisionTriggerFromCache = CharacterManager.Instance.GetAndAddPOIVisionTriggerFromCache(collision);
		if (andAddPOIVisionTriggerFromCache != null && andAddPOIVisionTriggerFromCache.damageable is Character p_character)
		{
			wardLightGameObject.ExecuteOnRemoveAction(p_character);
		}
	}
}
