using HtmlAgilityPack;
using System;
using System.Drawing;
using System.Linq;

namespace Quipbot
{
    public abstract class GameObserverBase : IGameObserver
    {
        public string SignInPageClass { get; } = "page-signin";

        public string GamePageClass { get; } = "quiplash2-page";

        public PageState PageState { get; protected set; }

        public string? PlayerName { get; protected set; }

        public void UpdateState(string html)
        {
            try
            {
                var htmlDocument = new HtmlDocument();
                htmlDocument.LoadHtml(html);

                var divs = htmlDocument.DocumentNode.Descendants("div");

                // check for disconnected
                var errorHeader = htmlDocument.DocumentNode.Descendants("h2").FirstOrDefault(n => n.HasClass("swal2-title"));
                if (errorHeader != null)
                {
                    if (errorHeader.InnerText == "Disconnected")
                    {
                        PageState = PageState.Disconnected;
                    }
                    else if (errorHeader.InnerText == "Error")
                    {
                        var errorContent = divs.FirstOrDefault(n => n.Id == "swal2-content")?.InnerText;

                        if (errorContent == "Room not found")
                            PageState = PageState.SignInFailed;
                    }

                    return;
                }

                // check if the landing page (signin) is visible
                var signinPage = divs.SingleOrDefault(node => node.Id == SignInPageClass);
                if (signinPage != null)
                {
                    PageState = PageState.SignIn;
                    ResetGameState();
                    return;
                }

                // swal2-title h2 Disconnected

                // swal2-title h2 Error
                // swal2-content div Room not found

                ResetGameState();
                UpdateGameState(htmlDocument);
            }
            catch (Exception)
            {
                PageState = PageState.Unknown;
                ResetGameState();
                throw;
            }
        }

        protected abstract void UpdateGameState(HtmlDocument htmlDocument);

        protected abstract void ResetGameState();

        protected static Color ParseBackgroundColor(string styleValue)
        {
            if (styleValue == null)
                throw new ArgumentNullException(nameof(styleValue));

            if (!styleValue.StartsWith("background-color: rgb("))
                throw new ArgumentException("Excepted background-color as style.", nameof(styleValue));

            if (!styleValue.EndsWith(");"))
                throw new ArgumentException("Excepted background-color as style.", nameof(styleValue));

            var trimmedValue = styleValue.Split(new[] { '(', ')' }, StringSplitOptions.RemoveEmptyEntries);

            if (trimmedValue.Length != 3)
                throw new ArgumentException("Excepted background-color as style.", nameof(styleValue));

            var splitValues = trimmedValue[1].Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

            if (splitValues.Length != 3)
                throw new ArgumentException("Excepted background-color as style.", nameof(styleValue));

            return Color.FromArgb(int.Parse(splitValues[0]), int.Parse(splitValues[1]), int.Parse(splitValues[2]));
        }
    }
}
