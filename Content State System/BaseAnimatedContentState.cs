/**
 *	Common Utilities Library for developing in Unity.
 *	https://github.com/seahyx/Unity-CommonUtils
 *
 *	MIT License
 *
 *	Copyright (c) 2021 Seah Ying Xiang
 */

using UnityEngine;

namespace CommonUtils.ContentState
{
	/// <summary>
	/// Inheritable content state class with preset serialized fields for animation settings.
	/// </summary>
	public class BaseAnimatedContentState : BaseContentState
	{
		#region Serialization

		[Header("Transition Animation Config")]

		[Tooltip("Transition In Animation Duration")]
		[Range(0.0f, 10.0f)]
		[SerializeField]
		protected float inAnimDuration = 0.2f;

		[Tooltip("Transition In Animation Curve")]
		[SerializeField]
		protected AnimationCurve inAnimCurve = AnimationCurve.Linear(0.0f, 0.0f, 1.0f, 1.0f);

		[Tooltip("Transition out Animation Duration")]
		[Range(0.0f, 10.0f)]
		[SerializeField]
		protected float outAnimDuration = 0.2f;

		[Tooltip("Transition out Animation Curve")]
		[SerializeField]
		protected AnimationCurve outAnimCurve = AnimationCurve.Linear(0.0f, 0.0f, 1.0f, 1.0f);

		#endregion
	}
}
