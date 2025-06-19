using Hrms.DesktopForm.Settings;
using Hrms.DesktopForm.Utilities;
using Npgsql;
using System.Data;
using Newtonsoft.Json;
using Hrms.Common.Data;
using Microsoft.EntityFrameworkCore;
using System.Windows.Forms;
using Hrms.Common.Models;

namespace Hrms.DesktopForm
{
    public partial class Form1 : Form
    {
        private NpgsqlConnection connection;
        private string appDataFolder;
        private string devicePath;
        private string databasePath;
        private List<Device> addedDevices;
        private DataContext _context;

        public Form1()
        {
            InitializeComponent();

            label2.Hide();
            deviceCheckBoxList.Hide();
            button1.Hide();

            appDataFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "Attendance");
            devicePath = Path.Combine(appDataFolder, "devices.config.json");
            databasePath = Path.Combine(appDataFolder, "database.config.json");

            try
            {
                using (StreamReader sr = new StreamReader(Path.Combine(appDataFolder, "devices.config.json")))
                {
                    string json = sr.ReadToEnd();
                    addedDevices = JsonConvert.DeserializeObject<List<Device>>(json);
                }
            }
            catch (Exception ex)
            {

            }


        }

        // Connect Database and list devices.
        private async void button1_Click(object sender, EventArgs e)
        {
            if (testDatabaseConnection.Text == "Connect")
            {
                string conString = connectionString.Text;

                if (string.IsNullOrEmpty(conString))
                {
                    connectionStatus.ForeColor = Color.Red;
                    connectionStatus.Text = "Please input connection string!";
                }
                else
                {
                    connection = new NpgsqlConnection(conString);

                    try
                    {
                        await connection.OpenAsync();
                        connectionStatus.ForeColor = Color.LightGreen;
                        connectionStatus.Text = "Connected!";
                    }
                    catch (Exception ex)
                    {
                        connectionStatus.ForeColor = Color.Red;
                        connectionStatus.Text = "Connection Failed!";

                        label2.Hide();
                        deviceCheckBoxList.Hide();
                    }
                }

                if (connection.State == ConnectionState.Open)
                {
                    testDatabaseConnection.Text = "Disconnect";
                    testDatabaseConnection.BackColor = Color.IndianRed;

                    label2.Show();
                    deviceCheckBoxList.Show();
                    button1.Show();

                    DbContextOptionsBuilder<DataContext> optionsBuilder = new DbContextOptionsBuilder<DataContext>();
                    optionsBuilder.UseNpgsql(connection);
                    _context = new DataContext(optionsBuilder.Options);

                    var deviceSettings = await _context.DeviceSettings.ToListAsync();

                    if (deviceSettings.Count == 0)
                    {

                    }
                    else
                    {
                        foreach (var deviceSetting in deviceSettings)
                        {
                            CheckBoxListItem item = new CheckBoxListItem(deviceSetting.DeviceModel, deviceSetting, true);

                            if (addedDevices is not null)
                            {
                                if (addedDevices.Any(x => int.Parse(AesOperation.DecryptString(x.DeviceId.ToString())) == deviceSetting.Id))
                                {
                                    deviceCheckBoxList.Items.Add(item, true);
                                    continue;
                                }
                            }

                            deviceCheckBoxList.Items.Add(item, false);
                        }
                    }
                };
            }
            else
            {
                connection.Close();
                connectionStatus.Text = "Disconnected";
                connectionStatus.ForeColor = Color.Red;

                testDatabaseConnection.Text = "Connect";
                testDatabaseConnection.BackColor = Color.LightGreen;

                label2.Hide();
                deviceCheckBoxList.Hide();
                button1.Hide();
            }

        }

        // Set Database and Devices into the file.
        private void button1_Click_1(object sender, EventArgs e)
        {
            string conString = connectionString.Text;

            if (string.IsNullOrEmpty(conString))
            {
                deviceStatus.ForeColor = Color.Red;
                deviceStatus.Text = "Please input connection string!";
                return;
            }
            else
            {
                connection = new NpgsqlConnection(conString);

                try
                {
                    connection.Open();
                }
                catch (Exception ex)
                {
                    deviceStatus.ForeColor = Color.Red;
                    deviceStatus.Text = "Connection failed with database!";
                    return;
                }
            }

            List<Device> devices = new List<Device>();

            foreach (object item in deviceCheckBoxList.CheckedItems)
            {
                if (item is CheckBoxListItem checkBoxItem)
                {
                    DeviceSetting selectedDevice = checkBoxItem.Value;

                    if (!UniversalStatic.PingTheDevice(selectedDevice.DeviceIp))
                    {
                        deviceStatus.Text = $"Could not ping the device {selectedDevice.DeviceModel} at ip {selectedDevice.DeviceIp}.";
                        deviceStatus.ForeColor = Color.Red;
                        return;
                    }

                    devices.Add(new Device
                    {
                        DeviceId = AesOperation.EncryptString(selectedDevice.Id.ToString()),
                        IpAddress = AesOperation.EncryptString(selectedDevice.DeviceIp),
                        PortNumber = selectedDevice.PortNumber
                    });
                }
            }

            try
            {
                if (!Directory.Exists(appDataFolder))
                {
                    // Create the directory
                    Directory.CreateDirectory(appDataFolder);
                }

                DatabaseSetting database = new()
                {
                    ConnectionString = AesOperation.EncryptString(conString).ToString()
                };

                string databaseJsonString = JsonConvert.SerializeObject(database);
                string deviceJsonString = JsonConvert.SerializeObject(devices);

                if (File.Exists(databasePath))
                {
                    File.Delete(databasePath);
                }

                if (File.Exists(devicePath))
                {
                    File.Delete(devicePath);
                }

                File.WriteAllText(databasePath, databaseJsonString);
                File.WriteAllText(devicePath, deviceJsonString);

                deviceStatus.ForeColor = Color.Green;
                deviceStatus.Text = "Successfully set database and devices.";
            }
            catch (Exception ex)
            {
                deviceStatus.ForeColor = Color.Red;
                deviceStatus.Text = ex.Message;
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {

        }

        private void textBox1_TextChanged_1(object sender, EventArgs e)
        {

        }

        private void label3_Click_1(object sender, EventArgs e)
        {

        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void label2_Click(object sender, EventArgs e)
        {

        }

        private void fontDialog1_Apply(object sender, EventArgs e)
        {

        }

        private void label3_Click(object sender, EventArgs e)
        {

        }

        private void checkedListBox1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }
    }

    public class CheckBoxListItem
    {
        public string Name { get; set; }
        public DeviceSetting Value { get; set; }
        public bool Checked { get; set; }

        public CheckBoxListItem(string name, DeviceSetting value, bool isChecked)
        {
            Name = name;
            Value = value;
            Checked = isChecked;
        }

        public override string ToString()
        {
            return Name;
        }
    }
}