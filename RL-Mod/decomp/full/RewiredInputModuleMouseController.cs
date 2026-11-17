using Rewired;
using Rewired.Integration.UnityUI;
using UnityEngine;

public class RewiredInputModuleMouseController : MonoBehaviour
{
	private RewiredStandaloneInputModule m_reInputModule;

	private void Awake()
	{
		m_reInputModule = GetComponent<RewiredStandaloneInputModule>();
	}

	private void OnEnable()
	{
		ReInput.controllers.RemoveLastActiveControllerChangedDelegate(LastControllerChanged);
		ReInput.controllers.AddLastActiveControllerChangedDelegate(LastControllerChanged);
	}

	private void LastControllerChanged(Controller controller)
	{
		if (controller.type == ControllerType.Joystick)
		{
			m_reInputModule.allowMouseInput = false;
		}
		else
		{
			m_reInputModule.allowMouseInput = true;
		}
	}
}
