using Playnite.SDK.Models;
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
    /// <summary>
    /// Interaction logic for EditListicle.xaml
    /// </summary>
    public partial class EditListicle : UserControl
    {
        public string NewTitle = String.Empty;
        public string NewLink = String.Empty;
        private Window Window { get; set; }
        public EditListicle()
        {
            InitializeComponent();
        }

        public EditListicle(Window window, Listicles.ListiclesItem selectedListicle)
        {
            InitializeComponent();
            txtEditListicleTitle.Text = selectedListicle.Title;
            txtEditListicleLink.Text = selectedListicle.Link;
            Window = window;
        }

        private void btnOK_Click(object sender, RoutedEventArgs e)
        {
            if (txtEditListicleTitle.Text != String.Empty & txtEditListicleLink.Text != String.Empty)
            {
                NewTitle = txtEditListicleTitle.Text;
                NewLink = txtEditListicleLink.Text;
            }
            Window.DialogResult = true;
            Window.Close();
        }
    }
}
