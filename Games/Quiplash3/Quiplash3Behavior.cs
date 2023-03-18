using Quipbot.Providers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quipbot.Games.Quiplash3
{
    public class Quiplash3Behavior : PlayerBehaviorBase<OpenAIProvider>
    {
        private const bool IsVotingEnabled = false;
        private readonly TimeSpan _votingDelay = TimeSpan.FromSeconds(0);
        private readonly TimeSpan _restartDelay = TimeSpan.FromSeconds(30);

        private readonly Random _random = new();

        private List<string>? _answers = null;

        private DateTime? _restartAt = null;

        public async Task<string?> ReactAsync(Quiplash3Observer gameObserver, Quiplash3Player player)
        {
            if (gameObserver == null)
                throw new ArgumentNullException(nameof(gameObserver));

            if (gameObserver.PageState != PageState.Connected)
                return null;

            if (gameObserver.GameState == Quiplash3State.SelectCharacter)
            {
                _answers = null;
                _restartAt = null;

                if (player.Avatar != null && player.Avatar != gameObserver.PlayerCharacter && gameObserver.AvailableCharacters != null && gameObserver.AvailableCharacters.Contains(player.Avatar))
                {
                    return @$"document.querySelector(""button[class='avatar']>img[alt='{player.Avatar}']"").parentNode.click();";
                }
            }
            else if (gameObserver.GameState == Quiplash3State.Waiting)
            {
                _answers = null;
                _restartAt = null;
            }
            else if (gameObserver.GameState == Quiplash3State.Writing)
            {
                if (gameObserver.Question == null || gameObserver.RequestedAnswers <= 0)
                    return null;

                _answers ??= await ResultProvider.ProvideResult(gameObserver.Question!, gameObserver.RequestedAnswers, TimeSpan.FromSeconds(20)).ToListAsync();

                try
                {
                    var scriptBuilder = new StringBuilder();

                    scriptBuilder.AppendLine(@"var inputs = document.querySelectorAll(""div[class='answer']>textarea"");");

                    for (int i = 0; i < _answers.Count && i < gameObserver.RequestedAnswers; i++)
                    {
                        scriptBuilder.AppendLine(@$"
                                inputs[{i}].value = '{_answers[i].Substring(0, Math.Min(45, _answers[i].Length))}';
                                inputs[{i}].dispatchEvent(new Event('input', {{ bubbles: true }}));
                            ");
                    }

                    scriptBuilder.AppendLine(@"document.querySelector(""button[class='submit']"").click();");
                    return scriptBuilder.ToString();
                }
                finally
                {
                    _answers = null;
                }
            }
            else if (gameObserver.GameState == Quiplash3State.Vote)
            {
                _answers = null;

                if (!IsVotingEnabled || gameObserver.VoteOptions == null || !gameObserver.VoteOptions.Any())
                    return null;

                var voteId = _random.Next(gameObserver.VoteOptions.Count);

                await Task.Delay(_votingDelay);

                return $"document.querySelectorAll(\"button[class='choice']\")[{voteId}].click();";
            }
            else if (gameObserver.GameState == Quiplash3State.PostGame)
            {
                if (!gameObserver.CanRestartGame)
                    return null;

                _restartAt ??= DateTime.UtcNow.Add(_restartDelay);

                if (DateTime.UtcNow < _restartAt)
                    return null;

                _restartAt = null;
                return "document.querySelectorAll(\"div[class='post-game-actions vip']>button\")[0].click();";
            }

            return null;
        }

        public override Task<string?> ReactAsync(IGameObserver gameObserver, IPlayer player) => ReactAsync((Quiplash3Observer)gameObserver, (Quiplash3Player)player);
    }
}
