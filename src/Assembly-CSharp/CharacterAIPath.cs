using Pathfinding;
using UnityEngine;

public class CharacterAIPath : AILerp
{
	public CharacterMarker marker;

	public int searchLength = 1000;

	public int spread = 5000;

	public float aimStrength = 1f;

	private bool _hasReachedTarget;

	public bool isStopMovement { get; private set; }

	public Path currentPath { get; private set; }

	public bool hasReachedTarget { get; private set; }

	private void OnDestroy()
	{
		marker = null;
		currentPath = null;
		marker = null;
	}

	public override void OnTargetReached()
	{
		base.OnTargetReached();
		if (!_hasReachedTarget && base.reachedEndOfPath && (marker.destinationSetter.target != null || !float.IsPositiveInfinity(base.destination.x) || marker.hasFleePath))
		{
			_hasReachedTarget = true;
			canSearch = true;
			bool shouldRecomputePath = false;
			marker.ArrivedAtTarget(ref shouldRecomputePath);
			currentPath = null;
			if (shouldRecomputePath)
			{
				marker.StartMovement();
			}
		}
	}

	protected override void OnPathComplete(Path newPath)
	{
		if (marker.character == null || marker.character.isDead)
		{
			ClearAllCurrentPathData();
			return;
		}
		currentPath = newPath;
		if (newPath is ConstantPath constantPath)
		{
			marker.OnStrollPathComputed(constantPath);
		}
		else
		{
			base.OnPathComplete(newPath);
		}
		_hasReachedTarget = false;
		if (marker.character.currentJob == null)
		{
			return;
		}
		int jobTypeNodeDistanceLimit = marker.character.currentJob.jobType.GetJobTypeNodeDistanceLimit();
		if (jobTypeNodeDistanceLimit != -1 && newPath.vectorPath.Count > jobTypeNodeDistanceLimit)
		{
			if (marker.character.currentJob.jobType == JOB_TYPE.REPORT_CRIME)
			{
				marker.character.currentJob.LogUnableToDoReportCrime(marker.character);
			}
			marker.character.currentJob.CancelJob();
			ClearAllCurrentPathData();
		}
	}

	public override void SearchPath()
	{
		if (!float.IsPositiveInfinity(base.destination.x) && marker.character != null)
		{
			_hasReachedTarget = false;
			if (base.onSearchPath != null)
			{
				base.onSearchPath();
			}
			lastRepath = Time.time;
			Vector3 feetPosition = GetFeetPosition();
			canSearchAgain = false;
			ABPath aBPath = ABPath.Construct(feetPosition, base.destination);
			aBPath.calculatePartial = true;
			SetPath(aBPath);
		}
	}

	public override bool UpdateMe()
	{
		if (!marker.gameObject.activeSelf || marker.character == null)
		{
			return false;
		}
		marker.UpdatePosition();
		if (marker.character == null || !marker.character.limiterComponent.canMove || isStopMovement || GameManager.Instance.isPaused)
		{
			return false;
		}
		UpdateRotation();
		return base.UpdateMe();
	}

	public override void SetTransformPosition(Vector3 nextPosition, Quaternion nextRotation)
	{
		if (updatePosition)
		{
			marker.rigidbody.MovePosition(nextPosition);
		}
	}

	private void UpdateRotation()
	{
		if (marker.isMoving && marker.character.carryComponent.IsNotBeingCarried() && currentPath != null)
		{
			Quaternion localRotation = Quaternion.LookRotation(upwards: interpolator.valid ? interpolator.tangent : Vector3.zero, forward: Vector3.forward);
			marker.visualsParent.localRotation = localRotation;
		}
		else if (marker.character.currentActionNode != null && marker.character.currentActionNode.poiTarget != marker.character && marker.character.currentActionNode.poiTarget.gridTileLocation != null)
		{
			marker.LookAt(marker.character.currentActionNode.poiTarget.gridTileLocation.centeredWorldLocation);
		}
	}

	public void SetIsStopMovement(bool state)
	{
		isStopMovement = state;
	}

	public void ClearAllCurrentPathData()
	{
		currentPath = null;
		path = null;
		_hasReachedTarget = false;
		marker.SetTargetTransform(null);
		marker.SetDestination(Vector3.positiveInfinity, null);
		marker.ClearArrivalAction();
		interpolator.SetPath(null);
		marker.StopMovement();
		if (marker.character != null)
		{
			marker.character.partyComponent.UnfollowBeacon();
		}
	}

	public void ResetThis()
	{
		ResetEndReachedDistance();
		ClearAllCurrentPathData();
		isStopMovement = false;
	}

	public void ResetEndReachedDistance()
	{
		endReachDistance = marker.endReachedDistance;
	}

	public void SetEndReachedDistance(float amount)
	{
		endReachDistance = amount;
	}
}
