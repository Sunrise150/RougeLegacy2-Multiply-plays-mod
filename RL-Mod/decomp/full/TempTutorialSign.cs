using UnityEngine;

public class TempTutorialSign : MonoBehaviour
{
	public void ActivateTutorialSign()
	{
		_ = PlayerManager.IsInstantiated;
	}

	private void DisplaySwordTutorial()
	{
		string text = "[Attack] - Slash.  **You can move while swinging.</LB>(Air) [MoveVertical-]+[Jump] - Spinkick";
		DialogueManager.AddNonLocDialogue("Sword Tutorial - Demo", text);
	}

	private void DisplaySaberTutorial()
	{
		string text = "(Ground) [Attack] - Lunge</LB>(Air) [Attack] - Cut. **Cuts can combo into Lunges if timed correctly.</LB>(Air) [MoveVertical-]+[Jump] - Spinkick";
		DialogueManager.AddNonLocDialogue("Saber Tutorial - Demo", text);
	}

	private void DisplayAxeTutorial()
	{
		string text = "(Ground) [Attack] - Crescent Strike</LB>(Air) [Attack] - Lunar Orbit into (Landing) Lunar Landing</LB>(Air) [MoveVertical-]+[Jump] - Spinkick";
		DialogueManager.AddNonLocDialogue("Axe Tutorial - Demo", text);
	}

	private void DisplayBowTutorial()
	{
		string text = "[Attack] (Hold) - Draw.  Release to fire. Requires Ammo</LB>(Air) [MoveVertical-]+[Jump] - Spinkick **kicking stuff restores ammo.";
		DialogueManager.AddNonLocDialogue("Bow Tutorial - Demo", text);
	}

	private void DisplayMageTutorial()
	{
		string text = "You are a master of magic.  No weapon, but you regenerate mana automatically.</LB>(Air) [MoveVertical-]+[Jump] - Spinkick";
		DialogueManager.AddNonLocDialogue("Mage Tutorial", text);
	}
}
