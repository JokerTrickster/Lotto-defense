using UnityEngine;
using UnityEngine.UI;
using System.Collections;

namespace LottoDefense.UI
{
    public class LoadingOverlay : MonoBehaviour
    {
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private Image loadingSpinner;
        [SerializeField] private Text loadingText;
        [SerializeField] private float spinSpeed = 180f;
        [SerializeField] private float fadeSpeed = 2f;

        private Coroutine _spinCoroutine;

        private void Awake()
        {
            if (canvasGroup == null)
            {
                canvasGroup = GetComponent<CanvasGroup>();
            }

            Hide(true);
        }

        public void Show(string message = "Loading...")
        {
            gameObject.SetActive(true);

            if (loadingText != null)
            {
                loadingText.text = message;
            }

            StopAllCoroutines();
            StartCoroutine(FadeIn());

            if (loadingSpinner != null)
            {
                _spinCoroutine = StartCoroutine(SpinAnimation());
            }
        }

        public void Hide(bool immediate = false)
        {
            if (_spinCoroutine != null)
            {
                StopCoroutine(_spinCoroutine);
                _spinCoroutine = null;
            }

            if (immediate)
            {
                canvasGroup.alpha = 0f;
                gameObject.SetActive(false);
            }
            else
            {
                StartCoroutine(FadeOut());
            }
        }

        private IEnumerator FadeIn()
        {
            canvasGroup.alpha = 0f;

            while (canvasGroup.alpha < 1f)
            {
                canvasGroup.alpha += Time.deltaTime * fadeSpeed;
                yield return null;
            }

            canvasGroup.alpha = 1f;
        }

        private IEnumerator FadeOut()
        {
            while (canvasGroup.alpha > 0f)
            {
                canvasGroup.alpha -= Time.deltaTime * fadeSpeed;
                yield return null;
            }

            canvasGroup.alpha = 0f;
            gameObject.SetActive(false);
        }

        private IEnumerator SpinAnimation()
        {
            while (true)
            {
                loadingSpinner.transform.Rotate(0f, 0f, -spinSpeed * Time.deltaTime);
                yield return null;
            }
        }
    }
}
