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
using UnityEngine;

namespace CommonUtils.ContentState
{
	/// <summary>
	/// Animates a pop in and pop out transisition.
	/// </summary>
	public class PopInContentState : BaseAnimatedContentState
	{
		#region Serialization

		[Header("Pop In/Out Animation Settings")]

		[Tooltip("Ending position offset.")]
		[SerializeField]
		private Vector3 endingOffset = Vector3.zero;

		#endregion

		#region Member Declarations

		/// <summary>
		/// Original position of this object.
		/// </summary>
		private Vector3 originalPosition { get; set; } = Vector3.zero;

		/// <summary>
		/// Original scale of this object.
		/// </summary>
		private Vector3 originalScale { get; set; } = Vector3.one;

		#endregion

		#region Monobehaviour

		/// <summary>
		/// Resource initialization.
		/// </summary>
		private void Awake()
		{
			originalPosition = transform.localPosition;
			originalScale = transform.localScale;
		}

		#endregion

		#region Content State Animation Overrides

		protected override IEnumerator AnimateTransitionOut(OnTransitionCompleted onTransitionCompleted)
		{
			float timeElapsed = 0.0f;

			originalPosition = transform.localPosition;
			originalScale = transform.localScale;

			while (timeElapsed < outAnimDuration)
			{
				float t = outAnimCurve.Evaluate(timeElapsed / outAnimDuration);

				transform.localPosition = Vector3.LerpUnclamped(originalPosition, originalPosition + endingOffset, t);
				transform.localScale = Vector3.LerpUnclamped(originalScale, Vector3.zero, t);

				timeElapsed += Time.deltaTime;

				yield return null;
			}

			transform.localPosition = originalPosition + endingOffset;
			transform.localScale = Vector3.zero;

			yield return StartCoroutine(base.AnimateTransitionOut(onTransitionCompleted));
		}

		protected override IEnumerator AnimateTransitionIn(OnTransitionCompleted onTransitionCompleted)
		{
			float timeElapsed = 0.0f;

			while (timeElapsed < inAnimDuration)
			{
				// Reverse the animation
				float t = inAnimCurve.Evaluate((inAnimDuration - timeElapsed) / inAnimDuration);

				transform.localPosition = Vector3.LerpUnclamped(originalPosition, originalPosition + endingOffset, t);
				transform.localScale = Vector3.LerpUnclamped(originalScale, Vector3.zero, t);

				timeElapsed += Time.deltaTime;

				yield return null;
			}

			transform.localPosition = originalPosition;
			transform.localScale = originalScale;

			yield return StartCoroutine(base.AnimateTransitionIn(onTransitionCompleted));
		}

		#endregion
	}
}
