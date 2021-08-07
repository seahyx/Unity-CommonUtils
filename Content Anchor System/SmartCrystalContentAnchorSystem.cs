using Microsoft.MixedReality.Toolkit.UI.BoundsControl;
using Microsoft.MixedReality.Toolkit.Input;
using Microsoft.MixedReality.Toolkit.UI;
using Microsoft.MixedReality.Toolkit.Utilities;
using Microsoft.MixedReality.Toolkit.Utilities.Solvers;
using Sirenix.OdinInspector;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;

namespace HelloHolo.Framework.UI.ContentAnchorSystem
{
	/// <summary>
	/// Content Anchor System that uses a grabbable "Crystal" to achieve placement
	/// with a plane representing the content as a guide.
	/// 
	/// This is an altered version of the <see cref="CrystalContentAnchorSystem"/>. It attempts to align the content anchor to the floor by doing a raycast to the spatial mesh. The height of the content anchor will be set to a <see cref="UIReference"/>'s <see cref="GameObject"/> as reference.
	/// </summary>
	public class SmartCrystalContentAnchorSystem : ContentAnchorSystem
	{
		#region Constants

		/// <summary>
		/// The minimum depth and width of the content planes.
		/// </summary>
		private const float MinContentDimension = 0.2f;

		/// <summary>
		/// The height of the alignment line.
		/// </summary>
		private const float AlignmentLineDepth = 0.01f;

		/// <summary>
		/// The delay duration before the EnsureComponent is checked.
		/// </summary>
		private const int DelayedEnsureComponentTime = 250;

		#endregion

		#region Serialization

		[Title("Configuration - Content Plane")]

		[Tooltip("The thickness of the Alignment Line.")]
		[SerializeField]
		[Range(MinContentDimension, 10.0f)]
		[OnValueChanged("onContentWidthAdjustedInspector")]
		private float contentWidth = 1.0f;

		[Tooltip("The thickness of the Alignment Line (in Y and Z axes). Try not to make it too thick.")]
		[SerializeField]
		[Range(0, 10)]
		[OnValueChanged("onContentWidthAdjustedInspector")]
		private float contentThickness = 0.01f;

		[Tooltip("Anchor offset from the handle.")]
		[OnValueChanged("onAnchorOffsetAdjustedInspector")]
		[SerializeField]
		private Vector3 anchorOffset = Vector3.zero;

		[Tooltip("Anchor border size.")]
		[OnValueChanged("onBorderSizeAdjustedInspector")]
		[SerializeField]
		private Vector3 borderSize = Vector3.one;

		[Tooltip("Whether to use Border Offset's position rather than the outline position. Border Offset will only stay in the same position as the handle, while Border Outline takes into account the position of the guiding hologram from the handle.")]
		[SerializeField]
		private bool useBorderOffset = false;


		[Title("Configuration - Animation")]

		[Tooltip("The duration that should be taken for the interactive elements (grab handle, tooltip and control panel) to grow.")]
		[SerializeField]
		[Range(0, 10)]
		private float interactiveAnimationDuration = 0.5f;

		[Tooltip("The animation curve for the interactive elements (grab handle, tooltip and control panel).")]
		[SerializeField]
		private AnimationCurve interactiveAnimationCurve = AnimationCurve.EaseInOut(0.0f, 0.0f, 1.0f, 1.0f);

		[Tooltip("The duration that should be taken for the alignment line to extend.")]
		[SerializeField]
		[Range(0, 10)]
		private float alignmentLineAnimationDuration = 0.5f;

		[Tooltip("The animation curve for the alignment line.")]
		[SerializeField]
		private AnimationCurve alignmentLineAnimationCurve = AnimationCurve.EaseInOut(0.0f, 0.0f, 1.0f, 1.0f);

		[Tooltip("The duration that should be taken for the border to extend.")]
		[SerializeField]
		[Range(0, 10)]
		private float borderAnimationDuration = 0.5f;

		[Tooltip("The animation curve for the border.")]
		[SerializeField]
		private AnimationCurve borderAnimationCurve = AnimationCurve.EaseInOut(0.0f, 0.0f, 1.0f, 1.0f);


		[Title("Configuration - Anchoring")]

		[Tooltip("Maximum raycast length.")]
		[SerializeField]
		[Range(0, 10)]
		private float maxRaycastLength = 3.0f;

		[Tooltip("Raycast layer mask.")]
		[SerializeField]
		private LayerMask layerMask;

		[Tooltip("Fallback anchor height. This is the assumed height of the grab handle from the floor if raycast does not succeed.")]
		[SerializeField]
		[Range(0, 5)]
		private float fallbackHeight = 1.5f;


