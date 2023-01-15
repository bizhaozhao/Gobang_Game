using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using System.Linq;
using UnityEngine.Device;


public class Player : MonoBehaviour
{
    public Vector3 zeroPointPosition;//left bottom corner position
    public float gridWidth;
    public PieceColor pieceColor = PieceColor.Black;

    private PhotonView photonView;
    private int row;
    private int column;

    public GameObject black_Piece;
    public GameObject white_Piece;

    public List<Piece> currentPieceList = new List<Piece>();
    Piece currentPiece;

    // Start is called before the first frame update
    void Start()
    {
        photonView = this.GetComponent<PhotonView>();
    }

    // Update is called once per frame
    void Update()
    {
        //step1: check 如果玩家是本客户端创建的
        if (!photonView.IsMine) return;

        //step2: check player order
        if (GameObject.FindObjectOfType<NetworkManager>().playerTurn != pieceColor) return;

        //step3: check game status
        if (GameObject.FindObjectOfType<NetworkManager>().gameStatus != NetworkManager.GameStatus.Ready) return;

        //step4: check mouse click
        if (Input.GetMouseButtonDown(0))
        {
            //step5: compute row, column positon
            //convert screen coordinate to Unity coordinate
            ConvertScreenCoordinateToUnityCoordinate();
           
            //step6: check pieceboard border, avoid that the piece is deployed outside of the pieceboard           
            if (CheckPieceboardBorder(row, column) == false) return;

            //step7: check piece conflict
            currentPieceList = GameObject.FindObjectsOfType<Piece>().ToList();
            if (SameLocation(currentPieceList, row, column)) return;

            //step8: create net piece clone
            currentPiece = CreateNetPiece(row, column);

            //step9: check game over or not
            if (CheckFivePieceAndGameover()) return;

            //ste10: player take turns          
            GameObject.FindObjectOfType<NetworkManager>().gameObject.GetComponent<PhotonView>().RPC("ChangeTurn", RpcTarget.All);

        }
    }


    void ConvertScreenCoordinateToUnityCoordinate()
    {
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector3 offsetPos = mousePos - zeroPointPosition;
        row = (int) Mathf.Round(offsetPos.y / gridWidth);
        column = (int) Mathf.Round(offsetPos.x / gridWidth);
    }

    public bool CheckPieceboardBorder(int row, int column)
    {
        if (row < 0 || row > 14 || column < 0 || column > 14)
        {
            return false;
        }
        return true;

    }

    public bool SameLocation(List<Piece> currentPieceList, int row, int column)
    {
        foreach (var item in currentPieceList)
        {
            if (item.row == row && item.column == column)
            {
                return true;
            }
        }
        return false;
    }

    Piece CreateNetPiece(int row, int column)
    {
        int[] rowAndColumnValue = { row, column };
        Vector3 piecePos = new Vector3(column * gridWidth, row * gridWidth, zeroPointPosition.z) + zeroPointPosition;
        
        GameObject newPiece;
        currentPiece = gameObject.AddComponent<Piece>();

        if (pieceColor == PieceColor.Black)
        {
            if (black_Piece != null)
            {               
                newPiece = PhotonNetwork.Instantiate(black_Piece.name, piecePos, black_Piece.transform.rotation);
                //synchronize row and column for two client sides
                newPiece.GetComponent<PhotonView>().RPC("SetRowAndColumnValue", RpcTarget.All, rowAndColumnValue);
                currentPiece = newPiece.GetComponent<Piece>();
            }
        }
        else
        {
            if (white_Piece != null)
            {
                newPiece = PhotonNetwork.Instantiate(white_Piece.name, piecePos, white_Piece.transform.rotation);
                //synchronize row and column for two client sides
                newPiece.GetComponent<PhotonView>().RPC("SetRowAndColumnValue", RpcTarget.All, rowAndColumnValue);
                currentPiece = newPiece.GetComponent<Piece>();
            }
        }

        return currentPiece;

    }

    bool CheckFivePieceAndGameover()

    {
        bool isFive = CheckFivePiece(currentPieceList, currentPiece);
        if (isFive)
        {   //use RPC call GameOver method
            GameObject.FindObjectOfType<NetworkManager>().gameObject.GetComponent<PhotonView>().RPC("GameOver", RpcTarget.All);
            return true;
        }
        return false;
    }


