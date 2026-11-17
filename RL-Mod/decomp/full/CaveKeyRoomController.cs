using System;
using UnityEngine;

public class CaveKeyRoomController : BaseSpecialRoomController
{
	private Action<MonoBehaviour, EventArgs> m_onRelicChanged;

	protected override void Awake()
	{
		base.Awake();
		m_onRelicChanged = OnRelicChanged;
		Messenger<GameMessenger, GameEvent>.AddListener(GameEvent.RelicLevelChanged, m_onRelicChanged);
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		Messenger<GameMessenger, GameEvent>.RemoveListener(GameEvent.RelicLevelChanged, m_onRelicChanged);
	}

	private void OnRelicChanged(object sender, EventArgs args)
	{
		if (SaveManager.PlayerSaveData.GetFlag(PlayerSaveFlag.CaveTuningForkTriggered))
		{
			MapController.SetCaveWhitePipVisibility(visible: true);
		}
	}
}
