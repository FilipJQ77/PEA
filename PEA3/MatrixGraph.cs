using System;
using System.Collections.Generic;

namespace PEA
{
    class MatrixGraph
    {
        public int[,] Matrix { get; }

        public int Size { get; }

        public MatrixGraph(int size)
        {
            Matrix = new int[size, size];
            Size = size;
        }

        public int CalculateRoute(IList<int> permutation)
        {
            int sum = 0;
            int i, j;
            for (int it = 0; it < permutation.Count - 1; it++)
            {
                i = permutation[it];
                j = permutation[it + 1];
                sum += Matrix[i, j];
            }
            sum += Matrix[permutation[permutation.Count - 1], 0];
            return sum;
        }

        public void SetMatrixRow(int row, IList<int> arr)
        {
            for (int i = 0; i < Size; i++)
            {
                Matrix[row, i] = arr[i];
            }
        }

        public static MatrixGraph GenerateRandomGraph(int size)
        {
            var graph = new MatrixGraph(size);
            var rand = new Random();
            for (int i = 0; i < size; i++)
            {
                for (int j = 0; j < size; j++)
                {
                    if (i != j)
                        graph.Matrix[i, j] = rand.Next(1, 100);
                    else
                        graph.Matrix[i, j] = int.MaxValue;
                }
            }
            return graph;
        }

        public void WriteToFile()
        {
            using (var file = new System.IO.StreamWriter("graph.txt", false))
            {
                file.WriteLine(Size);
                for (int i = 0; i < Size; i++)
                {
                    for (int j = 0; j < Size; j++)
                    {
                        file.Write($"{Matrix[i, j]} ");
                    }
                    file.WriteLine();
                }
            }
        }
    }
}
