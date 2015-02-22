using System;

namespace DMPReporterReceiverReceiver
{
    public class MainClass
    {
        NetworkListener networkListener;
        public static void Main()
        {
            MainClass mainClass = new MainClass();
            mainClass.Run();
        }

        public void Run()
        {
            DatabaseSettings dbSettings = new DatabaseSettings();
            dbSettings.LoadFromFile("dbsettings.xml");
            DatabaseBackend databaseBackend = new DatabaseBackend(dbSettings);
            ReportHandler reportHandler = new ReportHandler(databaseBackend);
            networkListener = new NetworkListener(reportHandler);
            networkListener.Start();
            while (true)
            {
                System.Threading.Thread.Sleep(1000);
            }
        }
    }
}

