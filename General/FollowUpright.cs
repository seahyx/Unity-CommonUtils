/**
 *	Common Utilities Library for developing in Unity.
 *	https://github.com/seahyx/Unity-CommonUtils
 *
 *	MIT License
 *
 *	Copyright (c) 2021 Seah Ying Xiang
 */

using UnityEngine;

namespace CommonUtils
{
	/// <summary>
	/// Positions this object with an offset from a target <see cref="Transform"/> while it is enabled.
	/// </summary>
	public class FollowTarget : MonoBehaviour
	{
		#region Serialization

		[Header("Configuration")]

		[Tooltip("The target transform to follow.")]
		[SerializeField]
		private Transform targetPoint;

		[Tooltip("The distance from the transform at which this gameobject should float.")]
		[SerializeField]
		private Vector3 offset = new Vector3(0.0f, 0.1f, 0.0f);

		#endregion

		#region MonoBehaviour

		private void Update()
		{
			// Update the position of this object
			UpdatePosition();
		}

		#endregion

		#region Methods

		/// <summary>
		/// Position this object at an <see cref="offset"/> from <see cref="targetPoint"/>.
		/// </summary>
		private void UpdatePosition()
		{
			transform.position = targetPoint.position + offset;
		}

		#endregion
	}
}