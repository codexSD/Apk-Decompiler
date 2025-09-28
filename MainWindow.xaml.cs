using System.Windows;
using Apk_Decompiler.ViewModels;
using ModernWpf;

namespace Apk_Decompiler
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            DataContext = new MainViewModel();
            
            // Set theme
            ThemeManager.Current.ApplicationTheme = ApplicationTheme.Light;
        }
    }
}