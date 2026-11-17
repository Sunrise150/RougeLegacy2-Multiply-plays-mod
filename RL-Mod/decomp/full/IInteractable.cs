public interface IInteractable
{
	UnityEvent_GameObject TriggerOnEnterEvent { get; }

	UnityEvent_GameObject TriggerOnExitEvent { get; }

	bool IsInteractable { get; }

	void SetIsInteractableActive(bool canInteract);
}
