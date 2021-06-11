using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LiteNetLib;
using LiteNetLib.Utils;

public class Player : MonoBehaviour
{
    public NetPeer client;
    public string login;
    public int idRoom;

    public List<Card> cardsInHand = new List<Card>();

    public void AddCard(Card card)
    {
        cardsInHand.Add(card);
    }

    public Card LowTrump(string trumpSuit)
    {
        Card lowTrump = null;
        for(int i = 0; i < 6; i++)
        {
            if(cardsInHand[i].suit == trumpSuit)
            {
                try
                {
                    if (lowTrump.suit != "" && cardsInHand[i].numberRank < lowTrump.numberRank)
                    {
                        lowTrump = cardsInHand[i];
                    }
                }
                catch
                {
                    lowTrump = cardsInHand[i];
                }

            }
        }

        try
        {
            if (lowTrump.suit != "")
            {
                return lowTrump;
            }
        }
        catch
        {
            return null;
        }
        return null;
    }

    public void RemoveCard(Card card)
    {
        
        foreach (Card _card in cardsInHand)
        {
            if (_card.suit == card.suit && _card.rank == card.rank)
            {
                cardsInHand.Remove(_card);
                break;
            }
        }
    }
}
