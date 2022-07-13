/**
 *	Common Utilities Library for developing in Unity.
 *	https://github.com/seahyx/Unity-CommonUtils
 *
 *	MIT License
 *
 *	Copyright (c) 2021 Seah Ying Xiang
 */

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace CommonUtils
{
    /// <summary>
    /// Exposes animation events as an <see cref="UnityEvent"/>.
    /// </summary>
    public class AnimationEventRelay : MonoBehaviour
    {
        [SerializeField, Tooltip("If enabled, the integer value of the animation event int parameter will correspond to the index of the UnityEvent in the event list.")]
        public bool UseIntParamAsEventIndex = false;

        /// <summary>
        /// Unity Event with a single <see cref="AnimationEvent"/> parameter.
        /// </summary>
        [System.Serializable]
        public class UnityAnimationEvent : UnityEvent<AnimationEvent> { }

        public List<UnityAnimationEvent> UnityAnimEvent = new List<UnityAnimationEvent>();

        public void OnAnimationEvent(AnimationEvent animEvent)
		{
            if (!UseIntParamAsEventIndex)
                UnityAnimEvent[0]?.Invoke(animEvent);
            else
                UnityAnimEvent[animEvent.intParameter]?.Invoke(animEvent);
        }
    }
}
