using System;
using System.Collections;
using UnityEngine;

using DTAnimatorStateMachine;

namespace DT.Game {
	[RequireComponent(typeof(Animator))]
	public class GameStateMachine : MonoBehaviour {
		// PRAGMA MARK - Internal
		private Animator animator_;

		private void Awake() {
			animator_ = this.GetRequiredComponent<Animator>();
			this.ConfigureAllStateBehaviours(animator_);
		}
	}
}