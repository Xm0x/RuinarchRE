using System.Collections.Generic;
using UnityEngine.SceneManagement;

public static class SignalHandler
{
	public delegate void SignalListener();

	private static Dictionary<string, List<SignalListener>> _handles = new Dictionary<string, List<SignalListener>>(50);

	private static bool _hasInitialized = false;

	private static void Initialize()
	{
		if (!_hasInitialized)
		{
			_hasInitialized = true;
			SceneManager.sceneUnloaded += CleanUp;
		}
	}

	public static void AddListener(string p_key, SignalListener p_value, bool shouldLock)
	{
		Initialize();
		if (shouldLock)
		{
			lock (_handles)
			{
				if (!_handles.ContainsKey(p_key))
				{
					_handles.Add(p_key, new List<SignalListener>(100));
				}
				_handles[p_key].Add(p_value);
				return;
			}
		}
		if (!_handles.ContainsKey(p_key))
		{
			_handles.Add(p_key, new List<SignalListener>(100));
		}
		_handles[p_key].Add(p_value);
	}

	public static void RemoveListener(string p_key, SignalListener p_value, bool shouldLock)
	{
		Initialize();
		if (shouldLock)
		{
			lock (_handles)
			{
				if (_handles.ContainsKey(p_key))
				{
					_handles[p_key].Remove(p_value);
				}
				return;
			}
		}
		if (_handles.ContainsKey(p_key))
		{
			_handles[p_key].Remove(p_value);
		}
	}

	public static void Broadcast(string p_key)
	{
		Initialize();
		if (!_handles.ContainsKey(p_key))
		{
			return;
		}
		List<SignalListener> list = _handles[p_key];
		int count = list.Count;
		for (int i = 0; i < count; i++)
		{
			list[i]();
			if (list.Count < count)
			{
				count = list.Count;
				i--;
			}
			else if (list.Count > count)
			{
				count = list.Count;
			}
		}
	}

	public static void CleanUp(Scene current)
	{
		if (current.buildIndex != 1)
		{
			return;
		}
		foreach (KeyValuePair<string, List<SignalListener>> handle in _handles)
		{
			if (!Messenger.permanentMessages.Contains(handle.Key))
			{
				handle.Value.Clear();
			}
		}
	}
}
public static class SignalHandler<T>
{
	public delegate void SignalListener(T arg1);

	private static Dictionary<string, List<SignalListener>> _handles = new Dictionary<string, List<SignalListener>>(50);

	private static bool _hasInitialized = false;

	private static void Initialize()
	{
		if (!_hasInitialized)
		{
			SceneManager.sceneUnloaded += CleanUp;
			_hasInitialized = true;
		}
	}

	public static void AddListener(string p_key, SignalListener p_value, bool shouldLock)
	{
		Initialize();
		if (shouldLock)
		{
			lock (_handles)
			{
				if (!_handles.ContainsKey(p_key))
				{
					_handles.Add(p_key, new List<SignalListener>(100));
				}
				_handles[p_key].Add(p_value);
				return;
			}
		}
		if (!_handles.ContainsKey(p_key))
		{
			_handles.Add(p_key, new List<SignalListener>(100));
		}
		_handles[p_key].Add(p_value);
	}

	public static void RemoveListener(string p_key, SignalListener p_value, bool shouldLock)
	{
		Initialize();
		if (shouldLock)
		{
			lock (_handles)
			{
				if (_handles.ContainsKey(p_key))
				{
					_handles[p_key].Remove(p_value);
				}
				return;
			}
		}
		if (_handles.ContainsKey(p_key))
		{
			_handles[p_key].Remove(p_value);
		}
	}

	public static void Broadcast(string p_key, T arg1)
	{
		Initialize();
		if (!_handles.ContainsKey(p_key))
		{
			return;
		}
		List<SignalListener> list = _handles[p_key];
		int count = list.Count;
		for (int i = 0; i < count; i++)
		{
			list[i](arg1);
			if (list.Count < count)
			{
				count = list.Count;
				i--;
			}
			else if (list.Count > count)
			{
				count = list.Count;
			}
		}
	}

	private static void CleanUp(Scene current)
	{
		if (current.buildIndex != 1)
		{
			return;
		}
		foreach (KeyValuePair<string, List<SignalListener>> handle in _handles)
		{
			if (!Messenger.permanentMessages.Contains(handle.Key))
			{
				handle.Value.Clear();
			}
		}
	}
}
public static class SignalHandler<T, U>
{
	public delegate void SignalListener(T arg1, U arg2);

	private static Dictionary<string, List<SignalListener>> _handles = new Dictionary<string, List<SignalListener>>(50);

	private static bool _hasInitialized = false;

	private static void Initialize()
	{
		if (!_hasInitialized)
		{
			SceneManager.sceneUnloaded += CleanUp;
			_hasInitialized = true;
		}
	}

	public static void AddListener(string p_key, SignalListener p_value, bool shouldLock)
	{
		Initialize();
		if (shouldLock)
		{
			lock (_handles)
			{
				if (!_handles.ContainsKey(p_key))
				{
					_handles.Add(p_key, new List<SignalListener>(100));
				}
				_handles[p_key].Add(p_value);
				return;
			}
		}
		if (!_handles.ContainsKey(p_key))
		{
			_handles.Add(p_key, new List<SignalListener>(100));
		}
		_handles[p_key].Add(p_value);
	}

