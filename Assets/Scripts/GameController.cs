using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class GameController : MonoBehaviour
{
    static public Dictionary<string, Queue<GameObject>> poolDictionary;
    public List<Pool> pools;
    [SerializeField] GameObject ballPrefab;
    [SerializeField] private TextMeshProUGUI countTMP;
    static public int ballCount = 0;
    public bool debugReverse = false;
    private bool reverseOn = false;

    [System.Serializable]
    public class Pool
    {
        public string tag;
        public GameObject prefab;
        public int size;
    }

    void Start()
    {
        //Object pool creation
        poolDictionary = new Dictionary<string, Queue<GameObject>>();
        StartCoroutine(Spawner());
        foreach (Pool pool in pools)
        {
            Queue<GameObject> objectPool = new Queue<GameObject>();
            for (int i = 0; i < pool.size; i++)
            {
                GameObject obj = Instantiate(pool.prefab);
                obj.SetActive(false);
                objectPool.Enqueue(obj);
            }
            poolDictionary.Add(pool.tag, objectPool);
        }
    }


    void Update()
    {
        if (!reverseOn)
        {
            countTMP.text = "Ball Count: " + ballCount;
            if (ballCount >= 250)
            {
                StopSpawn();
            }

        }
        //Click debugReverse in inspector to trigger reverse mode without condition check
        if (debugReverse) StopSpawn();

    }


    //Takes Ball from object pool and spawns it within random area in camera view
    IEnumerator Spawner()
    {
        yield return new WaitForSeconds(0.25f);
        Vector3 screenPosition = Camera.main.ScreenToWorldPoint(new Vector3(Random.Range(0, Screen.width), Random.Range(0, Screen.height), Camera.main.nearClipPlane + 5));
        SpawnFromPool("Ball", screenPosition, Quaternion.identity); 
        StartCoroutine(Spawner());
    }

    static public GameObject SpawnFromPool(string tag, Vector3 position, Quaternion rotation)
    {
        if (!poolDictionary.ContainsKey(tag)) return null;
        GameObject hookedBall = poolDictionary[tag].Dequeue();
        hookedBall.SetActive(true);
        hookedBall.transform.position = position;
        hookedBall.transform.rotation = rotation;
        ballCount++;

        return hookedBall;
    }


    //Stops balls from spawning and prevents all current balls from merging. Force mode is reversed
    void StopSpawn()
    {
        debugReverse = false;
        reverseOn = true;
        ballCount = 250;
        StopAllCoroutines();
        countTMP.text = "Reverse Mode\n(Can't merge, force push)";
        countTMP.color = new Color(1, 0, 0);

        GameObject[] balls = GameObject.FindGameObjectsWithTag("ball");
        foreach (GameObject ball in balls)
        {
            ball.GetComponent<BallScript>().Reverse();
        }
    }

}



