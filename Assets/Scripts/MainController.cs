using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Threading;
using DG.Tweening;
using UnityEngine.Events;
using Cysharp.Threading.Tasks;
using TMPro;


public class MainController : MonoBehaviour
{
    public Button playButton;
    public Button pauseButton;
    public Button resetButton;
    public Button rescaleButton;
    public Button randomSpriteButton;
    public Button jumpButton;
    public Button spinButton;
    public Button fadeButton;
    public Button runAllButton;
    public Button animatedButton;
    public Button cancelButton;
    public GameObject animationControlUI;
    public TextMeshProUGUI currentAnimationText;
    public Vector3 targetScale;
    public Sprite[] sprites;

    [SerializeField] private Button currentActionControlButton;
    [SerializeField] private CancellationTokenSource _cts;
    [SerializeField] private bool isAnimating = false;
    [SerializeField] private bool pauseAnimationFlag = false;
    [SerializeField] private bool resetButtonFlag = true;
    [SerializeField] private Quaternion originalRotation;
    [SerializeField] private Vector3 originalScale;
    [SerializeField] private Vector3 originalLocation;
    [SerializeField] private Vector3 highestJumpLocation;
    [SerializeField] private Sprite originalSprite;
    [SerializeField] private Color originalButtonImageColor;
    //private float animationRuntime = 4f;
    [SerializeField] private List<UnityAction> buttonActionListeners = new List<UnityAction>();
    // Start is called before the first frame update


    void OnEnable()
    {
        originalRotation = animatedButton.transform.rotation;
        originalScale = animatedButton.transform.localScale;
        originalSprite = animatedButton.GetComponent<Image>().sprite;
        originalButtonImageColor = animatedButton.GetComponent<Image>().color;
        originalLocation = animatedButton.transform.position;
    }
    void Start()
    {
        _cts = new CancellationTokenSource();
        AddListenerToButton(playButton);
        AddListenerToButton(pauseButton);
        AddListenerToButton(resetButton);
        AddListenerToButton(rescaleButton);
        AddListenerToButton(randomSpriteButton);
        AddListenerToButton(jumpButton);
        AddListenerToButton(spinButton);
        AddListenerToButton(fadeButton);
        AddListenerToButton(animatedButton);
        AddListenerToButton(runAllButton);
        AddListenerToButton(cancelButton);
    }

    // Update is called once per frame
    void Update()
    {
        resetButton.interactable = resetButtonFlag;

    }

    void OnDisable()
    {
        RemoveAllListenerOfButton(playButton);
        RemoveAllListenerOfButton(pauseButton);
        RemoveAllListenerOfButton(resetButton);
        RemoveAllListenerOfButton(rescaleButton);
        RemoveAllListenerOfButton(randomSpriteButton);
        RemoveAllListenerOfButton(jumpButton);
        RemoveAllListenerOfButton(spinButton);
        RemoveAllListenerOfButton(fadeButton);
        RemoveAllListenerOfButton(animatedButton);
        RemoveAllListenerOfButton(runAllButton);
        RemoveAllListenerOfButton(cancelButton);
    }

    private void ButtonClicked(Button button)
    {
        if (isAnimating)
        {
            if (button.gameObject.tag == "Cancel")
            {
                isAnimating = false;
                CancelAnimation();
            }
            else return;
        }


        Debug.Log(button.name + " is Clicked!");

        if (button.gameObject.tag == "Action Control Button")
        {
            currentActionControlButton = button;
            currentAnimationText.text = currentActionControlButton.name + " is Active";
            ActiveUI(animationControlUI);
            resetButtonFlag = true;
        }
        else if (button.gameObject.tag == "Animation Control Button")
        {
            switch (button.name)
            {
                case "Play Button":
                    isAnimating = true;
                    resetButtonFlag = true;
                    pauseAnimationFlag = false;
                    _cts = new CancellationTokenSource();
                    RunAnimation(_cts.Token);
                    break;
                case "Pause/Resume Button":
                    pauseAnimationFlag = !pauseAnimationFlag;
                    if (pauseAnimationFlag)
                        DOTween.PauseAll();
                    else
                        DOTween.PlayAll();
                    break;
                case "Reset Button":
                    resetButtonFlag = false;
                    isAnimating = false;
                    
                    ResetButtonState(animatedButton);
                    break;
            }
        }
        else if (button.gameObject.tag == "Untagged")
        {
            DisableUI(animationControlUI);
        }


    }

    async void RunAnimation(CancellationToken cancellationToken)
    {
        Debug.Log(currentActionControlButton.name + " animation is playing");


        if (cancellationToken.IsCancellationRequested)
            cancellationToken.ThrowIfCancellationRequested();

        switch (currentActionControlButton.name)
        {
            case "Scale Button":
                await ScalingButton(animatedButton, targetScale, 1, cancellationToken);
                isAnimating = false;
                break;
            case "Random Sprite Button":
                await ChangingToRandomSprites(animatedButton, sprites, 1, cancellationToken);
                isAnimating = false;
                break;
            case "Jump Button":
                await JumpButton(animatedButton, 1, cancellationToken);
                isAnimating = false;
                break;
            case "Spin Button":
                await SpinButton(animatedButton, 1, cancellationToken);
                isAnimating = false;
                break;
            case "Fade Button":
                await FadeButton(animatedButton, 1, cancellationToken);
                isAnimating = false;
                break;
            case "Run All Button":
                await RunAllAnimation(animatedButton, cancellationToken);
                isAnimating = false;
                break;
        }
    }


