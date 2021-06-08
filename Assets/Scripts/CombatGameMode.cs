using System;
using System.Collections.Generic;
using System.Linq;
using Assets.Scripts;
using UnityEngine;
using UnityEngine.UI;

public class CombatGameMode : MonoBehaviour
{
    [SerializeField] Timer _Timer;
    Canvas canvas;
    GameObject[] Characters;

    List<GameObject> ShuffledCharacters = new List<GameObject>();

    Queue<GameObject> TurnQueue = new Queue<GameObject>();

   
    void Start()
    {
        CreateTurn();
    }

    void Update()
    {
        if (_Timer && _Timer.isTurnOver && TurnQueue.Count > 0)
        {
           
            //distrugge il flag della turnazione precedente
            Destroy(TurnQueue.Peek().transform.Find("_TurnIcon").gameObject); 
            TurnQueue.Dequeue();
            if(TurnQueue.Count > 0) 
                PutIconAtFirstElementOfQueue();
            else
            {
                DestroyTurn();
                CreateTurn();
            }
               
        }
       
    }

    void PutIconAtFirstElementOfQueue()
    {
        GameObject _TurnIcon = new GameObject("_TurnIcon");
        Image turnIcon = _TurnIcon.AddComponent<Image>();
        turnIcon.color = Color.red;
        turnIcon.transform.SetParent(TurnQueue.Peek().transform);
        turnIcon.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, -100);
        turnIcon.GetComponent<RectTransform>().sizeDelta = new Vector2(15, 50);
    }

    // Prendiamo tutti i personaggi sul campo di battaglia
    // e creiamo un'immagine per ognuno di essi inserendola 
    // nella turnazione
    void CreateTurn()
    {
        float imgOffset = -300f;
        canvas = (Canvas)GameObject.FindObjectOfType(typeof(Canvas));

        if (Characters == null)
            Characters = GameObject.FindGameObjectsWithTag("Character");

        var randomizedList = Randomize(Characters.ToList());

        foreach (GameObject character in randomizedList)
        {
            if (canvas)
            {
                //genera le immagini dei personaggi nel campo di battaglia
                //e le mette nella coda di turni
                GameObject imgObject = new GameObject(character.name);
                //aggiungiamo una classe custom per tipizzare il gameobject
                imgObject.AddComponent<CharacterIcon>();

                RectTransform trans = imgObject.AddComponent<RectTransform>();
                trans.transform.SetParent(canvas.transform);
                trans.localScale = Vector3.one;
                trans.anchoredPosition = new Vector2(imgOffset, 400f);
                trans.sizeDelta = new Vector2(200, 200);

                Image image = imgObject.AddComponent<Image>();
                imgObject.transform.SetParent(canvas.transform);
                imgOffset += 230;

                GameObject text = new GameObject(character.name);

                Text _text = text.AddComponent<Text>();
                _text.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
                _text.alignment = TextAnchor.MiddleCenter;
                _text.color = Color.black;
                _text.text = character.name;
                _text.fontSize = 22;
                _text.transform.SetParent(image.transform);

                text.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 0);

                //Mette i personaggi nella coda
                TurnQueue.Enqueue(imgObject);
            }
        }

        PutIconAtFirstElementOfQueue();
    }

    void DestroyTurn()
    {
        var charactersInGame = GameObject.FindObjectsOfType<CharacterIcon>();

        foreach (var ele in charactersInGame)
        {
            Destroy(ele.gameObject);
        }
    }

    List<GameObject> Randomize(List<GameObject> list)
    {
        List<GameObject> randomizedList = new List<GameObject>();
        System.Random rnd = new System.Random();

        while (list.Count > 0)
        {
            int index = rnd.Next(0, list.Count);
            randomizedList.Add(list[index]);
            list.RemoveAt(index);
        }
        return randomizedList;
    }
}