	public static void RemoveListener(string p_key, SignalListener p_value, bool shouldLock)
	{
		Initialize();
		if (shouldLock)
		{
			lock (_handles)
			{
				if (_handles.ContainsKey(p_key))
				{
					_handles[p_key].Remove(p_value);
				}
				return;
			}
		}
		if (_handles.ContainsKey(p_key))
		{
			_handles[p_key].Remove(p_value);
		}
	}

	public static void Broadcast(string p_key, T arg1, U arg2)
	{
		Initialize();
		if (!_handles.ContainsKey(p_key))
		{
			return;
		}
		List<SignalListener> list = _handles[p_key];
		int count = list.Count;
		for (int i = 0; i < count; i++)
		{
			list[i](arg1, arg2);
			if (list.Count < count)
			{
				count = list.Count;
				i--;
			}
			else if (list.Count > count)
			{
				count = list.Count;
			}
		}
	}

	private static void CleanUp(Scene current)
	{
		if (current.buildIndex != 1)
		{
			return;
		}
		foreach (KeyValuePair<string, List<SignalListener>> handle in _handles)
		{
			if (!Messenger.permanentMessages.Contains(handle.Key))
			{
				handle.Value.Clear();
			}
		}
	}
}
public static class SignalHandler<T, U, V>
{
	public delegate void SignalListener(T arg1, U arg2, V arg3);

	private static Dictionary<string, List<SignalListener>> _handles = new Dictionary<string, List<SignalListener>>(50);

	private static bool _hasInitialized = false;

	private static void Initialize()
	{
		if (!_hasInitialized)
		{
			SceneManager.sceneUnloaded += CleanUp;
			_hasInitialized = true;
		}
	}

	public static void AddListener(string p_key, SignalListener p_value, bool shouldLock)
	{
		Initialize();
		if (shouldLock)
		{
			lock (_handles)
			{
				if (!_handles.ContainsKey(p_key))
				{
					_handles.Add(p_key, new List<SignalListener>(100));
				}
				_handles[p_key].Add(p_value);
				return;
			}
		}
		if (!_handles.ContainsKey(p_key))
		{
			_handles.Add(p_key, new List<SignalListener>(100));
		}
		_handles[p_key].Add(p_value);
	}

	public static void RemoveListener(string p_key, SignalListener p_value, bool shouldLock)
	{
		Initialize();
		if (shouldLock)
		{
			lock (_handles)
			{
				if (_handles.ContainsKey(p_key))
				{
					_handles[p_key].Remove(p_value);
				}
				return;
			}
		}
		if (_handles.ContainsKey(p_key))
		{
			_handles[p_key].Remove(p_value);
		}
	}

	public static void Broadcast(string p_key, T arg1, U arg2, V arg3)
	{
		Initialize();
		if (!_handles.ContainsKey(p_key))
		{
			return;
		}
		List<SignalListener> list = _handles[p_key];
		int count = list.Count;
		for (int i = 0; i < count; i++)
		{
			list[i](arg1, arg2, arg3);
			if (list.Count < count)
			{
				count = list.Count;
				i--;
			}
			else if (list.Count > count)
			{
				count = list.Count;
			}
		}
	}

	private static void CleanUp(Scene current)
	{
		if (current.buildIndex != 1)
		{
			return;
		}
		foreach (KeyValuePair<string, List<SignalListener>> handle in _handles)
		{
			if (!Messenger.permanentMessages.Contains(handle.Key))
			{
				handle.Value.Clear();
			}
		}
	}
}
public static class SignalHandler<T, U, V, W>
{
	public delegate void SignalListener(T arg1, U arg2, V arg3, W arg4);

	private static Dictionary<string, List<SignalListener>> _handles = new Dictionary<string, List<SignalListener>>(50);

	private static bool _hasInitialized = false;

	private static void Initialize()
	{
		if (!_hasInitialized)
		{
			SceneManager.sceneUnloaded += CleanUp;
			_hasInitialized = true;
		}
	}

	public static void AddListener(string p_key, SignalListener p_value, bool shouldLock)
	{
		Initialize();
		if (shouldLock)
		{
			lock (_handles)
			{
				if (!_handles.ContainsKey(p_key))
				{
					_handles.Add(p_key, new List<SignalListener>(100));
				}
				_handles[p_key].Add(p_value);
				return;
			}
		}
		if (!_handles.ContainsKey(p_key))
		{
			_handles.Add(p_key, new List<SignalListener>(100));
		}
		_handles[p_key].Add(p_value);
	}

	public static void RemoveListener(string p_key, SignalListener p_value, bool shouldLock)
	{
		Initialize();
		if (shouldLock)
		{
			lock (_handles)
			{
				if (_handles.ContainsKey(p_key))
				{
					_handles[p_key].Remove(p_value);
				}
				return;
			}
		}
		if (_handles.ContainsKey(p_key))
		{
			_handles[p_key].Remove(p_value);
		}
	}

	public static void Broadcast(string p_key, T arg1, U arg2, V arg3, W arg4)
	{
		Initialize();
		if (!_handles.ContainsKey(p_key))
		{
			return;
		}
		List<SignalListener> list = _handles[p_key];
		int count = list.Count;
		for (int i = 0; i < count; i++)
		{
			list[i](arg1, arg2, arg3, arg4);
			if (list.Count < count)
			{
				count = list.Count;
				i--;
			}
			else if (list.Count > count)
			{
				count = list.Count;
			}
		}
	}

	private static void CleanUp(Scene current)
	{
		if (current.buildIndex != 1)
		{
			return;
		}
		foreach (KeyValuePair<string, List<SignalListener>> handle in _handles)
		{
			if (!Messenger.permanentMessages.Contains(handle.Key))
			{
				handle.Value.Clear();
			}
		}
	}
}
