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
	/// Unity Event with a single <see cref="bool"/> argument.
	/// </summary>
	[System.Serializable]
	public class UnityBooleanEvent : UnityEvent<bool> { }

	/// <summary>
	/// Unity Event with a single <see cref="string"/> argument.
	/// </summary>
	[System.Serializable]
	public class UnityStringEvent : UnityEvent<string> { }

	/// <summary>
	/// Unity Event with an <see cref="string"/> and <see cref="bool"/> argument.
	/// </summary>
	[System.Serializable]
	public class UnityStringBooleanEvent : UnityEvent<string, bool> { }
}
