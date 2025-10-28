using UnityEngine;
using System.Collections;

namespace LottoDefense.VFX
{
    /// <summary>
    /// Utility class providing simple tween animation functions without external dependencies.
    /// Provides fade, scale, and movement animations using coroutines.
    /// </summary>
    public static class AnimationHelper
    {
        #region Scale Animations
        /// <summary>
        /// Animate transform scale from current to target over duration.
        /// </summary>
        public static IEnumerator ScaleTo(Transform transform, Vector3 targetScale, float duration, AnimationCurve curve = null)
        {
            if (transform == null)
                yield break;

            Vector3 startScale = transform.localScale;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);

                // Apply curve if provided
                if (curve != null)
                    t = curve.Evaluate(t);

                transform.localScale = Vector3.Lerp(startScale, targetScale, t);
                yield return null;
            }

            transform.localScale = targetScale;
        }

        /// <summary>
        /// Animate scale with a punch effect (scale up then back to original).
        /// </summary>
        public static IEnumerator ScalePunch(Transform transform, float punchScale = 1.2f, float duration = 0.3f)
        {
            if (transform == null)
                yield break;

            Vector3 originalScale = transform.localScale;
            Vector3 targetScale = originalScale * punchScale;

            // Scale up (first half of duration)
            float halfDuration = duration * 0.5f;
            yield return ScaleTo(transform, targetScale, halfDuration);

            // Scale back (second half of duration)
            yield return ScaleTo(transform, originalScale, halfDuration);
        }
        #endregion

        #region Fade Animations
        /// <summary>
        /// Animate SpriteRenderer alpha from current to target.
        /// </summary>
        public static IEnumerator FadeTo(SpriteRenderer sprite, float targetAlpha, float duration, AnimationCurve curve = null)
        {
            if (sprite == null)
                yield break;

            float startAlpha = sprite.color.a;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);

                // Apply curve if provided
                if (curve != null)
                    t = curve.Evaluate(t);

                Color color = sprite.color;
                color.a = Mathf.Lerp(startAlpha, targetAlpha, t);
                sprite.color = color;

                yield return null;
            }

            // Ensure final value is set
            Color finalColor = sprite.color;
            finalColor.a = targetAlpha;
            sprite.color = finalColor;
        }

        /// <summary>
        /// Animate CanvasGroup alpha from current to target.
        /// </summary>
        public static IEnumerator FadeTo(CanvasGroup canvasGroup, float targetAlpha, float duration, AnimationCurve curve = null)
        {
            if (canvasGroup == null)
                yield break;

            float startAlpha = canvasGroup.alpha;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);

                // Apply curve if provided
                if (curve != null)
                    t = curve.Evaluate(t);

                canvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, t);
                yield return null;
            }

            canvasGroup.alpha = targetAlpha;
        }
        #endregion

        #region Movement Animations
        /// <summary>
        /// Animate transform position from current to target.
        /// </summary>
        public static IEnumerator MoveTo(Transform transform, Vector3 targetPosition, float duration, AnimationCurve curve = null)
        {
            if (transform == null)
                yield break;

            Vector3 startPosition = transform.position;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);

                // Apply curve if provided
                if (curve != null)
                    t = curve.Evaluate(t);

                transform.position = Vector3.Lerp(startPosition, targetPosition, t);
                yield return null;
            }

            transform.position = targetPosition;
        }

        /// <summary>
        /// Animate transform moving towards target and back.
        /// </summary>
        public static IEnumerator MoveToAndBack(Transform transform, Vector3 targetPosition, float duration)
        {
            if (transform == null)
                yield break;

            Vector3 originalPosition = transform.position;
            float halfDuration = duration * 0.5f;

            // Move to target
            yield return MoveTo(transform, targetPosition, halfDuration);

            // Move back
            yield return MoveTo(transform, originalPosition, halfDuration);
        }
        #endregion

        #region Color Animations
        /// <summary>
        /// Flash sprite color by lerping to target color and back.
        /// </summary>
        public static IEnumerator FlashColor(SpriteRenderer sprite, Color flashColor, float duration, int loops = 1)
        {
            if (sprite == null)
                yield break;

            Color originalColor = sprite.color;
            float halfDuration = duration / (2f * loops);

            for (int i = 0; i < loops; i++)
            {
                // Fade to flash color
                yield return ColorTo(sprite, flashColor, halfDuration);

                // Fade back to original
                yield return ColorTo(sprite, originalColor, halfDuration);
            }
        }

        /// <summary>
        /// Animate sprite color from current to target.
        /// </summary>
        public static IEnumerator ColorTo(SpriteRenderer sprite, Color targetColor, float duration, AnimationCurve curve = null)
        {
            if (sprite == null)
                yield break;

            Color startColor = sprite.color;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);

                // Apply curve if provided
                if (curve != null)
                    t = curve.Evaluate(t);

                sprite.color = Color.Lerp(startColor, targetColor, t);
                yield return null;
            }

            sprite.color = targetColor;
        }
        #endregion

        #region Shake Effects
        /// <summary>
        /// Shake transform position for impact effect.
        /// </summary>
        public static IEnumerator Shake(Transform transform, float intensity = 0.1f, float duration = 0.2f, int shakeCount = 10)
        {
            if (transform == null)
                yield break;

            Vector3 originalPosition = transform.position;
            float elapsed = 0f;
            float interval = duration / shakeCount;

            while (elapsed < duration)
            {
                // Random offset within intensity range
                Vector3 randomOffset = Random.insideUnitSphere * intensity;
                transform.position = originalPosition + randomOffset;

                yield return new WaitForSeconds(interval);
                elapsed += interval;
            }

            // Restore original position
            transform.position = originalPosition;
        }
        #endregion

        #region Utility
        /// <summary>
        /// Create default ease-in-out curve.
        /// </summary>
        public static AnimationCurve GetDefaultCurve()
        {
            return AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
        }

        /// <summary>
        /// Create bounce curve for punch effects.
        /// </summary>
        public static AnimationCurve GetBounceCurve()
        {
            AnimationCurve curve = new AnimationCurve();
            curve.AddKey(0f, 0f);
            curve.AddKey(0.5f, 1.2f); // Overshoot
            curve.AddKey(1f, 1f);
            return curve;
        }
        #endregion
    }
}
