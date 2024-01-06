using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class AnimationToTextureRenderer : MonoBehaviour
{
    private static Dictionary<string, int> _materialPropertyIDs = new Dictionary<string, int>();

    [Header("Animation Data")]
    public AnimationToTextureDataObject animationDataObject;
    public Vector4 animationParams = new Vector4(0, 0, 1, 1); // x: index, y: time, z: animLengthInv, w: isLooping (0 or 1)

    [Header("References")]
    [SerializeField] private Animator _animator;
    [SerializeField] private SkinnedMeshRenderer _skinnedMeshRenderer; // original
    [SerializeField] private MeshRenderer _meshRenderer; // copy
    [SerializeField] private MeshFilter _meshFilter;
    public Renderer renderer => _meshRenderer;

    // Cache property IDs (Use this on Update)
    private int _animationParamsPropertyID;

    /// <summary>
    /// Play animation clip. No blending supported. AnimationClip count can be maximum 4. (0~3)
    /// </summary>
    public void PlayAnimationClip(int clipIndex, bool initialize = false)
    {
        if (clipIndex < 0 || clipIndex >= animationDataObject.animationClipDatas.Length || clipIndex >= 4)
        {
            int maxClipIndex = Mathf.Min(animationDataObject.animationClipDatas.Length - 1, 3);
            Debug.LogError($"Clip index({clipIndex}) out of range. Index should be 0 ~ {maxClipIndex}.");
            clipIndex = Mathf.Clamp(clipIndex, 0, maxClipIndex);
        }

        if (initialize || clipIndex != animationParams.x)
        {
            AnimationToTextureDataObject.AnimationClipData animationClipData = animationDataObject.animationClipDatas[clipIndex];
            // x: index, y: time, z: animLengthInv, w: isLooping (0 or 1)
            animationParams = new Vector4(
                clipIndex,
                Time.time,
                animationClipData.animationLengthInv,
                animationClipData.isLooping);
        }
        else
        {
            animationParams.y = Time.time;
        }
        _meshRenderer.material.SetVector(_animationParamsPropertyID, animationParams);
    }

    private void Awake()
    {
        Setup();
    }

    private void Setup()
    {
        if (_meshFilter == null)
        {
            _meshFilter = GetComponent<MeshFilter>();
        }
        if (_meshRenderer == null)
        {
            _meshRenderer = GetComponent<MeshRenderer>();
        }
        if (animationDataObject.materialOverride != null)
        {
            _meshRenderer.material = animationDataObject.materialOverride;
        }
        else
        {
            if (_meshRenderer.sharedMaterial == null)
            {
                _meshRenderer.material = animationDataObject.fallbackMaterial;
            }
        }
        _meshFilter.mesh = animationDataObject.mesh;
        _meshRenderer.enabled = true;
        _skinnedMeshRenderer.enabled = false;
        _animator.enabled = false;

        PlayAnimationClip(0, initialize: true); // Initialize parameters

        // Cache property IDs
        if (!_materialPropertyIDs.ContainsKey(animationDataObject.positionTexture0PropertyName))
        {
            _materialPropertyIDs.Add(animationDataObject.positionTexture0PropertyName, Shader.PropertyToID(animationDataObject.positionTexture0PropertyName));
        }
        if (!_materialPropertyIDs.ContainsKey(animationDataObject.normalTexture0PropertyName))
        {
            _materialPropertyIDs.Add(animationDataObject.normalTexture0PropertyName, Shader.PropertyToID(animationDataObject.normalTexture0PropertyName));
        }
        if (!_materialPropertyIDs.ContainsKey(animationDataObject.positionTexture1PropertyName))
        {
            _materialPropertyIDs.Add(animationDataObject.positionTexture1PropertyName, Shader.PropertyToID(animationDataObject.positionTexture1PropertyName));
        }
        if (!_materialPropertyIDs.ContainsKey(animationDataObject.normalTexture1PropertyName))
        {
            _materialPropertyIDs.Add(animationDataObject.normalTexture1PropertyName, Shader.PropertyToID(animationDataObject.normalTexture1PropertyName));
        }
        if (!_materialPropertyIDs.ContainsKey(animationDataObject.positionTexture2PropertyName))
        {
            _materialPropertyIDs.Add(animationDataObject.positionTexture2PropertyName, Shader.PropertyToID(animationDataObject.positionTexture2PropertyName));
        }
        if (!_materialPropertyIDs.ContainsKey(animationDataObject.normalTexture2PropertyName))
        {
            _materialPropertyIDs.Add(animationDataObject.normalTexture2PropertyName, Shader.PropertyToID(animationDataObject.normalTexture2PropertyName));
        }
        if (!_materialPropertyIDs.ContainsKey(animationDataObject.positionTexture3PropertyName))
        {
            _materialPropertyIDs.Add(animationDataObject.positionTexture3PropertyName, Shader.PropertyToID(animationDataObject.positionTexture3PropertyName));
        }
        if (!_materialPropertyIDs.ContainsKey(animationDataObject.normalTexture3PropertyName))
        {
            _materialPropertyIDs.Add(animationDataObject.normalTexture3PropertyName, Shader.PropertyToID(animationDataObject.normalTexture3PropertyName));
        }

        if (!_materialPropertyIDs.ContainsKey(animationDataObject.texelSizePropertyName))
        {
            _materialPropertyIDs.Add(animationDataObject.texelSizePropertyName, Shader.PropertyToID(animationDataObject.texelSizePropertyName));
        }
        if (!_materialPropertyIDs.ContainsKey(animationDataObject.animationParamsPropertyName))
        {
            _materialPropertyIDs.Add(animationDataObject.animationParamsPropertyName, Shader.PropertyToID(animationDataObject.animationParamsPropertyName));
        }

        int[] positionTexturePropertyIDs = new int[4]
        {
            _materialPropertyIDs[animationDataObject.positionTexture0PropertyName],
            _materialPropertyIDs[animationDataObject.positionTexture1PropertyName],
            _materialPropertyIDs[animationDataObject.positionTexture2PropertyName],
            _materialPropertyIDs[animationDataObject.positionTexture3PropertyName],
        };
        int[] normalTexturePropertyIDs = new int[4]
        {
            _materialPropertyIDs[animationDataObject.normalTexture0PropertyName],
            _materialPropertyIDs[animationDataObject.normalTexture1PropertyName],
            _materialPropertyIDs[animationDataObject.normalTexture2PropertyName],
            _materialPropertyIDs[animationDataObject.normalTexture3PropertyName],
        };
        int texelSizePropertyID = _materialPropertyIDs[animationDataObject.texelSizePropertyName];
        _animationParamsPropertyID = _materialPropertyIDs[animationDataObject.animationParamsPropertyName];

        for (int j = 0; j < animationDataObject.animationClipDatas.Length; j++)
        {
            if (animationDataObject.animationClipDatas[j] != null)
            {
                _meshRenderer.material.SetTexture(positionTexturePropertyIDs[j], animationDataObject.animationClipDatas[j].positionTexture);
                _meshRenderer.material.SetTexture(normalTexturePropertyIDs[j], animationDataObject.animationClipDatas[j].normalTexture);
            }
        }
        _meshRenderer.material.SetFloat(texelSizePropertyID, animationDataObject.texelSize); // 1/vertex count
        _meshRenderer.material.SetVector(_animationParamsPropertyID, animationParams);
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (_animator == null)
        {
            _animator = GetComponentInChildren<Animator>();
        }
        if (_skinnedMeshRenderer == null)
        {
            _skinnedMeshRenderer = GetComponentInChildren<SkinnedMeshRenderer>();
        }
        if (_skinnedMeshRenderer != null && animationDataObject != null && _skinnedMeshRenderer.sharedMesh != animationDataObject.mesh)
        {
            Debug.LogWarning($"SkinnedMeshRenderer.sharedMesh does not match animationDataObject.mesh on {gameObject.name}.");
        }

        if (_meshFilter == null)
        {
            _meshFilter = GetComponent<MeshFilter>();
        }
        if (_meshFilter != null && animationDataObject != null)
        {
            _meshFilter.mesh = animationDataObject.mesh;
        }
        if (_meshRenderer == null)
        {
            _meshRenderer = GetComponent<MeshRenderer>();
        }
        if (_meshRenderer != null)
        {
            if (_meshRenderer.sharedMaterial == null && animationDataObject != null)
            {
                _meshRenderer.sharedMaterial = animationDataObject.fallbackMaterial;
            }
            _meshRenderer.enabled = false;
        }
    }

    public void PlayAnimationClip0()
    {
        PlayAnimationClip(0);
    }
    public void PlayAnimationClip1()
    {
        PlayAnimationClip(1);
    }
    public void PlayAnimationClip2()
    {
        PlayAnimationClip(2);
    }
    public void PlayAnimationClip3()
    {
        PlayAnimationClip(3);
    }
#endif
}

#if UNITY_EDITOR
[CustomEditor(typeof(AnimationToTextureRenderer))]
public class AnimationToTextureRendererInspector : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        GUILayout.Space(10f);

        AnimationToTextureRenderer renderer = target as AnimationToTextureRenderer;

        if (GUILayout.Button(nameof(renderer.PlayAnimationClip0)))
        {
            renderer.PlayAnimationClip0();
        }

        EditorGUILayout.Space(5);
        if (GUILayout.Button(nameof(renderer.PlayAnimationClip1)))
        {
            renderer.PlayAnimationClip1();
        }

        EditorGUILayout.Space(5);
        if (GUILayout.Button(nameof(renderer.PlayAnimationClip2)))
        {
            renderer.PlayAnimationClip2();
        }

        EditorGUILayout.Space(5);
        if (GUILayout.Button(nameof(renderer.PlayAnimationClip3)))
        {
            renderer.PlayAnimationClip3();
        }
    }
}
#endif