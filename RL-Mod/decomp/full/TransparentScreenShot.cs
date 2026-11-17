using System.Collections;
using System.IO;
using UnityEngine;

public class TransparentScreenShot : MonoBehaviour
{
	public enum Format
	{
		RAW,
		JPG,
		PNG,
		PPM
	}

	[Tooltip("Resolution of image width")]
	public int resWidth = 3840;

	[Tooltip("Resolution of the image height")]
	public int resHeight = 2160;

	[Tooltip("Image format")]
	public Format format = Format.PNG;

	[Tooltip("A folder will be created with this base name in your project root")]
	public string folderBaseName = "screenshots";

	[Tooltip("How many frames should be captured per second of game time")]
	public int frameRate = 60;

	[Tooltip("How many frames should be captured before quitting")]
	public int framesToCapture = 1;

	private string folderName = "";

	private GameObject whiteCamGameObject;

	private Camera whiteCam;

	private GameObject blackCamGameObject;

	private Camera blackCam;

	private Camera mainCam;

	private int videoFrame;

	private float originalTimescaleTime;

	private bool done;

	private Texture2D textureBlack;

	private Texture2D textureWhite;

	private Texture2D textureTransparentBackground;

	private bool takeHiResShot;

	private int counter;

	private void Awake()
	{
		mainCam = base.gameObject.GetComponent<Camera>();
		CreateBlackAndWhiteCameras();
		CacheAndInitialiseFields();
	}

	private string uniqueFilename(int width, int height)
	{
		if (folderName == null || folderName.Length == 0)
		{
			folderName = Application.dataPath;
			if (Application.isEditor)
			{
				string path = folderName + "/..";
				folderName = Path.GetFullPath(path);
			}
			folderName += "/screenshots";
			Directory.CreateDirectory(folderName);
			string searchPattern = $"screen_{width}x{height}*.{format.ToString().ToLower()}";
			counter = Directory.GetFiles(folderName, searchPattern, SearchOption.TopDirectoryOnly).Length;
		}
		string result = $"{folderName}/screen_{width}x{height}_{counter}.{format.ToString().ToLower()}";
		counter++;
		return result;
	}

	public void TakeHiResShot()
	{
		takeHiResShot = true;
	}

	private void LateUpdate()
	{
		takeHiResShot |= Input.GetKeyDown("k");
		if (takeHiResShot)
		{
			Debug.Log("CAPTURE");
			StartCoroutine(CaptureFrame());
		}
	}

	private IEnumerator CaptureFrame()
	{
		yield return new WaitForEndOfFrame();
		RenderCamToTexture(blackCam, textureBlack);
		RenderCamToTexture(whiteCam, textureWhite);
		CalculateOutputTexture();
		SavePng();
		videoFrame++;
		Debug.Log("Rendered frame " + videoFrame);
		takeHiResShot = false;
		StopCoroutine("CaptureFrame");
	}

	private void RenderCamToTexture(Camera cam, Texture2D tex)
	{
		cam.enabled = true;
		cam.Render();
		WriteScreenImageToTexture(tex);
		cam.enabled = false;
	}

	private void CreateBlackAndWhiteCameras()
	{
		whiteCamGameObject = new GameObject();
		whiteCamGameObject.name = "White Background Camera";
		whiteCam = whiteCamGameObject.AddComponent<Camera>();
		whiteCam.CopyFrom(mainCam);
		whiteCam.backgroundColor = Color.white;
		whiteCamGameObject.transform.SetParent(base.gameObject.transform, worldPositionStays: true);
		blackCamGameObject = new GameObject();
		blackCamGameObject.name = "Black Background Camera";
		blackCam = blackCamGameObject.AddComponent<Camera>();
		blackCam.CopyFrom(mainCam);
		blackCam.backgroundColor = Color.black;
		blackCamGameObject.transform.SetParent(base.gameObject.transform, worldPositionStays: true);
	}

	private void CreateNewFolderForScreenshots()
	{
		folderName = folderBaseName;
		int num = 1;
		while (Directory.Exists(folderName))
		{
			folderName = folderBaseName + num;
			num++;
		}
		Directory.CreateDirectory(folderName);
	}

	private void WriteScreenImageToTexture(Texture2D tex)
	{
		tex.ReadPixels(new Rect(0f, 0f, resWidth, resHeight), 0, 0);
		tex.Apply();
	}

	private void CalculateOutputTexture()
	{
		for (int i = 0; i < textureTransparentBackground.height; i++)
		{
			for (int j = 0; j < textureTransparentBackground.width; j++)
			{
				float num = textureWhite.GetPixel(j, i).r - textureBlack.GetPixel(j, i).r;
				num = 1f - num;
				Color color = ((num != 0f) ? (textureBlack.GetPixel(j, i) / num) : Color.clear);
				color.a = num;
				textureTransparentBackground.SetPixel(j, i, color);
			}
		}
	}

	private void SavePng()
	{
		Debug.Log("SAVING");
		string path = uniqueFilename(resWidth, resHeight);
		byte[] bytes = textureTransparentBackground.EncodeToPNG();
		File.WriteAllBytes(path, bytes);
	}

	private void CacheAndInitialiseFields()
	{
		originalTimescaleTime = Time.timeScale;
		resWidth = Screen.width;
		resHeight = Screen.height;
		textureBlack = new Texture2D(resWidth, resHeight, TextureFormat.RGB24, mipChain: false);
		textureWhite = new Texture2D(resWidth, resHeight, TextureFormat.RGB24, mipChain: false);
		textureTransparentBackground = new Texture2D(resWidth, resHeight, TextureFormat.ARGB32, mipChain: false);
	}
}
