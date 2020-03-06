using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace XwaBackupRestorer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public string XwaDirectory
        {
            get { return (string)GetValue(XwaDirectoryProperty); }
            set { SetValue(XwaDirectoryProperty, value); }
        }

        public static readonly DependencyProperty XwaDirectoryProperty =
            DependencyProperty.Register("XwaDirectory", typeof(string), typeof(MainWindow));

        public ObservableCollection<BackupCraft> BackupCrafts { get; } = new ObservableCollection<BackupCraft>();

        public MainWindow()
        {
            InitializeComponent();

            this.DataContext = this;

            this.OpenButton_Click(null, null);
        }

        [SuppressMessage("Reliability", "CA2000:Supprimer les objets avant la mise hors de portée", Justification = "Reviewed.")]
        [SuppressMessage("Design", "CA1031:Ne pas intercepter les types d'exception générale", Justification = "Reviewed.")]
        private void OpenButton_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new WPFFolderBrowser.WPFFolderBrowserDialog
            {
                Title = "Choose a working directory containing XWingAlliance.exe"
            };

            if (dlg.ShowDialog(this) != true)
            {
                return;
            }

            try
            {
                string fileName = dlg.FileName;

                if (!System.IO.File.Exists(System.IO.Path.Combine(fileName, "XWingAlliance.exe")))
                {
                    return;
                }

                this.ClearCrafts();
                this.XwaDirectory = fileName;
                this.LoadCrafts();
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.ToString(), this.Title, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ClearCrafts()
        {
            this.BackupCrafts.Clear();
        }

        private void LoadCrafts()
        {
            this.ClearCrafts();

            List<BackupCraft> crafts = BackupCraft.LoadList(this.XwaDirectory);

            foreach (var craft in crafts.OrderByDescending(t => t.CreationDate))
            {
                this.BackupCrafts.Add(craft);
            }
        }

        [SuppressMessage("Design", "CA1031:Ne pas intercepter les types d'exception générale", Justification = "Reviewed.")]
        private void RestoreButton_Click(object sender, RoutedEventArgs e)
        {
            var button = (Button)sender;

            if (button.Tag != null)
            {
                var selectedCraft = (BackupCraft)button.Tag;
                int seletedCraftIndex = this.BackupCrafts.IndexOf(selectedCraft);

                if (MessageBox.Show(this, "Do you want to restore " + selectedCraft.FolderName + " and more recent backups?", this.Title, MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                {
                    try
                    {
                        for (int index = 0; index <= seletedCraftIndex; index++)
                        {
                            var craft = this.BackupCrafts[0];
                            this.BackupCrafts.RemoveAt(0);
                            craft.Restore(this.XwaDirectory);
                        }

                        MessageBox.Show(this, "Done", this.Title);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(this, ex.ToString(), this.Title, MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }
    }
}
