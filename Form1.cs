using NAudio.CoreAudioApi;
using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Windows.Forms;
using VoiceTransform.Helpers;
using VoiceTransform.ImageCreation;
using VoiceTransform.ImageCreation.Converters;
using VoiceTransform.Models;

namespace VoiceTransform
{
    public partial class Form1 : Form
    {
        WaveIn waveIn;
        WaveFileWriter writer;

        string record = "F:\\Diplom\\VoiceTransform\\Buffer\\имя_файла.wav";
        string uncrypt = "F:\\Diplom\\VoiceTransform\\Buffer\\Uncrypted\\uncrypted.wav";
        string crypted = "F:\\Diplom\\VoiceTransform\\Buffer\\Crypted\\crypted.png";
        string result = "F:\\Diplom\\VoiceTransform\\Buffer\\Crypted\\result";
        string decripted_sign = "F:\\Diplom\\VoiceTransform\\Buffer\\Uncrypted\\decripted_sign.png";

        public Form1()
        {
            InitializeComponent();

            foreach (KeyValuePair<string, MMDevice> device in GetInputAudioDevices())
            {
                Console.WriteLine("Name: {0}, State: {1}", device.Key, device.Value.State);
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            {
                waveIn = null;
                writer = null;

                try
                {
                    MessageBox.Show("Start Recording");

                    waveIn = new WaveIn();
                    waveIn.DeviceNumber = 0;

                    waveIn.DataAvailable += DataAvailable;
                    waveIn.RecordingStopped += new EventHandler<StoppedEventArgs>(RecordingStopped);

                    waveIn.WaveFormat = new WaveFormat(41000, 1);
                    writer = new WaveFileWriter(record, waveIn.WaveFormat);

                    waveIn.StartRecording();
                }
                catch (Exception ex)
                { MessageBox.Show(ex.Message); }
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (waveIn != null)
            {
                try
                {
                    StopRecording();
                }
                finally
                {
                    waveIn.Dispose();
                    writer.Close();
                }
            }
        }

        void StopRecording()
        {
            MessageBox.Show("StopRecording");
            waveIn.StopRecording();
        }

        private void RecordingStopped(object sender, EventArgs e)
        {
            if (InvokeRequired)
            {
                BeginInvoke(new EventHandler(RecordingStopped), sender, e);
            }
            else
            {
                waveIn.Dispose();
                waveIn = null;
                writer.Close();
                writer = null;
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {

            var fileDialog = new OpenFileDialog();

            if (fileDialog.ShowDialog() == DialogResult.OK)
            {

                var hiddenBytes = ImageHider.Decode(fileDialog.FileName);

                BmpToByteArreyConverter.ByteArrayToBitmap(hiddenBytes.Data).Save(decripted_sign, ImageFormat.Png);

                if (!checkBox1.Checked)
                {
                    var byte_buffer = VoiceCrypter.Uncrypt_Voice(decripted_sign, hiddenBytes.Lenght);
                    var write_uncrypted = new WaveFileWriter(uncrypt, new WaveFormat(41000, 1));

                    write_uncrypted.Write(byte_buffer, 0, byte_buffer.Length);

                    write_uncrypted.Close();
                    write_uncrypted = null;
                }
                else
                {

                    var n = Convert.ToInt32(textBox1.Text) * Convert.ToInt32(textBox2.Text); 

                    var byte_buffer = VoiceCrypter.Uncrypt_Voice(decripted_sign, n, hiddenBytes.Lenght);
                    var write_uncrypted = new WaveFileWriter(uncrypt, new WaveFormat(41000, 1));

                    write_uncrypted.Write(byte_buffer, 0, byte_buffer.Length);

                    write_uncrypted.Close();
                    write_uncrypted = null;
                }
                MessageBox.Show("Выберите один из вариантов", "Сообщение");
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            var p = PrimeGenerator.Generate();
            var q = PrimeGenerator.Generate();

            if(p == q || p * q < 255)
            {
                while(p == q || p * q < 255)
                {
                    q = PrimeGenerator.Generate();
                }
            }

            textBox1.Text = p.ToString();
            textBox2.Text = q.ToString();
        }

        void DataAvailable(object sender, WaveInEventArgs e)
        {
            if (InvokeRequired)
            {
                BeginInvoke(new EventHandler<WaveInEventArgs>(DataAvailable), sender, e);
            }
            else
            {
                bool result = ProcessData(e);

                if (result == true)
                {
                    writer.WriteData(e.Buffer, 0, e.BytesRecorded);
                }

            }
        }

        private bool ProcessData(WaveInEventArgs e)
        {
            var porog = 0.02;
            var result = false;

            var Tr = false;
            double Sum2 = 0;

            int Count = e.BytesRecorded / 2;

            for (int index = 0; index < e.BytesRecorded; index += 2)
            {
                double Tmp = (short)((e.Buffer[index + 1] << 8) | e.Buffer[index + 0]);
                Tmp /= 32768.0;
                Sum2 += Tmp * Tmp;
                if (Tmp > porog)
                    Tr = true;
            }

            Sum2 /= Count;

            if (Tr || Sum2 > porog)
            {
                result = true;
            }
            else
            {
                result = false;
            }

            return result;
        }

        public Dictionary<string, MMDevice> GetInputAudioDevices()
        {
            Dictionary<string, MMDevice> retVal = new Dictionary<string, MMDevice>();
            MMDeviceEnumerator enumerator = new MMDeviceEnumerator();
            int waveInDevices = WaveIn.DeviceCount;
            for (int waveInDevice = 0; waveInDevice < waveInDevices; waveInDevice++)
            {
                WaveInCapabilities deviceInfo = WaveIn.GetCapabilities(waveInDevice);
                foreach (MMDevice device in enumerator.EnumerateAudioEndPoints(DataFlow.Capture, DeviceState.All))
                {
                    if (device.FriendlyName.StartsWith(deviceInfo.ProductName))
                    {
                        retVal.Add(device.FriendlyName, device);
                        break;
                    }
                }
            }

            return retVal;
        }

        private void button5_Click(object sender, EventArgs e)
        {
            var data_AES = new AES_DataModel();
            var data_RGB = new RGB_DataModel();

            if (!checkBox1.Checked)
            {
                data_RGB = VoiceCrypter.Crypt_Voice(record, 41000);
            }
            else
            {
                var p = Convert.ToInt32(textBox1.Text);
                var q = Convert.ToInt32(textBox2.Text);

                data_AES = VoiceCrypter.Crypt_Voice(record, 41000, p, q);
            }

            if ((data_AES != null && data_AES.ByteArray != null && data_AES.ByteArray.Length > 0) || (data_RGB != null && data_RGB.ByteArray.Length > 0))
            {
                var fileDialog = new OpenFileDialog();

                if (fileDialog.ShowDialog() == DialogResult.OK)
                {
                    if (!checkBox1.Checked)
                    {
                        var bmp = BmpToPictureConverter.CopyDataToBitmap(data_RGB.ByteArray, data_RGB.ByteArray.Length, fileDialog.FileName);

                        bmp.Save(crypted, ImageFormat.Png);
                    }
                    else
                    {
                        var d = BitConverter.GetBytes(data_AES.Value);

                        var updated_array = data_AES.ByteArray.ToList();

                        foreach (var part in d)
                        {
                            updated_array.Add(part);
                        }

                        updated_array.Add(253);

                        var array = updated_array.ToArray();

                        var bmp = BmpToPictureConverter.CopyDataToBitmap(array, array.Length, fileDialog.FileName);

                        bmp.Save(crypted, ImageFormat.Png);
                    }

                    byte[] hiddenBytes = BmpToByteArreyConverter.BitmapToByteArray(Image.FromFile(crypted));

                    ImageHider.Encode(hiddenBytes, fileDialog.FileName, result + ".png");

                    MessageBox.Show("Выберите один из вариантов","Сообщение");
                }
            }
        }
    }
}
