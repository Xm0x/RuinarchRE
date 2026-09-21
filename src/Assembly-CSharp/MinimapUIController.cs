using System;
using Ruinarch;
using Ruinarch.MVCFramework;
using UnityEngine;
using UnityEngine.EventSystems;

public class MinimapUIController : MVCUIController, MinimapUIView.IListener
{
	[SerializeField]
	private MinimapUIModel m_minimapUIModel;

	private MinimapUIView m_minimapUIView;

	private int _minimapLayerMask;

	[ContextMenu("Instantiate UI")]
	public override void InstantiateUI()
	{
		MinimapUIView.Create(_canvas, m_minimapUIModel, delegate(MinimapUIView p_ui)
		{
			m_minimapUIView = p_ui;
			m_minimapUIView.Subscribe(this);
			InitUI(p_ui.UIModel, p_ui);
			p_ui.UIModel.transform.SetSiblingIndex(siblingIndex);
			InitializeSizeAndPositionBasedOnMapSize();
		});
	}

	private void Awake()
	{
		_minimapLayerMask = LayerMask.GetMask("Minimap");
	}

	private void OnDestroy()
	{
		m_minimapUIView?.Unsubscribe(this);
	}

	private void InitializeSizeAndPositionBasedOnMapSize()
	{
		Vector2 anchoredPosition;
		Vector2 sizeDelta;
		switch (WorldSettings.Instance.worldSettingsData.mapSettings.mapSize)
		{
		case MAP_SIZE.Small:
			anchoredPosition = new Vector2(12.5f, 11f);
			sizeDelta = new Vector2(400f, 396f);
			break;
		case MAP_SIZE.Medium:
			anchoredPosition = new Vector2(12.5f, -54.7f);
			sizeDelta = new Vector2(400f, 265f);
			break;
		case MAP_SIZE.Large:
			anchoredPosition = new Vector2(12.5f, -63f);
			sizeDelta = new Vector2(400f, 248f);
			break;
		case MAP_SIZE.Extra_Large:
			anchoredPosition = new Vector2(12.5f, -63f);
			sizeDelta = new Vector2(400f, 245f);
			break;
		default:
			throw new ArgumentOutOfRangeException();
		}
		m_minimapUIView.UIModel.rectContainer.sizeDelta = sizeDelta;
		m_minimapUIView.UIModel.rectMinimapImage.anchoredPosition = anchoredPosition;
	}

	public void OnClickMinimapImage(PointerEventData p_data)
	{
		if (RectTransformUtility.ScreenPointToLocalPointInRectangle(m_minimapUIView.UIModel.minimapImage.rawImage.rectTransform, InputManager.Instance.mousePosition, p_data.pressEventCamera, out var localPoint))
		{
			Vector3 pos = GridMap.Instance.mainRegion.innerMap.minimapCamera.ScreenToViewportPoint(localPoint);
			if (Physics.Raycast(GridMap.Instance.mainRegion.innerMap.minimapCamera.ViewportPointToRay(pos), out var hitInfo, float.PositiveInfinity, _minimapLayerMask))
			{
				InnerMapCameraMove.Instance.ClearOutCameraTargets();
				Vector3 point = hitInfo.point;
				point.z = -10f;
				InnerMapCameraMove.Instance.MoveCameraForMinimap(point);
			}
		}
	}
}
