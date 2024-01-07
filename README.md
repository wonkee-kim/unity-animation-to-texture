# unity-animation-to-texture
A tool for baking SkinnedMesh animation clip data into textures, including vertex positions and normals.



### Demo (Available on Web, Mobile and MetaQuest - Powered by [Spatial Creator Toolkit](https://www.spatial.io/toolkit))
https://www.spatial.io/s/Animation-To-Texture-Demo-6599ea775c6853df735e9cd7



### Performance Improvement
**7 times faster** performance with 2,500 animated models, going from 10fps (100ms) to 70fps (14ms).
<img src="https://github.com/wonkee-kim/unity-animation-to-texture/assets/830808/a31d2fb2-0994-4aaf-8634-5227645639d6" height="160">  
<img src="https://github.com/wonkee-kim/unity-animation-to-texture/assets/830808/1a3f8af3-7e91-4bc7-9d81-a1d00802b8b5" height="160">  



## How it works
This tool gets animation clips from AnimatorController, bakes vertex position and normal into textures and update position and normal in vertex shader. 
Texture width is vertex count, and height is frame count of the animation clips. Thus, maintaining a low vertex count for the model can save a significant amount of texture memory.  
<img src="https://github.com/wonkee-kim/unity-animation-to-texture/assets/830808/be42838b-c11c-4388-b888-d18ebcf3c49e" height="80">  



## How to use
https://github.com/wonkee-kim/unity-animation-to-texture/assets/830808/fcfc7c4d-0bbb-4634-9e48-6fff803ad6a5


1. SkinnedMesh model, that has Animator and AnimatorController with animation clips.  
<img src="https://github.com/wonkee-kim/unity-animation-to-texture/assets/830808/46720a4e-213d-4a50-bc73-faa4ee3af308" height="160">  


2. Attach the [AnimationToTextureDataGenerator](https://github.com/wonkee-kim/unity-animation-to-texture/blob/main/unity-animation-to-texture-unity/Assets/AnimationToTexture/Scripts/AnimationToTextureDataGenerator.cs) to the gameObject. (Recommended attaching it to the root gameObject)  
<img src="https://github.com/wonkee-kim/unity-animation-to-texture/assets/830808/4dfdd14c-9d12-4eb9-a36c-db8a3dc25b96" height="160">  


3. Assign the root gameObject to the 'Target GameObject' field.  
<img src="https://github.com/wonkee-kim/unity-animation-to-texture/assets/830808/480b9850-4fc1-42c8-b7ca-2b1ff5fd7060" height="160">  


4. Click the ‘SetupGenerator’ button to obtain reference components automatically.  
<img src="https://github.com/wonkee-kim/unity-animation-to-texture/assets/830808/1cdd35a8-f616-41e4-ac1a-92ad85a86dac" height="160">  


5. Verify that references are imported properly: Animator, SkinnedMeshRenderer and AnimationClipNames. (Maximum 4 animation clips are supported)  
<img src="https://github.com/wonkee-kim/unity-animation-to-texture/assets/830808/94c0c541-8134-42ec-a80a-de5dd6ed548a" height="160">  


6. Click 'Add Renderer after generation' if you want it to be set automatically, then click the 'GenerateAnimationData' button to generate animation data.  
<img src="https://github.com/wonkee-kim/unity-animation-to-texture/assets/830808/e6a831dd-6c96-4583-b916-f51d6397ee2c" height="160">  


7. Generated data will be shown below the button.  
<img src="https://github.com/wonkee-kim/unity-animation-to-texture/assets/830808/f10d0684-e97d-420a-b0bd-0729e26a642b" height="160">  


8. Play to test. The Animator and SkinnedMeshRenderer components will be disabled, and the MeshRenderer will be enabled automatically by [AnimationToTextureRenderer](https://github.com/wonkee-kim/unity-animation-to-texture/blob/main/unity-animation-to-texture-unity/Assets/AnimationToTexture/Scripts/AnimationToTextureRenderer.cs).  
<img src="https://github.com/wonkee-kim/unity-animation-to-texture/assets/830808/6db009e6-d7d9-49e7-aecb-cb2e1737dac2" height="160">  
<img src="https://github.com/wonkee-kim/unity-animation-to-texture/assets/830808/b23cd472-6e1f-44da-af16-b32c259f1479" height="160">
<img src="https://github.com/wonkee-kim/unity-animation-to-texture/assets/830808/ff13b2ca-a150-478c-96c0-b24188992325" height="160">


9. You can test animation clips through buttons in [AnimationToTextureRenderer](https://github.com/wonkee-kim/unity-animation-to-texture/blob/main/unity-animation-to-texture-unity/Assets/AnimationToTexture/Scripts/AnimationToTextureRenderer.cs).  
Use [AnimationToTextureRenderer.PlayAnimationClip(int clipIndex)](https://github.com/wonkee-kim/unity-animation-to-texture/blob/main/unity-animation-to-texture-unity/Assets/AnimationToTexture/Scripts/AnimationToTextureRenderer.cs#L31) to play animation clips in C# scripts.
<img src="https://github.com/wonkee-kim/unity-animation-to-texture/assets/830808/53ee58f2-c3d3-429a-99d3-afa61ea702ab" height="160">



## Shader
There are two shader examples,
- HLSL example: [TextureAnimationExampleShader](https://github.com/wonkee-kim/unity-animation-to-texture/blob/main/unity-animation-to-texture-unity/Assets/AnimationToTexture/Shaders/TextureAnimationExampleShader.shader)
- ShaderGraph example: [TextureAnimationShaderGraphExample](https://github.com/wonkee-kim/unity-animation-to-texture/blob/main/unity-animation-to-texture-unity/Assets/AnimationToTexture/Shaders/TextureAnimationShaderGraphExample.shadergraph)

### HLSL
Include [TextureAnimation.hlsl](https://github.com/wonkee-kim/unity-animation-to-texture/blob/main/unity-animation-to-texture-unity/Assets/AnimationToTexture/Shaders/TextureAnimation.hlsl) in your shader. ([example](https://github.com/wonkee-kim/unity-animation-to-texture/blob/main/unity-animation-to-texture-unity/Assets/AnimationToTexture/Shaders/TextureAnimationExampleShader.shader#L35))
```hlsl
#include "./TextureAnimation.hlsl" // Make sure the path is correct
```
and get position and normal by using `TEXTURE_ANIMATION_OUTPUT(positionOS, normalOS, vertexID)` in vertex shader. ([example](https://github.com/wonkee-kim/unity-animation-to-texture/blob/main/unity-animation-to-texture-unity/Assets/AnimationToTexture/Shaders/TextureAnimationExampleShader.shader#L71-L73))
```hlsl
float3 positionOS;
float3 normalOS;
TEXTURE_ANIMATION_OUTPUT(positionOS, normalOS, vertexID);
```

### ShaderGraph
Retrieve the [TextureAnimationSubGraph](https://github.com/wonkee-kim/unity-animation-to-texture/blob/main/unity-animation-to-texture-unity/Assets/AnimationToTexture/Shaders/TextureAnimationSubGraph.shadersubgraph) subgraph and connect it to the Position and Normal in the Vertex block.
<img src="https://github.com/wonkee-kim/unity-animation-to-texture/assets/830808/1db5de29-a1b6-4313-bfbd-9758f8783dbf" height="160">  
