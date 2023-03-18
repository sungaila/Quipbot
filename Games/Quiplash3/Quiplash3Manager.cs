using OpenAI.GPT3.ObjectModels;
using Quipbot.Providers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static OpenAI.GPT3.ObjectModels.Models;

namespace Quipbot.Games.Quiplash3
{
    public class Quiplash3Manager : GameManagerBase<Quiplash3Player>
    {
        private const int _minPlayerCount = 3;

        public override int MinPlayerCount { get => _minPlayerCount; }

        private const int _maxPlayerCount = 8;

        public override int MaxPlayerCount { get => _maxPlayerCount; }

        private static readonly PlayerSettings[] _predefinedPlayers = new[]
        {
            new PlayerSettings("Mr. Robot", "star-shaped quip", "funny"),
            new PlayerSettings("Kain", "three-eyed quip", "ridiculous"),
            new PlayerSettings("JC Denton", "kitten-shaped quip", "nonsensical"),
            new PlayerSettings("Evelynn", "coffin-shaped quip", "profane"),
            new PlayerSettings("Bella Goth", "cactus quip", "emotional"),
            new PlayerSettings("Wooldoor", "moon-shaped quip", "insane"),
            new PlayerSettings("Bobby ", "teardrop-shaped quip", "smug"),
            new PlayerSettings("Isaac", "poop-shaped quip", "emoji"),
        };

        public override async Task InitPlayersAsync(string roomCode)
        {
            for (int i = 0; i < Players.Count; i++)
            {
                Players[i].Name = _predefinedPlayers[i].Name;
                Players[i].Avatar = _predefinedPlayers[i].Avatar;
                ((OpenAIProvider)Players[i].Behavior.ResultProvider).PromptAdjective = _predefinedPlayers[i].Adjective;
            }

            await base.InitPlayersAsync(roomCode);
        }

        private record PlayerSettings(string Name, string Avatar, string Adjective);
    }
}
