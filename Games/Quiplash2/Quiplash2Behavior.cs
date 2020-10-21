using Quipbot.Providers;
using System;
using System.Linq;

namespace Quipbot.Games.Quiplash2
{
    public class Quiplash2Behavior : PlayerBehaviorBase<CleverbotProvider>
    {
        private readonly Random _random = new Random();

        private string? _lastQuestion = null;

        private string? _answer = null;

        private DateTime? _restartAt = null;

        public string? React(Quiplash2Observer gameObserver)
        {
            if (gameObserver == null)
                throw new ArgumentNullException(nameof(gameObserver));

            if (gameObserver.PageState != PageState.Connected)
                return null;

            if (gameObserver.GameState == Quiplash2State.Lobby)
            {
                if (!gameObserver.CanRestartGame)
                    return null;

                _restartAt ??= DateTime.UtcNow.AddSeconds(10);

                if (DateTime.UtcNow < _restartAt)
                    return null;

                _restartAt = null;
                return "document.getElementById('quiplash-sameplayers').click();";
            }
            else if (gameObserver.GameState == Quiplash2State.Logo)
            {
                _answer = null;
                _restartAt = null;
            }
            else if (gameObserver.GameState == Quiplash2State.AnswerQuestion)
            {
                if (gameObserver.Question == null || _lastQuestion == gameObserver.Question)
                    return null;

                if (string.IsNullOrWhiteSpace(_answer))
                    _answer = ResultProvider.ProvideResult(gameObserver.Question, TimeSpan.FromSeconds(20)).GetAwaiter().GetResult();

                if (string.IsNullOrWhiteSpace(_answer))
                    return null;

                _lastQuestion = gameObserver.Question;

                var script = @$"
                    document.getElementById('quiplash-answer-input').value = '{_answer.Substring(0, Math.Min(45, _answer.Length))}';
                    setTimeout(function(){{ document.getElementById('quiplash-submit-answer').click(); }}, 3000);
                ";

                _answer = null;
                return script;
            }
            else if (gameObserver.GameState == Quiplash2State.Vote)
            {
                _answer = null;

                if (gameObserver.VoteOptions == null || !gameObserver.VoteOptions.Any())
                    return null;

                var voteId = _random.Next(gameObserver.VoteOptions.Count);

                return $"document.querySelector(\"button[data-vote='{gameObserver.VoteOptions.Keys.ElementAt(voteId)}']\").click();";
            }

            return null;
        }

        public override string? React(IGameObserver gameObserver) => React((Quiplash2Observer)gameObserver);
    }
}
