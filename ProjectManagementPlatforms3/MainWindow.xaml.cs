using ProjectManagementPlatform.ViewModels;
using System.Windows;

namespace ProjectManagementPlatform
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            DataContext = new MainViewModel();
        }
    }
}