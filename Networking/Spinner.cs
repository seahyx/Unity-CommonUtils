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
	/// Spins an object.
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

		[Tooltip("Spin direction.")]
		[SerializeField]
		private bool clockwise = true;

		[Tooltip("Spin up duration.")]
		[SerializeField]
		private float spinUpDur = 1.0f;

		[Tooltip("Spin up curve.")]
		[SerializeField]
		private AnimationCurve spinUpCurve = AnimationCurve.EaseInOut(0.0f, 0.0f, 1.0f, 1.0f);

		public string testString { get; set; }

		#endregion

		#region Member Declarations

		/// <summary>
		/// Determines how much the object has spun up. [0..1]
		/// </summary>
		private float spinUpFactor
		{
			get => spinUpCurve.Evaluate(spinUpTimer / spinUpDur);
		}

		/// <summary>
		/// Property for spinUpTimer.
		/// </summary>
		private float _spinUpTimer = 0.0f;

		/// <summary>
		/// Timer for the spin up animation. Clamped between 0 and spinUpDur.
		/// </summary>
		private float spinUpTimer
		{
			get => _spinUpTimer;
			set => _spinUpTimer = Mathf.Clamp(value, 0.0f, spinUpDur);
		}

		#endregion

		#region MonoBehaviour

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

					// Find current rotation speed
					float speed = spinSpeed * spinUpFactor * Time.deltaTime;
					if (!clockwise)
						speed *= -1;

					if (!target)
						target = gameObject;

					// Rotate object
					target.transform.Rotate(new Vector3(0.0f, speed));
				}
				else
				{
					// Spun up already

					if (!target)
						target = gameObject;

					// Rotate object
					target.transform.Rotate(new Vector3(0.0f, spinSpeed * Time.deltaTime));
				}
			}
			else
			{
				// Spinning down
				if (spinUpFactor > 0.0f)
				{
					// Update timer
					spinUpTimer -= Time.deltaTime;

					// Find current rotation speed
					float speed = spinSpeed * spinUpFactor * Time.deltaTime;
					if (!clockwise)
						speed *= -1;

					if (!target)
						target = gameObject;

					// Rotate object
					target.transform.Rotate(new Vector3(0.0f, speed));
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
		/// Toggles spin with a bool value.
		/// </summary>
		/// <param name="isSpinning">Spin or not.</param>
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
