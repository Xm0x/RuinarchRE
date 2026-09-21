using System;
using System.Collections.Generic;
using Ruinarch.MVCFramework;
using UnityEngine;
using UnityEngine.UI;
using UtilityScripts;

public class ContextMenuUIView : MVCUIView
{
	public interface IListener
	{
		void OnHoverOverParentDisplay();

		void OnHoverOutParentDisplay();
	}

	private List<ContextMenuUIObject> clickableMenuUIObjects = new List<ContextMenuUIObject>();

	public ContextMenuUIModel UIModel => _baseAssetModel as ContextMenuUIModel;

	public static void Create(Canvas p_canvas, ContextMenuUIModel p_assets, Action<ContextMenuUIView> p_onCreate)
	{
		ContextMenuUIView contextMenuUIView = new GameObject(typeof(ContextMenuUIView).ToString()).AddComponent<ContextMenuUIView>();
		ContextMenuUIModel assets = UnityEngine.Object.Instantiate(p_assets);
		contextMenuUIView.Init(p_canvas, assets);
		p_onCreate?.Invoke(contextMenuUIView);
	}

	public void InitializeUI(List<IContextMenuItem> p_mainItems, Canvas p_canvas)
	{
		DisplayMenu(p_mainItems, 0, p_canvas);
	}

	private void DisplayMenu(List<IContextMenuItem> p_UIMenu, int p_targetColumn, Canvas p_canvas, bool dontShowName = false)
	{
		clickableMenuUIObjects.Clear();
		ScrollRect scrollRect = UIModel.menuParent[p_targetColumn].scrollRect;
		scrollRect.gameObject.SetActive(value: true);
		clickableMenuUIObjects.AddRange(scrollRect.content.GetComponentsInChildren<ContextMenuUIObject>());
		int count = clickableMenuUIObjects.Count;
		for (int i = 0; i < count; i++)
		{
			ObjectPoolManager.Instance.DestroyObject(clickableMenuUIObjects[i]);
		}
		if (p_UIMenu != null)
		{
			for (int j = 0; j < p_UIMenu.Count; j++)
			{
				ContextMenuUIObject component = ObjectPoolManager.Instance.InstantiateObjectFromPool("ContextMenuItem", Vector3.zero, Quaternion.identity, scrollRect.content.transform).GetComponent<ContextMenuUIObject>();
				component.SetMenuDetails(p_UIMenu[j], dontShowName);
				component.btnActivate.ForceUpdateGlow();
			}
		}
	}

	public void HideColumn(int p_targetColumn)
	{
		if (p_targetColumn >= 0 && p_targetColumn < UIModel.menuParent.Length)
		{
			UIModel.menuParent[p_targetColumn].scrollRect.gameObject.SetActive(value: false);
		}
	}

	public void DisplaySubMenu(List<IContextMenuItem> p_UIMenu, int p_targetColumn, Canvas p_canvas, bool dontShowName = false)
	{
		DisplayMenu(p_UIMenu, p_targetColumn, p_canvas, dontShowName);
		ScrollRect scrollRect = UIModel.menuParent[p_targetColumn].scrollRect;
		scrollRect.gameObject.SetActive(value: true);
		if (p_targetColumn == 1)
		{
			RectTransform rectTransform = scrollRect.transform as RectTransform;
			rectTransform.anchoredPosition = UIModel.column2RightPos;
			if (!GameUtilities.IsRectFullyInCanvas(rectTransform, p_canvas.transform as RectTransform))
			{
				rectTransform.anchoredPosition = UIModel.column2LeftPos;
			}
		}
	}

	public void SetTitleName(string p_Name)
	{
		UIModel.lblTitle.text = p_Name;
	}

	public void SetPosition(Vector3 p_pos, Canvas p_canvas)
	{
		UIModel.parentDisplay.position = p_pos;
	}

	private ContextMenuColumn GetLeftMostColumn()
	{
		if (UIModel.menuParent[1].gameObject.activeSelf && UIModel.menuParent[1].rectTransform.anchoredPosition == UIModel.column2LeftPos)
		{
			return UIModel.menuParent[1];
		}
		return UIModel.menuParent[0];
	}

	private ContextMenuColumn GetRightMostColumn()
	{
		if (UIModel.menuParent[1].gameObject.activeSelf && UIModel.menuParent[1].rectTransform.anchoredPosition == UIModel.column2RightPos)
		{
			return UIModel.menuParent[1];
		}
		return UIModel.menuParent[0];
	}

	public UIHoverPosition GetTooltipHoverPositionToUse()
	{
		ContextMenuColumn leftMostColumn = GetLeftMostColumn();
		RectTransform boundsRT = leftMostColumn.leftHoverPosition.transform as RectTransform;
		Rect canvasRT = new Rect(0f, 0f, Screen.width, Screen.height);
		if (GameUtilities.IsRectFullyInCanvas(boundsRT, canvasRT))
		{
			return leftMostColumn.leftHoverPosition;
		}
		return GetRightMostColumn().rightHoverPosition;
	}

	public void Subscribe(IListener p_listener)
	{
		ContextMenuUIModel uIModel = UIModel;
		uIModel.parentDisplayHoverOver = (Action)Delegate.Combine(uIModel.parentDisplayHoverOver, new Action(p_listener.OnHoverOverParentDisplay));
		ContextMenuUIModel uIModel2 = UIModel;
		uIModel2.parentDisplayHoverOut = (Action)Delegate.Combine(uIModel2.parentDisplayHoverOut, new Action(p_listener.OnHoverOutParentDisplay));
	}

	public void Unsubscribe(IListener p_listener)
	{
		ContextMenuUIModel uIModel = UIModel;
		uIModel.parentDisplayHoverOver = (Action)Delegate.Remove(uIModel.parentDisplayHoverOver, new Action(p_listener.OnHoverOverParentDisplay));
		ContextMenuUIModel uIModel2 = UIModel;
		uIModel2.parentDisplayHoverOut = (Action)Delegate.Remove(uIModel2.parentDisplayHoverOut, new Action(p_listener.OnHoverOutParentDisplay));
	}
}
