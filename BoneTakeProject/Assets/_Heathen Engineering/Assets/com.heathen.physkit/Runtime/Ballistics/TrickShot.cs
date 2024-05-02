#if HE_SYSCORE

using System.Collections.Generic;
using UnityEngine;

namespace HeathenEngineering.PhysKit
{
    public class TrickShot : MonoBehaviour
    {
        public float speed;
        public float radius;
        public BallisticPathFollow template;
        public float resolution = 0.01f;
        public float distance = 10f;
        public LayerMask collisionLayers = 0;
        public int bounces = 0;
        public float bounceDamping = 0.5f;
        public bool distanceIsTotalLength = false;
        [System.Obsolete("Use prediction instead.")]
        [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
        public List<BallisticPath> perdiction => prediction;
        public List<BallisticPath> prediction = new List<BallisticPath>();

        Transform selfTransform;

        private void Start()
        {
            selfTransform = transform;
        }

        public void Shoot()
        {
            var GO = Instantiate(template.gameObject);
            var comp = GO.GetComponent<BallisticPathFollow>();
            comp.projectile = new BallisticsData { velocity = selfTransform.forward * speed, radius = radius };
            comp.path = new List<BallisticPath>(prediction);
            GO.transform.SetPositionAndRotation(selfTransform.position, selfTransform.rotation);
        }

        [System.Obsolete("Use Predict() instead.")]
        [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
        public void Perdict() => Predict();
        public void Predict()
        {
            var projectileSettings = new BallisticsData { velocity = selfTransform.forward * speed, radius = radius };
            if (bounces == 0)
            {
                prediction.Clear();
                prediction.Add(projectileSettings.Perdict(selfTransform.position, null, resolution, distance, collisionLayers));
            }
            else if (distanceIsTotalLength)
            {
                prediction = new List<BallisticPath>();
                var current = projectileSettings.Perdict(selfTransform.position, null, resolution, distance, collisionLayers);
                prediction.Add(current);

                if (current.impact.HasValue
                    && bounces > 0)
                {
                    var project = projectileSettings;
                    var (position, velocity, time) = current.steps[current.steps.Length - 1];
                    project.velocity = Vector3.Reflect(velocity, current.impact.Value.normal);
                    project.velocity *= (1f - bounceDamping);
                    var remainingDistance = distance - current.flightDistance;

                    for (int i = 0; i < bounces; i++)
                    {
                        current = project.Perdict(position, current.impact.Value.collider, resolution, remainingDistance, collisionLayers);
                        prediction.Add(current);

                        if (current.impact.HasValue)
                        {
                            (position, velocity, time) = current.steps[current.steps.Length - 1];
                            project.velocity = Vector3.Reflect(velocity, current.impact.Value.normal);
                            project.velocity *= (1f - bounceDamping);
                            remainingDistance -= current.flightDistance;
                        }
                        else
                            break;

                    }
                }
            }
            else
            {
                prediction = new List<BallisticPath>();
                var current = projectileSettings.Perdict(selfTransform.position, null, resolution, distance, collisionLayers);
                prediction.Add(current);

                if (current.impact.HasValue
                    && bounces > 0)
                {
                    var project = projectileSettings;
                    var (position, velocity, time) = current.steps[current.steps.Length - 1];
                    project.velocity = Vector3.Reflect(velocity, current.impact.Value.normal);
                    project.velocity *= (1f - bounceDamping);

                    for (int i = 0; i < bounces; i++)
                    {
                        current = project.Perdict(position, current.impact.Value.collider, resolution, distance, collisionLayers);
                        prediction.Add(current);

                        if (current.impact.HasValue)
                        {
                            (position, velocity, time) = current.steps[current.steps.Length - 1];
                            project.velocity = Vector3.Reflect(velocity, current.impact.Value.normal);
                            project.velocity *= (1f - bounceDamping);
                        }
                        else
                            break;

                    }
                }
            }
        }

        public bool Aim(Vector2 target)
        {
            var projectileSettings = new BallisticsData { velocity = selfTransform.up * speed, radius = radius };
            return projectileSettings.Aim(selfTransform.position, target);
        }
        public bool AimHigh(Vector2 target)
        {
            var projectileSettings = new BallisticsData { velocity = selfTransform.up * speed, radius = radius };
            return projectileSettings.AimHigh(selfTransform.position, target);
        }
    }
}


#endif