﻿/**
 *	Common Utilities Library for developing in Unity.
 *	https://github.com/seahyx/Unity-CommonUtils
 *
 *	MIT License
 *
 *	Copyright (c) 2021 Seah Ying Xiang
 */

using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CommonUtils
{
	/// <summary>
	/// Scales the model with smooth interpolation. Scales all axis equally.
	/// </summary>
	public class ModelScaler : MonoBehaviour
	{
		#region Serialization

		[InfoBox("This thing makes the model dance around elegantly even if you spam SetScale on this like a drunk madman on steroids.")]

		[Title("Configuration - General Settings")]

		[Tooltip("Target Transform to scale. Leave empty to reference the Transform on this GameObject.")]
		[SerializeField]
		private Transform target;

		[Title("Configuration - Animation")]

		[Tooltip("Animation duration.")]
		[SerializeField]
		private float animDuration = 0.5f;

		#endregion

		#region Member Declarations

		[Title("Debug Values")]

		/// <summary>
		/// Currently running <see cref="AnimationCurve"/>.
		/// </summary>
		[ShowInInspector, ReadOnly]
		private AnimationCurve currentAnimCurve { get; set; }

		/// <summary>
		/// Current elapsed time of the animation.
		/// </summary>
		private float elapsedTime { get; set; } = 0.0f;

		/// <summary>
		/// Ending duration of the animation.
		/// </summary>
		private float endTimer { get; set; }  = 0.0f;

		/// <summary>
		/// Flag for whether the animation is currently running.
		/// </summary>
		[ShowInInspector, ReadOnly]
		private bool isAnimating { get; set; } = false;

		/// <summary>
		/// Final target scale for the animation.
		/// </summary>
		[ShowInInspector, ReadOnly]
		private float finalScale { get; set; } = 1.0f;

		#endregion

		#region MonoBehaviour

		/// <summary>
		/// Initialize scale.
		/// </summary>
		private void Awake()
		{
			if (!target)
				target = transform;
		}

		/// <summary>
		/// Update scaling animation (if any).
		/// </summary>
		private void Update()
		{
			if (!isAnimating)
				return;

			if (elapsedTime < endTimer)
			{
				// Advance timer
				elapsedTime += Time.deltaTime;

				// Animation curve reflects actual value on actual time
				float resultScale = currentAnimCurve.Evaluate(elapsedTime);

				// Update scale
				target.localScale = new Vector3(resultScale, resultScale, resultScale);
			}
			else
			{
				// Animation end, clamp to final values
				target.localScale = new Vector3(finalScale, finalScale, finalScale);

				// Toggle flag
				isAnimating = false;
			}
		}

		#endregion

		#region Public Functions

		/// <summary>
		/// Start the animation or update an existing animation to scale to <paramref name="scale"/>.
		/// </summary>
		/// <param name="scale">Model scale value for x, y, and z.</param>
		public void SetScale(float scale)
		{
			finalScale = scale;

			if (!isAnimating)
			{
				// Not animating, so generate the anim curve and start
				currentAnimCurve = GenerateAnimCurve(target.localScale.x, scale);

				// Reset timer
				elapsedTime = 0.0f;

				// Set end time
				endTimer = animDuration;

				// Toggle flag
				isAnimating = true;
			}
			else
			{
				// Is already animating, so we need to recalculate the curve
				RecalculateAnimCurve(currentAnimCurve, elapsedTime, scale);

				// Add time to animation
				endTimer = elapsedTime + animDuration;
			}
		}

		#endregion

		#region Helper Functions

		/// <summary>
		/// Generates an <see cref="AnimationCurve"/> when none is running.
		/// </summary>
		/// <param name="startValue">Starting animation value.</param>
		/// <param name="finalValue">Final target value.</param>
		/// <returns>Ease-in-out animation curve./></returns>
		private AnimationCurve GenerateAnimCurve(float startValue, float finalValue)
		{
			return AnimationCurve.EaseInOut(0.0f, startValue, animDuration, finalValue);
		}

		/// <summary>
		/// Recalculates the <see cref="currentAnimCurve"/>, extending it based on the current position and velocity of the animation.
		/// </summary>
		/// <param name="animCurve">The <see cref="AnimationCurve"/> to recalculate.</param>
		/// <param name="timer">Current time on the animation.</param>
		/// <param name="finalValue">Next target scale value.</param>
		private void RecalculateAnimCurve(AnimationCurve animCurve, float timer, float finalValue)
		{
			// Get current value of the animation
			float currentValue = animCurve.Evaluate(timer);

			// Add key on the current position and value, so the curve gets interpolated from this position
			animCurve.AddKey(timer, currentValue);

			// Replace the ending key
			Keyframe endKey = new Keyframe(timer + animDuration, finalValue, 0.0f, 0.0f);
			animCurve.MoveKey(animCurve.keys.Length - 1, endKey);
		}

		#endregion
	}

}