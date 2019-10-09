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
            var allPrograms = Session.QueryInstances(@"root\cimv2", "WQL", "SELECT * FROM Win32_Product");

            foreach (CimInstance program in allPrograms) {
                outputTextbox.AppendText($"Program Name [{program.CimInstanceProperties["Name"].ToString()}]\n");
            }

/*            foreach (CimInstance volume in allVolumes) {
                if (volume.CimInstanceProperties["DriveLetter"].ToString()[0] > ' ') {
                    Console.WriteLine("Volume [{0}] has [{1}] bytes total and [{2}] bytes available",
                        volume.CimInstanceProperties["DriveLetter"],
                        volume.CimInstanceProperties["Size"],
                        volume.CimInstanceProperties["SizeRemaining"]);
                }
            }

            foreach (CimInstance onePDisk in allPDisks) {
                // Show physical disk information
                Console.WriteLine("\n\nDisk {0}\n is model {1},\n serial number {2}\n\n",
                                  onePDisk.CimInstanceProperties["DeviceId"],
                                  onePDisk.CimInstanceProperties["Model"].ToString().TrimEnd(),
                                  onePDisk.CimInstanceProperties["SerialNumber"]);
            }*/



        }
    }
}
