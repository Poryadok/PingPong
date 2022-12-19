using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PM.PingPong.Gameplay
{
    public static class PingPongPhysics
    {
        public static void SimulateBall(Ball ball, PlayerRocket rocketBlue, PlayerRocket rocketRed)
        {
            var ballVelocity = ball.Velocity;

            if (ballVelocity.magnitude == 0)
            {
                return;
            }

            Vector3 ballSizeOffset;
            PlayerRocket targetRocket;
            if (ballVelocity.z > 0)
            {
                targetRocket = rocketRed;
                ballSizeOffset = Vector3.forward;
            }
            else
            {
                targetRocket = rocketBlue;
                ballSizeOffset = Vector3.back;
            }

            var ballSize = ball.Radius;
            ballSizeOffset *= ballSize;

            var ballTransform = ball.transform;
            var frameVelocity = ballVelocity * Time.deltaTime;
            
            var ballPosition = ballTransform.position;
            var oldBottomCorner = ballPosition + ballSizeOffset;
            var newBottomCorner = oldBottomCorner + frameVelocity;
            var oldRightCorner = ballPosition + Vector3.right * ballSize;
            var newRightCorner = oldBottomCorner + frameVelocity;
            var oldLeftCorner = ballPosition + Vector3.left * ballSize;
            var newLeftCorner = oldBottomCorner + frameVelocity;

            var rocketPosition = targetRocket.transform.position;
            var rocketLeftCorner = rocketPosition + Vector3.left * (targetRocket.Size / 2);
            var rocketRightCorner = rocketPosition + Vector3.right * (targetRocket.Size / 2);

            var rightWall = 10f;
            var leftWall = -10f;
            
            
            // check if ball has a chance to touch the rocket
            if ( ballVelocity.z * ballPosition.z > 0 // don't try to hit the rocket you moving away from
                && Mathf.Abs(ballPosition.z) + ballSize + frameVelocity.magnitude >= Mathf.Abs(rocketPosition.z))
            {
                // default state of direct hit.
                if (IsIntersects(oldBottomCorner, newBottomCorner, rocketLeftCorner, rocketRightCorner))
                {
                    ProcessDirectHit(ball, oldBottomCorner, rocketPosition, newBottomCorner, ballTransform, targetRocket);
                    return;
                }
                
                // hit to the corner by side
                {
                    var checkPosition = ballPosition + frameVelocity;
                    if (Mathf.Abs(checkPosition.z) > Mathf.Abs(rocketPosition.z))
                    {
                        var movementPercent = (ballPosition.z - rocketPosition.z) / (ballPosition.z - checkPosition.z);
                        checkPosition = ballPosition + ballVelocity * (Time.deltaTime * movementPercent);
                    }

                    if (GetPointSegmentDistance(ballPosition, checkPosition, rocketLeftCorner) < ballSize
                        || GetPointSegmentDistance(ballPosition, checkPosition, rocketRightCorner) < ballSize)
                    {
                        if (Mathf.Abs(ballPosition.z) > Mathf.Abs(rocketPosition.z))
                        {
                            ball.Velocity =
                                new Vector3(
                                    ballPosition.x > rocketPosition.x
                                        ? Mathf.Abs(ball.Velocity.x)
                                        : -Mathf.Abs(ball.Velocity.x), 0, ballVelocity.z);
                        }
                        else
                        {
                            ball.transform.position = checkPosition;
                            ProcessHit(ball, rocketPosition, ballTransform, targetRocket);
                            return;   
                        }
                    }
                }
            }

            // hit returns, so here is no hit processing:
            
            // touch the right wall
            if (ballVelocity.x > 0 && IsIntersects(oldRightCorner, newRightCorner,
                         new Vector3(rightWall, 0, -1000), new Vector3(rightWall, 0, 1000)))
            {
                var movementPercent = (rightWall - oldRightCorner.x) / (newRightCorner.x - oldRightCorner.x);
                ballTransform.position += ballVelocity * (Time.deltaTime * movementPercent);

                ball.Velocity = new Vector3(-ballVelocity.x, 0, ballVelocity.z);
            }
            // touch the left wall
            else if (ballVelocity.x < 0 && IsIntersects(oldLeftCorner, newLeftCorner, new Vector3(leftWall, 0, -1000),
                         new Vector3(leftWall, 0, 1000)))
            {
                var movementPercent = (leftWall - oldLeftCorner.x) / (newLeftCorner.x - oldLeftCorner.x);
                ballTransform.position += ballVelocity * (Time.deltaTime * movementPercent);

                ball.Velocity = new Vector3(-ballVelocity.x, 0, ballVelocity.z);
            }
            // just moving
            else
            {
                ball.transform.position += frameVelocity;
            }
            
            // it's like an effect for the goal.
            if (Mathf.Abs(ball.transform.position.z) - 2 * ballSize > Mathf.Abs(rocketPosition.z))
            {
                ball.transform.position = new Vector3(ball.transform.position.x, 0,
                    rocketPosition.z + (rocketPosition.z > 0 ? 2 * ballSize : -2 * ballSize));
                ball.Velocity = Vector3.zero;
            }
        }

        private static void ProcessDirectHit(Ball ball, Vector3 oldBottomCorner, Vector3 rocketPosition,
            Vector3 newBottomCorner, Transform ballTransform, PlayerRocket targetRocket)
        {
            var movementPercent = (oldBottomCorner.z - rocketPosition.z) / (oldBottomCorner.z - newBottomCorner.z);
            ballTransform.position += ball.Velocity * (Time.deltaTime * movementPercent);
            
            ProcessHit(ball, rocketPosition, ballTransform, targetRocket);
        }
        
        private static void ProcessHit(Ball ball, Vector3 rocketPosition,
            Transform ballTransform, PlayerRocket targetRocket)
        {
            var maxSpeed = 1000f;
            var maxSqrSpeed = 1000000f;

            ball.Velocity = new Vector3(ball.Velocity.x, 0, -ball.Velocity.z) * 1.1f;
            if (ball.Velocity.sqrMagnitude > maxSqrSpeed)
            {
                ball.Velocity = ball.Velocity.normalized * maxSpeed;
            }
            
            // this code makes rocket change ball angle further away from rocket center
            
            var currentAngle = Vector3.SignedAngle(targetRocket.transform.forward, ball.Velocity, Vector3.up);
            var hitAnglePercent = Mathf.Clamp((ballTransform.position.x - rocketPosition.x) / targetRocket.Size, -1, 1);
            if (ball.Velocity.z > 0)
            {
                hitAnglePercent *= -1; // inversed rocket
            }

            float minAngle = 0;
            float maxAngle = 90;

            if (hitAnglePercent < 0)
            {
                maxAngle = -90;
                hitAnglePercent *= -1; // abs
            }
            
            var angleOffset = Mathf.Lerp(minAngle, maxAngle, hitAnglePercent);

            var newAngle = currentAngle - angleOffset;
            
            // clamp angle. else there will be close to horizontal movement balls. nobody wants to wait for them.
            newAngle = Mathf.Clamp(newAngle, -70, 70);

            ball.Velocity = Quaternion.Euler(0, newAngle, 0) * targetRocket.transform.forward *
                            ball.Velocity.magnitude;
            
            ball.OnHit();
        }

        private static bool IsCounterClockWise(Vector3 A, Vector3 B, Vector3 C)
        {
            return (C.z - A.z) * (B.x - A.x) > (B.z - A.z) * (C.x - A.x);
        }

        private static bool IsIntersects(Vector3 A, Vector3 B, Vector3 C, Vector3 D)
        {
            return IsCounterClockWise(A, C, D) != IsCounterClockWise(B, C, D) &&
                   (IsCounterClockWise(A, B, C) != IsCounterClockWise(A, B, D) ||
                    IsCounterClockWise(B, A, C) != IsCounterClockWise(B, A, D));
        }

        // Return minimum distance between line segment AB and point P
        private static float GetPointSegmentDistance(Vector3 A, Vector3 B, Vector3 P)
        {
            float l2 = (A - B).sqrMagnitude;

            if (l2 == 0.0)
                return Vector3.Distance(P, A); // A == B

            // Consider the line extending the segment, parameterized as A + t (B- A).
            // We find projection of point p onto the line. 
            // It falls where t = [(P - A) . (B - A)] / |B- A|^2
            // We clamp t from [0,1] to handle points outside the segment AB.
            float t = Mathf.Clamp01(Vector3.Dot(P - A, B - A) / l2);
            var projection = A + t * (B - A); // Projection falls on the segment
            return Vector3.Distance(P, projection);
        }
    }
}