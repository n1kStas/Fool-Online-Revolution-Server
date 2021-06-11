using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LiteNetLib;
using LiteNetLib.Utils;

public class ControlRooms : MonoBehaviour
{
    public int lastIDRoom;
    public List<GameRoom> gameRooms = new List<GameRoom>();
    public MainManager mainManager;

    public void CallSorter(GameObject client, string[] reader)
    {        
        switch (reader[1])
        {
            case ("CreateNewRoom"):
                CreateNewRoom(client, reader);
                break;

            case ("FindFreeRoom"):
                SendClientIDFreeRoom(client);
                break;

            case ("FindOutInformationAboutTheRoom"):
                SendRoomInformation(client, reader);
                break;


        }
    }

    private void CreateNewRoom(GameObject client, string[] reader)
    {
        GameRoom gameRoom = new GameRoom();
        gameRoom.mainManager = mainManager;
        gameRoom.logger = mainManager.logger;
        gameRoom.ID = SetIDRoom();
        client.GetComponent<Player>().idRoom = gameRoom.ID;
        gameRoom.SetGameMode(client, reader);
        gameRooms.Add(gameRoom);

        NetDataWriter writer = new NetDataWriter();
        string[] callMethod = new[] { "CreateNewGame", "CreateRoom", gameRoom.ID.ToString()};
        writer.PutArray(callMethod);
        client.GetComponent<Player>().client.Send(writer, DeliveryMethod.ReliableSequenced);
    }

    private int SetIDRoom()
    {
        if(lastIDRoom == 9999999)
        {
            lastIDRoom = 1000000;
        }
        else
        {
            lastIDRoom++;
        }
        return lastIDRoom;
    }

    public void SendClientIDFreeRoom(GameObject client)
    {
        string idFreeRoom = "";
        foreach (GameRoom gameRoom in gameRooms)
        {
            if (gameRoom.FreeRoom())
            {
                idFreeRoom += gameRoom.ID.ToString() + "\n";
            }
        }
        NetDataWriter writer = new NetDataWriter();
        string[] callMethod = { "FindGame", "ShowFreeRoom", idFreeRoom};
        writer.PutArray(callMethod);
        client.GetComponent<Player>().client.Send(writer, DeliveryMethod.ReliableSequenced);
    }

    public void SendRoomInformation(GameObject client, string[] reader)
    {
        NetDataWriter writer = new NetDataWriter();
        List <string> callMethod = new List<string> { "FindGame", "ShowRoomInformation", reader[3]};
        GameRoom desiredRoom = null;
        foreach(GameRoom room in gameRooms)
        {
            if(room.ID == int.Parse(reader[2]))
            {
                desiredRoom = room;
                break;
            }
        }
        callMethod.AddRange(desiredRoom.GetGameMode());
        writer.PutArray(callMethod.ToArray());
        client.GetComponent<Player>().client.Send(writer, DeliveryMethod.ReliableSequenced);

    }

    public void ThePlayerLeftTheGame(GameObject client)
    {
        if(client.GetComponent<Player>().idRoom != 0)
        {
            foreach (GameRoom room in gameRooms)
            {
                if (room.ID == client.GetComponent<Player>().idRoom)
                {                    
                    room.PlayerLeaveTheRoom(client);                    
                    if (room.players.Count == 0)
                    {
                        gameRooms.Remove(room);
                    }
                    break;
                }
            }
        }
    }
}
