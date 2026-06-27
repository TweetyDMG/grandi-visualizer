using System;
using System.Drawing;
using ProjectCourse.Models;

namespace ProjectCourse.Visualization
{
    /// <summary>
    /// Отрисовка ориентированного графа с подписями значений функции Гранди
    /// с использованием GDI+ (System.Drawing).
    /// </summary>
    public static class GraphVisualizer
    {
        private const float ArrowSize = 10f;

        /// <summary>
        /// Рисует граф на заданном Graphics-контексте.
        /// Вершины располагаются по кругу, рёбра — со стрелками.
        /// Каждая вершина подписана: "номер (значение_Гранди)".
        /// </summary>
        public static void DrawGraphWithGrundy(
            Graph graph, int[] grundyValues, Graphics graphics,
            Size panelSize, float scale, Point translation)
        {
            if (graph == null || grundyValues == null ||
                panelSize.Width <= 0 || panelSize.Height <= 0)
                return;

            int[,] adjacencyMatrix = graph.AdjacencyMatrix;
            int vertexCount = adjacencyMatrix.GetLength(0);
            int width = panelSize.Width;
            int height = panelSize.Height;
            int margin = 50;
            int radius = (int)(Math.Min(width, height) / 20 * scale);
            double angleStep = 2 * Math.PI / vertexCount;

            graphics.Clear(Color.White);
            graphics.TranslateTransform(translation.X, translation.Y);
            graphics.ScaleTransform(scale, scale);

            float centerX = width / 2;
            float centerY = height / 2;
            float graphRadius = (Math.Min(width, height) / 2 - margin) * scale;

            PointF[] vertexPositions = new PointF[vertexCount];
            for (int i = 0; i < vertexCount; i++)
            {
                double angle = i * angleStep;
                float x = (float)(centerX + graphRadius * Math.Cos(angle));
                float y = (float)(centerY + graphRadius * Math.Sin(angle));
                vertexPositions[i] = new PointF(x, y);
            }

            using (Pen pen = new Pen(Color.Black, 2))
            {
                for (int i = 0; i < vertexCount; i++)
                {
                    for (int j = 0; j < vertexCount; j++)
                    {
                        if (adjacencyMatrix[i, j] == 1)
                        {
                            PointF start = vertexPositions[i];
                            PointF end = vertexPositions[j];

                            graphics.DrawLine(pen, start, end);
                            DrawArrow(graphics, pen, start, end, radius);
                        }
                    }
                }
            }

            using (Brush vertexBrush = new SolidBrush(Color.Blue))
            using (Font font = new Font("Arial", Math.Max(10, radius / 2)))
            using (Brush textBrush = new SolidBrush(Color.White))
            {
                for (int i = 0; i < vertexCount; i++)
                {
                    PointF position = vertexPositions[i];
                    graphics.FillEllipse(
                        vertexBrush,
                        position.X - radius, position.Y - radius,
                        radius * 2, radius * 2);

                    string label = $"{i} ({grundyValues[i]})";
                    var textSize = graphics.MeasureString(label, font);
                    graphics.DrawString(
                        label, font, textBrush,
                        position.X - textSize.Width / 2,
                        position.Y - textSize.Height / 2);
                }
            }

            graphics.ResetTransform();
        }

        /// <summary>
        /// Рисует стрелку на конце ребра.
        /// </summary>
        private static void DrawArrow(
            Graphics graphics, Pen pen, PointF start, PointF end, float radius)
        {
            float angle = (float)Math.Atan2(end.Y - start.Y, end.X - start.X);

            // Смещаем конец стрелки к границе вершины (с учётом радиуса)
            end = new PointF(
                end.X - radius * (float)Math.Cos(angle),
                end.Y - radius * (float)Math.Sin(angle)
            );

            PointF arrowPoint1 = new PointF(
                end.X - ArrowSize * (float)Math.Cos(angle - Math.PI / 6),
                end.Y - ArrowSize * (float)Math.Sin(angle - Math.PI / 6)
            );

            PointF arrowPoint2 = new PointF(
                end.X - ArrowSize * (float)Math.Cos(angle + Math.PI / 6),
                end.Y - ArrowSize * (float)Math.Sin(angle + Math.PI / 6)
            );

            graphics.DrawLine(pen, end, arrowPoint1);
            graphics.DrawLine(pen, end, arrowPoint2);
        }
    }
}
