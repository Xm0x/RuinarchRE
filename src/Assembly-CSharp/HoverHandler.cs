using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using UnityEngine.UI;

public class HoverHandler : MonoBehaviour, IPointerEnterHandler, IEventSystemHandler, IPointerExitHandler
{
	protected bool isHovering;

	[SerializeField]
	private UIHoverPosition tooltipPos;

	[SerializeField]
	protected string tooltipHeader;

	[SerializeField]
	protected bool ignoreInteractable;

	[SerializeField]
	protected UnityEvent onHoverOverAction;

	[SerializeField]
	protected UnityEvent onHoverExitAction;

	[SerializeField]
	protected bool executeHoverEnterActionPerFrame = true;

	protected Selectable selectable;

	private void OnEnable()
	{
		selectable = GetComponent<Selectable>();
	}

	private void OnDisable()
	{
		isHovering = false;
		onHoverExitAction?.Invoke();
	}

	private void OnDestroy()
	{
		isHovering = false;
	}

	public virtual void OnPointerEnter(PointerEventData eventData)
	{
		if (ignoreInteractable || !(selectable != null) || selectable.IsInteractable())
		{
			if (executeHoverEnterActionPerFrame)
			{
				isHovering = true;
				return;
			}
			isHovering = true;
			onHoverOverAction?.Invoke();
		}
	}

	public virtual void OnPointerExit(PointerEventData eventData)
	{
		if (ignoreInteractable || !(selectable != null) || selectable.IsInteractable())
		{
			isHovering = false;
			onHoverExitAction?.Invoke();
		}
	}

	public void SetOnHoverOverAction(UnityAction e)
	{
		onHoverOverAction.RemoveAllListeners();
		onHoverOverAction.AddListener(e);
	}

	public void SetOnHoverOutAction(UnityAction e)
	{
		onHoverExitAction.RemoveAllListeners();
		onHoverExitAction.AddListener(e);
	}

	public void AddOnHoverOverAction(UnityAction e)
	{
		onHoverOverAction.AddListener(e);
	}

	public void AddOnHoverOutAction(UnityAction e)
	{
		onHoverExitAction.AddListener(e);
	}

	public void RemoveOnHoverOverAction(UnityAction e)
	{
		onHoverOverAction.RemoveListener(e);
	}

	public void RemoveOnHoverOutAction(UnityAction e)
	{
		onHoverExitAction.RemoveListener(e);
	}

	public void ClearHoverActions()
	{
		onHoverOverAction.RemoveAllListeners();
		onHoverExitAction.RemoveAllListeners();
	}

	public void ExecuteHoverEnterActionPerFrame(bool p_state)
	{
		executeHoverEnterActionPerFrame = p_state;
	}

	private void Update()
	{
		if (executeHoverEnterActionPerFrame && isHovering)
		{
			onHoverOverAction?.Invoke();
		}
	}

	public void HideSmallInfoString()
	{
		if (UIManager.Instance != null)
		{
			UIManager.Instance.HideSmallInfo();
		}
	}

	public void ShowSmallInfoInSpecificPosition(string message)
	{
		if (UIManager.Instance != null)
		{
			if (LocalizationManager.Instance.HasLocalizedValue("UIStrings_Table", message))
			{
				message = LocalizationManager.Instance.GetLocalizedValue("UIStrings_Table", message);
			}
			if (tooltipPos != null)
			{
				UIManager.Instance.ShowSmallInfo(message, tooltipPos, tooltipHeader);
			}
			else
			{
				UIManager.Instance.ShowSmallInfo(message, tooltipHeader);
			}
		}
	}

	public void SetToolTipPosition(UIHoverPosition p_pos)
	{
		tooltipPos = p_pos;
	}
}
