using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy: MonoBehaviour {

	[SerializeField] private int _maxHealth = 1;
	[SerializeField] private float _moveSpeed = 1f;
	[SerializeField] private SpriteRenderer _healthBar;
	[SerializeField] private SpriteRenderer _healthFill;
	public Vector3 TargetPosition { get; private set; }
	public int CurrentPathIndex { get; private set; }
	private int _currentHealth;

	private void OnEnable() {
		_currentHealth = _maxHealth;
		_healthFill.size = _healthBar.size;
	}

	public void MoveToTarget() {
		transform.position = Vector3.MoveTowards( transform.position, TargetPosition, _moveSpeed * Time.deltaTime );
	}

	public void SetTargetPosition(Vector3 targetPosition) {

		TargetPosition = targetPosition;
		_healthBar.transform.parent = null;

		// Mengubah rotasi dari enemy
		Vector3 distance = TargetPosition - transform.position;

		if(Mathf.Abs( distance.y ) > Mathf.Abs( distance.x )) {
			// Menghadap atas
			if(distance.y > 0) {
				transform.rotation = Quaternion.Euler( new Vector3( 0f, 0f, 90f ) );
			}
			// Menghadap bawah
			else {
				transform.rotation = Quaternion.Euler( new Vector3( 0f, 0f, -90f ) );
			}
		} else {
			// Menghadap kanan (default)
			if(distance.x > 0) {
				transform.rotation = Quaternion.Euler( new Vector3( 0f, 0f, 0f ) );
			}
			// Menghadap kiri
			else {
				transform.rotation = Quaternion.Euler( new Vector3( 0f, 0f, 180f ) );
			}
		}
		_healthBar.transform.parent = transform;
	}

	// Menandai indeks terakhir pada path
	public void SetCurrentPathIndex(int currentIndex) {
		CurrentPathIndex = currentIndex;
	}
	public void ReduceEnemyHealth(int damage) {
		_currentHealth -= damage;
		AudioPlayer.Instance.PlaySFX( "hit-enemy" );
		_healthFill.size = new Vector2( (_healthFill.size.x - 0.1f), _healthFill.size.y );
		if(_currentHealth <= 0) {
			AudioPlayer.Instance.PlaySFX( "enemy-die" );
			gameObject.SetActive( false );
		}
	}

}
