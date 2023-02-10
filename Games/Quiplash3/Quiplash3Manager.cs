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

        private static readonly string[] _predefinedNames = new[] { "Mr. Robot", "Kain", "JC Denton", "Evelynn", "Bella Goth", "Wooldoor", "Bobby B", "Isaac" };

        private static readonly string[] _predefinedAdjectives = new[] { "funny", "ridiculous", "nonsensical", "profane", "comical", "insane", "smug", "emoji" };

        public override async Task InitPlayersAsync(string roomCode)
        {
            for (int i = 0; i < Players.Count; i++)
            {
                Players[i].Name = _predefinedNames[i];
                ((OpenAIProvider)Players[i].Behavior.ResultProvider).PromptAdjective = _predefinedAdjectives[i];
            }

            await base.InitPlayersAsync(roomCode);
        }
    }
}
