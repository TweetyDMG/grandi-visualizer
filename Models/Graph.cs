using System.Collections.Generic;

namespace ProjectCourse.Models
{
    /// <summary>
    /// Модель ориентированного графа, заданного матрицей смежности.
    /// </summary>
    public class Graph
    {
        /// <summary>
        /// Матрица смежности: AdjacencyMatrix[i, j] == 1 означает ребро i → j.
        /// </summary>
        public int[,] AdjacencyMatrix { get; private set; }

        /// <summary>
        /// Количество вершин в графе.
        /// </summary>
        public int VertexCount => AdjacencyMatrix.GetLength(0);

        public Graph(int[,] adjacencyMatrix)
        {
            AdjacencyMatrix = adjacencyMatrix;
        }

        /// <summary>
        /// Возвращает перечисление соседей вершины (исходящие рёбра).
        /// </summary>
        public IEnumerable<int> GetNeighbors(int vertex)
        {
            for (int i = 0; i < VertexCount; i++)
            {
                if (AdjacencyMatrix[vertex, i] == 1)
                    yield return i;
            }
        }
    }
}
