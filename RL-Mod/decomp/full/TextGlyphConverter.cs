using System;
using System.Collections.Generic;
using System.Text;
using Rewired;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TextGlyphConverter : MonoBehaviour, ITextChangedObj
{
	private struct TextGlyphEntry
	{
		public string ActionName;

		public string GamepadTextGlyph;

		public int KeyboardKeyCode;
	}

	private static bool m_loggedControllerMissingWarning = false;

	[SerializeField]
	private bool m_overrideControllerType;

	[SerializeField]
	[ConditionalHide("m_overrideControllerType", true)]
	private ControllerType m_forcedControllerType;

	private TMP_Text m_tmpText;

	private string m_preGlyphsAppliedText;

	private string m_postAppliedText;

	private List<TMP_SubMeshUI> m_tmpSubmeshList = new List<TMP_SubMeshUI>(4);

	private List<KeyboardButtonTextAligner> m_keyboardTextAlignerList = new List<KeyboardButtonTextAligner>(4);

	private bool m_isInitialized;

	private ControllerType m_currentlyUsedControllerType = ControllerType.Custom;

	private GamepadType m_currentlyUsedGamepadType;

	private bool m_onTextChangedEventAdded;

	private bool m_onControllerChangedEventAdded;

	private string m_stringReplacedText;

	private StringReplacementUtility m_stringReplacement;

	private List<TextGlyphEntry> m_assignedGamepadTextGlyphs = new List<TextGlyphEntry>(4);

	private List<TextGlyphEntry> m_assignedKeyboardKeyCodes = new List<TextGlyphEntry>(4);

	private static StringBuilder m_stringBuilderHelper = new StringBuilder();

	public bool IsInitialized => m_isInitialized;

	public bool OverrideControllerType
	{
		get
		{
			return m_overrideControllerType;
		}
		set
		{
			m_overrideControllerType = value;
		}
	}

	public ControllerType ForcedControllerType
	{
		get
		{
			return m_forcedControllerType;
		}
		set
		{
			m_forcedControllerType = value;
		}
	}

	public ControllerType CurrentControllerType
	{
		get
		{
			if (OverrideControllerType && ForcedControllerType != ControllerType.Custom)
			{
				return ForcedControllerType;
			}
			return m_currentlyUsedControllerType;
		}
	}

	public GamepadType ForcedGamepadType { get; set; }

	public GamepadType CurrentGamepadType
	{
		get
		{
			if (ForcedGamepadType != 0)
			{
				return ForcedGamepadType;
			}
			return m_currentlyUsedGamepadType;
		}
	}

	private void Awake()
	{
		m_tmpText = GetComponent<TMP_Text>();
		m_stringReplacement = GetComponent<StringReplacementUtility>();
	}

	private void Start()
	{
		Initialize();
	}

	private void OnEnable()
	{
		if (IsInitialized)
		{
			UpdateText(forceUpdate: false);
			if (!m_onTextChangedEventAdded)
			{
				GlobalTextChangedStaticListener.AddTextChangedListener(m_tmpText, this);
				m_onTextChangedEventAdded = true;
			}
			if (!OverrideControllerType && !m_onControllerChangedEventAdded)
			{
				GlobalTextChangedStaticListener.AddControllerChangedListener(this);
				m_onControllerChangedEventAdded = true;
			}
		}
	}

	private void OnDisable()
	{
		if (m_onTextChangedEventAdded)
		{
			GlobalTextChangedStaticListener.RemoveTextChangedListener(m_tmpText);
			m_onTextChangedEventAdded = false;
		}
		if (!OverrideControllerType && ReInput.isReady && m_onControllerChangedEventAdded)
		{
			GlobalTextChangedStaticListener.RemoveControllerChangedListener(this);
			m_onControllerChangedEventAdded = false;
		}
	}

	private void Initialize()
	{
		if ((bool)m_tmpText)
		{
			UpdateText(forceUpdate: true);
			if (!OverrideControllerType && !m_onControllerChangedEventAdded)
			{
				GlobalTextChangedStaticListener.AddControllerChangedListener(this);
				m_onControllerChangedEventAdded = true;
			}
			if (!m_onTextChangedEventAdded)
			{
				GlobalTextChangedStaticListener.AddTextChangedListener(m_tmpText, this);
				m_onTextChangedEventAdded = true;
			}
			m_isInitialized = true;
			return;
		}
		throw new Exception("TextGlyphConverter cannot find TextMeshPro text on GameObject.");
	}

	public void LastControllerChanged(Controller controller)
	{
		if ((m_currentlyUsedControllerType == ControllerType.Custom || (m_currentlyUsedControllerType != ControllerType.Joystick && controller.type == ControllerType.Joystick) || (m_currentlyUsedControllerType == ControllerType.Joystick && controller.type != ControllerType.Joystick) || (m_currentlyUsedControllerType == ControllerType.Joystick && CurrentGamepadType != RewiredOnStartupController.CurrentActiveGamepadType)) && IsInitialized)
		{
			UpdateText(forceUpdate: true);
		}
	}

	public void OnTextChanged()
	{
		UpdateText(forceUpdate: false);
	}

	public void UpdateText(bool forceUpdate)
	{
		if ((bool)m_stringReplacement && m_tmpText.text != m_stringReplacedText)
		{
			m_stringReplacement.OnTextChanged();
			m_stringReplacedText = m_tmpText.text;
		}
		if (m_onTextChangedEventAdded)
		{
			GlobalTextChangedStaticListener.RemoveTextChangedListener(m_tmpText);
			m_onTextChangedEventAdded = false;
		}
		if (!forceUpdate)
		{
			if (CurrentControllerType == ControllerType.Custom || (OverrideControllerType && m_currentlyUsedControllerType != ForcedControllerType) || (!OverrideControllerType && ((CurrentControllerType == ControllerType.Joystick && RewiredOnStartupController.CurrentActiveControllerType != ControllerType.Joystick) || (CurrentControllerType != ControllerType.Joystick && RewiredOnStartupController.CurrentActiveControllerType == ControllerType.Joystick))) || (m_currentlyUsedControllerType == ControllerType.Joystick && CurrentGamepadType != RewiredOnStartupController.CurrentActiveGamepadType))
			{
				forceUpdate = true;
			}
			if (!forceUpdate && m_postAppliedText != m_tmpText.text && m_preGlyphsAppliedText != m_tmpText.text)
			{
				forceUpdate = true;
			}
			if (!forceUpdate)
			{
				if (CurrentControllerType == ControllerType.Joystick)
				{
					foreach (TextGlyphEntry assignedGamepadTextGlyph in m_assignedGamepadTextGlyphs)
					{
						if (TextGlyphManager.GetGamepadTextGlyph(assignedGamepadTextGlyph.ActionName, CurrentGamepadType) != assignedGamepadTextGlyph.GamepadTextGlyph)
						{
							forceUpdate = true;
							break;
						}
					}
				}
				else
				{
					foreach (TextGlyphEntry assignedKeyboardKeyCode in m_assignedKeyboardKeyCodes)
					{
						if (TextGlyphManager.GetKeyboardTextGlyph(assignedKeyboardKeyCode.ActionName, out var _) != assignedKeyboardKeyCode.KeyboardKeyCode)
						{
							forceUpdate = true;
							break;
						}
					}
				}
			}
		}
		if (forceUpdate)
		{
			ApplyGlyphs();
			m_tmpText.GetComponentsInChildren(includeInactive: true, m_tmpSubmeshList);
			m_tmpText.GetComponentsInChildren(includeInactive: true, m_keyboardTextAlignerList);
			AlignGlyphs();
		}
		else
		{
			if (m_tmpText.text != m_postAppliedText)
			{
				m_tmpText.text = m_postAppliedText;
				m_tmpText.Rebuild(CanvasUpdate.PreRender);
			}
			AlignGlyphs();
		}
		if (!m_onTextChangedEventAdded)
		{
			GlobalTextChangedStaticListener.AddTextChangedListener(m_tmpText, this);
			m_onTextChangedEventAdded = true;
		}
	}

	private void AlignGlyphs()
	{
		AlignKeycodeInputText();
		CorrectSubmeshHeirarchy();
	}

	private void CorrectSubmeshHeirarchy()
	{
		foreach (TMP_SubMeshUI tmpSubmesh in m_tmpSubmeshList)
		{
			if ((bool)tmpSubmesh)
			{
				tmpSubmesh.transform.SetAsFirstSibling();
			}
		}
	}

	private void AlignKeycodeInputText()
	{
		foreach (KeyboardButtonTextAligner keyboardTextAligner in m_keyboardTextAlignerList)
		{
			keyboardTextAligner.AlignText();
		}
	}

	private void ApplyGlyphs()
	{
		string text = m_tmpText.text;
		if (m_postAppliedText != text && m_preGlyphsAppliedText != text)
		{
			m_preGlyphsAppliedText = text;
		}
		else
		{
			text = m_preGlyphsAppliedText;
		}
		if (RewiredOnStartupController.CurrentActiveControllerType == ControllerType.Joystick)
		{
			m_currentlyUsedControllerType = ControllerType.Joystick;
			m_currentlyUsedGamepadType = RewiredOnStartupController.CurrentActiveGamepadType;
		}
		else
		{
			m_currentlyUsedControllerType = ControllerType.Keyboard;
		}
		ConvertTextGlyphs_V2(text, CurrentControllerType, CurrentGamepadType);
		m_postAppliedText = m_tmpText.text;
	}

	private void ConvertTextGlyphs_V2(string textToConvert, ControllerType controllerType = ControllerType.Custom, GamepadType gamepadType = GamepadType.None)
	{
		if (IsInitialized)
		{
			ResetAllKeycodeText_V2();
		}
		if (string.IsNullOrEmpty(textToConvert))
		{
			m_assignedGamepadTextGlyphs.Clear();
			m_assignedKeyboardKeyCodes.Clear();
			return;
		}
		if (controllerType == ControllerType.Joystick && !Rewired_RL.IsGamepadConnected)
		{
			if (!m_loggedControllerMissingWarning)
			{
				m_loggedControllerMissingWarning = true;
				Debug.Log("<color=yellow>Could not display text glyph for ControllerType: " + controllerType.ToString() + ". Controller not plugged in. Defaulting to ControllerType.Custom</color>");
			}
			controllerType = ControllerType.Custom;
		}
		if (controllerType == ControllerType.Joystick)
		{
			m_assignedGamepadTextGlyphs.Clear();
		}
		else
		{
			m_assignedKeyboardKeyCodes.Clear();
		}
		int num = 0;
		int num2 = 0;
		int num3 = textToConvert.IndexOf('[');
		m_stringBuilderHelper.Clear();
		m_stringBuilderHelper.Append(textToConvert);
		while (num3 != -1)
		{
			int num4 = textToConvert.IndexOf(']', num3);
			string text = textToConvert.Substring(num3 + 1, num4 - num3 - 1);
			string oldValue = "[" + text + "]";
			if (TextGlyphLibrary.ContainsTextGlyph(text))
			{
				string textGlyphRichTextString = TextGlyphLibrary.GetTextGlyphRichTextString(text);
				m_stringBuilderHelper.Replace(oldValue, textGlyphRichTextString);
				num++;
				num3 = textToConvert.IndexOf('[', num3 + 1);
				continue;
			}
			string fullTMPTextGlyph = TextGlyphManager.GetFullTMPTextGlyph(text, controllerType == ControllerType.Joystick, gamepadType);
			if (!string.IsNullOrEmpty(fullTMPTextGlyph))
			{
				m_stringBuilderHelper.Replace(oldValue, fullTMPTextGlyph);
				TextGlyphEntry item = default(TextGlyphEntry);
				item.ActionName = text;
				if (controllerType == ControllerType.Joystick)
				{
					item.GamepadTextGlyph = TextGlyphManager.GetGamepadTextGlyph(text, gamepadType);
					m_assignedGamepadTextGlyphs.Add(item);
				}
				else
				{
					item.KeyboardKeyCode = TextGlyphManager.GetKeyboardTextGlyph(text, out var _);
					m_assignedKeyboardKeyCodes.Add(item);
				}
			}
			num++;
			if (controllerType != ControllerType.Joystick && AddKeyCodeTextToIcon_V2(text, num, num2))
			{
				num2++;
			}
			num3 = textToConvert.IndexOf('[', num3 + 1);
		}
		m_tmpText.text = m_stringBuilderHelper.ToString();
		m_tmpText.ForceMeshUpdate(ignoreActiveState: true);
	}

	private bool AddKeyCodeTextToIcon_V2(string actionName, int iconCount, int keyboardIconCount)
	{
		if (actionName == "Window_AllMovement_LStick" || actionName == "Window_AllMovement_RStick")
		{
			return false;
		}
		bool isKeyCode;
		int keyboardTextGlyph = TextGlyphManager.GetKeyboardTextGlyph(actionName, out isKeyCode);
		if (!isKeyCode)
		{
			return false;
		}
		string text = KeycodeConverter.GetKeycodeStringName((KeyboardKeyCode)keyboardTextGlyph, shift: false, capsLock: false, numLock: false).ToUpper();
		bool flag = false;
		if (!string.IsNullOrEmpty(text))
		{
			KeyboardButtonTextAligner[] componentsInChildren = m_tmpText.GetComponentsInChildren<KeyboardButtonTextAligner>();
			KeyboardButtonTextAligner keyboardButtonTextAligner = null;
			TMP_Text tMP_Text = null;
			if (componentsInChildren.Length > keyboardIconCount)
			{
				keyboardButtonTextAligner = componentsInChildren[keyboardIconCount];
				if (keyboardButtonTextAligner.BaseTMPObject == null)
				{
					keyboardButtonTextAligner.BaseTMPObject = keyboardButtonTextAligner.gameObject.GetComponent<TextMeshPro>();
					if (keyboardButtonTextAligner.BaseTMPObject == null)
					{
						keyboardButtonTextAligner.BaseTMPObject = keyboardButtonTextAligner.gameObject.GetComponent<TextMeshProUGUI>();
					}
				}
				tMP_Text = keyboardButtonTextAligner.BaseTMPObject;
				TextMeshPro textMeshPro = m_tmpText as TextMeshPro;
				if ((bool)textMeshPro)
				{
					(tMP_Text as TextMeshPro).renderer.sortingLayerID = textMeshPro.renderer.sortingLayerID;
					(tMP_Text as TextMeshPro).renderer.sortingOrder = 1;
				}
			}
			else
			{
				GameObject gameObject = new GameObject("TMP Input Text");
				gameObject.transform.SetParent(m_tmpText.transform, worldPositionStays: false);
				gameObject.layer = m_tmpText.gameObject.layer;
				TextMeshPro textMeshPro2 = m_tmpText as TextMeshPro;
				if ((bool)textMeshPro2)
				{
					tMP_Text = gameObject.AddComponent<TextMeshPro>();
					(tMP_Text as TextMeshPro).renderer.sortingLayerID = textMeshPro2.renderer.sortingLayerID;
					(tMP_Text as TextMeshPro).renderer.sortingOrder = 1;
				}
				else
				{
					tMP_Text = gameObject.AddComponent<TextMeshProUGUI>();
				}
				keyboardButtonTextAligner = gameObject.AddComponent<KeyboardButtonTextAligner>();
				keyboardButtonTextAligner.BaseTMPObject = tMP_Text;
				flag = true;
			}
			if (flag)
			{
				TextMeshProUGUI iconTextStyleTemplate = ControllerGlyphLibrary.IconTextStyleTemplate;
				if (iconTextStyleTemplate != null)
				{
					tMP_Text.font = iconTextStyleTemplate.font;
					tMP_Text.alignment = iconTextStyleTemplate.alignment;
					tMP_Text.color = iconTextStyleTemplate.color;
					tMP_Text.fontSharedMaterial = iconTextStyleTemplate.fontSharedMaterial;
					tMP_Text.SetMaterialDirty();
					tMP_Text.enableAutoSizing = true;
					tMP_Text.fontSizeMax = iconTextStyleTemplate.fontSizeMax;
					tMP_Text.fontSizeMin = iconTextStyleTemplate.fontSizeMin;
					tMP_Text.enableVertexGradient = iconTextStyleTemplate.enableVertexGradient;
					tMP_Text.colorGradientPreset = iconTextStyleTemplate.colorGradientPreset;
					tMP_Text.colorGradient = iconTextStyleTemplate.colorGradient;
				}
				else
				{
					tMP_Text.font = m_tmpText.font;
					tMP_Text.alignment = TextAlignmentOptions.Center;
					tMP_Text.color = m_tmpText.color;
					tMP_Text.fontSharedMaterial = m_tmpText.fontSharedMaterial;
					tMP_Text.SetMaterialDirty();
					tMP_Text.enableAutoSizing = true;
					tMP_Text.fontSizeMax = m_tmpText.fontSizeMax;
					tMP_Text.fontSizeMin = 1f;
					tMP_Text.enableVertexGradient = m_tmpText.enableVertexGradient;
					tMP_Text.colorGradientPreset = m_tmpText.colorGradientPreset;
					tMP_Text.colorGradient = m_tmpText.colorGradient;
				}
			}
			tMP_Text.SetText(text);
			tMP_Text.ForceMeshUpdate(ignoreActiveState: true);
			keyboardButtonTextAligner.SpriteIndex = iconCount;
			keyboardButtonTextAligner.ParentTMPObject = m_tmpText;
			return true;
		}
		return false;
	}

	private void ResetAllKeycodeText_V2()
	{
		foreach (KeyboardButtonTextAligner keyboardTextAligner in m_keyboardTextAlignerList)
		{
			TMP_Text baseTMPObject = keyboardTextAligner.BaseTMPObject;
			baseTMPObject.text = string.Empty;
			baseTMPObject.ForceMeshUpdate(ignoreActiveState: true);
		}
	}
}
