using System.Collections;
using UnityEngine;

namespace LapinerTools.uMyGUI;

public class uMyGUI_AnimationTrigger : MonoBehaviour
{
	public enum ETriggerMode
	{
		ON_ENABLE,
		ON_DISABLE,
		ON_UMYGUI_ACTIVATETAB,
		ON_UMYGUI_DEACTIVATETAB,
		REDIRECT_ONMYGUI_EVENTS
	}

	[SerializeField]
	private Animation m_animation;

	[SerializeField]
	private string m_clipName;

	[SerializeField]
	private ETriggerMode m_condition;

	[SerializeField]
	private bool m_isActivateOnAnimStart;

	[SerializeField]
	private bool m_isDeactivateOnAnimEnd;

	[SerializeField]
	private MonoBehaviour m_alternativeCoroutineWorker;

	[SerializeField]
	private GameObject m_redirectDestination;

	private void OnEnable()
	{
		if (m_condition == ETriggerMode.ON_ENABLE)
		{
			Play();
		}
	}

	private void OnDisable()
	{
		if (m_condition == ETriggerMode.ON_DISABLE)
		{
			Play();
		}
	}

	private void uMyGUI_OnActivateTab()
	{
		if (m_condition == ETriggerMode.ON_UMYGUI_ACTIVATETAB)
		{
			Play();
		}
		else if (m_condition == ETriggerMode.REDIRECT_ONMYGUI_EVENTS)
		{
			if (m_redirectDestination == null)
			{
				Debug.LogError("LE_AnimationTrigger: uMyGUI_OnActivateTab: REDIRECT_ONMYGUI_EVENTS mode requires m_redirectDestination to be set!");
			}
			else if (m_redirectDestination.activeInHierarchy)
			{
				m_redirectDestination.SendMessage("uMyGUI_OnActivateTab");
			}
		}
	}

	private void uMyGUI_OnDeactivateTab()
	{
		if (m_condition == ETriggerMode.ON_UMYGUI_DEACTIVATETAB)
		{
			Play();
		}
		else if (m_condition == ETriggerMode.REDIRECT_ONMYGUI_EVENTS)
		{
			if (m_redirectDestination == null)
			{
				Debug.LogError("LE_AnimationTrigger: uMyGUI_OnDeactivateTab: REDIRECT_ONMYGUI_EVENTS mode requires m_redirectDestination to be set!");
			}
			else if (m_redirectDestination.activeInHierarchy)
			{
				m_redirectDestination.SendMessage("uMyGUI_OnDeactivateTab");
			}
		}
	}

	private void Play()
	{
		if (m_animation != null)
		{
			if (m_isActivateOnAnimStart)
			{
				m_animation.gameObject.SetActive(value: true);
			}
			if (m_isDeactivateOnAnimEnd && m_animation[m_clipName] != null)
			{
				((m_alternativeCoroutineWorker != null) ? m_alternativeCoroutineWorker : this).StartCoroutine(DeactivateAfterDelay(m_animation.gameObject, m_animation[m_clipName].length));
			}
			m_animation.Play(m_clipName);
		}
		else
		{
			Debug.LogError("LE_AnimationTrigger: OnDisable: lost reference to Animation!");
		}
	}

	private IEnumerator DeactivateAfterDelay(GameObject p_object, float p_delay)
	{
		yield return new WaitForSeconds(p_delay);
		if (p_object != null)
		{
			p_object.SetActive(value: false);
		}
	}
}
