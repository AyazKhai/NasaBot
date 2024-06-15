using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace NasaBot.Objects
{
    public class Translator
    {
        public static string Translatesentence(string text, string to)
        {
            var url = $"https://translate.googleapis.com/translate_a/single?client=gtx&sl=auto&tl={to}&dt=t&q={HttpUtility.UrlEncode(text)}";
            var webClient = new WebClient { Encoding = Encoding.UTF8 };
            var result = webClient.DownloadString(url);
            try
            {
                result = result.Substring(4, result.IndexOf("\"", 4, StringComparison.Ordinal) - 4);
                return result;
            }
            catch (Exception e)
            {
                Console.WriteLine($"Transalte Error: {e.Message}");
                return null;
            }
        }


        public static string TranslateText(string text, string to)
        {
            List<string> sentences = SplitIntoSentences(text);
            for (int i = 0; i < sentences.Count; i++)
            {
                sentences[i] = Translatesentence(sentences[i], to);
            }
            return JoinSentences(sentences);
        }

        private static List<string> SplitIntoSentences(string text)
        {
            char[] separators = { '.' };

            // Разделение текста на предложения по точке
            string[] sentenceArray = text.Split(separators, StringSplitOptions.RemoveEmptyEntries);

            // Преобразование массива предложений в список для удобства
            List<string> sentences = new List<string>(sentenceArray);

            return sentences;
        }

        private static string JoinSentences(List<string> sentences)
        {
            // Объединение предложений в текст, добавляя точку и пробел после каждого предложения
            string joinedText = string.Join(" ", sentences);

            // Добавление точки в конце текста, если это необходимо
            if (!string.IsNullOrEmpty(joinedText) && !joinedText.EndsWith("."))
            {
                joinedText += ".";
            }

            return joinedText;
        }
    }
}
