using DG.Tweening;
using UnityEngine;

public class Materials : MonoBehaviour
{
	public GameObject target;

	public Color toColor;

	private Tween colorTween;

	private Tween emissionTween;

	private Tween offsetTween;

	private void Start()
	{
		Material material = target.GetComponent<Renderer>().material;
		colorTween = material.DOColor(toColor, 1f).SetLoops(-1, LoopType.Yoyo).Pause();
		emissionTween = material.DOColor(new Color(0f, 0f, 0f, 0f), "_EmissionColor", 1f).SetLoops(-1, LoopType.Yoyo).Pause();
		offsetTween = material.DOOffset(new Vector2(1f, 1f), 1f).SetEase(Ease.Linear).SetLoops(-1, LoopType.Incremental)
			.Pause();
	}

	public void ToggleColor()
	{
		colorTween.TogglePause();
	}

	public void ToggleEmission()
	{
		emissionTween.TogglePause();
	}

	public void ToggleOffset()
	{
		offsetTween.TogglePause();
	}
}
