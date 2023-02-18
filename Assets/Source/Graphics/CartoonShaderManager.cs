using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Cartoon Settings", menuName = "Graphics/Cartoon Settings", order = 0)]
public class CartoonShaderManager : ScriptableObject
{
    [Header("Outline")]
    public Color outlineColor;
    [Min(0)] public float outlineWidth;
    [Min(0)] public float outlineIntensity;
    [Range(1, 2)]
    public float outlineFade;

    [Header("Shading")]
    [Min(1)] public float cellsAmount;
    [Range(0, 1)] public float cellFade;
    public Color specularColor;
    public float shadowStart;
    public float shadowEnd;
    public Color shadowColor;

    public CartoonShaderManager()
    {
        // default outline 
        outlineColor = Color.black;
        outlineWidth = 1;
        outlineIntensity = 80000;
        outlineFade = 1.5f;

        // default shading
        cellsAmount = 2;
        specularColor = Color.white;
        shadowColor = Color.grey;
    }

    private void OnEnable()
    {
        UpdateAllShaders();
      
    }

    private void OnValidate()
    {
        UpdateAllShaders();
    }

    public void UpdateAllShaders()
    {
        // outline shading
        Shader.SetGlobalColor("_Outline_Color", outlineColor);
        Shader.SetGlobalFloat("_Outline_Width", outlineWidth);
        Shader.SetGlobalFloat("_Outline_Intensity", outlineIntensity);
        Shader.SetGlobalFloat("_Outline_Fade", outlineFade);

        // shading
        Shader.SetGlobalFloat("_Cartoon_Cells", cellsAmount);
        Shader.SetGlobalFloat("_Shadow_Cell_Fade", cellFade);
        Shader.SetGlobalFloat("_Shadow_Start", shadowStart);
        Shader.SetGlobalFloat("_Shadow_End", shadowEnd);
        Shader.SetGlobalColor("_Cartoon_Specular_Color", specularColor);
        Shader.SetGlobalColor("_Shadow_Color", shadowColor);
    }
}
