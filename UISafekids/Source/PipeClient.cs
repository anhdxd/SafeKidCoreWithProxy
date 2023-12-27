using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.IO.Pipes;
using System.Threading;
using System.Windows;

namespace UISafekids.Source
{
    class PipeClient
    {
        public static int FlagSend { get; set; }
        public static bool bResult;
        public static string StringSend { get; set; }
        public static EventWaitHandle signal = new EventWaitHandle(false, EventResetMode.AutoReset);
        private static readonly string PipeName = "KMASKPipe";
        public static NamedPipeClientStream pipeClient;
        public enum fText
        {
            AddHostToDB = 0,
            AddAppToDB,
            DeleteHostDB,
            DeleteAppDB,
            ChangeSetting,
            OpenGUIAdmin,
            GetListBlock,
        }
        
        public static void OninitPipes()
        {
            Thread threadSetting = new Thread(() =>
            {
                while (true)
                {
                    
                    try
                    {
                        pipeClient = new NamedPipeClientStream(".", PipeName, PipeDirection.InOut, PipeOptions.WriteThrough | PipeOptions.Asynchronous);
                        if (!pipeClient.IsConnected)
                        {
                            pipeClient.Connect();
                            signal.WaitOne(); // signer close
                        }
                        //SendSettingChange();
                    }
                    catch (Exception)
                    {
                        continue;
                    }
                }
            });
            threadSetting.IsBackground = true;
            threadSetting.Start();
        }
        public static void SendRequestToServer(String sSend)
        {
            try
            {
                StreamString ss = new StreamString(pipeClient);
                ss.WriteString(sSend);
            }
            catch (Exception e)
            {
                MessageBox.Show("Error send request to server" + e.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
        }
        public static void SendRequestChangeSetting()
        {
            try
            { 
            StreamString ss = new StreamString(pipeClient);
            String sSend = String.Format("{{'flag':{0}}}", (int)fText.ChangeSetting);
            ss.WriteString(sSend);
            }
            catch (Exception e)
            {
                MessageBox.Show("Error SendRequestChangeSetting" + e.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);

                return;
            }
        }
        public static void SendRequestOpenGUIWithAdmin()
        {
            try
            {
                StreamString ss = new StreamString(pipeClient);
                string sSend = string.Format("{{'flag':{0},'sPath':'{1}'}}", (int)fText.OpenGUIAdmin, System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName.Replace("\\", "\\\\"));
                ss.WriteString(sSend);
            }
            catch (Exception e)
            {
                return;
            }
        }
        public static string GetDataFromServer()
        {
            string sBuff = "";
            StreamString ss = new StreamString(pipeClient);
            if (pipeClient.CanRead)
            {
                sBuff = ss.ReadString();
            }
            return sBuff;
        }
        public static string GetListUserBlockFromServer(string sSend)
        {
            string sBuff = "";
            StreamString ss = new StreamString(pipeClient);
            //String sSend = String.Format("{{'flag':{0}}}", (int)fText.GetListBlock);
            ss.WriteString(sSend);
            if (pipeClient.CanRead)
            {
                sBuff = ss.ReadString();
            }
            return sBuff;
        }

 
    }
    public class StreamString
    {
        private Stream ioStream;
        private UnicodeEncoding streamEncoding;

        public StreamString(Stream ioStream)
        {
            this.ioStream = ioStream;
            streamEncoding = new UnicodeEncoding();
        }

        public string ReadString()
        {
            try
            {
                int len;
                len = ioStream.ReadByte() * 256;
                len += ioStream.ReadByte();
                var inBuffer = new byte[len];
                ioStream.Read(inBuffer, 0, len);

                return streamEncoding.GetString(inBuffer);
            }
            catch (Exception)
            {
                MessageBox.Show("Error readstring pipe", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                return "";
            }
        }

        public int WriteString(string outString)
        {
            try
            {
                byte[] outBuffer = streamEncoding.GetBytes(outString);
                int len = outBuffer.Length;
                if (len > UInt16.MaxValue)
                {
                    len = (int)UInt16.MaxValue;
                }
                ioStream.WriteByte((byte)(len / 256));
                ioStream.WriteByte((byte)(len & 255));
                ioStream.Write(outBuffer, 0, len);
                ioStream.Flush();

                return outBuffer.Length + 2;
            }
            catch (Exception)
            {
                MessageBox.Show("Error writestring pipe", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                return 0;
            }
        }
    }
}
