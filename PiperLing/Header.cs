using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AI_Dollmetscher
{
    internal class Header
    {
        public static void Add(string text)
        {
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.WriteLine("______ _                 _     _             \r\n| ___ (_)               | |   (_)            \r\n| |_/ /_ _ __   ___ _ __| |    _ _ __   __ _ \r\n|  __/| | '_ \\ / _ \\ '__| |   | | '_ \\ / _` |\r\n| |   | | |_) |  __/ |  | |___| | | | | (_| |\r\n\\_|   |_| .__/ \\___|_|  \\_____/_|_| |_|\\__, |\r\n        | |   By AestheticDreamsAI      __/ |\r\n        |_|                            |___/ ");
            Console.ForegroundColor = ConsoleColor.Green;
            const int totalLength = 50; // Total length of the output
            int messageLength = text.Length;
            int dashesOnEachSide = (totalLength - messageLength) / 2;

            string padding = new string('-', dashesOnEachSide);
            string output = $"{padding}{text.ToUpper().Replace(" ", "-")}{padding}";

            // If the message length is odd, one more dash is needed at the end
            if (output.Length < totalLength)
            {
                output += "-";
            }

            Console.WriteLine(output);
        }
    }
}
