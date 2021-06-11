using LiteNetLib;
using LiteNetLib.Utils;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Data.Common;

public class MainManager : MonoBehaviour
{
    public ServerManager serverManager;
    public MyDataBasaManager myDataBasaManager;

    public Text statusServer;

    public Text logger;

    public void StartServer()
    {
        serverManager.StartServer();
        statusServer.text = "Сервер запущен";
        statusServer.color = Color.green;
    }

    public void StopServer()
    {
        serverManager.StopServer();
        statusServer.text = "Сервер отключен";
        statusServer.color = Color.red;
    }

    public void CallSorter(NetPeer client ,string[] reader)
    {
        switch (reader[1])
        {
            case ("MakeAConnection"):
                NetDataWriter writer = new NetDataWriter();
                string[] callMethod = new[] { "MainManager", "ConnectionComplited" };
                writer.PutArray(callMethod);
                client.Send(writer, DeliveryMethod.ReliableSequenced);
                break;

            case ("RegisterNewPlayer"):
                RegisterNewPlayer(reader[2], reader[3], reader[4], client);
                break;

            case ("AuthenticationPlayer"):
                AuthenticationPlayer(reader[2], reader[3], client);
                break;
        }
    }

    private void RegisterNewPlayer(string login, string gender, string avatar,NetPeer client)
    { 
        string symbols = "1234567890QWERTYUIOPASDFGHJKLZXCVBNM!@#$%^&*()qwertyuiopasdfghjklzxcvbnm[]{}/?";
        string password = "";
        for (int i = 0; i < 20; i++)
        {
            password += symbols[Random.Range(0, symbols.Length)];
        }
        if(myDataBasaManager.RegisterNewPlayer(login, password, gender, 5000, 100, 1, 0, avatar, "defaultFrame"))
        {
            NetDataWriter writer = new NetDataWriter();
            string[] callMethod = new[] { "MainManager", "RegistrationComplited", login, password, gender, "5000", "100", "1", "0", avatar, "defaultFrame"};
            writer.PutArray(callMethod);
            client.Send(writer, DeliveryMethod.ReliableSequenced);
            AuthenticationPlayer(login, password, client);
        }
        else
        {
            NetDataWriter writer = new NetDataWriter();
            string[] callMethod = new[] { "MainManager", "RegistrationError" };
            writer.PutArray(callMethod);
            client.Send(writer, DeliveryMethod.ReliableSequenced);
        }
    }

    private void AuthenticationPlayer(string login, string password, NetPeer client)
    {
        List<DbDataRecord> dataPlayer = myDataBasaManager.AuthenticationPlayer(login);
        try
        {
            if (dataPlayer == null || dataPlayer[0]["password"].ToString() != password)
            {
                NetDataWriter writer = new NetDataWriter();
                string[] callMethod = new[] { "MainManager", "AuthenticationError" };
                writer.PutArray(callMethod);
                client.Send(writer, DeliveryMethod.ReliableSequenced);
            }
            else
            {
                NetDataWriter writer = new NetDataWriter();
                string[] callMethod = new[] { "MainManager", "AuthenticationSuccessful", dataPlayer[0]["gender"].ToString(), dataPlayer[0]["cotbucks"].ToString(), dataPlayer[0]["hotbucks"].ToString(), dataPlayer[0]["level"].ToString(), dataPlayer[0]["levelProgress"].ToString(), dataPlayer[0]["avatar"].ToString(), dataPlayer[0]["frame"].ToString()};
                writer.PutArray(callMethod);
                client.Send(writer, DeliveryMethod.ReliableSequenced);
                serverManager.PlayerConnected(client, login);
            }
        }
        catch
        {
            NetDataWriter writer = new NetDataWriter();
            string[] callMethod = new[] { "MainManager", "AuthenticationError" };
            writer.PutArray(callMethod);
            client.Send(writer, DeliveryMethod.ReliableSequenced);
        }
    }
}
