using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;

namespace NeurBox
{
    /// <summary>
    /// Interaction logic for HelpContent.xaml
    /// </summary>
    public partial class HelpContent : UserControl
    {
        public HelpContent()
        {
            InitializeComponent();
        }

        private void HandleLinkClick(object sender, RoutedEventArgs e)
        {
            Hyperlink hl = (Hyperlink)sender;
            string navigateUri = hl.NavigateUri.ToString();
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo("cmd", $"/c start {navigateUri}") { CreateNoWindow = true });
            e.Handled = true;
        }
    }
}
