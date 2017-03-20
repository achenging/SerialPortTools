using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Globalization;

namespace SerialPortTools
{
    public partial class MainWindow : Window
    {
        private CommProfile _commProfile;
        private SerialPort _serialPort = new SerialPort();
        private SerialPortWatcher _serialPortWatcher;


        private UpdateSerialPostListEventHandler UpdateSerialPortListEvent;
        private void initCore()
        {
            initProfile();
            initSerialPort();
        }


        private void initProfile()
        {
            if (!hasProfile())
            {
                _commProfile = new CommProfile();
                _commProfile.Baudrates = new ObservableCollection<string> { "110", "300", "1200", "2400", "4800", "9600", "14400", "19200", "38400", "57600", "115200", "128000", "256000" };
                _commProfile.VerifyBit = new ObservableCollection<string> { "无", "奇校验", "偶校验", "标记", "空格" };
                _commProfile.DataBit = new ObservableCollection<string> { "8位", "7位", "6位", "5位" };
                _commProfile.StopBit = new ObservableCollection<string> { "无", "1位", "1.5位", "2位" };
                _commProfile.Encoding = new ObservableCollection<string> { "DEFAULT", "UTF-8", "UNICODE", "GB2312", "ASCII" };
                _commProfile.BaudrateSelected = 5;
                _commProfile.VerifyBitSelected = 0;
                _commProfile.DataBitSelected = 0;
                _commProfile.StopBitSelected = 1;
                _commProfile.EncodingSelected = 1;
                _commProfile.IsReceivedToHex = false;
                _commProfile.IsSendToHex = false;
                _commProfile.IsAddNewLine = true;
                _commProfile.TimerInterval = 1000;
                _commProfile.NewLineType = 1;
                _commProfile.Commands = initCommands();
                var json = JsonConvert.SerializeObject(_commProfile);
                writeProfile(json);
            }
            else
            {
                _commProfile = JsonConvert.DeserializeObject<CommProfile>(readProfile());
            }

            dataContext.DataContext = _commProfile;


            UpdateSerialPortListEvent = (obj, args) =>
            {
                var changePortName = args.SerialPortName;

                if (btnOpenSerialPort.Content.Equals("关闭串口"))
                {
                    if (_serialPort.PortName.Equals(changePortName))
                    {
                        ClosePort();
                    }
                }

                if (_serialPortWatcher.ComPorts.Count == 1)
                {
                    comboBoxComm.SelectedIndex = 0;
                }
            };
            _serialPortWatcher = new SerialPortWatcher(UpdateSerialPortListEvent);
        }

        private ObservableCollection<Command> initCommands()
        {
            const int length = 10;
            var commands = new ObservableCollection<Command>();
            for (var i = 0; i < length; i++)
            {
                Command command = new Command() { CommandIsHex = false, CommandData = string.Empty, Comment = string.Empty };
                commands.Add(command);
            }

            return commands;

        }

        private void initSerialPort()
        {
            var serialPortNames = _serialPortWatcher.ComPorts;
            comboBoxComm.DataContext = serialPortNames;
            comboBoxComm.SelectedIndex = 0;
            tbFindCommNumber.Text = "欢迎使用串口助手";
        }

        private bool hasProfile()
        {
            var currentDirectory = Directory.GetCurrentDirectory();
            var profilePath = currentDirectory + "\\comm.ini";
            if (File.Exists(profilePath))
            {
                return true;
            }
            return false;
        }

        private void writeProfile(string profile)
        {
            var currentDirectory = Directory.GetCurrentDirectory();
            var profilePath = currentDirectory + "\\comm.ini";
            File.WriteAllText(profilePath, profile);
        }

        private string readProfile()
        {
            var currentDirectory = Directory.GetCurrentDirectory();
            var profilePath = currentDirectory + "\\comm.ini";
            return File.ReadAllText(profilePath);
        }


        private Parity GetParity()
        {
            Parity parity = Parity.None;
            var selectedIndex = comboBoxVerifyBit.SelectedIndex;
            switch (selectedIndex)
            {
                case 0:
                    parity = Parity.None;
                    break;
                case 1:
                    parity = Parity.Odd;
                    break;
                case 2:
                    parity = Parity.Even;
                    break;
                case 3:
                    parity = Parity.Space;
                    break;
                case 4:
                    parity = Parity.Mark;
                    break;


            }
            return parity;
        }

        private int GetDataBit()
        {
            var dataBit = 8;
            int selectedIndex = comboBoxDataBit.SelectedIndex;
            switch (selectedIndex)
            {
                case 0:
                    dataBit = 8;
                    break;
                case 1:
                    dataBit = 7;
                    break;
                case 2:
                    dataBit = 6;
                    break;
                case 3:
                    dataBit = 5;
                    break;
            }

            return dataBit;
        }

        private StopBits GetStopBit()
        {
            var stopBit = StopBits.None;
            int selectedIndex = comboBoxStopBit.SelectedIndex;
            switch (selectedIndex)
            {
                case 0:
                    stopBit = StopBits.None;
                    break;
                case 1:
                    stopBit = StopBits.One;
                    break;
                case 2:
                    stopBit = StopBits.OnePointFive;
                    break;
                case 3:
                    stopBit = StopBits.Two;
                    break;
            }

            return stopBit;
        }

        private Encoding GetEncoding()
        {
            Encoding enc = Encoding.Default;
            int selectedIndex = comboBoxEncoding.SelectedIndex;
            switch (selectedIndex)
            {
                case 0:
                    enc = Encoding.Default;
                    break;
                case 1:
                    enc = Encoding.UTF8;
                    break;
                case 2:
                    enc = Encoding.Unicode;
                    break;
                case 3:
                    enc = Encoding.GetEncoding("GB2312"); ;
                    break;
                case 4:
                    enc = Encoding.ASCII;
                    break;
            }
            return enc;
        }

        private void SetComboxFocusable(bool focusable)
        {
            comboBoxComm.IsEnabled = focusable;
            comboBoxBaudrate.IsEnabled = focusable;
            comboBoxVerifyBit.IsEnabled = focusable;
            comboBoxDataBit.IsEnabled = focusable;
            comboBoxStopBit.IsEnabled = focusable;
            comboBoxEncoding.IsEnabled = focusable;
        }


    }

}



