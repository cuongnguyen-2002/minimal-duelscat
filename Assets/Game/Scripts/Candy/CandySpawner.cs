using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using UnityEngine;

[System.Serializable]
public class NoteData
{
    public int id;
    public int n;
    public double ta;
    public double ts;
    public double d;
    public int v;
    public int pid;
}

public class CandySpawner : MonoBehaviour
{
    [Header("Notes JsonData")]
    [SerializeField] private TextAsset _notesData;
    private List<NoteData> _notes = new();
    public List<NoteData> Notes => _notes;

    private readonly Dictionary<int, int> _pidToLane = new Dictionary<int, int>()
    {
        { 0, 0 },
        { 2, 1 },
        { 3, 2 },
        { 5, 3 },
    };
    
    [Header("Spawn Point")]
    [SerializeField] private Transform[] _spawnPoints;
    
    [Header("Candy Pool")]
    [SerializeField] private CandyFactory _candyFactory;
    
    void Start()
    {
        Init();
    }

    private void Init()
    {
        _notes = JsonConvert.DeserializeObject<List<NoteData>>(_notesData.text);
        _notes.Sort((a, b) => a.ta.CompareTo(b.ta));
    }
    
    public Candy SpawnCandy(int index, GameController controller)
    {
        if(index < 0 || index >= _notes.Count) return null;
        NoteData noteData = _notes[index];
        int land = _pidToLane.ContainsKey(noteData.pid) ? _pidToLane[noteData.pid] : 0;
        
        var candyObject = _candyFactory.CreateCandy();
        candyObject.Init(
            hitTime: controller.HitY,
            fallSpeed: controller.FallSpeed,
            missWindows: controller.MissWindows);
        candyObject.transform.position = _spawnPoints[land].position;
        candyObject.OnComplete += CompleteCandy;
        return candyObject;
    }

    private void CompleteCandy(Candy candy)
    {
        candy.OnComplete -= CompleteCandy;
        _candyFactory.ReleaseCandy(candy);
    }
    
}
