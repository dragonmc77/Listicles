using Playnite.SDK;
using Playnite.SDK.Events;
using Playnite.SDK.Models;
using Playnite.SDK.Plugins;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
    /// Interaction logic for AddToListicle.xaml
    /// </summary>
    public partial class AddToListicle : UserControl
    {
        private ListiclesViewModel ViewModel { get; set; }
        private bool skipEvent;
        private List<Game> gameList;
        public AddToListicle()
        {
            InitializeComponent();
        }

        public AddToListicle(ListiclesViewModel viewModel, Window window, List<Game> selectedGames)
        {
            InitializeComponent();
            ViewModel = viewModel;
            DataContext = ViewModel;
            cmbListicles.SelectedItem = ViewModel.CurrentListicle;
            Window = window;
            gameList = selectedGames;
        }

        public Window Window { get; }
        private void btnOK_Click(object sender, RoutedEventArgs e)
        {
            if (rdoNew.IsChecked == true)
            {
                Listicles.ListiclesItem newItem = new Listicles.ListiclesItem();
                newItem.Index = ViewModel.Listicles.Count;
                newItem.Title = txtNewListicleTitle.Text;
                newItem.Link = txtNewListicleLink.Text;
                newItem.Games = new ObservableCollection<Game>();
                gameList.ForEach(x => newItem.Games.Add(x));
                ViewModel.Listicles.Add(newItem);
            }
            else
            {
                if (cmbListicles.SelectedIndex > -1)
                {
                    Listicles.ListiclesItem item = ViewModel.Listicles.Single(x => x.Id == Guid.Parse(cmbListicles.SelectedValue.ToString()));
                    gameList.ForEach(x => item.Games.Add(x));
                    ViewModel.CurrentListicle = ViewModel.Listicles.First(x => x.Id.ToString() == cmbListicles.SelectedValue.ToString());
                }
            }
            Window.Close();
        }

        private void cmbListicles_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            rdoExisting.IsChecked = true;
        }

        private void txtNewListicleTitle_TextChanged(object sender, TextChangedEventArgs e)
        {
            rdoNew.IsChecked = true;
        }

        private void txtNewListicleLink_TextChanged(object sender, TextChangedEventArgs e)
        {
            rdoNew.IsChecked = true;
        }

        private void AddToListicleDialog_Unloaded(object sender, RoutedEventArgs e)
        {
            cmbListicles.SelectedItem = ViewModel.CurrentListicle;
        }

        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            cmbListicles.SelectedItem = ViewModel.CurrentListicle;
        }
    }
}
