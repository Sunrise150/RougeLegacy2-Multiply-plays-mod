using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class FileShortcutEntry
{
	public bool TitleModifiable;

	public string ShortcutTitle = "New Shortcut";

	public string Notes;

	public List<UnityEngine.Object> ObjectList = new List<UnityEngine.Object>();
}