		[Title("References")]

		[Tooltip("Reference to the object that the user is supposed to grab to move the object around.")]
		[SerializeField]
		[SceneObjectsOnly]
		private GameObject grabHandle;

		[Tooltip("Reference to the blue line that indicates the alignmentof the content.")]
		[SerializeField]
		[SceneObjectsOnly]
		private GameObject alignmentLine;

		[Tooltip("Reference to the 'Grab Me' tooltip that directs the user what to do.")]
		[SerializeField]
		[SceneObjectsOnly]
		private GameObject grabTooltip;

		[Tooltip("Reference to the control panel that holds the buttons for users to confirm the placement.")]
		[SerializeField]
		[SceneObjectsOnly]
		private GameObject controlPanel;

		[Tooltip("Reference to the object border offset, which should be the parent of the border outline.")]
		[SerializeField]
		[SceneObjectsOnly]
		private GameObject borderOffset;

		[Tooltip("Reference to the object border outline.")]
		[SerializeField]
		[SceneObjectsOnly]
		private GameObject borderOutline;

		[Tooltip("UI height reference. This GameObject will preserve the height of the anchor that the user has set.")]
		[SerializeField]
		private UIReference UIHeightRef;

		#endregion

		#region Member Declarations

		/// <summary>
		/// Keeps track of the handle's size.
		/// </summary>
		private Vector3 handleMaxSize;

		/// <summary>
		/// Keeps track of the tooltip's size.
		/// </summary>
		private Vector3 tooltipMaxSize;

		/// <summary>
		/// Keeps track of the control panel's size.
		/// </summary>
		private Vector3 controlPanelMaxSize;

		/// <summary>
		/// Keeps track of the border's Y scale.
		/// </summary>
		private float borderMaxHeight;

		/// <summary>
		/// Whether a full grab (start + end) has been done at least once.
		/// </summary>
		private bool firstGrabCompleted = false;


		/// <summary>
		/// <see cref="BoundsControl"/> reference.
		/// </summary>
		public BoundsControl BoundsControl { get; private set; }

		/// <summary>
		/// <see cref="ObjectManipulator"/> reference.
		/// </summary>
		public ObjectManipulator ObjectManipulator { get; private set; }

		/// <summary>
		/// <see cref="RadialView"/> reference.
		/// </summary>
		public RadialView RadialView { get; private set; }


		/// <summary>
		/// Whether or not the anchoring is in progress.
		/// </summary>
		public override bool IsAnchoring => gameObject.activeSelf;

		#endregion

		#region MonoBehaviour

		/// <summary>
		/// Resource initialization.
		/// </summary>
		private void Awake()
		{
			// Get Components
			ObjectManipulator = DebugUtility.AssertComponentInChildren<ObjectManipulator>(this);
			RadialView = DebugUtility.AssertComponent<RadialView>(this);
			BoundsControl = DebugUtility.AssertComponentInChildren<BoundsControl>(this);

			// Error Checks
			DebugUtility.AssertNotNull(this, grabHandle);
			DebugUtility.AssertNotNull(this, alignmentLine);
			DebugUtility.AssertNotNull(this, grabTooltip);
			DebugUtility.AssertNotNull(this, controlPanel);
			DebugUtility.AssertNotNull(this, borderOffset);

			// Disable scale handles on start
			BoundsControl.ScaleHandlesConfig.ShowScaleHandles = false;

			// Save the original sizes of certain elements for scaling animations later
			handleMaxSize = grabHandle.transform.localScale;
			tooltipMaxSize = grabTooltip.transform.localScale;
			controlPanelMaxSize = controlPanel.transform.localScale;
			borderMaxHeight = borderOffset.transform.localScale.y;
		}

		/// <summary>
		/// Initialization.
		/// </summary>
		protected override void Start()
		{
			base.Start();

			// Add a listener 
			// -- Define the delegate for on manipulation start
			UnityAction<ManipulationEventData> onUserGrabbedAction = null;
			onUserGrabbedAction = new UnityAction<ManipulationEventData>(delegate (ManipulationEventData eventData)
			{
				if (firstGrabCompleted)
				{
					return;
				}

				// Begin opening animation
				_ = playOpeningAnimation();

				// Disable radial view
				RadialView.enabled = false;
			});
			// -- Add the delegate in
			ObjectManipulator.OnManipulationStarted.AddListener(onUserGrabbedAction);

			// -- Define the delegate for on manipulation end
			UnityAction<ManipulationEventData> onUserReleaseAction = null;
			onUserReleaseAction = new UnityAction<ManipulationEventData>(delegate (ManipulationEventData eventData)
			{
				if (firstGrabCompleted)
				{
					return;
				}

				// Show handles for controlling
				BoundsControl.enabled = true;

				// HACK: Ensure that a NearInteractionGrabbable is available so that when it is destroyed by BoundsControl.OnEnable(), we still have one to use with
				_ = grabHandle.EnsureComponentAsync<NearInteractionGrabbable>(DelayedEnsureComponentTime);

				// Begin control panel unveiling animation
				_ = playControlPanelShowAnimation();

				// Flag that first grab was completed
				firstGrabCompleted = true;
			});
			// -- Add the delegate in
			ObjectManipulator.OnManipulationEnded.AddListener(onUserReleaseAction);
		}

