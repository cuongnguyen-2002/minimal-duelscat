using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class CandyItemData
{
    public int ID;
    public Sprite CandySprite;
    public int Score;
}

[CreateAssetMenu(fileName = "CandyConfig", menuName = "Data/CandyConfig")]
public class CandyConfig : ScriptableObject
{
    [SerializeField] private List<CandyItemData> _candyItems = new();
    
    public CandyItemData GetCandyItem(int ID)
    {
        var item = _candyItems.Find(x => x.ID == ID);
        return item;
    }
}
