using DG.Tweening;
using EZObjectPools;
using UnityEngine;
using UnityEngine.UI;

public class ConstructionProgress : PooledObject
{
	[SerializeField]
	private GameObject constructionGO;

	[SerializeField]
	private Image constructionFill;

	public void ShowConstructionVisual()
	{
		constructionGO.SetActive(value: true);
	}

	public void HideConstructionVisual()
	{
		constructionGO.SetActive(value: false);
	}

	public void SetConstructionProgress(float p_progress)
	{
		constructionFill.DOFillAmount(p_progress, 0.3f);
	}

	public override void Reset()
	{
		base.Reset();
		constructionFill.fillAmount = 0f;
	}
}
