using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using ShaneSpace.VisualStudio.InvisibleCharacterVisualizer.Core;
using System.Linq;
using System.Text.RegularExpressions;

namespace Console1
{
    public class Program
    {
        static void Main(string[] args)
        {
            var outputPath = $"{Environment.GetFolderPath(Environment.SpecialFolder.UserProfile)}/ShaneSpace.VisualStudio.InvisibleCharacterVisualizer";
            var unicodeDictionary = GetUnicodeDictionary();
            var homoglyphDictionary = GetHomoglyphUnicodeDictionary();
            Directory.CreateDirectory(outputPath);
            //GenerateDictionaryEntries(outputPath, unicodeDictionary);
            //GenerateReadMeEntries(outputPath, unicodeDictionary);

            GenerateHomoglyphDictionaryEntries(outputPath, homoglyphDictionary, unicodeDictionary);
        }

        private static void GenerateHomoglyphDictionaryEntries(string outputPath, Dictionary<string, HomoglyphInfo> homoglyphDictionary, Dictionary<string, string> unicodeDictionary)
        {
            File.WriteAllText(Path.Combine(outputPath, "homoglyph-regex.txt"), string.Empty, Encoding.Unicode);
            File.AppendAllText(Path.Combine(outputPath, "homoglyph-regex.txt"), string.Join("|", UnicodeHelper.HomoglyphDictionary.Value.Keys), Encoding.Unicode);

            outputPath = Path.Combine(outputPath, "homoglyph.txt");
            File.WriteAllText(outputPath, string.Empty, Encoding.Unicode);
            foreach (var homoglyph in homoglyphDictionary)
            {
                var homoglyphCharString = UnicodeHexToString(homoglyph.Key);
                File.AppendAllText(outputPath, $"                {{ \"{homoglyph.Value.Characters}\", new HomoglyphInfo(\"{homoglyph.Value.Characters}\", \"{homoglyph.Value.HomoglyphCharacters}\", \"({homoglyph.Value.Characters} → {homoglyph.Value.HomoglyphCharacters}) {homoglyph.Value.Description}\") }},{Environment.NewLine}", Encoding.Unicode);
            }
        }

        private static string UnicodeHexToString(string hex)
        {
            if (hex.Contains(" "))
            {
                var split = hex.Split(" ");
                var chars = split
                    .SelectMany(x => UnicodeHexToString(x.ToString()))
                    .ToArray();
                var output = new string(chars);
                return output;
            }

            var code = int.Parse(hex, NumberStyles.HexNumber);
            var unicodeString = char.ConvertFromUtf32(code);
            return unicodeString;
        }

        private static void GenerateDictionaryEntries(string outputPath, Dictionary<string, string> unicodeDictionary)
        {
            outputPath = Path.Combine(outputPath, "unicode.txt");
            File.WriteAllText(outputPath, string.Empty);
            for (int i = char.MinValue; i <= char.MaxValue; i++)
            {
                var character = Convert.ToChar(i);
                var characterString = character.ToString();
                var category = CharUnicodeInfo.GetUnicodeCategory(character);
                var isMatched = UnicodeHelper.InvisibleCharacterRegex.Match(characterString);
                var hex = characterString.ToHex();

                if (isMatched.Success)
                {
                    var name = GetUnicodeCharacterName(unicodeDictionary, hex);

                    try
                    {
                        File.AppendAllText(outputPath, $"                {{ \"{hex}\", new UnicodeInfo({name}, {category}) }},{Environment.NewLine}");
                    }
                    catch (EncoderFallbackException ex)
                    {
                        File.AppendAllText(outputPath, $"// {ex.Message}{Environment.NewLine}");
                    }
                }
            }
        }

