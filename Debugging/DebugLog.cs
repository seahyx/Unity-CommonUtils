/**
 *	Common Utilities Library for developing in Unity.
 *	https://github.com/seahyx/Unity-CommonUtils
 *
 *	MIT License
 *
 *	Copyright (c) 2021 Seah Ying Xiang
 */

using TMPro;
using UnityEngine;

namespace CommonUtils.Debugging
{
	/// <summary>
	/// Tool to display the console log into a TMP component.
	/// </summary>
	public class DebugLog : MonoBehaviour
	{

		/// <summary>
		/// Log string.
		/// </summary>
		static string myLog = "";

		/// <summary>
		/// Stack trace.
		/// </summary>
		private string stack;

		/// <summary>
		/// Output string.
		/// </summary>
		private string output;

		private TextMeshPro tmp;

		/// <summary>
		/// Register logging callback to event.
		/// </summary>
		private void Awake()
		{
			Application.logMessageReceived += Log;
			tmp = GetComponent<TextMeshPro>();
		}

		/// <summary>
		/// Unregister logging callback from event.
		/// </summary>
		private void OnDestroy()
		{
			Application.logMessageReceived -= Log;
		}

		/// <summary>
		/// Log output to a TMP component.
		/// </summary>
		/// <param name="logString"></param>
		/// <param name="stackTrace"></param>
		/// <param name="type"></param>
		public void Log(string logString, string stackTrace, LogType type)
		{
			output = logString;
			stack = stackTrace;
			myLog += output + "\n";

			tmp.text = myLog;
		}
	}

}