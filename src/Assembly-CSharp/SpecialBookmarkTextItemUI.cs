using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class SpecialBookmarkTextItemUI : BookmarkTextItemUI
{
	[SerializeField]
	private Image imgGlowEffect;

	private void OnEnable()
	{
		imgGlowEffect.DOFade(0f, 2f).SetEase(Ease.InQuart).SetLoops(-1, LoopType.Yoyo);
	}

	private void OnDisable()
	{
		imgGlowEffect.DOKill(complete: true);
		Color color = imgGlowEffect.color;
		color.a = 1f;
		imgGlowEffect.color = color;
	}
}
