using System;
using System.Collections;
using UnityEngine;

namespace LapinerTools.uMyGUI;

public class uMyGUI_Popup : MonoBehaviour
{
	protected int m_createFrame;

	public virtual bool IsShown => base.gameObject.activeSelf;

	public virtual bool DestroyOnHide { get; set; }

	public event Action OnShow;

	public event Action OnHide;

	public virtual void Show()
	{
		base.gameObject.transform.SetAsLastSibling();
		base.gameObject.SetActive(value: true);
		if (this.OnShow != null)
		{
			this.OnShow();
		}
	}

	public virtual void Hide()
	{
		base.gameObject.SetActive(value: false);
		if (this.OnHide != null)
		{
			this.OnHide();
		}
		if (DestroyOnHide && m_createFrame != Time.frameCount && uMyGUI_PopupManager.IsInstanceSet)
		{
			uMyGUI_PopupManager.Instance.StartCoroutine(DestroyOnEndOfFrame());
		}
	}

	protected virtual void Awake()
	{
		m_createFrame = Time.frameCount;
	}

	protected virtual void Start()
	{
	}

	protected IEnumerator DestroyOnEndOfFrame()
	{
		yield return new WaitForEndOfFrame();
		if (uMyGUI_PopupManager.IsInstanceSet)
		{
			uMyGUI_PopupManager.Instance.RemovePopup(this);
			UnityEngine.Object.Destroy(base.gameObject);
		}
	}
}
