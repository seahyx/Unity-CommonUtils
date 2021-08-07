/**
 *	Common Utilities Library for developing in Unity.
 *	https://github.com/seahyx/Unity-CommonUtils
 *
 *	MIT License
 *
 *	Copyright (c) 2021 Seah Ying Xiang
 */

using Sirenix.OdinInspector;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;

namespace CommonUtils
{
	/// <summary>
	/// Saves the target's original position, rotation, and scale on <see cref="Awake"/>.
	/// <br></br>
	/// When <see cref="ResetTransform"/> is called, <see cref="ResetAnimation"/> will move the target back to the original position, rotation, and scale.
	/// </summary>
	public class TransformReset : MonoBehaviour
	{
		#region Type Definitions

		[Flags]
		public enum TransformFlags : short
		{
			None = 0,
			Position = 1,
			Rotation = 2,
			Scale = 4,
			All = 7
		}

		#endregion

		#region Serialization

		[InfoBox("Saves the target's original position, rotation, and scale on Awake. When ResetTransform() is called, reset animation will move the target back to the original position, rotation, and scale.\n\n" +
			"Leave target empty to reference the transform on this GameObject.")]

		[Title("Configuration")]

		[Tooltip("Target transform. Leave empty to reference the transform on this GameObject.")]
		[SerializeField]
		private Transform target;

		[Title("Animation Settings")]

		[Tooltip("Animation Duration")]
		[Range(0.001f, 10.0f)]
		[SerializeField]
		private float animDuration = 1.5f;

		[Tooltip("Animation Curve")]
		[SerializeField]
		private AnimationCurve animCurve = AnimationCurve.Linear(0.0f, 0.0f, 1.0f, 1.0f);

		[Title("Events")]

		[Tooltip("Invoked on start of reset animation.")]
		[SerializeField]
		public UnityEvent OnResetStart = new UnityEvent();

		[Tooltip("Invoked on end of reset animation.")]
		[SerializeField]
		public UnityEvent OnResetEnd = new UnityEvent();

		#endregion

		#region Member Declarations

		/// <summary>
		/// Original position of <see cref="Transform"/>.
		/// </summary>
		public Vector3 originalPosition { get; set; }

		/// <summary>
		/// Original rotation of <see cref="Transform"/>.
		/// </summary>
		public Quaternion originalRotation { get; set; }

		/// <summary>
		/// Original scale of <see cref="Transform"/>.
		/// </summary>
		public Vector3 originalScale { get; set; }

		/// <summary>
		/// Whether the <see cref="Transform"/> is currently being animated.
		/// </summary>
		public bool isAnimating { get; set; } = false;

		#endregion

		#region Monobehaviour

		/// <summary>
		/// Save the original position, rotation, and scale.
		/// </summary>
		private void Awake()
		{
			if (!target)
				target = transform;

			UpdateOriginalTransform();
		}

		#endregion

		#region Public Functions

		/// <summary>
		/// Reset position and rotation only.
		/// </summary>
		public void ResetPosRot()
		{
			ResetTransform(TransformFlags.Position | TransformFlags.Rotation);
		}

		/// <summary>
		/// Reset all transforms.
		/// </summary>
		public void ResetAll()
		{
			ResetTransform(TransformFlags.All);
		}

		/// <summary>
		/// Resets the <see cref="Transform"/>'s position, rotation, and scale to its original state.
		/// </summary>
		/// <param name="tflags">Flags for which part of the transform should be reset.</param>
		[Button(name: "Test Reset Transform")]
		public void ResetTransform(TransformFlags tflags = TransformFlags.All)
		{
			StartCoroutine(ResetAnimation(tflags));
		}

		/// <summary>
		/// Updates the original position, rotation, and scale from the current transform.
		/// The <see cref="Transform"/> will reset to this position.
		/// </summary>
		[Button(name: "Test Update Original Transform")]
		public void UpdateOriginalTransform()
		{
			originalPosition = transform.localPosition;
			originalRotation = transform.localRotation;
			originalScale = transform.localScale;
		}

		/// <summary>
		/// Stop all animation coroutines if any. If there is an animation running, invoke the reset end event.
		/// </summary>
		public void StopAllAnimations()
		{
			if (isAnimating)
			{
				StopAllCoroutines();

				// Set flag to complete
				isAnimating = false;

				// Invoke end event
				OnResetEnd.Invoke();
			}
		}

		#endregion

		#region Animation Coroutine

		/// <summary>
		/// Reset animation coroutine.
		/// </summary>
		public IEnumerator ResetAnimation(TransformFlags tflags)
		{
			Vector3 startPosition = transform.localPosition;
			Quaternion startRotation = transform.localRotation;
			Vector3 startScale = transform.localScale;

			// Invoke start event
			OnResetStart.Invoke();

			if (tflags == TransformFlags.None)
			{
				// Invoke end event
				OnResetEnd.Invoke();

				yield break;
			}

			// Check if transform is already original
			if (!(tflags.HasFlag(TransformFlags.Position) && startPosition != originalPosition) &&
				!(tflags.HasFlag(TransformFlags.Rotation) && startRotation != originalRotation) &&
				!(tflags.HasFlag(TransformFlags.Scale) && startScale != originalScale))
			{
				Debug.Log($"[{name}] Transform is already at its original position. Reset animation will end.");

				// Invoke end event
				OnResetEnd.Invoke();

				yield break;
			}

			float elapsedTime = 0.0f;
			isAnimating = true;

			while (elapsedTime < animDuration)
			{
				// Evaluate animation curve position
				float eval = animCurve.Evaluate(elapsedTime / animDuration);

				// Set transforms
				if (tflags.HasFlag(TransformFlags.Position))
					transform.localPosition = Vector3.Lerp(startPosition, originalPosition, eval);

				if (tflags.HasFlag(TransformFlags.Rotation))
					transform.localRotation = Quaternion.Slerp(startRotation, originalRotation, eval);

				if (tflags.HasFlag(TransformFlags.Scale))
					transform.localScale = Vector3.Lerp(startScale, originalScale, eval);

				// Elapse time
				elapsedTime += Time.deltaTime;

				// Wait for next frame
				yield return null;
			}

			// Snap transforms when animation ends
			if (tflags.HasFlag(TransformFlags.Position))
				transform.localPosition = originalPosition;

			if (tflags.HasFlag(TransformFlags.Rotation))
				transform.localRotation = originalRotation;

			if (tflags.HasFlag(TransformFlags.Scale))
				transform.localScale = originalScale;

			// Set flag to complete
			isAnimating = false;

			// Invoke end event
			OnResetEnd.Invoke();
		}

		#endregion
	}
}