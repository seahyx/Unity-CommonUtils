/**
 *	Common Utilities Library for developing in Unity.
 *	https://github.com/seahyx/Unity-CommonUtils
 *
 *	MIT License
 *
 *	Copyright (c) 2021 Seah Ying Xiang
 */

using Sirenix.OdinInspector;
using UnityEngine;

namespace CommonUtils
{
	/// <summary>
	/// Spins an object, with configurable acceleration/deceleration and spinning speed.
	/// </summary>
	public class Spinner : MonoBehaviour
	{
		#region Serialization

		[InfoBox("You spin me right 'round, baby.")]

		[Title("Configuration")]

		[Tooltip("Target to adjust alignment. Leave empty to reference this GameObject.")]
		[SerializeField]
		private GameObject target;

		[Tooltip("Whether this object is spinning.")]
		[SerializeField]
		public bool IsSpinning = true;

		[Tooltip("Speed of object spin in degrees per second.")]
		[SerializeField]
		private float spinSpeed = 40.0f;

		[Tooltip("Local spin axis. The object will spin around this vector. This vector will be normalized.")]
		[SerializeField]
		private Vector3 spinAxis = Vector3.up;

		[Tooltip("Spin up duration.")]
		[SerializeField]
		private float spinUpDur = 1.0f;

		[Tooltip("Spin up curve.")]
		[SerializeField]
		private AnimationCurve spinUpCurve = AnimationCurve.EaseInOut(0.0f, 0.0f, 1.0f, 1.0f);

		#endregion

		#region Member Declarations

		/// <summary>
		/// Determines how much the object has spun up. <c>[0..1]</c>
		/// </summary>
		private float spinUpFactor
		{
			get => spinUpCurve.Evaluate(spinUpTimer / spinUpDur);
		}

		/// <summary>
		/// Field for <see cref="spinUpTimer"/>.
		/// </summary>
		private float _spinUpTimer = 0.0f;

		/// <summary>
		/// Timer for the spin up animation. Clamped between <c>0</c> and <see cref="spinUpDur"/>.
		/// </summary>
		private float spinUpTimer
		{
			get => _spinUpTimer;
			set => _spinUpTimer = Mathf.Clamp(value, 0.0f, spinUpDur);
		}

		/// <summary>
		/// Time-adjusted current speed of the spin.
		/// </summary>
		private float speed => spinSpeed * spinUpFactor * Time.deltaTime;

		/// <summary>
		/// Normalized spin vector, from value <see cref="spinAxis"/>.
		/// </summary>
		private Vector3 nSpinAxis => Vector3.Normalize(spinAxis);

		#endregion

		#region MonoBehaviour

		/// <summary>
		/// Set <see cref="target"/> to this <see cref="GameObject"/> if <see langword="null"/>.
		/// </summary>
		private void Awake()
		{
			if (!target)
				target = gameObject;
		}

		/// <summary>
		/// Spinning here!
		/// </summary>
		private void Update()
		{
			// Check if spinning
			if (!IsSpinning && spinUpFactor <= 0)
				return;

			if (IsSpinning)
			{
				// Spinning up
				if (spinUpFactor < 1.0f)
				{
					// Update timer
					spinUpTimer += Time.deltaTime;

					// Rotate object
					target.transform.localRotation *= Quaternion.AngleAxis(speed, nSpinAxis);
				}
				else
				{
					// Spun up already

					// Rotate object
					target.transform.localRotation *= Quaternion.AngleAxis(spinSpeed * Time.deltaTime, nSpinAxis);
				}
			}
			else
			{
				// Spinning down
				if (spinUpFactor > 0.0f)
				{
					// Update timer
					spinUpTimer -= Time.deltaTime;

					// Rotate object
					target.transform.localRotation *= Quaternion.AngleAxis(speed, nSpinAxis);
				}
			}

		}

		#endregion

		#region Unity Events

		/// <summary>
		/// Start spinning if not already. Will not do anything if already spinning.
		/// </summary>
		public void StartSpin()
		{
			if (!IsSpinning)
				IsSpinning = true;
		}

		/// <summary>
		/// Stops the spin.
		/// </summary>
		public void StopSpin()
		{
			if (IsSpinning)
				IsSpinning = false;
		}

		/// <summary>
		/// Toggles spinning with a <see langword="bool"/> value.
		/// </summary>
		/// <param name="isSpinning">To spin or not to spin.</param>
		public void SetSpin(bool isSpinning)
		{
			if (isSpinning)
				StartSpin();
			else
				StopSpin();
		}

		#endregion
	}
}
