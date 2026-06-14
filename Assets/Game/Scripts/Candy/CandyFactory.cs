using System;
using UnityEngine;
using UnityEngine.Pool;

public class CandyFactory : MonoBehaviour
{
    [SerializeField] private Candy _candyPrefab;
    [SerializeField] private CandyConfig _candyConfig;
    private ObjectPool<Candy> _candyPool;

    private void Start()
    {
        _candyPool = new ObjectPool<Candy>(
            createFunc: () => Instantiate(_candyPrefab),
            actionOnGet: candy => candy.gameObject.SetActive(true),
            actionOnRelease: candy => candy.gameObject.SetActive(false),
            collectionCheck: false,
            defaultCapacity: 10,
            maxSize: 100
            );
    }

    public Candy CreateCandy(int id)
    {
        var candyObject = _candyPool.Get();
        candyObject.transform.SetParent(transform);
        var candyConfigItem = _candyConfig.GetCandyItem(id);
        if (candyConfigItem != null) candyObject.Init(candyConfigItem.CandySprite, candyConfigItem.Score);
        return candyObject;
    }

    public void ReleaseCandy(Candy candy)
    {
        candy.OnComplete -= ReleaseCandy;
        _candyPool.Release(candy);
    }
}
