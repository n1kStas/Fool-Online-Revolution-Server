using System.Collections;
using System.Collections.Generic;


public class Card
{
    public string suit;
    public string rank;
    public int numberRank;

    public void SetValue()
    {
        if (rank == "2")
        {
            numberRank = 0;
        }
        else if (rank == "3")
        {
            numberRank = 1;
        }
        else if (rank == "4")
        {
            numberRank = 2;
        }
        else if (rank == "5")
        {
            numberRank = 3;
        }
        else if (rank == "6")
        {
            numberRank = 4;
        }
        else if (rank == "7")
        {
            numberRank = 5;
        }
        else if (rank == "8")
        {
            numberRank = 6;
        }
        else if (rank == "9")
        {
            numberRank = 7;
        }
        else if (rank == "10")
        {
            numberRank = 8;
        }
        else if (rank == "Jack")
        {
            numberRank = 9;
        }
        else if (rank == "Queen")
        {
            numberRank = 10;
        }
        else if (rank == "King")
        {
            numberRank = 11;
        }
        else if (rank == "Ace")
        {
            numberRank = 12;
        }
    }
}
