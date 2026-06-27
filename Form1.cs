using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using OfficeOpenXml;
using ProjectCourse.Models;
using ProjectCourse.Services;
using ProjectCourse.Visualization;

namespace ProjectCourse
{
    public partial class MainForm : Form
    {
        private Graph graph;
        private int[] grundyValues;
        private bool isGraphDrawn = false;
        private string currentFilePath = null;
        private bool isGraphSaved = true;
        private float scale = 1.0f; //текущий масштаб
        private Point translation = new Point(0, 0); //текущее смещение
        private Point lastMousePosition; //последняя позиция мыши
        private bool isDragging = false; //флаг для перемещения
        private const float MinScale = 0.5f; 
        private const float MaxScale = 3.0f;
        private Rectangle movementBounds;


        public MainForm()
        {
            InitializeComponent();
            this.Resize += MainForm_Resize;
            InitializeGraph();
            pictureBoxGraph.Paint += pictureBoxGraph_Paint;
            this.pictureBoxGraph.MouseWheel += pictureBoxGraph_MouseWheel;
            this.pictureBoxGraph.MouseDown += pictureBoxGraph_MouseDown;
            this.pictureBoxGraph.MouseMove += pictureBoxGraph_MouseMove;
            this.pictureBoxGraph.MouseUp += pictureBoxGraph_MouseUp;
            movementBounds = new Rectangle(
                -pictureBoxGraph.Width, 
                -pictureBoxGraph.Height,
                2 * pictureBoxGraph.Width,
                2 * pictureBoxGraph.Height 
            );
        }

        private void InitializeGraph()
        {
            int vertexCount = (int)numVertices.Value;
            int[,] matrix = new int[vertexCount, vertexCount];
            graph = new Graph(matrix);
            RedrawGraph();
        }

        private void InitializeDataGridView(int vertexCount)
        {
            dataGridViewMatrix.ColumnCount = vertexCount;
            dataGridViewMatrix.RowCount = vertexCount;

            for (int i = 0; i < vertexCount; i++)
            {

                dataGridViewMatrix.Columns[i].Name = $"V{i}";
                dataGridViewMatrix.Rows[i].HeaderCell.Value = $"V{i}";
                dataGridViewMatrix.Columns[i].Width = 40;
            }

            dataGridViewMatrix.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells;
        }


