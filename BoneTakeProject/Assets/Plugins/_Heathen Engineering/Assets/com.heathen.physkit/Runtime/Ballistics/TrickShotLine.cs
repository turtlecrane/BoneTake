#if HE_SYSCORE

using System.Collections.Generic;
using UnityEngine;

namespace HeathenEngineering.PhysKit
{
    [RequireComponent(typeof(LineRenderer))]
    [RequireComponent(typeof(TrickShot))]
    public class TrickShotLine : MonoBehaviour
    {
        private TrickShot trickShot;
        private LineRenderer lineRenderer;
        public bool runOnStart = true;
        public bool continiousRun = true;

        private void Start()
        {
            lineRenderer = GetComponent<LineRenderer>();
            lineRenderer.useWorldSpace = true;

            trickShot = GetComponent<TrickShot>();
        }

        private void LateUpdate()
        {
            if (continiousRun)
                trickShot.Perdict();

            List<Vector3> trajectory = new List<Vector3>();
            foreach (var path in trickShot.perdiction)
            {
                foreach (var step in path.steps)
                    trajectory.Add(step.position);
            }

            lineRenderer.positionCount = trajectory.Count;
            lineRenderer.SetPositions(trajectory.ToArray());
        }
    }
}


#endif