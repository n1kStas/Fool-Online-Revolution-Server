using LiteNetLib;
using LiteNetLib.Utils;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameRoom
{
    public int ID;
    private string _betCount;
    private int _playerCount;
    private int _maxPlayerCount;
    private string _abilityToTransferToFightOffAnotherPlayer;
    private string _whoCanThrowCards;
    private string _hiddenFool;
    private string _deck;
    private int _confirmedPlayers;

    public GameObject roomCreator;
    public List<GameObject> players = new List<GameObject>();
    public MainManager mainManager;

    public List<Card> deckOfCards = new List<Card>();
    public string trumpSuit;
    public Card trump;
    public List<Card> cardsOnTheTable = new List<Card>();

    public GameObject attacker;
    public GameObject defender;
    private bool _defenderTakesCards;
    public Text logger;

    void Update()
    {
        Debug.Log("карт на столе " + cardsOnTheTable.Count);
    }

    public void CallSorter(GameObject player, string[] reader)
    {
        switch (reader[1])
        {
            case ("ThePlayerConnectsToTheGame"):
                ThePlayerConnectsToTheGame(player);
                break;

            case ("ThePlayerIsReadyToPlay"):
                ThePlayerIsReadyToPlay();
                break;

            case ("AttackCard"):
                AttackCard(player, reader);
                break;

            case ("DefendCard"):
                DefendCard(player, reader);
                break;

            case ("DiscardPile"):
                DiscardPile();
                break;

            case ("ToAbandonTheDefense"):
                ToAbandonTheDefense();
                break;
        }
    }

    public void ThePlayerConnectsToTheGame(GameObject client)
    {
        client.GetComponent<Player>().idRoom = ID;
        NetDataWriter writer = new NetDataWriter();
        string[] callMethod = new[] { "FindGame", "ConnectionComplited", ID.ToString() };
        writer.PutArray(callMethod);
        client.GetComponent<Player>().client.Send(writer, DeliveryMethod.ReliableSequenced);
        AddClient(client);
    }

    public void AddClient(GameObject player)
    {
        _playerCount++;
        InformAllPlayersAboutANewConnectedPlayer(player);
        players.Add(player);
        if (_playerCount == _maxPlayerCount)
        {
            ConfirmReadiness();
        }
    }

    public void SetGameMode(GameObject player, string[] reader)
    {
        AddClient(player);
        roomCreator = player;
        _betCount = reader[2];
        _maxPlayerCount = int.Parse(reader[3]);
        _abilityToTransferToFightOffAnotherPlayer = reader[4];
        _whoCanThrowCards = reader[5];
        _hiddenFool = reader[6];
        _deck = reader[7];
    }

    public string[] GetGameMode()
    {
        string[] gameMode = { roomCreator.GetComponent<Player>().login, _betCount, _maxPlayerCount.ToString(), _playerCount.ToString(), _abilityToTransferToFightOffAnotherPlayer, _whoCanThrowCards, _hiddenFool, _deck };
        return gameMode;
    }

    public void InformAllPlayersAboutANewConnectedPlayer(GameObject player)
    {
        foreach (var clientOfList in players)
        {
            NetDataWriter writer = new NetDataWriter();
            List<string> callMethod = new List<string> { "GameRoom", "ShowNewConnectedPlayer", player.GetComponent<Player>().login };
            callMethod.AddRange(mainManager.myDataBasaManager.GetInformationPlayer(player.GetComponent<Player>().login));
            writer.PutArray(callMethod.ToArray());
            clientOfList.GetComponent<Player>().client.Send(writer, DeliveryMethod.ReliableSequenced);
        }
        foreach (var clientOfList in players)
        {
            NetDataWriter writer = new NetDataWriter();
            List<string> callMethod = new List<string> { "GameRoom", "ShowAlreadyConnectedPlayers", clientOfList.GetComponent<Player>().login };
            callMethod.AddRange(mainManager.myDataBasaManager.GetInformationPlayer(clientOfList.GetComponent<Player>().login));
            writer.PutArray(callMethod.ToArray());
            player.GetComponent<Player>().client.Send(writer, DeliveryMethod.ReliableSequenced);
        }
    }

    public void PlayerLeaveTheRoom(GameObject player)
    {
        players.Remove(player);
        _confirmedPlayers = 0;
        _playerCount--;
        if (player == roomCreator && _playerCount != 0)
        {
            roomCreator = players[Random.Range(0, players.Count)];

        }
        foreach (GameObject mPlayer in players)
        {
            NetDataWriter writer = new NetDataWriter();
            string[] callMethod = { "GameRoom", "EnemyLeaveTheRoom", player.GetComponent<Player>().login };
            writer.PutArray(callMethod);
            mPlayer.GetComponent<Player>().client.Send(writer, DeliveryMethod.ReliableSequenced);
        }
    }

    public bool FreeRoom()
    {
        if (_playerCount < _maxPlayerCount)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    public void ConfirmReadiness()
    {
        foreach (var player in players)
        {
            NetDataWriter writer = new NetDataWriter();
            string[] callMethod = { "GameRoom", "ConfirmReadiness" };
            writer.PutArray(callMethod);
            player.GetComponent<Player>().client.Send(writer, DeliveryMethod.ReliableSequenced);
        }
    }

    public void ThePlayerIsReadyToPlay()
    {
        _confirmedPlayers++;
        if (_confirmedPlayers == _maxPlayerCount)
        {
            StartGame();
        }
    }

    private void StartGame()
    {
        CreateADeckOfCards();
        ShuffleTheDeck();
        HandOutMissingCards();
        SetTrump();
        attacker = SelectFirstAttacker();
        Defender(attacker);
        WaitingPlayers(attacker);
    }

    private void CreateADeckOfCards()
    {
        string[] suits;
        string[] rank;

        switch (_deck)
        {
            case ("24"):

                suits = new string[] { "Hearts", "Clubs", "Diamonds", "Spades" };
                rank = new string[] { "9", "10", "Jack", "Queen", "King", "Ace" };

                for (int i = 0; i < suits.Length; i++)
                {
                    for (int j = 0; j < rank.Length; j++)
                    {
                        Card card = new Card();
                        card.suit = suits[i];
                        card.rank = rank[j];
                        card.SetValue();
                        deckOfCards.Add(card);
                    }
                }

                break;

            case ("36"):

                suits = new string[] { "Hearts", "Clubs", "Diamonds", "Spades" };
                rank = new string[] { "6", "7", "8", "9", "10", "Jack", "Queen", "King", "Ace" };

                for (int i = 0; i < suits.Length; i++)
                {
                    for (int j = 0; j < rank.Length; j++)
                    {
                        Card card = new Card();
                        card.suit = suits[i];
                        card.rank = rank[j];
                        card.SetValue();
                        deckOfCards.Add(card);
                    }
                }
                break;

            case ("52"):

                suits = new string[] { "Hearts", "Clubs", "Diamonds", "Spades" };
                rank = new string[] { "2", "3", "4", "5", "6", "7", "8", "9", "10", "Jack", "Queen", "King", "Ace" };

                for (int i = 0; i < suits.Length; i++)
                {
                    for (int j = 0; j < rank.Length; j++)
                    {
                        Card card = new Card();
                        card.suit = suits[i];
                        card.rank = rank[j];
                        card.SetValue();
                        deckOfCards.Add(card);
                    }
                }
                break;
        }


    }

    private void ShuffleTheDeck()
    {
        int theNumberOfCardsInTheDeck = deckOfCards.Count;

        for (int i = 0; i < theNumberOfCardsInTheDeck; i++)
        {
            int j = Random.Range(0, theNumberOfCardsInTheDeck);
            Card card1 = deckOfCards[i];
            Card card2 = deckOfCards[j];
            deckOfCards[i] = card2;
            deckOfCards[j] = card1;
        }


    }

    private void SetTrump()
    {
        trump = deckOfCards[0];
        deckOfCards.Remove(trump);
        deckOfCards.Add(trump);
        trumpSuit = trump.suit;

        foreach (var player in players)
        {
            NetDataWriter writer = new NetDataWriter();
            string[] callMethod = { "GameRoom", "GetTrump", trumpSuit, trump.suit + " " + trump.rank };
            writer.PutArray(callMethod);
            player.GetComponent<Player>().client.Send(writer, DeliveryMethod.ReliableSequenced);
        }


    }

    private GameObject SelectFirstAttacker()
    {
        GameObject firstAttacker = null;
        foreach (var player in players)
        {
            try
            {
                if (player.GetComponent<Player>().LowTrump(trumpSuit).suit != "")
                {
                    if (firstAttacker == null)
                    {
                        firstAttacker = player;
                    }
                    else
                    {
                        if (player.GetComponent<Player>().LowTrump(trumpSuit).numberRank < firstAttacker.GetComponent<Player>().LowTrump(trumpSuit).numberRank)
                        {
                            firstAttacker = player;
                        }
                    }
                }
            }
            catch
            {

            }
        }
        if (firstAttacker == null)
        {
            firstAttacker = players[0];
        }

        NetDataWriter writer = new NetDataWriter();
        string[] callMethod = { "GameRoom", "Attacker" };
        writer.PutArray(callMethod);
        firstAttacker.GetComponent<Player>().client.Send(writer, DeliveryMethod.ReliableSequenced);

        return firstAttacker;
    }

    private void Attacker(GameObject player)
    {
        attacker = player;
        NetDataWriter writer = new NetDataWriter();
        string[] callMethod = { "GameRoom", "Attacker" };
        writer.PutArray(callMethod);
        attacker.GetComponent<Player>().client.Send(writer, DeliveryMethod.ReliableSequenced);
    }

    private void Defender(GameObject attacker)
    {
        for (int i = 0; i < players.Count; i++)
        {
            if (attacker == players[i])
            {
                if (i + 1 == players.Count)
                {
                    defender = players[0];
                }
                else
                {
                    defender = players[i + 1];
                }
                NetDataWriter writer = new NetDataWriter();
                string[] callMethod = { "GameRoom", "Defender" };
                writer.PutArray(callMethod);
                defender.GetComponent<Player>().client.Send(writer, DeliveryMethod.ReliableSequenced);
                break;
            }
        }
    }

    private void WaitingPlayers(GameObject attacker)
    {
        foreach (GameObject player in players)
        {
            if (player != attacker && player != defender)
            {
                NetDataWriter writer = new NetDataWriter();
                string[] callMethod = { "GameRoom", "Waiting" };
                writer.PutArray(callMethod);
                player.GetComponent<Player>().client.Send(writer, DeliveryMethod.ReliableSequenced);
            }
        }
    }

    private void AttackCard(GameObject attacker, string[] reader)
    {
        Card card = new Card();
        card.suit = reader[3];
        card.rank = reader[4];
        card.SetValue();
        attacker.GetComponent<Player>().RemoveCard(card);
        cardsOnTheTable.Add(card);

        foreach (GameObject player in players)
        {
            if(attacker != player)
            {
                NetDataWriter writer = new NetDataWriter();
                string[] callMethod = { "GameRoom", "AttackCard", reader[3] + " " + reader[4] };
                writer.PutArray(callMethod);
                player.GetComponent<Player>().client.Send(writer, DeliveryMethod.ReliableSequenced);
            }
        }
    }

    private void DefendCard(GameObject defender, string[] reader)
    {
        Card card = new Card();
        card.suit = reader[3];
        card.rank = reader[4];
        card.SetValue();
        defender.GetComponent<Player>().RemoveCard(card);
        cardsOnTheTable.Add(card);

        foreach (GameObject player in players)
        {
            if (defender != player)
            {
                NetDataWriter writer = new NetDataWriter();
                string[] callMethod = { "GameRoom", "DefendCard", reader[3] + " " + reader[4], reader[5], reader[6]};
                writer.PutArray(callMethod);
                player.GetComponent<Player>().client.Send(writer, DeliveryMethod.ReliableSequenced);
            }
        }
    }

    private void ClearGameTable()
    {
        cardsOnTheTable.Clear();
        foreach (var player in players)
        {
            NetDataWriter writer = new NetDataWriter();
            string[] callMethod = { "GameRoom", "ClearGameTable" };
            writer.PutArray(callMethod);
            player.GetComponent<Player>().client.Send(writer, DeliveryMethod.ReliableSequenced);
        }
    }

    private void HandOutMissingCards()
    {
        foreach(GameObject player in players)
        {
            
            if(player.GetComponent<Player>().cardsInHand.Count < 6)
            {
                while (true)
                {
                    if (deckOfCards.Count > 0)
                    {
                        Card card = deckOfCards[0];
                        player.GetComponent<Player>().AddCard(card);
                        deckOfCards.RemoveAt(0);
                        NetDataWriter writer = new NetDataWriter();
                        string[] callMethod = { "GameRoom", "GetMissingCard", card.suit + " " + card.rank };
                        writer.PutArray(callMethod);
                        player.GetComponent<Player>().client.Send(writer, DeliveryMethod.ReliableSequenced);
                        if (player.GetComponent<Player>().cardsInHand.Count == 6)
                        {
                            break;
                        }
                    }
                    else
                    {
                        break;
                    }
                }
            }
            logger.text = deckOfCards.Count.ToString();
        }
        foreach (GameObject player in players)
        {
            NetDataWriter writer = new NetDataWriter();
            string[] callMethod = { "GameRoom", "NumberOfRemainingCard", deckOfCards.Count.ToString() };
            writer.PutArray(callMethod);
            player.GetComponent<Player>().client.Send(writer, DeliveryMethod.ReliableSequenced);
        }
    }

    private void DiscardPile()
    {
        if (!_defenderTakesCards)
        {
            ClearGameTable();
            HandOutMissingCards();
            FindAWinner();
            FindALoser();
            Attacker(defender);
            Defender(attacker);
        }
        else
        {
            DefenderTakesTheCardsFromTheTable();
        }
    }

    private void DefenderTakesTheCardsFromTheTable()
    {
        for(int i = 0; i < cardsOnTheTable.Count; i++)
        {
            Debug.Log(cardsOnTheTable[i].suit + " " + cardsOnTheTable[i].rank);
        }
        NetDataWriter writer = new NetDataWriter();
        string[] callMethod = { "GameRoom", "DefenderTakesTheCardsFromTheTable" };
        writer.PutArray(callMethod);
        defender.GetComponent<Player>().client.Send(writer, DeliveryMethod.ReliableSequenced);
        _defenderTakesCards = false;
        foreach (Card card in cardsOnTheTable)
        {
            defender.GetComponent<Player>().AddCard(card);
        }
        cardsOnTheTable.Clear();
        foreach (GameObject player in players)
        {
            if (player != defender)
            {
                ClearGameTable();
            }
        }
        HandOutMissingCards();
        FindAWinner();
        FindALoser();
        for (int i = 0; i < players.Count; i++)
        {
            if (players[i] == defender)
            {
                if (i + 1 == players.Count)
                {
                    Attacker(players[0]);
                }
                else
                {
                    Attacker(players[i + 1]);
                }
                break;
            }
        }
        Defender(attacker);
    }

    private void ToAbandonTheDefense()
    {
        _defenderTakesCards = true;
        foreach (GameObject player in players)
        {
            if (player != defender)
            {
                NetDataWriter writer = new NetDataWriter();
                string[] callMethod = { "GameRoom", "DefenderTakesTheCards" };
                writer.PutArray(callMethod);
                player.GetComponent<Player>().client.Send(writer, DeliveryMethod.ReliableSequenced);
            }
        }
    }

    private void FindAWinner()
    {
        foreach (GameObject player in players)
        {
            if(player.GetComponent<Player>().cardsInHand.Count == 0)
            {
                NetDataWriter writer = new NetDataWriter();
                string[] callMethod = { "GameRoom", "Win"};
                writer.PutArray(callMethod);
                player.GetComponent<Player>().client.Send(writer, DeliveryMethod.ReliableSequenced);
                players.Remove(player);
            }
        }
    }

    private void FindALoser()
    {
        if(players.Count == 1)
        {
            NetDataWriter writer = new NetDataWriter();
            string[] callMethod = { "GameRoom", "Lose" };
            writer.PutArray(callMethod);
            players[0].GetComponent<Player>().client.Send(writer, DeliveryMethod.ReliableSequenced);
            players.Remove(players[0]);
        }
    }
}
