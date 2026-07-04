using GreenKeeper.Models;
using GreenKeeper.ViewModels.Notes;
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
using System.Windows.Shapes;

namespace GreenKeeper.Views.Notes
{
    /// <summary>
    /// Interaction logic for NotesView.xaml
    /// </summary>
    public partial class NotesView : Window
    {
        public NotesView(Plant plant)
        {
            InitializeComponent();
            this.DataContext = new NotesViewModel(plant);
        }
    }
}
