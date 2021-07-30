using UnityEngine;
using UnityEngine.Events;

namespace CommonUtils
{
    /// <summary>
    /// Helps to route OnTrigger events from this gameObject into public unity events.
    /// </summary>
    public class TriggerEventHelper : MonoBehaviour
    {
		#region Type Definitions

		/// <summary>
		/// Unity Event with a single <see cref="Collider"/> parameter.
		/// </summary>
		[System.Serializable]
		public class UnityTriggerEvent : UnityEvent<Collider> { }

		#endregion

		#region Serialization

		[Header("Events")]

		[Tooltip("Rigidbody OnTriggerEnter event.")]
		[SerializeField]
		public UnityTriggerEvent OnTriggerEnterEvent = new UnityTriggerEvent();

		[Tooltip("Rigidbody OnTriggerExit event.")]
		[SerializeField]
		public UnityTriggerEvent OnTriggerExitEvent = new UnityTriggerEvent();

		#endregion

		#region Trigger Events

		private void OnTriggerEnter(Collider other)
		{
			OnTriggerEnterEvent.Invoke(other);
		}

		private void OnTriggerExit(Collider other)
		{
			OnTriggerExitEvent.Invoke(other);
		}

		#endregion
	}
}