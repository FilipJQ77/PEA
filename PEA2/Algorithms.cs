using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace PEA
{
    static class Algorithms
    {
        /// <summary>
        /// obiekt wykorzystywany do generowania loswych liczb
        /// </summary>
        static Random random = new Random();

        /// <summary>
        /// zamienia 2 elementy kolekcji
        /// </summary>
        /// <param name="arr"></param>
        /// <param name="index1"></param>
        /// <param name="index2"></param>
        public static void Swap(IList<int> arr, int index1, int index2)
        {
            var temp = arr[index1];
            arr[index1] = arr[index2];
            arr[index2] = temp;
        }

        /// <summary>
        /// odwraca kolejność elementów kolekcji od elementu o index1 do elementu o index2
        /// </summary>
        /// <param name="arr"></param>
        /// <param name="index1"></param>
        /// <param name="index2"></param>
        public static void Reverse(IList<int> arr, int index1, int index2)
        {
            while (index1 < index2)
            {
                Swap(arr, index1, index2);
                index1++;
                index2--;
            }
        }

        /// <summary>
        /// Generuje losową permutację tablicy, zostawiając pierwszy element.
        /// https://en.wikipedia.org/wiki/Fisher%E2%80%93Yates_shuffle#The_modern_algorithm
        /// </summary>
        public static void Shuffle(IList<int> arr)
        {
            for (int i = 1; i < arr.Count; i++)
            {
                Swap(arr, i, random.Next(1, arr.Count));
            }
        }

        /// <summary>
        /// https://cs.pwr.edu.pl/zielinski/lectures/om/localsearch.pdf - strona 13
        /// http://www.pi.zarz.agh.edu.pl/intObl/notes/IntObl_w2.pdf - strona 21
        /// </summary>
        /// <param name="graph"></param>
        /// <param name="neighbourhood">typ sąsiedztwa</param>
        /// <param name="time">czas W SEKUNDACH</param>
        /// <param name="notImportant">bool jako parametr tylko po to by parametry zgadzały się z tabu search</param>
        /// <returns></returns>
        public static IList<int> SimulatedAnnealing(MatrixGraph graph, Action<IList<int>, int, int> neighbourhood,
            long time,
            bool notImportant)
        {
            int numberOfCities = graph.Size;
            int bestSolutionValue;
            int currentSolutionValue;
            int[] bestSolution = new int[numberOfCities];
            int[] currentSolution = new int[numberOfCities];
            int[] neighbourSolution = new int[numberOfCities];

            #region generowanie pierwszego, losowego rozwiązania

            for (int i = 0; i < numberOfCities; i++)
            {
                currentSolution[i] = i;
            }

            Shuffle(currentSolution);
            bestSolutionValue = currentSolutionValue = graph.CalculateRoute(currentSolution);
            Array.Copy(currentSolution, bestSolution, numberOfCities);

            #endregion

            #region temperatura początkowa, współczynnik skalowania, końcowa temperatura

            double temperature = bestSolutionValue * numberOfCities;
            const double alpha = 0.99;
            const double endTemperature = 0.000000001;

            #endregion

            var stopwatch = new Stopwatch();
            time *= 1000; //zamiana sekund na milisekundy
            stopwatch.Start();
            while (stopwatch.ElapsedMilliseconds <= time && temperature > endTemperature)
            {
                bool foundBetterSolution;
                do
                {
                    foundBetterSolution = false;

                    for (int i = 0; i < numberOfCities * 5; i++)
                    {
                        #region generowanie losowej permutacji z sąsiedztwa obecnego rozwiązania

                        Array.Copy(currentSolution, neighbourSolution, numberOfCities);
                        neighbourhood(neighbourSolution, random.Next(1, numberOfCities),
                            random.Next(1, numberOfCities));
                        int neighbourSolutionValue = graph.CalculateRoute(neighbourSolution);

                        #endregion

                        int delta = neighbourSolutionValue - currentSolutionValue;

                        #region jeśli nowa permutacja jest najlepsza, ustaw ją jako obecną i najlepszą permutację

                        if (neighbourSolutionValue < bestSolutionValue)
                        {
                            foundBetterSolution = true;
                            Array.Copy(neighbourSolution, currentSolution, numberOfCities);
                            currentSolutionValue = neighbourSolutionValue;

                            Array.Copy(neighbourSolution, bestSolution, numberOfCities);
                            bestSolutionValue = neighbourSolutionValue;
                        }

                        #endregion

                        #region w przeciwnym razie, jeśli nowa permutacja jest lepsza niż obecna, ustaw ją jako obecną

                        else if (delta < 0)
                        {
                            Array.Copy(neighbourSolution, currentSolution, numberOfCities);
                            currentSolutionValue = neighbourSolutionValue;
                        }

                        #endregion

                        #region w przeciwnym razie, jeśli nowa permutacja jest gorsza od obecnej, ustaw ją jako obecną z prawdopodobieństwem exp(-delta/T)

                        else if (random.NextDouble() < Math.Exp(-delta / temperature))
                        {
                            Array.Copy(neighbourSolution, currentSolution, numberOfCities);
                            currentSolutionValue = neighbourSolutionValue;
                        }

                        #endregion
                    }
                } while (foundBetterSolution);

                temperature *= alpha;
            }

            return bestSolution;
        }

        public static void TabuSearchEmptyTabuList(int[,] list)
        {
            int size = list.GetLength(0);
            for (int i = 1; i < size; i++)
            {
                for (int j = i + 1; j < size; j++)
                {
                    list[i, j] = 0;
                }
            }
        }

        public static void TabuSearchDecrementTabuList(int[,] list)
        {
            int size = list.GetLength(0);
            for (int i = 1; i < size; i++)
            {
                for (int j = i + 1; j < size; j++)
                {
                    if (list[i, j] > 0)
                        list[i, j]--;
                }
            }
        }

        /// <summary>
        /// http://www.zio.iiar.pwr.wroc.pl/pea/w5_ts.pdf - strona 20
        /// http://www.pi.zarz.agh.edu.pl/intObl/notes/IntObl_w2.pdf - strona 38
        /// </summary>
        /// <param name="graph"></param>
        /// <param name="neighbourhood">typ sąsiedztwa</param>
        /// <param name="time">czas W SEKUNDACH</param>
        /// <param name="diversification"></param>
        /// <returns></returns>
        public static IList<int> TabuSearch(MatrixGraph graph, Action<IList<int>, int, int> neighbourhood, long time,
            bool diversification)
        {
            int numberOfCities = graph.Size;
            int bestSolutionValue;
            int currentSolutionValue;
            int[] bestSolution = new int[numberOfCities];
            int[] currentSolution = new int[numberOfCities];
            int[] neighbourSolution = new int[numberOfCities];

            // int lifetime = numberOfCities;
            int lifetime = 10;
            int criticalEvents = 0;

            // w C# tablice domyślnie wypełnione są zerami
            int[,] tabuList = new int[numberOfCities, numberOfCities];

            #region generowanie pierwszego, losowego rozwiązania

            for (int i = 0; i < numberOfCities; i++)
            {
                currentSolution[i] = i;
            }

            Shuffle(currentSolution);
            bestSolutionValue = currentSolutionValue = graph.CalculateRoute(currentSolution);
            Array.Copy(currentSolution, bestSolution, numberOfCities);

            #endregion

            var stopwatch = new Stopwatch();
            stopwatch.Start();
            time *= 1000; // zamiana sekund na milisekundy
            while (stopwatch.ElapsedMilliseconds <= time)
            {
                int previousCurrentSolutionValue = currentSolutionValue;

                #region znalezienie najlepszego rozwiązania w sąsiedztwie

                int bestI = 0, bestJ = 0;
                for (int i = 1; i < numberOfCities; i++)
                {
                    for (int j = i + 1; j < numberOfCities; j++)
                    {
                        Array.Copy(currentSolution, neighbourSolution, numberOfCities);

                        neighbourhood(neighbourSolution, i, j);
                        int neighbourSolutionValue = graph.CalculateRoute(neighbourSolution);
                        // jeżeli rozpatrywane rozwiązanie jest lepsze od najlepszego dotychczas znalezionego, to przyjmujemy je jako obecne nawet jeżeli jest zakazane
                        // w przeciwnym wypadku przyjmujemy tylko jeśli jest lepsze od obecnego rozwiązania i nie ma go na liście tabu
                        if (neighbourSolutionValue < bestSolutionValue ||
                            (neighbourSolutionValue < currentSolutionValue && tabuList[i, j] == 0))
                        {
                            currentSolutionValue = neighbourSolutionValue;
                            bestI = i;
                            bestJ = j;
                        }
                    }
                }

                neighbourhood(currentSolution, bestI, bestJ);
                tabuList[bestI, bestJ] = lifetime;

                #endregion

                TabuSearchDecrementTabuList(tabuList);

                if (currentSolutionValue < bestSolutionValue)
                {
                    Array.Copy(currentSolution, bestSolution, numberOfCities);
                    bestSolutionValue = currentSolutionValue;
                    criticalEvents = 0;
                }
                else if (diversification && currentSolutionValue >= previousCurrentSolutionValue)
                {
                    criticalEvents++;
                    // zbyt dużo krytycznych momentów, dywersyfikacja (jeśli włączona)
                    if (criticalEvents >= 20/*10*//*lifetime*/)
                    {
                        Shuffle(currentSolution);
                        currentSolutionValue = graph.CalculateRoute(currentSolution);
                        TabuSearchEmptyTabuList(tabuList);
                    }
                }
            }

            return bestSolution;
        }
    }
}