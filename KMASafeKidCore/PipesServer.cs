using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.IO.Pipes;
using Newtonsoft.Json.Linq;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Data.SQLite;
namespace KMASafeKidCore
{
    public enum fText
    {
        AddHostToDB = 0,
        AddAppToDB,
        DeleteHostDB,
        DeleteAppDB,
        ChangeSetting,
        OpenGUIAdmin,
        GetListBlock,
        //AddPassword,
        //GetPassword
    }
    
    class PipesServer
    {
        private static String PipeName = "KMASKPipe";

        public static bool PipeServer()
        {

            String sRead;
            //Process procActive = null;

            var pipeSecurity = new PipeSecurity();
            pipeSecurity.AddAccessRule(new PipeAccessRule(new SecurityIdentifier(WellKnownSidType.BuiltinUsersSid, null), PipeAccessRights.ReadWrite, AccessControlType.Allow));
            NamedPipeServerStream pipeServer = new NamedPipeServerStream(PipeName, PipeDirection.InOut, 1, PipeTransmissionMode.Byte, PipeOptions.Asynchronous | PipeOptions.WriteThrough, 0, 0, pipeSecurity);

        // Wait for a client to connect
        _WAITCONNECT:
            //WriteLog("WAIT CONNECT");
            pipeServer.WaitForConnection();
            StreamString ss = new StreamString(pipeServer);
            Console.WriteLine("CONNECT OKE");
            while (true)
            {
                try
                {
                    if (!pipeServer.IsConnected)
                    {
                        pipeServer.Disconnect();
                        Console.WriteLine("DISCONNECT");
                        //return false;
                        goto _WAITCONNECT;
                    }

                    sRead = ss.ReadString();
                    var parse = JObject.Parse(sRead);
                    //WriteLog(sRead);
                    switch ((int)parse["flag"])
                    {
                        case (int)fText.AddHostToDB:
                            BlockDB.AddUserHostDB(parse["sDomain"].ToString());
                            break;

                        case (int)fText.AddAppToDB:
                            BlockDB.AddUserAppDB(parse["sPath"].ToString());
                            break;

                        case (int)fText.DeleteHostDB:
                            BlockDB.DeleteUserHostDB(parse["sDomain"].ToString());
                            break;

                        case (int)fText.DeleteAppDB:
                            BlockDB.DeleteUserAppDB(parse["sPath"].ToString());
                            break;

                        case (int)fText.ChangeSetting:
                            //ConfigInApp.ChangeConfig(parse["Field"].ToString(),parse["val"].ToString());
                            ConfigInApp.ReadConfigFile();
                            break;
                        case (int)fText.OpenGUIAdmin:
                            System.Threading.Thread.Sleep(2500);
                            System.Diagnostics.Process.Start(parse["sPath"].ToString());
                            break;

                        default:
                            break;
                    }
                    //System.Threading.Thread.Sleep(500);
                    //ss.WriteString("deo biet");
                    //Console.WriteLine(sRead); // Đọc từ client gửi về
                }
                catch (Exception ex)
                {
                    WriteLog("ERROR: " + ex.Message);
                    continue;
                }
            }
        }

        private static void WriteLog(string Message)
        {
            string path = AppDomain.CurrentDomain.BaseDirectory + "\\Logs";
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            string filepath = AppDomain.CurrentDomain.BaseDirectory + "\\Logs\\ServiceLog.log";
            if (!File.Exists(filepath))
            {
                // Create a file to write to.   
                using (StreamWriter sw = File.CreateText(filepath))
                {
                    sw.WriteLine(Message);
                }
            }
            else
            {
                using (StreamWriter sw = File.AppendText(filepath))
                {
                    sw.WriteLine(Message);
                }
            }
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
            int len;
            len = ioStream.ReadByte() * 256;
            len += ioStream.ReadByte();
            var inBuffer = new byte[len];
            ioStream.Read(inBuffer, 0, len);

            return streamEncoding.GetString(inBuffer);
        }

        public int WriteString(string outString)
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
    }
}
