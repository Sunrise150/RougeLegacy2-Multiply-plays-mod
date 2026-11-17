using System;
using System.Collections.Generic;
using UnityEngine;

namespace Foreground;

public class ForegroundController : MonoBehaviour
{
	[SerializeField]
	private bool m_lowerLeft;

	[SerializeField]
	private bool m_lowerRight;

	[SerializeField]
	private bool m_upperLeft;

	[SerializeField]
	private bool m_upperRight;

	[SerializeField]
	private Room m_room;

	[SerializeField]
	private CameraZoomController m_zoomController;

	private List<ForegroundGroup> m_foregroundGroups = new List<ForegroundGroup>();

	public BaseRoom Room { get; private set; }

	public bool IsInitialized { get; private set; }

	private void Awake()
	{
		m_room.RoomMergedRelay.AddListener(OnRoomMerged);
		m_room.RoomDestroyedRelay.AddListener(OnRoomDestroyed);
	}

	public void CreateForeground(ForegroundLocation foregroundLocation)
	{
		if (m_zoomController == null || foregroundLocation == ForegroundLocation.None)
		{
			return;
		}
		ForegroundGroup[] array = new ForegroundGroup[0];
		ForegroundLocation foregroundLocation2 = foregroundLocation;
		switch (foregroundLocation)
		{
		case ForegroundLocation.LowerRightCorner:
			foregroundLocation2 = ForegroundLocation.LowerLeftCorner;
			break;
		case ForegroundLocation.UpperRightCorner:
			foregroundLocation2 = ForegroundLocation.UpperLeftCorner;
			break;
		}
		if (foregroundLocation2 == ForegroundLocation.LowerLeftCorner && BiomeArtDataLibrary.GetArtData(m_room.AppearanceBiomeType).ForegroundData.Lower != null)
		{
			array = BiomeArtDataLibrary.GetArtData(m_room.AppearanceBiomeType).ForegroundData.Lower.GetForegrounds();
		}
		else if (BiomeArtDataLibrary.GetArtData(m_room.AppearanceBiomeType).ForegroundData.Upper != null)
		{
			array = BiomeArtDataLibrary.GetArtData(m_room.AppearanceBiomeType).ForegroundData.Upper.GetForegrounds();
		}
		if (m_zoomController.OverrideZoomLevel)
		{
			_ = m_zoomController.ZoomLevel;
		}
		if (array.Length == 0)
		{
			return;
		}
		ForegroundGroup randomForegroundGroupInstance = GetRandomForegroundGroupInstance(array);
		if (!randomForegroundGroupInstance)
		{
			return;
		}
		m_foregroundGroups.Add(randomForegroundGroupInstance);
		randomForegroundGroupInstance.transform.localPosition = GetLocalCornerCoordinates(foregroundLocation);
		for (int i = 0; i < randomForegroundGroupInstance.PropSpawnControllers.Length; i++)
		{
			if (foregroundLocation == ForegroundLocation.LowerRightCorner || foregroundLocation == ForegroundLocation.UpperRightCorner)
			{
				randomForegroundGroupInstance.PropSpawnControllers[i].Mirror();
				randomForegroundGroupInstance.PropSpawnControllers[i].transform.localPosition = RoomUtility.GetMirrorPosition(randomForegroundGroupInstance.PropSpawnControllers[i].transform);
			}
			randomForegroundGroupInstance.PropSpawnControllers[i].SetRoom(m_room);
			SpriteRenderer component = randomForegroundGroupInstance.PropSpawnControllers[i].GetComponent<SpriteRenderer>();
			if ((bool)component)
			{
				component.enabled = false;
			}
		}
	}

