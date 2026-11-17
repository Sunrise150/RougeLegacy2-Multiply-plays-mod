using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using SceneManagement_RL;
using TMPro;
using Unity.Cloud.UserReporting;
using Unity.Cloud.UserReporting.Client;
using Unity.Cloud.UserReporting.Plugin;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UserReportingScript_TMP : MonoBehaviour
{
	[Tooltip("The username input on the user report form.")]
	public TMP_InputField UserNameInput;

	[Tooltip("The email input on the user report form.")]
	public TMP_InputField EmailInput;

	[Tooltip("The description input on the user report form.")]
	public TMP_InputField DescriptionInput;

	[Tooltip("The UI shown when there's an error.")]
	public Canvas ErrorPopup;

	private bool isCreatingUserReport;

	[Tooltip("A value indicating whether the hotkey is enabled (Left Alt + Left Shift + B).")]
	public bool IsHotkeyEnabled;

	[Tooltip("A value indicating whether the prefab is in silent mode. Silent mode does not show the user report form.")]
	public bool IsInSilentMode;

	[Tooltip("A value indicating whether the user report client reports metrics about itself.")]
	public bool IsSelfReporting;

	private bool isShowingError;

	private bool isSubmitting;

	[Tooltip("The display text for the progress text.")]
	public TMP_Text ProgressText;

	[Tooltip("A value indicating whether the user report client send events to analytics.")]
	public bool SendEventsToAnalytics;

	[Tooltip("The UI shown while submitting.")]
	public Canvas SubmittingPopup;

	public Button SubmissionCompleteButton;

	[Tooltip("The summary input on the user report form.")]
	public TMP_InputField SummaryInput;

	public Toggle BugReportToggle;

	public Toggle FeatureToggle;

	public Toggle ScreenshotToggle;

	public Toggle SaveFileToggle;

	[Tooltip("The thumbnail viewer on the user report form.")]
	public Image ThumbnailViewer;

	private UnityUserReportingUpdater unityUserReportingUpdater;

	[Tooltip("The user report button used to create a user report.")]
	public Button UserReportButton;

	[Tooltip("The UI for the user report form. Shown after a user report is created.")]
	public Canvas UserReportForm;

	[Tooltip("The User Reporting platform. Different platforms have different features but may require certain Unity versions or target platforms. The Async platform adds async screenshotting and report creation, but requires Unity 2018.3 and above, the package manager version of Unity User Reporting, and a target platform that supports asynchronous GPU readback such as DirectX.")]
	public UserReportingPlatformType UserReportingPlatform;

	[Tooltip("The event raised when a user report is submitting.")]
	public UnityEvent UserReportSubmitting;

	private UserReportWindowController m_windowController;

	private Coroutine m_errorMessageWaitCoroutine;

	public UserReport CurrentUserReport { get; private set; }

	public UserReportingState State
	{
		get
		{
			if (CurrentUserReport != null)
			{
				if (IsInSilentMode)
				{
					return UserReportingState.Idle;
				}
				if (isSubmitting)
				{
					return UserReportingState.SubmittingForm;
				}
				return UserReportingState.ShowingForm;
			}
			if (isCreatingUserReport)
			{
				return UserReportingState.CreatingUserReport;
			}
			return UserReportingState.Idle;
		}
	}

	public bool IsInitialized { get; private set; }

	public UserReportingScript_TMP()
	{
		UserReportSubmitting = new UnityEvent();
		unityUserReportingUpdater = new UnityUserReportingUpdater();
	}

	public void CancelUserReport()
	{
		if (UserNameInput != null)
		{
			SaveManager.ConfigData.UserReportName = UserNameInput.text;
		}
		if (EmailInput != null)
		{
			SaveManager.ConfigData.UserReportEmail = EmailInput.text;
			SaveManager.SaveConfigFile();
		}
		CurrentUserReport = null;
		isSubmitting = false;
		isCreatingUserReport = false;
		isShowingError = false;
		ClearForm();
	}

	private IEnumerator ClearError()
	{
		yield return new WaitForSeconds(5f);
		isShowingError = false;
	}

	private void ClearForm()
	{
		UnityUserReporting.CurrentClient.ClearScreenshots();
		SummaryInput.text = null;
		DescriptionInput.text = null;
		UserNameInput.text = null;
		EmailInput.text = null;
	}

	public void CreateUserReport()
	{
		if (isCreatingUserReport)
		{
			return;
		}
		SubmissionCompleteButton.gameObject.SetActive(value: false);
		ProgressText.gameObject.SetActive(value: true);
		isCreatingUserReport = true;
		UnityUserReporting.CurrentClient.TakeScreenshot(1280, 720, delegate
		{
		});
		UnityUserReporting.CurrentClient.CreateUserReport(delegate(UserReport br)
		{
			if (string.IsNullOrEmpty(br.ProjectIdentifier))
			{
				Debug.LogWarning("The user report's project identifier is not set. Please setup cloud services using the Services tab or manually specify a project identifier when calling UnityUserReporting.Configure().");
			}
			if (UserNameInput != null)
			{
				UserNameInput.text = SaveManager.ConfigData.UserReportName;
			}
			if (EmailInput != null)
			{
				EmailInput.text = SaveManager.ConfigData.UserReportEmail;
			}
			string arg = "Unknown";
			string arg2 = "0.0";
			foreach (UserReportNamedValue deviceMetadatum in br.DeviceMetadata)
			{
				if (deviceMetadatum.Name == "Platform")
				{
					arg = deviceMetadatum.Value;
				}
				if (deviceMetadatum.Name == "Version")
				{
					arg2 = deviceMetadatum.Value;
				}
			}
			br.Dimensions.Add(new UserReportNamedValue("Platform.Version", $"{arg}.{arg2}"));
			CurrentUserReport = br;
			isCreatingUserReport = false;
			SetThumbnail(br);
			if (IsInSilentMode)
			{
				SubmitUserReport();
			}
		});
	}

	private UserReportingClientConfiguration GetConfiguration()
	{
		return new UserReportingClientConfiguration(0, MetricsGatheringMode.Disabled, 0, 0, 2);
	}

	public bool IsSubmitting()
	{
		return isSubmitting;
	}

	private void SetThumbnail(UserReport userReport)
	{
		if (userReport != null && ThumbnailViewer != null)
		{
			byte[] data = Convert.FromBase64String(userReport.Thumbnail.DataBase64);
			Texture2D texture2D = new Texture2D(1, 1);
			texture2D.LoadImage(data);
			ThumbnailViewer.sprite = Sprite.Create(texture2D, new Rect(0f, 0f, texture2D.width, texture2D.height), new Vector2(0.5f, 0.5f));
			ThumbnailViewer.preserveAspect = true;
		}
	}

	private void Start()
	{
		bool flag = false;
		if (UserReportingPlatform == UserReportingPlatformType.Async)
		{
			Type type = Assembly.GetExecutingAssembly().GetType("Unity.Cloud.UserReporting.Plugin.Version2018_3.AsyncUnityUserReportingPlatform");
			if (type != null && Activator.CreateInstance(type) is IUserReportingPlatform platform)
			{
				UnityUserReporting.Configure(platform, GetConfiguration());
				flag = true;
			}
		}
		if (!flag)
		{
			UnityUserReporting.Configure(GetConfiguration());
		}
		string endpoint = $"https://userreporting.cloud.unity3d.com/api/userreporting/projects/{UnityUserReporting.CurrentClient.ProjectIdentifier}/ping";
		UnityUserReporting.CurrentClient.Platform.Post(endpoint, "application/json", Encoding.UTF8.GetBytes("\"Ping\""), delegate
		{
		}, delegate
		{
		});
		IsInitialized = true;
	}

	public void SubmitUserReport()
	{
		if (isSubmitting || CurrentUserReport == null)
		{
			return;
		}
		isSubmitting = true;
		if (SummaryInput != null)
		{
			CurrentUserReport.Summary = SummaryInput.text;
		}
		UserReportNamedValue item = default(UserReportNamedValue);
		item.Name = "Game Version";
		item.Value = System_EV.GetVersionString();
		CurrentUserReport.Fields.Add(item);
		if (UserNameInput != null)
		{
			UserReportNamedValue item2 = default(UserReportNamedValue);
			item2.Name = "User Name";
			item2.Value = UserNameInput.text;
			CurrentUserReport.Fields.Add(item2);
			SaveManager.ConfigData.UserReportName = UserNameInput.text;
		}
		if (EmailInput != null)
		{
			UserReportNamedValue item3 = default(UserReportNamedValue);
			item3.Name = "E-mail";
			item3.Value = EmailInput.text;
			CurrentUserReport.Fields.Add(item3);
			SaveManager.ConfigData.UserReportEmail = EmailInput.text;
			SaveManager.SaveConfigFile();
		}
		if (BugReportToggle != null && FeatureToggle != null)
		{
			if (BugReportToggle.isOn)
			{
				CurrentUserReport.Dimensions.Add(new UserReportNamedValue("ReportType", "Bug Report"));
				CurrentUserReport.Fields.Add(new UserReportNamedValue("ReportType", "Bug Report"));
			}
			else
			{
				CurrentUserReport.Dimensions.Add(new UserReportNamedValue("ReportType", "Feature Suggestion"));
				CurrentUserReport.Fields.Add(new UserReportNamedValue("ReportType", "Feature Suggestion"));
			}
		}
		if (ScreenshotToggle != null && !ScreenshotToggle.isOn)
		{
			UnityUserReporting.CurrentClient.ClearScreenshots();
			CurrentUserReport.Screenshots.Clear();
		}
		CurrentUserReport.Thumbnail = default(UserReportScreenshot);
		if (SaveFileToggle != null && SaveFileToggle.isOn)
		{
			BinaryFormatter binaryFormatter = new BinaryFormatter();
			using (MemoryStream memoryStream = new MemoryStream())
			{
				binaryFormatter.Serialize(memoryStream, SaveManager.PlayerSaveData);
				CurrentUserReport.Attachments.Add(new UserReportAttachment("PlayerSave", "Player.rc2dat", "binary", memoryStream.ToArray()));
			}
			using (MemoryStream memoryStream2 = new MemoryStream())
			{
				binaryFormatter.Serialize(memoryStream2, SaveManager.EquipmentSaveData);
				CurrentUserReport.Attachments.Add(new UserReportAttachment("EquipmentSave", "Equipment.rc2dat", "binary", memoryStream2.ToArray()));
			}
			using (MemoryStream memoryStream3 = new MemoryStream())
			{
				binaryFormatter.Serialize(memoryStream3, SaveManager.StageSaveData);
				CurrentUserReport.Attachments.Add(new UserReportAttachment("StageSave", "Stage.rc2dat", "binary", memoryStream3.ToArray()));
			}
			using (MemoryStream memoryStream4 = new MemoryStream())
			{
				binaryFormatter.Serialize(memoryStream4, SaveManager.ModeSaveData);
				CurrentUserReport.Attachments.Add(new UserReportAttachment("GameModeSave", "GameMode.rc2dat", "binary", memoryStream4.ToArray()));
			}
			using MemoryStream memoryStream5 = new MemoryStream();
			binaryFormatter.Serialize(memoryStream5, SaveManager.LineageSaveData);
			CurrentUserReport.Attachments.Add(new UserReportAttachment("LineageSave", "Lineage.rc2dat", "binary", memoryStream5.ToArray()));
		}
		if (DescriptionInput != null)
		{
			UserReportNamedValue item4 = default(UserReportNamedValue);
			item4.Name = "Description";
			item4.Value = DescriptionInput.text;
			CurrentUserReport.Fields.Add(item4);
		}
		UserReportNamedValue item5 = default(UserReportNamedValue);
		item5.Name = "Current Scene";
		item5.Value = SceneLoader_RL.CurrentScene;
		CurrentUserReport.Fields.Add(item5);
		UserReportNamedValue item6 = default(UserReportNamedValue);
		item6.Name = "Previous Scene";
		item6.Value = SceneLoader_RL.PreviousScene;
		CurrentUserReport.Fields.Add(item6);
		if (SceneManager.GetActiveScene().name == "World")
		{
			UserReportNamedValue item7 = default(UserReportNamedValue);
			item7.Name = "Current Seed";
			item7.Value = RNGSeedManager.GetSeedAsHex(RNGSeedManager.GetCurrentSeed("World")) + "-" + BurdenManager.GetBurdenLevel(BurdenType.RoomCount);
			CurrentUserReport.Fields.Add(item7);
			BaseRoom currentPlayerRoom = PlayerManager.GetCurrentPlayerRoom();
			UserReportNamedValue item8 = default(UserReportNamedValue);
			item8.Name = "Biome";
			item8.Value = currentPlayerRoom.BiomeType.ToString();
			CurrentUserReport.Fields.Add(item8);
			UserReportNamedValue item9 = default(UserReportNamedValue);
			item9.Name = "Biome Controller Index";
			item9.Value = currentPlayerRoom.BiomeControllerIndex.ToString();
			CurrentUserReport.Fields.Add(item9);
			string[] array = null;
			if (currentPlayerRoom is Room)
			{
				string text = (currentPlayerRoom as Room).GridPointManager.RoomMetaData.ID.ToString();
				if ((currentPlayerRoom as Room).GridPointManager.IsRoomMirrored)
				{
					text += "M";
				}
				array = new string[1] { text };
			}
			else
			{
				MergeRoom mergeRoom = currentPlayerRoom as MergeRoom;
				array = new string[mergeRoom.StandaloneGridPointManagers.Length];
				for (int i = 0; i < mergeRoom.StandaloneGridPointManagers.Length; i++)
				{
					array[i] = mergeRoom.StandaloneGridPointManagers[i].RoomMetaData.ID.ToString();
					if (mergeRoom.StandaloneGridPointManagers[i].IsRoomMirrored)
					{
						array[i] += "M";
					}
				}
			}
			string text2 = string.Empty;
			for (int j = 0; j < array.Length; j++)
			{
				text2 += array[j].ToString();
				if (j < array.Length - 1)
				{
					text2 += ", ";
				}
			}
			UserReportNamedValue item10 = default(UserReportNamedValue);
			item10.Name = "Room ID(s)";
			item10.Value = text2;
			CurrentUserReport.Fields.Add(item10);
		}
		if (RNGSeedManager.PreviousMasterSeedOverrides != null)
		{
			UserReportNamedValue item11 = default(UserReportNamedValue);
			item11.Name = "Previous Seeds";
			string text3 = string.Empty;
			for (int num = RNGSeedManager.PreviousMasterSeedOverrides.Count - 1; num >= 0; num--)
			{
				text3 = text3 + RNGSeedManager.GetSeedAsHex(RNGSeedManager.PreviousMasterSeedOverrides.ElementAt(num)) + "-" + BurdenManager.GetBurdenLevel(BurdenType.RoomCount);
				if (num > 0)
				{
					text3 += ", ";
				}
			}
			item11.Value = text3;
			CurrentUserReport.Fields.Add(item11);
		}
		ClearForm();
		RaiseUserReportSubmitting();
		UnityUserReporting.CurrentClient.SendUserReport(CurrentUserReport, delegate(float uploadProgress, float downloadProgress)
		{
			if (ProgressText != null)
			{
				string text4 = $"{uploadProgress:P}";
				ProgressText.text = text4;
			}
		}, delegate(bool success, UserReport br2)
		{
			if (!success)
			{
				if (m_errorMessageWaitCoroutine != null)
				{
					StopCoroutine(m_errorMessageWaitCoroutine);
				}
				m_errorMessageWaitCoroutine = StartCoroutine(ErrorMessageTimeout());
				isShowingError = true;
			}
			else
			{
				SubmissionCompleteButton.gameObject.SetActive(value: true);
				ProgressText.text = "COMPLETE";
			}
		});
	}

	private void OnDisable()
	{
		if (m_errorMessageWaitCoroutine != null)
		{
			StopCoroutine(m_errorMessageWaitCoroutine);
			m_errorMessageWaitCoroutine = null;
		}
	}

	private IEnumerator ErrorMessageTimeout()
	{
		float timeStart = Time.unscaledTime;
		while (Time.unscaledTime - timeStart < 10f)
		{
			yield return null;
		}
		if (m_windowController == null)
		{
			m_windowController = GetComponentInParent<UserReportWindowController>();
		}
		m_errorMessageWaitCoroutine = null;
		m_windowController.CompleteUserReport();
	}

	private void Update()
	{
		UnityUserReporting.CurrentClient.IsSelfReporting = IsSelfReporting;
		UnityUserReporting.CurrentClient.SendEventsToAnalytics = SendEventsToAnalytics;
		if (UserReportButton != null)
		{
			UserReportButton.interactable = State == UserReportingState.Idle;
		}
		if (UserReportForm != null)
		{
			UserReportForm.enabled = State == UserReportingState.ShowingForm;
		}
		if (SubmittingPopup != null)
		{
			SubmittingPopup.enabled = State == UserReportingState.SubmittingForm;
		}
		if (ErrorPopup != null)
		{
			ErrorPopup.enabled = isShowingError;
		}
		unityUserReportingUpdater.Reset();
		StartCoroutine(unityUserReportingUpdater);
	}

	protected virtual void RaiseUserReportSubmitting()
	{
		if (UserReportSubmitting != null)
		{
			UserReportSubmitting.Invoke();
		}
	}
}
