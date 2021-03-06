using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using DTAnimatorStateMachine;
using DTObjectPoolManager;
using InControl;

using DT.Game.Battle.Players;

namespace DT.Game.Battle {
	public class SpawnDummyPlayerIndefinitely : MonoBehaviour, IRecycleSetupSubscriber, IRecycleCleanupSubscriber {
		// PRAGMA MARK - Public Interface
		public event Action<BattlePlayer> OnPlayerSpawned = delegate {};


		// PRAGMA MARK - IRecycleSetupSubscriber Implementation
		public void OnRecycleSetup() {
			if (this.gameObject.activeSelf) {
				SpawnDummyPlayer();
			}
		}


		// PRAGMA MARK - IRecycleCleanupSubscriber Implementation
		public void OnRecycleCleanup() {
			CleanupDummyPlayer(recycle: true);
		}

		// PRAGMA MARK - Internal
		[SerializeField]
		private GameObject playerPrefab_;

		[SerializeField]
		private BattlePlayerSkin skin_;

		private RecyclablePrefab dummyPlayerRecyclable_;

		private void CleanupDummyPlayer(bool recycle) {
			if (dummyPlayerRecyclable_ != null) {
				GameObject recycleReference = dummyPlayerRecyclable_.gameObject;

				dummyPlayerRecyclable_.OnCleanup -= RespawnDummyPlayer;
				dummyPlayerRecyclable_ = null;

				if (recycle) {
					ObjectPoolManager.Recycle(recycleReference);
				}
			}
		}

		private void SpawnDummyPlayer() {
			BattlePlayer player = ObjectPoolManager.Create<BattlePlayer>(playerPrefab_, Vector3.zero, Quaternion.identity, parent: this.gameObject);
			player.SetSkin(skin_);
			dummyPlayerRecyclable_ = player.GetComponentInChildren<RecyclablePrefab>();
			dummyPlayerRecyclable_.OnCleanup += RespawnDummyPlayer;
			OnPlayerSpawned.Invoke(player);
		}

		private void RespawnDummyPlayer(RecyclablePrefab unused) {
			CleanupDummyPlayer(recycle: false);
			CoroutineWrapper.DoAfterDelay(3.0f, () => {
				SpawnDummyPlayer();
			});
		}
	}
}