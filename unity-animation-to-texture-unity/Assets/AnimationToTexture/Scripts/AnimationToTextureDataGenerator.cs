using UnityEngine;

#if UNITY_EDITOR
using System;
using System.IO;
using UnityEditor;
#endif

public class AnimationToTextureDataGenerator : MonoBehaviour
{
    [Tooltip("The target root GameObject that contains SkinnedMeshRenderer and Animator components.")]
    [SerializeField] private GameObject _targetGameObject;

    [Tooltip("If this is true, AnimationToTextureRenderer component will be added to the target GameObject after generation.")]
    [SerializeField] private bool _addRenderer = false;

    // References
    [SerializeField] private SkinnedMeshRenderer _skinnedMeshRenderer;
    [SerializeField] private Animator _animator;
    [Tooltip("Animation clip names that will be used to generate animation data. Maximum 4.")]
    [SerializeField] private string[] _animationClipNames; // maximum 4
    public string[] animationClipNames => _animationClipNames;
    [Tooltip("Default material that will be used when AnimationToTextureRenderer component is added to the target GameObject.")]
    [SerializeField] private Material _fallbackMaterial;

    [Tooltip("Generated AnimationToTextureDataObject.")]
    [SerializeField] private AnimationToTextureDataObject _animationDataObject;
    public AnimationToTextureDataObject animationDataObject => _animationDataObject;

#if UNITY_EDITOR
    public void SetupGenerator()
    {
        _skinnedMeshRenderer = _targetGameObject.GetComponentInChildren<SkinnedMeshRenderer>();
        _animator = _targetGameObject.GetComponentInChildren<Animator>();
        GetClipNamesFromAnimatorController();
    }

    public void GetClipNamesFromAnimatorController()
    {
        if (_animator.runtimeAnimatorController == null)
        {
            Debug.LogError("No Animator Controller found in the Animator component.");
            return;
        }
        else
        {
            _animationClipNames = Array.ConvertAll(_animator.runtimeAnimatorController.animationClips, c => c.name);
            if (_animationClipNames.Length > 4)
            {
                Debug.LogWarning("Maximum 4 animation clips are supported. Only the first 4 will be used.");
            }
        }
    }

    public void GenerateAnimationData()
    {
        string path = GetFilePath() + _skinnedMeshRenderer.sharedMesh.name + "/";
        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
        }

        int vertexCount = _skinnedMeshRenderer.sharedMesh.vertexCount;

        int clipCount = Mathf.Min(_animationClipNames.Length, 4); // maximum 4
        AnimationToTextureDataObject.AnimationClipData[] animationClipDatas = new AnimationToTextureDataObject.AnimationClipData[clipCount];
        for (int clipIndex = 0; clipIndex < clipCount; clipIndex++)
        {
            // Get Animation Clip
            string animationClipName = _animationClipNames[clipIndex];
            AnimationClip animationClip;
            if (String.IsNullOrEmpty(animationClipName))
            {
                animationClip = _animator.runtimeAnimatorController.animationClips[0];
            }
            else
            {
                animationClip = Array.Find(_animator.runtimeAnimatorController.animationClips, c => c.name == animationClipName);
            }

            // Get Positions and Normals
            float animationLength = animationClip.length;
            int frameRate = Mathf.CeilToInt(animationClip.frameRate);
            int frameCount = Mathf.CeilToInt(animationLength * frameRate);
            bool isLooping = animationClip.isLooping;

            Vector3[][] positions = new Vector3[frameCount][];
            Vector3[][] normals = new Vector3[frameCount][];

            Mesh targetMesh = new Mesh();

            _animator.enabled = false; // disable animator to sample animation
            for (int i = 0; i < frameCount; i++) // ignore the last frame
            {
                float time = i / (float)frameRate;
                animationClip.SampleAnimation(_animator.gameObject, time);
                _skinnedMeshRenderer.BakeMesh(targetMesh);

                positions[i] = targetMesh.vertices;
                normals[i] = targetMesh.normals;
            }
            _animator.enabled = true; // turn it back on

            // Convert to Texture
            Color[] positionColors = new Color[vertexCount * frameCount];
            Color[] normalColors = new Color[vertexCount * frameCount];
            for (int i = 0; i < frameCount; i++)
            {
                for (int j = 0; j < vertexCount; j++)
                {
                    positionColors[i * vertexCount + j] = new Color(positions[i][j].x, positions[i][j].y, positions[i][j].z, 0f);
                    normalColors[i * vertexCount + j] = new Color(normals[i][j].x, normals[i][j].y, normals[i][j].z, 0f);
                }
            }

            Texture2D positionTexture = new Texture2D(vertexCount, frameCount, TextureFormat.RGBAHalf, false);
            positionTexture.filterMode = FilterMode.Bilinear;
            positionTexture.wrapMode = isLooping ? TextureWrapMode.Repeat : TextureWrapMode.Clamp;
            Texture2D normalTexture = new Texture2D(vertexCount, frameCount, TextureFormat.RGBAHalf, false);
            normalTexture.filterMode = FilterMode.Bilinear;
            normalTexture.wrapMode = isLooping ? TextureWrapMode.Repeat : TextureWrapMode.Clamp;
            positionTexture.SetPixels(positionColors);
            normalTexture.SetPixels(normalColors);
            positionTexture.Apply();
            normalTexture.Apply();

            // Save texture asset
            AssetDatabase.CreateAsset(positionTexture, path + $"clip_{clipIndex}_{animationClip.name}_pos.asset");
            AssetDatabase.CreateAsset(normalTexture, path + $"clip_{clipIndex}_{animationClip.name}_norm.asset");

            animationClipDatas[clipIndex] = new AnimationToTextureDataObject.AnimationClipData()
            {
                clipName = animationClip.name,
                positionTexture = positionTexture,
                normalTexture = normalTexture,
                frameCount = frameCount,
                frameRate = frameRate,
                isLooping = isLooping ? 1f : 0f,
            };

            // Destroy temporary mesh
            DestroyImmediate(targetMesh);
        }

