using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace PEA
{
    static class Algorithms
    {
        /// <summary>
        /// obiekt wykorzystywany do generowania loswych liczb
        /// </summary>
        static Random random = new Random();

        /// <summary>
        /// najlepsze znalezione rozwiązanie
        /// </summary>
        public static IList<int> bestSolution;

        /// <summary>
        /// wielkość problemu
        /// </summary>
        static int numberOfCities;

        /// <summary>
        /// rozmiar populacji
        /// </summary>
        static int populationSize;

        /// <summary>
        /// populacja
        /// </summary>
        static List<IList<int>> population;

        /// <summary>
        /// wartości rozwiązań dla każdego elementu populacji
        /// </summary>
        static int[] populationSolutionValues;

        /// <summary>
        /// funkcja krzyżowania
        /// </summary>
        static Func<IList<int>, IList<int>, IList<int>> crossover; // rodzic1, rodzic2, zwraca dziecko

        /// <summary>
        /// współczynnik krzyżowania
        /// </summary>
        static double crossoverCoefficient;

        /// <summary>
        /// funkcja mutacji
        /// </summary>
        static Func<IList<int>, IList<int>> mutation; //dziecko, zwraca zmutowane dziecko

        /// <summary>
        /// współczynnik mutacji
        /// </summary>
        static double mutationCoefficient;

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
        /// zamienia kolejność elementów listy w przedziale [begin, end)
        /// https://en.wikipedia.org/wiki/Fisher%E2%80%93Yates_shuffle#The_modern_algorithm
        /// </summary>
        public static void Shuffle(IList<int> arr, int begin, int end)
        {
            for (int i = begin; i < end; i++)
            {
                Swap(arr, i, random.Next(begin, end));
            }
        }

        public static IList<int> GenerateRandomPermutation()
        {
            // tworzy tablicę 0,1,2,...,size-1
            var permutation = Enumerable.Range(0, numberOfCities).ToArray();
            Shuffle(permutation, 1, permutation.Length);
            return permutation;
        }

        /// <summary>
        /// http://aragorn.pb.bialystok.pl/~wkwedlo/EA5.pdf - strona 15
        /// http://user.ceng.metu.edu.tr/~ucoluk/research/publications/tsp.pdf - strona 2
        /// https://stackoverflow.com/a/11584750 - pomysł implementacji
        /// </summary>
        /// <param name="parent1"></param>
        /// <param name="parent2"></param>
        /// <returns></returns>
        public static IList<int> PartiallyMatchedCrossover(IList<int> parent1, IList<int> parent2)
        {
            int length = random.Next(2, parent1.Count - 1);
            int startPoint = random.Next(1, parent1.Count - length);

            var child = new int[numberOfCities];
            var map = new int[numberOfCities];
            parent1.CopyTo(child, 0);

            for (int i = 0; i < numberOfCities; i++)
            {
                map[child[i]] = i;
            }

            for (int i = startPoint; i < startPoint + length; i++)
            {
                int city = parent2[i];
                Swap(child, i, map[city]);
                Swap(map, child[i], child[map[city]]);
            }

            return child;
        }

        /// <summary>
        /// http://aragorn.pb.bialystok.pl/~wkwedlo/EA5.pdf - strona 17
        /// </summary>
        /// <param name="parent1"></param>
        /// <param name="parent2"></param>
        /// <returns></returns>
        public static IList<int> OrderCrossover(IList<int> parent1, IList<int> parent2)
        {
            // int length = random.Next(2, parent1.Count - 1);
            // int startPoint = random.Next(1, parent1.Count - length);
            
            int length = 4;
            int startPoint = 4;


            var child = new int[numberOfCities];
            var used = new bool[numberOfCities]; // domyślnie wszystkie elementy to false
            int i;
            for (i = startPoint; i < startPoint + length; i++)
            {
                child[i] = parent1[i];
                used[parent1[i]] = true;
            }

            int j = startPoint + length;
            while (i != startPoint)
            {
                if (!used[parent2[j]])
                {
                    child[i] = parent2[j];
                    i++;
                }

                j++;

                if (i >= numberOfCities)
                    i = 1; // z końca na początek

                if (j >= numberOfCities)
                    j = 1; // z końca na początek
            }

            return child;
        }

        public static IList<int> MutationSwap(IList<int> permutation)
        {
            int index1 = random.Next(1, permutation.Count);
            int index2;
            do
            {
                index2 = random.Next(1, permutation.Count);
            } while (index1 == index2);

            Swap(permutation, index1, index2);
            return permutation;
        }

        public static IList<int> MutationReverse(IList<int> permutation)
        {
            int length = random.Next(2, permutation.Count - 1);
            int indexStart = random.Next(1, permutation.Count - length);
            Reverse(permutation, indexStart, indexStart + length);
            return permutation;
        }

        /// <summary>
        /// selekcja z poprzedniej populacji
        /// losujemy 1/20 elementów z populacji
        /// wybieramy z nich najlepszego, aby został rodzicem
        /// powtarzamy aż uzyskamy tyle kandydatów na rodziców, ile wynosi wielkość populacji
        /// </summary>
        /// <returns></returns>
        public static List<int> Selection()
        {
            var parents = new List<int>(numberOfCities);
            for (int i = 0; i < populationSize; i++)
            {
                int opponent1Index = random.Next(populationSize);
                int opponent2Index;
                do
                {
                    opponent2Index = random.Next(populationSize);
                } while (opponent1Index == opponent2Index);

                int minIndex =
                    populationSolutionValues[opponent1Index] < populationSolutionValues[opponent2Index]
                        ? opponent1Index
                        : opponent2Index;
                
                // dodanie indeksu zwycięzcy do listy przyszłych rodziców
                parents.Add(minIndex);
            }

            return parents;
        }

        public static List<IList<int>> CreateNewGeneration(List<int> parents)
        {
            var newPopulation = new List<IList<int>>(populationSize);
            for (int i = 0; i < populationSize; i += 2)
            {
                var parent1 = population[parents[i]];
                var parent2 = population[parents[i + 1]];
                IList<int> child1;
                IList<int> child2;
                double crossoverChance = random.NextDouble();
                // jeśli nastąpiło krzyżowanie, do nowej populacji dodajemy skrzyżowane osobniki
                if (crossoverChance < crossoverCoefficient)
                {
                    child1 = crossover(parent1, parent2);
                    child2 = crossover(parent2, parent1);
                }
                // w przeciwnym wypadku dodajemy niezmienionych rodziców
                else
                {
                    child1 = parent1;
                    child2 = parent2;
                }

                double mutationChance = random.NextDouble();
                if (mutationChance < mutationCoefficient)
                    child1 = mutation(child1);

                mutationChance = random.NextDouble();
                if (mutationChance < mutationCoefficient)
                    child2 = mutation(child2);

                newPopulation.Add(child1);
                newPopulation.Add(child2);
            }

            return newPopulation;
        }

        public static IList<int> GeneticAlgorithm(
            MatrixGraph graph,
            int populationSize,
            long timeToPerform,
            Func<IList<int>, IList<int>, IList<int>> crossover, // rodzic1, rodzic2, zwraca dziecko
            double crossoverCoefficient,
            Func<IList<int>, IList<int>> mutation, //dziecko, zwraca zmutowane dziecko
            double mutationCoefficient
        )
        {
            numberOfCities = graph.Size;
            Algorithms.populationSize = populationSize;
            Algorithms.crossover = crossover;
            Algorithms.crossoverCoefficient = crossoverCoefficient;
            Algorithms.mutation = mutation;
            Algorithms.mutationCoefficient = mutationCoefficient;
            int bestSolutionValue = int.MaxValue;
            var stopwatch = new Stopwatch();
            stopwatch.Start();

            #region stworzenie populacji startowej

            population = new List<IList<int>>(populationSize);
            populationSolutionValues = new int[populationSize];
            for (int i = 0; i < populationSize; i++)
            {
                population.Add(GenerateRandomPermutation());
                populationSolutionValues[i] = graph.CalculateRoute(population[i]);
            }

            #endregion

            bestSolution = population[0]; // defaultowe przypisanie

            while (stopwatch.ElapsedMilliseconds <= timeToPerform)
            {
                // selekcja rodziców (indeksy rodziców)
                List<int> parents = Selection();
                // tworzenie dzieci - krzyżowanie, mutacja
                List<IList<int>> newPopulation = CreateNewGeneration(parents);
                population = newPopulation; // nowa populacja
                for (int i = 0; i < populationSize; i++)
                {
                    populationSolutionValues[i] = graph.CalculateRoute(population[i]);
                    if (populationSolutionValues[i] < bestSolutionValue)
                    {
                        bestSolution = population[i];
                        bestSolutionValue = populationSolutionValues[i];
                    }
                }
            }

            return bestSolution;
        }
    }
}