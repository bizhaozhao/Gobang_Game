using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;


public class Piece : MonoBehaviour
{

    public int row;
    public int column;
    public PieceColor pieceColor = PieceColor.Black;

 
    // Start is called before the first frame update
    void Start()
    {
    
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    [PunRPC]//overload
    public void SetRowAndColumnValue(int[] rowAndColumnValue)
    {
        if (rowAndColumnValue.Length != 2) return;
        row = rowAndColumnValue[0];
        column = rowAndColumnValue[1];
    }
}


public enum PieceColor
{
    Black = 0,
    White = 1,
}