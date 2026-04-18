using System;

namespace CaesarCipherApp
{
    class Program
    {
        static string CaesarCipher(string text, int key)
        {
            string result = "";

            foreach (char c in text)
            {
                if (!char.IsLetter(c))
                {
                    result += c;
                    continue;
                }

                char offset = char.IsUpper(c) ? 'A' : 'a';
                result += (char)((((c + key) - offset) % 26 + 26) % 26 + offset);
            }

            return result;
        }

        static void Main(string[] args)
        {
            Console.Write("Enter text: ");
            string input = Console.ReadLine();

            Console.Write("Enter shift key (number): ");
            int shift = int.Parse(Console.ReadLine());

            string encrypted = CaesarCipher(input, shift);
            Console.WriteLine($"Encrypted: {encrypted}");

            string decrypted = CaesarCipher(encrypted, -shift);
            Console.WriteLine($"Decrypted: {decrypted}");
            
            Console.ReadKey();
        }
    }
}
