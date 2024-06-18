#if HE_SYSCORE

using HeathenEngineering.PhysKit.API;
using Unity.Mathematics;
using UnityEngine;

namespace HeathenEngineering.PhysKit
{
    public class BallisticAim : MonoBehaviour
    {
        public float initialVelocity;
        [SerializeField]
        private Transform yPivot;
        [SerializeField]
        private Transform xPivot;
        public Vector2 yLimit = new Vector2(-180, 180);
        public Vector2 xLimit = new Vector2(-180, 180);

        public bool Aim(Vector3 target)
        {
            if (Ballistics.Solution(yPivot.position, initialVelocity, target, Physics.gravity.magnitude, out Quaternion lowAngle, out Quaternion highAngle) > 0)
            {
                var ret = true;
                var eular = lowAngle.eulerAngles;
                eular.x = 0;
                
                if (eular.y > 180)
                    eular.y -= 360;
                else if (eular.y < -180)
                    eular.y += 360;

                if (eular.y < yLimit.x || eular.y > yLimit.x)
                {
                    eular.y = math.clamp(eular.y, yLimit.x, yLimit.y);
                    ret = false;
                }
                eular.z = 0;
                yPivot.rotation = Quaternion.Euler(eular);

                eular = lowAngle.eulerAngles;

                if (eular.x > 180)
                    eular.x -= 360;
                else if (eular.x < -180)
                    eular.x += 360;

                if (eular.x < xLimit.x || eular.x > xLimit.x)
                {
                    eular.x = math.clamp(eular.x, xLimit.x, xLimit.y);
                    ret = false;
                }

                if (eular.y > 180)
                    eular.y -= 360;
                else if (eular.y < -180)
                    eular.y += 360;

                if (eular.y < yLimit.x || eular.y > yLimit.x)
                {
                    eular.y = math.clamp(eular.y, yLimit.x, yLimit.y);
                    ret = false;
                }

                eular.z = 0;
                xPivot.rotation = Quaternion.Euler(eular);

                return ret;
            }
            else
                return false;
        }

        public bool Aim(Vector3 target, Vector3 targetVelocity)
        {
            if (Ballistics.Solution(yPivot.position, initialVelocity, target, Physics.gravity.magnitude, out Quaternion lowAngle, out Quaternion highAngle) > 0)
            {
                var ret = true;
                var eular = lowAngle.eulerAngles;
                eular.x = 0;

                if (eular.y > 180)
                    eular.y -= 360;
                else if (eular.y < -180)
                    eular.y += 360;

                if (eular.y < yLimit.x || eular.y > yLimit.x)
                {
                    eular.y = math.clamp(eular.y, yLimit.x, yLimit.y);
                    ret = false;
                }
                eular.z = 0;
                yPivot.rotation = Quaternion.Euler(eular);

                eular = lowAngle.eulerAngles;

                if (eular.x > 180)
                    eular.x -= 360;
                else if (eular.x < -180)
                    eular.x += 360;

                if (eular.x < xLimit.x || eular.x > xLimit.x)
                {
                    eular.x = math.clamp(eular.x, xLimit.x, xLimit.y);
                    ret = false;
                }

                if (eular.y > 180)
                    eular.y -= 360;
                else if (eular.y < -180)
                    eular.y += 360;

                if (eular.y < yLimit.x || eular.y > yLimit.x)
                {
                    eular.y = math.clamp(eular.y, yLimit.x, yLimit.y);
                    ret = false;
                }

                eular.z = 0;
                xPivot.rotation = Quaternion.Euler(eular);

                return ret;
            }
            else
                return false;
        }
    }
}

#endif