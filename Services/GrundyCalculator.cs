using System.Collections.Generic;
using ProjectCourse.Models;

namespace ProjectCourse.Services
{
    /// <summary>
    /// Вычисление функции Гранди (Grundy numbers) для вершин DAG.
    /// Алгоритм: топологическая сортировка → обход в обратном порядке → mex.
    /// </summary>
    public static class GrundyCalculator
    {
        /// <summary>
        /// Вычисляет значения функции Гранди для всех вершин графа.
        /// </summary>
        /// <returns>Массив grundyValues[i] — значение функции Гранди для вершины i.</returns>
        public static int[] CalculateGrundyValues(Graph graph)
        {
            int vertexCount = graph.VertexCount;
            int[] grundyValues = new int[vertexCount];

            List<int> topologicalOrder = GetTopologicalOrder(graph);

            foreach (int vertex in topologicalOrder)
            {
                grundyValues[vertex] = CalculateGrundyForVertex(graph, vertex, grundyValues);
            }

            return grundyValues;
        }

        /// <summary>
        /// Вычисляет значение функции Гранди для одной вершины
        /// как mex (минимальное неотрицательное целое, не встречающееся среди соседей).
        /// </summary>
        private static int CalculateGrundyForVertex(Graph graph, int vertex, int[] grundyValues)
        {
            HashSet<int> mexSet = new HashSet<int>();

            foreach (int neighbor in graph.GetNeighbors(vertex))
            {
                mexSet.Add(grundyValues[neighbor]);
            }

            int mex = 0;
            while (mexSet.Contains(mex))
            {
                mex++;
            }

            return mex;
        }

        /// <summary>
        /// Топологическая сортировка графа (алгоритм Кана).
        /// </summary>
        /// <returns>Список вершин в топологическом порядке.</returns>
        private static List<int> GetTopologicalOrder(Graph graph)
        {
            int vertexCount = graph.VertexCount;
            int[] inDegree = new int[vertexCount];
            Queue<int> queue = new Queue<int>();
            List<int> order = new List<int>();

            for (int i = 0; i < vertexCount; i++)
            {
                foreach (int neighbor in graph.GetNeighbors(i))
                {
                    inDegree[neighbor]++;
                }
            }

            for (int i = 0; i < vertexCount; i++)
            {
                if (inDegree[i] == 0)
                {
                    queue.Enqueue(i);
                }
            }

            while (queue.Count > 0)
            {
                int current = queue.Dequeue();
                order.Add(current);

                foreach (int neighbor in graph.GetNeighbors(current))
                {
                    inDegree[neighbor]--;
                    if (inDegree[neighbor] == 0)
                    {
                        queue.Enqueue(neighbor);
                    }
                }
            }

            return order;
        }
    }
}
