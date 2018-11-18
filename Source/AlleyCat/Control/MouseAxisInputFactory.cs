using Godot;
using LanguageExt;
using static LanguageExt.Prelude;

namespace AlleyCat.Control
{
    public class MouseAxisInputFactory : AxisInputFactory<MouseAxisInput>
    {
        [Export]
        public MouseAxis Axis { get; set; }

        [Export(PropertyHint.ExpRange, "0, 1")]
        public float Maximum { get; set; }

        protected override Validation<string, MouseAxisInput> CreateService()
        {
            return new MouseAxisInput(
                GetName(),
                Axis,
                GetViewport(),
                this,
                this,
                Active)
            {
                Maximum = Maximum,
                Sensitivity = Sensitivity,
                Curve = Optional(Curve),
                DeadZone = DeadZone,
                Interpolate = Interpolate,
                WindowSize = WindowSize,
                WindowShift = WindowShift
            };
        }
    }
}