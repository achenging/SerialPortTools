
using System.Windows;

namespace SerialPortTools
{
    /// <summary>
    /// Window1.xaml 的交互逻辑
    /// </summary>
    public partial class LineSettingWindow : Window
    {

        public delegate void IntentHandler(object sender, int selected);

        public event IntentHandler IntentEvent;

        public int DefaultSelected { set; get; }

        public LineSettingWindow(int index,double left, double top)
        {
            InitializeComponent();
            DefaultSelected = index;
            Checked(DefaultSelected);
            Left = left;
            Top = top;
        }

        private void Checked(int index)
        {
            switch(index)
            {
                case 0:
                    rbCRLF.IsChecked = true;
                    break;
                case 1:
                    rbCR.IsChecked = true;
                    break;
                case 2:
                    rbLF.IsChecked = true;
                    break;
            }
        }

        private void NewLineTypeRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            if (rbCRLF.IsChecked == true)
            {
                DefaultSelected = 0;
            }
            else if (rbCR.IsChecked == true)
            {
                DefaultSelected = 1;
            }
            else if (rbLF.IsChecked == true)
            {
                DefaultSelected = 2;
            }
        }


        private void LineSettingsWindow_Closed(object sender, System.EventArgs e)
        {

            IntentEvent(this, DefaultSelected);
        }
    }
}
