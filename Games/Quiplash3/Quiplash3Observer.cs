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

        public string? Question { get; protected set; }

        public int RequestedAnswers { get; protected set; }

        public List<string>? VoteOptions { get; protected set; }

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

        protected override void UpdateGameState(HtmlDocument htmlDocument)
        {
            var gameDiv = htmlDocument.DocumentNode.Descendants("div").FirstOrDefault(n => n.HasClass("quiplash3"));

            if (gameDiv == null)
            {
                PageState = PageState.Unknown;
                return;
            }

            var buttons = gameDiv.Descendants("button").ToList();

            PageState = PageState.Connected;

            // read player name and character
            PlayerName = gameDiv.Descendants("span").SingleOrDefault(n => n.HasClass("name"))?.InnerText;
            PlayerCharacter = gameDiv.Descendants("img").SingleOrDefault(n => n.HasClass("avatar"))?.GetAttributeValue("alt", null);

            if (gameDiv.Descendants("div").SingleOrDefault(n => n.HasClass("lobby")) is HtmlNode lobbyDiv)
            {
                GameState = Quiplash3State.SelectCharacter;
                CanStartGame = buttons.SingleOrDefault(n => n.HasClass("action") && n.InnerText == "Press to Start") != null;

                if (gameDiv.Descendants("div").SingleOrDefault(n => n.HasClass("avatars")) is HtmlNode avatarsDiv)
                {
                    AvailableCharacters = avatarsDiv.Descendants("button")
                        .Where(n => n.HasClass("avatar"))
                        .Where(n => !n.HasClass("selected") && !n.Attributes.Any(a => a.Name == "disabled"))
                        .Select(n => n.Descendants("img").Single().GetAttributeValue("alt", null))
                        .ToList();
                }
            }
            else if (gameDiv.Descendants("div").SingleOrDefault(n => n.HasClass("waiting")) is HtmlNode waitingDiv)
            {
                GameState = Quiplash3State.Waiting;
            }
            else if (gameDiv.Descendants("div").SingleOrDefault(n => n.HasClass("writing")) is HtmlNode writingDiv)
            {
                GameState = Quiplash3State.Writing;
                Question = writingDiv.Descendants("label").SingleOrDefault(l => l.HasClass("prompt"))?.InnerText;
                RequestedAnswers = writingDiv.Descendants("textarea").Count();

                if (buttons.SingleOrDefault(n => n.HasClass("submit"))?.GetAttributes("disabled")?.FirstOrDefault() != null)
                {
                    GameState = Quiplash3State.SubmittingAnswer;
                }
            }
            else if (gameDiv.Descendants("div").SingleOrDefault(n => n.HasClass("voting")) is HtmlNode votingDiv)
            {
                GameState = Quiplash3State.Vote;
                VoteOptions = votingDiv.Descendants("button").Where(n => n.HasClass("choice")).Select(n => n.InnerText).ToList();
            }
            else if (gameDiv.Descendants("div").SingleOrDefault(n => n.HasClass("post-game")) is HtmlNode postgameDiv)
            {
                GameState = Quiplash3State.PostGame;
                CanRestartGame = buttons.SingleOrDefault(n => n.HasClass("action") && n.InnerText == "Same Players") != null;
            }
        }
    }
}
