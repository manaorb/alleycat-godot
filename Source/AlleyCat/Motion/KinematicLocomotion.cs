﻿using AlleyCat.Event;
using AlleyCat.Setting.Project;
using EnsureThat;
using Godot;
using Microsoft.Extensions.Logging;

namespace AlleyCat.Motion
{
    public abstract class KinematicLocomotion : Locomotion<KinematicBody>
    {
        public float Gravity { get; }

        public Vector3 GravityVector { get; }

        public bool ApplyGravity { get; set; } = true;

        public override ProcessMode ProcessMode => ProcessMode.Physics;

        protected float FallDuration { get; private set; }

        protected KinematicLocomotion(
            KinematicBody target,
            Physics3DSettings physicsSettings,
            ITimeSource timeSource,
            bool active,
            ILogger logger) : base(target, timeSource, active, logger)
        {
            Ensure.That(physicsSettings, nameof(physicsSettings)).IsNotNull();

            Gravity = physicsSettings.DefaultGravity;
            GravityVector = physicsSettings.DefaultGravityVector;
        }

        protected override void Process(float delta, Vector3 velocity, Vector3 rotationalVelocity)
        {
            var effective = KinematicProcess(delta, velocity, rotationalVelocity);

            if (Target.IsOnFloor() || !ApplyGravity)
            {
                FallDuration = 0;
            }
            else
            {
                FallDuration += delta;

                effective += GravityVector * Gravity * FallDuration;
            }

            Target.MoveAndSlide(effective, Vector3.Up);
        }

        protected abstract Vector3 KinematicProcess(float delta, Vector3 velocity, Vector3 rotationalVelocity);
    }
}
