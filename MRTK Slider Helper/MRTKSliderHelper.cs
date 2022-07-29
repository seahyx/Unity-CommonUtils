/**
 *	Common Utilities Library for developing in Unity.
 *	https://github.com/seahyx/Unity-CommonUtils
 *
 *	MIT License
 *
 *	Copyright (c) 2021 Seah Ying Xiang
 */

using Microsoft.MixedReality.Toolkit.UI;
using Nestle;
using UnityEngine;

namespace CommonUtils
{
	/// <summary>
	/// Provides useful functions for interfacing with the <see cref="PinchSlider"/>.
	/// </summary>
	public class MRTKSliderHelper : MonoBehaviour
	{
		#region Serialization

		[Header("Configuration")]

		[SerializeField, Tooltip("PinchSlider reference. Leave empty to reference the component on this GameObject.")]
		private PinchSlider _pinchSlider;

		[SerializeField, Tooltip("SliderSounds reference. Leave empty to reference the component on this GameObject.")]
		private SliderSounds _sliderSounds;

		[Header("Slider Settings")]

		[SerializeField, Tooltip("Total slider length.")]
		public float TotalLength = 0.2f;

		[SerializeField, Tooltip("Maximum scroll value. Does not change the total length of the slider, but is a multiplier of the slider value.")]
		public float MaxScrollValue = 1.0f;

		[SerializeField, Tooltip("Invert slider start/end direction.")]
		public bool InvertDirection = false;

		[SerializeField, Tooltip("Whether to round slider values to nearest integers. Value change events will only invoke when the rounded integer value changes.")]
		private bool roundSliderValueToInteger = false;

		[SerializeField, Tooltip("Whether to resize the slider thumb to fit maximum scroll size better.")]
		public bool FitThumbToMaxScroll = true;

		[SerializeField, Tooltip("Minumum slider thumb length. Only will be used if FitThumbToMaxScroll is true.")]
		private float minSliderThumbLength = 0.05f;

		[SerializeField, Tooltip("The smaller the value, the less scrolling distance, with increased scrolling sensitivity.")]
		[Range(0.0f, 1.0f)]
		private float scrollLengthFactor = 1.0f;

		[SerializeField, Tooltip("Fixed padding on the thumb box colliders in the axis of the slider.")]
		public float ColliderLengthwisePadding = 0.0f;

		[SerializeField, Tooltip("Fixed padding on the ends of the scroll bar.")]
		private float backgroundLengthPadding = 0.0f;

		[SerializeField, Tooltip("Slider background sliced sprite. Can be left empty if there is none.")]
		public SpriteRenderer SliderBackground;

		[SerializeField, Tooltip("Slider thumb sliced sprite. Required.")]
		public SpriteRenderer SliderThumb;

		[SerializeField, Tooltip("Slider thumb box collider. Required.")]
		public BoxCollider SliderThumbCollider;

		[Header("Events")]

		[SerializeField, Tooltip("Integer slider value updates. Will only be invoked if roundSliderValueToInteger is true.")]
		public UnityIntEvent OnIntegerValueChanged = new UnityIntEvent();

		#endregion

		#region Member Declarations

		/// <summary>
		/// <see cref="PinchSlider"/> reference. Will attempt to get the component reference on the attached <see cref="GameObject"/> if the value is <see langword="null"/>.
		/// </summary>
		public PinchSlider PinchSlider
		{
			set => _pinchSlider = value;
			get
			{
				if (_pinchSlider == null)
					_pinchSlider = gameObject.GetComponent<PinchSlider>();

				if (_pinchSlider == null)
					Debug.LogError($"[{name}] PinchSlider reference on this component is null.");

				return _pinchSlider;
			}
		}

		/// <summary>
		/// <see cref="SliderSounds"/> reference. Will attempt to get the component reference on the attached <see cref="GameObject"/> if the value is <see langword="null"/>.
		/// </summary>
		public SliderSounds SliderSounds
		{
			set => _sliderSounds = value;
			get
			{
				if (_sliderSounds == null)
					_sliderSounds = gameObject.GetComponent<SliderSounds>();

				if (_sliderSounds == null)
					Debug.LogError($"[{name}] SliderSounds reference on this component is null.");

				return _sliderSounds;
			}
		}

