using System.Collections;
using System.Collections.Generic;

namespace RLWorldCreation;

public class MergeRoomStrategy_Bridge : MergeRoomStrategy
{
	public override IEnumerator Run(CreateMergeRooms_BuildStage buildStage, BiomeController biomeController)
	{
		m_buildStage = buildStage;
		List<GridPointManager> potentialMergeTargets_V = GetPotentialMergeTargets_V2(biomeController, BiomeType.Stone, RoomType.Transition);
		if (potentialMergeTargets_V.Count > 0)
		{
			yield return MergeRooms_V2(biomeController, BiomeType.Stone, potentialMergeTargets_V, shuffle: false);
		}
	}

	protected override bool AttemptMerge(BiomeController biomeController, RoomSide side, GridPointManager connectedRoom)
	{
		bool result = base.AttemptMerge(biomeController, side, connectedRoom);
		if (connectedRoom.RoomMetaData == RoomReferenceLibrary.GetRoomMetaData(RoomReferenceType.SplitRoomRight_Bridge))
		{
			result = false;
		}
		return result;
	}
}
