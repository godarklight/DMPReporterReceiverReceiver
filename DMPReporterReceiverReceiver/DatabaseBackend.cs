using System;
using System.Collections.Generic;
using System.Data;
using MySql.Data.MySqlClient;
using MessageStream2;

namespace DMPReporterReceiverReceiver
{
    public class DatabaseBackend
    {
        private MySqlConnection mysqlConnection;
        private HashSet<int> connectedList;

        public DatabaseBackend(DatabaseSettings dbSettings)
        {
            connectedList = new HashSet<int>();
            mysqlConnection = new MySqlConnection(dbSettings.GetConnectionString());
            mysqlConnection.Open();
            RunSQL("CREATE TABLE IF NOT EXISTS DMPServerList (id INT PRIMARY KEY, serverHash VARCHAR(255), serverName VARCHAR(255), description TEXT, gamePort INT, gameAddress VARCHAR(255), protocolVersion INT, programVersion VARCHAR(255), maxPlayers INT, modcontrol INT, modControlSha VARCHAR(255), gameMode INT, cheats INT, warpMode INT, universeSize INT, banner VARCHAR(255), homepage VARCHAR(255), httpPort INT, admin VARCHAR(255),team VARCHAR(255), location VARCHAR(255), fixedIP INT, players TEXT, playerCount INT) ENGINE=InnoDB DEFAULT CHARSET=utf8", null);
        }

        private int RunSQL(string sqlText, Dictionary<string, object> parameters)
        {
            int returnValue;
            using (MySqlCommand command = new MySqlCommand(sqlText, mysqlConnection))
            {
                if (parameters != null)
                {
                    foreach (KeyValuePair<string,object> kvp in parameters)
                    {
                        command.Parameters.AddWithValue(kvp.Key, kvp.Value);
                    }
                }
                returnValue = command.ExecuteNonQuery();
            }
            return returnValue;
        }

        public void WipeDatabase()
        {
            int deleted = RunSQL("DELETE FROM DMPServerList", null);
            connectedList.Clear();
            Console.WriteLine("Deleted " + deleted + " stale entries");
        }

        public void ConnectClient(int client, string gameAddress)
        {
            if (!connectedList.Contains(client))
            {
                connectedList.Add(client);
                Console.WriteLine("Client " + client + " connected.");
                Dictionary<string, object> parameters = new Dictionary<string, object>();
                parameters.Add("id", client);
                parameters.Add("serverName", "CONNECTING");
                parameters.Add("gameAddress", gameAddress);
                parameters.Add("maxPlayers", 0);
                parameters.Add("playerCount", 0);
                RunSQL("INSERT INTO DMPServerList (id, serverName, gameAddress, maxPlayers, playerCount) VALUES (@id, @serverName, @gameAddress, @maxPlayers, @playerCount)", parameters);
            }
            else
            {
                Console.WriteLine("Client " + client + " is already connected");
            }
        }

        public void ReportClient(int client, byte[] reportBytes)
        {
            if (connectedList.Contains(client))
            {
                using (MessageReader mr = new MessageReader(reportBytes))
                {
                    string serverHash = mr.Read<string>();
                    string serverName = mr.Read<string>();
                    string description = mr.Read<string>();
                    int gamePort = mr.Read<int>();
                    string gameAddress = mr.Read<string>();
                    int protocolVersion = mr.Read<int>();
                    string programVersion = mr.Read<string>();
                    int maxPlayers = mr.Read<int>();
                    int modControl = mr.Read<int>();
                    string modControlSha = mr.Read<string>();
                    int gameMode = mr.Read<int>();
                    bool cheats = mr.Read<bool>();
                    int warpMode = mr.Read<int>();
                    long universeSize = mr.Read<long>();
                    string banner = mr.Read<string>();
                    string homepage = mr.Read<string>();
                    int httpPort = mr.Read<int>();
                    string admin = mr.Read<string>();
                    string team = mr.Read<string>();
                    string location = mr.Read<string>();
                    bool fixedIP = mr.Read<bool>();
                    string[] players = mr.Read<string[]>();
                    //Bind parameters
                    Dictionary<string, object> parameters = new Dictionary<string, object>();
                    parameters.Add("id", client);
                    parameters.Add("serverHash", serverHash);
                    parameters.Add("serverName", serverName);
                    parameters.Add("description", description);
                    parameters.Add("gamePort", gamePort);
                    parameters.Add("gameAddress", gameAddress);
                    parameters.Add("protocolVersion", protocolVersion);
                    parameters.Add("programVersion", programVersion);
                    parameters.Add("maxPlayers", maxPlayers);
                    parameters.Add("modControl", modControl);
                    parameters.Add("modControlSha", modControlSha);
                    parameters.Add("gameMode", gameMode);
                    parameters.Add("cheats", cheats);
                    parameters.Add("warpMode", warpMode);
                    parameters.Add("universeSize", universeSize);
                    parameters.Add("banner", banner);
                    parameters.Add("homepage", homepage);
                    parameters.Add("httpPort", httpPort);
                    parameters.Add("admin", admin);
                    parameters.Add("team", team);
                    parameters.Add("location", location);
                    parameters.Add("fixedIP", fixedIP);
                    parameters.Add("players", String.Join(", ", players));
                    parameters.Add("playerCount", players.Length);
                    //Build SQL text
                    string sqlText = "UPDATE DMPServerList SET ";
                    sqlText += "`serverHash`=@serverHash, ";
                    sqlText += "`serverName`=@serverName, ";
                    sqlText += "`description`=@description, ";
                    sqlText += "`gamePort`=@gamePort, ";
                    sqlText += "`gameAddress`=@gameAddress, ";
                    sqlText += "`protocolVersion`=@protocolVersion, ";
                    sqlText += "`programVersion`=@programVersion, ";
                    sqlText += "`maxPlayers`=@maxPlayers, ";
                    sqlText += "`modControl`=@modControl, ";
                    sqlText += "`modControlSha`=@modControlSha, ";
                    sqlText += "`gameMode`=@gameMode, ";
                    sqlText += "`cheats`=@cheats, ";
                    sqlText += "`warpMode`=@warpMode, ";
                    sqlText += "`universeSize`=@universeSize, ";
                    sqlText += "`banner`=@banner, ";
                    sqlText += "`homepage`=@homepage, ";
                    sqlText += "`httpPort`=@httpPort, ";
                    sqlText += "`admin`=@admin, ";
                    sqlText += "`team`=@team, ";
                    sqlText += "`location`=@location, ";
                    sqlText += "`fixedIP`=@fixedIP, ";
                    sqlText += "`players`=@players, ";
                    sqlText += "`playerCount`=@playerCount ";
                    sqlText += "WHERE `id`=@id";
                    //Run SQL
                    RunSQL(sqlText, parameters);
                    Console.WriteLine("Client " + client + " reported " + gameAddress + ":" + gamePort);
                }
            }
            else
            {
                Console.WriteLine("Client " + client + " reported without being connected.");
            }
        }

        public void DisconnectClient(int client)
        {
            if (connectedList.Contains(client))
            {
                connectedList.Remove(client);
                Console.WriteLine("Client " + client + " disconnected.");
                Dictionary<string, object> parameters = new Dictionary<string, object>();
                parameters.Add("id", client);
                RunSQL("DELETE FROM DMPServerList WHERE `id`=@id", parameters);
            }
            else
            {
                Console.WriteLine("Client " + client + " is already disconnected");
            }
        }
    }
}

