using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LiteNetLib;
using LiteNetLib.Utils;
using System.Net;
using System.Net.Sockets;
using System;
using UnityEngine.UI;
using System.Linq;

public class ServerManager : MonoBehaviour, INetEventListener, INetLogger
{
    public MainManager mainManager;
    public ControlRooms controlRooms;
    private NetManager server;
    public string key;
    public bool enabledServer;
    public Text numberOfPlayersOnline;

    List<GameObject> clientsCollection = new List<GameObject>();
    List<NetPeer> clients = new List<NetPeer>();
    public List<GameObject> players = new List<GameObject>();
    public GameObject collection;

    public void StartServer()
    {
        if (!enabledServer)
        {
            NetDebug.Logger = this;
            server = new NetManager(this);
            server.Start(5005);
            server.BroadcastReceiveEnabled = true;
            server.UpdateTime = 15;
            enabledServer = true;
        }
    }

    public void StopServer()
    {
        if (enabledServer)
        {
            server.Stop();
            enabledServer = false;
        }
    }

    void Update()
    {
        if (enabledServer)
        {
            server.PollEvents();
            numberOfPlayersOnline.text = "Онлайн: " + clients.Count;
        }
        else
        {
            numberOfPlayersOnline.text = "Онлайн: -";
        }
        
    }

    public void OnPeerConnected(NetPeer peer)
    {

    }

    public void OnNetworkError(IPEndPoint endPoint, SocketError socketErrorCode)
    {
        Debug.Log("[SERVER] error " + socketErrorCode);
    }

    public void OnNetworkReceiveUnconnected(IPEndPoint remoteEndPoint, NetPacketReader reader,
        UnconnectedMessageType messageType)
    {
        if (messageType == UnconnectedMessageType.Broadcast)
        {
            Debug.Log("[SERVER] Received discovery request. Send discovery response");
            NetDataWriter resp = new NetDataWriter();
            resp.Put(1);
            server.SendUnconnectedMessage(resp, remoteEndPoint);
        }
    }

    public void OnNetworkLatencyUpdate(NetPeer client, int latency)
    {
    }

    public void OnConnectionRequest(ConnectionRequest request)
    {
        request.AcceptIfKey(key);
    }

    public void OnPeerDisconnected(NetPeer client, DisconnectInfo disconnectInfo)
    {
        ClientDisconnected(client);
    }

    public void OnNetworkReceive(NetPeer client, NetPacketReader reader, DeliveryMethod deliveryMethod) //получение данных от клиента
    {
        string[] mReader = reader.GetStringArray();       
        switch (mReader[0])
        {
            case ("MainManager"):
                mainManager.CallSorter(client, mReader);
                break;
            case ("ControlRooms"):
                GameObject mClient = null;
                foreach (GameObject soughtFor in clientsCollection)
                {                     
                    if (client == soughtFor.GetComponent<Player>().client)
                    {                        mClient = soughtFor;
                        break;
                    }
                }
                controlRooms.CallSorter(mClient, mReader);
                break;

            case ("GameRoom"):
                foreach (GameRoom gameRoom in controlRooms.gameRooms)
                {
                    if (gameRoom.ID.ToString() == mReader[2])
                    {
                        mClient = null;
                        foreach (GameObject soughtFor in clientsCollection)
                        {
                            if (client == soughtFor.GetComponent<Player>().client)
                            {
                                mClient = soughtFor;
                                break;
                            }
                        }
                        gameRoom.CallSorter(mClient, mReader);
                    }
                }
                break;
        }
    }

    public void WriteNet(NetLogLevel level, string str, params object[] args)
    {
        Debug.LogFormat(str, args);
    }

    public void PlayerConnected(NetPeer peer, string login)
    {
        clients.Add(peer);
        GameObject client = new GameObject(login);
        players.Add(client);
        client.AddComponent<Player>();
        client.GetComponent<Player>().client = peer;
        client.GetComponent<Player>().login = login;
        clientsCollection.Add(client);
        client.transform.parent = collection.transform;
    }

    private void ClientDisconnected(NetPeer peer)
    {
        foreach (GameObject client in clientsCollection)
        {
            if(client.GetComponent<Player>().client == peer)
            {
                controlRooms.ThePlayerLeftTheGame(client);
                clients.Remove(peer);
                players.Remove(players.Where(p => p.GetComponent<Player>().client == peer).First());
                clientsCollection.Remove(client);
                Destroy(client);
                break;
            }
        }
    }
}
