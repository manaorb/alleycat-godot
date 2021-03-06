using System;
using AlleyCat.Action;
using EnsureThat;
using LanguageExt;
using Microsoft.Extensions.Logging;

namespace AlleyCat.Control
{
    public class PlayerActionDelegate : PlayerAction
    {
        public string Action { get; }

        public PlayerActionDelegate(
            string key,
            string displayName,
            string action,
            Func<Option<IPlayerControl>> playerControl,
            ITriggerInput input,
            bool active,
            ILoggerFactory loggerFactory) : base(key, displayName, playerControl, input, active, loggerFactory)
        {
            Ensure.That(action, nameof(action)).IsNotNull();

            Action = action;
        }

        protected override void DoExecute(IActionContext context)
        {
            Ensure.That(context, nameof(context)).IsNotNull();

            context.Actor.Bind(a => a.Actions.TryGetValue(Action)).Iter(a => a.Execute(context));
        }

        public override bool AllowedFor(IActionContext context)
        {
            Ensure.That(context, nameof(context)).IsNotNull();

            return context.Actor.Bind(a => a.Actions.TryGetValue(Action)).Exists(a => a.AllowedFor(context));
        }
    }
}
