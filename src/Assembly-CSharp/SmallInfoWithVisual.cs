using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class SmallInfoWithVisual : MonoBehaviour
{
	[Header("Small Info with Visual")]
	[SerializeField]
	private GameObject smallInfoVisualGO;

	[SerializeField]
	private RectTransform smallInfoVisualRT;

	[SerializeField]
	private VideoPlayer smallInfoVideoPlayer;

	[SerializeField]
	private RenderTexture smallInfoVisualRenderTexture;

	[SerializeField]
	private RawImage smallInfoVisualImage;

	[SerializeField]
	private RuinarchText smallInfoVisualLbl;

	public void ShowSmallInfo(string info, [NotNull] VideoClip videoClip, string header = "", UIHoverPosition pos = null)
	{
		string text = string.Empty;
		if (!string.IsNullOrEmpty(header))
		{
			text = "<font=\"Eczar-Medium\"><line-height=100%><size=18>" + header + "</font>\n";
		}
		text = text + "<line-height=70%><size=16>" + info;
		text = text.Replace("\\n", "\n");
		smallInfoVisualLbl.SetTextAndReplaceWithIcons(text);
		if (!UIManager.Instance.IsSmallInfoShowing())
		{
			smallInfoVisualGO.transform.SetParent(base.transform);
			smallInfoVisualGO.SetActive(value: true);
		}
		if (pos == null)
		{
			UIManager.Instance.PositionTooltip(smallInfoVisualGO, smallInfoVisualRT, smallInfoVisualRT);
		}
		else
		{
			UIManager.Instance.PositionTooltip(pos, smallInfoVisualGO, smallInfoVisualRT);
		}
		if (smallInfoVisualImage.texture != smallInfoVisualRenderTexture)
		{
			smallInfoVisualImage.texture = smallInfoVisualRenderTexture;
		}
		if (smallInfoVideoPlayer.clip != videoClip)
		{
			smallInfoVideoPlayer.clip = videoClip;
			smallInfoVideoPlayer.Stop();
			smallInfoVideoPlayer.Play();
		}
	}

	public void ShowSmallInfo(string info, Texture visual, string header = "", UIHoverPosition pos = null)
	{
		string text = string.Empty;
		if (!string.IsNullOrEmpty(header))
		{
			text = "<font=\"Eczar-Medium\"><line-height=100%><size=18>" + header + "</font>\n";
		}
		text = text + "<line-height=70%><size=16>" + info;
		text = text.Replace("\\n", "\n");
		smallInfoVisualLbl.SetText(text);
		if (!UIManager.Instance.IsSmallInfoShowing())
		{
			smallInfoVisualGO.transform.SetParent(base.transform);
			smallInfoVisualGO.SetActive(value: true);
		}
		if (pos == null)
		{
			UIManager.Instance.PositionTooltip(smallInfoVisualGO, smallInfoVisualRT, smallInfoVisualRT);
		}
		else
		{
			UIManager.Instance.PositionTooltip(pos, smallInfoVisualGO, smallInfoVisualRT);
		}
		smallInfoVisualImage.texture = visual;
	}

	public void Hide()
	{
		base.gameObject.SetActive(value: false);
	}

	private void OnDisable()
	{
		smallInfoVideoPlayer.clip = null;
		smallInfoVideoPlayer.Stop();
	}
}
