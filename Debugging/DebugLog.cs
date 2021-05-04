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
	/// Tool to display the console log into a <see cref="TextMeshPro"/> component.
	/// </summary>
	public class DebugLog : MonoBehaviour
	{
		#region Member Declarations

		/// <summary>
		/// Log <see langword="string"/>.
		/// </summary>
		static string myLog { get; set; } = "";

		/// <summary>
		/// Stack trace.
		/// </summary>
		private string stack { get; set; }

		/// <summary>
		/// Output <see langword="string"/>.
		/// </summary>
		private string output { get; set; }

		/// <summary>
		/// <see cref="TextMeshPro"/> reference.
		/// </summary>
		private TextMeshPro tmp { get; set; }

		#endregion

		#region MonoBehaviour

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

		#endregion

		#region Public Functions

		/// <summary>
		/// Logs output to a TMP component.
		/// </summary>
		/// <param name="logString">Log string.</param>
		/// <param name="stackTrace">Stack trace.</param>
		/// <param name="type">Log level.</param>
		public void Log(string logString, string stackTrace, LogType type)
		{
			output = logString;
			stack = stackTrace;
			myLog += output + "\n";

			tmp.text = myLog;
		}

		#endregion
	}

}