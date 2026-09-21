using System;
using System.Collections.Generic;
using UnityEngine;

internal static class Messenger
{
	public class BroadcastException : Exception
	{
		public BroadcastException(string msg)
			: base(msg)
		{
		}
	}

	public class ListenerException : Exception
	{
		public ListenerException(string msg)
			: base(msg)
		{
		}
	}

	public static Dictionary<string, Delegate> eventTable = new Dictionary<string, Delegate>(100);

	public static HashSet<string> permanentMessages = new HashSet<string>();

	public static void MarkAsPermanent(string eventType)
	{
		permanentMessages.Add(eventType);
	}

	public static void Cleanup()
	{
		List<string> list = new List<string>();
		foreach (KeyValuePair<string, Delegate> item in eventTable)
		{
			if (!permanentMessages.Contains(item.Key))
			{
				list.Add(item.Key);
			}
		}
		for (int i = 0; i < list.Count; i++)
		{
			eventTable.Remove(list[i]);
		}
	}

	public static void PrintEventTable()
	{
		Debug.Log("\t\t\t=== MESSENGER PrintEventTable ===");
		foreach (KeyValuePair<string, Delegate> item in eventTable)
		{
			Debug.Log($"\t\t\t{item.Key}\t\t{item.Value}");
		}
		Debug.Log("\n");
	}

	public static bool ContainsSignal(string p_signalName)
	{
		return eventTable.ContainsKey(p_signalName);
	}

	public static void OnListenerAdding(string eventType, Delegate listenerBeingAdded)
	{
		if (!eventTable.ContainsKey(eventType))
		{
			eventTable.Add(eventType, null);
		}
	}

	public static void OnListenerRemoving(string eventType, Delegate listenerBeingRemoved)
	{
	}

	public static void OnListenerRemoved(string eventType)
	{
		if ((object)eventTable[eventType] == null)
		{
			eventTable.Remove(eventType);
		}
	}

	public static void OnBroadcasting(string eventType)
	{
	}

	public static BroadcastException CreateBroadcastSignatureException(string eventType)
	{
		return new BroadcastException($"Broadcasting message \"{eventType}\" but listeners have a different signature than the broadcaster.");
	}

	public static void AddListener(string eventType, SignalHandler.SignalListener handler, bool shouldLock = false)
	{
		SignalHandler.AddListener(eventType, handler, shouldLock);
	}

	public static void AddListener<T>(string eventType, SignalHandler<T>.SignalListener handler, bool shouldLock = false)
	{
		SignalHandler<T>.AddListener(eventType, handler, shouldLock);
	}

	public static void AddListener<T, U>(string eventType, SignalHandler<T, U>.SignalListener handler, bool shouldLock = false)
	{
		SignalHandler<T, U>.AddListener(eventType, handler, shouldLock);
	}

	public static void AddListener<T, U, V>(string eventType, SignalHandler<T, U, V>.SignalListener handler, bool shouldLock = false)
	{
		SignalHandler<T, U, V>.AddListener(eventType, handler, shouldLock);
	}

	public static void AddListener<T, U, V, W>(string eventType, SignalHandler<T, U, V, W>.SignalListener handler, bool shouldLock = false)
	{
		SignalHandler<T, U, V, W>.AddListener(eventType, handler, shouldLock);
	}

	public static void AddListener<T, U, V, W, X>(string eventType, Callback<T, U, V, W, X> handler, bool shouldLock = false)
	{
		if (shouldLock)
		{
			lock (eventTable)
			{
				OnListenerAdding(eventType, handler);
				eventTable[eventType] = (Callback<T, U, V, W, X>)Delegate.Combine((Callback<T, U, V, W, X>)eventTable[eventType], handler);
				return;
			}
		}
		OnListenerAdding(eventType, handler);
		eventTable[eventType] = (Callback<T, U, V, W, X>)Delegate.Combine((Callback<T, U, V, W, X>)eventTable[eventType], handler);
	}

