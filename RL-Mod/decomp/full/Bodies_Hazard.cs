using UnityEngine;

public class Bodies_Hazard : Hazard
{
	[SerializeField]
	private GameObject[] m_bodies;

	private int m_activeBodyIndex;

	public override void Initialize(HazardArgs hazardArgs)
	{
		base.Initialize(hazardArgs);
		GameObject[] bodies = m_bodies;
		foreach (GameObject gameObject in bodies)
		{
			if (gameObject.activeSelf)
			{
				gameObject.SetActive(value: false);
			}
		}
		if (base.transform.localEulerAngles.z > 45f && base.transform.localEulerAngles.z < 315f)
		{
			return;
		}
		int num = 37;
		int num2 = RNGManager.GetRandomNumber(RngID.SpecialProps_RoomSeed, "Bodies_Hazard.Initialize()", 0, 100);
		if (GameUtility.IsInLevelEditor)
		{
			num2 = Random.Range(0, 100);
		}
		if (num2 <= num)
		{
			m_activeBodyIndex = RNGManager.GetRandomNumber(RngID.SpecialProps_RoomSeed, "Bodies_Hazard.Initialize()", 0, m_bodies.Length);
			bool flag = RNGManager.GetRandomNumber(RngID.SpecialProps_RoomSeed, "Bodies_Hazard.Initialize()", 0, 2) == 1;
			GameObject obj = m_bodies[m_activeBodyIndex];
			obj.SetActive(value: true);
			Vector3 localScale = obj.transform.localScale;
			localScale.x = Mathf.Abs(localScale.x);
			if (flag)
			{
				localScale.x = 0f - localScale.x;
			}
			obj.transform.localScale = localScale;
			SpriteRenderer componentInChildren = obj.GetComponentInChildren<SpriteRenderer>();
			Vector2 leftSide = componentInChildren.bounds.center - componentInChildren.bounds.extents;
			leftSide.y = base.transform.position.y;
			Vector2 rightSide = componentInChildren.bounds.center + componentInChildren.bounds.extents;
			rightSide.y = base.transform.position.y;
			CheckGroundRaycast(leftSide, rightSide);
		}
	}

	private void CheckGroundRaycast(Vector2 leftSide, Vector2 rightSide)
	{
		if (!Physics2D.Raycast(leftSide, Vector2.down, 0.5f, PlayerManager.GetPlayerController().ControllerCorgi.SavedPlatformMask) || !Physics2D.Raycast(rightSide, Vector2.down, 0.5f, PlayerManager.GetPlayerController().ControllerCorgi.SavedPlatformMask))
		{
			m_bodies[m_activeBodyIndex].SetActive(value: false);
		}
	}

	public override void ResetHazard()
	{
	}
}
