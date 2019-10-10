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
using System.Management;

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

            //clear out the textboxes
            outputTextbox.Text = "";
            diskInfoTextbox.Text = "";
            computerInfoTextbox.Text = "";
            
            //computer name, domain, username, and password
            //used to actually connect to a machine and authenticate
            string computerName = computerNameTextbox.Text;
            string domain = "net.ucf.edu";
            string username = usernameTextbox.Text;
            string password = passwordTextbox.Password;

            SecureString securePassword = new SecureString(); //change the password to a secure string
            foreach (char c in password) {
                securePassword.AppendChar(c);
            }

            CimCredential Credentials = new CimCredential(PasswordAuthenticationMechanism.Default, domain, username, securePassword);

            WSManSessionOptions SessionOptions = new WSManSessionOptions();
            SessionOptions.AddDestinationCredentials(Credentials);

            CimSession Session = CimSession.Create(computerName, SessionOptions);

            //var allVolumes = Session.QueryInstances(@"root\cimv2", "WQL", "SELECT * FROM Win32_Volume");
            //========================================== PROGRAMS ===================================================

            int totalPrograms = 0;

            if (programCheckbox.IsChecked == true) {
                var allPrograms = Session.QueryInstances(@"root\cimv2", "WQL", "SELECT * FROM Win32_Product");
                foreach (CimInstance program in allPrograms) {
                    string text = Convert.ToString(program.CimInstanceProperties["Name"].Value);
                    outputTextbox.AppendText(text + "\n");
                    totalPrograms++;
                }
            }

            totalLabel.Content = $"Total Programs: {totalPrograms}";
            //========================================== DISK INFO ===================================================
            if (diskInfoCheckbox.IsChecked == true) {
                var allPDisks = Session.QueryInstances(@"root\cimv2", "WQL", "SELECT * FROM Win32_DiskDrive");
                foreach (CimInstance pDisk in allPDisks) {
                    diskInfoTextbox.AppendText(pDisk.CimInstanceProperties["Name"].ToString() + "\n");
                    diskInfoTextbox.AppendText(pDisk.CimInstanceProperties["Model"].ToString() + "\n");
                    diskInfoTextbox.AppendText(pDisk.CimInstanceProperties["Status"].ToString() + "\n");
                    diskInfoTextbox.AppendText(pDisk.CimInstanceProperties["SerialNumber"].ToString() + "\n");
                    diskInfoTextbox.AppendText(pDisk.CimInstanceProperties["Size"].ToString() + "\n");
                }
            }
            //======================================= COMPUTER INFO ===================================================
            if (computerInfoCheckbox.IsChecked == true) {
                var allOS = Session.QueryInstances(@"root\cimv2", "WQL", "SELECT * FROM Win32_OperatingSystem");
                foreach (CimInstance os in allOS) {
                    //======= Available Memory =======
                    string availableMemory = Convert.ToString(Convert.ToDouble(os.CimInstanceProperties["FreePhysicalMemory"].Value) / Math.Pow(2, 10));
                    availableMemory = Convert.ToString(Math.Round(Convert.ToDouble(availableMemory), 0, MidpointRounding.AwayFromZero));
                    //======= Last Boot Up Time =======
                    DateTime lastBootUpTimeDate = Convert.ToDateTime(os.CimInstanceProperties["LastBootUpTime"].Value);
                    string lastBootUpTime = lastBootUpTimeDate.ToString(@"yyyy-MM-dd hh:mm:ss");
                    //======= Operating System =======
                    string osName = Convert.ToString(os.CimInstanceProperties["Caption"].Value);
                    //======= OS Install Date =======
                    DateTime osInstallDate = Convert.ToDateTime(os.CimInstanceProperties["InstallDate"].Value);
                    string osInstall = osInstallDate.ToString(@"yyyy-MM-dd hh:mm:ss");
                    //======= OS Version =======
                    string version = Convert.ToString(os.CimInstanceProperties["Version"].Value);

                    computerInfoTextbox.AppendText($"Operating System: {osName}\n");
                    computerInfoTextbox.AppendText($"Windows Version: {version}\n");
                    computerInfoTextbox.AppendText($"Free Memory [MB]: {availableMemory}\n");
                    computerInfoTextbox.AppendText($"Last Boot Up Time: {lastBootUpTime}\n");
                    computerInfoTextbox.AppendText($"OS Install Date: {osInstall}\n");


                    //computerInfoTextbox.AppendText(Convert.ToString(totalVisableMemorySize));
                }
            }
        }

        private void filterProgramsTextbox_TextChanged(object sender, TextChangedEventArgs e) {

/*            string[] lines = outputTextbox.Text.Split(new[] { Environment.NewLine }, StringSplitOptions.None);
            //outputTextbox.Text = "";

            foreach (string line in lines) {
                if (line.Contains(filterProgramsTextbox.Text)) {
                    outputTextbox.AppendText(line);
                }
            }*/

        }
    }
}