		/// <summary>
		/// Update the border position.
		/// </summary>
		private void Update()
		{
			// Make sure it is after first grab
			if (firstGrabCompleted)
			{
				borderOffset.transform.position = GetFloorAnchorPosition();
			}
		}

		#endregion

		#region Content Anchor System

		/// <summary>
		/// Called when the anchoring process is started. Use this function to set up the anchoring process
		/// </summary>
		protected override void onAnchoringStarted()
		{
			// Unflag first grab
			firstGrabCompleted = false;

			// Set the default states of the content plane and alignment lines
			alignmentLine.SetActive(false);
			grabHandle.SetActive(false);
			grabTooltip.SetActive(false);
			controlPanel.SetActive(false);
			borderOffset.SetActive(false);

			// Deactivate rotational anchors at the start
			BoundsControl.enabled = false;

			// HACK: Ensure that a NearInteractionGrabbable is available so that when it is destroyed by BoundsControl.OnDisable(), we still have one to use with
			_ = grabHandle.EnsureComponentAsync<NearInteractionGrabbable>(DelayedEnsureComponentTime);

			// Set the default position at the start
			transform.position = CameraCache.Main.transform.position + CameraCache.Main.transform.forward * (RadialView.MinDistance + (RadialView.MaxDistance - RadialView.MinDistance));

			// Enable radial view
			RadialView.enabled = true;

			// Begin animating the initial handle appearance
			_ = playHandleAppearAnimation();
		}

		/// <summary>
		/// Called when the anchoring process is completed. Use this function to clean up the anchoring process.
		/// Casts a raycast from the main camera to the spatial mesh on the floor to align content to the floor.
		/// </summary>
		protected override void onAnchoringEnded()
		{
			// Initialize anchor world position
			borderOffset.transform.position = GetFloorAnchorPosition(true);

			// Set the UI height reference
			if (UIHeightRef)
			{
				UIHeightRef.transform.position = grabHandle.transform.position;
				UIHeightRef.transform.rotation = grabHandle.transform.rotation;

				UIHeightRef.SetCounterPosition(borderOffset.transform.position);
			}

			Vector3 finalAnchorPosition;
			if (useBorderOffset)
			{
				// We want the X and Z position of the handle, but the Y position of the floor
				finalAnchorPosition = borderOffset.transform.position;
			}
			else
			{
				// We want the position that is the center of the border hologram, which we can get from the position of the border offset child.
				finalAnchorPosition = borderOffset.transform.GetChild(0).position;
			}

			SetAnchor(transform.parent.InverseTransformVector(finalAnchorPosition), transform.localRotation);
		}

		#endregion

		#region Inspector Functions

		/// <summary>
		/// Function that will be automatically adjusts the size of the content plane whent he value is adjusted in 
		/// the inspector.
		/// </summary>
		private void onContentWidthAdjustedInspector()
		{
			if (alignmentLine == null)
				return;

			// Adjust alignment line
			alignmentLine.transform.localScale = new Vector3(contentWidth, contentThickness, AlignmentLineDepth);
		}

		/// <summary>
		/// Inspector function to adjust the offset of the border outline when the anchor offset is changed.
		/// </summary>
		private void onAnchorOffsetAdjustedInspector()
		{
			if (borderOutline == null)
				return;

			borderOutline.transform.localPosition = anchorOffset;
		}

		/// <summary>
		/// Inspector function to adjust the scale of the border outline when the border size is changed.
		/// </summary>
		private void onBorderSizeAdjustedInspector()
		{
			if (borderOutline == null)
				return;

			borderOutline.transform.localScale = borderSize;
		}

		#endregion

		#region Animation Functions

