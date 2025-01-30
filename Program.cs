using System;
using System.IO;
using System.Windows.Forms;
using PDFSign.Modulos;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using iText.Layout.Properties;


namespace PDFSign;

static class Program
{
    /// <summary>
    ///  The main entry point for the application.
    /// </summary>
    [STAThread]
    static void Main()
    {
        // To customize application configuration such as set high DPI settings or default font,
        // see https://aka.ms/applicationconfiguration.
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);
        //ApplicationConfiguration.Initialize();
        Application.Run(new MainForm());
    }

    public partial class MainForm : Form
    {
        private TextBox fileNameTextBox = null!;
        private Button generateButton = null!;
        private Label statusLabel = null!;
        private string pdfDirectory = @"C:\Dev\PDFSign\pdfs";

        public MainForm()
        {
            try
            {
                InitializeComponents();
                CreatePdfDirectory();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao inicializar componentes: {ex.Message}", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void InitializeComponents()
        {
            // Configurar o formulário
            Text = "Gerador de PDF";
            Width = 500;
            Height = 200;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            StartPosition = FormStartPosition.CenterScreen;

            // Label para o nome do arquivo
            Label fileNameLabel = new Label
            {
                Text = "Nome do arquivo PDF (sem extensão):",
                Location = new Point(20, 20),
                Width = 200
            };

            // TextBox para o nome do arquivo
            fileNameTextBox = new TextBox
            {
                Location = new Point(20, 45),
                Width = 440
            };

            // Botão Gerar PDF
            generateButton = new Button
            {
                Text = "Gerar PDF",
                Location = new Point(20, 80),
                Width = 100,
                Height = 30
            };
            generateButton.Click += GenerateButton_Click;

            // Label para status
            statusLabel = new Label
            {
                Text = "",
                Location = new Point(20, 120),
                Width = 440,
                Height = 40
            };

            // Adicionar controles ao formulário
            Controls.AddRange(new Control[] { 
                fileNameLabel, 
                fileNameTextBox, 
                generateButton, 
                statusLabel 
            });
        }

        private void CreatePdfDirectory()
        {
            try
            {
                if (!Directory.Exists(pdfDirectory))
                {
                    Directory.CreateDirectory(pdfDirectory);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao criar diretório: {ex.Message}", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void GenerateButton_Click(object? sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(fileNameTextBox.Text))
            {
                MessageBox.Show("Por favor, insira um nome para o arquivo.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string outputPath = Path.Combine(pdfDirectory, $"{fileNameTextBox.Text}.pdf");

            try
            {
                CreatePdf(outputPath);
                statusLabel.Text = $"PDF criado com sucesso em:\n{outputPath}";

                if (MessageBox.Show("PDF criado com sucesso! Deseja abrir o arquivo?", "Sucesso", 
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                    {
                        FileName = outputPath,
                        UseShellExecute = true
                    });
                }
            }
            catch (Exception ex)
            {
                statusLabel.Text = "Erro ao criar PDF";
                MessageBox.Show($"Erro ao criar PDF: {ex.Message}", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void CreatePdf(string outputPath)
        {
            var dados = new (string, string)[]
            {
                ("ASSUNTO", "Relatório Anual"),
                ("AUTOR (NOME)", "rickgateiro"),
                ("LOCAL DA DIGITALIZAÇÃO", "São Paulo"),
                ("DATA DA DIGITALIZAÇÃO", DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss")),
                ("IDENTIFICADOR DO DOCUMENTO", "123456"),
                ("RESPONSÁVEL PELA DIGITALIZAÇÃO", "rickgateiro"),
                ("TÍTULO", "Relatório de Desempenho"),
                ("TIPO DOCUMENTAL", "Relatório"),
                ("HASH", "abc123xyz"),
                ("CLASSE", "Confidencial"),
                ("DATA DE PRODUÇÃO", "2025-01-29 16:17:17"),
                ("DESTINAÇÃO DO DOCUMENTO", "Arquivo Permanente"),
                ("GÊNERO", "Documento Eletrônico"),
                ("PRAZO DE GUARDA", "Indeterminado")
            };

            using (PdfWriter writer = new PdfWriter(outputPath))
            using (PdfDocument pdf = new PdfDocument(writer))
            {
                Document document = new Document(pdf);

                Table table = new Table(UnitValue.CreatePercentArray(2)).UseAllAvailableWidth();

                foreach (var (label, value) in dados)
                {
                    table.AddCell(new Cell().Add(new Paragraph(label)));
                    table.AddCell(new Cell().Add(new Paragraph(value)));
                }

                document.Add(table);
                document.Close();
            }
        }
    }

}