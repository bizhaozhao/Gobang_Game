using System;
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class PlayerTests
{

    [Test]
    public void PieceIsFoundSameLocation()
    {
        //set up
        int row1 = 3;
        int column1 = 7;

        int row2 = 9;
        int column2 = 2;

        GameObject gameObject = new GameObject();
        Piece piece1 = gameObject.AddComponent<Piece>();
        Piece piece2 = gameObject.AddComponent<Piece>();

        piece1.row = 3;
        piece1.column = 7;

        piece2.row = 9;
        piece2.column = 2;

        List<Piece> currentPieceList = new List<Piece> { piece1, piece2 };


        Player player = gameObject.AddComponent<Player>();
        
        //action
        bool result1= player.SameLocation(currentPieceList, row1, column1);
        bool result2 = player.SameLocation(currentPieceList, row2, column2);

        //assert
        Assert.AreEqual(true, result1);
        Assert.AreEqual(true, result2);
    }

    [Test]
    public void PieceIsNotOutside()
    {
        //set up
        int row1 = 0;
        int column1 = 0;

        int row2 = 14;
        int column2 = 14;

        GameObject gameObject = new GameObject();
        Player player = gameObject.AddComponent<Player>();

        //action
        bool result1 = player.CheckPieceboardBorder( row1, column1);
        bool result2 = player.CheckPieceboardBorder( row2, column2);
        //assert
        Assert.AreEqual(true, result1);
        Assert.AreEqual(true, result2);
    }



    [Test]
    public void PieceIsFoundSameLocation1()
    {
        //set up
        List<Piece> currentPieceList = new List<Piece> { new Piece { row = 5, column = 10 } };
        int row = 5;
        int column = 10;

        var target = new LocalGoPlayer { MyFunc =  direction =>
        {
            if (direction == Direction.Up)
            {
                return currentPieceList;
            }

            return new List<Piece>();
        }
        };

        //action
        bool result = target.SameLocation(currentPieceList, row, column);
        

        //assert
        Assert.AreEqual(true, result);

    }


    private class LocalGoPlayer : Player
    {
        //public List<Piece> MyResult;

        public Func<Direction, List<Piece>> MyFunc;

        protected override List<Piece> GetSamePieceByDirection(List<Piece> currentColorList, Piece currentPiece, Direction direction)
        {
            return MyFunc(direction);
        }
    }
}
