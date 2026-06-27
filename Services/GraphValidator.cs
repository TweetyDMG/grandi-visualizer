using System.Collections.Generic;
using System.Windows.Forms;
using ProjectCourse.Models;

namespace ProjectCourse.Services
{
    /// <summary>
    /// Валидация ориентированного графа: проверка на петли и циклы.
    /// </summary>
    public static class GraphValidator
    {
        /// <summary>
        /// Проверяет корректность графа: не null, без петель и без циклов.
        /// </summary>
        public static bool IsValidGraph(Graph graph)
        {
            if (graph == null || graph.AdjacencyMatrix == null)
            {
                MessageBox.Show("Граф пустой или некорректный.", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            if (!HasNoSelfLoops(graph))
            {
                MessageBox.Show("Граф содержит петли.", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            if (!IsAcyclic(graph))
            {
                MessageBox.Show("Граф содержит циклы.", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            return true;
        }

        /// <summary>
        /// Проверяет отсутствие петель (диагональные элементы равны 0).
        /// </summary>
        private static bool HasNoSelfLoops(Graph graph)
        {
            int vertexCount = graph.VertexCount;

            for (int i = 0; i < vertexCount; i++)
            {
                if (graph.AdjacencyMatrix[i, i] != 0)
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Проверяет ацикличность графа через топологическую сортировку (алгоритм Кана).
        /// Если удалось посетить все вершины — циклов нет.
        /// </summary>
        private static bool IsAcyclic(Graph graph)
        {
            int vertexCount = graph.VertexCount;
            int[] inDegree = new int[vertexCount];
            Queue<int> queue = new Queue<int>();

            for (int i = 0; i < vertexCount; i++)
            {
                for (int j = 0; j < vertexCount; j++)
                {
                    if (graph.AdjacencyMatrix[j, i] == 1)
                    {
                        inDegree[i]++;
                    }
                }
            }

            for (int i = 0; i < vertexCount; i++)
            {
                if (inDegree[i] == 0)
                {
                    queue.Enqueue(i);
                }
            }

            int visitedCount = 0;
            while (queue.Count > 0)
            {
                int current = queue.Dequeue();
                visitedCount++;

                for (int i = 0; i < vertexCount; i++)
                {
                    if (graph.AdjacencyMatrix[current, i] == 1)
                    {
                        inDegree[i]--;
                        if (inDegree[i] == 0)
                        {
                            queue.Enqueue(i);
                        }
                    }
                }
            }

            return visitedCount == vertexCount;
        }
    }
}
