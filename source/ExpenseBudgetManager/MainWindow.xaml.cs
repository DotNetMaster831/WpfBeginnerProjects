using ExpenseBudgetManager.Models;
using ExpenseBudgetManager.ViewModels;
using MaterialDesignThemes.Wpf;
using Serilog.Core;
using System.Collections.ObjectModel;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ExpenseBudgetManager
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        protected override void OnContentRendered(EventArgs e)
        {
            base.OnContentRendered(e);

            if (DataContext is MainViewModel vm)
            {
                vm.PropertyChanged += (_, args) =>
                {
                    if (args.PropertyName == nameof(MainViewModel.CurrentView))
                        FadeInContent();
                };
            }
        }

        private void FadeInContent()
        {
            var fade = new DoubleAnimation(0, 1,
                new Duration(TimeSpan.FromMilliseconds(250)));
            MainContent.BeginAnimation(OpacityProperty, fade);
        }
    }
}