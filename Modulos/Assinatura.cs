using System;
using System.IO;
using System.Windows.Forms;
using iText.Kernel.Pdf;
using iText.Signatures;
using Org.BouncyCastle.Pkcs;
using Org.BouncyCastle.X509;
using Org.BouncyCastle.Security;

namespace PDFSign.Modulos
{
    public partial class Assinatura : Form
    {
        private Button selectButton = null!;
        private Label statusLabel = null!;

        public Assinatura()
        {
            InitializeComponents();
        }

        private void InitializeComponents()
        {
            // Configurar o formulário
            Text = "Assinatura de PDF";
            Width = 500;
            Height = 200;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            StartPosition = FormStartPosition.CenterScreen;

            // Botão Selecionar PDF
            selectButton = new Button
            {
                Text = "Selecionar PDF",
                Location = new Point(20, 20),
                Width = 100,
                Height = 30
            };
            selectButton.Click += SelectButton_Click;

            // Label para status
            statusLabel = new Label
            {
                Text = "",
                Location = new Point(20, 70),
                Width = 440,
                Height = 40
            };

            // Adicionar controles ao formulário
            Controls.AddRange(new Control[] { selectButton, statusLabel });
        }

        private void SelectButton_Click(object? sender, EventArgs e)
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = "PDF files (*.pdf)|*.pdf";
                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    string pdfPath = openFileDialog.FileName;
                    try
                    {
                        SignPdf(pdfPath);
                        statusLabel.Text = "PDF assinado com sucesso!";
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Erro ao assinar PDF: {ex.Message}", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void SignPdf(string pdfPath)
        {
            string keyPath = @"caminho\para\arquivo_de_chave.p12"; // Alterar para o caminho do arquivo P12
            char[] password = "sua_senha".ToCharArray(); // Alterar para a senha correta

            Pkcs12Store pk12 = new Pkcs12Store(new FileStream(keyPath, FileMode.Open, FileAccess.Read), password);
            string alias = "";
            foreach (string tAlias in pk12.Aliases)
            {
                if (pk12.IsKeyEntry(tAlias))
                {
                    alias = tAlias;
                    break;
                }
            }

            ICipherParameters pk = pk12.GetKey(alias).Key;
            X509CertificateEntry[] chain = pk12.GetCertificateChain(alias);

            using (PdfReader reader = new PdfReader(pdfPath))
            using (PdfWriter writer = new PdfWriter(pdfPath.Replace(".pdf", "_signed.pdf")))
            using (PdfDocument pdfDoc = new PdfDocument(reader, writer))
            {
                PdfSigner signer = new PdfSigner(pdfDoc, new FileStream(pdfPath.Replace(".pdf", "_signed.pdf"), FileMode.Create), new StampingProperties());

                IExternalSignature pks = new PrivateKeySignature(pk, DigestAlgorithms.SHA256);
                IExternalDigest digest = new BouncyCastleDigest();

                signer.SignDetached(digest, pks, chain, null, null, null, 0, PdfSigner.CryptoStandard.CMS);
            }
        }
    }
}
