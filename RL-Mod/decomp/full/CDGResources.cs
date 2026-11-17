using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;

public static class CDGResources
{
	public class CDGResources_AsyncLoader : MonoBehaviour
	{
		private int m_numBundlesLoaded;

		private bool m_loadFromMemory;

		public void LoadBundlesAsync(string[] bundleNames)
		{
			QualitySettings.asyncUploadTimeSlice = 8;
			QualitySettings.asyncUploadBufferSize = 64;
			QualitySettings.asyncUploadPersistentBuffer = true;
			StartCoroutine(LoadBundlesCoroutine(bundleNames));
		}

		private IEnumerator LoadBundlesCoroutine(string[] bundleNames)
		{
			Stopwatch sw = new Stopwatch();
			sw.Start();
			m_isLoading = true;
			string bundleRootPath = Path.Combine(Application.streamingAssetsPath, "AssetBundles");
			foreach (string path in bundleNames)
			{
				string bundleFilePath = Path.Combine(bundleRootPath, path);
				if (m_loadFromMemory)
				{
					byte[] fileBytes = null;
					Task task = Task.Run(delegate
					{
						fileBytes = File.ReadAllBytes(bundleFilePath);
					});
					while (!task.IsCompleted)
					{
						yield return null;
					}
					AssetBundle.LoadFromMemoryAsync(fileBytes).completed += OnBundleLoaded;
				}
				else
				{
					AssetBundle.LoadFromFileAsync(bundleFilePath).completed += OnBundleLoaded;
				}
			}
			while (m_numBundlesLoaded < bundleNames.Length)
			{
				yield return null;
			}
			sw.Stop();
			UnityEngine.Debug.Log("Async loading bundles into memory took: " + sw.Elapsed.TotalSeconds);
			m_isLoading = false;
			Object.Destroy(base.gameObject);
		}

		private void OnBundleLoaded(AsyncOperation op)
		{
			AssetBundle assetBundle = (op as AssetBundleCreateRequest).assetBundle;
			m_bundles[assetBundle.name] = assetBundle;
			m_numBundlesLoaded++;
		}
	}

	private static bool m_hasInitialized;

	private static bool m_isLoading;

	private static Dictionary<string, AssetBundle> m_bundles;

	public const string ROOT_DIRECTORY = "Assets/Content/";

	private const string BUNDLE_SUBDIRECTORY_NAME = "AssetBundles";

	public static bool IsLoading => m_isLoading;

	public static void Init(bool loadSynchronous)
	{
		if (m_hasInitialized)
		{
			return;
		}
		m_hasInitialized = true;
		UnityEngine.Debug.Log("CDGResources backend: Asset Bundles");
		string path = Path.Combine(Application.streamingAssetsPath, "AssetBundles");
		string[] allAssetBundles = AssetBundle.LoadFromFile(Path.Combine(path, "AssetBundles")).LoadAsset<AssetBundleManifest>("assetbundlemanifest").GetAllAssetBundles();
		m_bundles = new Dictionary<string, AssetBundle>(allAssetBundles.Length);
		if (loadSynchronous)
		{
			string[] array = allAssetBundles;
			foreach (string text in array)
			{
				m_bundles[text] = AssetBundle.LoadFromFile(Path.Combine(path, text));
			}
		}
		else
		{
			new GameObject("Async Loader").AddComponent<CDGResources_AsyncLoader>().LoadBundlesAsync(allAssetBundles);
		}
	}

	public static T Load<T>(string path, string bundleName = "", bool throwOnNull = true) where T : Object
	{
		string text = ((typeof(T) == typeof(GameObject)) ? ".prefab" : ".asset");
		string text2 = "Assets/Content/" + path + text;
		text2 = text2.ToLower();
		if (bundleName == "")
		{
			bundleName = ((text == ".asset") ? "scriptable_objects" : "prefabs");
		}
		T val = m_bundles[bundleName].LoadAsset<T>(text2);
		if (val == null && throwOnNull)
		{
			throw new FileNotFoundException("Could not locate asset: " + text2);
		}
		return val;
	}

	public static CDGAsyncLoadRequest<T> LoadAsync<T>(string path, string bundleName = "") where T : Object
	{
		string text = ((typeof(T) == typeof(GameObject)) ? ".prefab" : ".asset");
		string text2 = "Assets/Content/" + path + text;
		text2 = text2.ToLower();
		if (bundleName == "")
		{
			bundleName = ((text == ".asset") ? "scriptable_objects" : "prefabs");
		}
		AssetBundleRequest request = m_bundles[bundleName].LoadAssetAsync<T>(text2);
		return new CDGAsyncLoadRequest<T>(isAssetDatabase: false, null, request);
	}
}
