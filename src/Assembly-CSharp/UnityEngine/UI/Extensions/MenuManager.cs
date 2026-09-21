using System.Collections.Generic;
using System.Reflection;

namespace UnityEngine.UI.Extensions;

[AddComponentMenu("UI/Extensions/Menu Manager")]
[DisallowMultipleComponent]
public class MenuManager : MonoBehaviour
{
	public Menu[] MenuScreens;

	public int StartScreen;

	private Stack<Menu> menuStack = new Stack<Menu>();

	public static MenuManager Instance { get; set; }

	private void Awake()
	{
		Instance = this;
		if (MenuScreens.Length > StartScreen)
		{
			CreateInstance(MenuScreens[StartScreen].name);
			OpenMenu(MenuScreens[StartScreen]);
		}
		else
		{
			Debug.LogError("Not enough Menu Screens configured");
		}
	}

	private void OnDestroy()
	{
		Instance = null;
	}

	public void CreateInstance<T>() where T : Menu
	{
		Object.Instantiate(GetPrefab<T>(), base.transform);
	}

	public void CreateInstance(string MenuName)
	{
		Object.Instantiate(GetPrefab(MenuName), base.transform);
	}

	public void OpenMenu(Menu instance)
	{
		if (menuStack.Count > 0)
		{
			if (instance.DisableMenusUnderneath)
			{
				foreach (Menu item in menuStack)
				{
					item.gameObject.SetActive(value: false);
					if (item.DisableMenusUnderneath)
					{
						break;
					}
				}
			}
			Canvas component = instance.GetComponent<Canvas>();
			Canvas component2 = menuStack.Peek().GetComponent<Canvas>();
			component.sortingOrder = component2.sortingOrder + 1;
		}
		menuStack.Push(instance);
	}

	private GameObject GetPrefab(string PrefabName)
	{
		for (int i = 0; i < MenuScreens.Length; i++)
		{
			if (MenuScreens[i].name == PrefabName)
			{
				return MenuScreens[i].gameObject;
			}
		}
		throw new MissingReferenceException("Prefab not found for " + PrefabName);
	}

	private T GetPrefab<T>() where T : Menu
	{
		FieldInfo[] fields = GetType().GetFields(BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public);
		for (int i = 0; i < fields.Length; i++)
		{
			T val = fields[i].GetValue(this) as T;
			if (val != null)
			{
				return val;
			}
		}
		throw new MissingReferenceException($"Prefab not found for type {typeof(T)}");
	}

	public void CloseMenu(Menu menu)
	{
		if (menuStack.Count == 0)
		{
			Debug.LogErrorFormat(menu, "{0} cannot be closed because menu stack is empty", menu.GetType());
		}
		else if (menuStack.Peek() != menu)
		{
			Debug.LogErrorFormat(menu, "{0} cannot be closed because it is not on top of stack", menu.GetType());
		}
		else
		{
			CloseTopMenu();
		}
	}

	public void CloseTopMenu()
	{
		Menu menu = menuStack.Pop();
		if (menu.DestroyWhenClosed)
		{
			Object.Destroy(menu.gameObject);
		}
		else
		{
			menu.gameObject.SetActive(value: false);
		}
		foreach (Menu item in menuStack)
		{
			item.gameObject.SetActive(value: true);
			if (item.DisableMenusUnderneath)
			{
				break;
			}
		}
	}

	private void Update()
	{
		if (Input.GetKeyDown(KeyCode.Escape) && menuStack.Count > 0)
		{
			menuStack.Peek().OnBackPressed();
		}
	}
}
