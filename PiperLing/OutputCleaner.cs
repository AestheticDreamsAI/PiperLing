using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Cherry_Lite
{
    internal class OutputCleaner
    {
        public static string RemoveEmojis(string input)
        {
            if (string.IsNullOrEmpty(input))
                return input;

            // This regex pattern matches typical emojis by using ranges from the Unicode emoji blocks
            string emojiPattern = @"[\u1F600-\u1F64F" +    // Emoticons
                                   @"\u1F300-\u1F5FF" +    // Misc Symbols and Pictographs
                                   @"\u1F680-\u1F6FF" +    // Transport and Map Symbols
                                   @"\u1F700-\u1F77F" +    // Alchemical Symbols
                                   @"\u1F780-\u1F7FF" +    // Geometric Shapes Extended
                                   @"\u1F800-\u1F8FF" +    // Supplemental Arrows-C
                                   @"\u1F900-\u1F9FF" +    // Supplemental Symbols and Pictographs
                                   @"\u1FA70-\u1FAFF" +    // Symbols and Pictographs Extended-A
                                   @"\u2702-\u27B0" +      // Dingbats
                                   @"\u24C2-\u1F251" +     // Enclosed Characters (Corrected range)
                                   @"\u2600-\u26FF" +      // Miscellaneous Symbols
                                   @"\u2700-\u27BF" +      // Dingbats & Miscellaneous Symbols and Arrows
                                   @"\u2300-\u23FF]";      // Miscellaneous Technical
            var ss = Regex.Replace(input, @"[^\u0000-\u007F]+", "").Replace("ð", "").Trim();
            return ss;
        }
        public static string CleanString(string s)
        {
            s = System.Text.RegularExpressions.Regex.Replace(s, @"\[[^\]]*\]|\([^\)]*\)", "").Trim();
            var cleaned = RemoveEmojis(s).Replace("\n", " ").Trim().ToLower().Replace("*", "").Replace("\"", "").Trim();

            string timePattern = @"\b\d{1,2}:\d{2}\s*(AM|PM|am|pm)?\b";
            string generalPattern = @".*?:";

            if (Regex.IsMatch(s, timePattern))
            {
                return s;
            }
            return Regex.Replace(s, generalPattern, "").Replace("*","").Trim();
        }

    }
}
