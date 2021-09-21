using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class TowerUi: MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler {

	[SerializeField] private Image _towerIcon;
	private Tower _currentSpawnedTower;
	private Tower _towerPrefab;

	public void OnBeginDrag(PointerEventData eventData) {
		GameObject newTowerObj = Instantiate( _towerPrefab.gameObject );
		_currentSpawnedTower = newTowerObj.GetComponent<Tower>();
		_currentSpawnedTower.ToggleOrderInLayer( true );
	}

	public void OnDrag(PointerEventData eventData) {
		Camera mainCamera = Camera.main;
		Vector3 mousePosition = Input.mousePosition;
		mousePosition.z = -mainCamera.transform.position.z;
		Vector3 targetPosition = Camera.main.ScreenToWorldPoint( mousePosition );
		_currentSpawnedTower.transform.position = targetPosition;
	}

	public void OnEndDrag(PointerEventData eventData) {
		if(_currentSpawnedTower.PlacePosition == null) {
			Destroy( _currentSpawnedTower.gameObject );
		} else {
			_currentSpawnedTower.LockPlacement();
			_currentSpawnedTower.ToggleOrderInLayer( false );
			LevelManager.levelManagerInstance.RegisterSpawnedTower( _currentSpawnedTower );
			_currentSpawnedTower = null;
		}
	}
	public void SetTowerPrefab(Tower tower) {
		_towerPrefab = tower;
		_towerIcon.sprite = tower.towerHeadIcon;
	}
}
