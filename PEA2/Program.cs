/// Projektowanie efektywnych algorytmów - zadanie 2
/// Filip Przygoński, 248892

using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace PEA
{
    static class Program
    {
        static MatrixGraph graph;
        static IList<int> solution;
        static Stopwatch stopwatch = new Stopwatch();
        static Action<IList<int>, int, int> neighbourhoodType = Algorithms.Swap;
        static int neighbourhoodTypeInt = 1;
        static long timePerform = long.MaxValue / 1000;
        static bool diversification = true;

        public static void Main(string[] args)
        {
            ConsoleMenu();
        }

        static void ConsoleMenu()
        {
            while (true)
            {
                Console.WriteLine(graph == null ? "\nNIE WCZYTANO MIAST." : "\nWCZYTANO MIASTA.");
                switch (neighbourhoodTypeInt)
                {
                    case 1:
                        Console.Write("Sąsiedztwo: Swap, ");
                        break;
                    case 2:
                        Console.Write("Sąsiedztwo: Reverse, ");
                        break;
                }

                Console.Write(timePerform != long.MaxValue / 1000
                    ? $"Maksymalny czas: {timePerform}s, "
                    : "Brak limitu czasu, ");


                Console.Write($"Dywersyfikacja włączona: {diversification}\n");


                Console.WriteLine(
                    @"1. Wczytanie danych z pliku
2. Wprowadzenie kryterium stopu
3. Włączenie/Wyłączenie dywersyfikacji
4. Wybór sąsiedztwa
5. Algorytm symulowanego wyżarzania
6. Algorytm tabu search
0. Wyjdź");

                var strInput = Console.ReadLine();
                int intInput;
                try
                {
                    intInput = int.Parse(strInput);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                    continue;
                }

                switch (intInput)
                {
                    case 1:
                        Console.WriteLine("Podaj ścieżkę do pliku: ");
                        strInput = Console.ReadLine();
                        graph = ReadGraphFromFile(strInput);
                        break;
                    case 2:
                        Console.WriteLine("Podaj maksymalny czas w sekundach (0 - brak limitu)");
                        strInput = Console.ReadLine();
                        try
                        {
                            intInput = int.Parse(strInput);
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(e.Message);
                            break;
                        }

                        timePerform = intInput > 0 ? intInput : long.MaxValue / 1000;

                        break;
                    case 3:
                        Console.WriteLine("Dywersyfikacja włączona? Y/N");
                        strInput = Console.ReadLine();
                        switch (strInput)
                        {
                            case "Y":
                            case "y":
                                diversification = true;
                                break;
                            case "N":
                            case "n":
                                diversification = false;
                                break;
                        }

                        break;
                    case 4:
                        Console.WriteLine("Wybór sąsiedztwa: 1. Swap, 2. Reverse");
                        strInput = Console.ReadLine();
                        try
                        {
                            intInput = int.Parse(strInput);
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(e.Message);
                            break;
                        }

                        switch (intInput)
                        {
                            case 1:
                                neighbourhoodTypeInt = 1;
                                neighbourhoodType = Algorithms.Swap;
                                break;
                            case 2:
                                neighbourhoodTypeInt = 2;
                                neighbourhoodType = Algorithms.Reverse;
                                break;
                        }

                        break;
                    case 5:
                        if (graph != null)
                            PerformAlgorithm(Algorithms.SimulatedAnnealing);
                        break;
                    case 6:
                        if (graph != null)
                            PerformAlgorithm(Algorithms.TabuSearch);
                        break;
                    case 0:
                        Environment.Exit(0);
                        break;
                    default:
                        Console.WriteLine("Niepoprawna opcja.");
                        break;
                }
            }
        }

        static void PerformAlgorithm(Func<MatrixGraph, Action<IList<int>, int, int>, long, bool, IList<int>> algorithm)
        {
            stopwatch.Restart();
            solution = algorithm(graph, neighbourhoodType, timePerform, diversification);
            stopwatch.Stop();
            Console.WriteLine($"Czas: {stopwatch.ElapsedMilliseconds / 1000.0}s");
            Console.Write("Trasa: ");
            WriteList(solution);
            Console.Write("Koszt: ");
            Console.WriteLine(graph.CalculateRoute(solution));
        }

        static MatrixGraph ReadGraphFromFile(string filePath)
        {
            string[] lines;
            int cities;
            try
            {
                lines = System.IO.File.ReadAllLines(filePath);
                cities = int.Parse(lines[0]);
                if (cities < 1)
                    throw new Exception("Liczba miast musi być większa od 0");
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return null;
            }

            var readGraph = new MatrixGraph(cities);
            for (int i = 0; i < cities; i++)
            {
                char[] ss = {' ', '\t'};
                var strNumbers = lines[i + 1].Split(ss, StringSplitOptions.RemoveEmptyEntries);
                var intNumbers = new List<int>();
                try
                {
                    for (int j = 0; j < cities; j++)
                    {
                        int a = int.Parse(strNumbers[j]);
                        if (i == j)
                        {
                            a = int.MaxValue;
                        }

                        intNumbers.Add(a);
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                    return null;
                }

                readGraph.SetMatrixRow(i, intNumbers);
            }

            return readGraph;
        }

        public static void WriteList<T>(IList<T> list)
        {
            foreach (var elem in list)
            {
                Console.Write($"{elem}, ");
            }

            Console.WriteLine();
        }
    }
}