using FirstFloor.ModernUI.Windows.Controls;
using pixiv_API;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
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
using System.Diagnostics;

namespace pixiv_downloader
{
    /// <summary>
    /// Interaction logic for Login.xaml
    /// </summary>
    public partial class Login : ModernWindow
    {
        public Login()
        {
            InitializeComponent();
        }
        
        private void button_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
            this.Close();

        }

        private void closeButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
        private bool processing = false;
        private CancellationTokenSource cts;
        public pixivAPI pixivAPI;
        public pixivUser pixivUser;
        public string refresh_token = null;
        private async void loginButton_Click(object sender, RoutedEventArgs e)
        {
            if (processing)
            {
                cts.Cancel();
                processing = false;
                loginButton.IconData = PathGeometry.Parse("F1 M 19.0002, 34L 19.0002, 42L 43.7502, 42L 33.7502, 52L 44.2502, 52L 58.2502, 38L 44.2502, 24L 33.7502, 24L 43.7502, 34L 19.0002, 34 Z ");
                return;
            }
            processing = true;
            grid.IsEnabled = false;
            progressring.IsActive = true;
            loginButton.Content = "取消";
            loginButton.IconData = PathGeometry.Parse("F1 M 38,19C 48.4934,19 57,27.5066 57,38C 57,48.4934 48.4934,57 38,57C 27.5066,57 19,48.4934 19,38C 19,27.5066 27.5066,19 38,19 Z M 38,23.75C 35.2116,23.75 32.6102,24.5509 30.4134,25.9352L 50.0648,45.5866C 51.4491,43.3898 52.25,40.7884 52.25,38C 52.25,30.13 45.87,23.75 38,23.75 Z M 23.75,38C 23.75,45.8701 30.1299,52.25 38,52.25C 40.7884,52.25 43.3897,51.4491 45.5865,50.0649L 25.9351,30.4136C 24.5509,32.6103 23.75,35.2117 23.75,38 Z ");
            cts = new CancellationTokenSource();

            OAuth oauth = new OAuth();
            bool result = false;
            try
            {
                result = (!string.IsNullOrWhiteSpace(this.refresh_token)) ? await oauth.authAsync(this.refresh_token, cts) : await oauth.authAsync(usernameTextBox.Text, passwordTextBox.Password, cts);
            }
            catch (Exception error)
            {//taskCancel or overtime
                progressring.IsActive = false;
                loginButton.Content = "登陆";
                grid.IsEnabled = true;
                loginButton.IconData = PathGeometry.Parse("F1 M 19.0002, 34L 19.0002, 42L 43.7502, 42L 33.7502, 52L 44.2502, 52L 58.2502, 38L 44.2502, 24L 33.7502, 24L 43.7502, 34L 19.0002, 34 Z ");
                MessageBox.Show(error.Message);
                return;
            }
            if (result)
            {
                pixivAPI = new pixivAPI(oauth);
                pixivUser = oauth.User;
                this.refresh_token = pixivUser.refresh_token;
                this.DialogResult = true;
                this.Close();
            }
            else
            {
                error.Visibility = Visibility.Visible;
                Thread hidelabel = new Thread(() =>
                  {
                      Thread.Sleep(5000);
                      try
                      {
                          error.Dispatcher.Invoke(() =>
                          {
                              error.Visibility = Visibility.Hidden;
                          });
                      }
                      catch
                      {

                      }
                  });
                hidelabel.Start();
            }
            progressring.IsActive = false;
            loginButton.Content = "登陆";
            grid.IsEnabled = true;
            loginButton.IconData = PathGeometry.Parse("F1 M 19.0002, 34L 19.0002, 42L 43.7502, 42L 33.7502, 52L 44.2502, 52L 58.2502, 38L 44.2502, 24L 33.7502, 24L 43.7502, 34L 19.0002, 34 Z ");
        }
    }
}
