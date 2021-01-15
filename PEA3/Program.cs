// Projektowanie efektywnych algorytmów - zadanie 3
// Filip Przygoński, 248892

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

namespace PEA
{
    static class Program
    {
        private const int Mutation1 = 1;
        private const int Mutation2 = 2;
        private const int Crossover1 = 1;
        private const int Crossover2 = 2;

        static MatrixGraph graph;
        static IList<int> solution;
        static long timeToPerform = 1000; // domyślnie sekunda
        static Stopwatch stopwatch = new Stopwatch();
        static int populationSize = 100;
        static double mutationCoefficient = 0.01;
        static double crossoverCoefficient = 0.8;
        static int mutationType = 1;
        static Func<IList<int>, IList<int>> mutation = Algorithms.MutationSwap;
        static int crossoverType = 1;
        static Func<IList<int>, IList<int>, IList<int>> crossover = Algorithms.PartiallyMatchedCrossover;

        public static void Main(string[] args)
        {
            ConsoleMenu();
        }

        static void ConsoleMenu()
        {
            while (true)
            {
                Console.WriteLine(graph == null ? "\nNIE WCZYTANO MIAST." : "\nWCZYTANO MIASTA.");

                Console.Write(
                    timeToPerform != long.MaxValue / 1000
                        ? $"Maksymalny czas: {timeToPerform / 1000}s, "
                        : "Brak limitu czasu, "
                );

                switch (mutationType)
                {
                    case 1:
                        Console.Write("Mutacja: Swap, ");
                        break;
                    case 2:
                        Console.Write("Mutacja: Reverse, ");
                        break;
                }

                switch (crossoverType)
                {
                    case 1:
                        Console.Write("Krzyżowanie: Partially Matched Crossover\n");
                        break;
                    case 2:
                        Console.Write("Krzyżowanie: Order Crossover\n");
                        break;
                }

                Console.Write(
                    $"Wielkość populacji: {populationSize}, Współczynnik mutacji: {mutationCoefficient}, Współczynnik krzyżowania: {crossoverCoefficient}");

                Console.WriteLine();
                Console.WriteLine();

                Console.WriteLine(
                    @"1. Wczytanie danych z pliku
2. Wprowadzenie kryterium stopu
3. Ustawienie wielkości populacji początkowej
4. Ustawienie współczynnika mutacji
5. Ustawienie współczynnika krzyżowania
6. Wybór metody mutacji
7. Wybór metody krzyżowania
8. Uruchomienie algorytmu genetycznego
0. Wyjdź");

                var strInput = Console.ReadLine();
                int intInput;
                double doubleInput;
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

                        timeToPerform = intInput > 0 ? intInput * 1000 : long.MaxValue / 1000;

                        break;
                    case 3:
                        Console.WriteLine("Podaj nową wielkość populacji, musi być parzysta");
                        strInput = Console.ReadLine();
                        try
                        {
                            intInput = int.Parse(strInput);
                            if (intInput <= 0)
                                throw new Exception("Wielkość populacji musi być większa od 0");
                            if (intInput % 2 == 1)
                                throw new Exception("Wielkość populacji w założeniu jest parzysta");
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(e.Message);
                            break;
                        }

                        populationSize = intInput;
                        break;
                    case 4:
                        Console.WriteLine("Podaj nowy współczynnik mutacji");
                        strInput = Console.ReadLine();
                        try
                        {
                            doubleInput = double.Parse(strInput);
                            if (doubleInput < 0.0 || doubleInput > 1.0)
                            {
                                throw new Exception("Współczynnik mutacji musi być w przedziale [0, 1]");
                            }
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(e.Message);
                            break;
                        }

                        mutationCoefficient = doubleInput;
                        break;
                    case 5:
                        Console.WriteLine("Podaj nowy współczynnik krzyżowania");
                        strInput = Console.ReadLine();
                        try
                        {
                            doubleInput = double.Parse(strInput);
                            if (doubleInput < 0.0 || doubleInput > 1.0)
                            {
                                throw new Exception("Współczynnik krzyżowania musi być w przedziale [0, 1]");
                            }
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(e.Message);
                            break;
                        }

                        crossoverCoefficient = doubleInput;
                        break;
                    case 6:
                        Console.WriteLine("Wybór mutacji: 1. Swap, 2. Reverse");
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
                                mutationType = Mutation1;
                                mutation = Algorithms.MutationSwap;
                                break;
                            case 2:
                                mutationType = Mutation2;
                                mutation = Algorithms.MutationReverse;
                                break;
                        }

                        break;
                    case 7:
                        Console.WriteLine("Wybór krzyżowania: 1. Partially Matched Crossover, 2. Order Crossover");
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
                                crossoverType = Crossover1;
                                crossover = Algorithms.PartiallyMatchedCrossover;
                                break;
                            case 2:
                                crossoverType = Crossover2;
                                crossover = Algorithms.OrderCrossover;
                                break;
                        }

                        break;
                    case 8:
                        if (graph != null)
                        {
                            // tworzenie wątku odpowiadającego za wyświetlanie pośrednich wyników
                            var timesThread = new Thread(() =>
                            {
                                const int n = 5; // liczba próbek czasu
                                for (int i = 0; i < n; i++)
                                {
                                    Thread.Sleep((int) (timeToPerform / n));
                                    Console.WriteLine(
                                        $"{stopwatch.ElapsedMilliseconds / 1000.0}s - {graph.CalculateRoute(Algorithms.bestSolution)}");
                                }
                            });
                            Console.WriteLine("Czas - koszt uzyskanej trasy");
                            timesThread.Start();
                            stopwatch.Restart();
                            solution = Algorithms.GeneticAlgorithm(
                                graph,
                                populationSize,
                                timeToPerform,
                                crossover,
                                crossoverCoefficient,
                                mutation,
                                mutationCoefficient
                            );
                            timesThread.Join();
                            stopwatch.Stop();
                            Console.WriteLine($"Czas: {stopwatch.ElapsedMilliseconds / 1000.0}s");
                            Console.Write("Trasa: ");
                            WriteList(solution);
                            Console.Write("Koszt: ");
                            Console.WriteLine(graph.CalculateRoute(solution));
                        }

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