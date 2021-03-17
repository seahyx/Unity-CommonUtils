using Sirenix.OdinInspector;
using UnityEngine;

namespace CommonUtils
{
	/// <summary>
	/// Aligns the GameObject this script is attached to, to the height of a reference GameObject on OnEnable.
	/// </summary>
	public class AlignToReference : MonoBehaviour
	{
		#region Serialization

		[Title("Configuration")]

		[Tooltip("Target to adjust alignment. Leave empty to reference this GameObject.")]
		[SerializeField]
		private GameObject target;

		[Tooltip("The reference GameObject.")]
		[SerializeField]
		[Required]
		private GameObject reference;

		[Tooltip("If toggled, this script will disable itself after the initial setting.")]
		[SerializeField]
		private bool singleUse = true;

		#endregion

		#region MonoBehaviour

		/// <summary>
		/// Align to reference.
		/// </summary>
		private void OnEnable()
		{
			if (!target)
				target = gameObject;

			target.transform.position = new Vector3(
				transform.position.x,
				reference.transform.position.y,
				transform.position.z);

			// Disable after use if enabled
			if (singleUse)
				gameObject.GetComponent<AlignToReference>().enabled = false;
		}

		#endregion
	}
}