		/// <summary>
		/// Function for animating the crystal handle appearing
		/// </summary>
		/// <returns>Asynchronous Task Handler</returns>
		private async Task playHandleAppearAnimation()
		{
			// Set initial states of objects
			grabHandle.SetActive(true);
			grabHandle.transform.localScale = Vector3.zero;
			grabTooltip.SetActive(true);
			grabTooltip.transform.localScale = Vector3.zero;

			// Begin growing the grab handle
			float timer = 0.0f;
			while (timer < interactiveAnimationDuration)
			{
				// Increase the timer for tracking the animation
				timer += Time.deltaTime;

				// Calculate timer progress
				var progress = timer / interactiveAnimationDuration;

				// Calculate value from animation curve
				float t = interactiveAnimationCurve.Evaluate(progress);

				// Scale the grab handle
				grabHandle.transform.localScale = Vector3.Lerp(Vector3.zero, handleMaxSize, t);

				// Scale the tooltip
				grabTooltip.transform.localScale = Vector3.Lerp(Vector3.zero, tooltipMaxSize, t);

				// Wait for next frame
				await Task.Yield();
			}

			// Snap to final values
			grabHandle.transform.localScale = handleMaxSize;
			grabTooltip.transform.localScale = tooltipMaxSize;
		}

		/// <summary>
		/// Function for animating the tooltip hiding itself after the user grabs the handle
		/// </summary>
		/// <returns>Asynchronous Task Handler</returns>
		private async Task playHideTooltipAnimation()
		{
			// Begin growing the grab handle
			float timer = 0.0f;
			while (timer < interactiveAnimationDuration)
			{
				// Increase the timer for tracking the animation
				timer += Time.deltaTime;

				// Calculate timer progress
				var progress = timer / interactiveAnimationDuration;

				// Scale the tooltip
				grabTooltip.transform.localScale = Vector3.Lerp(tooltipMaxSize, Vector3.zero, progress);

				// Wait for next frame
				await Task.Yield();
			}

			// Hide the object
			grabTooltip.SetActive(false);
		}

		/// <summary>
		/// Asynchronous function for handling the content plane opening animation
		/// </summary>
		/// <returns>Asynchronous Task Handler</returns>
		private async Task playOpeningAnimation()
		{
			// Set initial states of objects
			alignmentLine.transform.localScale = new Vector3(0.0f, contentThickness, contentThickness);
			alignmentLine.SetActive(true);

			// Hide the grab tooltip too
			_ = playHideTooltipAnimation();

			// Begin extending the alignment line
			float timer = 0.0f;
			while (timer < alignmentLineAnimationDuration)
			{
				// Increase the timer for tracking the animation
				timer += Time.deltaTime;

				// Calculate value from animation curve
				float t = alignmentLineAnimationCurve.Evaluate(timer / alignmentLineAnimationDuration);

				// Scale the line
				alignmentLine.transform.localScale = new Vector3(Mathf.Lerp(0.0f, contentWidth, t), contentThickness, AlignmentLineDepth);

				// Wait for next frame
				await Task.Yield();
			}

			// Snap to final values
			alignmentLine.transform.localScale = new Vector3(contentWidth, contentThickness, AlignmentLineDepth);
		}

		/// <summary>
		/// Function for animating the control panel animation
		/// </summary>
		/// <returns>Asynchronous Task Handler</returns>
		private async Task playControlPanelShowAnimation()
		{
			// Set initial states of objects
			controlPanel.SetActive(true);
			controlPanel.transform.localScale = Vector3.zero;

			// Start the border animation
			_ = playBorderAnimation();

			// Begin growing the control panel
			float timer = 0.0f;
			while (timer < interactiveAnimationDuration)
			{
				// Increase the timer for tracking the animation
				timer += Time.deltaTime;

				// Calculate value from animation curve
				float t = interactiveAnimationCurve.Evaluate(timer / interactiveAnimationDuration);

				// Scale the grab handle
				controlPanel.transform.localScale = Vector3.Lerp(Vector3.zero, controlPanelMaxSize, t);

				// Wait for next frame
				await Task.Yield();
			}

			// Snap to final values
			controlPanel.transform.localScale = controlPanelMaxSize;
		}

