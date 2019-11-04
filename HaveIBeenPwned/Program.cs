using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace HaveIBeenPwned
{
    class HashItem
    {
        public string Hash { get; set; }
        public int Usages { get; set; }
    }

    class Program
    {
        static void Main(string[] args)
        {
            string password;

            if (args.Length <= 0)
            {
                Console.Write("Type in password: ");
                password = Console.ReadLine();
            }
            else
            {
                password = args[0];
            }

            var hash = Hash(password);
            var firstChars = hash.Substring(0, 5);

            Console.WriteLine($"Searching for password with hash: {hash}");

            var response = GetPwndHashes(firstChars).Result;
            var items = ConvertHashResponseToHashItems(response, firstChars);

            Console.WriteLine($"Number of pwned passwords with the same first hash chars: {items.Count}");

            var item = items.FirstOrDefault(e => e.Hash.Equals(hash));
            if (item == null)
            {
                Console.WriteLine();
                Console.WriteLine("CONGRATULATIONS: Your password is not owned!");
                Console.WriteLine();
            }
            else
            {
                Console.WriteLine();
                Console.WriteLine($"SORRY: Your password was owned at least {item.Usages} times!");
                Console.WriteLine();
            }
        }

        private static List<HashItem> ConvertHashResponseToHashItems(string response, string firstChars)
        {
            return response
                .Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries)
                .Select(l => l.Split(':'))
                .Select(e => new HashItem { Hash = firstChars + e[0], Usages = Convert.ToInt32(e[1]) })
                .ToList();
        }

        private static async Task<string> GetPwndHashes(string firstChars)
        {
            var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Add("User-Agent", "HaveIBeenPwned-App");

            var url = "https://api.pwnedpasswords.com/range/" + firstChars;
            return await httpClient.GetStringAsync(url);
        }

        static string Hash(string input)
        {
            using (SHA1Managed sha1 = new SHA1Managed())
            {
                var hash = sha1.ComputeHash(Encoding.UTF8.GetBytes(input));
                var sb = new StringBuilder(hash.Length * 2);

                foreach (byte b in hash)
                {
                    sb.Append(b.ToString("X2"));
                }

                return sb.ToString();
            }
        }
    }
}