		/// <summary>
		/// The previous rounded slider value.
		/// </summary>
		private int prevRoundedValue = -999;

		#endregion

		#region MonoBehaviour

		/// <summary>
		/// Attach to events on <see cref="_pinchSlider"/>.
		/// </summary>
		private void Awake()
		{
			PinchSlider.OnValueUpdated.AddListener(OnValueChanged);
		}

		private void OnEnable()
		{
			UpdateSlider();
		}

		#endregion

		#region Pinch Slider Functions

		/// <summary>
		/// Update various slider components.
		/// </summary>
		public void UpdateSlider()
		{
			// Calculations
			float sliderThumbLength = CalculateThumbLength();
			float actualSliderLength = CalculateActualSliderLength();

			// First, update PinchSlider values
			if (PinchSlider)
			{
				PinchSlider.SliderStartDistance = actualSliderLength / 2 * (InvertDirection ? 1 : -1);
				PinchSlider.SliderEndDistance = -PinchSlider.SliderStartDistance;
			}

			// Second, update background sprite size
			if (SliderBackground)
				SliderBackground.size = new Vector2(TotalLength, SliderBackground.size.y);

			// Third, update slider thumb
			if (SliderThumb && FitThumbToMaxScroll)
				SliderThumb.size = new Vector2(sliderThumbLength, SliderThumb.size.y);

			if (SliderThumbCollider)
				SliderThumbCollider.size = new Vector3(
					sliderThumbLength + ColliderLengthwisePadding,
					SliderThumbCollider.size.y,
					SliderThumbCollider.size.z);
		}

		/// <summary>
		/// Calculate the slider thumb length.
		/// </summary>
		/// <returns></returns>
		public float CalculateThumbLength()
		{
			// Check if background padding is valid
			float bgPadding = ValidateBGPadding();

			if (FitThumbToMaxScroll)
			{
				// Fitting calculation

				// Total slider length minus padding
				float usableLength = TotalLength - (bgPadding * 2);

				// What the scroll bar length would be if there was no minimum thumb size
				// (which is subtracting one thumb size from the usableLength, accounted for MaxScrollValue, then factoring in the ScrollLengthFactor)
				float reducedLength = (usableLength - (usableLength / (MaxScrollValue + 1))) * scrollLengthFactor;

				// Thumb length is basically twice the gap between the start/end of the scrolling area to the edge (minus padding ofc)
				float thumbLength = usableLength - reducedLength;

				// Check if the thumb size is smaller than the minimum size
				if (thumbLength > minSliderThumbLength)
				{
					// If it is bigger than min size, we can use that
					return thumbLength;
				}
				else
				{
					// If it is smaller than min size, we use the min size
					return minSliderThumbLength;
				}
			}

			// If we are not resizing anything, we just use the thumb length as is
			return SliderThumb.size.x;
		}

		/// <summary>
		/// Calculate the slider length from start to end.
		/// </summary>
		/// <returns></returns>
		public float CalculateActualSliderLength()
		{
			// Check if background padding is valid
			float bgPadding = ValidateBGPadding();

			// Total slider length minus padding
			float usableLength = TotalLength - (bgPadding * 2);

			// We can get the actual slider length from the thumb length
			return usableLength - CalculateThumbLength();
		}

		public float ValidateBGPadding()
		{
			if (2 * backgroundLengthPadding < TotalLength)
				return backgroundLengthPadding;

			Debug.LogWarning($"[{name}] Slider background padding is greater than the total length of the slider, thus invalid. It will be ignored.");
			return 0.0f;
		}

		#endregion

		#region Event Callbacks

		private void OnValueChanged(SliderEventData eventData)
		{
			if (!roundSliderValueToInteger)
				return;

			int roundedValue = Mathf.RoundToInt(eventData.NewValue * MaxScrollValue);
			if (roundedValue != prevRoundedValue)
			{
				prevRoundedValue = roundedValue;
				OnIntegerValueChanged.Invoke(roundedValue);
			}
		}

		#endregion
	}
}