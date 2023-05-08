using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System;

public class Lobby : MonoBehaviour
{
    [SerializeField]
    private GameObject credit;

    [SerializeField]
    private GameObject play;

    [SerializeField]
    private InputField roomId;

    [SerializeField]
    private WSClient ws;

    [SerializeField]
    private GameObject waiting;

    [SerializeField]
    private Text waitingPlayers;

    public void Join()
    {
        if (!ws.socket.Connected)
        {
            ToastMessage.Instrance.showMessage("서버에 연결되지 않았습니다.", 3f);
            return;
        }
        ws.socket.On("room_waiting", (response) =>
        {
            Debug.Log("room_waiting");
            UnityMainThreadDispatcher.Instance().Enqueue(() =>
            {
                RoomWaiting roomWaiting = response.GetValue<RoomWaiting>();
                string text = roomWaiting.players.ToString() + "/4";
                waitingPlayers.text = text;
            });
        });
        ws.socket.On("game_start", (response) =>
        {
            Debug.Log("game_start");
            UnityMainThreadDispatcher.Instance().Enqueue(() =>
            {
                Room room = response.GetValue<Room>();
                if (Array.IndexOf(room.teams[0].players, ws.socket.Id) > -1)
                {
                    ws.myTeam = 0;
                }
                else
                {
                    ws.myTeam = 1;
                }
                string[] animals = { "Pig", "Rabbit" };
                ws.myAnimal = animals[Array.IndexOf(room.teams[ws.myTeam].players, ws.socket.Id)];
                ws.room = room;
                play.transform.parent.gameObject.SetActive(false);
                SceneManager.LoadScene("Game");
            });
        });
        ws.JoinRoom(roomId.text);
        waiting.SetActive(true);
    }
    public void CancelJoining()
    {
        ws.CancelJoinRoom();
        waiting.SetActive(false);
        ws.Emit("cancel_joining", JsonUtility.ToJson(new JoinRoom(roomId.text)));
        ws.socket.Off("room_waiting");
        ws.socket.Off("game_start");
    }

    public void ActivatePlay()
    {
        play.SetActive(true);
    }
    public void DeactivatePlay()
    {
        play.SetActive(false);
    }

    public void Exit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
    }

    public void ActivateCredit()
    {
        credit.SetActive(true);
    }
    public void DeactivateCredit()
    {
        credit.SetActive(false);
    }
}
