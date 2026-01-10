using Assets.Scripts;


/// <summary>
/// Test implementation of IGameProvider for unit testing.
/// Allows tests to run without requiring the full Unity Game singleton.
/// </summary>
public class TestGameProvider : IGameProvider
{
    private readonly string _currentLang;
    private readonly GameType _gameType;

    /// <summary>
    /// Creates a TestGameProvider with default values (English, NoGameType).
    /// </summary>
    public TestGameProvider()
    {
        _currentLang = ValkyrieConstants.DefaultLanguage;
        _gameType = new NoGameType();
    }

    /// <summary>
    /// Creates a TestGameProvider with a specific language.
    /// </summary>
    /// <param name="currentLang">The language to use (e.g., "English", "Spanish")</param>
    public TestGameProvider(string currentLang)
    {
        _currentLang = currentLang ?? ValkyrieConstants.DefaultLanguage;
        _gameType = new NoGameType();
    }

    /// <summary>
    /// Creates a TestGameProvider with specific language and game type.
    /// </summary>
    /// <param name="currentLang">The language to use</param>
    /// <param name="gameType">The game type to use</param>
    public TestGameProvider(string currentLang, GameType gameType)
    {
        _currentLang = currentLang ?? ValkyrieConstants.DefaultLanguage;
        _gameType = gameType ?? new NoGameType();
    }

    public string CurrentLang => _currentLang;
    public GameType GameType => _gameType;
}