	public static void AddListener<T, U, V, W, X, Y>(string eventType, Callback<T, U, V, W, X, Y> handler, bool shouldLock = false)
	{
		if (shouldLock)
		{
			lock (eventTable)
			{
				OnListenerAdding(eventType, handler);
				eventTable[eventType] = (Callback<T, U, V, W, X, Y>)Delegate.Combine((Callback<T, U, V, W, X, Y>)eventTable[eventType], handler);
				return;
			}
		}
		OnListenerAdding(eventType, handler);
		eventTable[eventType] = (Callback<T, U, V, W, X, Y>)Delegate.Combine((Callback<T, U, V, W, X, Y>)eventTable[eventType], handler);
	}

	public static void AddListener<T, U, V, W, X, Y, Z>(string eventType, Callback<T, U, V, W, X, Y, Z> handler, bool shouldLock = false)
	{
		if (shouldLock)
		{
			lock (eventTable)
			{
				OnListenerAdding(eventType, handler);
				eventTable[eventType] = (Callback<T, U, V, W, X, Y, Z>)Delegate.Combine((Callback<T, U, V, W, X, Y, Z>)eventTable[eventType], handler);
				return;
			}
		}
		OnListenerAdding(eventType, handler);
		eventTable[eventType] = (Callback<T, U, V, W, X, Y, Z>)Delegate.Combine((Callback<T, U, V, W, X, Y, Z>)eventTable[eventType], handler);
	}

	public static void RemoveListener(string eventType, SignalHandler.SignalListener handler, bool shouldLock = false)
	{
		SignalHandler.RemoveListener(eventType, handler, shouldLock);
	}

	public static void RemoveListener<T>(string eventType, SignalHandler<T>.SignalListener handler, bool shouldLock = false)
	{
		SignalHandler<T>.RemoveListener(eventType, handler, shouldLock);
	}

	public static void RemoveListener<T, U>(string eventType, SignalHandler<T, U>.SignalListener handler, bool shouldLock = false)
	{
		SignalHandler<T, U>.RemoveListener(eventType, handler, shouldLock);
	}

	public static void RemoveListener<T, U, V>(string eventType, SignalHandler<T, U, V>.SignalListener handler, bool shouldLock = false)
	{
		SignalHandler<T, U, V>.RemoveListener(eventType, handler, shouldLock);
	}

	public static void RemoveListener<T, U, V, W>(string eventType, SignalHandler<T, U, V, W>.SignalListener handler, bool shouldLock = false)
	{
		SignalHandler<T, U, V, W>.RemoveListener(eventType, handler, shouldLock);
	}

	public static void RemoveListener<T, U, V, W, X>(string eventType, Callback<T, U, V, W, X> handler, bool shouldLock = false)
	{
		if (shouldLock)
		{
			lock (eventTable)
			{
				if (eventTable.ContainsKey(eventType))
				{
					OnListenerRemoving(eventType, handler);
					eventTable[eventType] = (Callback<T, U, V, W, X>)Delegate.Remove((Callback<T, U, V, W, X>)eventTable[eventType], handler);
					OnListenerRemoved(eventType);
				}
				return;
			}
		}
		if (eventTable.ContainsKey(eventType))
		{
			OnListenerRemoving(eventType, handler);
			eventTable[eventType] = (Callback<T, U, V, W, X>)Delegate.Remove((Callback<T, U, V, W, X>)eventTable[eventType], handler);
			OnListenerRemoved(eventType);
		}
	}

