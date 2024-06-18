#if HE_SYSCORE

using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace HeathenEngineering.PhysKit.API
{
    /// <summary>
    /// A helper math library with commonly used physics funcitons.
    /// </summary>
    public static class Maths
    {
        /// <summary>
        /// <para>Find the point of intercept;</para>
        /// <para>This is a faster calculation than SafeIntercept but does not handle situations where there is no solution such as a target moving away faster than the interceptor moves toward</para>
        /// </summary>
        /// <remarks>
        /// <para>Fast intercept is appropreate for projectile to target such as a bullet to a character or vehicle where the bullet is substantially faster than the subject
        /// however for other intercept cases where the interceptor is nearer the speed of the interceptee this method may result in frequent misses in such cases
        /// use the SafeIntercept and test for null result indicating no solution</para>
        /// </remarks>
        /// <param name="position">the point in space to start from</param>
        /// <param name="speed">the speed of travel</param>
        /// <param name="targetSubject">the subject to target</param>
        /// <returns></returns>
        public static Vector3 FastIntercept(Vector3 position, float speed, Rigidbody targetSubject)
        {
            var distance = Vector3.Distance(position, targetSubject.position);
            return ValidateNaN(targetSubject.position + targetSubject.velocity * (distance / speed));
        }

        /// <summary>
        /// <para>Find the point of intercept;</para>
        /// <para>This is a faster calculation than SafeIntercept but does not handle situations where there is no solution such as a target moving away faster than the interceptor moves toward</para>
        /// </summary>
        /// <remarks>
        /// <para>Fast intercept is appropreate for projectile to target such as a bullet to a character or vehicle where the bullet is substantially faster than the subject
        /// however for other intercept cases where the interceptor is nearer the speed of the interceptee this method may result in frequent misses in such cases
        /// use the SafeIntercept and test for null result indicating no solution</para>
        /// </remarks>
        /// <param name="position">the point in space to start from</param>
        /// <param name="speed">the speed of travel</param>
        public static Vector3 FastIntercept(Vector3 position, float speed, Vector3 targetPosition, Vector3 targetVelocity)
        {
            var distance = Vector3.Distance(position, targetPosition);
            return ValidateNaN(targetPosition + targetVelocity * (distance / speed));
        }

        /// <summary>
        /// <para>Find the point of intercept;</para>
        /// <para>This is a faster calculation than SafeIntercept but does not handle situations where there is no solution such as a target moving away faster than the interceptor moves toward</para>
        /// </summary>
        /// <remarks>
        /// <para>Fast intercept is appropreate for projectile to target such as a bullet to a character or vehicle where the bullet is substantially faster than the subject
        /// however for other intercept cases where the interceptor is nearer the speed of the interceptee this method may result in frequent misses in such cases
        /// use the SafeIntercept and test for null result indicating no solution</para>
        /// </remarks>
        /// <param name="position">the point in space to start from</param>
        /// <param name="speed">the speed of travel</param>
        public static Vector3 FastIntercept(Vector3 position, float speed, Vector3 targetPosition, Vector3 targetHeading, float targetSpeed)
        {
            var distance = Vector3.Distance(position, targetPosition);
            return ValidateNaN(targetPosition + (targetHeading * targetSpeed) * (distance / speed));
        }

        /// <summary>
        /// <para>Find the point of intercept if any</para>
        /// <para>This is a slower calculation than FastIntercept but indicates when no intercept solution is available.
        /// Use this when the interceptor's speed is at or below the speed of the target</para>
        /// </summary>
        /// <param name="position">The position of the interceptor</param>
        /// <param name="speed">The speed of the interceptor</param>
        /// <param name="targetSubject">The subject to be intercepted</param>
        /// <returns></returns>
        public static Vector3? SafeIntercept(Vector3 position, float speed, Rigidbody targetSubject)
        {
            Vector3? result = null;

            var localPosition = targetSubject.position - position;
            var closingVelocity = targetSubject.velocity - ((position - targetSubject.position).normalized * speed);

            var t = InterceptTime(speed, localPosition, closingVelocity);
            if(t > 0)
            {
                result = ValidateNaN(targetSubject.position + targetSubject.velocity * t);
            }

            return result;
        }

        /// <summary>
        /// <para>Find the point of intercept if any</para>
        /// <para>This is a slower calculation than FastIntercept but indicates when no intercept solution is available.
        /// Use this when the interceptor's speed is at or below the speed of the target</para>
        /// </summary>
        public static Vector3? SafeIntercept(Vector3 position, float speed, Vector3 targetPosition, Vector3 targetVelocity)
        {
            Vector3? result = null;

            var localPosition = targetPosition - position;
            var closingVelocity = targetVelocity - ((position - targetPosition).normalized * speed);

            var t = InterceptTime(speed, localPosition, closingVelocity);
            if (t > 0)
            {
                result = ValidateNaN(targetPosition + targetVelocity * t);
            }

            return result;
        }

        /// <summary>
        /// <para>Find the point of intercept if any</para>
        /// <para>This is a slower calculation than FastIntercept but indicates when no intercept solution is available.
        /// Use this when the interceptor's speed is at or below the speed of the target</para>
        /// </summary>
        public static Vector3? SafeIntercept(Vector3 position, float speed, Vector3 targetPosition, Vector3 targetHeading, float targetSpeed)
        {
            Vector3? result = null;

            var localPosition = targetPosition - position;
            var closingVelocity = (targetHeading * targetSpeed) - ((position - targetPosition).normalized * speed);

            var t = InterceptTime(speed, localPosition, closingVelocity);
            if (t > 0)
            {
                result = ValidateNaN(targetPosition + (targetHeading * targetSpeed) * t);
            }

            return result;
        }

        /// <summary>
        /// Calculates the time to intercept given the local position e.g. relative position and the closing velocity e.g. relative velocity
        /// </summary>
        /// <param name="speed">The speed of interceptor</param>
        /// <param name="localPosition">The local or 'relative' position of the target to interceptor</param>
        /// <param name="closingVelocity">The closing or 'relative' velocity between the interceptor and target</param>
        /// <returns>The time to intercept ... if 0 then no valid intercept solution exists</returns>
        public static float InterceptTime(float speed, Vector3 localPosition, Vector3 closingVelocity)
        {
            var sqrMag = closingVelocity.sqrMagnitude;

            //No solution
            if (sqrMag < 0.001f)
                return 0;

            var velDelta = sqrMag - speed * speed;

            //Handle similar speeds
            if(Mathf.Abs(velDelta) < 0.001f)
            {
                var t = -localPosition.sqrMagnitude / (2 * Vector3.Dot(closingVelocity, localPosition));
                return Mathf.Max(t, 0);
            }

            var quote = 2 * Vector3.Dot(closingVelocity, localPosition);
            var sqrPos = localPosition.sqrMagnitude;
            var determinant = quote * quote - 4 * velDelta * sqrPos;

            if(determinant > 0)
            {
                //Two possible solutions
                var t1 = (-quote + Mathf.Sqrt(determinant)) / (2f * velDelta);
                var t2 = (-quote - Mathf.Sqrt(determinant)) / (2f * velDelta);

                if (t1 > 0)
                {
                    if (t2 > 0)
                        return Mathf.Min(t1, t2);
                    else
                        return t1;
                }
                else
                    return Mathf.Max(t2, 0);
            }
            else if (determinant < 0)
            {
                //No solution
                return 0;
            }
            else
            {
                //One possible solution
                return Mathf.Max(-quote / (2 * velDelta), 0);
            }
        }

        /// <summary>
        /// Find the effect of drag on an object in motion through a volume
        /// </summary>
        /// <param name="DragCoefficient">Use DragCoefficients helper for common values</param>
        /// <param name="FluidDensity">Use VolumetricMassDensity helper for common values</param>
        /// <param name="FlowVelocity">Typically the inverse of the subject velocity</param>
        /// <param name="CrossSectionArea">Typically the orthagraphicly projected surface or 'lead face' of the subject i.e. exposed surface area in the direction of movement</param>
        /// <returns></returns>
        public static Vector3 QuadraticDrag(float DragCoefficient, float FluidDensity, Vector3 FlowVelocity, float CrossSectionArea)
        {
            return DragCoefficient * CrossSectionArea * 0.5f * FluidDensity * FlowVelocity.sqrMagnitude * FlowVelocity.normalized;
        }

        /// <summary>
        /// Find the drag on an object based on its speed through a volume
        /// </summary>
        public static float QuadraticDrag(float DragCoefficient, float FluidDensity, float speed, float CrossSectionArea)
        {
            return DragCoefficient * CrossSectionArea * 0.5f * FluidDensity * speed * speed;
        }

        /// <summary>
        /// Returns the force to be applied to achieve a given velocity
        /// </summary>
        /// <param name="body"></param>
        /// <param name="targetVelocity"></param>
        /// <returns></returns>
        public static Vector3 ForceToReachLinearVelocity(Rigidbody body, Vector3 targetVelocity)
        {
            return body.mass * ValidateNaN(((targetVelocity - body.velocity) / Time.fixedDeltaTime));
        }

        /// <summary>
        /// Returns the force to be applied to achieve a given angular velocity
        /// </summary>
        /// <param name="body"></param>
        /// <param name="targetVelocity"></param>
        /// <returns></returns>
        public static Vector3 ForceToReachAngularVelocity(Rigidbody body, Vector3 targetVelocity)
        {
            return body.mass * ValidateNaN(((targetVelocity - body.angularVelocity) / Time.fixedDeltaTime));
        }

        /// <summary>
        /// Returns an impulse force to rotate a body to align forward with a given direction
        /// </summary>
        /// <param name="body"></param>
        /// <param name="targetDirection"></param>
        /// <returns></returns>
        public static Vector3 TorqueToReachDirection(Rigidbody body, Vector3 targetDirection)
        {
            var x = Vector3.Cross(body.transform.forward, targetDirection.normalized);
            float th = Mathf.Asin(x.magnitude);
            var w = x.normalized * th / Time.fixedDeltaTime;

            var q = body.rotation * body.inertiaTensorRotation;
            return body.mass * ValidateNaN(((q * Vector3.Scale(body.inertiaTensor, Quaternion.Inverse(q) * w)) - body.angularVelocity));
        }

        /// <summary>
        /// Returns an impulse force to rotate a body to align to a given rotation
        /// </summary>
        /// <param name="body"></param>
        /// <param name="targetDirection"></param>
        /// <returns></returns>
        public static Vector3 TorqueToReachRotation(Rigidbody body, Quaternion targetRotation)
        {
            var targetDirection = targetRotation * Vector3.forward;
            return ValidateNaN(TorqueToReachDirection(body, targetDirection));
        }

        /// <summary>
        /// returns the intermediate position simulating constant movement at speed from position toward target
        /// </summary>
        /// <param name="position">The current position</param>
        /// <param name="targetPosition">The target position</param>
        /// <param name="speed">The current speed</param>
        /// <param name="deltaTime">The elapsed time this frame</param>
        /// <returns>The new position of the subject being moved</returns>
        public static Vector3 LerpTo(Vector3 position, Vector3 targetPosition, float speed, float deltaTime)
        {
            var tDistance = Vector3.Distance(position, targetPosition);
            var aDistance = speed * deltaTime;

            //If we are within range for this step
            if (aDistance > tDistance)
                return targetPosition;
            else //We wont be at target this step so lerp toward the target by the quata of aDistance and tDistance
                return ValidateNaN(Vector3.Lerp(position, targetPosition, aDistance / tDistance));
        }

        /// <summary>
        /// returns the intermediate rotation simulating constant rotation at speed from rotation toward target
        /// </summary>
        /// <param name="rotation">The current rotation</param>
        /// <param name="targetRotation">The target rotation</param>
        /// <param name="speed">The current speed</param>
        /// <param name="deltaTime">The elapsed time this frame</param>
        /// <returns>The new rotation of the subject being rotated</returns>
        public static Quaternion LerpTo(Quaternion rotation, Quaternion targetRotation, float speed, float deltaTime)
        {
            var tDistance = Quaternion.Angle(rotation, targetRotation);
            var aDistance = speed * deltaTime;

            if (aDistance > tDistance)
                return targetRotation;
            else
                return Quaternion.Lerp(rotation, targetRotation, aDistance / tDistance);
        }

        /// <summary>
        /// returns the force to apply to the rigidbody to accelerate toward the desired velocity at the indicated rate
        /// </summary>
        /// <param name="subject"></param>
        /// <param name="targetVelocity"></param>
        /// <param name="acceleration"></param>
        /// <param name="deltaTime"></param>
        /// <returns></returns>
        public static Vector3 LerpTo(Rigidbody subject, Vector3 targetVelocity, float acceleration, float deltaTime)
        {
            var stepVelocity = LerpTo(subject.velocity, targetVelocity, acceleration, deltaTime);
            return ForceToReachLinearVelocity(subject, stepVelocity);
        }

        /// <summary>
        /// returns the torque force to apply to the rigidbody to move toward the target rotation at the indicated speed
        /// </summary>
        /// <param name="subject"></param>
        /// <param name="targetRotation"></param>
        /// <param name="speed"></param>
        /// <param name="deltaTime"></param>
        /// <returns></returns>
        public static Vector3 LerpTo(Rigidbody subject, Quaternion targetRotation, float speed, float deltaTime)
        {
            var stepRotation = LerpTo(subject.rotation, targetRotation, speed, deltaTime);
            return TorqueToReachRotation(subject, stepRotation);
        }

        /// <summary>
        /// Resolves NaN values to 0s
        /// </summary>
        public static Vector3 ValidateNaN(Vector3 value)
        {
            var val = new Vector3(
                float.IsNaN(value.x) ? 0f : value.x,
                float.IsNaN(value.y) ? 0f : value.y,
                float.IsNaN(value.z) ? 0f : value.z);

            return val;
        }

        /// <summary>
        /// Resolves NaN values to 0s
        /// </summary>
        public static Vector2 ValidateNaN(Vector2 value)
        {
            var val = new Vector2(
                float.IsNaN(value.x) ? 0f : value.x,
                float.IsNaN(value.y) ? 0f : value.y);

            return val;
        }

        /// <summary>
        /// Resolves NaN values to 0s
        /// </summary>
        public static float ValidateNaN(float value)
        {
            return float.IsNaN(value) ? 0f : value;
        }

        /// <summary>
        /// Rotates a vector around a vector by eular angles
        /// </summary>
        /// <param name="point"></param>
        /// <param name="pivot"></param>
        /// <param name="angles"></param>
        /// <returns></returns>
        public static Vector3 RotatePointAroundPivot(Vector3 point, Vector3 pivot, Vector3 angles)
        {
            Vector3 dir = point - pivot; 
            dir = Quaternion.Euler(angles) * dir;
            point = dir + pivot; 
            return point; 
        }

        /// <summary>
        /// Rotates a vector around a vector by the rotation provided
        /// </summary>
        /// <param name="point"></param>
        /// <param name="pivot"></param>
        /// <param name="angles"></param>
        /// <returns></returns>
        public static Vector3 RotatePointAroundPivot(Vector3 point, Vector3 pivot, Quaternion rotation)
        {
            Vector3 dir = point - pivot;
            dir = rotation * dir;
            point = dir + pivot;
            return point;
        }

        /// <summary>
        /// Returns a point on the line nearest the subject
        /// </summary>
        /// <param name="lineStart">A end of the line segment</param>
        /// <param name="lineEnd">A end of the line segment</param>
        /// <param name="subject">The point to test from</param>
        /// <returns></returns>
        public static Vector3 NearestPointOnLineSegment(Vector3 lineStart, Vector3 lineEnd, Vector3 subject)
        {
            var line = (lineEnd - lineStart);
            var length = line.magnitude;
            line.Normalize();

            var subjectHeading = subject - lineStart;
            var dot = Vector3.Dot(subjectHeading, line);
            dot = Mathf.Clamp(dot, 0f, length);
            return lineStart + line * dot;
        }

        /// <summary>
        /// Returns a point on a ray (line without end)
        /// </summary>
        /// <param name="lineStart"></param>
        /// <param name="lineDirection"></param>
        /// <param name="subject"></param>
        /// <returns></returns>
        public static Vector3 NearestPointOnLine(Vector3 lineStart, Vector3 lineDirection, Vector3 subject)
        {
            var direction = lineDirection.normalized;
            var subjectHeading = subject - lineStart;
            var dot = Vector3.Dot(subjectHeading, direction);
            return lineStart + direction * dot;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <remarks>
        /// Ported from GraphicsGems by Jochen Schwarze
        /// </remarks>
        /// <param name="value"></param>
        /// <returns></returns>
        private static double GetCubicRoot(double value)
        {
            if (value > 0.0)
            {
                return System.Math.Pow(value, 1.0 / 3.0);
            }
            else if (value < 0)
            {
                return -System.Math.Pow(-value, 1.0 / 3.0);
            }
            else
            {
                return 0.0;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <remarks>
        /// Ported from GraphicsGems by Jochen Schwarze
        /// </remarks>
        /// <param name="value"></param>
        /// <returns></returns>
        public static int SolveCubic(double c0, double c1, double c2, double c3, out double s0, out double s1, out double s2)
        {
            s0 = double.NaN;
            s1 = double.NaN;
            s2 = double.NaN;

            int num;
            double sub;
            double A, B, C;
            double sq_A, p, q;
            double cb_p, D;

            /* normal form: x^3 + Ax^2 + Bx + C = 0 */
            A = c1 / c0;
            B = c2 / c0;
            C = c3 / c0;

            /*  substitute x = y - A/3 to eliminate quadric term:  x^3 +px + q = 0 */
            sq_A = A * A;
            p = 1.0 / 3 * (-1.0 / 3 * sq_A + B);
            q = 1.0 / 2 * (2.0 / 27 * A * sq_A - 1.0 / 3 * A * B + C);

            /* use Cardano's formula */
            cb_p = p * p * p;
            D = q * q + cb_p;

            if (D == 0)
            {
                if (q == 0) /* one triple solution */
                {
                    s0 = 0;
                    num = 1;
                }
                else /* one single and one double solution */
                {
                    double u = GetCubicRoot(-q);
                    s0 = 2 * u;
                    s1 = -u;
                    num = 2;
                }
            }
            else if (D < 0) /* Casus irreducibilis: three real solutions */
            {
                double phi = 1.0 / 3 * System.Math.Acos(-q / System.Math.Sqrt(-cb_p));
                double t = 2 * System.Math.Sqrt(-p);

                s0 = t * System.Math.Cos(phi);
                s1 = -t * System.Math.Cos(phi + System.Math.PI / 3);
                s2 = -t * System.Math.Cos(phi - System.Math.PI / 3);
                num = 3;
            }
            else /* one real solution */
            {
                double sqrt_D = System.Math.Sqrt(D);
                double u = GetCubicRoot(sqrt_D - q);
                double v = -GetCubicRoot(sqrt_D + q);

                s0 = u + v;
                num = 1;
            }

            /* resubstitute */
            sub = 1.0 / 3 * A;

            if (num > 0) s0 -= sub;
            if (num > 1) s1 -= sub;
            if (num > 2) s2 -= sub;

            return num;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <remarks>
        /// Ported from GraphicsGems by Jochen Schwarze
        /// </remarks>
        /// <param name="value"></param>
        /// <returns></returns>
        public static int SolveQuadric(double c0, double c1, double c2, out double s0, out double s1)
        {
            s0 = double.NaN;
            s1 = double.NaN;

            double p, q, D;

            /* normal form: x^2 + px + q = 0 */
            p = c1 / (2 * c0);
            q = c2 / c0;

            D = p * p - q;

            if (D == 0)
            {
                s0 = -p;
                return 1;
            }
            else if (D < 0)
            {
                return 0;
            }
            else /* if (D > 0) */
            {
                double sqrt_D = System.Math.Sqrt(D);

                s0 = sqrt_D - p;
                s1 = -sqrt_D - p;
                return 2;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <remarks>
        /// Ported from GraphicsGems by Jochen Schwarze
        /// </remarks>
        /// <param name="value"></param>
        /// <returns></returns>
        public static int SolveQuartic(double c0, double c1, double c2, double c3, double c4, out double s0, out double s1, out double s2, out double s3)
        {
            s0 = double.NaN;
            s1 = double.NaN;
            s2 = double.NaN;
            s3 = double.NaN;

            double[] coeffs = new double[4];
            double z, u, v, sub;
            double A, B, C, D;
            double sq_A, p, q, r;
            int num;

            /* normal form: x^4 + Ax^3 + Bx^2 + Cx + D = 0 */
            A = c1 / c0;
            B = c2 / c0;
            C = c3 / c0;
            D = c4 / c0;

            /*  substitute x = y - A/4 to eliminate cubic term: x^4 + px^2 + qx + r = 0 */
            sq_A = A * A;
            p = -3.0 / 8 * sq_A + B;
            q = 1.0 / 8 * sq_A * A - 1.0 / 2 * A * B + C;
            r = -3.0 / 256 * sq_A * sq_A + 1.0 / 16 * sq_A * B - 1.0 / 4 * A * C + D;

            if (r == 0)
            {
                /* no absolute term: y(y^3 + py + q) = 0 */

                coeffs[3] = q;
                coeffs[2] = p;
                coeffs[1] = 0;
                coeffs[0] = 1;

                num = SolveCubic(coeffs[0], coeffs[1], coeffs[2], coeffs[3], out s0, out s1, out s2);
            }
            else
            {
                /* solve the resolvent cubic ... */
                coeffs[3] = 1.0 / 2 * r * p - 1.0 / 8 * q * q;
                coeffs[2] = -r;
                coeffs[1] = -1.0 / 2 * p;
                coeffs[0] = 1;

                SolveCubic(coeffs[0], coeffs[1], coeffs[2], coeffs[3], out s0, out s1, out s2);

                /* ... and take the one real solution ... */
                z = s0;

                /* ... to build two quadric equations */
                u = z * z - r;
                v = 2 * z - p;

                if (u == 0)
                    u = 0;
                else if (u > 0)
                    u = System.Math.Sqrt(u);
                else
                    return 0;

                if (v == 0)
                    v = 0;
                else if (v > 0)
                    v = System.Math.Sqrt(v);
                else
                    return 0;

                coeffs[2] = z - u;
                coeffs[1] = q < 0 ? -v : v;
                coeffs[0] = 1;

                num = SolveQuadric(coeffs[0], coeffs[1], coeffs[2], out s0, out s1);

                coeffs[2] = z + u;
                coeffs[1] = q < 0 ? v : -v;
                coeffs[0] = 1;

                if (num == 0) num += SolveQuadric(coeffs[0], coeffs[1], coeffs[2], out s0, out s1);
                else if (num == 1) num += SolveQuadric(coeffs[0], coeffs[1], coeffs[2], out s1, out s2);
                else if (num == 2) num += SolveQuadric(coeffs[0], coeffs[1], coeffs[2], out s2, out s3);
            }

            /* resubstitute */
            sub = 1.0 / 4 * A;

            if (num > 0) s0 -= sub;
            if (num > 1) s1 -= sub;
            if (num > 2) s2 -= sub;
            if (num > 3) s3 -= sub;

            return num;
        }

        /// <summary>
        /// How do I get the X direction vector from quaternion rotaiton
        /// </summary>
        /// <remarks>
        /// What we assume you are asking is if I was standing up facing forward and rotated by this quaternion what direction would my X direction vector be pointing.
        /// The formula is simply direction = rotaiton * normal ... so for forward it would be imFacing = rotation * Vector3.forward;
        /// </remarks>
        /// <param name="rotation"></param>
        /// <returns></returns>
        public static Vector3 GetForwardDirection(Quaternion rotation) => rotation * Vector3.forward;
        /// <summary>
        /// How do I get the X direction vector from quaternion rotaiton
        /// </summary>
        /// <remarks>
        /// What we assume you are asking is if I was standing up facing forward and rotated by this quaternion what direction would my X direction vector be pointing.
        /// The formula is simply direction = rotaiton * normal ... so for forward it would be imFacing = rotation * Vector3.forward;
        /// </remarks>
        /// <param name="rotation"></param>
        /// <returns></returns>
        public static Vector3 GetBackDirection(Quaternion rotation) => rotation * Vector3.back;
        /// <summary>
        /// How do I get the X direction vector from quaternion rotaiton
        /// </summary>
        /// <remarks>
        /// What we assume you are asking is if I was standing up facing forward and rotated by this quaternion what direction would my X direction vector be pointing.
        /// The formula is simply direction = rotaiton * normal ... so for forward it would be imFacing = rotation * Vector3.forward;
        /// </remarks>
        /// <param name="rotation"></param>
        /// <returns></returns>
        public static Vector3 GetUpDirection(Quaternion rotation) => rotation * Vector3.up;
        /// <summary>
        /// How do I get the X direction vector from quaternion rotaiton
        /// </summary>
        /// <remarks>
        /// What we assume you are asking is if I was standing up facing forward and rotated by this quaternion what direction would my X direction vector be pointing.
        /// The formula is simply direction = rotaiton * normal ... so for forward it would be imFacing = rotation * Vector3.forward;
        /// </remarks>
        /// <param name="rotation"></param>
        /// <returns></returns>
        public static Vector3 GetDownDirection(Quaternion rotation) => rotation * Vector3.down;
        /// <summary>
        /// How do I get the X direction vector from quaternion rotaiton
        /// </summary>
        /// <remarks>
        /// What we assume you are asking is if I was standing up facing forward and rotated by this quaternion what direction would my X direction vector be pointing.
        /// The formula is simply direction = rotaiton * normal ... so for forward it would be imFacing = rotation * Vector3.forward;
        /// </remarks>
        /// <param name="rotation"></param>
        /// <returns></returns>
        public static Vector3 GetRightDirection(Quaternion rotation) => rotation * Vector3.right;
        /// <summary>
        /// How do I get the X direction vector from quaternion rotaiton
        /// </summary>
        /// <remarks>
        /// What we assume you are asking is if I was standing up facing forward and rotated by this quaternion what direction would my X direction vector be pointing.
        /// The formula is simply direction = rotaiton * normal ... so for forward it would be imFacing = rotation * Vector3.forward;
        /// </remarks>
        /// <param name="rotation"></param>
        /// <returns></returns>
        public static Vector3 GetLeftDirection(Quaternion rotation) => rotation * Vector3.left;
        /// <summary>
        /// How do I convert a direction vector to a rotaiton
        /// </summary>
        /// <remarks>
        /// We assume by this you mean if I wanted to rotate to look down this direction what would that rotaiton be
        /// Unity provides for this as well x = Quaternion.LookRotation(direction);
        /// </remarks>
        /// <param name="direction"></param>
        /// <returns></returns>
        public static Quaternion GetForwardDirection(Vector3 direction) => Quaternion.LookRotation(direction);

        /// <summary>
        /// The distance an object will have fallen given an input gravity and amount of time. 
        /// This is useful for various calcualtions.
        /// </summary>
        /// <param name="gravity"></param>
        /// <param name="time"></param>
        /// <returns></returns>
        public static float FallDistance(float gravity, float time) => 0.5f * gravity * time * time;

        public static Vector3 SimulateWindForce(float main, float frequency, float magnitude, float turbulence, Vector3 direction, float gameTime)
        {
            Vector3 d = direction;
            if (turbulence > 0)
            {
                //var range = (90f * turbulence);
                var range = Mathf.LerpAngle(-90f, 90f, Mathf.PerlinNoise(gameTime * turbulence, 1f));
                d = Quaternion.AngleAxis(range, Vector3.up) * d;
                d.Normalize();
                //d = (direction + Vector3.Slerp(direction * -1f, direction * 1f, Mathf.PerlinNoise(gameTime * turbulence, 1f))).normalized;
            }
            var f = d * main;
            var p = d * Mathf.Sin(gameTime * frequency * 90) * magnitude;

            return f + p;
        }

        /// <summary>
        /// Retrurns the size in world units at a given distance from the screen
        /// </summary>
        /// <param name="camera"></param>
        /// <param name="distance"></param>
        /// <returns></returns>
        public static float ScreenSizeAtDistance(Camera camera, float distance)
        {
            if (camera.orthographic)
                return camera.orthographicSize * 2;
            else
                return 2f * distance * Mathf.Tan(Mathf.Deg2Rad * camera.fieldOfView * 0.5f);
        }

        /// <summary>
        /// Finds the scale an object should take to have the same size on screen at the testDistance as it has at the natural distnace
        /// </summary>
        /// <param name="camera">The camera to test for</param>
        /// <param name="naturalDistance">The distance at which the object has a scale of 1 and is the desired size on screen. This is typically the "starting" distance for an object you want to appear to remain the same size on screen</param>
        /// <param name="testDistance">The distance at which the object is now</param>
        /// <returns></returns>
        public static float ScalarByDistance(Camera camera, float naturalDistance, float testDistance)
        {
            var natural = ScreenSizeAtDistance(camera, naturalDistance);
            var test = ScreenSizeAtDistance(camera, testDistance);
            return test / natural;
        }

        public static bool CircleContains(Vector2 center, float radius, Vector2 point)
        {
            return Vector2.Distance(center, point) <= radius;
        }

        public static bool SphereContains(Vector3 center, float radius, Vector3 point)
        {
            return Vector3.Distance(center, point) <= radius;
        }

        public static bool EllipseContains(Vector2 center, float xRadius, float yRadius, Vector2 point)
        {
            var local = point - center;
            return ((local.x * local.x) / (xRadius * xRadius)) + ((local.y * local.y) / (yRadius * yRadius)) <= 1f;
        }

        public static bool EllipsoidContains(Vector3 center, float xRadius, float yRadius, float zRadius, Vector3 point)
        {
            var local = point - center;
            return ((local.x * local.x) / (xRadius * xRadius)) + ((local.y * local.y) / (yRadius * yRadius)) + ((local.z * local.z) / (zRadius * zRadius)) <= 1f;
        }

        public static Vector2 GetPointOnCircle(Vector2 center, float radius, float angle)
        {
            return new Vector2(center.x + radius * Mathf.Sin(angle * Mathf.Deg2Rad), center.y + radius * Mathf.Cos(angle * Mathf.Deg2Rad));
        }

        public static Vector2 GetPointOnEllipse(Vector2 center, float xRadius, float yRadius, float angle)
        {
            return new Vector2(center.x + xRadius * Mathf.Sin(angle * Mathf.Deg2Rad), center.y + yRadius * Mathf.Cos(angle * Mathf.Deg2Rad));
        }
    }
}

#endif