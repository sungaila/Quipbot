namespace Quipbot.Games.Quiplash3
{
    public enum Quiplash3State
    {
        /// <summary>
        /// The game is in an unknown state
        /// </summary>
        Unknown,

        /// <summary>
        /// Selecting a character
        /// </summary>
        SelectCharacter,

        /// <summary>
        /// Waiting for the round to begin or for other players to finish
        /// </summary>
        Waiting,

        /// <summary>
        /// Writing one or more answers
        /// </summary>
        Writing,

        /// <summary>
        /// Waiting for the given answer to submit
        /// </summary>
        SubmittingAnswer,

        /// <summary>
        /// Voting on shown answers
        /// </summary>
        Vote,

        /// <summary>
        /// Game finished and can be restarted
        /// </summary>
        PostGame
    }
}
