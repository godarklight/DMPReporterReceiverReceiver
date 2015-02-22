using System;
using System.Net;
using System.Net.Sockets;

namespace DMPReporterReceiverReceiver
{

    public class NetworkListener
    {
        private ReportHandler reportHandler;
        private TcpListener tcpListener;
        private RemoteConnection remoteConnection;

        public NetworkListener(ReportHandler reportHandler)
        {
            this.reportHandler = reportHandler;
        }

        public void Start()
        {
            if (tcpListener == null)
            {
                reportHandler.Reset();
                tcpListener = new TcpListener(IPAddress.IPv6Any, 9003);
                tcpListener.Start();
                tcpListener.BeginAcceptTcpClient(ConnectCallback, null);
            }
        }

        private void ConnectCallback(IAsyncResult ar)
        {
            try
            {
                TcpClient newConnection = tcpListener.EndAcceptTcpClient(ar);
                IPAddress[] hostAddresses = Dns.GetHostAddresses("d-mp.org");
                bool found = false;
                IPAddress remoteAddress = ((IPEndPoint)newConnection.Client.RemoteEndPoint).Address;
                foreach (IPAddress testAddress in hostAddresses)
                {
                    if (remoteAddress.ToString() == testAddress.ToString())
                    {
                        found = true;
                    }
                    if (testAddress.AddressFamily == AddressFamily.InterNetwork && remoteAddress.AddressFamily == AddressFamily.InterNetworkV6)
                    {
                        if (remoteAddress.ToString() == ("::ffff:" + testAddress.ToString()))
                        {
                            found = true;
                        }
                    }
                }
                if (found)
                {
                    if (remoteConnection != null)
                    {
                        Console.WriteLine("Disconnecting stale connection");
                        remoteConnection.Close();
                        reportHandler.Reset();
                    }
                    Console.WriteLine("Connected to d-mp.org");
                    remoteConnection = new RemoteConnection(newConnection, reportHandler);
                }
                else
                {
                    Console.WriteLine("Disconnecting client from " + ((IPEndPoint)newConnection.Client.RemoteEndPoint).Address);
                    newConnection.Close();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Error accepting client: " + e.Message);
            }
            tcpListener.BeginAcceptTcpClient(ConnectCallback, null);
        }

        public void Stop()
        {
            if (tcpListener != null)
            {
                if (remoteConnection != null)
                {
                    try
                    {
                        remoteConnection.Close();
                    }
                    catch
                    {
                    }
                    remoteConnection = null;
                }
                tcpListener.Stop();
                tcpListener = null;
            }
        }
    }
}