	public static void RemoveListener<T, U, V, W, X, Y>(string eventType, Callback<T, U, V, W, X, Y> handler, bool shouldLock = false)
	{
		if (shouldLock)
		{
			lock (eventTable)
			{
				if (eventTable.ContainsKey(eventType))
				{
					OnListenerRemoving(eventType, handler);
					eventTable[eventType] = (Callback<T, U, V, W, X, Y>)Delegate.Remove((Callback<T, U, V, W, X, Y>)eventTable[eventType], handler);
					OnListenerRemoved(eventType);
				}
				return;
			}
		}
		if (eventTable.ContainsKey(eventType))
		{
			OnListenerRemoving(eventType, handler);
			eventTable[eventType] = (Callback<T, U, V, W, X, Y>)Delegate.Remove((Callback<T, U, V, W, X, Y>)eventTable[eventType], handler);
			OnListenerRemoved(eventType);
		}
	}

	public static void RemoveListener<T, U, V, W, X, Y, Z>(string eventType, Callback<T, U, V, W, X, Y, Z> handler, bool shouldLock = false)
	{
		if (shouldLock)
		{
			lock (eventTable)
			{
				if (eventTable.ContainsKey(eventType))
				{
					OnListenerRemoving(eventType, handler);
					eventTable[eventType] = (Callback<T, U, V, W, X, Y, Z>)Delegate.Remove((Callback<T, U, V, W, X, Y, Z>)eventTable[eventType], handler);
					OnListenerRemoved(eventType);
				}
				return;
			}
		}
		if (eventTable.ContainsKey(eventType))
		{
			OnListenerRemoving(eventType, handler);
			eventTable[eventType] = (Callback<T, U, V, W, X, Y, Z>)Delegate.Remove((Callback<T, U, V, W, X, Y, Z>)eventTable[eventType], handler);
			OnListenerRemoved(eventType);
		}
	}

	public static void Broadcast(string eventType)
	{
		SignalHandler.Broadcast(eventType);
	}

	public static void Broadcast<T>(string eventType, T arg1)
	{
		SignalHandler<T>.Broadcast(eventType, arg1);
	}

	public static void Broadcast<T, U>(string eventType, T arg1, U arg2)
	{
		SignalHandler<T, U>.Broadcast(eventType, arg1, arg2);
	}

	public static void Broadcast<T, U, V>(string eventType, T arg1, U arg2, V arg3)
	{
		SignalHandler<T, U, V>.Broadcast(eventType, arg1, arg2, arg3);
	}

	public static void Broadcast<T, U, V, W>(string eventType, T arg1, U arg2, V arg3, W arg4)
	{
		SignalHandler<T, U, V, W>.Broadcast(eventType, arg1, arg2, arg3, arg4);
	}

	public static void Broadcast<T, U, V, W, X>(string eventType, T arg1, U arg2, V arg3, W arg4, X arg5)
	{
		OnBroadcasting(eventType);
		if (eventTable.TryGetValue(eventType, out var value))
		{
			if (!(value is Callback<T, U, V, W, X> callback))
			{
				throw CreateBroadcastSignatureException(eventType);
			}
			callback(arg1, arg2, arg3, arg4, arg5);
		}
	}

	public static void Broadcast<T, U, V, W, X, Y>(string eventType, T arg1, U arg2, V arg3, W arg4, X arg5, Y arg6)
	{
		OnBroadcasting(eventType);
		if (eventTable.TryGetValue(eventType, out var value))
		{
			if (!(value is Callback<T, U, V, W, X, Y> callback))
			{
				throw CreateBroadcastSignatureException(eventType);
			}
			callback(arg1, arg2, arg3, arg4, arg5, arg6);
		}
	}

	public static void Broadcast<T, U, V, W, X, Y, Z>(string eventType, T arg1, U arg2, V arg3, W arg4, X arg5, Y arg6, Z arg7)
	{
		OnBroadcasting(eventType);
		if (eventTable.TryGetValue(eventType, out var value))
		{
			if (!(value is Callback<T, U, V, W, X, Y, Z> callback))
			{
				throw CreateBroadcastSignatureException(eventType);
			}
			callback(arg1, arg2, arg3, arg4, arg5, arg6, arg7);
		}
	}
}
