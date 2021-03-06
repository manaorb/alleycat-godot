using System.Collections.Generic;
using System.Linq;
using AlleyCat.Event;
using AlleyCat.Logging;
using EnsureThat;
using Godot;
using Microsoft.Extensions.Logging;

namespace AlleyCat.Animation
{
    public abstract class AnimationControl : ReactiveObject, IAnimationControl, ILoggable
    {
        public string Key { get; }

        public ILogger Logger { get; }

        protected AnimationGraphContext Context { get; }

        protected AnimationControl(string key, AnimationGraphContext context)
        {
            Ensure.That(key, nameof(key)).IsNotNullOrEmpty();
            Ensure.That(context, nameof(context)).IsNotNull();

            Key = key;
            Context = context;
            Logger = context.LoggerFactory.CreateLogger(this.GetLogCategory());
        }

        protected override void PostConstruct()
        {
            this.LogDebug("Initializing animation control.");

            base.PostConstruct();
        }

        protected override void PreDestroy()
        {
            this.LogDebug("Disposing animation control.");

            base.PreDestroy();
        }

        protected static IEnumerable<NodePath> FindTransformTracks(Godot.Animation animation)
        {
            Ensure.That(animation, nameof(animation)).IsNotNull();

            return Enumerable
                .Range(0, animation.GetTrackCount())
                .Select(i => (path: animation.TrackGetPath(i), type: animation.TrackGetType(i)))
                .Where(t => t.type == Godot.Animation.TrackType.Transform)
                .Select(t => t.path);
        }
    }
}
