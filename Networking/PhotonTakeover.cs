/**
 *	Common Utilities Library for developing in Unity.
 *	https://github.com/seahyx/Unity-CommonUtils
 *
 *	MIT License
 *
 *	Copyright (c) 2021 Seah Ying Xiang
 */

using Microsoft.MixedReality.Toolkit.UI;
using Microsoft.MixedReality.Toolkit.UI.BoundsControl;
using Photon.Pun;
using UnityEngine;

namespace GEM.Networking
{
	/// <summary>
	/// Network takeover for object manipulator components.
	/// </summary>
	public class PhotonTakeover : MonoBehaviour
	{
		#region Serialization

		[Tooltip("Object Manipulator component. Leave empty to reference the component on this object.")]
		[SerializeField]
		private ObjectManipulator objManipulator;

		[Tooltip("Bounds Control component. Leave empty to reference the component on this object.")]
		[SerializeField]
		private BoundsControl boundsControl;

		#endregion

		public void Start()
		{
			if (objManipulator == null)
				objManipulator = GetComponent<ObjectManipulator>();

			Debug.Assert(objManipulator, $"[{name}] Object Manipulator cannot be found on this object!");

			objManipulator.OnManipulationStarted.AddListener(TakeOwnership);

			if (boundsControl == null)
				boundsControl = GetComponent<BoundsControl>();

			if (boundsControl != null)
			{
				boundsControl.RotateStarted.AddListener(TakeOwnership);
				boundsControl.ScaleStarted.AddListener(TakeOwnership);
				boundsControl.TranslateStarted.AddListener(TakeOwnership);
			}
		}
		private void TakeOwnership()
		{
			Debug.Log($"[{name}] Photon takeover, requesting ownership for: {objManipulator.HostTransform.name}");

			if (objManipulator.HostTransform.GetComponent<PhotonView>() != null)
				objManipulator.HostTransform.GetComponent<PhotonView>().TransferOwnership(PhotonNetwork.LocalPlayer);
		}

		private void TakeOwnership(ManipulationEventData arg0)
		{
			Debug.Log($"[{name}] Photon takeover, requesting ownership for: {objManipulator.HostTransform.name}");

			if (objManipulator.HostTransform.GetComponent<PhotonView>() != null)
				objManipulator.HostTransform.GetComponent<PhotonView>().TransferOwnership(PhotonNetwork.LocalPlayer);
		}
	}
}
