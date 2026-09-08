using System;
using System.Text;

namespace StringReverseSolutions
{
    class Program
    {
        static void Main(string[] args)
        {
            string word = "привет";
            Console.WriteLine($"исходное: {word}");
            Console.WriteLine($"перевернутое: {ReverseWord(word)}");
            
            string sentence = "привет мир";
            Console.WriteLine($"исходное: {sentence}");
            Console.WriteLine($"перевернутое: {ReverseWordsInSentence(sentence)}");
            
            Console.ReadKey();
        }

        static string ReverseWord(string word)
        {
            if (string.IsNullOrEmpty(word))
                return word;
            
            char[] chars = word.ToCharArray();
            Array.Reverse(chars);
            return new string(chars);
        }
        
        static string ReverseWordsInSentence(string sentence)
        {
            if (string.IsNullOrEmpty(sentence))
                return sentence;
            
            string[] words = sentence.Split(' ');
            
            for (int i = 0; i < words.Length; i++)
            {
                words[i] = ReverseWord(words[i]);
            }
            
            return string.Join(" ", words);
        }
    }
}