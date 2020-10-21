using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Quipbot.Games.Quiplash3
{
    public class Quiplash3Manager : GameManagerBase<Quiplash3Player>
    {
        private const int _minPlayerCount = 3;

        public override int MinPlayerCount { get => _minPlayerCount; }

        private const int _maxPlayerCount = 8;

        public override int MaxPlayerCount { get => _maxPlayerCount; }

        private static readonly string[] _predefinedNames = new[] { "Taffer", "Mr. Robot", "Kain", "JC Denton", "Evelynn", "Bella Goth", "Wooldoor", "Bobby B", "Wright", "Isaac", "Best Waifu" };

        private static readonly Random _random = new Random();

        public override async Task InitPlayersAsync(string roomCode)
        {
            var names = new List<string>(_predefinedNames);

            for (int i = 0; i < Players.Count; i++)
            {
                var nameIndex = _random.Next(names.Count);
                Players[i].Name = names.ElementAt(nameIndex);
                names.RemoveAt(nameIndex);
            }

            Players[0].Behavior.ResultProvider.Visible = true;

            await base.InitPlayersAsync(roomCode);
        }
    }
}
