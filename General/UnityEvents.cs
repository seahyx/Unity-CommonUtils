/**
 *	Common Utilities Library for developing in Unity.
 *	https://github.com/seahyx/Unity-CommonUtils
 *
 *	MIT License
 *
 *	Copyright (c) 2021 Seah Ying Xiang
 */

using System.Collections.Generic;
using UnityEngine.Events;

/// <summary>
/// Common unity events.
/// </summary>
namespace CommonUtils
{
	/// <summary>
	/// Unity Event with a single boolean argument.
	/// </summary>
	[System.Serializable]
	public class UnityBooleanEvent : UnityEvent<bool> { }

	/// <summary>
	/// Unity Event with a single string argument.
	/// </summary>
	[System.Serializable]
	public class UnityStringEvent : UnityEvent<string> { }

	/// <summary>
	/// Unity Event with an int and boolean argument.
	/// </summary>
	[System.Serializable]
	public class UnityStringBooleanEvent : UnityEvent<string, bool> { }
}
