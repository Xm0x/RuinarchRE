using System;
using UnityEngine;

namespace Pathfinding;

[UniqueComponent(tag = "ai.destination")]
[HelpURL("http://arongranberg.com/astar/docs/class_pathfinding_1_1_a_i_destination_setter.php")]
public class CharacterDestinationSetter : VersionedMonoBehaviour
{
	public Vector3 target;

	private CharacterAIPath ai;

	public Transform targetTrans;

	private void OnEnable()
	{
		ai = GetComponent<CharacterAIPath>();
		if (ai != null)
		{
			CharacterAIPath characterAIPath = ai;
			characterAIPath.onSearchPath = (Action)Delegate.Combine(characterAIPath.onSearchPath, new Action(Update));
		}
	}

	private void OnDisable()
	{
		if (ai != null)
		{
			CharacterAIPath characterAIPath = ai;
			characterAIPath.onSearchPath = (Action)Delegate.Remove(characterAIPath.onSearchPath, new Action(Update));
		}
	}

	private void Update()
	{
		if (targetTrans != null && ai != null)
		{
			ai.destination = targetTrans.position;
		}
	}

	public void SetDestination(Vector3 destination)
	{
		target = destination;
		ai.destination = destination;
		ai.canSearch = true;
	}

	public void SetTranformTarget(Transform target)
	{
		ai.canSearch = true;
		targetTrans = target;
	}

	public void ClearPath()
	{
		target = new Vector3(float.PositiveInfinity, float.PositiveInfinity, float.PositiveInfinity);
	}
}
