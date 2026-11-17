using UnityEngine;

public struct CDGAsyncLoadRequest<T> where T : Object
{
	private bool m_isAssetDatabase;

	private T m_asset;

	private AssetBundleRequest m_bundleRequest;

	public T Asset
	{
		get
		{
			if (m_isAssetDatabase)
			{
				return m_asset;
			}
			return m_bundleRequest.asset as T;
		}
	}

	public bool IsDone
	{
		get
		{
			if (m_isAssetDatabase)
			{
				return true;
			}
			return m_bundleRequest.isDone;
		}
	}

	public CDGAsyncLoadRequest(bool isAssetDatabase, T asset, AssetBundleRequest request)
	{
		m_isAssetDatabase = isAssetDatabase;
		m_asset = asset;
		m_bundleRequest = request;
	}
}
