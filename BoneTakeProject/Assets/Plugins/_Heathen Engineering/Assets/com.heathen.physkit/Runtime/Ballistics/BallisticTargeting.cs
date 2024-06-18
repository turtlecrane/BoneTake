﻿#if HE_SYSCORE

using UnityEngine;

namespace HeathenEngineering.PhysKit
{
    [RequireComponent(typeof(BallisticAim))]
    public class BallisticTargeting : MonoBehaviour
    {
        [Tooltip("The target to aim at")]
        public Transform targetTransform;
        [Tooltip("If provided the tool will account for target movement")]
        public Rigidbody targetBody;
        public bool HasSolution { get; private set; }

        private BallisticAim ballisticAim;

        private void Start()
        {
            ballisticAim = GetComponent<BallisticAim>();
        }

        private void LateUpdate()
        {
            if(targetTransform != null) 
            { 
                if(targetBody != null)
                    HasSolution = ballisticAim.Aim(targetTransform.position, targetBody.velocity);
                else
                    HasSolution = ballisticAim.Aim(targetTransform.position);
            }
        }
    }
}

#endif