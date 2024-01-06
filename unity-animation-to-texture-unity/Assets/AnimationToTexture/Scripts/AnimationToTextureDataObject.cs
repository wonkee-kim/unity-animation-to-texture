using UnityEngine;

public class AnimationToTextureDataObject : ScriptableObject
{
    [System.Serializable]
    public class AnimationClipData
    {
        public string clipName;
        public Texture2D positionTexture;
        public Texture2D normalTexture;
        // public Texture2D tangentTexture; // Use this when use normal map

        public int frameCount; // texture height
        public int frameRate = 30;
        public float frameTime => 1f / frameRate;
        public float animationLength => (float)frameCount * frameTime;
        public float animationLengthInv => (float)frameRate / frameCount;

        public float isLooping = 1f; // 0: false, 1: true
    }

    [Header("Animation Data")]
    public string meshName;
    public Mesh mesh;
    public int vertexCount; // texture width
    public float texelSize => 1f / vertexCount;
    public AnimationClipData[] animationClipDatas; // max 4

    [Header("Material")]
    [Tooltip("This will override material on start for all renderers that use this animation data object.")]
    public Material materialOverride;
    public Material fallbackMaterial
    {
        get
        {
            if (_fallbackMaterial == null)
            {
                _fallbackMaterial = new Material(Shader.Find("AnimationToTexture/TextureAnimationExampleShader"));
            }
            return _fallbackMaterial;
        }
        set
        {
            _fallbackMaterial = value;
        }
    }
    private Material _fallbackMaterial;

    [Header("Shader Property Names")]
    [HideInInspector]
    public string positionTexture0PropertyName = "_AnimTexPos0";
    [HideInInspector] public string normalTexture0PropertyName = "_AnimTexNorm0";
    [HideInInspector] public string positionTexture1PropertyName = "_AnimTexPos1";
    [HideInInspector] public string normalTexture1PropertyName = "_AnimTexNorm1";
    [HideInInspector] public string positionTexture2PropertyName = "_AnimTexPos2";
    [HideInInspector] public string normalTexture2PropertyName = "_AnimTexNorm2";
    [HideInInspector] public string positionTexture3PropertyName = "_AnimTexPos3";
    [HideInInspector] public string normalTexture3PropertyName = "_AnimTexNorm3";

    [HideInInspector] public string texelSizePropertyName = "_TexelSize";
    [HideInInspector] public string animationParamsPropertyName = "_AnimParams";
}
