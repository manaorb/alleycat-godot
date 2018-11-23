using System;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using AlleyCat.Common;
using AlleyCat.Event;
using EnsureThat;
using Godot;
using LanguageExt;
using Microsoft.Extensions.Logging;

namespace AlleyCat.Animation
{
    public class AnimationManager : GameObject, IAnimationManager
    {
        public bool Active
        {
            get => _active.Value;
            set => _active.OnNext(value);
        }

        public IObservable<bool> OnActiveStateChange => _active.AsObservable();

        public AnimationPlayer Player { get; }

        public IObservable<Unit> OnBeforeAdvance => _onBeforeAdvance.AsObservable();

        public IObservable<float> OnAdvance => _onAdvance.AsObservable();

        public ProcessMode ProcessMode { get; }

        protected ITimeSource TimeSource { get; }

        public virtual IObservable<AnimationEvent> OnAnimationEvent => _onAnimationEvent.AsObservable();

        private readonly BehaviorSubject<bool> _active;

        private readonly ISubject<Unit> _onBeforeAdvance;

        private readonly ISubject<float> _onAdvance;

        private readonly ISubject<AnimationEvent> _onAnimationEvent;

        public AnimationManager(
            AnimationPlayer player,
            ProcessMode processMode,
            ITimeSource timeSource,
            bool active,
            ILogger logger) : base(logger)
        {
            Ensure.That(player, nameof(player)).IsNotNull();
            Ensure.That(timeSource, nameof(timeSource)).IsNotNull();

            Player = player;
            ProcessMode = processMode;
            TimeSource = timeSource;

            _active = new BehaviorSubject<bool>(active).AddTo(this);
            _onBeforeAdvance = new Subject<Unit>().AddTo(this);
            _onAdvance = new Subject<float>().AddTo(this);
            _onAnimationEvent = new Subject<AnimationEvent>().AddTo(this);

            Player.PlaybackProcessMode = AnimationPlayer.AnimationProcessMode.Manual;
        }

        protected override void PostConstruct()
        {
            base.PostConstruct();

            TimeSource.OnProcess(ProcessMode)
                .Where(_ => Active)
                .Subscribe(Advance)
                .AddTo(this);
        }

        public virtual void Advance(float delta)
        {
            _onBeforeAdvance.OnNext(Unit.Default);

            ProcessFrames(delta);

            _onAdvance.OnNext(delta);
        }

        protected virtual void ProcessFrames(float delta) => Player.Advance(delta);

        public virtual void Play(Godot.Animation animation)
        {
            Ensure.That(animation, nameof(animation)).IsNotNull();

            Player.Play(Player.AddAnimation(animation));
        }

        public void FireEvent(string name, Option<object> argument)
        {
            Ensure.That(name, nameof(name)).IsNotNull();

            _onAnimationEvent.OnNext(new AnimationEvent(name, argument, this));
        }
    }
}
