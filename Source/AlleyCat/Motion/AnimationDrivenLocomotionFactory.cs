using AlleyCat.Animation;
using AlleyCat.Autowire;
using AlleyCat.Common;
using AlleyCat.Setting.Project;
using Godot;
using LanguageExt;
using Microsoft.Extensions.Logging;

namespace AlleyCat.Motion
{
    public class AnimationDrivenLocomotionFactory : KinematicLocomotionFactory<AnimationDrivenLocomotion>
    {
        [Service]
        public Option<IAnimationStateManager> AnimationManager { get; set; }

        [Service]
        public Option<Skeleton> Skeleton { get; set; }

        [Export]
        public string IdleState { get; set; } = "Idle";

        [Export]
        public string MoveState { get; set; } = "Moving";

        [Export]
        public string StatesPath { get; set; } = "States";

        [Export]
        public string Blend2DPath { get; set; } = "States/Moving/Direction";

        [Export]
        public string TimeScalePath { get; set; } = "States/Moving/Speed";

        protected override Validation<string, AnimationDrivenLocomotion> CreateService(
            KinematicBody target,
            Physics3DSettings physicsSettings,
            ILoggerFactory loggerFactory)
        {
            return
                from manager in AnimationManager
                    .ToValidation("Failed to find the animation manager.")
                from skeleton in Skeleton
                    .ToValidation("Failed to find the skeleton.")
                from states in StatesPath.TrimToOption().Bind(manager.FindStates)
                    .ToValidation($"Unable to find an AnimationStates control at '{StatesPath}'.")
                from blender in Blend2DPath.TrimToOption().Bind(manager.FindBlender2D)
                    .ToValidation($"Unable to find a Blender2D control at '{Blend2DPath}'.")
                from timeScale in TimeScalePath.TrimToOption().Bind(manager.FindTimeScale)
                    .ToValidation($"Unable to find a TimeScale control at '{TimeScalePath}'.")
                from idleState in IdleState.TrimToOption()
                    .ToValidation("Idle state value was not specified.")
                from moveState in MoveState.TrimToOption()
                    .ToValidation("Move state value was not specified.")
                select new AnimationDrivenLocomotion(
                    manager.AnimationTree,
                    skeleton,
                    states,
                    blender,
                    timeScale,
                    idleState,
                    moveState,
                    target,
                    physicsSettings,
                    this,
                    Active,
                    loggerFactory);
        }
    }
}