        // Save to ScriptableObject
        AnimationToTextureDataObject animationDataObject = CreateOrLoadDataScriptableObject<AnimationToTextureDataObject>(path + $"{nameof(AnimationToTextureDataObject)}_{_skinnedMeshRenderer.sharedMesh.name}.asset");
        animationDataObject.meshName = _skinnedMeshRenderer.sharedMesh.name;
        animationDataObject.mesh = _skinnedMeshRenderer.sharedMesh;
        animationDataObject.vertexCount = vertexCount;
        animationDataObject.animationClipDatas = animationClipDatas;
        animationDataObject.fallbackMaterial = _fallbackMaterial;

        _animationDataObject = animationDataObject;

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        if (_addRenderer)
        {
            AnimationToTextureRenderer renderer = _targetGameObject.AddComponent<AnimationToTextureRenderer>();
            renderer.animationDataObject = animationDataObject;
        }
    }

    private string GetFilePath()
    {
        string[] res = System.IO.Directory.GetFiles(Application.dataPath, nameof(AnimationToTextureDataObject) + ".cs", SearchOption.AllDirectories);
        if (res.Length == 0)
        {
            return "Assets/" + nameof(AnimationToTextureDataObject) + "/";
        }
        else
        {
            string path = res[0];
            path = path.Replace('\\', '/');
            path = path.Replace(Application.dataPath, "Assets");
            path = path.Substring(0, path.LastIndexOf('/') + 1);
            path += nameof(AnimationToTextureDataObject) + "/";
            return path;
        }
    }

    private T CreateOrLoadDataScriptableObject<T>(string assetPath) where T : ScriptableObject
    {
        T dataObject = AssetDatabase.LoadAssetAtPath<T>(assetPath);
        if (dataObject == null)
        {
            dataObject = ScriptableObject.CreateInstance<T>();
            AssetDatabase.CreateAsset(dataObject, assetPath);

            dataObject = AssetDatabase.LoadAssetAtPath<T>(assetPath);
        }

        return dataObject;
    }
#endif
}

#if UNITY_EDITOR
[CustomEditor(typeof(AnimationToTextureDataGenerator))]
public class AnimationToTextureDataGeneratorInspector : Editor
{
    private SerializedProperty targetGameObject;

    private SerializedProperty addRenderer;

    private bool showReferences = false;
    private SerializedProperty skinnedMeshRenderer;
    private SerializedProperty animator;
    private SerializedProperty animationClipNames;
    private SerializedProperty fallbackMaterial;

    private SerializedProperty animationDataObject;

