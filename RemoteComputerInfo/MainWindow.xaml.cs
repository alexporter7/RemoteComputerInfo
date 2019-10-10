using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
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
using Microsoft.Management.Infrastructure;
using Microsoft.Management.Infrastructure.Options;

namespace RemoteComputerInfo {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {
        public MainWindow() {
            InitializeComponent();
            usernameTextbox.Text = "al086950admin";
        }

        private void submitNameButton_Click(object sender, RoutedEventArgs e) {
            //computer name, domain, username, and password
            //used to actually connect to a machine and authenticate
            string computerName = computerNameTextbox.Text;
            string domain = "net.ucf.edu";
            string username = usernameTextbox.Text;
            string password = passwordTextbox.Password;


            SecureString securePassword = new SecureString();
            foreach (char c in password) {
                securePassword.AppendChar(c);
            }

            CimCredential Credentials = new CimCredential(PasswordAuthenticationMechanism.Default, domain, username, securePassword);

            WSManSessionOptions SessionOptions = new WSManSessionOptions();
            SessionOptions.AddDestinationCredentials(Credentials);

            CimSession Session = CimSession.Create(computerName, SessionOptions);

            var allVolumes = Session.QueryInstances(@"root\cimv2", "WQL", "SELECT * FROM Win32_Volume");
            var allPDisks = Session.QueryInstances(@"root\cimv2", "WQL", "SELECT * FROM Win32_DiskDrive");
/*            var allPrograms = Session.QueryInstances(@"root\cimv2", "WQL", "SELECT * FROM Win32_Product");

            //add programs to list
            foreach (CimInstance program in allPrograms) {
                outputTextbox.AppendText(program.CimInstanceProperties["Name"].ToString() + "\n");
            }*/

            foreach (CimInstance pDisk in allPDisks) {
                diskInfoTextbox.AppendText(pDisk.CimInstanceProperties["Name"].ToString() + "\n");
                diskInfoTextbox.AppendText(pDisk.CimInstanceProperties["Model"].ToString() + "\n");
                diskInfoTextbox.AppendText(pDisk.CimInstanceProperties["Status"].ToString() + "\n");
                diskInfoTextbox.AppendText(pDisk.CimInstanceProperties["SerialNumber"].ToString() + "\n");
                diskInfoTextbox.AppendText(pDisk.CimInstanceProperties["Size"].ToString() + "\n");
            }

        }

        private void filterProgramsTextbox_TextChanged(object sender, TextChangedEventArgs e) {

            string[] lines = outputTextbox.Text.Split(new[] { Environment.NewLine }, StringSplitOptions.None);
            //outputTextbox.Text = "";

            foreach (string line in lines) {
                if (line.Contains(filterProgramsTextbox.Text)) {
                    outputTextbox.AppendText(line);
                }
            }

        }
    }
}
