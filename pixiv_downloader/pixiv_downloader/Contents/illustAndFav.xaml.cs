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

namespace pixiv_downloader.Contents
{
    /// <summary>
    /// Interaction logic for illustAndFav.xaml
    /// </summary>
    public partial class illustAndFav : UserControl
    {
        public pixivAuthor authorSelected;
        public illustAndFav()
        {
            InitializeComponent();
        }

        private void TabChooser_SelectedSourceChanged(object sender, FirstFloor.ModernUI.Windows.Controls.SourceEventArgs e)
        {
            if (TabChooser.SelectedSource == null)
            {
                return;
            }
            
        }
    }
}
