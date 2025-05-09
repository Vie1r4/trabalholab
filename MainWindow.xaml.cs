using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace FinalLab
{
    public partial class MainWindow : Window
    {
        private List<Tarefa> listaDeTarefasPrincipal = new List<Tarefa>();

        public MainWindow()
        {
            InitializeComponent();
            UpdatePlaceholderVisibility();
            // Exemplo: Adicionar algumas tarefas iniciais para teste
            // listaDeTarefasPrincipal.Add(new Tarefa { Nome = "Projeto Inicial", Prazo = DateTime.Today.AddDays(10), Peso = 40 });
            // listaDeTarefasPrincipal.Add(new Tarefa { Nome = "Apresentação Teste", Prazo = DateTime.Today.AddDays(20), Peso = 25 });
            // AtualizarTabelaDeTarefasUI(); // Para mostrar as tarefas iniciais
        }

        private void UpdatePlaceholderVisibility()
        {
            if (SearchTextBox != null && PlaceholderTextBlock != null)
            {
                PlaceholderTextBlock.Visibility = string.IsNullOrEmpty(SearchTextBox.Text) ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        private void SearchTextBox_TextChanged_UpdatePlaceholder(object sender, TextChangedEventArgs e)
        {
            UpdatePlaceholderVisibility();
            // Aqui poderia adicionar lógica de filtragem para a tabela de tarefas baseada no texto de pesquisa
        }

        // --- Manipuladores de Botões de Navegação (sem alterações) ---
        private void DashboardButton_Click(object sender, RoutedEventArgs e) { MessageBox.Show("Dashboard Clicado!"); }
        private void AlunosButton_Click(object sender, RoutedEventArgs e) { MessageBox.Show("Alunos Clicado!"); }
        private void GruposButton_Click(object sender, RoutedEventArgs e) { MessageBox.Show("Grupos Clicado!"); }
        private void TarefasButton_Click(object sender, RoutedEventArgs e) { MessageBox.Show("Tarefas Clicado!"); }
        private void PautaButton_Click(object sender, RoutedEventArgs e) { MessageBox.Show("Pauta Clicado!"); }
        private void ConfiguracoesButton_Click(object sender, RoutedEventArgs e) { MessageBox.Show("Configurações Clicado!"); }

        // --- Funcionalidade de Tarefas ---
        private void CriarTarefaButton_Click(object sender, RoutedEventArgs e)
        {
            CriarTarefaWindow criarTarefaWin = new CriarTarefaWindow();
            criarTarefaWin.Owner = this;
            bool? resultado = criarTarefaWin.ShowDialog();

            if (resultado == true && criarTarefaWin.TarefaCriadaComSucesso)
            {
                Tarefa tarefaRecemCriada = criarTarefaWin.NovaTarefa;
                listaDeTarefasPrincipal.Add(tarefaRecemCriada);
                MessageBox.Show($"Nova tarefa '{tarefaRecemCriada.Nome}' adicionada.", "Tarefa Adicionada");
                AtualizarTabelaDeTarefasUI();
            }
        }

        private void ApagarTarefaButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button apagarButton && apagarButton.Tag is Tarefa tarefaParaApagar)
            {
                MessageBoxResult confirmacao = MessageBox.Show(
                    $"Tem a certeza que deseja apagar a tarefa '{tarefaParaApagar.Nome}'?",
                    "Confirmar Apagar Tarefa",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning);

                if (confirmacao == MessageBoxResult.Yes)
                {
                    listaDeTarefasPrincipal.Remove(tarefaParaApagar);
                    AtualizarTabelaDeTarefasUI();
                    MessageBox.Show($"Tarefa '{tarefaParaApagar.Nome}' apagada.", "Tarefa Apagada");
                }
            }
        }

        private void AtualizarTabelaDeTarefasUI()
        {
            // Limpar linhas de dados antigas (exceto o cabeçalho e a RowDefinition final para espaço)
            // A primeira RowDefinition (índice 0) é o cabeçalho.
            // A última RowDefinition é para o '*' (espaço restante).
            while (TaskTableGrid.RowDefinitions.Count > 2) // Se > 2, significa que há linhas de dados + cabeçalho + linha '*'
            {
                // Remove os elementos da penúltima RowDefinition (que é a última linha de dados)
                for (int i = TaskTableGrid.Children.Count - 1; i >= 0; i--)
                {
                    UIElement child = TaskTableGrid.Children[i];
                    if (Grid.GetRow(child) == TaskTableGrid.RowDefinitions.Count - 2) // -2 para apontar para a última linha de dados
                    {
                        TaskTableGrid.Children.RemoveAt(i);
                    }
                }
                // Remove a própria RowDefinition da última linha de dados
                TaskTableGrid.RowDefinitions.RemoveAt(TaskTableGrid.RowDefinitions.Count - 2);
            }
            // Se só sobrou cabeçalho e linha '*', mas ainda há Children (de algum erro anterior), limpar.
            // Esta parte é uma segurança extra, idealmente a lógica acima é suficiente.
            if (TaskTableGrid.RowDefinitions.Count <= 2)
            {
                for (int i = TaskTableGrid.Children.Count - 1; i >= 0; i--)
                {
                    if (Grid.GetRow(TaskTableGrid.Children[i]) > 0)
                    { // Maior que 0 para não remover cabeçalho
                        TaskTableGrid.Children.RemoveAt(i);
                    }
                }
            }


            // Adicionar novas linhas para cada tarefa na lista
            int rowIndex = 1; // Começa na linha 1 (após o cabeçalho que está na linha 0)
            foreach (var tarefa in listaDeTarefasPrincipal)
            {
                // Insere uma nova RowDefinition para esta tarefa ANTES da última (que é a linha '*')
                TaskTableGrid.RowDefinitions.Insert(TaskTableGrid.RowDefinitions.Count - 1, new RowDefinition { Height = GridLength.Auto });

                TaskTableGrid.Children.Add(CreateTableCellUI(tarefa.Nome, rowIndex, 0));
                TaskTableGrid.Children.Add(CreateTableCellUI(tarefa.Prazo.ToShortDateString(), rowIndex, 1));
                TaskTableGrid.Children.Add(CreateTableCellUI($"{tarefa.Peso}%", rowIndex, 2));

                // Criar e adicionar o botão Apagar
                Button apagarButton = new Button
                {
                    Content = "Apagar", // Ou um ícone: Text="🗑", FontFamily="Segoe MDL2 Assets"
                    Tag = tarefa,       // Armazena a referência da tarefa no botão
                    Margin = new Thickness(5, 2, 5, 2),
                    Padding = new Thickness(5, 2, 5, 2),
                    Foreground = Brushes.Red // Opcional: estilizar o botão
                };
                apagarButton.Click += ApagarTarefaButton_Click; // Associar o evento
                Grid.SetRow(apagarButton, rowIndex);
                Grid.SetColumn(apagarButton, 3); // Coluna das Ações
                TaskTableGrid.Children.Add(apagarButton);

                rowIndex++;
            }
        }

        // Método auxiliar para criar uma célula da tabela (Border com TextBlock) para a UI
        private Border CreateTableCellUI(string text, int row, int column)
        {
            Border cellBorder = new Border
            {
                BorderBrush = Brushes.LightGray,
                BorderThickness = new Thickness(0, 0, 0, 1), // Apenas borda inferior para linhas de dados
                Padding = new Thickness(10)
            };
            TextBlock content = new TextBlock { Text = text };
            cellBorder.Child = content;
            Grid.SetRow(cellBorder, row);
            Grid.SetColumn(cellBorder, column);
            return cellBorder;
        }
    }
}