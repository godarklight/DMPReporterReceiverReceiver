using System;
using System.Diagnostics;
using System.Threading;
using System.Net;
using System.Net.Sockets;

namespace DMPReporterReceiverReceiver
{
    public class RemoteConnection
    {
        public bool Connected
        {
            get;
            private set;
        }

        private TcpClient tcpClient;
        private ReportHandler reportHandler;
        private int receivingType;
        private byte[] receivingBytes;
        private int receivingBytesLeft;
        private bool receivingHeader;
        private long lastSendTime;
        private long lastReceiveTime;
        private Stopwatch connectionTime;
        private Thread watchdogThread;
        private const int RECEIVE_TIMEOUT_MS = 60000;
        private const int HEARTBEAT_INTERVAL_MS = 30000;

        public RemoteConnection(TcpClient tcpClient, ReportHandler reportHandler)
        {
            //Setup state
            this.tcpClient = tcpClient;
            this.reportHandler = reportHandler;
            Connected = true;
            connectionTime = new Stopwatch();
            connectionTime.Start();
            //Start receiving
            ReceiveNewMessage();
            tcpClient.GetStream().BeginRead(receivingBytes, receivingBytes.Length - receivingBytesLeft, receivingBytesLeft, ReceiveCallback, null);
            //Start watchdog
            watchdogThread = new Thread(HeartbeatWatchdog);
            watchdogThread.IsBackground = true;
            watchdogThread.Start();    
        }

        private void HeartbeatWatchdog()
        {
            try
            {
                while (Connected)
                {
                    if ((connectionTime.ElapsedMilliseconds - lastSendTime) > HEARTBEAT_INTERVAL_MS)
                    {
                        Console.WriteLine("Sending heartbeat");
                        lastSendTime = connectionTime.ElapsedMilliseconds;
                        byte[] heartbeatBytes = new byte[8];
                        tcpClient.GetStream().Write(heartbeatBytes, 0, heartbeatBytes.Length);
                    }
                    if ((connectionTime.ElapsedMilliseconds - lastReceiveTime) > RECEIVE_TIMEOUT_MS)
                    {
                        Console.WriteLine("Connection timeout");
                        Close();
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Watchdog error: " + e);
                Close();
            }
        }

        private void ReceiveCallback(IAsyncResult ar)
        {
            try
            {
                int bytesReceived = tcpClient.GetStream().EndRead(ar);
                if (bytesReceived > 0)
                {
                    lastReceiveTime = connectionTime.ElapsedMilliseconds;
                    receivingBytesLeft -= bytesReceived;
                    if (receivingBytesLeft == 0)
                    {
                        if (!receivingHeader)
                        {
                            byte[] typeBytes = new byte[4];
                            byte[] lengthBytes = new byte[4];
                            Array.Copy(receivingBytes, 0, typeBytes, 0, typeBytes.Length);
                            Array.Copy(receivingBytes, typeBytes.Length, lengthBytes, 0, lengthBytes.Length);
                            if (BitConverter.IsLittleEndian)
                            {
                                Array.Reverse(typeBytes);
                                Array.Reverse(lengthBytes);
                            }
                            receivingType = BitConverter.ToInt32(typeBytes, 0);
                            int receivingLength = BitConverter.ToInt32(lengthBytes, 0);
                            if (receivingLength == 0)
                            {
                                if (receivingType == 0)
                                {
                                    Console.WriteLine("Received heartbeat");
                                }
                                ReceiveNewMessage();
                            }
                            else
                            {
                                receivingHeader = true;
                                receivingBytesLeft = receivingLength;
                                receivingBytes = new byte[receivingLength];
                            }
                        }
                        else
                        {
                            if (receivingType == 1)
                            {
                                reportHandler.HandleReport(receivingBytes);
                            }
                            ReceiveNewMessage();
                        }
                    }
                }
                else
                {
                    Thread.Sleep(10);
                }
                tcpClient.GetStream().BeginRead(receivingBytes, receivingBytes.Length - receivingBytesLeft, receivingBytesLeft, ReceiveCallback, null);
            }
            catch (Exception e)
            {
                Console.WriteLine("Receive error: " + e);
                Close();
            }
        }

        private void ReceiveNewMessage()
        {
            receivingType = 0;
            receivingBytesLeft = 8;
            receivingBytes = new byte[8];
            receivingHeader = false;
        }

        public void Close()
        {
            if (Connected)
            {
                Connected = false;
                try
                {
                    tcpClient.Close();
                }
                catch
                {
                }
                tcpClient = null;
                reportHandler = null;
                receivingBytes = null;
                Console.WriteLine("Closed connection to d-mp.org");
            }
        }
    }
}

