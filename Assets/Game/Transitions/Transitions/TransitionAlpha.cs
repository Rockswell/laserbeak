using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

using DTEasings;
using DTObjectPoolManager;

namespace DT.Game.Transitions {
	public class TransitionAlpha : TransitionUI, ITransition {
		// PRAGMA MARK - ITransition Implementation
		public override void Animate(TransitionType transitionType, float delay, Action<ITransition> callback) {
			float startAlpha = (transitionType == TransitionType.In) ? minAlpha_ : maxAlpha_;
			float endAlpha = (transitionType == TransitionType.In) ? maxAlpha_ : minAlpha_;

			EaseType easeType = (transitionType == TransitionType.In) ? inEaseType_ : outEaseType_;

			SetAlpha(startAlpha);
			CoroutineWrapper.DoAfterDelay(delay, () => {
				CoroutineWrapper.DoEaseFor(Duration_, easeType, (float p) => {
					SetAlpha(Mathf.Lerp(startAlpha, endAlpha, p));
				}, () => {
					callback.Invoke(this);
				});
			});
		}


		// PRAGMA MARK - Internal
		[Header("Alpha Outlets")]
		[SerializeField, DTValidator.Optional]
		private Image image_;

		[Header("Alpha Properties")]
		[SerializeField]
		private float minAlpha_ = 0.0f;
		[SerializeField]
		private float maxAlpha_ = 1.0f;

		[Space]
		[SerializeField]
		private EaseType inEaseType_ = EaseType.QuadraticEaseOut;
		[SerializeField]
		private EaseType outEaseType_ = EaseType.QuadraticEaseIn;

		private void SetAlpha(float alpha) {
			if (image_ != null) {
				image_.color = image_.color.WithAlpha(alpha);
			}
		}
	}
}