namespace Quipbot.Games.Quiplash3
{
    public enum Quiplash3State
    {
        /// <summary>
        /// The game is in an unknown state
        /// </summary>
        Unknown,

        /// <summary>
        /// Waiting at the lobby for the game to (re-)start
        /// </summary>
        Lobby,

        /// <summary>
        /// Selecting a character
        /// </summary>
        SelectCharacter,

        /// <summary>
        /// Showing the Quiplash 3 logo (as a transition between states)
        /// </summary>
        Logo,

        /// <summary>
        /// Showing the round to begin
        /// </summary>
        Round,

        /// <summary>
        /// Writing a single answer
        /// </summary>
        SingleAnswer,

        /// <summary>
        /// Finished with single answer (and waiting for others)
        /// </summary>
        SingleAnswerDone,

        /// <summary>
        /// Writing multiple answers
        /// </summary>
        MultipleAnswers,

        /// <summary>
        /// Finished with multiple answers (and waiting for others)
        /// </summary>
        MultipleAnswersDone,

        /// <summary>
        /// Voting on shown answers
        /// </summary>
        Vote,

        /// <summary>
        /// Creating or editing custom episodes
        /// </summary>
        UserGeneratedContent
    }
}
