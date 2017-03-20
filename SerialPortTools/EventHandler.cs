using Microsoft.Win32;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Management;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;

namespace SerialPortTools
{
    public partial class MainWindow : Window
    {
        private System.Timers.Timer timer;

        private ElapsedEventHandler ElapsedEventDelegate;



        private int _readCnt;
        private int _writeCnt;

        private byte[] buffer = new byte[2048];


        #region 程序加载与关闭

        /// <summary>
        /// 程序窗口关闭
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnCloseWindow(object sender, EventArgs e)
        {
            if (_serialPort.IsOpen)
            {
                ClosePort();
            }

            writeProfile(JsonConvert.SerializeObject(_commProfile));
            if (_serialPortWatcher != null)
            {
                _serialPortWatcher.Dispose();
            }
        }
        #endregion


        #region 端口 打开 关闭 读写
        private void btnOpenSerialPort_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if (_serialPort.IsOpen)
            {
                ClosePort();
            }
            else
            {
                try
                {
                    var watcher = new Stopwatch();
                    watcher.Start();
                    var portName = comboBoxComm.Text;

                    _serialPort.PortName = portName;
                    _serialPort.BaudRate = int.Parse(comboBoxBaudrate.Text);
                    _serialPort.Parity = GetParity();
                    _serialPort.StopBits = GetStopBit();
                    _serialPort.DataBits = GetDataBit();
                    _serialPort.Encoding = GetEncoding();
                    _serialPort.DataReceived += _serialPort_DataReceived;

                    _serialPort.Open();

                    button.Content = "关闭串口";
                    SetComboxFocusable(false);
                }
                catch (IOException ex)
                {
                    ClosePort();
                    MessageBox.Show(string.Format("{0}", ex.Message));
                }
                catch (UnauthorizedAccessException ex)
                {
                    ClosePort();
                    MessageBox.Show(string.Format("{0}", ex.Message));
                }
                catch (ArgumentOutOfRangeException ex)
                {
                    ClosePort();
                    MessageBox.Show(string.Format("{0}", ex.Message));
                }
                catch (InvalidOperationException ex)
                {
                    ClosePort();
                    MessageBox.Show(string.Format("{0}", ex.Message));
                }
                catch (ArgumentException ex)
                {
                    ClosePort();
                    MessageBox.Show(string.Format("{0}", ex.Message));
                }
                catch (Exception ex)
                {
                    button.Content = "打开串口";
                    SetComboxFocusable(true);
                    MessageBox.Show(string.Format("打开串口的时候出现了错误 {0}", ex.ToString()));
                }
            }
        }

        private void _serialPort_ErrorReceived(object sender, SerialErrorReceivedEventArgs e)
        {
        }

        private void _serialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            Thread.Sleep(100);
            if (!_serialPort.IsOpen)
            {
                return;
            }
            int len = _serialPort.Read(buffer, 0, 2048);
            var readText = _serialPort.Encoding.GetString(buffer, 0, len);



