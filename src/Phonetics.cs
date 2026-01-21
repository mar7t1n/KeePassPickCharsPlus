using System.Collections.Generic;

namespace PickCharsPlus
{
    public static class Phonetics 
    {

        private static readonly Dictionary<char, string> PhoneticMap = new Dictionary<char, string>
        {
            // Letters
            ['A'] = "Alpha", ['B'] = "Bravo", ['C'] = "Charlie", ['D'] = "Delta",
            ['E'] = "Echo", ['F'] = "Foxtrot", ['G'] = "Golf", ['H'] = "Hotel",
            ['I'] = "India", ['J'] = "Juliett", ['K'] = "Kilo", ['L'] = "Lima",
            ['M'] = "Mike", ['N'] = "November", ['O'] = "Oscar", ['P'] = "Papa",
            ['Q'] = "Quebec", ['R'] = "Romeo", ['S'] = "Sierra", ['T'] = "Tango",
            ['U'] = "Uniform", ['V'] = "Victor", ['W'] = "Whiskey", ['X'] = "X-ray",
            ['Y'] = "Yankee", ['Z'] = "Zulu",

            // Numbers
            ['0'] = "Zero", ['1'] = "One", ['2'] = "Two", ['3'] = "Three",
            ['4'] = "Four", ['5'] = "Five", ['6'] = "Six", ['7'] = "Seven",
            ['8'] = "Eight", ['9'] = "Nine",

            // Special characters
            ['!'] = "Exclamation", ['@'] = "At", ['#'] = "Hash", ['$'] = "Dollar",
            ['%'] = "Percent", ['^'] = "Caret", ['&'] = "Ampersand", ['*'] = "Asterisk",
            ['('] = "Left Parenthesis", [')'] = "Right Parenthesis", ['-'] = "Hyphen",
            ['_'] = "Underscore", ['='] = "Equals", ['+'] = "Plus", ['['] = "Left Bracket",
            [']'] = "Right Bracket", ['{'] = "Left Brace", ['}'] = "Right Brace",
            [';'] = "Semicolon", [':'] = "Colon", ['\''] = "Quote", ['"'] = "Double Quote",
            [','] = "Comma", ['.'] = "Period", ['<'] = "Less Than", ['>'] = "Greater Than",
            ['?'] = "Question", ['/'] = "Slash", ['\\'] = "Backslash", ['|'] = "Pipe",
            [' '] = "Space"
        };

        // optional: add lowercase mapping to distinguish upper/lower
        public static string GetPhonetic(char c)
        {
            char upper = char.ToUpperInvariant(c);
            if (PhoneticMap.TryGetValue(upper, out var word))
            {
                return char.IsUpper(c) ? word.ToUpper() : word;
            }
            return $"Unknown ({c})";
        }
    }
}
