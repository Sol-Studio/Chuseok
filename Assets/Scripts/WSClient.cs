using System;
using System.Collections.Generic;
using SocketIOClient;
using SocketIOClient.Newtonsoft.Json;
using UnityEngine;


public class WSClient : MonoBehaviour
{
    public string version = "1.0.0a1";
    public SocketIOUnity socket;

    public int myTeam;
    public string myAnimal;
    public Room room;


    // Start is called before the first frame update
    void Start()
    {
        DontDestroyOnLoad(this);

        var uri = new Uri("ws://114.207.98.231:8000");
        socket = new SocketIOUnity(uri, new SocketIOOptions
        {
            Query = new Dictionary<string, string>
                {
                    {"version", version },
                    {"client", "GAME"}
                }
            ,
            EIO = 4
            ,
            Transport = SocketIOClient.Transport.TransportProtocol.WebSocket
        });
        socket.JsonSerializer = new NewtonsoftJsonSerializer();

        // reserved socketio events
        socket.OnConnected += (sender, e) =>
        {
            Debug.Log("socket.OnConnected");
        };

        socket.OnPing += (sender, e) =>
        {
            Debug.Log("Ping");
        };
        socket.OnPong += (sender, e) =>
        {
            Debug.Log("Pong: " + e.TotalMilliseconds);
        };
        socket.OnDisconnected += (sender, e) =>
        {
            ToastMessage.Instrance.showMessage("서버와 연결이 끊어졌습니다.", 3f);
            Debug.Log("disconnect: " + e);
        };
        socket.OnReconnectAttempt += (sender, e) =>
        {
            ToastMessage.Instrance.showMessage("서버에 연결 재시도중... (" + e.ToString() + ")", 3f);
            Debug.Log($"{DateTime.Now} Reconnecting: attempt = {e}");
        };
        Debug.Log("Connecting...");
        socket.Connect();
    }

    public void Emit(string eventName, string data)
    {
        socket.EmitStringAsJSON(eventName, data);
    }

    public void JoinRoom(string roomId)
    {
        Emit("join_room", JsonUtility.ToJson(new JoinRoom(roomId)));
    }
    public void CancelJoinRoom()
    {
        Emit("cancel_join_room", null);
    }

}
public class JoinRoom
{
    public string roomId;

    public JoinRoom(string roomId)
    {
        this.roomId = roomId;
    }
}
public class RoomWaiting
{
    public int players;
}
public class Turn
{
    public int team;
    public string[] notes;
    public string songpyeon;
    public int time;
}
public class Team
{
    public int id;
    public string[] players;
    public int lives;
}
public class Room
{
    public string id;
    public string status;
    public string[] players;
    public Team[] teams;
    public Turn turn;
}
public class Note
{
    public int team;
    public string note;
    public Note(int team, string note)
    {
        this.team = team;
        this.note = note;
    }

}
public class GameEnd
{
    public int winner;
}