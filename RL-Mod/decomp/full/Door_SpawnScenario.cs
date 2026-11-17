using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Door_SpawnScenario : SpawnScenario
{
	public InclusiveOrExclusive InclusiveOrExclusive;

	public int Number = -1;

	public RoomSide Side = RoomSide.None;

	public DoorState State;

	public override bool IsTrue { get; protected set; }

	public override SpawnScenarioType Type => SpawnScenarioType.Door;

	public override string GizmoDescription
	{
		get
		{
			string text = "";
			text = ((InclusiveOrExclusive != 0) ? (text + "!") : (text + "*"));
			string text2 = "A";
			if (Number != -1)
			{
				text2 = Number.ToString();
			}
			char c = Side.ToString().First();
			char c2 = State.ToString().First();
			return $"{text}({c}{text2}){c2}";
		}
	}

	public Door_SpawnScenario()
		: this(RoomSide.Top, -1, InclusiveOrExclusive.Inclusive, DoorState.Open)
	{
	}

	public Door_SpawnScenario(RoomSide side, int doorNumber, InclusiveOrExclusive inclusiveOrExclusive, DoorState doorState)
	{
		if (side != RoomSide.Left && side != RoomSide.Right && side != 0 && side != RoomSide.Bottom)
		{
			side = RoomSide.Top;
		}
		Side = side;
		Number = doorNumber;
		InclusiveOrExclusive = inclusiveOrExclusive;
		State = doorState;
	}

	public override void RunIsTrueCheck(BaseRoom room)
	{
		if ((bool)room && room is Room)
		{
			Room room2 = room as Room;
			bool isTrue = true;
			bool flag = InclusiveOrExclusive == InclusiveOrExclusive.Exclusive;
			List<Door> doors = room2.Doors;
			List<Door> list = null;
			if (Number == -1)
			{
				list = room2.GetDoorsOnSide(Side);
			}
			else
			{
				Door door = room2.GetDoor(Side, Number);
				if (door != null)
				{
					list = new List<Door> { door };
				}
			}
			if (State == DoorState.Open)
			{
				if (list != null && list.Count > 0)
				{
					if (flag)
					{
						if (Side != RoomSide.Any)
						{
							foreach (Door item in doors)
							{
								if (!list.Contains(item))
								{
									isTrue = false;
									break;
								}
							}
						}
						else if (list.Count > 1)
						{
							isTrue = false;
						}
					}
				}
				else
				{
					isTrue = false;
				}
			}
			else if (State == DoorState.Closed && list != null && list.Count > 0)
			{
				isTrue = false;
			}
			IsTrue = isTrue;
		}
		else
		{
			Debug.LogFormat("<color=red>| {0} | Room argument is null</color>", this);
		}
	}

	public override void RunIsTrueCheck(GridPointManager gridPointManager)
	{
		bool isTrue = true;
		bool flag = InclusiveOrExclusive == InclusiveOrExclusive.Exclusive;
		if (gridPointManager.IsRoomMirrored)
		{
			if (Number != -1)
			{
				Number = RoomUtility.GetMirrorDoorNumber(new DoorLocation(Side, Number), gridPointManager.Size);
			}
			if (Side != RoomSide.Any)
			{
				Side = RoomUtility.GetMirrorSide(Side);
			}
		}
		List<DoorLocation> list = new List<DoorLocation>();
		if (Number == -1)
		{
			foreach (DoorLocation item in gridPointManager.GetDoorLocationsOnSide(Side))
			{
				if (gridPointManager.GetConnectedRoom(item) != null)
				{
					list.Add(item);
				}
			}
		}
		else
		{
			DoorLocation doorLocation = gridPointManager.GetDoorLocation(Side, Number);
			if (gridPointManager.GetConnectedRoom(doorLocation) != null)
			{
				list.Add(doorLocation);
			}
		}
		if (State == DoorState.Open)
		{
			if (list.Count > 0)
			{
				if (flag)
				{
					if (Side == RoomSide.Any)
					{
						Debug.LogFormat("<color=red>| Door_SpawnScenario | An exclusive Door Spawn Scenario in Room ({0}) has not has its <b>Side</b> field set.</color>", gridPointManager.RoomMetaData.ID);
					}
					List<DoorLocation> list2 = new List<DoorLocation>();
					foreach (DoorLocation doorLocation2 in gridPointManager.DoorLocations)
					{
						if (gridPointManager.GetConnectedRoom(doorLocation2) != null)
						{
							list2.Add(doorLocation2);
						}
					}
					if (list2.Count == 1)
					{
						if (Side != RoomSide.Any && (list2[0].RoomSide != Side || (Number != -1 && list2[0].DoorNumber != Number)))
						{
							isTrue = false;
						}
					}
					else
					{
						isTrue = false;
					}
				}
			}
			else
			{
				isTrue = false;
			}
		}
		else if (State == DoorState.Closed && list.Count > 0)
		{
			isTrue = false;
		}
		IsTrue = isTrue;
	}
}
