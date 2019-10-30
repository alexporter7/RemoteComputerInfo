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
using System.Net.NetworkInformation;
using System.Collections;

namespace RemoteComputerInfo {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// 

    public partial class MainWindow : Window {

        public static string username = "";
        public static string password = "";

        public MainWindow() {
            
            InitializeComponent();
            
            if (username.Equals("") == true || password.Equals("") == true ) {

                LoginScreen form = new LoginScreen();
                this.Hide();
                form.ShowDialog();
                this.Show();

                username = form.usernameBox.Text;
                password = form.passwordBox.Password;

            }

        }

        public static bool validateComputerName(string name) {

            /*if (name.Length != 10) {
                return false;
            }*/

            return true;

        }

        public static bool pingHost(string host) {

            Ping connectionResponse = new Ping();

            try {

                PingReply response = connectionResponse.Send(host);
                return (response.Status == IPStatus.Success);

            }
            catch {

                return false;

            }
            finally {

                if (connectionResponse != null) {
                    connectionResponse.Dispose();
                }

            }


        }

        private void submitNameButton_Click(object sender, RoutedEventArgs e) {

            //clear out the textboxes
            outputTextbox.Text = "";
            diskInfoTextbox.Text = "";
            computerInfoTextbox.Text = "";
            logTextbox.Text = "";

            //computer name, domain, username, and password
            //used to actually connect to a machine and authenticate
            string computerName = computerNameTextbox.Text;
            string domain = "net.ucf.edu";

/*          string username = usernameTextbox.Text;
            string password = passwordTextbox.Password;*/

            if (validateComputerName(computerName) == true && pingHost(computerName) == true) {

                logTextbox.AppendText($"Ping Successful for {computerName}\n");

                //computerNameValidLabel.Content = "";

                SecureString securePassword = new SecureString(); //change the password to a secure string
                foreach (char c in password) {
                    securePassword.AppendChar(c);
                }

                CimCredential Credentials = new CimCredential(PasswordAuthenticationMechanism.Default, domain, username, securePassword);

                WSManSessionOptions SessionOptions = new WSManSessionOptions();
                SessionOptions.AddDestinationCredentials(Credentials);

                CimSession Session = CimSession.Create(computerName, SessionOptions);

                //var allVolumes = Session.QueryInstances(@"root\cimv2", "WQL", "SELECT * FROM Win32_Volume");

                //========================================== USERNAME ===================================================

                var allUserNames = Session.QueryInstances(@"root\cimv2", "WQL", "SELECT * FROM Win32_ComputerSystem");

                foreach (CimInstance o in allUserNames) {

                    userLoggedInLablel1.Content = "User Logged In: " + Convert.ToString(o.CimInstanceProperties["Username"].Value);
                    logTextbox.AppendText($"{userLoggedInLablel1.Content}\n");

                }

                //========================================== RAM ===================================================

                var allComputerSystem = Session.QueryInstances(@"root\cimv2", "WQL", "SELECT * FROM Win32_OperatingSystem");

                double freeRam = 0;
                double totalRam = 0;
                double usedRam = 0;
                double ramPercentage = 0;

                foreach (CimInstance o in allComputerSystem) {
                    freeRam = Convert.ToDouble(o.CimInstanceProperties["FreePhysicalMemory"].Value) / Math.Pow(2, 20);
                    totalRam = Convert.ToDouble(o.CimInstanceProperties["TotalVisibleMemorySize"].Value) / Math.Pow(2, 20);

                    usedRam = (totalRam - freeRam);

                    freeRam = Math.Round(freeRam, 3);
                    usedRam = Math.Round(usedRam, 3);
                    totalRam = Math.Round(totalRam, 3);

                    logTextbox.AppendText($"Used RAM: {Convert.ToString(usedRam)} GB\n");
                    logTextbox.AppendText($"Total RAM: {Convert.ToString(totalRam)} GB\n");

                }

                ramPercentage = (usedRam / totalRam) * 100;
                ramPercentage = Math.Round(ramPercentage, 3);

                liveRamProgressBar1.Value = ramPercentage;
                ramComputerLabel1.Content = $"RAM: {usedRam} GB / {totalRam} GB | {ramPercentage}%";

                //========================================== PROGRAMS ===================================================

                int totalPrograms = 0;

                if (programCheckbox.IsChecked == true) {

                    logTextbox.AppendText("Importing Programs\n");
                    var allPrograms = Session.QueryInstances(@"root\cimv2", "WQL", "SELECT * FROM Win32_Product");
                    foreach (CimInstance program in allPrograms) {
                        string text = Convert.ToString(program.CimInstanceProperties["Name"].Value);
                        outputTextbox.AppendText(text + "\n");
                        totalPrograms++;
                    }

                }

                totalLabel.Content = $"Total Programs: {totalPrograms}";
                logTextbox.AppendText($"Total Programs: {totalPrograms}\n");
                //========================================== DISK INFO ===================================================
                if (diskInfoCheckbox.IsChecked == true) {

                    logTextbox.AppendText("Scanning Disk Drives\n");
                    var allPDisks = Session.QueryInstances(@"root\cimv2", "WQL", "SELECT * FROM Win32_DiskDrive");
                    foreach (CimInstance pDisk in allPDisks) {

                        string diskName = Convert.ToString(pDisk.CimInstanceProperties["Name"].Value);
                        string modelName = Convert.ToString(pDisk.CimInstanceProperties["Model"].Value);
                        string status = Convert.ToString(pDisk.CimInstanceProperties["Status"].Value);
                        string serialNumber = Convert.ToString(pDisk.CimInstanceProperties["SerialNumber"].Value);
                        
                        double size = Convert.ToDouble(pDisk.CimInstanceProperties["Size"].Value) / Math.Pow(2, 20);

                        diskInfoTextbox.AppendText($"Name: {diskName}\n" +
                            $"Model: {modelName}\n" +
                            $"Status: {status}\n" +
                            $"Serial Number: {serialNumber}\n" +
                            $"Size: {Math.Round(size, 3)} GB");
                    
                    }
                }
                //======================================= COMPUTER INFO ===================================================
                if (computerInfoCheckbox.IsChecked == true) {
                    var allOS = Session.QueryInstances(@"root\cimv2", "WQL", "SELECT * FROM Win32_OperatingSystem");
                    //var allComputerSystem = Session.QueryInstances(@"root\cimv2", "WQL", "SELECT * FROM Win32_OperatingSystem");

                    //============== allOS ==============
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

                    }

                    //============== allComputerSystem ==============
                    foreach (CimInstance comp in allComputerSystem) {
                        //======= User Logged In =======
                        string userLoggedIn = "";
                        try { Convert.ToString(comp.CimInstanceProperties["UserName"].Value); } catch { userLoggedIn = "None"; }


                        computerInfoTextbox.AppendText($"User Logged In: {userLoggedIn}\n");

                    }

                }

                if (processCheckbox.IsChecked == true) {

                    var allProcessList = Session.QueryInstances(@"root\cimv2", "WQL", "SELECT * FROM Win32_Process");
                    ArrayList processArrayList = new ArrayList();

                    foreach (CimInstance o in allProcessList) {

                        processArrayList.Add(Convert.ToString(o.CimInstanceProperties["Name"].Value));
                        if (Convert.ToString(o.CimInstanceProperties["Name"].Value).Equals("svchost.exe") == false) {
                            processComputerTextbox1.AppendText(Convert.ToString(o.CimInstanceProperties["Name"].Value) + "\n");
                        }

                    }

                    processCountLabel1.Content = $"Total Processes: {processArrayList.Count}";
                    logTextbox.AppendText($"Process Count: {processArrayList.Count}");

                }


            }
            else {

                //computerNameValidLabel.Content = "Computer Name Invalid";
                //computerNameValidLabel.Foreground = Brushes.Red;

            }
        }

