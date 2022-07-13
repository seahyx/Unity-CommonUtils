/**
 *	Common Utilities Library for developing in Unity.
 *	https://github.com/seahyx/Unity-CommonUtils
 *
 *	MIT License
 *
 *	Copyright (c) 2021 Seah Ying Xiang
 */

using UnityEngine;
using UnityEngine.Events;

namespace CommonUtils
{
    /// <summary>
    /// Exposes collision and trigger events as an <see cref="UnityEvent"/>.
    /// </summary>
    public class ColliderEventRelay : MonoBehaviour
    {
        /// <summary>
        /// Unity Event with a single <see cref="Collision"/> parameter.
        /// </summary>
        [System.Serializable]
        public class UnityCollisionEvent : UnityEvent<Collision> { }

        /// <summary>
        /// Unity Event with a single <see cref="Collider"/> parameter.
        /// </summary>
        [System.Serializable]
        public class UnityColliderEvent : UnityEvent<Collider> { }

        public UnityCollisionEvent OnCollisionEnterEvent = new UnityCollisionEvent();
        public UnityCollisionEvent OnCollisionExitEvent = new UnityCollisionEvent();
        public UnityCollisionEvent OnCollisionStayEvent = new UnityCollisionEvent();

        public UnityColliderEvent OnTriggerEnterEvent = new UnityColliderEvent();
        public UnityColliderEvent OnTriggerExitEvent = new UnityColliderEvent();
        public UnityColliderEvent OnTriggerStayEvent = new UnityColliderEvent();

		protected void OnCollisionEnter(Collision collision)
		{
            OnCollisionEnterEvent.Invoke(collision);
		}
		protected void OnCollisionExit(Collision collision)
		{
            OnCollisionExitEvent.Invoke(collision);
		}
		protected void OnCollisionStay(Collision collision)
		{
            OnCollisionStayEvent.Invoke(collision);
		}

		protected void OnTriggerEnter(Collider other)
		{
            OnTriggerEnterEvent.Invoke(other);
		}
		protected void OnTriggerExit(Collider other)
		{
            OnTriggerExitEvent.Invoke(other);
		}
		protected void OnTriggerStay(Collider other)
		{
            OnTriggerStayEvent.Invoke(other);
		}
	}
}