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

namespace CommonUtils.Networking
{
	/// <summary>
	/// Easy photon takeover utility for <see cref="ObjectManipulator"/>. Will automatically add itself into <see cref="ObjectManipulator"/> events and allow players to takeover and move objects.
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

		#region MonoBehaviour

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

		#endregion

		private void TakeOwnership()
		{
			Debug.Log($"[{name}] Photon takeover, requesting ownership for: {objManipulator.HostTransform.name}");

			PhotonView photonView;
			if (objManipulator.HostTransform.TryGetComponent(out photonView))
				photonView.TransferOwnership(PhotonNetwork.LocalPlayer);
		}

		private void TakeOwnership(ManipulationEventData arg0)
		{
			Debug.Log($"[{name}] Photon takeover, requesting ownership for: {objManipulator.HostTransform.name}");

			PhotonView photonView;
			if (objManipulator.HostTransform.TryGetComponent(out photonView))
				photonView.TransferOwnership(PhotonNetwork.LocalPlayer);
		}
	}
}
