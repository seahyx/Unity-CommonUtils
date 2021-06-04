/**
 *	Common Utilities Library for developing in Unity.
 *	https://github.com/seahyx/Unity-CommonUtils
 *
 *	MIT License
 *
 *	Copyright (c) 2021 Seah Ying Xiang
 */

using UnityEngine.Events;

/// <summary>
/// Common unity events.
/// </summary>
namespace CommonUtils
{
	/// <summary>
	/// Unity Event with a single <see cref="bool"/> parameter.
	/// </summary>
	[System.Serializable]
	public class UnityBooleanEvent : UnityEvent<bool> { }

	/// <summary>
	/// Unity Event with a single <see cref="int"/> parameter.
	/// </summary>
	[System.Serializable]
	public class UnityIntEvent : UnityEvent<int> { }

	/// <summary>
	/// Unity Event with a single <see cref="uint"/> parameter.
	/// </summary>
	[System.Serializable]
	public class UnityUIntEvent : UnityEvent<uint> { }

	/// <summary>
	/// Unity Event with a single <see cref="string"/> parameter.
	/// </summary>
	[System.Serializable]
	public class UnityStringEvent : UnityEvent<string> { }

	/// <summary>
	/// Unity Event with an <see cref="string"/> and <see cref="bool"/> parameter.
	/// </summary>
	[System.Serializable]
	public class UnityStringBooleanEvent : UnityEvent<string, bool> { }
}
