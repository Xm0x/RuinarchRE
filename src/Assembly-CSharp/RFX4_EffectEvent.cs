using UnityEngine;

public class RFX4_EffectEvent : MonoBehaviour
{
	public GameObject CharacterEffect;

	public Transform CharacterAttachPoint;

	public float CharacterEffect_DestroyTime = 10f;

	[Space]
	public GameObject CharacterEffect2;

	public Transform CharacterAttachPoint2;

	public float CharacterEffect2_DestroyTime = 10f;

	[Space]
	public GameObject MainEffect;

	public Transform AttachPoint;

	public float Effect_DestroyTime = 10f;

	[Space]
	public GameObject AdditionalEffect;

	public Transform AdditionalEffectAttachPoint;

	public float AdditionalEffect_DestroyTime = 10f;

	[HideInInspector]
	public bool IsMobile;

	public void ActivateEffect()
	{
		if (!(MainEffect == null))
		{
			GameObject gameObject = Object.Instantiate(MainEffect, AttachPoint.transform.position, AttachPoint.transform.rotation);
			UpdateEffectForMobileIsNeed(gameObject);
			if (Effect_DestroyTime > 0.01f)
			{
				Object.Destroy(gameObject, Effect_DestroyTime);
			}
		}
	}

	public void ActivateAdditionalEffect()
	{
		if (AdditionalEffect == null)
		{
			return;
		}
		if (AdditionalEffectAttachPoint != null)
		{
			GameObject gameObject = Object.Instantiate(AdditionalEffect, AdditionalEffectAttachPoint.transform.position, AdditionalEffectAttachPoint.transform.rotation);
			UpdateEffectForMobileIsNeed(gameObject);
			if (AdditionalEffect_DestroyTime > 0.01f)
			{
				Object.Destroy(gameObject, AdditionalEffect_DestroyTime);
			}
		}
		else
		{
			AdditionalEffect.SetActive(value: true);
		}
	}

	public void ActivateCharacterEffect()
	{
		if (!(CharacterEffect == null))
		{
			GameObject gameObject = Object.Instantiate(CharacterEffect, CharacterAttachPoint.transform.position, CharacterAttachPoint.transform.rotation, CharacterAttachPoint.transform);
			UpdateEffectForMobileIsNeed(gameObject);
			if (CharacterEffect_DestroyTime > 0.01f)
			{
				Object.Destroy(gameObject, CharacterEffect_DestroyTime);
			}
		}
	}

	public void ActivateCharacterEffect2()
	{
		if (!(CharacterEffect2 == null))
		{
			GameObject gameObject = Object.Instantiate(CharacterEffect2, CharacterAttachPoint2.transform.position, CharacterAttachPoint2.transform.rotation, CharacterAttachPoint2);
			UpdateEffectForMobileIsNeed(gameObject);
			if (CharacterEffect2_DestroyTime > 0.01f)
			{
				Object.Destroy(gameObject, CharacterEffect2_DestroyTime);
			}
		}
	}

	private void UpdateEffectForMobileIsNeed(GameObject instance)
	{
		_ = instance.GetComponent<RFX4_EffectSettings>() != null;
	}
}
