using Quipbot.Providers;
using System;
using System.Linq;

namespace Quipbot.Games.Quiplash3
{
    public class Quiplash3Behavior : PlayerBehaviorBase<CleverbotProvider>
    {
        private readonly Random _random = new Random();

        private string? _lastQuestion = null;

        private string? _answer = null;

        private DateTime? _restartAt = null;

        public string? React(Quiplash3Observer gameObserver)
        {
            if (gameObserver == null)
                throw new ArgumentNullException(nameof(gameObserver));

            if (gameObserver.PageState != PageState.Connected)
                return null;

            if (gameObserver.GameState == Quiplash3State.Lobby)
            {
                if (!gameObserver.CanRestartGame)
                    return null;

                _restartAt ??= DateTime.UtcNow.AddSeconds(10);

                if (DateTime.UtcNow < _restartAt)
                    return null;

                _restartAt = null;
                return "document.querySelector(\"button[data-action='PostGame_Continue']\").click();";
            }
            else if (gameObserver.GameState == Quiplash3State.Logo)
            {
                _answer = null;
                _restartAt = null;
            }
            else if (gameObserver.GameState == Quiplash3State.SingleAnswer)
            {
                if (gameObserver.Question == null || _lastQuestion == gameObserver.Question.Value.Key)
                    return null;

                if (string.IsNullOrWhiteSpace(_answer))
                    _answer = ResultProvider.ProvideResult(gameObserver.Question.Value.Value, TimeSpan.FromSeconds(20)).GetAwaiter().GetResult();

                if (string.IsNullOrWhiteSpace(_answer))
                    return null;

                _lastQuestion = gameObserver.Question.Value.Key;

                var script = @$"
                    var input = document.querySelector(""textarea[id='input-text-textarea']"");
                    input.value = '{_answer.Substring(0, Math.Min(45, _answer.Length))}';
                    input.dispatchEvent(new Event('input', {{ bubbles: true }}));

                    setTimeout(function(){{ document.querySelector(""button[data-action='submit']"").click(); }}, 3000);
                ";

                _answer = null;
                return script;
            }
            else if (gameObserver.GameState == Quiplash3State.MultipleAnswers)
            {
                if (gameObserver.Question == null)
                    return null;

                string script = string.Empty;

                if (string.IsNullOrWhiteSpace(_answer))
                    _answer = ResultProvider.ProvideResult(gameObserver.Question.Value.Value, TimeSpan.FromSeconds(20)).GetAwaiter().GetResult();

                if (string.IsNullOrWhiteSpace(_answer))
                    return null;

                for (int i = 0; i < 3; i++)
                {
                    script += @$"
                        var input = document.querySelectorAll(""textarea[id='input-text-textarea']"")[{i}];
                        input.value = '{_answer.Substring(_answer.Length / 3 * i, Math.Min(30, _answer.Length / 3))}';
                        input.dispatchEvent(new Event('input', {{ bubbles: true }}));
                    ";
                }

                script += $"setTimeout(function(){{ document.querySelector(\"button[data-action='submit']\").click(); }}, 3000);";

                _answer = null;
                return script;
            }
            else if (gameObserver.GameState == Quiplash3State.Vote)
            {
                _answer = null;

                if (gameObserver.VoteOptions == null || !gameObserver.VoteOptions.Any())
                    return null;

                var voteId = _random.Next(gameObserver.VoteOptions.Count);

                return $"document.querySelector(\"button[data-action='choose'][data-index='{gameObserver.VoteOptions.Keys.ElementAt(voteId)}']\").click();";
            }

            return null;
        }

        public override string? React(IGameObserver gameObserver) => React((Quiplash3Observer)gameObserver);
    }
}