    bool CheckFivePiece(List<Piece> currentList, Piece currentPiece)
    {
        bool result = false;
        List<Piece> currentColorList = currentList.Where(x => x.pieceColor == pieceColor).ToList();

        var upList = GetSamePieceByDirection(currentColorList, currentPiece, Direction.Up);
        var downList = GetSamePieceByDirection(currentColorList, currentPiece, Direction.Down);
        var leftList = GetSamePieceByDirection(currentColorList, currentPiece, Direction.Left);
        var rightList = GetSamePieceByDirection(currentColorList, currentPiece, Direction.Right);
        var topLeftList = GetSamePieceByDirection(currentColorList, currentPiece, Direction.TopLeft);
        var bottomRightList = GetSamePieceByDirection(currentColorList, currentPiece, Direction.BottomRight);
        var bottomLeftList = GetSamePieceByDirection(currentColorList, currentPiece, Direction.BottomLeft);
        var topRightList = GetSamePieceByDirection(currentColorList, currentPiece, Direction.TopRight);

        if (upList.Count + downList.Count + 1 >= 5 ||
            leftList.Count + rightList.Count + 1 >= 5 ||
            topLeftList.Count + bottomRightList.Count + 1 >= 5 ||
            topRightList.Count + bottomLeftList.Count + 1 >= 5)
        {
            result = true;
        }

        //print(upList.Count +" ,"+ downList.Count + " ," + leftList.Count + " ," + rightList.Count + " ," + topLeftList.Count + " ," + bottomLeftList.Count + " ," + bottomRightList.Count + " ," + topRightList.Count);
        return result;
    }

    protected virtual List<Piece> GetSamePieceByDirection(List<Piece> currentColorList, Piece currentPiece, Direction direction)
    {
        List<Piece> result = new List<Piece>();

        switch (direction)
        {
            case Direction.Up:
                foreach (var item in currentColorList)
                {
                    if (item.row == currentPiece.row + 1 && item.column == currentPiece.column)
                    {
                        result.Add(item);
                        //recursion to find if there still have same color pieces in the up direction
                        var resultList = GetSamePieceByDirection(currentColorList, item, Direction.Up);
                        result.AddRange(resultList);
                    }
                }
                break;
            case Direction.Down:
                foreach (var item in currentColorList)
                {
                    if (item.row == currentPiece.row - 1 && item.column == currentPiece.column)
                    {
                        result.Add(item);
                        //recursion
                        var resultList = GetSamePieceByDirection(currentColorList, item, Direction.Down);
                        result.AddRange(resultList);
                    }
                }
                break;
            case Direction.Left:
                foreach (var item in currentColorList)
                {
                    if (item.row == currentPiece.row && item.column == currentPiece.column - 1)
                    {
                        result.Add(item);
                        //recursion
                        var resultList = GetSamePieceByDirection(currentColorList, item, Direction.Left);
                        result.AddRange(resultList);
                    }
                }
                break;
            case Direction.Right:
                foreach (var item in currentColorList)
                {
                    if (item.row == currentPiece.row && item.column == currentPiece.column + 1)
                    {
                        result.Add(item);
                        //recursion
                        var resultList = GetSamePieceByDirection(currentColorList, item, Direction.Right);
                        result.AddRange(resultList);
                    }
                }
                break;
            case Direction.TopLeft:
                foreach (var item in currentColorList)
                {
                    if (item.row == currentPiece.row + 1 && item.column == currentPiece.column - 1)
                    {
                        result.Add(item);
                        //recursion
                        var resultList = GetSamePieceByDirection(currentColorList, item, Direction.TopLeft);
                        result.AddRange(resultList);
                    }
                }
                break;
            case Direction.BottomRight:
                foreach (var item in currentColorList)
                {
                    if (item.row == currentPiece.row - 1 && item.column == currentPiece.column + 1)
                    {
                        result.Add(item);
                        //recursion
                        var resultList = GetSamePieceByDirection(currentColorList, item, Direction.BottomRight);
                        result.AddRange(resultList);
                    }
                }
                break;
            case Direction.BottomLeft:
                foreach (var item in currentColorList)
                {
                    if (item.row == currentPiece.row - 1 && item.column == currentPiece.column - 1)
                    {
                        result.Add(item);
                        //recursion
                        var resultList = GetSamePieceByDirection(currentColorList, item, Direction.BottomLeft);
                        result.AddRange(resultList);
                    }
                }
                break;
            case Direction.TopRight:
                foreach (var item in currentColorList)
                {
                    if (item.row == currentPiece.row + 1 && item.column == currentPiece.column + 1)
                    {
                        result.Add(item);
                        //recursion
                        var resultList = GetSamePieceByDirection(currentColorList, item, Direction.TopRight);
                        result.AddRange(resultList);
                    }
                }
                break;

        }
        return result;
    }
}

public enum Direction
{
    Up = 0,
    Down = 1,
    Left = 2,
    Right = 3,
    TopLeft = 4,
    BottomRight = 5,
    BottomLeft = 6,
    TopRight = 7,

}
