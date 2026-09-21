using DG.Tweening;
using EZObjectPools;
using SpriteGlow;
using UnityEngine;

public class BreedEffect : PooledObject
{
	[SerializeField]
	private SpriteRenderer monsterSprite;

	[SerializeField]
	private SpriteGlowEffect glowEffect;

	[SerializeField]
	private TrailRenderer trailEffect;

	public void PlayEffect(Sprite sprite)
	{
		trailEffect.gameObject.SetActive(value: true);
		monsterSprite.sprite = sprite;
		Vector3 vector = InnerMapCameraMove.Instance.camera.ScreenToWorldPoint(PlayerUI.Instance.monsterToggle.transform.position);
		Vector3 position = base.transform.position;
		position.x -= 5f;
		Vector3 vector2 = vector;
		vector2.y -= 5f;
		Sequence sequence = DOTween.Sequence();
		sequence.Append(DOTween.To(delegate(float value)
		{
			glowEffect.AlphaThreshold = value;
		}, 0f, 0.2f, 1f));
		sequence.Append(monsterSprite.transform.DOScale(new Vector3(0.5f, 0.5f, 0.5f), 0.1f).SetEase(Ease.InBounce));
		sequence.Append(base.transform.DOPath(new Vector3[3] { vector, position, vector2 }, 0.7f, PathType.CubicBezier).SetEase(Ease.InSine));
		sequence.OnComplete(OnCompleteSequence);
		sequence.Play();
	}

	private void OnCompleteSequence()
	{
		AudioManager.Instance.PlayParticleMagnet();
		PlayerUI.Instance.DoMonsterTabPunchEffect();
		ObjectPoolManager.Instance.DestroyObject(this);
	}

	public override void Reset()
	{
		base.Reset();
		trailEffect.gameObject.SetActive(value: false);
	}
}
