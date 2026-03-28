using System;
using System.Collections.Generic;
using System.Linq;

class Program
{
    static void Main()
    {
        List<int> numbers = new List<int> { 1, 2, 4, 8, 16, 3, 5, 20, 9, 11 };

        foreach (int n in numbers)
        {
            if (n % 2 == 0) Console.WriteLine(n);
        }

        for (int i = 1; i < numbers.Count; i += 2)
        {
            Console.WriteLine(numbers[i]);
        }

        long product = 1;
        foreach (int n in numbers)
        {
            if (n % 4 == 0) product *= n;
        }
        Console.WriteLine(product);

        Dictionary<string, string> locations = new Dictionary<string, string>
        {
            {"france", "paris"}, {"kazakhstan", "astana"}, {"italy", "rome"},
            {"germany", "berlin"}, {"japan", "tokyo"}, {"usa", "washington"},
            {"greece", "athens"}, {"egypt", "cairo"}, {"turkey", "ankara"},
            {"algeria", "algiers"}
        };

        bool foundCity = false;
        foreach (var city in locations.Values)
        {
            if (city.ToLower().StartsWith("a") && city.ToLower().EndsWith("a"))
            {
                Console.WriteLine(city.ToLower());
                foundCity = true;
            }
        }
        if (!foundCity) Console.WriteLine("город отсутствует");

        Dictionary<string, double> movies = new Dictionary<string, double>
        {
            {"movie1", 8.5}, {"movie2", 4.2}, {"movie3", 5.0}, {"movie4", 9.1},
            {"movie5", 5.5}, {"movie6", 7.2}, {"movie7", 3.1}, {"movie8", 6.0},
            {"movie9", 10.0}, {"movie10", 4.8}
        };

        var toRemove = movies.Where(m => m.Value >= 4 && m.Value <= 6).Select(m => m.Key).ToList();
        foreach (var key in toRemove)
        {
            movies.Remove(key);
        }

        foreach (var movie in movies)
        {
            Console.WriteLine($"{movie.Key}: {movie.Value}");
        }
    }
}