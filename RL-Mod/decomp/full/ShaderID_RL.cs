using UnityEngine;

public class ShaderID_RL
{
	public static readonly int _AlphaBlendColor = Shader.PropertyToID("_AlphaBlendColor");

	public static readonly int _RimLightColor = Shader.PropertyToID("_RimLightColor");

	public static readonly int _MultiplyColor = Shader.PropertyToID("_MultiplyColor");

	public static readonly int _AddColor = Shader.PropertyToID("_AddColor");

	public static readonly int _MainColor = Shader.PropertyToID("_MainColor");

	public static readonly int _ShieldToggle = Shader.PropertyToID("_ShieldToggle");

	public static readonly int _Dissolve = Shader.PropertyToID("_Dissolve");

	public static readonly int _Opacity = Shader.PropertyToID("_Opacity");

	public static readonly int _RimBias = Shader.PropertyToID("_RimBias");

	public static readonly int _RimScale = Shader.PropertyToID("_RimScale");

	public static readonly int _ArmorColor = Shader.PropertyToID("_ArmorColor");

	public static readonly int _CapeColor = Shader.PropertyToID("_CapeColor");

	public static readonly int _HelmetColor = Shader.PropertyToID("_HelmetColor");

	public static readonly int _OutlineScale = Shader.PropertyToID("_OutlineScale");

	public static int GetShaderID(ShaderID shaderID)
	{
		return shaderID switch
		{
			ShaderID.AlphaBlendColor => _AlphaBlendColor, 
			ShaderID.RimLightColor => _RimLightColor, 
			ShaderID.MultiplyColor => _MultiplyColor, 
			ShaderID.AddColor => _AddColor, 
			ShaderID.MainColor => _MainColor, 
			ShaderID.ShieldToggle => _ShieldToggle, 
			ShaderID.Dissolve => _Dissolve, 
			ShaderID.Opacity => _Opacity, 
			ShaderID.RimBias => _RimBias, 
			ShaderID.RimScale => _RimScale, 
			ShaderID.ArmorColor => _ArmorColor, 
			ShaderID.CapeColor => _CapeColor, 
			ShaderID.HelmetColor => _HelmetColor, 
			ShaderID.OutlineScale => _OutlineScale, 
			_ => -1, 
		};
	}
}