	private Vector3 GetLocalCornerCoordinates(ForegroundLocation location)
	{
		return location switch
		{
			ForegroundLocation.LowerLeftCorner => new Vector3(-0.5f * (float)m_room.Size.x * 32f, -0.5f * (float)m_room.Size.y * 18f, 0f), 
			ForegroundLocation.LowerRightCorner => new Vector3(0.5f * (float)m_room.Size.x * 32f, -0.5f * (float)m_room.Size.y * 18f, 0f), 
			ForegroundLocation.UpperLeftCorner => new Vector3(-0.5f * (float)m_room.Size.x * 32f, 0.5f * (float)m_room.Size.y * 18f, 0f), 
			ForegroundLocation.UpperRightCorner => new Vector3(0.5f * (float)m_room.Size.x * 32f, 0.5f * (float)m_room.Size.y * 18f, 0f), 
			_ => throw new InvalidOperationException(), 
		};
	}

	private ForegroundGroup GetRandomForegroundGroupInstance(ForegroundGroup[] potentialForegroundGroups)
	{
		int num = 0;
		if (potentialForegroundGroups.Length > 1)
		{
			num = RNGManager.GetRandomNumber(RngID.Prop, ToString(), 0, potentialForegroundGroups.Length);
		}
		return UnityEngine.Object.Instantiate(potentialForegroundGroups[num], base.transform, worldPositionStays: false);
	}

	private void Mirror()
	{
		bool lowerLeft = m_lowerLeft;
		bool lowerRight = m_lowerRight;
		bool upperLeft = m_upperLeft;
		bool upperRight = m_upperRight;
		m_lowerLeft = lowerRight;
		m_lowerRight = lowerLeft;
		m_upperLeft = upperRight;
		m_upperRight = upperLeft;
	}

	private void OnRoomMerged(object sender, EventArgs eventArgs)
	{
		foreach (ForegroundGroup foregroundGroup in m_foregroundGroups)
		{
			PropSpawnController[] propSpawnControllers = foregroundGroup.PropSpawnControllers;
			for (int i = 0; i < propSpawnControllers.Length; i++)
			{
				propSpawnControllers[i].gameObject.SetActive(value: false);
			}
			foregroundGroup.gameObject.SetActive(value: false);
		}
	}

	private void OnRoomDestroyed(object sender, EventArgs eventArgs)
	{
		m_room.RoomMergedRelay.RemoveListener(OnRoomMerged);
		m_room.RoomDestroyedRelay.RemoveListener(OnRoomDestroyed);
		IsInitialized = false;
	}

	public void Initialize()
	{
		if ((!GameUtility.IsInLevelEditor || !(m_room != OnPlayManager.CurrentRoom)) && !(BiomeArtDataLibrary.GetArtData(m_room.AppearanceBiomeType) == null))
		{
			if (m_room.IsMirrored)
			{
				Mirror();
			}
			int bottomSpawnOdds = BiomeArtDataLibrary.GetArtData(m_room.AppearanceBiomeType).ForegroundData.BottomSpawnOdds;
			if (m_lowerLeft && RNGManager.GetRandomNumber(RngID.Prop, "Spawn LL FG Element?", 0, 100) < bottomSpawnOdds)
			{
				CreateForeground(ForegroundLocation.LowerLeftCorner);
			}
			if (m_lowerRight && RNGManager.GetRandomNumber(RngID.Prop, "Spawn LR FG Element?", 0, 100) < bottomSpawnOdds)
			{
				CreateForeground(ForegroundLocation.LowerRightCorner);
			}
			bottomSpawnOdds = BiomeArtDataLibrary.GetArtData(m_room.AppearanceBiomeType).ForegroundData.UpperSpawnOdds;
			if (m_upperLeft && RNGManager.GetRandomNumber(RngID.Prop, "Spawn UL FG Element?", 0, 100) < bottomSpawnOdds)
			{
				CreateForeground(ForegroundLocation.UpperLeftCorner);
			}
			if (m_upperRight && RNGManager.GetRandomNumber(RngID.Prop, "Spawn UR FG Element?", 0, 100) < bottomSpawnOdds)
			{
				CreateForeground(ForegroundLocation.UpperRightCorner);
			}
			IsInitialized = true;
		}
	}
}
