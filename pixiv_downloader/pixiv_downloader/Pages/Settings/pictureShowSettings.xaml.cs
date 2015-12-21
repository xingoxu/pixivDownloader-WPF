using FirstFloor.ModernUI.Windows;
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
using FirstFloor.ModernUI.Windows.Navigation;

namespace pixiv_downloader.Pages.Settings
{
    /// <summary>
    /// Interaction logic for pictureShowSettings.xaml
    /// </summary>
    public partial class pictureShowSettings : UserControl, IContent
    {
        public pictureShowSettings()
        {
            InitializeComponent();
        }
        ConfigSettings configsetting;
        public void OnFragmentNavigation(FirstFloor.ModernUI.Windows.Navigation.FragmentNavigationEventArgs e)
        {
        }

        public void OnNavigatedFrom(FirstFloor.ModernUI.Windows.Navigation.NavigationEventArgs e)
        {
        }

        public void OnNavigatedTo(FirstFloor.ModernUI.Windows.Navigation.NavigationEventArgs e)
        {
            configsetting = ((MainWindow)App.Current.MainWindow).configsettings;
            if (configsetting == null) App.Current.MainWindow.Close();//should be improved
            showR18.IsChecked = configsetting.showR18;
            savePassWord.IsChecked = configsetting.savePassword;
            autoSaveTask.IsChecked = configsetting.autoSaveTask;
            showDownloadDialog.IsChecked = configsetting.showDownloadDialog;
            FolderTextBox.Text = configsetting.workPath;
        }

        public void OnNavigatingFrom(FirstFloor.ModernUI.Windows.Navigation.NavigatingCancelEventArgs e)
        {

        }

        private void showR18_Click(object sender, RoutedEventArgs e)
        {
            configsetting.showR18 = (bool)showR18.IsChecked;
        }

        private void savePassWord_Click(object sender, RoutedEventArgs e)
        {
            configsetting.savePassword = (bool)savePassWord.IsChecked;
            if (!configsetting.savePassword)
            {
                configsetting.UserName = null;
                configsetting.PassWord = null;
            }
        }

        private void autoSaveTask_Click(object sender, RoutedEventArgs e)
        {
            configsetting.autoSaveTask = (bool)autoSaveTask.IsChecked;
        }

        private void selectFolderButton_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.FolderBrowserDialog folderbrowserdialog = new System.Windows.Forms.FolderBrowserDialog();
            if (folderbrowserdialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                FolderTextBox.Text = folderbrowserdialog.SelectedPath;
                if (FolderTextBox.Text != "") configsetting.workPath = FolderTextBox.Text;
            }
        }

        private void showDownloadDialog_Click(object sender, RoutedEventArgs e)
        {
            configsetting.showDownloadDialog = (bool)showDownloadDialog.IsChecked;
        }
    }
}
