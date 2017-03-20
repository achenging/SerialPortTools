
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq.Expressions;

namespace SerialPortTools
{
    class CommProfile : INotifyPropertyChanged
    {
        private string _portName;
        private int _baudrateSelected;
        private int _verifyBitSelected;
        private int _dataBitSelected;
        private int _stopBitSelected;
        private int _encodingSelected;
        private bool _isReceivedToHex;
        private bool _isSendToHex;
        private bool _isAddNewLine;
        private int _newLineType;
        private int _timerInterval;
        private bool _isExpand;
        private string _lastSend;

        public ObservableCollection<string> PortNames { set; get; }
        public ObservableCollection<string> Baudrates { set; get; }
        public ObservableCollection<string> VerifyBit { set; get; }
        public ObservableCollection<string> DataBit { set; get; }
        public ObservableCollection<string> StopBit { set; get; }
        public ObservableCollection<string> Encoding { set; get; }
        public ObservableCollection<Command> Commands { set; get; }


        public string PortName
        {
            set
            {
                if (_portName != value)
                {
                    _portName = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("PortName"));
                }
            }
            get { return _portName; }
        }


        public int BaudrateSelected
        {
            set
            {
                if (_baudrateSelected != value)
                {
                    _baudrateSelected = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("BaudrateSelected"));
                }
            }
            get
            {
                return _baudrateSelected;
            }
        }

        public int VerifyBitSelected
        {
            set
            {
                if (_verifyBitSelected != value)
                {
                    _verifyBitSelected = value;

                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("VerifyBitSelected"));
                }
            }
            get
            {
                return _verifyBitSelected;
            }
        }
        public int DataBitSelected
        {
            set
            {
                if (_dataBitSelected != value)
                {
                    _dataBitSelected = value;

                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("DataBitSelected"));
                }
            }

            get
            {
                return _dataBitSelected;
            }

        }
        public int StopBitSelected
        {
            set
            {
                if (_stopBitSelected != value)
                {
                    _stopBitSelected = value;

                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("StopBitSelected"));
                }

            }

            get
            {
                return _stopBitSelected;
            }
        }
        public int EncodingSelected
        {
            set
            {
                if (_encodingSelected != value)
                {
                    _encodingSelected = value;

                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("EncodingSelected"));
                }
            }
            get
            {
                return _encodingSelected;

            }
        }

        public bool IsReceivedToHex
        {
            set
            {
                if (_isReceivedToHex != value)
                {
                    _isReceivedToHex = value;

                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("IsReceivedToHex"));
                }
            }
            get
            {
                return _isReceivedToHex;

            }
        }
        public bool IsSendToHex
        {
            set
            {
                if (_isSendToHex != value)
                {
                    _isSendToHex = value;

                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("IsSendToHex"));
                }
            }
            get
            {
                return _isSendToHex;

            }
        }

        public bool IsAddNewLine
        {
            set
            {
                if (_isAddNewLine != value)
                {
                    _isAddNewLine = value;

                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("IsAddNewLine"));
                }
            }
            get
            {
                return _isAddNewLine;

            }
        }
        public int TimerInterval
        {
            set
            {
                if (_timerInterval != value)
                {
                    _timerInterval = value;

                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("TimerInterval"));
                }
            }
            get
            {
                return _timerInterval;

            }
        }
        public int NewLineType
        {
            set
            {
                if (_newLineType != value)
                {
                    _newLineType = value;

                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("NewLineType"));
                }
            }
            get
            {
                return _newLineType;

            }
        }
        public bool IsExpand
        {
            set
            {
                if (_isExpand != value)
                {
                    _isExpand = value;

                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("IsExpand"));
                }
            }
            get
            {
                return _isExpand;

            }
        }

        public string LastSend
        {
            set
            {
                if (_lastSend != value)
                {
                    _lastSend = value;

                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("LastSend"));
                }
            }
            get
            {
                return _lastSend;

            }
        }


        public event PropertyChangedEventHandler PropertyChanged;
    }

    class Command : INotifyPropertyChanged
    {
        private bool _commandIsHex;
        private string _commandData;
        private string _comment;

        public bool CommandIsHex
        {
            set
            {
                if (_commandIsHex != value)
                {
                    _commandIsHex = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("CommandIsHex"));
                }
            }

            get
            {
                return _commandIsHex;
            }
        }

        public string CommandData
        {
            set
            {
                if (_commandData != value)
                {
                    _commandData = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("CommandData"));
                }
            }
            get { return _commandData; }
        }

        public string Comment
        {
            set
            {
                if (_comment != value)
                {
                    _comment = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Comment"));
                }
            }
            get { return _comment; }
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
