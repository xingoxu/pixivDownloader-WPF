using FirstFloor.ModernUI.Windows.Controls;
using System;
using System.Collections.Generic;
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

namespace pixiv_downloader
{
    /// <summary>
    /// Interaction logic for newDownloadTaskDialog.xaml
    /// </summary>
    public partial class newDownloadTaskDialog : ModernDialog
    {
        public newDownloadTaskDialog()
        {
            InitializeComponent();

            // define the dialog buttons
            this.Buttons = new Button[] { this.OkButton, this.CancelButton };
        }

        private void selectFolderButton_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.FolderBrowserDialog folderbrowserdialog = new System.Windows.Forms.FolderBrowserDialog();
            if (folderbrowserdialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                FolderTextBox.Text = folderbrowserdialog.SelectedPath;
            }
        }
    }
}
