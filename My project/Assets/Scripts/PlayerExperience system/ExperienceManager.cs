using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using Debug = UnityEngine.Debug;
using Random = UnityEngine.Random;

public class ExperienceManager : MonoBehaviour
{
    public static ExperienceManager Instance { get; private set; }

    [Header("预制体")]
    public GameObject experienceBallPrefab;
    public int poolSize = 30;

    [Header("运动参数")]
    public float moveSpeed = 5f;
    public float pickUpRadius = 3f;
    public float collectDistance = 0.3f;

    private Queue<GameObject> ballPool = new Queue<GameObject>();
    private List<GameObject> activeBalls = new List<GameObject>();
    private Transform playerTransform;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
            playerTransform = player.transform;
        else
            Debug.LogError("ExperienceManager 找不到 Tag 为 Player 的对象！");

        for (int i = 0; i < poolSize; i++)
            CreateBall();
    }

    private GameObject CreateBall()
    {
        GameObject ball = Instantiate(experienceBallPrefab, transform);
        ball.SetActive(false);
        ballPool.Enqueue(ball);
        return ball;
    }

    public void SpawnExperienceBall(Vector3 position, int value)
    {
        if (experienceBallPrefab == null)
        {
            Debug.LogError("ExperienceManager：experienceBallPrefab 未设置！");
            return;
        }
        if (playerTransform == null) return;

        if (ballPool.Count == 0) CreateBall();

        GameObject ball = ballPool.Dequeue();
        ball.transform.position = position;
        ball.SetActive(true);
        activeBalls.Add(ball);

        ExperienceBall expScript = ball.GetComponent<ExperienceBall>();
        if (expScript != null)
            expScript.xpValue = value;
        else
            Debug.LogWarning("经验球预制体缺少 ExperienceBall 脚本！");
    }

    private void Update()
    {
        if (playerTransform == null || !playerTransform.gameObject.activeInHierarchy)
        {
            CollectAllBalls();
            return;
        }

        Vector3 playerPos = playerTransform.position;

        for (int i = activeBalls.Count - 1; i >= 0; i--)
        {
            GameObject ball = activeBalls[i];
            if (ball == null || !ball.activeInHierarchy)
            {
                activeBalls.RemoveAt(i);
                continue;
            }

            Vector3 direction = playerPos - ball.transform.position;
            float distance = direction.magnitude;

            if (distance <= pickUpRadius)
            {
                if (distance <= collectDistance)
                    CollectBall(ball, i);
                else
                    ball.transform.position += direction.normalized * moveSpeed * Time.deltaTime;
            }
        }
    }

    private void CollectBall(GameObject ball, int index)
    {
        ExperienceBall expScript = ball.GetComponent<ExperienceBall>();
        if (expScript == null) return;

        if (PlayerExperience.Instance != null)
            PlayerExperience.Instance.AddXP(expScript.xpValue);

        ReturnBall(ball, index);
    }

    private void ReturnBall(GameObject ball, int index)
    {
        ball.SetActive(false);
        if (index >= 0 && index < activeBalls.Count && activeBalls[index] == ball)
            activeBalls.RemoveAt(index);
        ballPool.Enqueue(ball);
    }

    public void CollectAllBalls()
    {
        for (int i = activeBalls.Count - 1; i >= 0; i--)
        {
            GameObject ball = activeBalls[i];
            if (ball == null || !ball.activeInHierarchy)
                activeBalls.RemoveAt(i);
            else
                CollectBall(ball, i);
        }
        activeBalls.Clear();
    }
}