using System;
using System.Windows.Forms;
using System.Text;
using System.IO.Ports;
using System.Threading;
using System.Collections.Generic;

namespace SerialPort_HomeworkWeGoFor
{
    public partial class MainForm : Form
    {
        private SerialPort _sp;

        public MainForm()
        {
            InitializeComponent();
            this.DoubleBuffered = true;
            _sp = new SerialPort();
            _sp.DataReceived += _sp_DataReceived;
            _sp.ReadTimeout = 500;
            _sp.WriteTimeout = 500;

            _curCode = _GB2312;
        }


        bool isBufferReceived = false;
        List<byte> _buf = new List<byte>();
        void _sp_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            //button1.Enabled = false;
            int bytesToRead = _sp.BytesToRead;
            byte[] buf = new byte[bytesToRead];
            try
            {
                _sp.Read(buf, 0, bytesToRead);
            }
            catch (Exception ex)
            {
                //button1.Enabled = true;
                return;
            }

            for (int i = 0; i < bytesToRead; i++ )
            {
                _buf.Add(buf[i]);
            }
            isBufferReceived = true;
            //button1.Enabled = true;
        }

        private bool isPortLegal()
        {
            #region portname
            string _portname = "";
            string[] _legalports = SerialPort.GetPortNames();
            foreach (string s in _legalports)
            {
                if (s.Equals(comboBox1.Text))
                {
                    _portname = s;
                    break;
                }
            }
            if (_portname != "")
            {
                _sp.PortName = _portname;
            }
            else
            {
                textBox1.Text += Environment.NewLine + "不存在的端口号！无法开启";
                return false;
            }
            #endregion
            #region baudrate
            if (comboBox2.Items.Contains(comboBox2.Text))
            {
                _sp.BaudRate = int.Parse(comboBox2.Text);
            }
            else
            {
                textBox1.Text += Environment.NewLine + "不科学的波特率！无法开启";
                return false;
            }
            #endregion
            #region parity
            switch (comboBox3.Text)
            {
                case "NONE":
                case "None":
                    _sp.Parity = Parity.None;
                    break;
                case "EVEN":
                    _sp.Parity = Parity.Even;
                    break;
                case "ODD":
                    _sp.Parity = Parity.Odd;
                    break;
                case "MARK":
                    _sp.Parity = Parity.Mark;
                    break;
                case "SPACE":
                    _sp.Parity = Parity.Space;
                    break;
                default:
                    textBox1.Text += Environment.NewLine + "你这狗比给了什么校验协议！无法开启";
                    return false;
            }
            #endregion
            #region databits
            if (comboBox4.Items.Contains(comboBox4.Text))
            {
                _sp.DataBits = int.Parse(comboBox4.Text);
            }
            else
            {
                textBox1.Text += Environment.NewLine + "不科学的数据位！无法开启";
                return false;
            }
            #endregion
            #region stopbits
            switch (comboBox5.Text)
            {
                case "1":
                    _sp.StopBits = StopBits.One;
                    break;
                case "0":
                    _sp.StopBits = StopBits.None;
                    break;
                case "1.5":
                    _sp.StopBits = StopBits.OnePointFive;
                    break;
                case "2":
                    _sp.StopBits = StopBits.Two;
                    break;
                default:
                    textBox1.Text += Environment.NewLine + "你这狗比给了什么停止位！无法开启";
                    return false;
            }
            #endregion
            return true;
        }
        private void checkPortName()
        {
            comboBox1.Items.Clear();
            string[] ports = SerialPort.GetPortNames();
            Array.Sort(ports);
            foreach (string port in ports)
            {
                comboBox1.Items.Add(port);
            }
            if (comboBox1.Items.Count > 0)
            {
                int index = 0;
                foreach (object o in comboBox1.Items)
                {
                    if (o.ToString() == _sp.PortName)
                    {
                        comboBox1.SelectedIndex = index;
                    }
                    else
                    {
                        index++;
                    }
                }
                if (index == comboBox1.Items.Count)
                {
                    comboBox1.SelectedIndex = 0;
                }
            }
        }
        private void button1_Click(object sender, EventArgs e)
        {
            if (_sp.IsOpen)
            {
                try
                {
                    _sp.Close();
                }
                catch (Exception ex)
                {
                    textBox1.Text += Environment.NewLine + ex.Message;
                    return;
                }
                if (!_sp.IsOpen)
                {
                    textBox1.Text += Environment.NewLine;
                    textBox1.Text += "端口已经关闭";
                    textBox1.Text += Environment.NewLine;
                    button1.Text = "Open";
                }  
            } 
            else
            {
                if (!isPortLegal())
                {
                    return;
                }
                try
                {
                    _sp.Open();
                }
                catch (Exception ex)
                {
                    textBox1.Text += Environment.NewLine + ex.Message;
                    return;
                }
                if (_sp.IsOpen)
                {
                    textBox1.Text = "";
                    textBox1.Text += "端口成功打开！在" + _sp.PortName + "上监听中";
                    textBox1.Text += Environment.NewLine;
                    button1.Text = "Close";
                }  
            }
        }
        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            textBox1.SelectionStart = textBox1.Text.Length;
            textBox1.ScrollToCaret();
        }


        private Encoding _GB2312 = Encoding.GetEncoding("GB2312");
        private Encoding _Unicode = Encoding.GetEncoding("Unicode");
        private Encoding _UTF_8 = Encoding.GetEncoding("UTF-8");
        private Encoding _curCode;
        private void button2_Click(object sender, EventArgs e)
        {
            if (_sp.IsOpen)
            {
                button1.Enabled = false;
                try
                {
                    byte[] _writebuf = _curCode.GetBytes(textBox2.Text);
                    _sp.Write(_writebuf, 0 ,_writebuf.Length);
                }
                catch { }
                finally
                {
                    button1.Enabled = true;
                    textBox2.Text = "";
                }
            }
        }

        private static int _t = 0;
        private void timer1_Tick(object sender, EventArgs e)
        {
            if (isBufferReceived)
            {

                textBox1.Text += Environment.NewLine;
                byte[] buf = new byte[_buf.Count];
                int i = 0;
                foreach (byte b in _buf)
                {
                    buf[i] = b;
                    i++;
                }
                _buf.Clear();
                textBox1.Text += Environment.NewLine;
                textBox1.Text += _curCode.GetString(buf);

                isBufferReceived = false;
                timer1.Enabled = true;
            }
            _t++;
            if (_t%100 == 0)
            {
                checkPortName();
            }
            
        }

        private void MainForm_KeyDown(object sender, KeyEventArgs e)
        {
            if(e.KeyCode == Keys.Enter)
            {
                button2_Click(sender, e);
                e.Handled = true;
            }
        }

        private void radioButton1_CheckedChanged(object sender, EventArgs e)
        {
            _curCode = _GB2312;
        }

        private void radioButton2_CheckedChanged(object sender, EventArgs e)
        {
            _curCode = _Unicode;
        }

        private void radioButton3_CheckedChanged(object sender, EventArgs e)
        {
            _curCode = _UTF_8;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            textBox1.Text = "";
        }







    }
}
