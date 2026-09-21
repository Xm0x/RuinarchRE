using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Steamworks;

namespace LapinerTools.Steam.Data.Internal;

public class SteamRequestList
{
	private Dictionary<Type, List<object>> m_requests = new Dictionary<Type, List<object>>();

	public void Add<T>(CallResult<T> p_request)
	{
		Type typeFromHandle = typeof(T);
		if (!m_requests.TryGetValue(typeFromHandle, out var value))
		{
			value = new List<object>();
			m_requests.Add(typeFromHandle, value);
		}
		value.Add(p_request);
	}

	public int Count()
	{
		return m_requests.Values.Sum((List<object> requestList) => requestList.Count);
	}

	public int Count<T>()
	{
		Type typeFromHandle = typeof(T);
		if (m_requests.TryGetValue(typeFromHandle, out var value))
		{
			return value.Count;
		}
		return 0;
	}

	public void Clear<T>()
	{
		Type typeFromHandle = typeof(T);
		if (m_requests.TryGetValue(typeFromHandle, out var value))
		{
			value.Clear();
		}
	}

	public void RemoveInactive()
	{
		foreach (KeyValuePair<Type, List<object>> request in m_requests)
		{
			GetType().GetMethod("RemoveInactiveInternal", BindingFlags.Static | BindingFlags.NonPublic).MakeGenericMethod(request.Key).Invoke(this, new object[1] { request.Value });
		}
	}

	public void RemoveInactive<T>()
	{
		Type typeFromHandle = typeof(T);
		if (m_requests.TryGetValue(typeFromHandle, out var value))
		{
			GetType().GetMethod("RemoveInactiveInternal", BindingFlags.Static | BindingFlags.NonPublic).MakeGenericMethod(typeFromHandle).Invoke(this, new object[1] { value });
		}
	}

	public void Cancel()
	{
		foreach (KeyValuePair<Type, List<object>> request in m_requests)
		{
			GetType().GetMethod("CancelInternal", BindingFlags.Static | BindingFlags.NonPublic).MakeGenericMethod(request.Key).Invoke(this, new object[1] { request.Value });
		}
	}

	public void Cancel<T>()
	{
		Type typeFromHandle = typeof(T);
		if (m_requests.TryGetValue(typeFromHandle, out var value))
		{
			GetType().GetMethod("CancelInternal", BindingFlags.Static | BindingFlags.NonPublic).MakeGenericMethod(typeFromHandle).Invoke(this, new object[1] { value });
		}
	}

	private static void CancelInternal<T>(List<object> p_requests)
	{
		for (int num = p_requests.Count - 1; num >= 0; num--)
		{
			(p_requests[num] as CallResult<T>).Cancel();
		}
	}

	private static void RemoveInactiveInternal<T>(List<object> p_requests)
	{
		for (int num = p_requests.Count - 1; num >= 0; num--)
		{
			if (!(p_requests[num] as CallResult<T>).IsActive())
			{
				p_requests.RemoveAt(num);
			}
		}
	}
}