    private void OnEnable()
    {
        targetGameObject = serializedObject.FindProperty("_targetGameObject");

        addRenderer = serializedObject.FindProperty("_addRenderer");

        skinnedMeshRenderer = serializedObject.FindProperty("_skinnedMeshRenderer");
        animator = serializedObject.FindProperty("_animator");
        animationClipNames = serializedObject.FindProperty("_animationClipNames");
        fallbackMaterial = serializedObject.FindProperty("_fallbackMaterial");

        animationDataObject = serializedObject.FindProperty("_animationDataObject");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        var generator = target as AnimationToTextureDataGenerator;

        EditorGUI.BeginDisabledGroup(true);
        EditorGUILayout.ObjectField("Script", MonoScript.FromMonoBehaviour((AnimationToTextureDataGenerator)target), typeof(AnimationToTextureDataGenerator), false);
        EditorGUI.EndDisabledGroup();

        // 1. Root
        GUILayout.Space(10f);
        GUILayout.Label("1. Assign Target GameObject and click 'SetupGenerator' button", EditorStyles.boldLabel);
        GUILayout.Space(2f);
        EditorGUI.indentLevel += 1;
        EditorGUILayout.PropertyField(targetGameObject, new GUIContent("Target GameObject"));
        if (GUILayout.Button(nameof(generator.SetupGenerator)))
        {
            generator.SetupGenerator();
        }
        EditorGUI.indentLevel -= 1;

        // 2. Check
        GUILayout.Space(20f);
        GUILayout.Label("2. Check animation clips and references", EditorStyles.boldLabel);
        GUILayout.Space(2f);
        EditorGUI.indentLevel += 1;
        if (generator.animationClipNames != null)
        {
            string clipInfo = "";
            if (generator.animationClipNames.Length <= 0)
            {
                clipInfo = "No animation clip found";
            }
            else
            {
                if (generator.animationClipNames.Length > 4)
                {
                    clipInfo = "(Warning!) Maximum 4 animation clips are supported. Only the first 4 will be used.\n\n";
                }
                clipInfo += $"Clip Count: {generator.animationClipNames.Length}";
                for (int i = 0; i < generator.animationClipNames.Length; i++)
                {
                    clipInfo += $"\n[{i}] {generator.animationClipNames[i]}";
                    if (i >= 4)
                    {
                        clipInfo += " (Ignored)";
                    }
                }
            }
            GUILayout.Label(clipInfo, EditorStyles.helpBox);
        }

        // References
        GUILayout.BeginVertical(EditorStyles.helpBox);
        showReferences = EditorGUILayout.Foldout(showReferences, "References");
        if (showReferences)
        {
            EditorGUI.indentLevel += 1;
            EditorGUILayout.PropertyField(skinnedMeshRenderer, new GUIContent("skinnedMeshRenderer"));
            EditorGUILayout.PropertyField(animator, new GUIContent("animator"));
            EditorGUILayout.PropertyField(animationClipNames, new GUIContent("animationClipNames"));
            if (GUILayout.Button(nameof(generator.GetClipNamesFromAnimatorController)))
            {
                generator.GetClipNamesFromAnimatorController();
            }
            GUILayout.Space(5f);
            EditorGUILayout.PropertyField(fallbackMaterial, new GUIContent("fallbackMaterial"));
            EditorGUI.indentLevel -= 1;
            GUILayout.Space(5f);
        }
        GUILayout.EndVertical();
        EditorGUI.indentLevel -= 1;

        // 3. Generate
        GUILayout.Space(20f);
        GUILayout.Label("3. Click 'GenerateAnimationData' button to generate animation data", EditorStyles.boldLabel);
        GUILayout.Space(2f);
        EditorGUI.indentLevel += 1;
        EditorGUILayout.PropertyField(addRenderer, new GUIContent("Add Renderer after generation"));
        if (GUILayout.Button(nameof(generator.GenerateAnimationData)))
        {
            generator.GenerateAnimationData();
        }
        EditorGUI.indentLevel -= 1;

        // Generated Data
        if (generator.animationDataObject != null)
        {
            GUILayout.Space(20f);
            GUILayout.Label("Generated Data", EditorStyles.boldLabel);
            GUILayout.Space(2f);
            EditorGUI.indentLevel += 1;
            EditorGUILayout.PropertyField(animationDataObject, new GUIContent("animationDataObject"));
            EditorGUI.indentLevel -= 1;
        }

        GUILayout.Space(20f);
        serializedObject.ApplyModifiedProperties();
    }
}
#endif