            Dispatcher.Invoke(new Action(() =>
            {
                if (rbHexReceived.IsChecked == true)
                {
                    tbReceivedData.Text += ConvertToHex(readText, GetEncoding());
                }
                else
                {
                    tbReceivedData.Text += readText;
                }

                tbReceivedData.ScrollToEnd();

                _readCnt += len;
                UpdateRxDTxD();
            }));
        }

        private void btnSendData_Click(object sender, RoutedEventArgs e)
        {
            if (_serialPort.IsOpen)
            {
                Send(GetSendData());
            }
        }
        private void ClosePort()
        {
            StopTimer();
            if (_serialPort != null)
            {
                _serialPort.DataReceived -= _serialPort_DataReceived;
                _serialPort.Close();
            }
            btnOpenSerialPort.Content = "打开串口";
            SetComboxFocusable(true);
        }

        private void btnMoreEndLineClick(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;

            var lineSettingWindow = new LineSettingWindow(_commProfile.NewLineType, Left + 130, Height * 9 / 11);
            lineSettingWindow.IntentEvent += (s, args) =>
            {
                _commProfile.NewLineType = args;
            };
            lineSettingWindow.ShowDialog();
        }

        private void checkBox_TimerSend(object sender, RoutedEventArgs e)
        {
            CheckBox cbTimerAutoSend = sender as CheckBox;

            if (!_serialPort.IsOpen && cbTimerAutoSend.IsChecked == true)
            {
                MessageBox.Show("还没有打开串口的！", "警告！", MessageBoxButton.OK, MessageBoxImage.Warning);
                cbTimerAutoSend.IsChecked = false;
                return;
            }

            if (cbTimerAutoSend.IsChecked == true && cbTimerAutoSend.Tag.ToString().Equals("false"))
            {
                cbTimerAutoSend.Tag = "true";
                StartTimer();
            }
            else if (cbTimerAutoSend.IsChecked == false && cbTimerAutoSend.Tag.ToString().Equals("true"))
            {
                cbTimerAutoSend.Tag = "false";
                StopTimer();
            }
        }


        private void StartTimer()
        {
            if (timer == null)
            {
                timer = new System.Timers.Timer();
            }

            if (ElapsedEventDelegate == null)
            {
                ElapsedEventDelegate = (send, args) =>
                {
                    Dispatcher.Invoke(new Action(() =>
                    {
                        Send(GetSendData());
                    }));
                };
            }

            timer.Interval = int.Parse(tbTimer.Text);
            timer.Elapsed += ElapsedEventDelegate;
            timer.Enabled = true;
        }

        private void StopTimer()
        {
            if (timer != null)
            {
                timer.Elapsed -= ElapsedEventDelegate;
                timer.Enabled = false;
            }
            cbTimerSend.IsChecked = false;
        }


        private string GetSendData()
        {
            var sendData = tbSendData.Text;
            if (cbAddNewLine.IsChecked == true)
            {
                if (rbHexSent.IsChecked == true)
                    sendData += " 0d 0a";
                else
                    sendData += "\r\n";
            }
            return sendData;

        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        private void Send(string sendData)
        {

            if (_serialPort.IsOpen)
            {
                byte[] data;
                if (rbHexSent.IsChecked == true)
                {
                    data = ConvertToHexBytes(sendData, _serialPort.Encoding);
                }
                else
                {
                    data = _serialPort.Encoding.GetBytes(sendData);
                }
                _serialPort.Write(data, 0, data.Length);

                _writeCnt += data.Length;
                UpdateRxDTxD();
            }
            else
            {
                MessageBox.Show("串口没有打开的!", "警告", MessageBoxButton.OK, MessageBoxImage.Warning);
            }

        }

        private void UpdateRxDTxD()
        {
            tbFindCommNumber.Text = string.Format("发送(字节数):{0}       接收(字节数):{1}", _writeCnt, _readCnt);
        }
        private void tbSend_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            var textBox = sender as TextBox;
            _commProfile.LastSend = textBox.Text;
        }


        #endregion

        #region 接收区域操作
        private void btnCopyReceivedData(object sender, RoutedEventArgs e)
        {
            var text = tbReceivedData.Text;
            if (!string.IsNullOrEmpty(text))
            {
                Clipboard.SetText(text);
            }
        }


        private void btnClearReceivedData(object sender, RoutedEventArgs e)
        {
            tbReceivedData.Text = string.Empty;
        }


        private void btnStopReceiveData(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if (button.Content.Equals("暂停显示"))
            {
                _serialPort.DataReceived -= _serialPort_DataReceived;
                button.Content = "开始显示";
            }
            else
            {
                _serialPort.DataReceived += _serialPort_DataReceived;
                button.Content = "暂停显示";
            }
        }

        private void btnSaveReceivedDataToFile(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveDialog = new SaveFileDialog();
            saveDialog.OverwritePrompt = true;
            saveDialog.DefaultExt = ".txt";
            saveDialog.Filter = "文本文档|*.txt|所有类型(*.*)|*";
            saveDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            if (saveDialog.ShowDialog() == true)
            {
                SaveFileAsync(saveDialog.FileName, tbReceivedData.Text);
            }
        }

        private void SaveFileAsync(string filePath, string fileContent)
        {
            var saveFileName = filePath;
            var text = tbReceivedData.Text;
            Task.Factory.StartNew(new Action(() =>
            {
                File.WriteAllText(filePath, fileContent);
            }));
        }
        #endregion

        #region 发送区域操作
        private void btnCopySendData(object sender, RoutedEventArgs e)
        {
            var text = tbSendData.Text;
            if (!string.IsNullOrEmpty(text))
            {
                Clipboard.SetText(text);
            }
        }

        private void btnClearSendData(object sender, RoutedEventArgs e)
        {
            tbSendData.Text = string.Empty;
        }

        private void btnClearCalcultor(object sender, RoutedEventArgs e)
        {
            _readCnt = 0;
            _writeCnt = 0;
            UpdateRxDTxD();
        }

        private void btnLoadDataFromFile(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog();
            openFileDialog.DefaultExt = ".txt";
            openFileDialog.Filter = "文本文档|*.txt|所有类型(*.*)|*";
            openFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            if (openFileDialog.ShowDialog() == true)
            {
                OpenFileSync(openFileDialog.FileName);
            }

        }

        private void OpenFileSync(string filePath)
        {
            var saveFileName = filePath;
            Task.Factory.StartNew(() =>
            {
                var readText = File.ReadAllText(filePath);
                Dispatcher.Invoke(new Action(() => { tbSendData.Text = readText; }));
            });
        }
        #endregion

        #region 发送接收格式转换
        private void ReceivedFormatRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            var rb = sender as RadioButton;
            rb.IsChecked = true;
            switch (rb.Tag.ToString())
            {
                case "char":
                    tbReceivedData.Text = ConvertToText(tbReceivedData.Text, GetEncoding());
                    break;
                case "hex":
                    tbReceivedData.Text = ConvertToHex(tbReceivedData.Text, GetEncoding());
                    break;
            }



        }

        private void SendFormatRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            RadioButton rb = sender as RadioButton;
            switch (rb.Tag.ToString())
            {
                case "char":
                    try
                    {
                        tbSendData.Text = ConvertToText(tbSendData.Text, GetEncoding());
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("要转换的十六进制有误");
                        rb.IsChecked = false;
                        return;
                    }
                    break;
                case "hex":
                    tbSendData.Text = ConvertToHex(tbSendData.Text, GetEncoding());
                    break;
            }
        }

        private string ConvertToText(string text, Encoding encoding)
        {
            string result = "";
            byte[] buf = null;
            try
            {
                buf = ConvertToHexBytes(text, encoding);
            }
            catch (Exception ex)
            {
                throw ex;
            }

            if (buf != null)
            {
                result = encoding.GetString(buf);
            }
            return result;
        }

        private string ConvertToHex(string text, Encoding encoding)
        {
            if (string.IsNullOrEmpty(text))
                return string.Empty;

            var sb = new StringBuilder();
            var byteData = encoding.GetBytes(text);
            foreach (var item in byteData)
            {
                var hex = Convert.ToString(item, 16);
                sb.Append(hex.Length == 1 ? "0" + hex : hex);
                sb.Append(' ');
            }
            return sb.ToString();
        }

        private byte[] ConvertToHexBytes(string text, Encoding encoding)
        {
            byte[] buf = null;
            var hexs = text.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            if (hexs.Length > 0)
            {
                buf = new byte[hexs.Length];
            }
            for (var i = 0; i < hexs.Length; i++)
            {
                try
                {
                    buf[i] = Convert.ToByte(hexs[i], 16);
                }
                catch (FormatException e)
                {
                    throw e;
                }
            }
            return buf;
        }
        #endregion

        #region
        private void OnCommandHexCheckClick(object sender, RoutedEventArgs e)
        {
            var checkBox = sender as CheckBox;
            int index = lbCommand.Items.IndexOf(checkBox.DataContext);
            var command = lbCommand.Items.GetItemAt(index) as Command;
            if (command != null)
            {
                var isHex = command.CommandIsHex;
                var text = command.CommandData;
                if (isHex)
                {
                    _commProfile.Commands[index].CommandIsHex = isHex;
                    _commProfile.Commands[index].CommandData = ConvertToHex(text, GetEncoding());
                }
                else
                {
                    try
                    {
                        _commProfile.Commands[index].CommandData = ConvertToText(text, GetEncoding());
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("要转换的十六进制有误");
                        checkBox.IsChecked = true;
                        return;
                    }
                    
                }
            }
        }

        private void OnCommandSendItemClick(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            int index = lbCommand.Items.IndexOf(button.DataContext);
            var command = lbCommand.Items.GetItemAt(index) as Command;
            if (command != null)
            {
                var text = command.CommandData;
                var isHex = command.CommandIsHex;
                if (isHex)
                {
                    _commProfile.Commands[index].CommandData = ConvertToHex(text, GetEncoding());
                }

                var sendData = _commProfile.Commands[index].CommandData;
                tbSendData.Text = sendData;
                if (cbAddNewLine.IsChecked == true)
                {
                    if (rbHexSent.IsChecked == true)
                        sendData += " 0d 0a";
                    else
                        sendData += "\r\n";
                }
                Send(sendData);
            }
        }


        private void btnExpandClick(object sender, RoutedEventArgs e)
        {
            var expandButton = sender as Button;
            if (">>>".Equals(expandButton.Content))
            {
                lbCommand.Visibility = Visibility.Visible;
                expandButton.Content = "<<<";
                Width = 918.5;
            }
            else
            {
                lbCommand.Visibility = Visibility.Collapsed;
                expandButton.Content = ">>>";
                Width = 580;
            }
        }
        #endregion
    }


    public delegate void UpdateSerialPostListEventHandler(object sender, SerialPortNameEventArgs args);
    /// <summary>
    /// Make sure you create this watcher in the UI thread if you are using the com port list in the UI
    /// </summary>
    [Export]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public sealed class SerialPortWatcher : IDisposable
    {

        private ManagementEventWatcher _watcher;
        private TaskScheduler _taskScheduler;

        public ObservableCollection<string> ComPorts { get; private set; }

        public event UpdateSerialPostListEventHandler UpdateSerialPostEvent;

        public SerialPortWatcher(UpdateSerialPostListEventHandler updateSerialPostEvent)
        {
            UpdateSerialPostEvent += updateSerialPostEvent;
            _taskScheduler = TaskScheduler.FromCurrentSynchronizationContext();
            ComPorts = new ObservableCollection<string>(SerialPort.GetPortNames().OrderBy(s => s));

            WqlEventQuery query = new WqlEventQuery("SELECT * FROM Win32_DeviceChangeEvent");

            _watcher = new ManagementEventWatcher(query);
            _watcher.EventArrived += (sender, eventArgs) => CheckForNewPorts(eventArgs);
            _watcher.Start();
        }

        private void CheckForNewPorts(EventArrivedEventArgs args)
        {
            // do it async so it is performed in the UI thread if this class has been created in the UI thread
            Task.Factory.StartNew(CheckForNewPortsAsync, CancellationToken.None, TaskCreationOptions.None, _taskScheduler);
        }

        private void CheckForNewPortsAsync()
        {
            IEnumerable<string> ports = SerialPort.GetPortNames().OrderBy(s => s);

            int length = ComPorts.Count;
            for (var i = 0; i < length; i++)
            {
                var comPort = ComPorts[i];
                if (!ports.Contains(comPort))
                {
                    var args = new SerialPortNameEventArgs();
                    args.SerialPortName = comPort;
                    UpdateSerialPostEvent(this, args);
                    ComPorts.Remove(comPort);
                }
            }

            foreach (var port in ports)
            {
                if (!ComPorts.Contains(port))
                {
                    AddPort(port);
                }
            }
        }

        private void AddPort(string port)
        {
            if (ComPorts.Count == 0)
            {
                ComPorts.Add(port);
                var args = new SerialPortNameEventArgs();
                args.SerialPortName = port;
                UpdateSerialPostEvent(this, args);
                return;
            }

            for (int j = 0; j < ComPorts.Count; j++)
            {
                if (port.CompareTo(ComPorts[j]) < 0)
                {
                    ComPorts.Insert(j, port);
                    var args = new SerialPortNameEventArgs();
                    args.SerialPortName = port;
                    UpdateSerialPostEvent(this, args);
                    break;
                }
            }

        }

        #region IDisposable Members

        public void Dispose()
        {
            _watcher.Stop();
            _watcher.Dispose();
        }

        #endregion
    }

    public class SerialPortNameEventArgs : EventArgs
    {
        public string SerialPortName { set; get; }
    }
}
