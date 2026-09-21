using System;
using DG.Tweening;
using EZObjectPools;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SubGoalNotificationItem : PooledObject, IPointerClickHandler, IEventSystemHandler
{
	[SerializeField]
	private RectTransform rectContent;

	[SerializeField]
	private CanvasGroup contentCanvasGroup;

	[SerializeField]
	private TextMeshProUGUI lblTitle;

	[SerializeField]
	private TextMeshProUGUI lblDescription;

	[SerializeField]
	private Image imgDestroyTimer;

	[SerializeField]
	private GameObject prefabEffect;

	private Action _onDestroyAction;

	private float _destroyTime;

	private float _activeTime;

	private bool _isHiding;

	private SubGoal _subGoal;

	public void Initialize(SubGoal p_subGoal, Action p_onDestroyAction, float p_destroyTime)
	{
		_subGoal = p_subGoal;
		lblTitle.text = p_subGoal.localizedTitle + "  <size=60%>+" + _subGoal.completionReward.GetCostStringWithIcon() + "  <i>" + LocalizationManager.Instance.GetLocalizedValue("UIStrings_Table", "SubGoal_Completed") + "</i>";
		lblDescription.text = p_subGoal.localizedDescriptiveName;
		_onDestroyAction = p_onDestroyAction;
		_destroyTime = p_destroyTime;
		_activeTime = 0f;
		PlayShowAnimation();
	}

	private void Update()
	{
		if (!_isHiding)
		{
			_activeTime += Time.deltaTime;
			imgDestroyTimer.DOFillAmount(1f - _activeTime / _destroyTime, 0.1f);
			if (_activeTime >= _destroyTime)
			{
				PlayHideAnimation(addDelay: true);
			}
		}
	}

	public override void Reset()
	{
		base.Reset();
		_onDestroyAction?.Invoke();
		_onDestroyAction = null;
		_activeTime = 0f;
		_destroyTime = 0f;
		_isHiding = false;
		rectContent.anchoredPosition = Vector2.zero;
		contentCanvasGroup.alpha = 1f;
		_subGoal = null;
	}

	private void PlayShowAnimation()
	{
		rectContent.anchoredPosition = new Vector2(570f, 0f);
		contentCanvasGroup.alpha = 0f;
		Sequence sequence = DOTween.Sequence();
		sequence.SetDelay(0.3f);
		sequence.Append(rectContent.DOAnchorPosX(0f, 0.5f));
		sequence.Join(contentCanvasGroup.DOFade(1f, 0.3f));
		sequence.AppendCallback(PlayStoreAnimation);
		sequence.PlayForward();
	}

	private void PlayHideAnimation(bool addDelay)
	{
		_isHiding = true;
		contentCanvasGroup.alpha = 1f;
		Sequence sequence = DOTween.Sequence();
		if (addDelay)
		{
			sequence.SetDelay(0.3f);
		}
		sequence.Append(contentCanvasGroup.DOFade(0f, 0.2f));
		sequence.OnComplete(delegate
		{
			ObjectPoolManager.Instance.DestroyObject(this);
		});
		sequence.PlayForward();
	}

	private void PlayStoreAnimation()
	{
		Vector3 position = rectContent.position;
		position.x -= 200f;
		position.z = 0f;
		GameObject effectGO = ObjectPoolManager.Instance.InstantiateObjectFromPool(prefabEffect.name, position, Quaternion.identity, rectContent.transform);
		effectGO.transform.position = position;
		Vector3 vector = ((_subGoal.completionReward.currency != CURRENCY.Spirit_Energy) ? PlayerUI.Instance.plaguePointLbl.transform.position : PlayerUI.Instance.spiritEnergyLabel.transform.position);
		Vector3 position2 = effectGO.transform.position;
		position2.z = 0f;
		Vector3 vector2 = vector;
		vector2.y -= 5f;
		vector2.z = 0f;
		effectGO.transform.DOPath(new Vector3[3] { vector, position2, vector2 }, 0.5f, PathType.CubicBezier).SetEase(Ease.InSine).OnComplete(delegate
		{
			OnEffectCompleted(effectGO);
		});
	}

	private void OnEffectCompleted(GameObject p_effect)
	{
		StoreTargetEffect component = p_effect.GetComponent<StoreTargetEffect>();
		component.trailParticles.Stop();
		component.SetImageState(p_state: false);
		_subGoal.GrantReward();
	}

	public void OnPointerClick(PointerEventData eventData)
	{
		if (!_isHiding)
		{
			PlayHideAnimation(addDelay: false);
		}
	}
}
