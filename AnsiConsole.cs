using System.Text.RegularExpressions;

namespace XCopyWrapper
{
    internal static class AnsiConsole
    {
        private static readonly Regex ansiMatch = new(@"[\x1b|\x33]\[([\d|;]+)m", RegexOptions.None);

        public static void WriteLineBlocking(string line, params string?[] args)
        {
            WriteLine(line, args);
            Console.ReadKey();
        }

        public static void WriteLine(string line, params string?[] args)
        {
            var str = string.Format(line, args);
            var m = ansiMatch.Matches(str).Where(x => x.Success && x.Groups.Count == 2);
            if (!m.Any()) return;

            var matches = m.ToArray();
            for(int i = 0; i < matches.Length; i++)
            {
                Match curMatch = matches[i];
                Match? nextMatch = i < matches.Length - 1 ? matches[i + 1] : null;

                var ansiContent = curMatch.Groups[1].Value;
                foreach(string code in ansiContent.Split(";"))
                {
                    if (int.TryParse(code, out int c)) ResolveAnsiControlCode(c);
                }

                var offset = curMatch.Index + curMatch.Length;
                var len = nextMatch?.Index - offset ?? (str.Length) - offset;

                var pstr = str.Substring(offset, len);
                Console.Write(pstr);
            }

            Console.WriteLine();
        }

        private static void ResolveAnsiControlCode(params int[] seqCodes)
        {
            foreach(int code in seqCodes)
            {
                switch(code)
                {
                    case 0:
                        Console.ResetColor(); break;
                    case 30:
                        Console.ForegroundColor = ConsoleColor.Black; break;
                    case 31:
                        Console.ForegroundColor = ConsoleColor.DarkRed; break;
                    case 32:
                        Console.ForegroundColor = ConsoleColor.DarkGreen; break;
                    case 33:
                        Console.ForegroundColor = ConsoleColor.DarkYellow; break;
                    case 34:
                        Console.ForegroundColor = ConsoleColor.DarkBlue; break;
                    case 35:
                        Console.ForegroundColor = ConsoleColor.DarkMagenta; break;
                    case 36:
                        Console.ForegroundColor = ConsoleColor.DarkCyan; break;
                    case 37:
                        Console.ForegroundColor = ConsoleColor.Gray; break;
                    case 40:
                        Console.BackgroundColor = ConsoleColor.Black; break;
                    case 41:
                        Console.BackgroundColor = ConsoleColor.DarkRed; break;
                    case 42:
                        Console.BackgroundColor = ConsoleColor.DarkGreen; break;
                    case 43:
                        Console.BackgroundColor = ConsoleColor.DarkYellow; break;
                    case 44:
                        Console.BackgroundColor = ConsoleColor.DarkBlue; break;
                    case 45:
                        Console.BackgroundColor = ConsoleColor.DarkMagenta; break;
                    case 46:
                        Console.BackgroundColor = ConsoleColor.DarkCyan; break;
                    case 47:
                        Console.BackgroundColor = ConsoleColor.Gray; break;
                    case 90:
                        Console.ForegroundColor = ConsoleColor.DarkGray; break;
                    case 91:
                        Console.ForegroundColor = ConsoleColor.Red; break;
                    case 92:
                        Console.ForegroundColor = ConsoleColor.Green; break;
                    case 93:
                        Console.ForegroundColor = ConsoleColor.Yellow; break;
                    case 94:
                        Console.ForegroundColor = ConsoleColor.Blue; break;
                    case 95:
                        Console.ForegroundColor = ConsoleColor.Magenta; break;
                    case 96:
                        Console.ForegroundColor = ConsoleColor.Cyan; break;
                    case 97:
                        Console.ForegroundColor = ConsoleColor.White; break;
                    case 100:
                        Console.BackgroundColor = ConsoleColor.DarkGray; break;
                    case 101:
                        Console.BackgroundColor = ConsoleColor.Red; break;
                    case 102:
                        Console.BackgroundColor = ConsoleColor.Green; break;
                    case 103:
                        Console.BackgroundColor = ConsoleColor.Yellow; break;
                    case 104:
                        Console.BackgroundColor = ConsoleColor.Blue; break;
                    case 105:
                        Console.BackgroundColor = ConsoleColor.Magenta; break;
                    case 106:
                        Console.BackgroundColor = ConsoleColor.Cyan; break;
                    case 107:
                        Console.BackgroundColor = ConsoleColor.White; break;
                };
            }
        }
    }
}
