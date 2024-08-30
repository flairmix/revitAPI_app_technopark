using Autodesk.Revit.UI;
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

namespace app_technopark_OT_XC_zones
{
    /// <summary>
    /// Interaction logic for MainView.xaml
    /// </summary>
    public partial class ZonesappView : Window
    {
        public ZonesappView(ZonesAppViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
        }

        private void TextBlock_Selected(object sender, RoutedEventArgs e)
        {

        }
    }
}
