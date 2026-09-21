using UnityEngine;

public class DisallowOverlapWithCharacterPointer : MonoBehaviour
{
	private RectTransform _rectTransform;

	public RectTransform rectTransform => _rectTransform;

	public Vector2 anchoredOffsetMin
	{
		get
		{
			Vector2 anchorMin = rectTransform.anchorMin;
			anchorMin.x *= UIManager.Instance.canvasScaler.referenceResolution.x;
			anchorMin.y *= UIManager.Instance.canvasScaler.referenceResolution.y;
			float num = (float)Screen.width / UIManager.Instance.canvasScaler.referenceResolution.x;
			float num2 = (float)Screen.height / UIManager.Instance.canvasScaler.referenceResolution.y;
			anchorMin += rectTransform.offsetMin;
			return new Vector2(anchorMin.x * num, anchorMin.y * num2);
		}
	}

	public Vector2 anchoredOffsetMax
	{
		get
		{
			Vector2 anchorMax = rectTransform.anchorMax;
			anchorMax.x *= UIManager.Instance.canvasScaler.referenceResolution.x;
			anchorMax.y *= UIManager.Instance.canvasScaler.referenceResolution.y;
			float num = (float)Screen.width / UIManager.Instance.canvasScaler.referenceResolution.x;
			float num2 = (float)Screen.height / UIManager.Instance.canvasScaler.referenceResolution.y;
			anchorMax += rectTransform.offsetMax;
			return new Vector2(anchorMax.x * num, anchorMax.y * num2);
		}
	}

	private void OnEnable()
	{
		if (_rectTransform == null)
		{
			_rectTransform = base.gameObject.GetComponent<RectTransform>();
		}
		if (UIManager.Instance != null && !UIManager.Instance.disallowOverlapsWithCP.Contains(this))
		{
			UIManager.Instance.AddDisallowOverlapWithCPUI(this);
		}
	}
}
