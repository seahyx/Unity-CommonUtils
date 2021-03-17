using Sirenix.OdinInspector;
using System.Collections;
using UnityEngine;

namespace CommonUtils
{
	/// <summary>
	/// Keeps the transform's position, rotation, and scale in memory so it can be reset back to the original state whenever.
	/// It also animates the transition.
	/// </summary>
	public class TransformReset : MonoBehaviour
	{

		#region Serialization

		[Title("Configuration")]

		[ToolTip("Target transform. Leave empty to reference the transform on this GameObject.")]
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
		/// Whether the transform is currently being animated.
		/// </summary>
		private bool isAnimating { get; set; } = false;

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
		/// Resets the transform's position, rotation, and scale to its original state.
		/// </summary>
		public void ResetTransform()
		{
			StartCoroutine(ResetAnimation());
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

		#region Animation Coroutine

		public IEnumerator ResetAnimation()
		{
			Vector3 startPosition = transform.localPosition;
			Quaternion startRotation = transform.localRotation;
			Vector3 startScale = transform.localScale;

			// Check if transform is already original
			if (startPosition == originalPosition && startRotation == originalRotation && startScale == originalScale)
			{
				Debug.Log($"[{name}] Transform is already at its original position. Reset animation will end.");
				yield break;
			}

			float elapsedTime = 0.0f;
			isAnimating = true;

			while (elapsedTime < animDuration)
			{
				// Evaluate animation curve position
				float eval = animCurve.Evaluate(elapsedTime / animDuration);

				// Set transforms
				transform.localPosition = Vector3.Lerp(startPosition, originalPosition, eval);
				transform.localRotation = Quaternion.Slerp(startRotation, originalRotation, eval);
				transform.localScale = Vector3.Lerp(startScale, originalScale, eval);

				// Elapse time
				elapsedTime += Time.deltaTime;

				// Wait for next frame
				yield return null;
			}

			// Snap transforms when animation ends
			transform.localPosition = originalPosition;
			transform.localRotation = originalRotation;
			transform.localScale = originalScale;

			// Set flag to complete
			isAnimating = false;
		}

		#endregion
	}
}