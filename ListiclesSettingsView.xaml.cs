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

namespace Listicles
{
    public partial class ListiclesSettingsView : UserControl
    {
        public ListiclesSettingsView()
        {
            InitializeComponent();
        }

        private void btnCopyFrom_Click(object sender, RoutedEventArgs e)
        {
            string newFolder = Playnite.SDK.API.Instance.Dialogs.SelectFolder();
            if (newFolder != String.Empty) { 
                txtCopyFrom.Text = newFolder;
                /*  i have to set focus to the text box manually because just changing the
                    .Text property does not trigger an update via the binding */
                txtCopyFrom.Focus();
            }
            
            
        }

        private void btnCopyTo_Click(object sender, RoutedEventArgs e)
        {
            string newFolder = Playnite.SDK.API.Instance.Dialogs.SelectFolder();
            if (newFolder != String.Empty)
            {
                txtCopyTo.Text = newFolder;
                /*  i have to set focus to the text box manually because just changing the
                    .Text property does not trigger an update via the binding */
                txtCopyTo.Focus();
            }
        }
    }
}