using System.Collections;
using System.Collections.Generic;
using System.Data;
using UnityEngine;
using System.Data.Common;

public class MyDataBasaManager : MonoBehaviour
{
    public bool RegisterNewPlayer(string login, string password, string gender, int cotbucks, int hotbucks, int level, double levelProgress, string avatar, string frame)
    {
        try
        {
            MyDataBase.RegisterNewPlayer($"INSERT INTO PlayersInformation (login, password, gender, cotbucks, hotbucks, level, levelProgress, avatar, frame) " +
                $"VALUES ('{login}', '{password}', '{gender}', '{cotbucks}', '{hotbucks}', '{level}', '{levelProgress}', '{avatar}', '{frame}');");
            return true;
        }
        catch
        {
            return false;
        }
    }

    public List<DbDataRecord> AuthenticationPlayer(string login)
    {
        try
        {
            List<DbDataRecord> dataPlayer = MyDataBase.GetDataPlayer($"SELECT * FROM PlayersInformation WHERE login = '{login}';");
            return dataPlayer;
        }
        catch
        {
            return null;
        }
    }

    public List<string> GetInformationPlayer(string login)
    {
        List<string> playerInformation = new List<string>();
        List<DbDataRecord> dataPlayer = MyDataBase.GetDataPlayer($"SELECT avatar, frame FROM PlayersInformation WHERE login = '{login}';");
        playerInformation.Add(dataPlayer[0]["avatar"].ToString());
        playerInformation.Add(dataPlayer[0]["frame"].ToString());
        return playerInformation;
    }
}
