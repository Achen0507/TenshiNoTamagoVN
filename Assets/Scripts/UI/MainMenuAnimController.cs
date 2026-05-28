using DG.Tweening;
using System.Collections;
using TenshiNoTamago.Core;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace TenshiNoTamago.UI
{
    public class MainMenuAnimController : MonoBehaviour
    {
        [Header("UI×é¼þ")]
        [SerializeField] private CanvasGroup buttonsGroup;      
        [SerializeField] private RectTransform topBorder;      
        [SerializeField] private RectTransform bottomBorder;
        [SerializeField] private Image leftMaskImage;       
        [SerializeField] private RectTransform girl;

        private void Start()
        {
            topBorder.anchoredPosition = new Vector2(0, 1000);
            bottomBorder.anchoredPosition = new Vector2(0, -1000);
            leftMaskImage.color = new Color(1, 1, 1, 0);
            leftMaskImage.transform.localPosition = new Vector3(-50, 0, 0);
            buttonsGroup.alpha = 0;

            StartCoroutine(PlayEntranceAnimation());
        }

        private IEnumerator PlayEntranceAnimation() {
            yield return new WaitForSeconds(0.2f);

            topBorder.DOAnchorPosY(546, 0.6f).SetEase(Ease.OutBack);
            bottomBorder.DOAnchorPosY(-536, 0.6f).SetEase(Ease.OutBack);
            yield return new WaitForSeconds(0.6f);

            leftMaskImage.DOFade(1, 0.8f);
            leftMaskImage.transform.DOLocalMoveX(0, 0.8f).SetEase(Ease.OutCubic);
            yield return new WaitForSeconds(0.8f);

            buttonsGroup.DOFade(1, 0.5f);
            yield return null;
        }

        public void OnStartGame() {
            GameManager.Instance.ResetGame();
            SceneManager.LoadScene("SampleScene");
        }
    }
}
