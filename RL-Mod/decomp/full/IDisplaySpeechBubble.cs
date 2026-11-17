public interface IDisplaySpeechBubble
{
	bool ShouldDisplaySpeechBubble { get; }

	SpeechBubbleType BubbleType { get; }
}
