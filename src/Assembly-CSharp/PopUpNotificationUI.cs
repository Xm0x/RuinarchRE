using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PopUpNotificationUI : MonoBehaviour
{
	public static PopUpNotificationUI Instance;

	[SerializeField]
	private RuinarchText m_popupText;

	[SerializeField]
	private GameObject m_parentGO;

	private readonly List<Action> m_pendingPopups = new List<Action>(5);

	private void Awake()
	{
		Instance = this;
	}

	public void ShowPlayerPoppingTextNotif(string p_message, Transform p_startingPosition = null)
	{
		if (m_parentGO.activeInHierarchy)
		{
			m_pendingPopups.Add(delegate
			{
				ShowPlayerPoppingTextNotif(p_message, p_startingPosition);
			});
			return;
		}
		AudioManager.Instance.OnTextPopUpSoundPlay();
		StopAllCoroutines();
		m_parentGO.SetActive(value: true);
		m_popupText.text = p_message;
		Vector3 p_targetPos = m_parentGO.transform.position;
		if (p_startingPosition != null)
		{
			Vector3 vector = (m_parentGO.transform.position = p_startingPosition.transform.position);
			p_targetPos = vector;
		}
		StartCoroutine(Move(p_targetPos));
	}

	public void ShowPlayerPoppingTextNotif(string p_message, int p_durationInSeconds, Transform p_startingPosition = null)
	{
		if (m_parentGO.activeInHierarchy)
		{
			m_pendingPopups.Add(delegate
			{
				ShowPlayerPoppingTextNotif(p_message, p_durationInSeconds, p_startingPosition);
			});
			return;
		}
		AudioManager.Instance.OnTextPopUpSoundPlay();
		StopAllCoroutines();
		m_parentGO.SetActive(value: true);
		m_popupText.text = p_message;
		Vector3 p_targetPos = m_parentGO.transform.position;
		if (p_startingPosition != null)
		{
			Vector3 vector = (m_parentGO.transform.position = p_startingPosition.transform.position);
			p_targetPos = vector;
		}
		StartCoroutine(Move(p_targetPos, p_durationInSeconds));
	}

	private IEnumerator Move(Vector3 p_targetPos, int p_durationInSeconds = 2)
	{
		yield return null;
		p_targetPos.y += 40f;
		float timer = 0f;
		while (timer < (float)p_durationInSeconds)
		{
			timer += Time.deltaTime;
			m_parentGO.transform.position = Vector3.MoveTowards(m_parentGO.transform.position, p_targetPos, 120f * Time.deltaTime);
			yield return 0;
		}
		m_parentGO.SetActive(value: false);
		TryShowNextPopup();
	}

	private void TryShowNextPopup()
	{
		if (m_pendingPopups.Count > 0)
		{
			m_pendingPopups[0]();
			m_pendingPopups.RemoveAt(0);
		}
	}
}
