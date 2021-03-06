using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

using DTAnimatorStateMachine;
using DTObjectPoolManager;
using InControl;

using DT.Game.Battle;
using DT.Game.Battle.AI;
using DT.Game.Battle.Lasers;
using DT.Game.Battle.Players;
using DT.Game.GameModes.Tag;
using DT.Game.Players;
using DT.Game.Scoring;

namespace DT.Game.GameModes {
	[CreateAssetMenu(fileName = "TagGameMode", menuName = "Game/Modes/TagGameMode")]
	public class TagGameMode : GameMode {
		// PRAGMA MARK - Public Interface
		public override string DisplayTitle {
			get { return "HOT POTATO - WITH BOMBS"; }
		}

		public override int Id {
			get { return GameMode.GetIdFor<TagGameMode>(); }
		}


		// PRAGMA MARK - Internal
		// the person who is "it" - also they will explode if they don't tag anyone soon :)
		private BattlePlayer itPlayer_ = null;
		private BattlePlayer ItPlayer_ {
			get { return itPlayer_; }
		}

		private void SetItPlayer(BattlePlayer battlePlayer, float? timeLeft = null) {
			if (battlePlayer == null) {
				Debug.LogWarning("Cannot set null battlePlayer as ItPlayer!");
				return;
			}

			if (itPlayer_ == battlePlayer) {
				Debug.LogWarning("Not setting ItPlayer since already is ItPlayer!");
				return;
			}

			itPlayer_ = battlePlayer;
			InGameConstants.AllowedChargingLasersWhitelist.Clear();
			InGameConstants.AllowedChargingLasersWhitelist.Add(itPlayer_);

			var explosive = ObjectPoolManager.Create<TagExplosive>(GamePrefabs.Instance.TagExplosivePrefab, parent: itPlayer_.AccessoriesContainer);
			if (timeLeft == null) {
				explosive.Init(itPlayer_);
			} else {
				explosive.Init(itPlayer_, timeLeft.Value);
			}
		}

		protected override void Activate() {
			AIIdleState.SetShouldCheckDashAttackPredicate(ShouldBattlePlayerDashAttack);
			PlayerSpawner.SpawnAllPlayers();

			List<GameModeIntroView.Icon> icons = new List<GameModeIntroView.Icon>();
			foreach (Player player in RegisteredPlayers.AllPlayers) {
				icons.Add(GameModeIntroView.Icon.Player);
				icons.Add(GameModeIntroView.Icon.Swords);
			}
			icons.RemoveLast();

			BattlePlayerHealth.LaserDamage = 0;
			InGameConstants.AllowChargingLasers = false;

			// NOTE (darren): avoid showing shields to have less visual noise with exploding object
			InGameConstants.ShowShields = false;

			GameModeIntroView.Show(DisplayTitle, icons, onFinishedCallback: () => {
				SetItPlayer(PlayerSpawner.AllSpawnedBattlePlayers.Random());
			});

			GameNotifications.OnBattlePlayerLaserHit.AddListener(HandleBattlePlayerHit);
			PlayerSpawner.OnSpawnedPlayerRemoved += HandleSpawnedPlayerRemoved;
		}

		protected override void CleanupInternal() {
			AIIdleState.ClearShouldCheckDashAttackPredicate();
			GameNotifications.OnBattlePlayerLaserHit.RemoveListener(HandleBattlePlayerHit);
			PlayerSpawner.OnSpawnedPlayerRemoved -= HandleSpawnedPlayerRemoved;
			BattlePlayerHealth.LaserDamage = 1;
			InGameConstants.AllowChargingLasers = true;
			InGameConstants.AllowedChargingLasersWhitelist.Clear();
			InGameConstants.ShowShields = true;

			itPlayer_ = null;

			foreach (BattlePlayer battlePlayer in PlayerSpawner.AllSpawnedBattlePlayers) {
				var explosive = battlePlayer.GetComponentInChildren<TagExplosive>();
				if (explosive == null) {
					continue;
				}

				ObjectPoolManager.Recycle(explosive);
			}
		}

		private bool ShouldBattlePlayerDashAttack(BattlePlayer battlePlayer) {
			return battlePlayer != itPlayer_;
		}

		private void HandleBattlePlayerHit(Laser laser, BattlePlayer playerHit) {
			BattlePlayer laserSourcePlayer = laser.BattlePlayer;
			if (laserSourcePlayer == playerHit) {
				return;
			}

			if (laserSourcePlayer != ItPlayer_) {
				return;
			}

			TagExplosive tagExplosive = laserSourcePlayer.GetComponentInChildren<TagExplosive>();
			if (tagExplosive == null) {
				Debug.LogError("Failed to get TagExplosive from It player, very weird!");
				return;
			}

			SetItPlayer(playerHit, tagExplosive.TimeLeft);
			ObjectPoolManager.Recycle(tagExplosive);
		}

		private void HandleSpawnedPlayerRemoved() {
			if (PlayerSpawner.AllSpawnedPlayers.Count() > 1) {
				// if ItPlayer_ is now dead - choose new itPlayer
				if (!PlayerSpawner.AllSpawnedBattlePlayers.Contains(ItPlayer_)) {
					SetItPlayer(PlayerSpawner.AllSpawnedBattlePlayers.Random());
				}
				return;
			}

			Finish();
			foreach (Player player in PlayerSpawner.AllSpawnedPlayers) {
				PlayerScores.IncrementPendingScoreFor(player);
			}
		}
	}
}