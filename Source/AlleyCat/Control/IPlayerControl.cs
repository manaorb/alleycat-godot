using AlleyCat.Character;
using AlleyCat.View;

namespace AlleyCat.Control
{
    public interface IPlayerControl : ICharacterAware<IHumanoid>, IPerspectiveSwitcher
    {
    }
}