        private void buttonGraph_Click(object sender, EventArgs e)
        {
            if (TryGetAdjacencyMatrix(out int[,] adjacencyMatrix))
            {
                if (graph != null && GraphValidator.IsValidGraph(graph)) 
                {
                    MessageBox.Show("Граф корректен!", "Проверка завершена", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    MessageBox.Show("Граф не инициализирован или некорректен!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
        }

        private void buttonBuild_Click(object sender, EventArgs e)
        {
            if (TryGetAdjacencyMatrix(out int[,] matrix))
            {
                graph = new Graph(matrix);
                if (!GraphValidator.IsValidGraph(graph)) return;

                grundyValues = GrundyCalculator.CalculateGrundyValues(graph);
                pictureBoxGraph.Invalidate();

                isGraphSaved = false;
            }
        }

        private bool TryGetAdjacencyMatrix(out int[,] adjacencyMatrix)
        {
            int vertexCount = dataGridViewMatrix.RowCount;
            adjacencyMatrix = new int[vertexCount, vertexCount];

            try
            {
                for (int i = 0; i < vertexCount; i++)
                {
                    for (int j = 0; j < vertexCount; j++)
                    {
                        int value = 0;
                        if (int.TryParse(dataGridViewMatrix.Rows[i].Cells[j].Value?.ToString(), out value) && (value == 0 || value == 1))
                        {
                            adjacencyMatrix[i, j] = value;
                        }
                        else
                        {
                            MessageBox.Show($"Некорректное значение в ячейке ({i}, {j}): {dataGridViewMatrix.Rows[i].Cells[j].Value}");
                            adjacencyMatrix = null;
                            return false;
                        }
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при обработке матрицы: {ex.Message}");
                adjacencyMatrix = null;
                return false;
            }
        }

        private void numVertices_ValueChanged_1(object sender, EventArgs e)
        {
            int vertexCount = (int)numVertices.Value;
            InitializeDataGridView(vertexCount);
        }

        private void MainForm_Resize(object sender, EventArgs e)
        {
            isGraphDrawn = false;
            RedrawGraph();
        }

        private void RedrawGraph()
        {
            if (graph != null && grundyValues != null)
            {
                using (Graphics graphics = pictureBoxGraph.CreateGraphics())
                {
                    graphics.Clear(Color.White);
                    GraphVisualizer.DrawGraphWithGrundy(graph, grundyValues, graphics, pictureBoxGraph.Size, scale, translation);
                }
            }
        }

        private void pictureBoxGraph_Paint(object sender, PaintEventArgs e)
        {
            e.Graphics.Clear(Color.White);

            if (graph != null && grundyValues != null)
            {
                GraphVisualizer.DrawGraphWithGrundy(graph, grundyValues, e.Graphics, pictureBoxGraph.Size, scale, translation);
            }
        }


        private void NewGraphToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!ConfirmSaveIfNeeded()) return;

            isGraphDrawn = false;
            graph = null;
            grundyValues = null;

            foreach (DataGridViewRow row in dataGridViewMatrix.Rows)
            {
                foreach (DataGridViewCell cell in row.Cells)
                {
                    cell.Value = null;
                }
            }

            pictureBoxGraph.Invalidate();
            isGraphSaved = false;
        }


        private void OpenToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = "Текстовые файлы (*.txt)|*.txt|Excel файлы (*.xlsx)|*.xlsx|Все файлы (*.*)|*.*";
                openFileDialog.Title = "Открыть файл с матрицей смежности";

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    string filePath = openFileDialog.FileName;
                    try
                    {
                        if (filePath.EndsWith(".xlsx", StringComparison.OrdinalIgnoreCase))
                        {
                            LoadMatrixFromExcel(filePath);
                        }
                        else
                        {
                            LoadMatrixFromTextFile(filePath);
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Ошибка при открытии файла: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void LoadMatrixFromExcel(string filePath)
        {
            try
            {
                if (!File.Exists(filePath))
                {
                    MessageBox.Show("Файл не существует.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                FileInfo fileInfo = new FileInfo(filePath);
                using (var package = new ExcelPackage(fileInfo))
                {
                    var worksheet = package.Workbook.Worksheets[0];

                    int rowCount = worksheet.Dimension.Rows;
                    int colCount = worksheet.Dimension.Columns;

                    int[,] matrix = new int[rowCount, colCount];

                    for (int row = 1; row <= rowCount; row++) //EPPlus использует 1-индексацию
                    {
                        for (int col = 1; col <= colCount; col++)
                        {
                            var cellValue = worksheet.Cells[row, col].Text; 
                            if (int.TryParse(cellValue, out int value))
                            {
                                matrix[row - 1, col - 1] = value; 
                            }
                            else
                            {
                                MessageBox.Show($"Некорректные данные в ячейке ({row}, {col})", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                return;
                            }
                        }
                    }

                    UpdateMatrixUI(matrix);
                }
            }
            catch (UnauthorizedAccessException)
            {
                MessageBox.Show("Нет прав для чтения файла.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (IOException ex)
            {
                MessageBox.Show($"Ошибка при чтении файла: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Произошла неожиданная ошибка: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }



        private void LoadMatrixFromTextFile(string filePath)
        {
            try
            {
                if (!File.Exists(filePath))
                {
                    MessageBox.Show("Файл не существует.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                string[] lines = File.ReadAllLines(filePath);

                if (lines.Length == 0)
                {
                    MessageBox.Show("Файл пуст.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                List<int[]> adjacencyMatrix = new List<int[]>();

                foreach (string line in lines)
                {
                    if (string.IsNullOrWhiteSpace(line))
                        continue;

                    string[] tokens = line.Split(new char[] { ' ', ',', '\t' }, StringSplitOptions.RemoveEmptyEntries);

                    if (tokens.Any(token => !int.TryParse(token, out _)))
                    {
                        MessageBox.Show($"Некорректный формат данных в строке: {line}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }

                    int[] row = Array.ConvertAll(tokens, int.Parse);
                    adjacencyMatrix.Add(row);
                }

                int rowCount = adjacencyMatrix.Count;
                int columnCount = adjacencyMatrix.FirstOrDefault()?.Length ?? 0;
                if (adjacencyMatrix.Any(row => row.Length != columnCount))
                {
                    MessageBox.Show("Матрица смежности имеет некорректный формат (неконсистентные размеры строк).", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                int[,] matrix = new int[rowCount, columnCount];
                for (int i = 0; i < rowCount; i++)
                {
                    for (int j = 0; j < columnCount; j++)
                    {
                        matrix[i, j] = adjacencyMatrix[i][j];
                    }
                }

                UpdateMatrixUI(matrix);
            }
            catch (UnauthorizedAccessException)
            {
                MessageBox.Show("Нет прав для чтения файла.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (IOException ex)
            {
                MessageBox.Show($"Ошибка при чтении файла: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Произошла неожиданная ошибка: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void UpdateMatrixUI(int[,] matrix)
        {
            dataGridViewMatrix.Rows.Clear();
            dataGridViewMatrix.Columns.Clear();

            for (int i = 0; i < matrix.GetLength(1); i++)
            {
                dataGridViewMatrix.Columns.Add($"V{i}", $"V{i}");
            }

            for (int i = 0; i < matrix.GetLength(0); i++)
            {
                dataGridViewMatrix.Rows.Add();

                dataGridViewMatrix.Rows[i].HeaderCell.Value = $"V{i}";

                for (int j = 0; j < matrix.GetLength(1); j++)
                {
                    dataGridViewMatrix.Rows[i].Cells[j].Value = matrix[i, j];
                }
            }

            dataGridViewMatrix.RowHeadersWidth = 50;
        }

        private void SaveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (currentFilePath != null)
            {
                try
                {
                    if (currentFilePath.EndsWith(".txt"))
                    {
                        SaveMatrixToTextFile(currentFilePath);
                    }
                    else if (currentFilePath.EndsWith(".xlsx"))
                    {
                        SaveMatrixToExcelFile(currentFilePath);
                    }

                    isGraphSaved = true;
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при сохранении файла: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else
            {
                SaveAsToolStripMenuItem_Click(sender, e);
            }
        }

        private void SaveAsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (SaveFileDialog saveFileDialog = new SaveFileDialog())
            {
                saveFileDialog.Filter = "Текстовые файлы (*.txt)|*.txt|Excel файлы (*.xlsx)|*.xlsx";
                saveFileDialog.Title = "Сохранить как";

                if (saveFileDialog.ShowDialog() == DialogResult.OK)
                {
                    string filePath = saveFileDialog.FileName;

                    try
                    {
                        if (filePath.EndsWith(".txt"))
                        {
                            SaveMatrixToTextFile(filePath);
                        }
                        else if (filePath.EndsWith(".xlsx"))
                        {
                            SaveMatrixToExcelFile(filePath);
                        }

                        currentFilePath = filePath;
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Ошибка при сохранении файла: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }
        private void SaveMatrixToTextFile(string filePath)
        {
            try
            {
                StringBuilder sb = new StringBuilder();

                for (int i = 0; i < dataGridViewMatrix.RowCount; i++)
                {
                    StringBuilder row = new StringBuilder();

                    for (int j = 0; j < dataGridViewMatrix.ColumnCount; j++)
                    {
                        row.Append(dataGridViewMatrix.Rows[i].Cells[j].Value);
                        if (j < dataGridViewMatrix.ColumnCount - 1)
                        {
                            row.Append(" "); 
                        }
                    }

                    sb.AppendLine(row.ToString());
                }

                File.WriteAllText(filePath, sb.ToString());
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при сохранении в файл: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void SaveMatrixToExcelFile(string filePath)
        {
            try
            {
                using (ExcelPackage package = new ExcelPackage())
                {
                    ExcelWorksheet worksheet = package.Workbook.Worksheets.Add("Матрица");

                    for (int i = 0; i < dataGridViewMatrix.RowCount; i++)
                    {
                        for (int j = 0; j < dataGridViewMatrix.ColumnCount; j++)
                        {
                            worksheet.Cells[i + 1, j + 1].Value = dataGridViewMatrix.Rows[i].Cells[j].Value;
                        }
                    }

                    FileInfo fi = new FileInfo(filePath);
                    package.SaveAs(fi);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при сохранении в Excel: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ExitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (ConfirmSaveIfNeeded())
            {
                Application.Exit();
            }
        }

        private bool ConfirmSaveIfNeeded()
        {
            if (!isGraphSaved)
            {
                var result = MessageBox.Show(
                    "Вы хотите сохранить изменения перед выходом?",
                    "Несохранённые данные",
                    MessageBoxButtons.YesNoCancel,
                    MessageBoxIcon.Warning
                );

                if (result == DialogResult.Yes)
                {
                    SaveToolStripMenuItem_Click(this, EventArgs.Empty); 
                    return true;
                }
                else if (result == DialogResult.No)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }

            return true;
        }

        private void pictureBoxGraph_MouseWheel(object sender, MouseEventArgs e)
        {
            const float scaleStep = 0.1f; 
            float oldScale = scale;

            if (e.Delta > 0)
            {
                scale = Math.Min(scale + scaleStep, MaxScale);
            }
            else if (e.Delta < 0) 
            {
                scale = Math.Max(scale - scaleStep, MinScale); 
            }

            float scaleChange = scale / oldScale;
            translation.X = (int)(e.X - scaleChange * (e.X - translation.X));
            translation.Y = (int)(e.Y - scaleChange * (e.Y - translation.Y));

            pictureBoxGraph.Invalidate();
        }


        private void pictureBoxGraph_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                isDragging = true;
                lastMousePosition = e.Location;
            }
        }

        private void pictureBoxGraph_MouseMove(object sender, MouseEventArgs e)
        {
            if (isDragging)
            {
                int newTranslationX = translation.X + e.X - lastMousePosition.X;
                int newTranslationY = translation.Y + e.Y - lastMousePosition.Y;

                translation.X = Math.Max(movementBounds.Left, Math.Min(newTranslationX, movementBounds.Right));
                translation.Y = Math.Max(movementBounds.Top, Math.Min(newTranslationY, movementBounds.Bottom));

                lastMousePosition = e.Location;

                pictureBoxGraph.Invalidate(); 
            }
        }
        private void pictureBoxGraph_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                isDragging = false;
            }
        }

        private void button1_Click_2(object sender, EventArgs e)
        {
            scale = 1.0f;
            translation = new Point(0, 0);

            pictureBoxGraph.Invalidate();
        }

        private void AboutProgramToolStripMenuItem_Click(object sender, EventArgs e)
        {
            AboutProgram aboutProgram = new AboutProgram();
            aboutProgram.ShowDialog();
        }

        private void DocumentationToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string documentationPath = "Documentation.pdf";

            try
            {
                System.Diagnostics.Process.Start(documentationPath);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Не удалось открыть файл документации: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }


    }
}