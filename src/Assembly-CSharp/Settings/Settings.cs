using System;
using UnityEngine;

namespace Settings;

[Serializable]
public class Settings
{
	[Header("Graphics")]
	public string resolution;

	public int graphicsQuality;

	public bool fullscreen;

	public bool isVsyncOn;

	public bool doNotShowVideos;

	[Header("Audio")]
	public float musicVolume;

	public float masterVolume;

	public float sfxVolume;

	[Header("Misc")]
	public string language;

	public bool skipEarlyAccessAnnouncement;

	public bool disableCameraShake;

	public bool randomizeMonsterNames;

	public bool arachnophobiaToggle;

	public bool shouldAutosave;

	public int logLimit;

	[Header("Controls")]
	public bool useEdgePanning;

	public bool confineCursor;

	public int cameraPanSpeed;

	public Settings(Settings p_copy)
	{
		resolution = p_copy.resolution;
		graphicsQuality = p_copy.graphicsQuality;
		fullscreen = p_copy.fullscreen;
		isVsyncOn = p_copy.isVsyncOn;
		doNotShowVideos = p_copy.doNotShowVideos;
		musicVolume = p_copy.musicVolume;
		masterVolume = p_copy.masterVolume;
		sfxVolume = p_copy.sfxVolume;
		language = p_copy.language;
		skipEarlyAccessAnnouncement = p_copy.skipEarlyAccessAnnouncement;
		disableCameraShake = p_copy.disableCameraShake;
		randomizeMonsterNames = p_copy.randomizeMonsterNames;
		arachnophobiaToggle = p_copy.arachnophobiaToggle;
		shouldAutosave = p_copy.shouldAutosave;
		logLimit = p_copy.logLimit;
		useEdgePanning = p_copy.useEdgePanning;
		confineCursor = p_copy.confineCursor;
		cameraPanSpeed = p_copy.cameraPanSpeed;
	}
}
