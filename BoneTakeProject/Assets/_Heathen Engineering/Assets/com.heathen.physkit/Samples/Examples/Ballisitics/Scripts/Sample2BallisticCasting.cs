#if HE_SYSCORE

using HeathenEngineering.PhysKit;
using HeathenEngineering.PhysKit.API;
using UnityEngine;

namespace HeathenEngineering.Demos
{
    [System.Obsolete("This script is for demonstration purposes ONLY")]
    public class Sample2BallisticCasting : MonoBehaviour
    {
        public float mouseSpeed = 1f;
        public float throwSpeed = 0.5f;
        public TrickShot trickShot;

        private Transform selfTransform;
        private Quaternion originalRotation;

        private void Start()
        {
            selfTransform = transform;
            originalRotation = selfTransform.localRotation;
        }

        private void Update()
        {
            // Read the mouse input axis
            if (Input.GetMouseButton(1))
            {
                var rotationX = Input.GetAxis("Mouse X") * mouseSpeed;
                var rotationY = Input.GetAxis("Mouse Y") * mouseSpeed;
                Quaternion xQuaternion = Quaternion.AngleAxis(rotationX, Vector3.up);
                Quaternion yQuaternion = Quaternion.AngleAxis(rotationY, -Vector3.right);
                selfTransform.localRotation = selfTransform.localRotation * xQuaternion * yQuaternion;
                selfTransform.LookAt(selfTransform.position + selfTransform.forward);
            }
            else if (!Input.GetKey(KeyCode.Space))
            {
                var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                if(Physics.Raycast(ray, out var hit))
                {
                    if (Ballistics.Solution(trickShot.transform.position, trickShot.speed, hit.point, Physics2D.gravity.magnitude, out Quaternion low, out Quaternion _) >= 1)
                        trickShot.transform.rotation = low;
                }
            }

            if (Input.GetMouseButtonDown(0))
                trickShot.Shoot();
        }
    }
}

#endif