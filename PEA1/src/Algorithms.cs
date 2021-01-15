using Medallion.Collections;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PEA
{
    static class Algorithms
    {
        static MatrixGraph graph;
        // do programowania dynamicznego:
        static int[,] solutionsMatrix;
        static int[,] verticesMatrix;
        static List<int> dynamicProgrammingPermutationList;

        //do Branch and Bound
        class BnBVertex : IComparable<BnBVertex>
        {
            public int LowerBound { get; set; }
            // wierchołki, przez które przeszliśmy, włącznie z tym
            public List<int> Vertices { get; }
            public int BitMask { get; }

            public BnBVertex(int newVertex, int bitMask, BnBVertex parent = null)
            {
                Vertices = new List<int>(graph.Size);
                if (parent != null)
                {
                    Vertices.AddRange(parent.Vertices);
                }
                Vertices.Add(newVertex);
                BitMask = bitMask;
                LowerBound = int.MaxValue;
            }

            public int CompareTo(BnBVertex other)
            {
                return LowerBound - other.LowerBound;
            }
        }

        /// <summary>
        /// C#-owa implementacja next_permutation z STL
        /// https://en.cppreference.com/w/cpp/algorithm/next_permutation
        /// https://stackoverflow.com/a/12768718
        /// </summary>
        public static bool NextPermutation<T>(IList<T> list) where T : IComparable
        {
            if (list.Count < 2) return false;
            var k = list.Count - 2;

            while (k >= 0 && list[k].CompareTo(list[k + 1]) >= 0) k--;
            if (k < 0) return false;

            var l = list.Count - 1;
            while (l > k && list[l].CompareTo(list[k]) <= 0) l--;

            var tmp = list[k];
            list[k] = list[l];
            list[l] = tmp;

            var i = k + 1;
            var j = list.Count - 1;
            while (i < j)
            {
                tmp = list[i];
                list[i] = list[j];
                list[j] = tmp;
                i++;
                j--;
            }

            return true;
        }

        /// <summary>
        /// Przegląd zupełny za pomocą NextPermutation
        /// </summary>
        public static IList<int> BruteForce(MatrixGraph graph)
        {
            int[] permutation = new int[graph.Size - 1];
            int[] bestPermutation = new int[graph.Size];
            bestPermutation[0] = 0;
            for (int i = 1; i < graph.Size; i++)
            {
                permutation[i - 1] = i;
            }
            int bestSum = int.MaxValue, newSum;
            do
            {
                newSum = graph.Matrix[0, permutation[0]] + graph.CalculateRoute(permutation);
                if (newSum < bestSum)
                {
                    bestSum = newSum;
                    Array.Copy(permutation, 0, bestPermutation, 1, graph.Size - 1);
                }
            } while (NextPermutation(permutation));
            return bestPermutation;
        }

        public static IList<int> DynamicProgramming(MatrixGraph givenGraph)
        {
            // 1 << n == de facto 2 do potęgi n
            int pow2 = 1 << givenGraph.Size;

            // pow2-1, ponieważ miasto 0 jest zawsze pierwsze, więc nigdy nie będzie w zbiorze pozostałych miast do przejścia
            solutionsMatrix = new int[pow2 - 1, givenGraph.Size];
            verticesMatrix = new int[pow2 - 1, givenGraph.Size];

            // problemy trywialne (trasa wierzchołek 'i' -> wierzchołek 1)
            for (int i = 1; i < givenGraph.Size; i++)
            {
                solutionsMatrix[0, i] = givenGraph.Matrix[i, 0];
            }

            graph = givenGraph;
            int bestSum = DynamicProgrammingGetMinimum(pow2 - 2, 0);
            dynamicProgrammingPermutationList = new List<int>(graph.Size);
            dynamicProgrammingPermutationList.Add(0);
            DynamicProgrammingReturnPath(pow2 - 2, 0);
            return dynamicProgrammingPermutationList;
        }

        static int DynamicProgrammingGetMinimum(int bitMask, int vertex)
        {
            // jeśli rozwiązaliśmy już dany podproblem, to bierzemy wcześniej obliczoną wartość
            if (solutionsMatrix[bitMask, vertex] != 0)
            {
                return solutionsMatrix[bitMask, vertex];
            }
            else
            {
                int min = int.MaxValue;
                int iPow2, logicAnd;
                for (int i = 1; i < graph.Size; i++)
                {
                    // sprawdzenie czy miasto 'i' jest w zbiorze pozostałych miast do przejścia
                    iPow2 = 1 << i;
                    logicAnd = bitMask & iPow2;
                    if (logicAnd != 0)
                    {
                        int newMin = graph.Matrix[vertex, i] + DynamicProgrammingGetMinimum(bitMask - iPow2, i);
                        if (newMin < min)
                        {
                            min = newMin;
                            // zaznaczenie w macierzy pomocniczej, że w optymalnej trasie z wierzchołka vertex idziemy do wierzchołka 'i'
                            verticesMatrix[bitMask, vertex] = i;
                        }
                    }
                }
                // zapisujemy rozwiązanie podproblemu
                solutionsMatrix[bitMask, vertex] = min;
                return min;
            }
        }

        static void DynamicProgrammingReturnPath(int bitMask, int vertex)
        {
            int nextVertex = verticesMatrix[bitMask, vertex];
            dynamicProgrammingPermutationList.Add(nextVertex);
            bitMask -= 1 << nextVertex; // ze zbioru wierzchołków do przejścia usuwamy następny wierzchołek
            if (bitMask != 0)
            {
                DynamicProgrammingReturnPath(bitMask, nextVertex);
            }
        }

        public static IList<int> BranchAndBoundBreadthSearch(MatrixGraph givenGraph)
        {
            graph = givenGraph;
            var queue = new PriorityQueue<BnBVertex>();
            queue.Enqueue(new BnBVertex(0, 0));
            int higherBound = int.MaxValue;
            List<int> bestPermutation = null;
            int iPow2, logicAnd;
            while (queue.Count != 0)
            {
                var vertexLowestBound = queue.Dequeue();
                for (int i = 1; i < graph.Size; i++)
                {
                    iPow2 = 1 << i;
                    logicAnd = iPow2 & vertexLowestBound.BitMask;
                    if (logicAnd == 0)
                    {
                        var childVertex = new BnBVertex(i, vertexLowestBound.BitMask + iPow2, vertexLowestBound);
                        int value = BranchAndBoundValue(childVertex);
                        if (value < higherBound)
                        {
                            higherBound = value;
                            bestPermutation = childVertex.Vertices;
                        }
                        BranchAndBoundLowerBound(childVertex);
                        if (childVertex.LowerBound < higherBound)
                        {
                            queue.Enqueue(childVertex);
                        }
                    }
                }
            }
            return bestPermutation;
        }

        public static IList<int> BranchAndBoundBestFirst(MatrixGraph givenGraph)
        {
            graph = givenGraph;
            var queue = new PriorityQueue<BnBVertex>();
            queue.Enqueue(new BnBVertex(0, 0));
            int higherBound = int.MaxValue;
            List<int> bestPermutation = null;
            int iPow2, logicAnd;
            while (queue.Count != 0)
            {
                var vertexLowestBound = queue.Dequeue();
                if (BranchAndBoundLowerBound(vertexLowestBound) < higherBound)
                {
                    for (int i = 1; i < graph.Size; i++)
                    {
                        iPow2 = 1 << i;
                        logicAnd = iPow2 & vertexLowestBound.BitMask;
                        if (logicAnd == 0)
                        {
                            var childVertex = new BnBVertex(i, vertexLowestBound.BitMask + iPow2, vertexLowestBound);
                            int value = BranchAndBoundValue(childVertex);
                            if (value < higherBound)
                            {
                                higherBound = value;
                                bestPermutation = childVertex.Vertices;
                            }
                            BranchAndBoundLowerBound(childVertex);
                            if (childVertex.LowerBound < higherBound)
                            {
                                queue.Enqueue(childVertex);
                            }
                        }
                    }
                }
            }
            return bestPermutation;
        }

        static int BranchAndBoundValue(BnBVertex vertex)
        {
            if (vertex.Vertices.Count == graph.Size)
            {
                return graph.CalculateRoute(vertex.Vertices);
            }
            else return int.MaxValue;
        }

        static int BranchAndBoundLowerBound(BnBVertex vertex)
        {
            if (vertex.LowerBound != int.MaxValue)
            {
                return vertex.LowerBound;
            }
            else
            {
                int lowerBound = 0;
                int i, j, iPow2, jPow2, logicAnd;
                // obliczanie wartości aktualnie zapisanej trasy
                for (int it = 0; it < vertex.Vertices.Count - 1; it++)
                {
                    i = vertex.Vertices[it];
                    j = vertex.Vertices[it + 1];
                    lowerBound += graph.Matrix[i, j];
                }
                // wyznaczenie minimów wychodzących z wierzchołków, z których jeszcze można wyjść
                for (i = 0; i < graph.Size; i++)
                {
                    int min = int.MaxValue;
                    iPow2 = 1 << i;
                    logicAnd = iPow2 & vertex.BitMask;
                    if (logicAnd == 0 || i == vertex.Vertices.Last())
                    {
                        for (j = 0; j < graph.Size; j++)
                        {
                            jPow2 = 1 << j;
                            logicAnd = jPow2 & vertex.BitMask;
                            if (logicAnd == 0)
                            {
                                int newMin = graph.Matrix[i, j];
                                if (newMin < min)
                                {
                                    min = newMin;
                                }
                            }
                        }
                        lowerBound += min;
                    }
                }
                vertex.LowerBound = lowerBound;
                return lowerBound;
            }
        }
    }
}