    private void ActiveUI(GameObject UI)
    {
        UI.SetActive(true);
        Debug.Log(UI.name + " is active!");
    }

    private void DisableUI(GameObject UI)
    {
        UI.SetActive(false);
        Debug.Log(UI.name + " is disabled!");
    }

    private void AddListenerToButton(Button button)
    {
        UnityAction listener = () => ButtonClicked(button);
        button.onClick.AddListener(listener);
        buttonActionListeners.Add(listener);
    }

    private void RemoveAllListenerOfButton(Button button)
    {
        foreach (var listener in buttonActionListeners)
        {
            button.onClick.RemoveListener(listener);
        }
        buttonActionListeners.Clear();
    }

    private void ResetButtonState(Button button)
    {
        button.transform.rotation = originalRotation;
        button.transform.localScale = originalScale;
        button.GetComponent<Image>().sprite = originalSprite;
        button.GetComponent<Image>().color = new Color(originalButtonImageColor.r, originalButtonImageColor.g, originalButtonImageColor.b);
        button.transform.position = originalLocation;
    }

    private void CancelAnimation()
    {
        _cts?.Cancel();
        _cts = new CancellationTokenSource();
    }

    private async UniTask ScalingButton(Button button, Vector3 targetScale, float duration, CancellationToken cancellationToken)
    {
        Vector3 originalScale = button.transform.localScale;

        if (cancellationToken.IsCancellationRequested)
            cancellationToken.ThrowIfCancellationRequested();

        var scaleUpTween = button.transform.DOScale(targetScale, duration).ToUniTask();
        await scaleUpTween;

        var scaleDownTween = button.transform.DOScale(originalScale, duration).ToUniTask();
        await scaleDownTween;
    }

    private async UniTask ChangingToRandomSprites(Button button, Sprite[] sprites, float duration, CancellationToken cancellationToken)
    {
        int randomIndex = Random.Range(0, sprites.Length);
        Sprite newSprite = sprites[randomIndex];
        Color buttonColor = button.GetComponent<Image>().color;

        if (cancellationToken.IsCancellationRequested)
            cancellationToken.ThrowIfCancellationRequested();

        var colorChangeTween = button.GetComponent<Image>().DOBlendableColor(buttonColor, duration);
        await colorChangeTween;

        button.GetComponent<Image>().sprite = newSprite;
    }

    private async UniTask JumpButton(Button button, float duration, CancellationToken cancellationToken)
    {
        Vector3 originalPosition = button.transform.position;
        highestJumpLocation = originalPosition;
        highestJumpLocation.y = originalPosition.y + 300;

        if (cancellationToken.IsCancellationRequested)
            cancellationToken.ThrowIfCancellationRequested();

        var jumpTween = button.transform.DOMove(highestJumpLocation, duration).ToUniTask();
        await jumpTween;

        var fallbackTween = button.transform.DOMove(originalPosition, duration).ToUniTask();
        await fallbackTween;
    }

    private async UniTask SpinButton(Button button, float duration, CancellationToken cancellationToken)
    {
        Quaternion originalRotation = button.transform.rotation;
        float totalRotation = 360f;

        if (cancellationToken.IsCancellationRequested)
            cancellationToken.ThrowIfCancellationRequested();

        var spinTween = button.transform.DORotate(new Vector3(0, 0, totalRotation), duration, RotateMode.FastBeyond360)
            .SetEase(Ease.Linear).ToUniTask();
        await spinTween;

        button.transform.rotation = originalRotation;
    }
    private async UniTask FadeButton(Button button, float duration, CancellationToken cancellationToken)
    {
        Image buttonImage = button.GetComponent<Image>();
        Color originalColor = buttonImage.color;

        if (cancellationToken.IsCancellationRequested)
            cancellationToken.ThrowIfCancellationRequested();

        var fadeTween = buttonImage.DOColor(new Color(originalColor.r, originalColor.g, originalColor.b, 0), duration)
            .SetEase(Ease.Linear).ToUniTask();
        await fadeTween;

        var unfadeTween = buttonImage.DOColor(originalColor, 1).ToUniTask();
        await unfadeTween;
    }

    private async UniTask RunAllAnimation(Button button, CancellationToken cancellationToken)
    {
        Debug.Log(button.name + " is playing all animation");

        if (cancellationToken.IsCancellationRequested)
            cancellationToken.ThrowIfCancellationRequested();

        await ScalingButton(button, targetScale, 1, cancellationToken);
        await ChangingToRandomSprites(button, sprites, 1, cancellationToken);
        await JumpButton(button, 1, cancellationToken);
        await SpinButton(button, 1, cancellationToken);
        await FadeButton(button, 1, cancellationToken);
    }
}