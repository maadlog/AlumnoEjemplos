/**************************************************************************************/
/* Variables comunes */
/**************************************************************************************/

//Matrices de transformacion
float4x4 matWorld; //Matriz de transformacion World
float4x4 matWorldView; //Matriz World * View
float4x4 matWorldViewProj; //Matriz World * View * Projection
float4x4 matInverseTransposeWorld; //Matriz Transpose(Invert(World))

//Textura para DiffuseMap
texture texDiffuseMap;
sampler2D diffuseMap = sampler_state
{
	Texture = (texDiffuseMap);
	ADDRESSU = WRAP;
	ADDRESSV = WRAP;
	MINFILTER = LINEAR;
	MAGFILTER = LINEAR;
	MIPFILTER = LINEAR;
};

float health;
float g_time;

/*********************************************** Vertex Shaders ***************************************************/

struct VS_OUTPUT
{
	float4 Position : POSITION;
	float4 Color : COLOR0;
	float2 TexCoord : TEXCOORD0;
};

VS_OUTPUT VS_health (
	float4 Position : POSITION,
	float3 Normal : NORMAL,
	float4 Color : COLOR,
	float2 TexCoord : TEXCOORD0
)
{
	VS_OUTPUT Out = (VS_OUTPUT)0;

	Out.Position = mul(Position, matWorldViewProj);
	Out.Color = Color;
	Out.TexCoord = TexCoord;
	
	return Out;
}


/*********************************************** Pixel Shaders ***************************************************/

float4 PS_health(VS_OUTPUT In): COLOR
{
	return tex2D(diffuseMap, In.TexCoord);
}

/*********************************************** Techniques ***************************************************/

technique HealthDependentShading {
	pass p0 {
		VertexShader = compile vs_2_0 VS_health();
		PixelShader = compile ps_2_0 PS_health();
	}
}

