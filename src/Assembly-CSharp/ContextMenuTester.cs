using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ContextMenuTester : MonoBehaviour
{
	[SerializeField]
	private List<ClickableMenuData> m_initialItems;

	[SerializeField]
	private ContextMenuUIController m_contextMenuController;

	private void Start()
	{
		ObjectPoolManager.Instance.InitializeObjectPools();
		m_contextMenuController.ShowContextMenu(((IEnumerable<ClickableMenuData>)m_initialItems).Select((Func<ClickableMenuData, IContextMenuItem>)((ClickableMenuData x) => x)).ToList(), new Vector3(0f, Screen.height), "Test", Cursor_Type.Default);
	}

	private void Update()
	{
		if (Input.GetMouseButtonDown(1))
		{
			m_contextMenuController.ShowContextMenu(((IEnumerable<ClickableMenuData>)m_initialItems).Select((Func<ClickableMenuData, IContextMenuItem>)((ClickableMenuData x) => x)).ToList(), Input.mousePosition, "Test", Cursor_Type.Default);
		}
	}
}
