using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameController : MonoBehaviour
{
    [SerializeField] private Sprite playerSprite;
    [SerializeField] private Sprite enemySprite;
    [SerializeField] private Sprite bulletsprite;
    [SerializeField] private TMP_Text scoreText;
    private Controller controller;
    private View view;
    private void Start()
    {
        view = new View(playerSprite, enemySprite, bulletsprite, scoreText);
        controller = new Controller(view);
        controller.Start();
    }

    private void Update()
    {
        controller.Update();
    }
}

public class Controller
{
    private PlayerModel player;
    private List<EnemyModel> enemies = new List<EnemyModel>();
    private List<BulletModel> bullets = new List<BulletModel>();
    private View view;
    private float enemySpawnTime;
    private float bulletSpawnTime;
    private float playerTimeScale = 5f;
    private float enemyTimeScale = 5f;
    private float bulletTimeScale = 7f;

    public Controller(View view)
    {
        this.view = view;
    }

    public void Start()
    {
        player = new PlayerModel();
        player.Position = new Vector3(-8, 0, 0);
        player.OnplayerPositionChanged += (position) =>
        {
            view.RenderPlayer(position);
        };
        player.OnplayerScoreChanged += (score) =>
        {
            view.UpdateScore(score);
        };
        view.SpawnPlayer(player.Position);
    }
    public void Update()
    {
        UpdatePlayer();
        UpdateBullets();
        UpdateEnemies();
        SpawnEnemies();
        SpawnBullets();
        UpdateCollisions();
    }
    private void SpawnBullets()
    {
        bulletSpawnTime += Time.deltaTime;
        if (bulletSpawnTime > 0.5f)
        {
            bulletSpawnTime = 0;
            BulletModel bullet = new BulletModel();
            bullet.Position = player.Position;
            bullet.OnBulletPositionChanged += (position, bulletModel) =>
            {
                view.RenderBullet(position, bulletModel);
            };
            bullets.Add(bullet);
            view.SpawnBullet(bullet.Position, bullet);
        }
    }
    private void SpawnEnemies()
    {
        enemySpawnTime += Time.deltaTime;
        if (enemySpawnTime > 1f)
        {
            enemySpawnTime = 0;
            EnemyModel enemy = new EnemyModel();
            enemy.Position = new Vector3(10, UnityEngine.Random.Range(-3, 3), 0);
            enemy.OnEnemyPositionChanged += (position, enemyModel) =>
            {
                view.RenderEnemy(position, enemyModel);
            };
            enemies.Add(enemy);
            view.SpawnEnemy(enemy.Position, enemy);
        }
    }
    private void UpdatePlayer()
    {
        player.Position += new Vector3(0, Input.GetAxis("Vertical"), 0) * Time.deltaTime * playerTimeScale;
        player.Position = new Vector3(player.Position.x, Mathf.Clamp(player.Position.y, -4f, 4f), player.Position.z);
    }
    private void UpdateBullets()
    {
        foreach (var bullet in bullets.ToList())
        {
            bullet.Position += new Vector3(1, 0, 0) * Time.deltaTime * bulletTimeScale;
            if (bullet.Position.x > 10)
            {
                bullets.Remove(bullet);
                GameObject obj = view.bulletObjectMap[bullet];
                view.bulletObjectMap.Remove(bullet);
                GameObject.Destroy(obj);
            }
        }
    }

    private void UpdateEnemies()
    {
        foreach (var enemy in enemies.ToList())
        {
            enemy.Position += new Vector3(-1, 0, 0) * Time.deltaTime * enemyTimeScale;
            if (enemy.Position.x < -10)
            {
                enemies.Remove(enemy);
                GameObject obj = view.enemyGameObjectMap[enemy];
                view.enemyGameObjectMap.Remove(enemy);
                GameObject.Destroy(obj);
            }
        }
    }

