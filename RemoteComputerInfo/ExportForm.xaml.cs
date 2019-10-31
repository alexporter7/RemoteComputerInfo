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

namespace RemoteComputerInfo {
    /// <summary>
    /// Interaction logic for ExportForm.xaml
    /// </summary>
    public partial class ExportForm : Window {
        public ExportForm() {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e) {

           

        }

        private void usernameTextbox_TextChanged(object sender, TextChangedEventArgs e) {

            if (usernameTextbox.Text.Length > 0 && passwordTextbox.Password.Length > 0) {
                emailButton.IsEnabled = true;
                emailButton.Content = "Email";
            } else {
                emailButton.IsEnabled = false;
                emailButton.Content = "Email [Enter Credentials To Enable]";
            }

        }

        private void passwordTextbox_PasswordChanged(object sender, RoutedEventArgs e) {

            if (usernameTextbox.Text.Length > 0 && passwordTextbox.Password.Length > 0) {
                emailButton.IsEnabled = true;
                emailButton.Content = "Email";
            }
            else {
                emailButton.IsEnabled = false;
                emailButton.Content = "Email [Enter Credentials To Enable]";
            }

        }
    }
}
