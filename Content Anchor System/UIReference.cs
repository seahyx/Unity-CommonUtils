using System.Collections.Generic;
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

		[Tooltip("Height to counter against.")]
		[SerializeField]
		public Vector3 counterPosition = Vector3.zero;

		#endregion

		#region Public Functions

		/// <summary>
		/// Updates position of all Transforms in targets.
		/// </summary>
		public void SendPositionUpdate()
		{
			Debug.Log($"[{name}] Countering height difference between Grab Handle and final anchor position: {transform.position.y - counterPosition.y}");
			foreach (Transform target in targets)
			{
				target.transform.localPosition = new Vector3(
					target.transform.localPosition.x,
					transform.position.y - counterPosition.y,
					target.transform.localPosition.z);
			}
		}

		#endregion
	}
}
