using System;
using UnityEngine;

[Serializable]
public struct TickRange
{
	[SerializeField]
	private int m_startTick;

	[SerializeField]
	private int m_endTick;

	public TickRange(int p_startTick, int p_endTick)
	{
		m_startTick = p_startTick;
		m_endTick = p_endTick;
	}

	public void IncreaseEndTick(int p_amount)
	{
		for (int i = 0; i < p_amount; i++)
		{
			m_endTick++;
			if (m_endTick > 480)
			{
				m_endTick = 0;
			}
		}
	}

	public bool IsInRange(int p_tick)
	{
		if (m_startTick < m_endTick)
		{
			if (p_tick >= m_startTick)
			{
				return p_tick < m_endTick;
			}
			return false;
		}
		if (m_startTick > m_endTick)
		{
			if (p_tick < m_startTick)
			{
				return p_tick < m_endTick;
			}
			return true;
		}
		return p_tick == m_startTick;
	}

	public int GetStartTick()
	{
		return m_startTick;
	}

	public int GetEndTick()
	{
		return m_endTick;
	}

	public override string ToString()
	{
		if (GameManager.Instance != null)
		{
			return GameManager.Instance.ConvertTickToTime(m_startTick) + " - " + GameManager.Instance.ConvertTickToTime(m_endTick);
		}
		return m_startTick + " - " + m_endTick;
	}
}
