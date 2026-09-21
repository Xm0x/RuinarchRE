using System;
using JetBrains.Annotations;
using Settings;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class GeneralConfirmationWithVisual : GeneralConfirmation
{
	[SerializeField]
	private RawImage picture;

	[SerializeField]
	private VideoPlayer _videoPlayer;

	[SerializeField]
	private RenderTexture _renderTexture;

	public void ShowGeneralConfirmation(string header, string body, [NotNull] Texture sprite, string buttonText = "OK", Action onClickOK = null)
	{
		if (PlayerUI.Instance.IsMajorUIShowing())
		{
			PlayerUI.Instance.AddPendingUI(delegate
			{
				ShowGeneralConfirmation(header, body, sprite, buttonText, onClickOK);
			});
		}
		else
		{
			base.ShowGeneralConfirmation(header, body, buttonText, onClickOK);
			SetVisual(sprite);
		}
	}

	public void ShowGeneralConfirmation(string header, string body, [NotNull] VideoClip videoClip, string buttonText = "OK", Action onClickOK = null)
	{
		if (PlayerUI.Instance.IsMajorUIShowing())
		{
			PlayerUI.Instance.AddPendingUI(delegate
			{
				ShowGeneralConfirmation(header, body, videoClip, buttonText, onClickOK);
			});
			return;
		}
		base.ShowGeneralConfirmation(header, body, buttonText, onClickOK);
		if (!SettingsManager.Instance.doNotShowVideos)
		{
			SetVisual(videoClip);
		}
		else
		{
			picture.gameObject.SetActive(value: false);
		}
	}

	public override void Close()
	{
		if (_videoPlayer.clip != null)
		{
			_videoPlayer.Stop();
		}
		base.Close();
	}

	private void SetVisual(Texture texture)
	{
		if (_videoPlayer.clip != null)
		{
			_videoPlayer.Stop();
		}
		picture.gameObject.SetActive(value: true);
		picture.texture = texture;
	}

	private void SetVisual(VideoClip videoClip)
	{
		_videoPlayer.clip = videoClip;
		_videoPlayer.Play();
		_videoPlayer.targetTexture = _renderTexture;
		picture.texture = _renderTexture;
	}
}
