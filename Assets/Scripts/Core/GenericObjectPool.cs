using System.Collections.Generic;
using UnityEngine;

public class GenericObjectPool : MonoBehaviour
{
    [Header("Pool Settings")]
    [SerializeField] private GameObject _prefab;
    [SerializeField] private int _initialPoolSize = 20;

    private readonly Queue<GameObject> _pool = new Queue<GameObject>();

    private void Awake()
    {
        InitializePool();
    }

    private void InitializePool()
    {
        for (int i = 0; i < _initialPoolSize; i++)
        {
            GameObject obj = Instantiate(_prefab, transform);
            obj.SetActive(false);
            _pool.Enqueue(obj);
        }
    }

    public GameObject Get()
    {
        if (_pool.Count > 0)
        {
            GameObject obj = _pool.Dequeue();
            obj.SetActive(true);
            return obj;
        }

        // Пул исчерпан — создаём объект сверх нормы, чтобы игра не вставала.
        // Явно активируем: не полагаемся на дефолтное состояние префаба.
        GameObject newObj = Instantiate(_prefab, transform);
        newObj.SetActive(true);
        return newObj;
    }

    public void Release(GameObject obj)
    {
        obj.SetActive(false);
        _pool.Enqueue(obj);
    }
}