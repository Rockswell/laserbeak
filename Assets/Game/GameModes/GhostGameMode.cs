using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

using DTAnimatorStateMachine;
using DTObjectPoolManager;
using InControl;

using DT.Game.Battle;
using DT.Game.Battle.Lasers;
using DT.Game.Battle.Players;
using DT.Game.GameModes.Ghost;
using DT.Game.Players;
using DT.Game.Scoring;

namespace DT.Game.GameModes {
	[CreateAssetMenu(fileName = "GhostGameMode", menuName = "Game/Modes/GhostGameMode")]
	public class GhostGameMode : GameMode {
		// PRAGMA MARK - Public Interface
		public override string DisplayTitle {
			get { return "GHOST MODE"; }
		}

		public override int Id {
			get { return GameMode.GetIdFor<GhostGameMode>(); }
		}


		// PRAGMA MARK - Internal
		private const float kAlphaLevel = 0.65f;
		private const float kAlphaDashDuration = 0.65f;
		private const float kAlphaShootDuration = 0.65f;

		private readonly Dictionary<BattlePlayer, GhostModePlayerAddOn> ghostModeAddOns_ = new Dictionary<BattlePlayer, GhostModePlayerAddOn>();

		protected override void Activate() {
			CleanupGhostModeAddOns();
			PlayerSpawner.SpawnAllPlayers();

			List<GameModeIntroView.Icon> icons = new List<GameModeIntroView.Icon>();
			foreach (Player player in RegisteredPlayers.AllPlayers) {
				icons.Add(GameModeIntroView.Icon.Player);
				icons.Add(GameModeIntroView.Icon.Swords);
			}
			icons.RemoveLast();

			GameModeIntroView.Show(DisplayTitle, icons);

			foreach (BattlePlayer battlePlayer in PlayerSpawner.AllSpawnedBattlePlayers) {
				ghostModeAddOns_[battlePlayer] = new GhostModePlayerAddOn(battlePlayer);
			}

			PlayerSpawner.OnSpawnedPlayerRemoved += HandleSpawnedPlayerRemoved;
			BattlePlayerInputDash.OnPlayerDash += HandlePlayerDash;
			GameNotifications.OnBattlePlayerShotLaser.AddListener(HandlePlayerShotLaser);
		}

		protected override void CleanupInternal() {
			PlayerSpawner.OnSpawnedPlayerRemoved -= HandleSpawnedPlayerRemoved;
			BattlePlayerInputDash.OnPlayerDash -= HandlePlayerDash;
			GameNotifications.OnBattlePlayerShotLaser.RemoveListener(HandlePlayerShotLaser);
			CleanupGhostModeAddOns();
		}

		private void HandlePlayerDash(BattlePlayer battlePlayer) {
			ghostModeAddOns_[battlePlayer].AnimateAlpha(kAlphaLevel, kAlphaDashDuration);
		}

		private void HandlePlayerShotLaser(Laser laser, BattlePlayer battlePlayer) {
			foreach (BattlePlayer p in PlayerSpawner.AllSpawnedBattlePlayers) {
				ghostModeAddOns_[p].AnimateAlpha(kAlphaLevel, kAlphaShootDuration);
			}
		}

		private void HandleSpawnedPlayerRemoved() {
			if (PlayerSpawner.AllSpawnedPlayers.Count() > 1) {
				return;
			}

			Finish();
			foreach (Player player in PlayerSpawner.AllSpawnedPlayers) {
				PlayerScores.IncrementPendingScoreFor(player);
			}
		}

		private void CleanupGhostModeAddOns() {
			foreach (var addOn in ghostModeAddOns_.Values) {
				addOn.Dispose();
			}
			ghostModeAddOns_.Clear();
		}
	}
}