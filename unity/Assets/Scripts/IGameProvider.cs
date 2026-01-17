using Assets.Scripts;

/// <summary>
/// Interface for providing game context to classes that need it.
/// This enables testing by allowing mock implementations to be injected.
/// </summary>
public interface IGameProvider
{
    /// <summary>
    /// Gets the current language code (e.g., "English", "Spanish").
    /// </summary>
    string CurrentLang { get; }

    /// <summary>
    /// Gets the current game type (D2E, MoM, IA, or NoGameType).
    /// </summary>
    GameType GameType { get; }
}

/// <summary>
/// Default implementation that uses the Game singleton.
/// This maintains backwards compatibility with existing code.
/// </summary>
public class DefaultGameProvider : IGameProvider
{
    public string CurrentLang => Game.Get()?.currentLang ?? ValkyrieConstants.DefaultLanguage;
    public GameType GameType => Game.Get()?.gameType ?? new NoGameType();
}
