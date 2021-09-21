using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LevelManager: MonoBehaviour {
	//stat
	[SerializeField] private int _maxLives = 3;
	[SerializeField] private int _totalEnemy = 15;
	[SerializeField] private GameObject _panel;
	[SerializeField] private Text _statusInfo;
	[SerializeField] private Text _livesInfo;
	[SerializeField] private Text _totalEnemyInfo;
	private int _currentLives;
	private int _enemyCounter;
	public bool IsOver { get; private set; }

	//tower
	[SerializeField] private Transform _towerUIParent;
	[SerializeField] private GameObject _towerUIPrefab;
	[SerializeField] private Tower[] _towerPrefabs;
	private List<Tower> _spawnedTowers = new List<Tower>();
	//enemy
	[SerializeField] private Enemy[] _enemyPrefabs;
	[SerializeField] private Transform[] _enemyPaths;
	[SerializeField] private float _spawnDelay = 5f;
	private List<Enemy> _spawnedEnemies = new List<Enemy>();
	private float _enemyRunningSpawnDelay;
	//bullet
	private List<Bullet> _spawnedBullets = new List<Bullet>();



	private static LevelManager _instance = null;
	public static LevelManager levelManagerInstance {
		get {
			if(_instance is null) {
				_instance = FindObjectOfType<LevelManager>();
			}
			return _instance;
		}
	}

	public void SetTotalEnemy(int totalEnemy) {
		_enemyCounter = totalEnemy;
		_totalEnemyInfo.text = $"Total Enemy left: {Mathf.Max( _enemyCounter, 0 )}";
	}

	public void SetGameOver(bool isWin) {
		IsOver = true;
		_statusInfo.text = isWin ? "You Win!" : "You Lose!";
		_panel.gameObject.SetActive( true );

	}

	public void ReduceLives(int value) {
		SetCurrentLives( _currentLives - value );
		if(_currentLives == 0) {
			SetGameOver( false );
		}
	}

	public void SetCurrentLives(int currentLives) {
		// Mathf.Max fungsi nya adalah mengambil angka terbesar
		// sehingga _currentLives di sini tidak akan lebih kecil dari 0
		_currentLives = currentLives;
		_livesInfo.text = $"Lives: {Mathf.Max( _currentLives, 0 )}";
	}

	public Bullet GetBulletFromPool(Bullet prefab) {
		GameObject newBulletObj = _spawnedBullets.Find(
		    b => !b.gameObject.activeSelf && b.name.Contains( prefab.name )
		)?.gameObject;

		if(newBulletObj == null) {
			newBulletObj = Instantiate( prefab.gameObject );
		}
		Bullet newBullet = newBulletObj.GetComponent<Bullet>();
		if(!_spawnedBullets.Contains( newBullet )) {
			_spawnedBullets.Add( newBullet );
		}
		return newBullet;
	}

	public void ExplodeAt(Vector2 point, float radius, int damage) {
		foreach(Enemy enemy in _spawnedEnemies) {
			if(enemy.gameObject.activeSelf) {
				if(Vector2.Distance( enemy.transform.position, point ) <= radius) {
					enemy.ReduceEnemyHealth( damage );
				}
			}
		}
	}

	public void RegisterSpawnedTower(Tower tower) {
		_spawnedTowers.Add( tower );
	}

	private void instantiateTowerUI() {
		foreach(Tower tower in _towerPrefabs) {
			GameObject towerInstant = Instantiate( _towerUIPrefab.gameObject, _towerUIParent );
			TowerUi towerUi = towerInstant.GetComponent<TowerUi>();
			towerUi.SetTowerPrefab( tower );
			towerUi.transform.name = tower.name;
		}
	}

	private void SpawnEnemy() {

		int randomIndex = Random.Range( 0, _enemyPrefabs.Length );
		string enemyIndexString = (randomIndex + 1).ToString();

		GameObject newEnemyObj = _spawnedEnemies.Find( e =>
		 !e.gameObject.activeSelf && e.name.Contains( enemyIndexString ) )?.gameObject;

		if(newEnemyObj == null) {
			newEnemyObj = Instantiate( _enemyPrefabs[randomIndex].gameObject );
		}
		Enemy newEnemy = newEnemyObj.GetComponent<Enemy>();
		if(!_spawnedEnemies.Contains( newEnemy )) {
			_spawnedEnemies.Add( newEnemy );
		}
		newEnemy.transform.position = _enemyPaths[0].position;
		newEnemy.SetTargetPosition( _enemyPaths[1].position );
		newEnemy.SetCurrentPathIndex( 1 );
		newEnemy.gameObject.SetActive( true );

		SetTotalEnemy( --_totalEnemy );

		if(_enemyCounter < 0) {
			//bool isAllEnemyDestroyed = _spawnedEnemies.Find( e => e.gameObject.activeSelf ) == null;
			//if(isAllEnemyDestroyed) {
			SetGameOver( true );
			//}
			return;
		}
	}

	// Untuk menampilkan garis penghubung dalam window Scene
	// tanpa harus di-Play terlebih dahulu
	private void OnDrawGizmos() {
		for(int i = 0; i < _enemyPaths.Length - 1; i++) {
			Gizmos.color = Color.cyan;
			Gizmos.DrawLine( _enemyPaths[i].position, _enemyPaths[i + 1].position );
		}
	}

	void Start() {
		instantiateTowerUI();
		_totalEnemyInfo.text = $"Total Enemy left: {_totalEnemy}";
		_livesInfo.text = $"Lives: {_maxLives}";
		_currentLives = _maxLives;
		_enemyCounter = _totalEnemy;
	}

	private void Update() {
		// Time.unscaledDeltaTime adalah deltaTime yang independent, tidak terpengaruh oleh apapun kecuali game object itu sendiri,
		// jadi bisa digunakan sebagai penghitung waktu
		_enemyRunningSpawnDelay -= Time.unscaledDeltaTime;

		if(_enemyRunningSpawnDelay <= 0f) {
			SpawnEnemy();
			_enemyRunningSpawnDelay = _spawnDelay;
		}

		foreach(Enemy enemy in _spawnedEnemies) {
			if(!enemy.gameObject.activeSelf) {
				continue;
			}

			// Kenapa nilainya 0.1? Karena untuk lebih mentoleransi perbedaan posisi,
			// akan terlalu sulit jika perbedaan posisinya harus 0 atau sama persis
			if(Vector2.Distance( enemy.transform.position, enemy.TargetPosition ) < 0.1f) {
				enemy.SetCurrentPathIndex( enemy.CurrentPathIndex + 1 );
				if(enemy.CurrentPathIndex < _enemyPaths.Length) {
					enemy.SetTargetPosition( _enemyPaths[enemy.CurrentPathIndex].position );
				} else {
					ReduceLives( 1 );
					enemy.gameObject.SetActive( false );
				}
			} else {
				enemy.MoveToTarget();
			}
		}

		foreach(Tower tower in _spawnedTowers) {
			tower.CheckNearestEnemy( _spawnedEnemies );
			tower.SeekTarget();
			tower.ShootTarget();
		}
	}
}
