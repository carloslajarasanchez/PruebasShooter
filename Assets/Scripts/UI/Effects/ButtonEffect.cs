using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class ButtonEffect : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    [Header("Sonidos")]
    public SoundType hoverSound = SoundType.UIHover;
    public SoundType clickSound = SoundType.UIClick;

    [Header("Glow - Hover")]
    public Color normalColor = Color.white;
    public Color glowColor = new Color(1f, 1f, 1f, 1f);
    public float glowTransitionSpeed = 8f;

    [Header("Shake - Click")]
    public float shakeStrength = 5f;
    public float shakeDuration = 0.2f;
    public int shakeVibrato = 10;

    [Header("Scale - Hover")]
    public float hoverScale = 1.05f;
    public float scaleSpeed = 8f;

    private Image _backgroundImage;
    private IAudioService _audioService;
    private RectTransform _rectTransform;

    private bool _isHovered = false;
    private Color _targetColor;
    private Vector3 _initialScale;
    private Vector3 _targetScale;
    private Coroutine _shakeCoroutine;

    private void Awake()
    {
        _audioService = AppContainer.Get<IAudioService>();
        _backgroundImage = GetComponent<Image>();
        _rectTransform = GetComponent<RectTransform>();

        _initialScale = _rectTransform.localScale;
        _targetScale = _initialScale;
        _targetColor = normalColor;

        if (_backgroundImage != null)
            _backgroundImage.color = normalColor;
    }

    private void Update()
    {
        // Interpolamos el color suavemente hacia el objetivo
        if (_backgroundImage != null)
            _backgroundImage.color = Color.Lerp(_backgroundImage.color, _targetColor, Time.deltaTime * glowTransitionSpeed);

        // Interpolamos la escala suavemente
        _rectTransform.localScale = Vector3.Lerp(_rectTransform.localScale, _targetScale, Time.deltaTime * scaleSpeed);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        _isHovered = true;
        _targetColor = glowColor;
        _targetScale = _initialScale * hoverScale;
        _audioService.Play(hoverSound);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        _isHovered = false;
        _targetColor = normalColor;
        _targetScale = _initialScale;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        _audioService.Play(clickSound);

        if (!gameObject.activeInHierarchy) return;

        if (_shakeCoroutine != null)
            StopCoroutine(_shakeCoroutine);
        _shakeCoroutine = StartCoroutine(ShakeCoroutine());
    }

    private IEnumerator ShakeCoroutine()
    {
        Vector3 originalPosition = _rectTransform.localPosition;
        float elapsed = 0f;
        float interval = shakeDuration / shakeVibrato;

        while (elapsed < shakeDuration)
        {
            float strength = Mathf.Lerp(shakeStrength, 0f, elapsed / shakeDuration); // se aten�a
            float offsetX = Random.Range(-strength, strength);
            float offsetY = Random.Range(-strength, strength);

            _rectTransform.localPosition = originalPosition + new Vector3(offsetX, offsetY, 0f);

            elapsed += interval;
            yield return new WaitForSeconds(interval);
        }

        _rectTransform.localPosition = originalPosition;
        _shakeCoroutine = null;
    }
}