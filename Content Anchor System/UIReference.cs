using System.Collections.Generic;
using CommonUtils.Networking;
using Photon.Pun;
using Sirenix.OdinInspector;
using UnityEngine;

namespace HelloHolo.Framework.UI.ContentAnchorSystem
{
	/// <summary>
	/// Updates the Y position of objects to counter the position difference between the anchor height and the floor height.
	/// Useful if the rest of the content is anchored at a different height from the user's preferred UI height position.
	/// </summary>
	public class UIReference : MonoBehaviour
	{
		#region Serialization

		[InfoBox("Assign GameObjects to the list below to have its height aligned to this GameObject whenever the height on this object is updated.")]

		[Title("Configuration")]

		[Tooltip("Target GameObject list. These GameObjects will have their heights aligned to this GameObject.")]
		[SerializeField]
		private List<Transform> targets = new List<Transform>();

		#endregion

		#region Member Declarations

		/// <summary>
		/// Height to counter against.
		/// </summary>
		public Vector3 CounterPosition { get; private set; }

		#endregion

		#region Public Functions

		public void SetCounterPosition(Vector3 counterPosition, bool isNetwork = false)
		{
			CounterPosition = counterPosition;

			SendPositionUpdate();
		}

		/// <summary>
		/// Updates position of all <see cref="Transform"/>s in <see cref="targets"/>.
		/// </summary>
		public void SendPositionUpdate()
		{
			Debug.Log($"[{name}] Countering height difference between Grab Handle and final anchor position: {transform.position.y - CounterPosition.y}");
			foreach (Transform target in targets)
			{
				target.transform.localPosition = new Vector3(
					target.transform.localPosition.x,
					transform.position.y - CounterPosition.y,
					target.transform.localPosition.z);
			}
		}

		#endregion
	}
}
