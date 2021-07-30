/**
 *	Common Utilities Library for developing in Unity.
 *	https://github.com/seahyx/Unity-CommonUtils
 *
 *	MIT License
 *
 *	Copyright (c) 2021 Seah Ying Xiang
 */

using System.Collections;
using UnityEngine;

namespace CommonUtils.ContentState
{
	/// <summary>
	/// Animates an <see cref="ExplodingPolygonController"/> object as transition.
	/// </summary>
	public class ExplodePolygonContentState : BaseAnimatedContentState
	{
		#region Serialization

		[Header("Exploding Polygon Animation Settings")]

		[Tooltip("Exploding polygon object reference.")]
		[SerializeField]
		private ExplodingPolygonController controller;

		#endregion

		#region Content State Animation Overrides

		protected override IEnumerator AnimateTransitionOut(OnTransitionCompleted onTransitionCompleted)
		{
			float timeElapsed = 0.0f;

			if (!controller)
			{
				Debug.LogError($"[{name}] ExplodingPolygonController is not assigned. Out transition will be skipped.");

				timeElapsed = outAnimDuration + 1.0f;
			}

			// Enable the controller
			controller.gameObject.SetActive(true);
			// When transitioning out, we should flip the controller so the animation goes upwards
			controller.transform.Rotate(new Vector3(0.0f, 0.0f, 180.0f));
			controller.SetPositionToCamera();

			while (timeElapsed < outAnimDuration)
			{
				float t = outAnimCurve.Evaluate(timeElapsed / outAnimDuration);

				// We're going from empty to covered, thus the animation is reversed
				controller.SetAnimationProgress(1.0f - t);

				timeElapsed += Time.deltaTime;

				yield return null;
			}

			// Snap to final position
			controller.SetAnimationProgress(0.0f);

			yield return StartCoroutine(base.AnimateTransitionOut(onTransitionCompleted));
		}

		protected override IEnumerator AnimateTransitionIn(OnTransitionCompleted onTransitionCompleted)
		{
			float timeElapsed = 0.0f;

			if (!controller)
			{
				Debug.LogError($"[{name}] ExplodingPolygonController is not assigned. In transition will be skipped.");

				timeElapsed = inAnimDuration + 1.0f;
			}

			// Enable the controller
			controller.gameObject.SetActive(true);
			// When transitioning in, we should flip the controller again so the animation is in the same orientation
			controller.transform.Rotate(new Vector3(0.0f, 0.0f, 180.0f));
			controller.SetPositionToCamera();

			while (timeElapsed < inAnimDuration)
			{
				float t = inAnimCurve.Evaluate((timeElapsed) / inAnimDuration);

				// We're going from covered to empty
				controller.SetAnimationProgress(t);

				timeElapsed += Time.deltaTime;

				yield return null;
			}

			// Snap to final position
			controller.SetAnimationProgress(1.0f);
			// Disable the controller
			controller.gameObject.SetActive(false);

			yield return StartCoroutine(base.AnimateTransitionIn(onTransitionCompleted));
		}

		#endregion
	}
}
