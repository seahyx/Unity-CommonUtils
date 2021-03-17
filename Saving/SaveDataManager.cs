/**
 *	Common Utilities Library for developing in Unity.
 *	https://github.com/seahyx/Unity-CommonUtils
 *
 *	MIT License
 *
 *	Copyright (c) 2021 Seah Ying Xiang
 */

using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace CommonUtils.Save
{

	/// <summary>
	/// Save data manager.
	/// </summary>
	public class SaveDataManager : MonoBehaviour
	{
		#region Member Declarations

		/// <summary>
		/// Save file name.
		/// </summary>
		private const string SAVE_NAME = "save";

		/// <summary>
		/// Path of the save file.
		/// </summary>
		private string saveFilePath { get => Application.persistentDataPath + $"/{SAVE_NAME}.json"; }

		#endregion

		#region SaveData Functions

		/// <summary>
		/// Saves data to a persistent json file.
		/// </summary>
		public void SaveGame(SaveData saveData)
		{
			string json = JsonUtility.ToJson(saveData);
			File.WriteAllText(saveFilePath, json);
		}

		/// <summary>
		/// Loads the last save if available.
		/// </summary>
		/// <returns>Last saved data, or null if there is no file.</returns>
		public SaveData LoadSave()
		{
			if (File.Exists(saveFilePath))
			{
				Debug.Log($"[{name}] Last save file exists... loading from save.");
				return JsonUtility.FromJson<SaveData>(File.ReadAllText(saveFilePath));
			}
			else
			{
				Debug.Log($"[{name}] Last save does not exist.");
				return null;
			}
		}

		#endregion
	}
}
