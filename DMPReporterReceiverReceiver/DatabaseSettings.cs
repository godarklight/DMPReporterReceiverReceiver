using System;
using System.Xml;
using System.IO;

namespace DMPReporterReceiverReceiver
{
    public class DatabaseSettings
    {
        public string host = "localhost";
        public int port = 3306;
        public string database = "DMPServerList";
        public string username = "USERNAME";
        public string password = "PASSWORD";

        public void LoadFromFile(string fileName)
        {
            if (!File.Exists(fileName))
            {
                SaveToFile(fileName);
            }
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load(fileName);
            host = xmlDoc.DocumentElement.GetElementsByTagName("host")[0].InnerText;
            port = Int32.Parse(xmlDoc.DocumentElement.GetElementsByTagName("port")[0].InnerText);
            database = xmlDoc.DocumentElement.GetElementsByTagName("database")[0].InnerText;
            username = xmlDoc.DocumentElement.GetElementsByTagName("username")[0].InnerText;
            password = xmlDoc.DocumentElement.GetElementsByTagName("password")[0].InnerText;
        }

        public void SaveToFile(string fileName)
        {
            string newFile = fileName + ".new";
            if (File.Exists(newFile))
            {
                File.Delete(newFile);
            }
            XmlDocument xmlDoc = new XmlDocument();
            XmlElement settingsElement = xmlDoc.CreateElement("settings");
            xmlDoc.AppendChild(settingsElement);
            //Settings
            XmlElement hostElement = xmlDoc.CreateElement("host");
            hostElement.InnerText = host;
            settingsElement.AppendChild(hostElement);
            XmlElement portElement = xmlDoc.CreateElement("port");
            portElement.InnerXml = port.ToString();
            settingsElement.AppendChild(portElement);
            XmlElement databaseElement = xmlDoc.CreateElement("database");
            databaseElement.InnerXml = database;
            settingsElement.AppendChild(databaseElement);
            XmlElement usernameElement = xmlDoc.CreateElement("username");
            usernameElement.InnerXml = username;
            settingsElement.AppendChild(usernameElement);
            XmlElement passwordElement = xmlDoc.CreateElement("password");
            passwordElement.InnerXml = password;
            settingsElement.AppendChild(passwordElement);
            //Save
            xmlDoc.Save(newFile);
            File.Move(newFile, fileName);
        }

        public string GetConnectionString()
        {
            string retString = "Server=" + host + ";";
            retString += "Port=" + port + ";";
            retString += "Database=" + database + ";";
            retString += "Uid=" + username + ";";
            retString += "Pwd=" + password + ";";
            return retString;
        }
    }
}

