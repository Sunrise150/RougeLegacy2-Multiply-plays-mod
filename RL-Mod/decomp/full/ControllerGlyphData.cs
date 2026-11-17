using System;
using System.Collections.Generic;
using Rewired;
using Rewired.Data.Mapping;
using UnityEngine;

[Serializable]
[CreateAssetMenu(menuName = "Custom/Rogue Legacy 2/Controller/ControllerGlyphData")]
public class ControllerGlyphData : ScriptableObject
{
	[Serializable]
	public class GlyphEntry
	{
		public int elementIdentifierId = -1;

		public string elementName = "";

		public string glyphNameFull = "";

		public string glyphNamePos = "";

		public string glyphNameNeg = "";

		public string GetGlyphName(AxisRange axisRange)
		{
			switch (axisRange)
			{
			case AxisRange.Full:
				return glyphNameFull;
			case AxisRange.Positive:
				if (!(glyphNamePos != ""))
				{
					return glyphNameFull;
				}
				return glyphNamePos;
			case AxisRange.Negative:
				if (!(glyphNameNeg != ""))
				{
					return glyphNameFull;
				}
				return glyphNameNeg;
			default:
				return "";
			}
		}
	}

	public const int LEFTSTICK_ALL_ELEMENT_ID = 23;

	public const int RIGHTSTICK_ALL_ELEMENT_ID = 24;

	public const int DPAD_ALL_ELEMENT_ID = 25;

	public GamepadType GamepadType;

	public string SpriteAssetName;

	public HardwareJoystickMap[] JoystickMaps;

	public TextAsset ElementIDTextAsset;

	public List<GlyphEntry> Glyphs = GenerateDefaultGlyphs();

	public string GetGlyphName(int elementIdentifierId, AxisRange axisRange)
	{
		if (Glyphs == null)
		{
			return null;
		}
		foreach (GlyphEntry glyph in Glyphs)
		{
			if (glyph != null && glyph.elementIdentifierId == elementIdentifierId)
			{
				return glyph.GetGlyphName(axisRange);
			}
		}
		return null;
	}

	private static List<GlyphEntry> GenerateDefaultGlyphs()
	{
		return new List<GlyphEntry>
		{
			new GlyphEntry
			{
				elementIdentifierId = 0,
				elementName = "Left Stick X",
				glyphNameFull = "X1_LeftStick_Glyph",
				glyphNameNeg = "X1_LeftStickLeft_Glyph",
				glyphNamePos = "X1_LeftStickRight_Glyph"
			},
			new GlyphEntry
			{
				elementIdentifierId = 1,
				elementName = "Left Stick Y",
				glyphNameFull = "X1_LeftStick_Glyph",
				glyphNameNeg = "X1_LeftStickDown_Glyph",
				glyphNamePos = "X1_LeftStickUp_Glyph"
			},
			new GlyphEntry
			{
				elementIdentifierId = 2,
				elementName = "Right Stick X",
				glyphNameFull = "X1_RightStick_Glyph",
				glyphNameNeg = "X1_RightStickLeft_Glyph",
				glyphNamePos = "X1_RightStickRight_Glyph"
			},
			new GlyphEntry
			{
				elementIdentifierId = 3,
				elementName = "Right Stick Y",
				glyphNameFull = "X1_RightStick_Glyph",
				glyphNameNeg = "X1_RightStickDown_Glyph",
				glyphNamePos = "X1_RightStickUp_Glyph"
			},
			new GlyphEntry
			{
				elementIdentifierId = 4,
				elementName = "Button A (Bottom Row 1)",
				glyphNameFull = "X1_ButtonA_Glyph"
			},
			new GlyphEntry
			{
				elementIdentifierId = 5,
				elementName = "Button B (Bottom Row 2)",
				glyphNameFull = "X1_ButtonB_Glyph"
			},
			new GlyphEntry
			{
				elementIdentifierId = 7,
				elementName = "Button X (Top Row 1)",
				glyphNameFull = "X1_ButtonX_Glyph"
			},
			new GlyphEntry
			{
				elementIdentifierId = 8,
				elementName = "Button Y (Top Row 2)",
				glyphNameFull = "X1_ButtonY_Glyph"
			},
			new GlyphEntry
			{
				elementIdentifierId = 10,
				elementName = "Left Shoulder Digital",
				glyphNameFull = "X1_LeftShoulderDigital_Glyph"
			},
			new GlyphEntry
			{
				elementIdentifierId = 11,
				elementName = "Left Shoulder Analog",
				glyphNameFull = "X1_LeftShoulderAnalog_Glyph"
			},
			new GlyphEntry
			{
				elementIdentifierId = 12,
				elementName = "Right Shoulder Digital",
				glyphNameFull = "X1_RightShoulderDigital_Glyph"
			},
			new GlyphEntry
			{
				elementIdentifierId = 13,
				elementName = "Right Shoulder Analog",
				glyphNameFull = "X1_RightShoulderAnalog_Glyph"
			},
			new GlyphEntry
			{
				elementIdentifierId = 14,
				elementName = "Back Button (Centre Row 1)",
				glyphNameFull = "X1_ButtonBack_Glyph"
			},
			new GlyphEntry
			{
				elementIdentifierId = 15,
				elementName = "Start Button (Centre Row 2)",
				glyphNameFull = "X1_ButtonStart_Glyph"
			},
			new GlyphEntry
			{
				elementIdentifierId = 16,
				elementName = "Guide Button (Centre Row 3)",
				glyphNameFull = "X1_ButtonGuide_Glyph"
			},
			new GlyphEntry
			{
				elementIdentifierId = 17,
				elementName = "Left Stick Button",
				glyphNameFull = "X1_LeftStickIn_Glyph"
			},
			new GlyphEntry
			{
				elementIdentifierId = 18,
				elementName = "Right Stick Button",
				glyphNameFull = "X1_RightStickIn_Glyph"
			},
			new GlyphEntry
			{
				elementIdentifierId = 19,
				elementName = "Dpad Up",
				glyphNameFull = "X1_DpadUp_Glyph"
			},
			new GlyphEntry
			{
				elementIdentifierId = 20,
				elementName = "Dpad Right",
				glyphNameFull = "X1_DpadRight_Glyph"
			},
			new GlyphEntry
			{
				elementIdentifierId = 21,
				elementName = "Dpad Down",
				glyphNameFull = "X1_DpadDown_Glyph"
			},
			new GlyphEntry
			{
				elementIdentifierId = 22,
				elementName = "Dpad Left",
				glyphNameFull = "X1_DpadLeft_Glyph"
			},
			new GlyphEntry
			{
				elementIdentifierId = 23,
				elementName = "Left Stick All",
				glyphNameFull = "X1_LeftStick_Glyph"
			},
			new GlyphEntry
			{
				elementIdentifierId = 24,
				elementName = "Right Stick All",
				glyphNameFull = "X1_RightStick_Glyph"
			},
			new GlyphEntry
			{
				elementIdentifierId = 25,
				elementName = "Dpad All",
				glyphNameFull = "X1_DpadAll_Glyph"
			}
		};
	}
}
