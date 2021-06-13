using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class LevelController : MonoBehaviour
{
    int _minValue = 1;
    int _maxValue = 20;
    int _erasersCount = 1;
    float _eraserSpawnRate = 10f;
    float _numberSpawnRate = 0.75f;
    int _goal = 50;
    readonly Vector2 _numberRadius = new Vector2(2f, 2f);
    Player _player;
    GameObject _parentObject;
    [SerializeField] GameObject storagePrefab;
    [SerializeField] GameObject playerPrefab;
    [SerializeField] GameObject numberPrefab;
    [SerializeField] GameObject eraserPrefab;
    [SerializeField] LevelSizeHolder levelSizeHolder;
    [SerializeField] LayerMask spawnCollisionLayer;

    Coroutine _spawnErasersCoro;
    Coroutine _spawnNumbersCoro;

    public void OnPlayerDeath()
    {
        Debug.Log(">пук");
        if (_spawnErasersCoro != null)
        {
            StopCoroutine(_spawnErasersCoro);
        }
        if (_spawnNumbersCoro != null)
        {
            StopCoroutine(_spawnNumbersCoro);
        }

        FinishLevel();
    }

    void OnStore(int value) {
        if (value >= _goal) {
            Debug.Log($"Goal achieved! {value}");
            FinishLevel();
        }
    }

    void FinishLevel() {
        Destroy(_parentObject);
    }

    public void InitializeLevel(int minNumber = 1, int maxNumber = 20, float numberSpawnRate = 0.75f, int goal = 50, int erasersCount = 1, float eraserSpawnInterval = 10f)
    {
        _minValue = minNumber;
        _maxValue = maxNumber;
        _erasersCount = erasersCount;
        _eraserSpawnRate = eraserSpawnInterval;
        _numberSpawnRate = numberSpawnRate;
        _goal = goal;

        _parentObject = new GameObject();

        SpawnPlayer();
        SpawnStorage();
        _spawnNumbersCoro = StartCoroutine(nameof(SpawnNewNumber));
        _spawnErasersCoro = StartCoroutine(nameof(SpawnEraser));
    }

    void SpawnPlayer()
    {
        _player = Instantiate(playerPrefab, Vector3.zero, Quaternion.identity).GetComponent<Player>();
        _player.onDeath = OnPlayerDeath;

        _player.transform.SetParent(_parentObject.transform);
    }
    void SpawnStorage()
    {
        
        var storage = Instantiate(storagePrefab, _parentObject.transform);
        var storageHeight = storage.GetComponent<SpriteRenderer>().size.y;
        var storageLocation = new Vector2(0, levelSizeHolder.levelSize.y / 2 - storageHeight / 2);
        storage.transform.position = storageLocation;
        storage.GetComponent<Storage>().OnStore += OnStore;

        // storage.transform.SetParent(parentObject.transform);
    }

    IEnumerator SpawnNewNumber()
    {
        for (; ; )
        {
            Vector3 spawnPoint = GetRandomPosition();
            if (spawnPoint == Vector3.zero)
                yield return null;
            var number = Instantiate(numberPrefab, spawnPoint, Quaternion.identity).GetComponent<Number>();
            number.Initiate(Random.Range(_minValue, _maxValue));

            number.transform.SetParent(_parentObject.transform);
            yield return new WaitForSeconds(_numberSpawnRate);
        }
    }

    IEnumerator SpawnEraser()
    {
        for (var i = 0; i < _erasersCount; ++i)
        {
            GameObject eraser = Instantiate(eraserPrefab, GetRandomPosition(), Quaternion.identity);

            eraser.transform.SetParent(_parentObject.transform);
            yield return new WaitForSeconds(_eraserSpawnRate);
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        // var numbersToSpawn = 20;
        // SpawnPlayer();
        // for (var i = 0; i < numbersToSpawn; i++) {
        //     SpawnNewNumber();
        // }
        // SpawnEraser();
        InitializeLevel();
    }

    public void ClearLevel()
    {
        Destroy(_parentObject);
    }

    Vector2 GetRandomPosition()
    {
        RaycastHit2D hit;
        var position = Vector2.zero;

        var counter = 1000;
        do
        {
            if (counter-- == 0)
            {
                Debug.Log("FUCK YOU");
                break;
            }
            var x = Random.Range(-levelSizeHolder.levelSize.x / 2, levelSizeHolder.levelSize.x / 2);
            var y = Random.Range(-levelSizeHolder.levelSize.y / 2, levelSizeHolder.levelSize.y / 2);
            position = new Vector3(x, y);
            hit = Physics2D.BoxCast(position, _numberRadius, 0, Vector2.up, 0, spawnCollisionLayer);

        } while (hit);

        return position;
    }
}
