using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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
using System.Windows.Shell;

namespace Chat
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class StartWindow : Window
    {
        public StartWindow()
        {
            InitializeComponent();
        }

        private void ConnectButton(object sender, RoutedEventArgs e)
        {
            if (Regex.IsMatch(IpBox.Text, @"^(?:[0-9]{1,3}\.){3}[0-9]{1,3}$"))
            {
                var clientWindow = new NewChat(false, NameBox.Text, IpBox.Text);
                //this.Visibility = Visibility.Hidden;
                //this.Hide();
                clientWindow.ShowDialog();
            }
            else 
            {
                MessageBox.Show("It's not an IP. Try one more time.");
            }
        }

        private void HostButton(object sender, RoutedEventArgs e)
        {
            var hostWindow = new NewChat(true, NameBox.Text);
            //this.Hide();
            hostWindow.ShowDialog();
        }

        private void IpBox_GotFocus(object sender, RoutedEventArgs e)
        {
            var textBox = sender as TextBox;
            textBox.Text = "";
            textBox.GotFocus -= IpBox_GotFocus;
        }

        private void IpBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter) 
            {
                connectButton.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
            }
        }
    }
}
