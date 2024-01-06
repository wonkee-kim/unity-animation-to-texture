using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GroupDemo : MonoBehaviour
{
    [Header("Renderers")]
    [SerializeField] private AnimationToTextureRenderer[] _renderers;
    [SerializeField] private MeshRenderer[] _mrs;
    [SerializeField] private SkinnedMeshRenderer[] _smrs;
    [SerializeField] private Animator[] _anims;

    [Header("UI")]
    [SerializeField] private TextMeshProUGUI _textCount;
    [SerializeField] private TextMeshProUGUI _textFPS;
    [SerializeField] private Toggle _toggleUseTextureAnimation;
    [SerializeField] private TextMeshProUGUI _textToggleUseTextureAnimation;

    [SerializeField] private Button _buttonPlayAnimationClip0;
    [SerializeField] private Button _buttonPlayAnimationClip1;

    [SerializeField] private RectTransform _anchorTransform;
    private Vector2 _anchorPosition;
    private Rect _safeAreaCache;

    private const float FPS_UPDATE_INTERVAL = 0.5f;
    private float _fpsAccumulator = 0f;
    private int _fpsFrames = 0;

    [ContextMenu(nameof(GetAllAnimationRenderer))]
    public void GetAllAnimationRenderer()
    {
        _renderers = this.GetComponentsInChildren<AnimationToTextureRenderer>();
        _mrs = this.GetComponentsInChildren<MeshRenderer>();
        _smrs = this.GetComponentsInChildren<SkinnedMeshRenderer>();
        _anims = this.GetComponentsInChildren<Animator>();
    }

    private void Awake()
    {
        GetAllAnimationRenderer();

        _textCount.text = $"Model Count: {_renderers.Length.ToString("N0")}";
        _toggleUseTextureAnimation.isOn = true;
        _toggleUseTextureAnimation.onValueChanged.AddListener(OnToggleUseTextureAnimation);
        _buttonPlayAnimationClip0.onClick.AddListener(PlayAnimationClip0);
        _buttonPlayAnimationClip1.onClick.AddListener(PlayAnimationClip1);

        _anchorPosition = _anchorTransform.anchoredPosition;
        _safeAreaCache = Screen.safeArea;
        _anchorTransform.anchoredPosition = _anchorPosition + new Vector2(_safeAreaCache.xMin, 0);
    }

    private void Update()
    {
        // Shortcuts
        if (Input.GetKeyDown(KeyCode.F))
        {
            _toggleUseTextureAnimation.isOn = !_toggleUseTextureAnimation.isOn;
        }
        if (Input.GetKeyDown(KeyCode.Z))
        {
            _buttonPlayAnimationClip0.onClick.Invoke();
        }
        if (Input.GetKeyDown(KeyCode.X))
        {
            _buttonPlayAnimationClip1.onClick.Invoke();
        }

        // Update FPS
        _fpsAccumulator += Time.deltaTime;
        _fpsFrames++;
        if (_fpsAccumulator >= FPS_UPDATE_INTERVAL)
        {
            _textFPS.text = $"{_fpsFrames / _fpsAccumulator:0.0} fps ({_fpsAccumulator * 1000 / _fpsFrames:0.0} ms)";
            _fpsAccumulator = 0f;
            _fpsFrames = 0;
        }

        // Update Safe Area
        if (_safeAreaCache.xMin != Screen.safeArea.xMin)
        {
            _safeAreaCache = Screen.safeArea;
            _anchorTransform.anchoredPosition = _anchorPosition + new Vector2(_safeAreaCache.xMin, 0);
        }
    }

    private void OnToggleUseTextureAnimation(bool useTextureAnimation)
    {
        string onOff = useTextureAnimation ? "On" : "Off";
        _textToggleUseTextureAnimation.text = $"[F] Toggle Renderer ({onOff})";
        foreach (var mr in _mrs)
        {
            mr.enabled = useTextureAnimation;
        }
        foreach (var smr in _smrs)
        {
            smr.enabled = !useTextureAnimation;
        }
        foreach (var anim in _anims)
        {
            anim.enabled = !useTextureAnimation;
        }
    }

    private void PlayAnimationClip0()
    {
        PlayAnimationClip(0);
    }
    private void PlayAnimationClip1()
    {
        PlayAnimationClip(1);
    }
    private void PlayAnimationClip(int clipIndex)
    {
        foreach (var renderer in _renderers)
        {
            renderer.PlayAnimationClip(clipIndex);
        }
    }
}
