using System.Windows;
using Orbit.ViewModels;

namespace Orbit.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml.
    /// Strict MVVM code-behind: only initializes components and binds DataContext.
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            DataContext = new MainWindowViewModel();
        }
    }
}
