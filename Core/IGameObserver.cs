namespace Quipbot
{
    public interface IGameObserver
    {
        string SignInPageClass { get; }
        string GamePageClass { get; }

        PageState PageState { get; }

        string? PlayerName { get; }

        void UpdateState(string html);
    }
}