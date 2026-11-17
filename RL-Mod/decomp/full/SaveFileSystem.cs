using System.Collections.Generic;
using System.IO;
using UnityEngine;

public static class SaveFileSystem
{
	public struct FileUpdateCommand
	{
		public int Profile;

		public string BlobName;

		public byte[] Bytes;

		public FileUpdateCommand(int profile, string filename, byte[] bytes)
		{
			Profile = profile;
			BlobName = filename;
			Bytes = bytes;
		}
	}

	public class SaveBatch
	{
		private bool m_didAnyWork;

		private bool m_batchEnded;

		public int Profile;

		public List<FileUpdateCommand> CommandList;

		public bool IsAvailable { get; private set; }

		public int RefCount { get; private set; }

		public SaveBatch()
		{
			m_batchEnded = false;
			m_didAnyWork = false;
			Profile = -1;
			RefCount = 0;
			CommandList = null;
			IsAvailable = true;
		}

		public void Initialize(int profile)
		{
			m_batchEnded = false;
			m_didAnyWork = false;
			Profile = profile;
			RefCount = 0;
			if (CommandList != null)
			{
				CommandList.Clear();
			}
			IsAvailable = false;
			m_totalBatchRefCount++;
		}

		public void End()
		{
			m_batchEnded = true;
			m_totalBatchRefCount--;
			if (RefCount == 0)
			{
				if (m_didAnyWork)
				{
					SubmitBatchUpdates(this);
				}
				IsAvailable = true;
			}
		}

		public void IncrementRefCount()
		{
			RefCount++;
			m_totalBatchRefCount++;
			m_didAnyWork = true;
		}

		public void DecrementRefCount()
		{
			RefCount--;
			m_totalBatchRefCount--;
			if (m_batchEnded && RefCount == 0)
			{
				if (m_didAnyWork)
				{
					SubmitBatchUpdates(this);
				}
				IsAvailable = true;
			}
		}
	}

	private interface ISaveFileSystem
	{
		string PersistentDataPath { get; }

		string BackupFolderName { get; }

		void MountSaveDirectory();

		bool DirectoryExists(string path);

		void CreateDirectory(string path);

		List<string> GetBackupFilesInDirectory(string directoryPath);

		bool FileExists(string path);

		byte[] ReadAllBytes(string path);

		void WriteAllBytes(SaveBatch batch, string path, byte[] bytes);

		void DeleteFile(SaveBatch batch, string path);

		void SubmitBatchUpdates(SaveBatch batch);
	}

	private class PCSaveFileSystem : ISaveFileSystem
	{
		public string PersistentDataPath => Application.persistentDataPath;

		public string BackupFolderName => "Backups";

		public void MountSaveDirectory()
		{
		}

		public void CreateDirectory(string path)
		{
			Directory.CreateDirectory(path);
		}

		public bool DirectoryExists(string path)
		{
			return Directory.Exists(path);
		}

		public List<string> GetBackupFilesInDirectory(string path)
		{
			return new List<string>(Directory.GetFiles(path, "*.rc2dat"));
		}

		public bool FileExists(string path)
		{
			return File.Exists(path);
		}

		public byte[] ReadAllBytes(string path)
		{
			return File.ReadAllBytes(path);
		}

		public void WriteAllBytes(SaveBatch batch, string path, byte[] bytes)
		{
			File.WriteAllBytes(path, bytes);
		}

		public void DeleteFile(SaveBatch batch, string path)
		{
			File.Delete(path);
		}

		public void SubmitBatchUpdates(SaveBatch batch)
		{
		}
	}

	public const string SAVE_FILE_EXT = ".rc2dat";

	private static ISaveFileSystem m_saveFileSystem;

	private static List<SaveBatch> m_saveBatchPool;

	private static int m_totalBatchRefCount;

	public static string PersistentDataPath => m_saveFileSystem.PersistentDataPath;

	public static string BackupFolderName => m_saveFileSystem.BackupFolderName;

	public static bool IsSaving => m_totalBatchRefCount > 0;

	public static void Initialize()
	{
		m_saveFileSystem = new PCSaveFileSystem();
		m_saveBatchPool = new List<SaveBatch>(2)
		{
			new SaveBatch(),
			new SaveBatch()
		};
	}

	public static void MountSaveDirectory()
	{
		m_saveFileSystem.MountSaveDirectory();
	}

	public static bool DirectoryExists(string path)
	{
		return m_saveFileSystem.DirectoryExists(path);
	}

	public static void CreateDirectory(string path)
	{
		m_saveFileSystem.CreateDirectory(path);
	}

	public static List<string> GetBackupFilesInDirectory(string path)
	{
		return m_saveFileSystem.GetBackupFilesInDirectory(path);
	}

	public static bool FileExists(string path)
	{
		return m_saveFileSystem.FileExists(path);
	}

	public static byte[] ReadAllBytes(string path)
	{
		return m_saveFileSystem.ReadAllBytes(path);
	}

	public static SaveBatch BeginSaveBatch(int profile)
	{
		foreach (SaveBatch item in m_saveBatchPool)
		{
			if (item.IsAvailable)
			{
				item.Initialize(profile);
				return item;
			}
		}
		SaveBatch saveBatch = new SaveBatch();
		m_saveBatchPool.Add(saveBatch);
		saveBatch.Initialize(profile);
		return saveBatch;
	}

	private static void SubmitBatchUpdates(SaveBatch batch)
	{
		m_saveFileSystem.SubmitBatchUpdates(batch);
	}

	public static void WriteAllBytes(SaveBatch batch, string path, byte[] bytes)
	{
		batch.IncrementRefCount();
		m_saveFileSystem.WriteAllBytes(batch, path, bytes);
		batch.DecrementRefCount();
	}

	public static void DeleteFile(SaveBatch batch, string path)
	{
		batch.IncrementRefCount();
		m_saveFileSystem.DeleteFile(batch, path);
		batch.DecrementRefCount();
	}
}
