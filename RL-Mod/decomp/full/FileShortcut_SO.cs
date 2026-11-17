using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Custom/Rogue Legacy 2/File Shortcut SO")]
public class FileShortcut_SO : ScriptableObject
{
	public List<FileShortcutEntry> FileShortcutList = new List<FileShortcutEntry>();
}