    private void UpdateCollisions()
    {
        foreach (var bullet in bullets.ToList())
        {
            foreach (var enemy in enemies.ToList())
            {
                if (Vector3.Distance(bullet.Position, enemy.Position) < 1)
                {
                    bullets.Remove(bullet);
                    enemies.Remove(enemy);
                    GameObject bulletObj = view.bulletObjectMap[bullet];
                    view.bulletObjectMap.Remove(bullet);
                    GameObject.Destroy(bulletObj);
                    GameObject enemyObj = view.enemyGameObjectMap[enemy];
                    view.enemyGameObjectMap.Remove(enemy);
                    GameObject.Destroy(enemyObj);
                    player.Score += 100;
                }
            }
        }
    }
}

public class View
{
    private Sprite playerSprite;
    private Sprite enemySprite;
    private Sprite bulletSprite;
    private TMP_Text scoreText;
    public GameObject playerGameObject;
    public Dictionary<EnemyModel, GameObject> enemyGameObjectMap = new Dictionary<EnemyModel, GameObject>();
    public Dictionary<BulletModel, GameObject> bulletObjectMap = new Dictionary<BulletModel, GameObject>();
    public View(Sprite playerSprite, Sprite enemySprite, Sprite bulletSprite, TMP_Text scoreText)
    {
        this.playerSprite = playerSprite;
        this.enemySprite = enemySprite;
        this.bulletSprite = bulletSprite;
        this.scoreText = scoreText;
    }
    public void SpawnPlayer(Vector3 position)
    {
        playerGameObject = new GameObject("Player");
        playerGameObject.AddComponent<SpriteRenderer>().sprite = playerSprite;
        playerGameObject.transform.position = position;
        playerGameObject.transform.rotation = Quaternion.Euler(0, 0, -90f);
    }

    public void SpawnEnemy(Vector3 position, EnemyModel enemyModel)
    {
        GameObject enemyGameObject = new GameObject("Enemy");
        enemyGameObject.AddComponent<SpriteRenderer>().sprite = enemySprite;
        enemyGameObject.transform.position = position;
        enemyGameObject.transform.rotation = Quaternion.Euler(0, 0, -90f);
        enemyGameObjectMap.Add(enemyModel, enemyGameObject);
    }
    public void SpawnBullet(Vector3 position, BulletModel bulletModel)
    {
        GameObject bulletGameObject = new GameObject("Bullet");
        bulletGameObject.AddComponent<SpriteRenderer>().sprite = bulletSprite;
        bulletGameObject.transform.position = position;
        bulletGameObject.transform.rotation = Quaternion.Euler(0, 0, -90f);
        bulletObjectMap.Add(bulletModel, bulletGameObject);
    }
    public void RenderPlayer(Vector3 position)
    {
        playerGameObject.transform.position = position;
    }

    public void RenderEnemy(Vector3 position, EnemyModel enemyModel)
    {
        enemyGameObjectMap[enemyModel].transform.position = position;
    }

    public void RenderBullet(Vector3 position, BulletModel bulletModel)
    {
        bulletObjectMap[bulletModel].transform.position = position;
    }
    public void UpdateScore(int score)
    {
        scoreText.text = score.ToString();
    }
}

public class PlayerModel
{
    public Action<Vector3> OnplayerPositionChanged;
    public Action<int> OnplayerScoreChanged;
    private Vector3 position;
    public Vector3 Position
    {
        get => position;
        set
        {
            position = value;
            OnplayerPositionChanged?.Invoke(position);
        }
    }
    private int score;
    public int Score
    {
        get => score;
        set
        {
            score = value;
            OnplayerScoreChanged?.Invoke(score);
        }
    }
}

public class EnemyModel
{
    public Action<Vector3, EnemyModel> OnEnemyPositionChanged;
    private Vector3 position;
    public Vector3 Position
    {
        get => position;
        set
        {
            position = value;
            OnEnemyPositionChanged?.Invoke(position, this);
        }
    }
}

public class BulletModel
{
    public Action<Vector3, BulletModel> OnBulletPositionChanged;
    private Vector3 position;
    public Vector3 Position
    {
        get => position;
        set
        {
            position = value;
            OnBulletPositionChanged?.Invoke(position, this);
        }
    }
}
