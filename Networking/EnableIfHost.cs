/**
 *	Common Utilities Library for developing in Unity.
 *	https://github.com/seahyx/Unity-CommonUtils
 *
 *	MIT License
 *
 *	Copyright (c) 2021 Seah Ying Xiang
 */

using Photon.Pun;
using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEngine;

namespace CommonUtils.Networking
{
	/// <summary>
	/// Enables/disables <see cref="GameObject"/>s depending on whether the client is a host (i.e. Master Client) or not.
	/// </summary>
	public class EnableIfHost : MonoBehaviour
	{
		#region Serialization

		[Title("Configuration")]

		[Tooltip("List of GameObjects to enable/activate if the client is the host, and vice versa.")]
		[SerializeField]
		private List<GameObject> hostEnableList = new List<GameObject>();

		[Tooltip("List of GameObjects to disable/deactivate if the client is the host, and vice versa.")]
		[SerializeField]
		private List<GameObject> hostDisableList = new List<GameObject>();

		[Tooltip("Check lists whenever this GameObject is enabled.")]
		[SerializeField]
		private bool checkOnEnable = true;

		#endregion

		#region MonoBehaviour

		private void OnEnable()
		{
			if (checkOnEnable)
				CheckLists();
		}

		#endregion

		#region Public Functions

		/// <summary>
		/// Check the enable and disable lists and updates the <see cref="GameObject"/>s accordingly.
		/// </summary>
		public void CheckLists()
		{
			bool isMasterClient = true;

			if (PhotonNetwork.InRoom)
			{
				if (PhotonNetwork.IsMasterClient)
					Debug.Log($"[{name}] The client is the host, toggling GameObjects respectively.");
				else
					Debug.Log($"[{name}] The client is not the host, toggling GameObjects respectively.");

				isMasterClient = PhotonNetwork.IsMasterClient;
			}

			foreach (GameObject obj in hostEnableList)
			{
				obj.SetActive(isMasterClient);
			}

			foreach (GameObject obj in hostDisableList)
			{
				obj.SetActive(!isMasterClient);
			}
		}

		#endregion
	}
}
