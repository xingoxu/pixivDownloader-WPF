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
    /// Interaction logic for chooseRoute.xaml
    /// </summary>
    public partial class chooseRoute : ModernDialog
    {
        public chooseRoute()
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

        private void routeBySystem_Click(object sender, RoutedEventArgs e)
        {
            if ((bool)routeByselfCheckBox.IsChecked)
            {
                selectFolderButton.IsEnabled = true;
            }
            else
            {
                selectFolderButton.IsEnabled = false;
            }

        }

        private void routeByselfCheckBox_Click(object sender, RoutedEventArgs e)
        {
            if ((bool)routeByselfCheckBox.IsChecked)
            {
                selectFolderButton.IsEnabled = true;
            }
            else
            {
                selectFolderButton.IsEnabled = false;
            }
        }
    }
}
