using System;
using UnityEngine;
using UnityEngine.Pool;

public class CandyFactory : MonoBehaviour
{
    [SerializeField] private Candy _candyPrefab;
    
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

    public Candy CreateCandy()
    {
        var candyObject = _candyPool.Get();
        candyObject.transform.SetParent(transform);
        return candyObject;
    }

    public void ReleaseCandy(Candy candy)
    {
        candy.OnComplete -= ReleaseCandy;
        _candyPool.Release(candy);
    }
}
