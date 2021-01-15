/// Projektowanie efektywnych algorytmów - zadanie 1
/// Filip Przygoński, 248892
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace PEA
{
    static class Program
    {
        public static void Main(string[] args)
        {
            ConsoleMenu();
        }

        static MatrixGraph graph = null;
        static IList<int> bestPermutation;
        static Stopwatch stopwatch = new Stopwatch();

        static void ConsoleMenu()
        {
            bool running = true;
            string strInput;
            int intInput;
            while (running)
            {
                if (graph == null)
                {
                    Console.WriteLine("\nNIE WCZYTANO MIAST.");
                }
                else
                {
                    Console.WriteLine("\nWCZYTANO MIASTA.");
                }
                Console.WriteLine("1. Przegląd zupełny\n2. Programowanie dynamiczne\n3. Podział i ograniczenia - przeszukiwanie wszerz\n4. Podział i ograniczenia - najpierw najlepszy\n8. Kompletny test\n9. Wczytaj zadania z pliku\n0. Wyjdź");

                strInput = Console.ReadLine();
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
                        if (graph != null)
                        {
                            PerformAlgorithm(Algorithms.BruteForce);
                        }
                        break;
                    case 2:
                        if (graph != null)
                        {
                            PerformAlgorithm(Algorithms.DynamicProgramming);
                        }
                        break;
                    case 3:
                        if (graph != null)
                        {
                            PerformAlgorithm(Algorithms.BranchAndBoundBreadthSearch);
                        }
                        break;
                    case 4:
                        if (graph != null)
                        {
                            PerformAlgorithm(Algorithms.BranchAndBoundBestFirst);
                        }
                        break;
                    case 8:
                        Console.WriteLine("Przeprowadzanie kompletnego testu.");
                        CompleteTest();
                        break;
                    case 9:
                        Console.WriteLine("Podaj ścieżkę do pliku: ");
                        strInput = Console.ReadLine();
                        graph = ReadGraphFromFile(strInput);
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

        static void PerformAlgorithm(Func<MatrixGraph, IList<int>> algorithm)
        {
            stopwatch.Restart();
            bestPermutation = algorithm(graph);
            stopwatch.Stop();
            Console.WriteLine(string.Format("Czas: {0}s", stopwatch.ElapsedMilliseconds / 1000.0));
            Console.Write("Trasa: ");
            WriteList(bestPermutation);
            Console.Write("Koszt: ");
            Console.WriteLine(graph.CalculateRoute(bestPermutation));
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
                char[] ss = { ' ', '\t' };
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

        static long TestAlgorithm(Func<MatrixGraph, IList<int>> algorithm, int problemSize)
        {
            int tests = 100;
            MatrixGraph graph;
            Stopwatch stopwatch = new Stopwatch();
            long nanosecondsPerTick = 1000000000L / Stopwatch.Frequency;
            long time = 0; // w nanosekundach
            for (int i = 0; i < tests + 1; ++i)
            {
                graph = MatrixGraph.GenerateRandomGraph(problemSize);
                stopwatch.Reset();
                stopwatch.Start();
                var bestPermutation = algorithm(graph);
                stopwatch.Stop();
                // wyrzucenie pierwszego testu, który może zepsuć średnią
                if (i != 0)
                {
                    time += stopwatch.ElapsedTicks * nanosecondsPerTick;
                }
            }
            return time /= tests;
        }

        static void CompleteTest()
        {
            int[] problemSizes = { 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20 };
            using (System.IO.StreamWriter file = new System.IO.StreamWriter("results.csv", false))
            {
                file.WriteLine("Rozmiar;BruteForce;Dynamic;BnBwszerz;BnBpierwszy");
            }
            foreach (int size in problemSizes)
            {
                double timeBruteForce = 0, timeDynamic = 0, timeBnBBreadth = 0, timeBnBBestFirst = 0;
                // czasy w milisekundach
                timeBruteForce = TestAlgorithm(Algorithms.BruteForce, size) / 1000000.0;
                timeDynamic = TestAlgorithm(Algorithms.DynamicProgramming, size) / 1000000.0;
                timeBnBBreadth = TestAlgorithm(Algorithms.BranchAndBoundBreadthSearch, size) / 1000000.0;
                timeBnBBestFirst = TestAlgorithm(Algorithms.BranchAndBoundBestFirst, size) / 1000000.0;
                using (System.IO.StreamWriter file = new System.IO.StreamWriter("results.csv", true))
                {
                    string str = string.Format("{0};{1};{2};{3};{4}", size, timeBruteForce, timeDynamic, timeBnBBreadth, timeBnBBestFirst);
                    file.WriteLine(str);
                    Console.WriteLine(str);
                }
            }
        }

        public static void WriteList<T>(IList<T> list)
        {
            foreach (var elem in list)
            {
                Console.Write(string.Format("{0}, ", elem.ToString()));
            }
            Console.WriteLine();
        }
    }
}
