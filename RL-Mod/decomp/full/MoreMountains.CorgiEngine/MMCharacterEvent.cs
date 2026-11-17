namespace MoreMountains.CorgiEngine;

public struct MMCharacterEvent
{
	public Character TargetCharacter;

	public MMCharacterEventTypes EventType;

	public MMCharacterEvent(Character character, MMCharacterEventTypes eventType)
	{
		TargetCharacter = character;
		EventType = eventType;
	}
}
