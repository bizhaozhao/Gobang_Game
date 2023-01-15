using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//step1
using Photon.Pun;
using Photon.Realtime;
using System;
using UnityEngine.UI;

//step2: inherit 
public class NetworkManager : MonoBehaviourPunCallbacks
{
    public GameObject player;
    public PieceColor playerTurn = PieceColor.Black;

    public GameStatus gameStatus = GameStatus.Ready;
    public Text gameOverText;

    // Start is called before the first frame update
    void Start()
    {
        //step3: connect to server
        PhotonNetwork.ConnectUsingSettings();
    }

    public override void OnConnectedToMaster()
    {

        try
        {
            base.OnConnectedToMaster();
            print("conncet to server successfully!");
        }
        catch(Exception ex)
        {
            print("ex: " + ex.Message);
        }

        //step4: set up or join room
        RoomOptions roomOptions = new RoomOptions();
        roomOptions.MaxPlayers = 2;
        PhotonNetwork.JoinOrCreateRoom("Sarah's Gobang room", roomOptions, TypedLobby.Default);

    }

    public override void OnJoinedRoom()
    {
        try
        {
            base.OnJoinedRoom();
            print("joined room successfully!");
        }
        catch (Exception ex)
        {
            print("ex: " + ex.Message);
        }

        //step5: create net player
        //Player克隆
        if (player == null) return;
        //initial setting 创建玩家
        GameObject newPlayer = PhotonNetwork.Instantiate(player.name,Vector3.zero,player.transform.rotation) ;//clone function
        if (PhotonNetwork.IsMasterClient)
        {
            newPlayer.GetComponent<Player>().pieceColor = PieceColor.Black;
        }
        else
        {
            newPlayer.GetComponent<Player>().pieceColor = PieceColor.White;
        }
        
    }

    [PunRPC]
    public void ChangeTurn()
    {

        if (playerTurn == PieceColor.Black)
        {
            playerTurn = PieceColor.White;
        }
        else
        {
            playerTurn = PieceColor.Black;
        }


    }

    [PunRPC]
    public void GameOver()
    {
        gameStatus = GameStatus.GameOver;
        if (gameOverText)
        {
            gameOverText.gameObject.SetActive(true);
            print("Game End!!!");
        }

    }

    public enum GameStatus
    {
        Ready = 1,
        GameOver = 3,
    }


}
