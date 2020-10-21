using HtmlAgilityPack;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace Quipbot.Games.Quiplash2
{
    public class Quiplash2Observer : GameObserverBase
    {
        public Quiplash2State GameState { get; protected set; }

        public bool CanStartGame { get; protected set; }

        public bool CanStopCountdown { get; protected set; }

        public bool CanRestartGame { get; protected set; }

        public Color? PlayerColor { get; protected set; }

        public string? Question { get; protected set; }

        public Dictionary<string, string>? VoteOptions { get; protected set; }

        public string? GalleryOfQuips { get; protected set; }

        protected override void ResetGameState()
        {
            GameState = Quiplash2State.Unknown;
            CanStartGame = false;
            PlayerName = null;
            PlayerColor = null;
            Question = null;
            VoteOptions = null;
        }

        protected override void UpdateGameState(HtmlDocument htmlDocument)
        {
            var divs = htmlDocument.DocumentNode.Descendants("div");

            // read player name and color
            var player = divs.Single(n => n.Id == "player");
            PlayerName = player.Descendants("h1").Single().InnerText;
            PlayerColor = ParseBackgroundColor(player.GetAttributeValue("style", null));

            // there should be some game pages
            var pages = divs.Where(n => n.HasClass(GamePageClass));
            if (!pages.Any())
            {
                PageState = PageState.Unknown;
                return;
            }

            // check for the visible game state
            PageState = PageState.Connected;
            var currentPage = pages.Single(n => !n.HasClass("pt-page-off"));

            GameState = currentPage.Id switch
            {
                "state-lobby" => Quiplash2State.Lobby,
                "state-logo" => Quiplash2State.Logo,
                "state-round" => Quiplash2State.Round,
                "state-answer-question" => Quiplash2State.AnswerQuestion,
                "state-done-answering" => Quiplash2State.DoneAnswering,
                "state-vote" => Quiplash2State.Vote,
                "state-ugc" => Quiplash2State.UserGeneratedContent,
                _ => Quiplash2State.Unknown,
            };

            if (GameState == Quiplash2State.Lobby)
            {
                var buttons = currentPage.Descendants("button");

                CanStartGame = buttons
                    .SingleOrDefault(n => n.Id == "quiplash-startgame" && n.GetAttributeValue("style", null) != "display: none;") != null;

                CanStopCountdown = buttons
                    .SingleOrDefault(n => n.Id == "quiplash-stopcountdown" && n.GetAttributeValue("style", null) != "display: none;") != null;

                CanRestartGame = buttons
                    .SingleOrDefault(n => n.Id == "quiplash-sameplayers" && n.GetAttributeValue("style", null) != "display: none;") != null;

                GalleryOfQuips = divs
                    .SingleOrDefault(n => n.Id == "quiplash2-lobby-postgame")
                    ?.GetAttributeValue("href", null);
            }
            if (GameState == Quiplash2State.AnswerQuestion)
            {
                Question = currentPage
                    .Descendants("p")
                    .Single(n => n.Id == "question-text")
                    .InnerText;
            }
            else if (GameState == Quiplash2State.Vote)
            {
                var voteForm = currentPage
                    .Descendants("form")
                    .Single(n => n.Id == "quiplash-vote");

                if (!voteForm.ChildNodes.Any())
                    return;

                VoteOptions = new Dictionary<string, string>();

                var buttons = voteForm
                    .Descendants("button")
                    .Where(n => n.HasClass("quiplash2-vote-button"));

                foreach (var button in buttons)
                {
                    VoteOptions.Add(button.GetAttributeValue("data-vote", null), button.InnerText);
                }
            }
        }
    }
}
