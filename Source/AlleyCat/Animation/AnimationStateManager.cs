﻿using System;
using System.Linq;
using System.Reactive.Concurrency;
using AlleyCat.Autowire;
using AlleyCat.Event;
using EnsureThat;
using Godot;
using Gen = System.Collections.Generic;

namespace AlleyCat.Animation
{
    [Singleton(typeof(IAnimationManager), typeof(IAnimationStateManager))]
    public class AnimationStateManager : AnimationManager, IAnimationStateManager
    {
        public const string ResetAnimation = "Reset";

        public const string OneShotNode = "One Shot";

        public const string OneShotTriggerNode = "One Shot Trigger";

        public const string OverrideNodePrefix = "Override ";

        public const string OverrideBlendNodePrefix = "Override Blend ";

        private IScheduler _scheduler;

        private Gen.IDictionary<int, string> _overrides = new Gen.Dictionary<int, string>(0);

        private int _overridableSlots;

        private IDisposable _oneShotAnimationCallback;

        [Service]
        public AnimationTreePlayer TreePlayer { get; private set; }

        protected override void OnInitialize()
        {
            base.OnInitialize();

            TreePlayer.Active = false;

            _scheduler = this.GetScheduler(ProcessMode);

            _overridableSlots = TreePlayer.GetNodeList().Count(n => n.StartsWith(OverrideBlendNodePrefix));
            _overrides = Enumerable
                .Range(1, _overridableSlots)
                .Select(i => (Index: i, Name: OverrideNodePrefix + i))
                .Select(t => (t.Index, Slot: TreePlayer.AnimationNodeGetAnimation(t.Name)?.GetName()))
                .Where(t => t.Slot != null)
                .ToDictionary(t => t.Index, t => t.Slot);
        }

        public override void Advance(float delta)
        {
            TreePlayer.Advance(0);

            base.Advance(delta);
        }

        protected override void ProcessFrames(float delta) => TreePlayer.Advance(delta);

        public override void Play(Godot.Animation animation, System.Action onFinish = null)
        {
            _oneShotAnimationCallback?.Dispose();

            TreePlayer.AnimationNodeSetAnimation(OneShotNode, animation);
            TreePlayer.OneshotNodeStart(OneShotTriggerNode);

            if (onFinish != null)
            {
                _oneShotAnimationCallback = _scheduler?.Schedule(
                    TimeSpan.FromSeconds(animation.Length), onFinish);
            }
        }

        public void Blend(Godot.Animation animation, float influence = 1f)
        {
            Ensure.Any.IsNotNull(animation, nameof(animation));

            var slot = Enumerable
                .Range(1, _overridableSlots)
                .FirstOrDefault(i => !_overrides.ContainsKey(i));

            if (slot == default)
            {
                throw new InvalidOperationException(
                    $"No overridable slots left: total = {_overridableSlots}.");
            }

            var blendNode = OverrideBlendNodePrefix + slot;

            _overrides[slot] = animation.GetName();

            TreePlayer.AnimationNodeSetAnimation(OverrideNodePrefix + slot, animation);
            TreePlayer.Blend2NodeSetAmount(blendNode, influence);

            var reset = Player.GetAnimation(ResetAnimation);

            if (reset == null) return;

            var filtered = new Gen.HashSet<string>(FindTransformTracks(animation).Select(p => p.ToString()));

            FindTransformTracks(reset)
                .ToList()
                .ForEach(p => TreePlayer.Blend2NodeSetFilterPath(blendNode, p, !filtered.Contains(p.ToString())));
        }

        public void Unblend(string animation)
        {
            Ensure.Any.IsNotNull(animation, nameof(animation));

            var slot = _overrides.FirstOrDefault(i => i.Value == animation).Key;

            if (slot == default) return;

            var animNode = OverrideNodePrefix + slot;
            var blendNode = OverrideBlendNodePrefix + slot;

            TreePlayer.Blend2NodeSetAmount(blendNode, 0);
            TreePlayer.AnimationNodeSetAnimation(animNode, null);

            var reset = Player.GetAnimation(ResetAnimation);

            if (reset != null)
            {
                FindTransformTracks(reset)
                    .ToList()
                    .ForEach(p => TreePlayer.Blend2NodeSetFilterPath(blendNode, p, true));
            }

            _overrides.Remove(slot);
        }

        private static Gen.IEnumerable<NodePath> FindTransformTracks(Godot.Animation animation)
        {
            var tracks = animation.GetTrackCount();

            return Enumerable
                .Range(0, tracks)
                .Select(i => (path: animation.TrackGetPath(i), type: animation.TrackGetType(i)))
                .Where(t => t.type == Godot.Animation.TrackType.Transform)
                .Select(t => t.path);
        }

        protected override void OnPreDestroy()
        {
            _oneShotAnimationCallback?.Dispose();
            _oneShotAnimationCallback = null;

            base.OnPreDestroy();
        }
    }
}
