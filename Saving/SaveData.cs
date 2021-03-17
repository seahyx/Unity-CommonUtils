/**
 *	Common Utilities Library for developing in Unity.
 *	https://github.com/seahyx/Unity-CommonUtils
 *
 *	MIT License
 *
 *	Copyright (c) 2021 Seah Ying Xiang
 */

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CommonUtils.Save
{
	/// <summary>
	/// Contains all values for a save data.
	/// </summary>
	[Serializable]
	public class SaveData
	{
		#region Type Definitions

		/// <summary>
		/// Serializable date time for JSON.
		/// </summary>
		[Serializable]
		struct JsonDateTime
		{
			public long value;
			public static implicit operator DateTime(JsonDateTime jdt)
			{
				return DateTime.FromFileTimeUtc(jdt.value);
			}
			public static implicit operator JsonDateTime(DateTime dt)
			{
				JsonDateTime jdt = new JsonDateTime();
				jdt.value = dt.ToFileTimeUtc();
				return jdt;
			}
		}

		#endregion

		#region Member Declarations

		/// <summary>
		/// Example data.
		/// </summary>
		public string SaveStringJSON;

		/// <summary>
		/// Example data.
		/// </summary>
		public string SaveIntJSON;

		/// <summary>
		/// JSON-serializable field for datetime.
		/// </summary>
		[SerializeField]
		private JsonDateTime dateTime;

		#endregion

		public SaveData(
			string stringJSON,
			string intJSON,
			DateTime date)
		{
			SaveStringJSON = stringJSON;
			SaveIntJSON = intJSON;
			dateTime = date;
		}
	}
}
