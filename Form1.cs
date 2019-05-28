using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BMSCtl
{
    public partial class Form1 : Form
    {
        public delegate void processCurrentDataDelegate(byte[] cData);
        public delegate void addTextDelegate(string data);
        public addTextDelegate myDelegate;
        public processCurrentDataDelegate pdDelegate;

        System.Threading.Thread CloseDown;
        int cnt = 0;
        int remBytes = 0;
        int timerDelta = 0;
        int wrCnt = 0;
        byte[] currentData = new byte[140];
        bool stop = false;

        int headerCnt = 0;

        StreamWriter w;

        void processCurrentData(byte[] cData)
        {
            if (stop)
                return;
            //Overall voltage
            textBox43.Text = (((currentData[4]<<8)|currentData[5])/10.0).ToString();
            //Overall current
            int cur = ((currentData[70] << 24) | (currentData[71] << 16) | (currentData[72] << 8) | currentData[73]);
            textBox44.Text = (cur / 10.0).ToString();
            if (Math.Abs(cur / 10.0) > 100.0)
            {
                textBox1.Text += "ERROR:" + stringToHexData(currentData);
                return;
            }
            //Cells 10s
            textBox2.Text = (((currentData[6] << 8) | currentData[7]) / 1000.0).ToString();
            textBox3.Text = (((currentData[8] << 8) | currentData[9]) / 1000.0).ToString();
            textBox4.Text = (((currentData[10] << 8) | currentData[11]) / 1000.0).ToString();
            textBox5.Text = (((currentData[12] << 8) | currentData[13]) / 1000.0).ToString();
            textBox6.Text = (((currentData[14] << 8) | currentData[15]) / 1000.0).ToString();
            textBox7.Text = (((currentData[16] << 8) | currentData[17]) / 1000.0).ToString();
            textBox8.Text = (((currentData[18] << 8) | currentData[19]) / 1000.0).ToString();
            textBox9.Text = (((currentData[20] << 8) | currentData[21]) / 1000.0).ToString();
            textBox13.Text = (((currentData[22] << 8) | currentData[23]) / 1000.0).ToString();
            textBox12.Text = (((currentData[24] << 8) | currentData[25]) / 1000.0).ToString();
            //Other voltage data
            textBox11.Text = (((currentData[26] << 8) | currentData[27]) / 1000.0).ToString();
            textBox10.Text = (((currentData[28] << 8) | currentData[29]) / 1000.0).ToString();
            textBox17.Text = (((currentData[30] << 8) | currentData[31]) / 1000.0).ToString();
            textBox16.Text = (((currentData[32] << 8) | currentData[33]) / 1000.0).ToString();
            textBox15.Text = (((currentData[34] << 8) | currentData[35]) / 1000.0).ToString();
            textBox14.Text = (((currentData[36] << 8) | currentData[37]) / 1000.0).ToString();
            textBox33.Text = (((currentData[38] << 8) | currentData[39]) / 1000.0).ToString();
            textBox32.Text = (((currentData[40] << 8) | currentData[41]) / 1000.0).ToString();
            textBox31.Text = (((currentData[42] << 8) | currentData[43]) / 1000.0).ToString();
            textBox30.Text = (((currentData[44] << 8) | currentData[45]) / 1000.0).ToString();
            textBox29.Text = (((currentData[46] << 8) | currentData[47]) / 1000.0).ToString();
            textBox28.Text = (((currentData[48] << 8) | currentData[49]) / 1000.0).ToString();
            textBox27.Text = (((currentData[50] << 8) | currentData[51]) / 1000.0).ToString();
            textBox26.Text = (((currentData[52] << 8) | currentData[53]) / 1000.0).ToString();
            textBox25.Text = (((currentData[54] << 8) | currentData[55]) / 1000.0).ToString();
            textBox24.Text = (((currentData[56] << 8) | currentData[57]) / 1000.0).ToString();
            textBox23.Text = (((currentData[58] << 8) | currentData[59]) / 1000.0).ToString();
            textBox22.Text = (((currentData[60] << 8) | currentData[61]) / 1000.0).ToString();
            textBox21.Text = (((currentData[62] << 8) | currentData[63]) / 1000.0).ToString();
            textBox20.Text = (((currentData[64] << 8) | currentData[65]) / 1000.0).ToString();
            textBox19.Text = (((currentData[66] << 8) | currentData[67]) / 1000.0).ToString();
            textBox18.Text = (((currentData[68] << 8) | currentData[69]) / 1000.0).ToString();

            //Temp data
            textBox39.Text = (((currentData[91] << 8) | currentData[92])).ToString();
            textBox38.Text = (((currentData[93] << 8) | currentData[94])).ToString();
            textBox37.Text = (((currentData[95] << 8) | currentData[96])).ToString();
            textBox36.Text = (((currentData[97] << 8) | currentData[98])).ToString();
            textBox35.Text = (((currentData[99] << 8) | currentData[100])).ToString();
            textBox34.Text = (((currentData[101] << 8) | currentData[102])).ToString();

            textBox45.Text = (((currentData[76] << 16) | (currentData[77] << 8) | currentData[78]) / 1000.0).ToString();
            textBox46.Text = (((currentData[80] << 16) | (currentData[81] << 8) | currentData[82]) / 1000.0).ToString();

            byte xorsum = 0;
            for (int i=0;i!=140;i++)
            {
                xorsum ^= currentData[i];
            }

            if (timerDelta==0)
            {
                timerDelta = currentData[139];
            }
            textBox41.Text = currentData[139].ToString();// (currentData[139] - timerDelta).ToString();
            timerDelta = currentData[139];
        }

        public void readProtocol(byte data)
        {
            if (headerCnt==0)
            {
                if (data == 0xAA)
                {
                    headerCnt++;
                    return;
                }
                else
                    headerCnt = 0;
            }
            if (headerCnt == 1)
            {
                if (data == 0x55)
                {
                    headerCnt++;
                    return;
                }
                else
                    headerCnt = 0;
            }
            if (headerCnt == 2)
            {
                if (data == 0xAA)
                {
                    headerCnt++;
                    currentData[0] = 0xAA;
                    currentData[1] = 0x55;
                    currentData[2] = 0xAA;
                    cnt = 140 - 3;
                    return;
                }
                else
                    headerCnt = 0;
            }
            if (headerCnt==3)
            {
                currentData[140 - cnt] = data;
                cnt--;
                if (cnt==0)
                {
                    string res = stringToHexData(currentData);
                    textBox1.Invoke(this.myDelegate, res);
                    if (checkIntegrity(currentData))
                    {
                        Invoke(this.pdDelegate, currentData);
                    }
                    
                    headerCnt = 0;
                }
            }
        }

        private bool checkIntegrity(byte[] data)
        {
            if ((data[139]==0)|| (data[138] == 0)|| (data[137] == 0)|| (data[136] == 0))
                    return false;
            if ((data[135] != 0) || (data[134] != 0) || (data[133] != 0) || (data[132] != 0))
                return false;

            for (int i=26;i!=69;i++)
            {
                if (data[i] != 0)
                    return false;
            }
            return true;
        }

        private void CloseSerialOnExit()
        {

            try
            {
                serialPort1.DtrEnable = false;
                serialPort1.RtsEnable = false;
                serialPort1.DiscardInBuffer();
                serialPort1.DiscardOutBuffer();
                serialPort1.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);

            }
        }

        public Form1()
        {
            InitializeComponent();
        }

        public void addText(string data)
        {
            if (!stop)
                textBox1.Text = data;
        }

        public string stringToHexData(byte[] data)
        {
            string res = "";
            for (int i = 0; i != data.Length; i++)
            {
                res += data[i].ToString("x2");
                res += " ";
                cnt++;
                if (cnt==140)
                {
                    res += "\r\n";
                    
                    cnt = 0;
                }
            }
            return res;
        }

        public byte[] addToArray(byte[] aArray, byte[] bArray)
        {
            byte[] newArray = new byte[aArray.Length + bArray.Length];
            aArray.CopyTo(newArray, 0);
            bArray.CopyTo(newArray, bArray.Length);
            return newArray;
        }

        /*public void checkHeader(byte[] data)
        {
            if (remBytes==0)
            {
                for (int i = 0; i != data.Length - 3; i++)
                {
                    if ((data[i] == 0xAA) && (data[i + 1] == 0x55) && (data[i + 2] == 0xAA))
                    {
                        toolStripStatusLabel2.Text = "OK";
                        currentData = new byte[140];
                        if (data.Length >= 140)
                        {
                            //Buffer Full
                            Buffer.BlockCopy(data, i, currentData, 0, currentData.Length);
                            string res = stringToHexData(currentData);
                            textBox1.Invoke(this.myDelegate, res);
                            remBytes = 0;
                        }
                        else
                        {
                            Buffer.BlockCopy(data, i, currentData, 0, data.Length-i);
                            remBytes = 140 - (data.Length - i);
                        }
                    }
                }
            }
            else
            {
                if (data.Length >= remBytes)
                {
                    //Buffer Full
                    Buffer.BlockCopy(data, 0, currentData, 140 - remBytes, remBytes);
                    string res = stringToHexData(currentData);
                    textBox1.Invoke(this.myDelegate, res);
                    remBytes = 0;                   
                }
                else
                {
                    Buffer.BlockCopy(data, 0, currentData, 140 - remBytes, data.Length);
                    remBytes = 140 - currentData.Length;
                    if (remBytes==0)
                    {
                        string res = stringToHexData(currentData);
                        textBox1.Invoke(this.myDelegate, res);
                    }
                }
            }

            
        }*/

        private void serialPort1_DataReceived(object sender, System.IO.Ports.SerialDataReceivedEventArgs e)
        {
            try
            {
                int bytes = serialPort1.BytesToRead;
                byte[] respBuffer = new byte[bytes];
                serialPort1.Read(respBuffer, 0, bytes);
                //string res = stringToHexData(respBuffer);
                //textBox1.Invoke(this.myDelegate, res);
                for (int i=0;i!=bytes;i++)
                    readProtocol(respBuffer[i]);
            }
            catch(Exception)
            {

            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (serialPort1.IsOpen)
            {
                timer1.Enabled = false;
                CloseDown = new System.Threading.Thread(new System.Threading.ThreadStart(CloseSerialOnExit));
                CloseDown.Start();
                button1.Text = "Запуск логирования";
            }
            else
            {
                serialPort1.PortName = textBox40.Text;
                remBytes = 0;
                try
                {
                    serialPort1.Open();
                    if (serialPort1.IsOpen)
                    {
                        int inter;
                        int.TryParse(textBox42.Text, out inter);
                        timer1.Interval = inter;
                        timer1.Enabled = true;
                        button1.Text = "Остановка логирования";
                    }
                }
                catch (Exception)
                {

                }
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            this.myDelegate = new addTextDelegate(addText);
            this.pdDelegate = new processCurrentDataDelegate(processCurrentData);
            w = File.AppendText("dat"+ DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss", CultureInfo.InvariantCulture) +".csv");
            Log("LOG STARTED!");
            label4.Text = "";
        }

        public void Log(string logMessage)
        {
            w.WriteLine(DateTime.Now+";"+wrCnt.ToString()+";"+ logMessage);
            wrCnt += 1;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            textBox1.Text = "";
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            
        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            w.Close();
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            byte[] send_data = new byte[6];
            send_data[0] = 0xDB;
            send_data[1] = 0xDB;
            send_data[2] = 0x00;
            send_data[3] = 0x00;
            send_data[4] = 0x00;
            send_data[5] = 0x00;
            serialPort1.Write(send_data,0,6);
            string message = textBox2.Text + ";"
                            + textBox3.Text + ";"
                            + textBox4.Text + ";"
                            + textBox5.Text + ";"
                            + textBox9.Text + ";"
                            + textBox8.Text + ";"
                            + textBox7.Text + ";"
                            + textBox6.Text + ";"
                            + textBox13.Text + ";"
                            + textBox12.Text + ";"
                            + textBox43.Text + ";"
                            + textBox44.Text + ";"
                            + textBox46.Text + ";"
                            + textBox39.Text + ";"
                            + textBox38.Text + ";"
                            + textBox37.Text + ";"
                            + textBox36.Text + ";"
                            + textBox35.Text + ";"
                            + textBox34.Text;
            Log(message);
            label4.Text = wrCnt.ToString();
        }
    }
}