		/// <summary>
		/// Asynchronous function for handling the border opening animation
		/// </summary>
		/// <returns>Asynchronous Task Handler</returns>
		private async Task playBorderAnimation()
		{
			// Set initial states of objects
			borderOffset.SetActive(true);
			borderOffset.transform.localScale = new Vector3(
				borderOffset.transform.localScale.x,
				0.0f,
				borderOffset.transform.localScale.z);

			// Begin animation
			float timer = 0.0f;
			while (timer < alignmentLineAnimationDuration)
			{
				// Increase the timer for tracking the animation
				timer += Time.deltaTime;

				// Calculate value from animation curve
				float t = borderAnimationCurve.Evaluate(timer / borderAnimationDuration);

				// Scale the line
				borderOffset.transform.localScale = new Vector3(
				borderOffset.transform.localScale.x,
				Mathf.Lerp(0.0f, borderMaxHeight, t),
				borderOffset.transform.localScale.z);

				// Wait for next frame
				await Task.Yield();
			}

			// Snap to final values
			borderOffset.transform.localScale = new Vector3(
				borderOffset.transform.localScale.x,
				borderMaxHeight,
				borderOffset.transform.localScale.z);
		}

		/// <summary>
		/// Asynchronous function for handling the post-placement confirmation animation
		/// </summary>
		/// <returns>Asynchronous Task Handler</returns>
		private async Task playClosingAnimation()
		{
			// Deactivate the bounds control first
			BoundsControl.enabled = false;

			// Begin shrinking the grab handle
			float timer = 0.0f;
			while (timer < interactiveAnimationDuration)
			{
				// Increase the timer for tracking the animation
				timer += Time.deltaTime;

				// Calculate timer progress
				var progress = timer / interactiveAnimationDuration;

				// Calculate value from animation curve
				float t = interactiveAnimationCurve.Evaluate(progress);

				// Scale the grab handle
				grabHandle.transform.localScale = Vector3.Lerp(handleMaxSize, Vector3.zero, t);

				// Scale the control panel
				controlPanel.transform.localScale = Vector3.Lerp(controlPanelMaxSize, Vector3.zero, t);

				// Wait for next frame
				await Task.Yield();
			}

			// Workaround to prevent errors from disabling a 0 scale object
			controlPanel.transform.localScale = controlPanelMaxSize;

			// Hide the objects
			grabHandle.SetActive(false);
			controlPanel.SetActive(false);

			// Proceed
			StopAnchoring();
		}

		#endregion

		#region Public Functions

		/// <summary>
		/// Function to be assigned to the confirm placement button
		/// </summary>
		public void OnConfirmButtonPress()
		{
			_ = playClosingAnimation();
		}

		/// <summary>
		/// Function to be assigned to the reset placement button
		/// </summary>
		public void OnResetButtonPress()
		{
			onAnchoringStarted();
		}

		#endregion

		#region Helper Functions

		/// <summary>
		/// <para>
		/// Gets the height of the floor below the player, and subtract it from the <see cref="borderOffset"/>'s world position.
		/// </para>
		/// <para>
		/// If there is no floor detected below player, it will try to detect a floor below the crystal anchor.
		/// </para>
		/// <para>
		/// If there is no floor detected below the crystal anchor, it will use <see cref="fallbackHeight"/> with the crystal as anchor.
		/// </para>
		/// </summary>
		/// <param name="isLogging">Whether to log results into the console.</param>
		/// <returns>World position of the floor position beneath the crystal.</returns>
		private Vector3 GetFloorAnchorPosition(bool isLogging = false)
		{
			// Initialize anchor world position
			Vector3 anchorPosition = new Vector3(
					borderOffset.transform.position.x,
					transform.position.y - fallbackHeight,
					borderOffset.transform.position.z);

			// Cast a ray from the main camera downwards to try and detect the floor.
			RaycastHit hit;
			if (Physics.Raycast(Camera.main.transform.position, Vector3.down, out hit, maxRaycastLength, layerMask))
			{
				// Set world anchor to the floor
				anchorPosition = new Vector3(
					borderOffset.transform.position.x,
					hit.point.y,
					borderOffset.transform.position.z);

				if (isLogging)
					Debug.Log($"[{name}] Has detected a raycast hit on the floor from the main camera with coordinates: {hit.point}");
			}
			else
			{
				if (isLogging)
					Debug.Log($"[{name}] Has failed to detect a raycast hit on the floor from the main camera. Attempting a raycast from the anchor crystal.");

				if (Physics.Raycast(transform.position, Vector3.down, out hit, maxRaycastLength, layerMask))
				{
					// Set world anchor to the floor
					anchorPosition = new Vector3(
						borderOffset.transform.position.x,
						hit.point.y,
						borderOffset.transform.position.z);

					if (isLogging)
						Debug.Log($"[{name}] Has detected a raycast hit on the floor from the anchor crystal with coordinates: {hit.point}");
				}
				else
				{
					if (isLogging)
						Debug.Log($"[{name}] Has failed to detect a raycast hit on the floor from the anchor crystal. Using the fallback height of {fallbackHeight}.");
				}
			}

			return anchorPosition;
		}

		#endregion
	}
}