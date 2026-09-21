using DG.Tweening;
using EZObjectPools;
using SpriteGlow;
using UnityEngine;

public class StonedEffect : PooledObject
{
	[SerializeField]
	private SpriteRenderer monsterSprite;

	[SerializeField]
	private SpriteGlowEffect glowEffect;

	public void PlayEffect(Sprite sprite)
	{
		monsterSprite.sprite = sprite;
		Sequence sequence = DOTween.Sequence();
		sequence.Append(DOTween.To(delegate(float value)
		{
			glowEffect.AlphaThreshold = value;
		}, 0f, 0.45f, 1f));
		sequence.Play();
	}
}