        private static void GenerateReadMeEntries(string outputPath, Dictionary<string, string> unicodeDictionary)
        {
            var seperator = new string('=', 25);
            outputPath = Path.Combine(outputPath, "readme.txt");
            var unicodeCharacters = Enumerable
                .Range(char.MinValue, char.MaxValue)
                .Select(x => new Tuple<char, UnicodeCategory>(Convert.ToChar(x), CharUnicodeInfo.GetUnicodeCategory(Convert.ToChar(x))))
                .Where(x => UnicodeHelper.InvisibleCharacterRegex.IsMatch(x.Item1.ToString()));

            File.WriteAllText(outputPath, $"Uses the Regex pattern `{UnicodeHelper.InvisibleCharacterRegex}` to identify {unicodeCharacters.Count():n0} invisible characters.{Environment.NewLine}{Environment.NewLine}");

            var unicodeCategories = unicodeCharacters.GroupBy(x => x.Item2, v => v.Item1);
            foreach (var unicodeCategory in unicodeCategories)
            {
                var category = unicodeCategory.Key;
                File.AppendAllText(outputPath, $"{category}{Environment.NewLine}{seperator}{Environment.NewLine}");
                if (category == UnicodeCategory.OtherNotAssigned || category == UnicodeCategory.PrivateUse || category == UnicodeCategory.Surrogate)
                {
                    File.AppendAllText(outputPath, $"{unicodeCategory.Count():n0} Characters.{Environment.NewLine}{Environment.NewLine}");
                    continue;
                }
                foreach (var unicodeCharacter in unicodeCategory)
                {
                    var character = unicodeCharacter;
                    var hex = character.ToString().ToHex();

                    var name = "NOT ASSIGNED";
                    if (unicodeDictionary.ContainsKey(hex))
                    {
                        name = unicodeDictionary[hex];
                        if (string.IsNullOrWhiteSpace(name))
                        {
                            name = "NO NAME";
                        }
                    }
                    try
                    {
                        File.AppendAllText(outputPath, $"{hex} [{character}] \"{name}\"{Environment.NewLine}");
                    }
                    catch (EncoderFallbackException ex)
                    {
                        File.AppendAllText(outputPath, $"{hex} {name} // {ex.Message}{Environment.NewLine}");
                    }
                }
                File.AppendAllText(outputPath, Environment.NewLine);
            }
        }

        private static Dictionary<string, string> GetUnicodeDictionary()
        {
            // https://www.unicode.org/Public/UCD/latest/ucd/UnicodeData.txt
            var unicodeData = File.ReadAllLines("UnicodeData.txt");
            var unicodeDictionary = new Dictionary<string, string>();
            foreach (var unicodeDataLine in unicodeData)
            {
                var splitString = unicodeDataLine.Split(";");
                var key = splitString[0];
                var value = splitString[1] == "<control>" ? splitString[10] : splitString[1];
                unicodeDictionary.Add($"0x{key}", value);
            }

            return unicodeDictionary;
        }

        private static Dictionary<string, HomoglyphInfo> GetHomoglyphUnicodeDictionary()
        {
            // https://www.unicode.org/Public/security/latest/confusables.txt
            // 01C3 ;	0021 ;	MA	# ( ǃ → ! ) LATIN LETTER RETROFLEX CLICK → EXCLAMATION MARK	# 
            var unicodeData = File.ReadAllLines("confusables.txt");
            var unicodeDictionary = new Dictionary<string, HomoglyphInfo>();
            var standardCharacters = new Regex("^[a-zA-Z0-9`~!@#$%^&*()_+|=\\][{}';\":/.,?><-]*$", RegexOptions.Compiled);
            var lineRegex = new Regex(@"(.+);\t(.+);\tMA\t#.+?\((.+?)→(.+?)\) (.+?)\t# ", RegexOptions.Compiled);
            foreach (var unicodeDataLine in unicodeData)
            {
                if (unicodeDataLine.StartsWith("#") || string.IsNullOrWhiteSpace(unicodeDataLine))
                {
                    continue;
                }
                var splitString = unicodeDataLine.Split(";").Select(x => x.Trim()).ToArray();
                var lineData = lineRegex.Match(unicodeDataLine);
                var characterHex = lineData.Groups[1].Value.Trim();
                var confusableHex = lineData.Groups[2].Value.Trim();
                var character = lineData.Groups[3].Value.Trim();
                var confusable = lineData.Groups[4].Value.Trim();
                var description = lineData.Groups[5].Value.Trim();
                if (string.IsNullOrWhiteSpace(confusable) || !standardCharacters.IsMatch(confusable) || standardCharacters.IsMatch(character))
                {
                    continue;
                }
                var homoglyph = new HomoglyphInfo(character, confusable, description);
                unicodeDictionary.Add(characterHex, homoglyph);
            }

            return unicodeDictionary;
        }

        private static string GetUnicodeCharacterName(Dictionary<string, string> unicodeDictionary, string hex)
        {
            var name = "NotAssigned";
            if (unicodeDictionary.ContainsKey(hex))
            {
                name = $"\"{unicodeDictionary[hex]}\"";
                if (name == "\"\"")
                {
                    name = "NoName";
                }
            }

            return name;
        }
    }
}
