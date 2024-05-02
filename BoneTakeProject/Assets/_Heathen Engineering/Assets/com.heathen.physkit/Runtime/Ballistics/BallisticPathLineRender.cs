﻿#if HE_SYSCORE

using System.Collections.Generic;
using UnityEngine;

namespace HeathenEngineering.PhysKit
{
    [RequireComponent(typeof(LineRenderer))]
    public class BallisticPathLineRender : MonoBehaviour
    {
        [Header("Launch Settings")]
        public Vector3 start;
        public BallisticsData projectile;

        [Header("Behaviour Settings")]
        public bool runOnStart = true;
        public bool continiousRun = false;
        public LayerMask collisionLayers = 0;
        public float resolution = 0.1f;
        public float maxLength = 10f;
        public float maxBounces = 0;
        public float bounceDamping = 0.2f;

        private LineRenderer lineRenderer;

        private void Start()
        {
            lineRenderer = GetComponent<LineRenderer>();
            lineRenderer.useWorldSpace = true;

            if (runOnStart)
                Simulate();
        }

        private void LateUpdate()
        {
            if (continiousRun)
                Simulate();
        }

        public void Simulate()
        {
            resolution = Mathf.Max(resolution, 0.001f);

            if (projectile.Speed > 0)
            {
                var impacts = new List<RaycastHit>();
                var trajectory = new List<Vector3>();

                var result = projectile.Perdict(start, null, resolution, maxLength, collisionLayers);
                foreach(var step in result.steps)
                    trajectory.Add(step.position);
                
                if (result.impact.HasValue)
                {
                    impacts.Add(result.impact.Value);

                    if (maxBounces > 0)
                    {
                        var remainingLength = maxLength - result.flightDistance;
                        var project = projectile;
                        var (position, velocity, time) = result.steps[result.steps.Length - 1];
                        project.velocity = Vector3.Reflect(velocity, result.impact.Value.normal);
                        project.velocity *= (1f - bounceDamping);

                        for (int i = 0; i < maxBounces; i++)
                        {
                            if (remainingLength > 0)
                            {
                                result = project.Perdict(position, result.impact.Value.collider, resolution, remainingLength, collisionLayers);
                                foreach (var step in result.steps)
                                    trajectory.Add(step.position);
                                if (result.impact.HasValue)
                                    impacts.Add(result.impact.Value);
                                remainingLength -= result.flightDistance;

                                if (result.impact.HasValue)
                                {
                                    (position, velocity, time) = result.steps[result.steps.Length - 1];
                                    project.velocity = Vector3.Reflect(velocity, result.impact.Value.normal);
                                    project.velocity *= (1f - bounceDamping);
                                }
                                else
                                    break;
                            }
                        }
                    }
                }

                lineRenderer.positionCount = trajectory.Count;
                lineRenderer.SetPositions(trajectory.ToArray());
            }
        }
    }
}


#endif