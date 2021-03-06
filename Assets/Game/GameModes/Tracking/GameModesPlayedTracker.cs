using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

using DTAnimatorStateMachine;
using DTCommandPalette;
using DTObjectPoolManager;
using InControl;

namespace DT.Game.GameModes {
	public static class GameModesPlayedTracker {
		public static event Action<GameMode> OnGameModePlayTracked = delegate {};

		public static int GetPlayedCountFor(GameMode gameMode) {
			return PlayedData_.GetCountFor(gameMode.Id);
		}

		public static void Reset() {
			PlayedData_.Reset();
			SavePlayedData();
		}

		public static void IncrementPlayedCountFor(GameMode gameMode) {
			PlayedData_.IncrementCountFor(gameMode.Id);
			SavePlayedData();

			OnGameModePlayTracked.Invoke(gameMode);
		}

		public static IEnumerable<GameMode> FilterByLeastPlayed(IEnumerable<GameMode> gameModes) {
			int minPlayed = gameModes.Min(gm => GetPlayedCountFor(gm));
			foreach (GameMode mode in gameModes) {
				if (GetPlayedCountFor(mode) == minPlayed) {
					yield return mode;
				}
			}
		}

		[Serializable]
		private class GameModesPlayedData {
			public void Reset() {
				PlayedMap_.Clear();
				SaveDataPoints();
			}

			public void IncrementCountFor(int gameModeId) {
				PlayedMap_.Increment(gameModeId);
				SaveDataPoints();
			}

			public int GetCountFor(int gameModeId) {
				return PlayedMap_.GetValue(gameModeId);
			}

			[SerializeField]
			private GameModesPlayedDataPoint[] dataPoints_;

			[NonSerialized]
			private CountMap<int> playedMap_ = null;
			private CountMap<int> PlayedMap_ {
				get {
					if (playedMap_ == null) {
						playedMap_ = new CountMap<int>();
						if (dataPoints_ != null) {
							foreach (var dataPoint in dataPoints_) {
								playedMap_[dataPoint.GameModeId] = dataPoint.Count;
							}
						}
					}

					return playedMap_;
				}
			}

			private void SaveDataPoints() {
				dataPoints_ = PlayedMap_.Select(kvp => new GameModesPlayedDataPoint(kvp.Key, kvp.Value)).ToArray();
			}
		}

		[Serializable]
		private class GameModesPlayedDataPoint {
			public int GameModeId;
			public int Count;

			public GameModesPlayedDataPoint(int gameModeId, int count) {
				GameModeId = gameModeId;
				Count = count;
			}
		}

		private static GameModesPlayedData playedData_ = null;
		private static GameModesPlayedData PlayedData_ {
			get { return playedData_ ?? (playedData_ = JsonUtility.FromJson<GameModesPlayedData>(PlayerPrefs.GetString("GameModesPlayedTracker::PlayedData"))) ?? (playedData_ = new GameModesPlayedData()); }
		}

		[RuntimeInitializeOnLoadMethod]
		private static void Initialize() {
			GameMode.OnActivate += HandleGameModeActivated;
		}

		[MethodCommand]
		private static void LogGameModePlayedTracker() {
			foreach (var gameMode in GameConstants.Instance.GameModes) {
				Debug.Log("Game mode: " + gameMode.DisplayTitle + " has been played " + GetPlayedCountFor(gameMode) + " times!");
			}
		}

		private static void HandleGameModeActivated(GameMode gameMode) {
			IncrementPlayedCountFor(gameMode);
		}

		private static void SavePlayedData() {
			PlayerPrefs.SetString("GameModesPlayedTracker::PlayedData", JsonUtility.ToJson(PlayedData_));
		}
	}
}