using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class UIManager : MonoBehaviour
{
    [SerializeField]
    private GameObject buttonsParent;

    [SerializeField]
    private Transform lives;

    [SerializeField]
    private GameObject notesParent;

    private NoteButton[] buttons;

    [SerializeField]
    private GameObject notePreset;

    [SerializeField]
    private GameObject noteRowPreset;

    [SerializeField]
    private Sprite blueNote;

    [SerializeField]
    private Sprite bluegreenNote;

    [SerializeField]
    private Sprite pinkNote;

    [SerializeField]
    private Sprite greenNote;

    [SerializeField]
    private Sprite orangeNote;

    [SerializeField]
    private Sprite redNote;

    [SerializeField]
    private Sprite purpleNote;

    [SerializeField]
    private Sprite yellowNote;
    [SerializeField]
    private int nextNoteIdx;
    [SerializeField]
    private Slider timer;

    [SerializeField]
    private GameObject gameend;

    private WSClient ws;
    private GameObject buttonSet;
    private Coroutine timerCoroutine;
    private void Start()
    {
        ws = GetDontDestroyOnLoadObjects()[0].GetComponent<WSClient>();
        buttonSet = buttonsParent.transform.Find(ws.myAnimal).gameObject;
        if (ws.myTeam == 0) buttonSet.SetActive(true);

        foreach (Transform button in buttonsParent.transform.Find(ws.myAnimal))
        {
            button.GetComponent<Button>().onClick.AddListener(delegate { pressNote(button.GetComponent<NoteButton>().color); });
        }
        ws.socket.On("note", (data) =>
        {
            // note
            UnityMainThreadDispatcher.Instance().Enqueue(() =>
            {
                pressNoteUI();
            });

        });
        ws.socket.On("turn", (data) =>
        {
            // new turn
            UnityMainThreadDispatcher.Instance().Enqueue(() =>
            {
                if (data.GetValue<Turn>().team == ws.myTeam)
                    buttonSet.SetActive(true);
                else
                    buttonSet.SetActive(false);
                if (timerCoroutine != null) StopCoroutine(timerCoroutine);
                setNotes(data.GetValue<Turn>().notes);
                timerCoroutine = StartCoroutine(startTimer(data.GetValue<Turn>().time));
            });
        });
        ws.socket.On("dec_life", (data) =>
        {
            if (data.GetValue<int>() == ws.myTeam)
                // -1 치즈스틱
                UnityMainThreadDispatcher.Instance().Enqueue(() =>
                {
                    decreaseLife();
                });
        });
        ws.socket.On("game_end", (data) =>
        {
            UnityMainThreadDispatcher.Instance().Enqueue(() =>
            {
                gameend.SetActive(true);
                string txt;
                if (data.GetValue<GameEnd>().winner == ws.myTeam)
                {
                    txt = "승리";
                }
                else
                {
                    txt = "패배";
                }
                gameend.transform.GetChild(1).GetComponent<Text>().text = txt;
            });

        });
        setNotes(ws.room.turn.notes);
        if (timerCoroutine != null) StopCoroutine(timerCoroutine);
        timerCoroutine = StartCoroutine(startTimer(ws.room.turn.time));
    }
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            pressNoteUI();
        }
    }
    public static GameObject[] GetDontDestroyOnLoadObjects()
    {
        GameObject temp = null;
        try
        {
            temp = new GameObject();
            Object.DontDestroyOnLoad(temp);
            UnityEngine.SceneManagement.Scene dontDestroyOnLoad = temp.scene;
            Object.DestroyImmediate(temp);
            temp = null;

            return dontDestroyOnLoad.GetRootGameObjects();
        }
        finally
        {
            if (temp != null)
                Object.DestroyImmediate(temp);
        }
    }
    public void setNotes(string[] notes)
    {
        foreach (Transform child in notesParent.transform)
        {
            GameObject.Destroy(child.gameObject);
        }
        GameObject noteRow = new GameObject();
        for (int i = 0; i < notes.Length; i++)
        {
            if (i % 6 == 0)
            {
                noteRow = GameObject.Instantiate(noteRowPreset, parent: notesParent.transform);
            }

            string note = notes[i];
            GameObject newNote = GameObject.Instantiate(notePreset);
            switch (note)
            {
                case "blue":
                    {
                        newNote.GetComponent<Image>().sprite = blueNote;
                        break;
                    }
                case "bluegreen":
                    {
                        newNote.GetComponent<Image>().sprite = bluegreenNote;
                        break;
                    }
                case "pink":
                    {
                        newNote.GetComponent<Image>().sprite = pinkNote;
                        break;
                    }
                case "green":
                    {
                        newNote.GetComponent<Image>().sprite = greenNote;
                        break;
                    }
                case "orange":
                    {
                        newNote.GetComponent<Image>().sprite = orangeNote;
                        break;
                    }
                case "red":
                    {
                        newNote.GetComponent<Image>().sprite = redNote;
                        break;
                    }
                case "purple":
                    {
                        newNote.GetComponent<Image>().sprite = purpleNote;
                        break;
                    }
                case "yellow":
                    {
                        newNote.GetComponent<Image>().sprite = yellowNote;
                        break;
                    }

            }

            newNote.transform.SetParent(noteRow.transform);
        }
        nextNoteIdx = 0;
    }
    private void pressNoteUI()
    {
        GameObject noteObject = notesParent.transform.GetChild(0).GetChild(nextNoteIdx % 6).gameObject;
        Image noteImage = noteObject.GetComponent<Image>();
        noteImage.color = Color.gray;
        if (notesParent.transform.GetChild(0).childCount == nextNoteIdx % 6 + 1)
        {
            GameObject.Destroy(notesParent.transform.GetChild(0).gameObject);
        }
        nextNoteIdx++;
    }
    public void pressNote(string color)
    {
        if (notesParent.transform.childCount == 0) return;

        ws.Emit("note", JsonUtility.ToJson(new Note(ws.myTeam, color)));
    }
    private void decreaseLife()
    {
        GameObject.Destroy(lives.GetChild(lives.childCount - 1).gameObject);
    }
    IEnumerator startTimer(float time)
    {
        timer.maxValue = time;
        timer.value = timer.maxValue;
        bool done = false;
        while (!done)
        {
            if (timer.value > 0.0f)
            {
                timer.value -= Time.deltaTime;
            }
            else
            {
                done = true;
            }
            yield return new WaitForEndOfFrame();
        }
    }
}
