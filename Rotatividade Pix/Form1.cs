using Rotatividade_Pix;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace Rotatividade_Pix 
{
    public partial class Form1 : Form
    {
        private List<ChavePix> listaChaves = new List<ChavePix>();
        private BindingSource bindingSource = new BindingSource();

        private TextBox txtNome, txtBanco, txtChave;
        private DataGridView gridChaves;
        private Button btnAdicionar, btnUsar, btnForcar, btnRemover;

        private string caminhoArquivo = "contas.txt";

        public Form1()
        {
            ConfigurarInterfaceModernizada();
            GarantirArquivoExiste();
            CarregarDados();

            bindingSource.DataSource = listaChaves;
            gridChaves.DataSource = bindingSource;

            gridChaves.Columns["UltimoUso"].Visible = false;
            gridChaves.Columns["EstaDisponivel"].Visible = false;

            // Renomeia os cabeçalhos para ficarem mais bonitos
            gridChaves.Columns["Nome"].HeaderText = "Nome do Titular";
            gridChaves.Columns["Banco"].HeaderText = "Instituição";
            gridChaves.Columns["Chave"].HeaderText = "Chave Pix";
            gridChaves.Columns["Status"].HeaderText = "Status Atual";
        }

       
        private void MostrarNotificacao(string mensagem, Color corFundo)
        {
            Label lblToast = new Label
            {
                Text = mensagem,
                BackColor = corFundo,
                ForeColor = Color.White,
                AutoSize = false,
                Width = 400,
                Height = 40,
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Location = new Point((this.Width - 400) / 2, 20), // Aparece centralizado no topo
                BorderStyle = BorderStyle.None
            };

            this.Controls.Add(lblToast);
            lblToast.BringToFront(); // Garante que fique por cima de tudo

            // Cria um cronômetro de 3 segundos
            System.Windows.Forms.Timer timerNotificacao = new System.Windows.Forms.Timer { Interval = 3000 };
            timerNotificacao.Tick += (s, ev) =>
            {
                // Quando o tempo acaba, remove a mensagem e desliga o cronômetro
                this.Controls.Remove(lblToast);
                lblToast.Dispose();
                timerNotificacao.Stop();
                timerNotificacao.Dispose();
            };
            timerNotificacao.Start();
        }

   
        private void GarantirArquivoExiste()
        {
            if (!File.Exists(caminhoArquivo)) File.Create(caminhoArquivo).Close();
        }

        private void CarregarDados()
        {
            if (File.Exists(caminhoArquivo))
            {
                string[] linhas = File.ReadAllLines(caminhoArquivo);
                foreach (string linha in linhas)
                {
                    string[] pedacos = linha.Split('|');
                    if (pedacos.Length >= 3)
                    {
                        var chave = new ChavePix { Nome = pedacos[0], Banco = pedacos[1], Chave = pedacos[2] };
                        if (pedacos.Length == 4 && !string.IsNullOrEmpty(pedacos[3]))
                        {
                            if (DateTime.TryParse(pedacos[3], out DateTime dataUso)) chave.UltimoUso = dataUso;
                        }
                        listaChaves.Add(chave);
                    }
                }
            }
        }

        private void SalvarDados()
        {
            List<string> linhasParaSalvar = new List<string>();
            foreach (var chave in listaChaves)
            {
                string dataUsoTexto = chave.UltimoUso.HasValue ? chave.UltimoUso.Value.ToString("o") : "";
                linhasParaSalvar.Add($"{chave.Nome}|{chave.Banco}|{chave.Chave}|{dataUsoTexto}");
            }
            File.WriteAllLines(caminhoArquivo, linhasParaSalvar);
        }

        private void btnAdicionar_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtNome.Text) || string.IsNullOrWhiteSpace(txtChave.Text))
            {
                MostrarNotificacao("Preencha ao menos o Nome e a Chave!", Color.Crimson);
                return;
            }

            listaChaves.Add(new ChavePix { Nome = txtNome.Text, Banco = txtBanco.Text, Chave = txtChave.Text, UltimoUso = null });
            AtualizarTabela();
            SalvarDados();

            txtNome.Clear(); txtBanco.Clear(); txtChave.Clear();
            MostrarNotificacao("Chave adicionada com sucesso!", Color.SeaGreen);
        }

        private void btnUsar_Click(object sender, EventArgs e)
        {
            if (gridChaves.CurrentRow == null)
            {
                MostrarNotificacao("Selecione uma chave na tabela primeiro.", Color.DarkOrange);
                return;
            }

            var chave = (ChavePix)gridChaves.CurrentRow.DataBoundItem;

            if (!chave.EstaDisponivel)
            {
                MostrarNotificacao($"Bloqueada até: {chave.UltimoUso.Value.AddHours(48):dd/MM às HH:mm}", Color.Crimson);
                return;
            }

            // A confirmação continua existindo para evitar erro
            if (CaixaConfirmacao.Mostrar("Confirmar Uso", $"Deseja marcar a chave de {chave.Nome} como USADA?", Color.SeaGreen))
            {
                chave.UltimoUso = DateTime.Now;
                AtualizarTabela();
                SalvarDados();
                MostrarNotificacao("Marcada como usada! Bloqueada por 48h.", Color.SeaGreen);
            }
        }

        private void btnRemover_Click(object sender, EventArgs e)
        {
            if (gridChaves.CurrentRow == null) return;
            var chave = (ChavePix)gridChaves.CurrentRow.DataBoundItem;

            if (CaixaConfirmacao.Mostrar("Confirmar Exclusão", $"Tem certeza que deseja EXCLUIR permanentemente a chave de {chave.Nome}?", Color.Crimson))
            {
                listaChaves.Remove(chave);
                AtualizarTabela();
                SalvarDados();
                MostrarNotificacao("Chave excluída do sistema.", Color.Crimson);
            }
        }

        private void btnForcar_Click(object sender, EventArgs e)
        {
            if (gridChaves.CurrentRow == null) return;
            var chave = (ChavePix)gridChaves.CurrentRow.DataBoundItem;
            if (chave.EstaDisponivel)
            {
                MostrarNotificacao("Esta chave já está disponível!", Color.SteelBlue);
                return;
            }

            if (CaixaConfirmacao.Mostrar("Forçar Liberação", $"Deseja FORÇAR a disponibilidade de {chave.Nome} agora?", Color.SteelBlue))
            {
                chave.UltimoUso = null;
                AtualizarTabela();
                SalvarDados();
                MostrarNotificacao("Liberação forçada com sucesso!", Color.SteelBlue);
            }
        }

        private void gridChaves_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;
            if (gridChaves.Columns[e.ColumnIndex].Name == "Chave")
            {
                string chaveTexto = gridChaves.Rows[e.RowIndex].Cells[e.ColumnIndex].Value?.ToString();
                if (!string.IsNullOrEmpty(chaveTexto))
                {
                    Clipboard.SetText(chaveTexto);
                    MostrarNotificacao($"Chave {chaveTexto} copiada!", Color.MediumSlateBlue);
                }
            }
        }

        private void AtualizarTabela()
        {
            bindingSource.ResetBindings(false);
            gridChaves.Refresh();
        }


        private void ConfigurarInterfaceModernizada()
        {
            this.Text = "Controle de Chave Pix";
            this.Size = new Size(650, 480);
            this.BackColor = Color.FromArgb(32, 33, 36);
            this.ForeColor = Color.White;
            this.Font = new Font("Segoe UI", 9.5f, FontStyle.Regular);
            this.StartPosition = FormStartPosition.CenterScreen;

            Color corCampos = Color.FromArgb(41, 42, 45);

            Label lblNome = new Label { Text = "Nome do Titular:", Location = new Point(20, 20), AutoSize = true, ForeColor = Color.LightGray };
            txtNome = new TextBox { Location = new Point(20, 45), Width = 160, BackColor = corCampos, ForeColor = Color.White, BorderStyle = BorderStyle.FixedSingle };

            Label lblBanco = new Label { Text = "Instituição:", Location = new Point(200, 20), AutoSize = true, ForeColor = Color.LightGray };
            txtBanco = new TextBox { Location = new Point(200, 45), Width = 160, BackColor = corCampos, ForeColor = Color.White, BorderStyle = BorderStyle.FixedSingle };

            Label lblChave = new Label { Text = "Chave Pix:", Location = new Point(380, 20), AutoSize = true, ForeColor = Color.LightGray };
            txtChave = new TextBox { Location = new Point(380, 45), Width = 150, BackColor = corCampos, ForeColor = Color.White, BorderStyle = BorderStyle.FixedSingle };

            btnAdicionar = new Button { Text = "➕ Adicionar", Location = new Point(540, 43), Width = 70, Height = 26, FlatStyle = FlatStyle.Flat, BackColor = Color.FromArgb(0, 120, 215), ForeColor = Color.White, Cursor = Cursors.Hand };
            btnAdicionar.FlatAppearance.BorderSize = 0;
            btnAdicionar.Click += btnAdicionar_Click;

    
            Button btnInfo = new Button
            {
                Text = "?",
                Location = new Point(580, 10), // Posicionado no cantinho direito superior
                Size = new Size(30, 30),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(50, 50, 55), // Um pouco mais claro que o fundo para destacar xddddddddddddddddddddd
                ForeColor = Color.LightGray,
                Font = new Font("Segoe UI", 11f, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnInfo.FlatAppearance.BorderSize = 0;
            //Chama a notificação flutuante azul com o seu nome
            btnInfo.Click += (s, e) => { MostrarNotificacao("Desenvolvido por Jarry", Color.FromArgb(0, 120, 215)); };

            gridChaves = new DataGridView
            {
                Location = new Point(20, 90),
                Size = new Size(590, 250),
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                ReadOnly = true,
                AllowUserToAddRows = false,
                BackgroundColor = Color.FromArgb(41, 42, 45),
                BorderStyle = BorderStyle.None,
                CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal,
                EnableHeadersVisualStyles = false,
                GridColor = Color.FromArgb(60, 60, 60),
                Cursor = Cursors.Hand,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
            };

            gridChaves.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            gridChaves.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            gridChaves.DefaultCellStyle.BackColor = Color.FromArgb(41, 42, 45);
            gridChaves.DefaultCellStyle.ForeColor = Color.White;
            gridChaves.DefaultCellStyle.SelectionBackColor = Color.FromArgb(75, 75, 80);
            gridChaves.DefaultCellStyle.SelectionForeColor = Color.White;
            gridChaves.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None;
            gridChaves.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(25, 25, 25);
            gridChaves.ColumnHeadersDefaultCellStyle.ForeColor = Color.LightGray;
            gridChaves.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9.5f, FontStyle.Bold);
            gridChaves.ColumnHeadersHeight = 35;
            gridChaves.CellClick += gridChaves_CellClick;

            btnUsar = new Button { Text = "✔️ Marcar Uso (48h)", Location = new Point(20, 360), Size = new Size(160, 40), FlatStyle = FlatStyle.Flat, BackColor = Color.SeaGreen, ForeColor = Color.White, Cursor = Cursors.Hand };
            btnUsar.FlatAppearance.BorderSize = 0;
            btnUsar.Click += btnUsar_Click;

            btnForcar = new Button { Text = "⚡ Forçar Liberação", Location = new Point(190, 360), Size = new Size(160, 40), FlatStyle = FlatStyle.Flat, BackColor = Color.SteelBlue, ForeColor = Color.White, Cursor = Cursors.Hand };
            btnForcar.FlatAppearance.BorderSize = 0;
            btnForcar.Click += btnForcar_Click;

            btnRemover = new Button { Text = "🗑️ Remover", Location = new Point(460, 360), Size = new Size(150, 40), FlatStyle = FlatStyle.Flat, BackColor = Color.Crimson, ForeColor = Color.White, Cursor = Cursors.Hand };
            btnRemover.FlatAppearance.BorderSize = 0;
            btnRemover.Click += btnRemover_Click;

            this.Controls.Add(lblNome);
            this.Controls.Add(txtNome);
            this.Controls.Add(lblBanco);
            this.Controls.Add(txtBanco);
            this.Controls.Add(lblChave);
            this.Controls.Add(txtChave);
            this.Controls.Add(btnAdicionar);
            this.Controls.Add(btnInfo); // <-- Adicionando o botão novo na tela
            this.Controls.Add(gridChaves);
            this.Controls.Add(btnUsar);
            this.Controls.Add(btnForcar);
            this.Controls.Add(btnRemover);
        }
    }
    public class CaixaConfirmacao : Form
    {
        public bool Confirmado { get; private set; } = false;

        public CaixaConfirmacao(string titulo, string mensagem, Color corDestaque)
        {
            this.Text = titulo;
            this.Size = new Size(420, 200);
            this.BackColor = Color.FromArgb(32, 33, 36);
            this.ForeColor = Color.White;
            this.Font = new Font("Segoe UI", 10f, FontStyle.Regular);
            this.StartPosition = FormStartPosition.CenterParent; // Abre no centro do programa
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.ShowIcon = false;

            Label lblMensagem = new Label
            {
                Text = mensagem,
                AutoSize = false,
                TextAlign = ContentAlignment.MiddleCenter,
                Dock = DockStyle.Top,
                Height = 90,
                Padding = new Padding(15)
            };

     
            Button btnSim = new Button
            {
                Text = "✔️ Confirmar",
                Size = new Size(130, 40),
                Location = new Point(70, 100),
                FlatStyle = FlatStyle.Flat,
                BackColor = corDestaque,
                ForeColor = Color.White,
                Cursor = Cursors.Hand
            };
            btnSim.FlatAppearance.BorderSize = 0;
            btnSim.Click += (s, e) => { Confirmado = true; this.Close(); };

           
            Button btnNao = new Button
            {
                Text = "✖️ Cancelar",
                Size = new Size(130, 40),
                Location = new Point(210, 100),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(75, 75, 80),
                ForeColor = Color.White,
                Cursor = Cursors.Hand
            };
            btnNao.FlatAppearance.BorderSize = 0;
            btnNao.Click += (s, e) => { Confirmado = false; this.Close(); };

            this.Controls.Add(lblMensagem);
            this.Controls.Add(btnSim);
            this.Controls.Add(btnNao);
        }

      
        public static bool Mostrar(string titulo, string mensagem, Color corDestaque)
        {
            using (var caixa = new CaixaConfirmacao(titulo, mensagem, corDestaque))
            {
                caixa.ShowDialog();
                return caixa.Confirmado;
            }
        }
    }
}