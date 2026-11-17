using UnityEngine;

public class ArenaRoomController : BaseSpecialRoomController
{
	[SerializeField]
	private TunnelSpawnController m_tunnelSpawner;

	protected override void OnPlayerEnterRoom(object sender, RoomViaDoorEventArgs eventArgs)
	{
		base.OnPlayerEnterRoom(sender, eventArgs);
		Tunnel tunnel = m_tunnelSpawner.Tunnel;
		if (!base.IsRoomComplete && (bool)tunnel)
		{
			tunnel.SetIsLocked(isLocked: false);
			tunnel.SetIsVisible(isVisible: true);
			ObjectReferenceFinder component = tunnel.GameObject.GetComponent<ObjectReferenceFinder>();
			if ((bool)component)
			{
				component.GetObject("PropPointLight_Lantern").SetActive(value: true);
				component.GetObject("Warp").SetActive(value: true);
				component.GetObject("PortalGlowSprite").SetActive(value: true);
				component.GetObject("Particles").SetActive(value: true);
			}
			SummonRuleController componentInChildren = tunnel.Destination.Room.gameObject.GetComponentInChildren<SummonRuleController>();
			if ((bool)componentInChildren && componentInChildren.IsArenaComplete)
			{
				RoomCompleted();
			}
		}
		if (base.IsRoomComplete && (bool)tunnel)
		{
			ObjectReferenceFinder component2 = tunnel.GameObject.GetComponent<ObjectReferenceFinder>();
			if ((bool)component2)
			{
				component2.GetObject("PropPointLight_Lantern").SetActive(value: false);
				component2.GetObject("Warp").SetActive(value: false);
				component2.GetObject("PortalGlowSprite").SetActive(value: false);
				component2.GetObject("Particles").SetActive(value: false);
			}
			else
			{
				tunnel.SetIsVisible(isVisible: false);
			}
			tunnel.SetIsLocked(isLocked: true);
		}
	}
}
