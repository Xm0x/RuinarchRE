using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(ScrollRect))]
public class VerticalScroller : MonoBehaviour
{
	private ScrollRect scrollView;

	[SerializeField]
	private Button scrollUpBtn;

	[SerializeField]
	private Button scrollDownBtn;

	[SerializeField]
	private float elementHeight;

	private void Awake()
	{
		scrollView = GetComponent<ScrollRect>();
	}

	public void ScrollUp()
	{
	}

	public void ScrollDown()
	{
	}

	public void OnScroll(Vector2 scrollPos)
	{
		if (!(scrollUpBtn != null) || !(scrollDownBtn != null))
		{
			return;
		}
		if (scrollView.content.rect.height <= scrollView.viewport.rect.height)
		{
			scrollDownBtn.gameObject.SetActive(value: false);
			scrollUpBtn.gameObject.SetActive(value: false);
			return;
		}
		if (Mathf.Approximately(scrollView.verticalNormalizedPosition, 0f))
		{
			scrollDownBtn.gameObject.SetActive(value: false);
		}
		else
		{
			scrollDownBtn.gameObject.SetActive(value: true);
		}
		if (Mathf.Approximately(scrollView.verticalNormalizedPosition, 1f))
		{
			scrollUpBtn.gameObject.SetActive(value: false);
		}
		else
		{
			scrollUpBtn.gameObject.SetActive(value: true);
		}
	}
}
