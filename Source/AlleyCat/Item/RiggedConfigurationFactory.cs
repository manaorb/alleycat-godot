using EnsureThat;
using Godot;
using Godot.Collections;
using JetBrains.Annotations;
using LanguageExt;
using Microsoft.Extensions.Logging;
using static LanguageExt.Prelude;

namespace AlleyCat.Item
{
    public class RiggedConfigurationFactory : EquipmentConfigurationFactory<RiggedConfiguration>
    {
        [Export, UsedImplicitly]
        public Array<string> MeshesToSync { get; set; }

        protected override Validation<string, RiggedConfiguration> CreateService(
            string key, string slot, Set<string> additionalSlots, ILogger logger)
        {
            Ensure.That(key, nameof(key)).IsNotNullOrEmpty();
            Ensure.That(slot, nameof(slot)).IsNotNullOrEmpty();
            Ensure.That(logger, nameof(logger)).IsNotNull();

            return new RiggedConfiguration(
                key,
                slot,
                additionalSlots,
                Tags,
                toSet(MeshesToSync),
                Active,
                logger)
            {
                Mesh = Mesh,
                Animation = Animation,
                AnimationBlend = AnimationBlend,
                AnimationTransition = AnimationTransition
            };
        }
    }
}
