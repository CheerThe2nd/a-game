using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Godot;

namespace agame.scripts.Player
{
    internal class PlayerMovement
    {
        private float _accel = 10.0f;
        private float _maxSpeed = 10.0f;
        private float _jumpStren = 20.0f;
        private float _gravityStren = 30.0f;
        private float _airCtl = 0.04f;
        private float _firction = 14.0f;
        private float _frictionSpeedThreshold = 0.5f;
        private Vector3 _desireMoveDir;
        public Vector3 finalVelocity;

        // This should be called in the physics tick since it uses the interal engine physics
        public void HandleMovement(bool isGrounded, float dTimeSec)
        {
            ApplyGravity(dTimeSec, isGrounded);
            if (isGrounded)
            {
                // --- Movement ---
                if (Input.IsActionJustPressed("jump"))
                    finalVelocity += new Vector3(0.0f, _jumpStren, 0.0f);

                if (finalVelocity.Length() < _maxSpeed)
                {
                    finalVelocity += new Vector3(
                    _desireMoveDir.X * _accel,
                    0.0f,
                    _desireMoveDir.Z * _accel);
                }
                DoFriction(dTimeSec);
            }
            else
            {
                if (finalVelocity.Length() < _maxSpeed && finalVelocity.Dot(_desireMoveDir) > -0.1f)
                {
                    finalVelocity += new Vector3(
                    _desireMoveDir.X * _accel * _airCtl,
                    0.0f,
                    _desireMoveDir.Z * _accel * _airCtl);
                }
            }
        }

        // Should not be called in physics tick or it can miss user input
        public void HandleInput(Godot.Basis transformBase)
        {
            Vector3 localVelocity = new Vector3(0.0f, 0.0f, 0.0f);
            if (Input.IsActionPressed("move_left"))
                localVelocity.X = -1.0f;
            else if (Input.IsActionPressed("move_right"))
                localVelocity.X = 1.0f;
            else
                localVelocity.X = 0.0f;

            if (Input.IsActionPressed("move_forward"))
                localVelocity.Z = -1.0f;
            else if (Input.IsActionPressed("move_backwards"))
                localVelocity.Z = 1.0f;
            else
                localVelocity.Z = 0.0f;

            localVelocity = localVelocity.Normalized();
            _desireMoveDir = transformBase.X * localVelocity.X + transformBase.Z * localVelocity.Z;
        }

        private void DoFriction(float dTimeSec)
        {
            float speed = finalVelocity.Length();
            if (speed <= 0.00001f)
                return;
            float downLimit = Mathf.Max(speed, _frictionSpeedThreshold);
            float dropAmount = speed - (downLimit * _firction * dTimeSec);
            if (dropAmount < 0)
                dropAmount = 0;
            finalVelocity *= dropAmount / speed;
        }
        private void ApplyGravity(double delta, bool isGrounded)
        {
            if (!isGrounded)
                finalVelocity -= new Vector3(0.0f, _gravityStren * (float)delta, 0.0f);
            else if (finalVelocity.Y < 0)
                finalVelocity = new Vector3(finalVelocity.X, 0f, finalVelocity.Z);
        }
    }
}
