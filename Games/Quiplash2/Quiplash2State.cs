using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quipbot.Games.Quiplash2
{
    public enum Quiplash2State
    {
        /// <summary>
        /// The game is in an unknown state
        /// </summary>
        Unknown,

        /// <summary>
        /// Waiting at the lobby for the game to start
        /// </summary>
        Lobby,
        
        /// <summary>
        /// Showing the Quiplash 2 logo (as a transition between states)
        /// </summary>
        Logo,
        
        /// <summary>
        /// Showing the round to begin
        /// </summary>
        Round,
        
        /// <summary>
        /// Answering a question in a form
        /// </summary>
        AnswerQuestion,
        
        /// <summary>
        /// Waiting for the other players to finish answering
        /// </summary>
        DoneAnswering,
        
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
