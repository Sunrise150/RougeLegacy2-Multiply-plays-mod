using System;

public class LineageHeirChangedEventArgs : EventArgs
{
	public CharacterData CharacterData { get; private set; }

	public bool ClassLocked { get; private set; }

	public bool SpellLocked { get; private set; }

	public LineageHeirChangedEventArgs(CharacterData charData, bool classLocked, bool spellLocked)
	{
		Initialize(charData, classLocked, spellLocked);
	}

	public void Initialize(CharacterData charData, bool classLocked, bool spellLocked)
	{
		CharacterData = charData;
		ClassLocked = classLocked;
		SpellLocked = spellLocked;
	}
}