        private void filterProgramsTextbox_TextChanged(object sender, TextChangedEventArgs e) {

            /*            string[] lines = outputTextbox.Text.Split(new[] { Environment.NewLine }, StringSplitOptions.None);
                        //outputTextbox.Text = "";

                        foreach (string line in lines) {
                            if (line.Contains(filterProgramsTextbox.Text)) {
                                outputTextbox.AppendText(line);
                            }
                        }
                        */
        }

        private void filterButtonSubmit_Click(object sender, RoutedEventArgs e) {

            /*            string[] lines = outputTextbox.Text.Split(new[] { Environment.NewLine }, StringSplitOptions.None);
                        outputTextbox.Text = "";

                        foreach (string line in lines) {
                            if (line.Contains(filterProgramsTextbox.Text)) {
                                outputTextbox.AppendText(line);
                            }
                        }*/

        }

        private void runComputerButton1_Click(object sender, RoutedEventArgs e) {

            /*string computerName = computerMonitoringTextbox1.Text;
            string domain = "net.ucf.edu";
            
*//*            string username = usernameTextbox.Text;
            string password = passwordTextbox.Password;*//*

            double refreshRate = 2;

            if (runComputerButton1.Content.Equals("Connect")) {

                if (validateComputerName(computerName) == true) {

                    computerMonitorLabel1.Content = $"Pinging {computerName}";
                    computerMonitorLabel1.Foreground = Brushes.Orange;

                    bool ping = pingHost(computerName);

                    if (ping == true) {

                        runComputerButton1.Content = "Disconnect";

                        computerMonitorLabel1.Content = $"Connection to {computerName} successful";
                        computerMonitorLabel1.Foreground = Brushes.Green;

                        logTextbox.AppendText($"Connected to {computerName}\n");

                        //============== CIM Instance Query Setup ==============

                        SecureString securePassword = new SecureString(); //change the password to a secure string
                        foreach (char c in password) {
                            securePassword.AppendChar(c);
                        }

                        CimCredential Credentials = new CimCredential(PasswordAuthenticationMechanism.Default, domain, username, securePassword);

                        WSManSessionOptions SessionOptions = new WSManSessionOptions();
                        SessionOptions.AddDestinationCredentials(Credentials);

                        CimSession Session = CimSession.Create(computerName, SessionOptions);

                        var allComputerSystem = Session.QueryInstances(@"root\cimv2", "WQL", "SELECT * FROM Win32_OperatingSystem");

                        //============== Username ==============

                        var allUserNames = Session.QueryInstances(@"root\cimv2", "WQL", "SELECT * FROM Win32_ComputerSystem");

                        foreach (CimInstance o in allUserNames) {

                            userLoggedInLablel1.Content = "User Logged In: " + Convert.ToString(o.CimInstanceProperties["Username"].Value);
                            logTextbox.AppendText($"{userLoggedInLablel1.Content}\n");

                        }

                        //============== RAM ==============

                        double freeRam = 0;
                        double totalRam = 0;
                        double usedRam = 0;
                        double ramPercentage = 0;

                        foreach (CimInstance o in allComputerSystem) {
                            freeRam = Convert.ToDouble(o.CimInstanceProperties["FreePhysicalMemory"].Value) / Math.Pow(2, 20);
                            totalRam = Convert.ToDouble(o.CimInstanceProperties["TotalVisibleMemorySize"].Value) / Math.Pow(2, 20);

                            usedRam = (totalRam - freeRam);

                            freeRam = Math.Round(freeRam, 3);
                            usedRam = Math.Round(usedRam, 3);
                            totalRam = Math.Round(totalRam, 3);

                            logTextbox.AppendText($"Used RAM: {Convert.ToString(usedRam)} GB\n");
                            logTextbox.AppendText($"Total RAM: {Convert.ToString(totalRam)} GB\n");

                        }

                        ramPercentage = (usedRam / totalRam) * 100;
                        ramPercentage = Math.Round(ramPercentage, 3);

                        liveRamProgressBar1.Value = ramPercentage;
                        ramComputerLabel1.Content = $"{usedRam} / {totalRam} GB | {ramPercentage}%";

                        //============== Processes ==============

                        logTextbox.AppendText("Scanning Processes...\n");

                        var allProcessList = Session.QueryInstances(@"root\cimv2", "WQL", "SELECT * FROM Win32_Process");
                        ArrayList processArrayList = new ArrayList();

                        foreach (CimInstance o in allProcessList) {

                            processArrayList.Add(Convert.ToString(o.CimInstanceProperties["Name"].Value));
                            if (Convert.ToString(o.CimInstanceProperties["Name"].Value).Equals("svchost.exe") == false) {
                                processComputerTextbox1.AppendText(Convert.ToString(o.CimInstanceProperties["Name"].Value) + "\n");
                            }

                        }

                        processCountLabel1.Content = $"Total Processes: {processArrayList.Count}";
                        logTextbox.AppendText($"Process Count: {processArrayList.Count}");


                    }
                    else { // ======= If ping has failed =======

                        computerMonitorLabel1.Content = $"Could not Connect to {computerName}";
                        computerMonitorLabel1.Foreground = Brushes.Red;

                    }


                }
                else { //======= If computer name was not valid =======

                    computerMonitorLabel1.Content = "Computer Name Invalid";
                    computerMonitorLabel1.Foreground = Brushes.Red;

                }

            }
            else { //======= If button already says disconnect =======

            }*/
        }

        private void TabControl_SelectionChanged(object sender, SelectionChangedEventArgs e) {

        }
    }
}
