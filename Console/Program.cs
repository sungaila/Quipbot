using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Quipbot.Console
{
    public class Program
    {
        [STAThread]
        public static void Main(string[] args)
        {
            var entryAssembly = Assembly.GetEntryAssembly();
            System.Console.WriteLine($"{entryAssembly?.GetName()?.ToString() ?? "Quipbot"}");

            // find game managers via reflection (search each assembly currently loaded in the AppDomain)
            IEnumerable<Type>? knownGameManagers = GetGameManagerTypes();
            System.Console.WriteLine($"[{string.Join(", ", knownGameManagers.Select(t => t.Name).OrderBy(t => t))}]");
            System.Console.WriteLine();

            // parse the input arguments
            ParseArguments(args, knownGameManagers, out Type? gameManagerType, out string? roomCode, out int? playerCount);
            // prompt any missing arguments in the console
            ParseConsoleInput(knownGameManagers, ref gameManagerType, ref roomCode, ref playerCount);

            if (gameManagerType == null)
                throw new ArgumentNullException(nameof(gameManagerType));

            if (roomCode == null)
                throw new ArgumentNullException(nameof(roomCode));

            if (playerCount == null)
                throw new ArgumentNullException(nameof(playerCount));

            try
            {
                // instantiate, setup and run the requested game manager
                using var gameManager = CreateManagerInstance(gameManagerType);
                gameManager.SetupAsync(roomCode, playerCount.Value).GetAwaiter().GetResult();
                gameManager.RunAsync().GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                System.Console.WriteLine();
                System.Console.WriteLine(ex);
            }
        }

        private static void ParseArguments(string[] args, IEnumerable<Type>? knownGameManagers, out Type? gameManagerType, out string? roomCode, out int? playerCount)
        {
            gameManagerType = null;
            roomCode = null;
            playerCount = null;

            for (int i = 0; i < args.Length; i++)
            {
                var arg = args[i].ToLower();

                if (arg.StartsWith("-g") || arg.StartsWith("-game") || arg.StartsWith("-game-manager"))
                {
                    if (i + 1 >= args.Length)
                    {
                        System.Console.Error.WriteLine("ERROR: No game manager specified.");
                        return;
                    }
                    gameManagerType = knownGameManagers.FirstOrDefault(t => t.Name.ToLower() == args[i + 1].ToLower());

                    if (gameManagerType == null)
                        throw new ArgumentException($"Unknown game manager '{args[i + 1]}'.");

                    i++;
                    System.Console.WriteLine($"Game manager: {gameManagerType.Name}");
                    continue;
                }
                else if (arg.StartsWith("-r") || arg.StartsWith("-room") || arg.StartsWith("-room-code"))
                {
                    if (i + 1 >= args.Length)
                    {
                        System.Console.Error.WriteLine("ERROR: No room code specified.");
                        return;
                    }
                    roomCode = args[i + 1].ToUpper();
                    i++;
                    System.Console.WriteLine($"Room code: {roomCode}");
                    continue;
                }
                else if (arg.StartsWith("-p") || arg.StartsWith("-player") || arg.StartsWith("-player-count"))
                {
                    if (i + 1 >= args.Length)
                    {
                        System.Console.Error.WriteLine("ERROR: No player count specified.");
                        return;
                    }


                    if (!int.TryParse(args[i + 1], out int parsedInt))
                    {
                        System.Console.Error.WriteLine("ERROR: Player count was not a valid number.");
                        return;
                    }

                    playerCount = parsedInt;
                    i++;
                    System.Console.WriteLine($"Player count: {playerCount}");
                    continue;
                }
            }
        }

        private static void ParseConsoleInput(IEnumerable<Type>? knownGameManagers, ref Type? gameManagerType, ref string? roomCode, ref int? playerCount)
        {
            while (gameManagerType == null)
            {
                System.Console.Write("Enter game manager: ");
                var input = System.Console.ReadLine().ToLower();
                gameManagerType = knownGameManagers.FirstOrDefault(t => t.Name.ToLower() == input);
            }

            while (string.IsNullOrWhiteSpace(roomCode) || roomCode.Length != 4 || roomCode.Any(c => !char.IsLetter(c)))
            {
                System.Console.Write("Enter room code: ");
                roomCode = System.Console.ReadLine();
            }

            while (playerCount == null || playerCount < 1)
            {
                System.Console.Write("Enter player count: ");

                if (int.TryParse(System.Console.ReadLine(), out int parsedInt))
                    playerCount = parsedInt;
            }
        }

        private static IEnumerable<Type> GetGameManagerTypes()
        {
            // force the games assembly to load
            var unused = typeof(Quipbot.Games.Quiplash2.Quiplash2Player);
            unused = unused.GetType();

            return AppDomain.CurrentDomain
                     .GetAssemblies()
                     .SelectMany(a => a.GetTypes())
                     .Where(type => !type.IsAbstract && typeof(IGameManager).IsAssignableFrom(type));
        }

        private static IGameManager CreateManagerInstance(Type type)
        {
            if (type.IsAbstract)
                throw new ArgumentException($"{type.FullName} cannot be abstract.", nameof(type));

            if (!typeof(IGameManager).IsAssignableFrom(type))
                throw new ArgumentException($"{type.FullName} must implement {nameof(IGameManager)}.", nameof(type));

            return (IGameManager)Activator.CreateInstance(type)!;
        }
    }
}
