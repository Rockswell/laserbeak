using System;
using System.Collections;
using System.Linq;
using UnityEngine;

using DTAnimatorStateMachine;
using DTObjectPoolManager;
using InControl;

using DT.Game.Battle.Pausing;
using DT.Game.Battle.Players;
using DT.Game.GameModes;
using DT.Game.Players;

namespace DT.Game.Battle {
	public class BattleState : DTStateMachineBehaviour<GameStateMachine> {
		// PRAGMA MARK - Internal
		private GameMode currentGameMode_ = null;

		private PauseController pauseController_;

		protected override void OnInitialized() {
			// stub
		}

		protected override void OnStateEntered() {
			// cleanup in-case
			PlayerSpawner.CleanupAllPlayers();
			CleanupCurrentGameMode();

			// TODO (darren): filtering based on options will be here
			currentGameMode_ = GameModesPlayedTracker.FilterByLeastPlayed(GameConstants.Instance.GameModes).ToArray().Random();

			currentGameMode_.LoadArena();
			currentGameMode_.ShowIntroductionIfNecessary(() => {
				currentGameMode_.Activate(FinishBattle);

				GameModeIntroView.OnIntroFinished += HandleIntroFinished;

				InGamePlayerCollectionView.Show();
				InGamePlayerHUDEffect.CreateForAllPlayers();
			});
		}

		protected override void OnStateExited() {
			GameModeIntroView.OnIntroFinished -= HandleIntroFinished;

			CleanupPauseController();
			CleanupCurrentGameMode();

			InGamePlayerCollectionView.Hide();
		}

		private void FinishBattle() {
			StateMachine_.HandleBattleFinished();
		}

		private void CleanupCurrentGameMode() {
			if (currentGameMode_ != null) {
				currentGameMode_.Cleanup();
				currentGameMode_ = null;
			}
		}

		private void GoToTitleScreen() {
			StateMachine_.GoToMainMenu();
		}

		private void HandleIntroFinished() {
			CleanupPauseController();
			pauseController_ = new PauseController(skipCallback: FinishBattle, restartCallback: GoToTitleScreen);
		}

		private void CleanupPauseController() {
			if (pauseController_ != null) {
				pauseController_.Dispose();
				pauseController_ = null;
			}
		}
	}
}