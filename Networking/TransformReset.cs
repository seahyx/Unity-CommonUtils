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
	/// Keeps the transform's position, rotation, and scale in memory so it can be reset back to the original state whenever.
	/// It also animates the transition.
	/// </summary>
	public class TransformReset : MonoBehaviour
	{
		#region Type Definitions

		[Flags]
		public enum AnimatingFlags
		{
			None = 0,
			All = Position | Rotation | Scale,
			Position = 1 << 0,
			Rotation = 1 << 1,
			Scale = 1 << 2
		}

		#endregion

		#region Serialization

		[Title("Configuration - References")]

		[Tooltip("Target transform. Leave empty to reference the Transform on this GameObject.")]
		[SerializeField]
		private Transform target;


		[Title("Configuration - Transform Events")]

		[Tooltip("Invoked when any aspect of the transform is being reset. Will not be invoked if a reset function is called while an animation is already executing.")]
		[SerializeField]
		private UnityEvent onResetStart = new UnityEvent();

		[Tooltip("Invoked when all reset animations have finished.")]
		[SerializeField]
		private UnityEvent onResetEnd = new UnityEvent();

		[Tooltip("Invoked when any aspect of the transform is being reset, regardless of whether an animation is already executing.")]
		[SerializeField]
		private UnityEvent onAnyResetStart = new UnityEvent();

		[Tooltip("Invoked when any aspect of the transform has finished animating, regardless of whether any animation is still executing.")]
		[SerializeField]
		private UnityEvent onAnyResetEnd = new UnityEvent();


		[Title("Configuration - Animation Settings")]

		[Tooltip("Animation Duration")]
		[Range(0.001f, 10.0f)]
		[SerializeField]
		private float animDuration = 1.5f;

		[Tooltip("Animation Curve")]
		[SerializeField]
		private AnimationCurve animCurve = AnimationCurve.Linear(0.0f, 0.0f, 1.0f, 1.0f);

		#endregion

		#region Member Declarations

		/// <summary>
		/// Original position of transform.
		/// </summary>
		private Vector3 originalPosition { get; set; }

		/// <summary>
		/// Original rotation of transform.
		/// </summary>
		private Quaternion originalRotation { get; set; }

		/// <summary>
		/// Original scale of transform.
		/// </summary>
		private Vector3 originalScale { get; set; }

		/// <summary>
		/// Flags the transform is currently being animated.
		/// </summary>
		private bool isAnimating
		{
			get => animFlags > 0;
		}

		/// <summary>
		/// Animation flags.
		/// </summary>
		private AnimatingFlags animFlags { get; set; } = AnimatingFlags.None;

		/// <summary>
		/// Enumerator coroutine reference for the position reset animation.
		/// </summary>
		private IEnumerator positionAnim { get; set; }

		/// <summary>
		/// Enumerator coroutine reference for the rotation reset animation.
		/// </summary>
		private IEnumerator rotationAnim { get; set; }

		/// <summary>
		/// Enumerator coroutine reference for the scale reset animation.
		/// </summary>
		private IEnumerator scaleAnim { get; set; }

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
		/// Reset model position.
		/// </summary>
		public void ResetPosition()
		{
			// Stop any existing animation
			if (positionAnim != null)
				StopCoroutine(positionAnim);

			// Start animation
			positionAnim = ResetPositionAnimation();
			StartCoroutine(positionAnim);
		}

		/// <summary>
		/// Reset model rotation.
		/// </summary>
		public void ResetRotation()
		{
			// Stop any existing animation
			if (rotationAnim != null)
				StopCoroutine(rotationAnim);

			// Start animation
			rotationAnim = ResetRotationAnimation();
			StartCoroutine(rotationAnim);
		}

		/// <summary>
		/// Reset model scale.
		/// </summary>
		public void ResetScale()
		{
			// Stop any existing animation
			if (scaleAnim != null)
				StopCoroutine(scaleAnim);

			// Start animation
			scaleAnim = ResetScaleAnimation();
			StartCoroutine(scaleAnim);
		}

		/// <summary>
		/// Resets the transform's position, rotation, and scale to its original state.
		/// </summary>
		public void ResetAllTransforms()
		{
			ResetPosition();
			ResetRotation();
			ResetScale();
		}

		/// <summary>
		/// Updates the original position, rotation, and scale from the current transform.
		/// The transform will reset to this position.
		/// </summary>
		public void UpdateOriginalTransform()
		{
			originalPosition = transform.localPosition;
			originalRotation = transform.localRotation;
			originalScale = transform.localScale;
		}

		#endregion

		#region Animation Coroutines

		/// <summary>
		/// Transform position reset animation coroutine.
		/// </summary>
		/// <returns></returns>
		public IEnumerator ResetPositionAnimation()
		{
			// Pre-animation preparation
			OnStartAnimation(AnimatingFlags.Position);

			Vector3 startPosition = transform.localPosition;

			// Check if transform is already original
			if (startPosition == originalPosition)
			{
				Debug.Log($"[{name}] Transform is already at its original position. Reset position animation will end.");

				// Post-animation preparation
				OnEndAnimation(AnimatingFlags.Position);

				yield break;
			}

			float elapsedTime = 0.0f;

			while (elapsedTime < animDuration)
			{
				// Elapse time
				elapsedTime += Time.deltaTime;

				// Evaluate animation curve position
				float eval = animCurve.Evaluate(elapsedTime / animDuration);

				// Update position
				transform.localPosition = Vector3.Lerp(startPosition, originalPosition, eval);

				// Wait for next frame
				yield return null;
			}

			// Snap transform position when animation ends
			transform.localPosition = originalPosition;

			// Post-animation preparation
			OnEndAnimation(AnimatingFlags.Position);
		}

		/// <summary>
		/// Transform rotation reset animation coroutine.
		/// </summary>
		/// <returns></returns>
		public IEnumerator ResetRotationAnimation()
		{
			// Pre-animation preparation
			OnStartAnimation(AnimatingFlags.Rotation);

			Quaternion startRotation = transform.localRotation;

			// Check if transform is already original
			if (startRotation == originalRotation)
			{
				Debug.Log($"[{name}] Transform is already at its original rotation. Reset rotation animation will end.");

				// Post-animation preparation
				OnEndAnimation(AnimatingFlags.Rotation);

				yield break;
			}

			float elapsedTime = 0.0f;

			while (elapsedTime < animDuration)
			{
				// Elapse time
				elapsedTime += Time.deltaTime;

				// Evaluate animation curve position
				float eval = animCurve.Evaluate(elapsedTime / animDuration);

				// Update rotation
				transform.localRotation = Quaternion.Slerp(startRotation, originalRotation, eval);

				// Wait for next frame
				yield return null;
			}

			// Snap transform rotation when animation ends
			transform.localRotation = originalRotation;

			// Post-animation preparation
			OnEndAnimation(AnimatingFlags.Rotation);
		}

		/// <summary>
		/// Transform scale reset animation coroutine.
		/// </summary>
		/// <returns></returns>
		public IEnumerator ResetScaleAnimation()
		{
			// Pre-animation preparation
			OnStartAnimation(AnimatingFlags.Scale);

			Vector3 startScale = transform.localScale;

			// Check if transform is already original
			if (startScale == originalScale)
			{
				Debug.Log($"[{name}] Transform is already at its original scale. Reset scale animation will end.");

				// Post-animation preparation
				OnEndAnimation(AnimatingFlags.Scale);

				yield break;
			}

			float elapsedTime = 0.0f;

			while (elapsedTime < animDuration)
			{
				// Elapse time
				elapsedTime += Time.deltaTime;

				// Evaluate animation curve position
				float eval = animCurve.Evaluate(elapsedTime / animDuration);

				// Update scale
				transform.localScale = Vector3.Lerp(startScale, originalScale, eval);

				// Wait for next frame
				yield return null;
			}

			// Snap transform scale when animation ends
			transform.localScale = originalScale;

			// Post-animation preparation
			OnEndAnimation(AnimatingFlags.Scale);
		}

		#endregion

		#region Helper Functions

		/// <summary>
		/// Executes pre-animation.
		/// </summary>
		/// <param name="animatingFlag">Which animation is being executed.</param>
		private void OnStartAnimation(AnimatingFlags animatingFlag)
		{
			// Invoke events

			// Check if anything else has started animation
			if (!isAnimating)
				onResetStart.Invoke();

			onAnyResetStart.Invoke();

			// | - Bitwise OR operator - 0010 0000 | 0000 1100 => 0010 1100
			// Toggles on the flag
			animFlags |= animatingFlag;
		}

		/// <summary>
		/// Executes post-animation.
		/// </summary>
		/// <param name="animatingFlag">Which animation is being executed.</param>
		private void OnEndAnimation(AnimatingFlags animatingFlag)
		{
			// Invoke events
			onAnyResetEnd.Invoke();

			// ~ - Bitwise complement operator - 0010 0000 => 1101 1111
			// & - Bitwise AND operator - 0010 1011 & 1101 1111 => 0000 1011
			// Toggles off the flag
			animFlags &= ~animatingFlag;

			// Check if anything is still being animated
			if (!isAnimating)
				onResetEnd.Invoke();
		}

		#endregion
	}
}