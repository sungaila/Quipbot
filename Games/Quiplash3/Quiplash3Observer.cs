using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Quipbot.Games.Quiplash3
{
    public class Quiplash3Observer : GameObserverBase
    {
        public Quiplash3State GameState { get; protected set; }

        public bool CanStartGame { get; protected set; }

        public bool CanRestartGame { get; protected set; }

        public string? PlayerCharacter { get; protected set; }

        public List<string>? AvailableCharacters { get; protected set; }

        public KeyValuePair<string, string>? Question { get; protected set; }

        public Dictionary<string, string>? VoteOptions { get; protected set; }

        protected override void ResetGameState()
        {
            GameState = Quiplash3State.Unknown;
            CanStartGame = false;
            PlayerName = null;
            PlayerCharacter = null;
            Question = null;
            VoteOptions = null;
            AvailableCharacters = null;
        }

        private static List<string> _characterButtonClasses = new List<string> { "characters", "active", "selected", "disabled" };

        protected override void UpdateGameState(HtmlDocument htmlDocument)
        {
            var divs = htmlDocument.DocumentNode.Descendants("div");
            var buttons = htmlDocument.DocumentNode.Descendants("button");

            var stateController = divs.FirstOrDefault(n => n.HasClass("state-controller"));
            var stateDiv = stateController?.ChildNodes.SingleOrDefault(n => n.Name == "div" && n.Id != "playerRegion"); // this one tells us the game state!

            if (stateDiv == null)
            {
                PageState = PageState.Unknown;
                return;
            }

            PageState = PageState.Connected;

            // read player name and character
            PlayerName = divs.Single(n => n.Id == "playername").InnerText;
            PlayerCharacter = divs.Single(n => n.Id == "playericon").GetClasses().SingleOrDefault(c => c != "playerIcon");

            if (stateDiv.HasClass("Lobby"))
            {
                GameState = Quiplash3State.Lobby;
                CanStartGame = buttons.SingleOrDefault(n => n.GetAttributeValue("data-action", null) == "start") != null;
                CanRestartGame = buttons.SingleOrDefault(n => n.GetAttributeValue("data-action", null) == "PostGame_Continue") != null;

                var characterButtons = buttons.Where(n => n.HasClass("characters"));

                if (characterButtons.Any())
                {
                    GameState = Quiplash3State.SelectCharacter;

                    PlayerCharacter = characterButtons.SingleOrDefault(n => n.HasClass("selected") || n.HasClass("active"))
                        ?.GetClasses()?.Single(c => !_characterButtonClasses.Contains(c));

                    AvailableCharacters = characterButtons
                        .Where(n => !n.HasClass("selected") && !n.HasClass("disabled") && !n.HasClass("active"))
                        .Select(n => n.GetClasses().Single(c => !_characterButtonClasses.Contains(c))).ToList();
                }
            }
            else if (stateDiv.HasClass("Logo"))
            {
                GameState = Quiplash3State.Logo;

                // TODO handle round counter
            }
            else if (stateDiv.HasClass("EnterSingleText"))
            {
                var doneDiv = divs.SingleOrDefault(n => n.HasClass("enterSingleTextDone"));

                if (doneDiv.GetAttributeValue("style", null) == "display: none;")
                {
                    GameState = Quiplash3State.SingleAnswer;

                    var promptDivs = divs.Single(n => n.Id == "prompt").Descendants("div");
                    var headerDiv = promptDivs.Single(n => n.HasClass("header"));
                    var questionDiv = headerDiv.NextSibling;

                    Question = new KeyValuePair<string, string>(headerDiv.InnerText, questionDiv.InnerText);
                }
                else
                {
                    GameState = Quiplash3State.SingleAnswerDone;
                }
            }
            else if (stateDiv.HasClass("EnterTextList"))
            {
                var doneDiv = divs.SingleOrDefault(n => n.HasClass("enterTextListDone"));

                if (doneDiv.GetAttributeValue("style", null) == "display: none;")
                {
                    GameState = Quiplash3State.MultipleAnswers;

                    var promptDivs = divs.Single(n => n.Id == "prompt").Descendants("div");
                    var headerDiv = promptDivs.Single(n => n.HasClass("header"));
                    var questionDiv = headerDiv.NextSibling;

                    Question = new KeyValuePair<string, string>(headerDiv.InnerText, questionDiv.InnerText);
                }
                else
                {
                    GameState = Quiplash3State.MultipleAnswersDone;
                }
            }
            else if (stateDiv.HasClass("MakeSingleChoice"))
            {
                GameState = Quiplash3State.Vote;

                var voteButtons = buttons.Where(n => n.GetAttributeValue("data-action", null) == "choose");
                VoteOptions = voteButtons.ToDictionary(
                    n => n.GetAttributeValue("data-index", null),
                    n => n.HasChildNodes ? string.Join(Environment.NewLine, n.ChildNodes.Select(c => c.InnerText)) : n.InnerText);
            }
        }
    }
}
