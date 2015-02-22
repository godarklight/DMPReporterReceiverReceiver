using System;
using MessageStream2;

namespace DMPReporterReceiverReceiver
{
    public class ReportHandler
    {
        DatabaseBackend databaseBackend;

        public ReportHandler(DatabaseBackend databaseBackend)
        {
            this.databaseBackend = databaseBackend;
        }

        public void HandleReport(byte[] messageData)
        {
            using (MessageReader mr = new MessageReader(messageData))
            {
                int clientID = mr.Read<int>();
                int typeID = mr.Read<int>();
                switch (typeID)
                {
                    case 0:
                        string gameAddress = mr.Read<string>();
                        databaseBackend.ConnectClient(clientID, gameAddress);
                        break;
                    case 1:
                        byte[] reportBytes = mr.Read<byte[]>();
                        databaseBackend.ReportClient(clientID, reportBytes);
                        break;
                    case 2:
                        databaseBackend.DisconnectClient(clientID);
                        break;
                }
            }
        }

        public void Reset()
        {
            databaseBackend.WipeDatabase();
        }
    }
}

