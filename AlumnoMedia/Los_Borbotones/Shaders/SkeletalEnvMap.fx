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

//Textura utilizada para BumpMapping
texture texNormalMap;
sampler2D normalMap = sampler_state
{
	Texture = (texNormalMap);
	ADDRESSU = WRAP;
	ADDRESSV = WRAP;
	MINFILTER = LINEAR;
	MAGFILTER = LINEAR;
	MIPFILTER = LINEAR;
};

//Textura utilizada para EnvironmentMap
texture texCubeMap;
samplerCUBE cubeMap = sampler_state
{
	Texture = (texCubeMap);
	ADDRESSU = WRAP;
	ADDRESSV = WRAP;
	MINFILTER = LINEAR;
	MAGFILTER = LINEAR;
	MIPFILTER = LINEAR;
};

//Material del mesh
float3 materialEmissiveColor; //Color RGB
float3 materialAmbientColor; //Color RGB
float4 materialDiffuseColor; //Color ARGB (tiene canal Alpha)
float3 materialSpecularColor; //Color RGB
float materialSpecularExp; //Exponente de specular

//Parametros de la Luz
float3 lightColor; //Color RGB de la luz
float4 lightPosition; //Posicion de la luz
float4 eyePosition; //Posicion de la camara
float lightIntensity; //Intensidad de la luz
float lightAttenuation; //Factor de atenuacion de la luz

//Intensidad de efecto Bump
float bumpiness;
const float3 BUMP_SMOOTH = { 0.5f, 0.5f, 1.0f };

//Factor de reflexion
float reflection;

//Matrix Pallette para skinning
static const int MAX_MATRICES = 26;
float4x3 bonesMatWorldArray[MAX_MATRICES];

/**************************************************************************************/
/* DIFFUSE_MAP */
/**************************************************************************************/

//Input del Vertex Shader
struct VS_INPUT_DIFFUSE_MAP
{
	float4 Position : POSITION0;
	float4 Color : COLOR;
	float2 Texcoord : TEXCOORD0;
	float3 Normal :   NORMAL0;
	float3 Tangent : TANGENT0;
	float3 Binormal : BINORMAL0;
	float4 BlendWeights : BLENDWEIGHT;
    float4 BlendIndices : BLENDINDICES;

};


//Output del Vertex Shader
struct VS_OUTPUT_DIFFUSE_MAP
{
	float4 Position : POSITION0;
	float4 Color : COLOR0; 
	float2 Texcoord : TEXCOORD0;
	float3 WorldNormal : TEXCOORD1;
    float3 WorldTangent	: TEXCOORD2;
    float3 WorldBinormal : TEXCOORD3;
	float3 WorldPosition : TEXCOORD4;
	float3 LightVec	: TEXCOORD5;
};


//Vertex Shader
VS_OUTPUT_DIFFUSE_MAP vs_DiffuseMap(VS_INPUT_DIFFUSE_MAP input)
{
	VS_OUTPUT_DIFFUSE_MAP output;

	//Pasar indices de float4 a array de int
	int BlendIndicesArray[4] = (int[4])input.BlendIndices;
	
	//Skinning de posicion
	float3 skinPosition = mul(input.Position, bonesMatWorldArray[BlendIndicesArray[0]]) * input.BlendWeights.x;;
	skinPosition += mul(input.Position, bonesMatWorldArray[BlendIndicesArray[1]]) * input.BlendWeights.y;
	skinPosition += mul(input.Position, bonesMatWorldArray[BlendIndicesArray[2]]) * input.BlendWeights.z;
	skinPosition += mul(input.Position, bonesMatWorldArray[BlendIndicesArray[3]]) * input.BlendWeights.w;
	
	//Skinning de normal
	float3 skinNormal = mul(input.Normal, (float3x3)bonesMatWorldArray[BlendIndicesArray[0]]) * input.BlendWeights.x;
	skinNormal += mul(input.Normal, (float3x3)bonesMatWorldArray[BlendIndicesArray[1]]) * input.BlendWeights.y;
	skinNormal += mul(input.Normal, (float3x3)bonesMatWorldArray[BlendIndicesArray[2]]) * input.BlendWeights.z;
	skinNormal += mul(input.Normal, (float3x3)bonesMatWorldArray[BlendIndicesArray[3]]) * input.BlendWeights.w; 
	output.WorldNormal = normalize(skinNormal);
	
	//Skinning de tangent
	float3 skinTangent = mul(input.Tangent, (float3x3)bonesMatWorldArray[BlendIndicesArray[0]]) * input.BlendWeights.x;
	skinTangent += mul(input.Tangent, (float3x3)bonesMatWorldArray[BlendIndicesArray[1]]) * input.BlendWeights.y;
	skinTangent += mul(input.Tangent, (float3x3)bonesMatWorldArray[BlendIndicesArray[2]]) * input.BlendWeights.z;
	skinTangent += mul(input.Tangent, (float3x3)bonesMatWorldArray[BlendIndicesArray[3]]) * input.BlendWeights.w;
	output.WorldTangent = normalize(skinTangent);
	
	//Skinning de binormal
	float3 skinBinormal = mul(input.Binormal, (float3x3)bonesMatWorldArray[BlendIndicesArray[0]]) * input.BlendWeights.x;
	skinBinormal += mul(input.Binormal, (float3x3)bonesMatWorldArray[BlendIndicesArray[1]]) * input.BlendWeights.y;
	skinBinormal += mul(input.Binormal, (float3x3)bonesMatWorldArray[BlendIndicesArray[2]]) * input.BlendWeights.z;
	skinBinormal += mul(input.Binormal, (float3x3)bonesMatWorldArray[BlendIndicesArray[3]]) * input.BlendWeights.w;
	output.WorldBinormal = normalize(skinBinormal);
	
	
	//Proyectar posicion (teniendo en cuenta lo que se hizo por skinning)
	output.Position = mul(float4(skinPosition.xyz, 1.0), matWorldViewProj);

	//Enviar color directamente
	output.Color = input.Color;

	//Enviar Texcoord directamente
	output.Texcoord = input.Texcoord;
	  
	//Posicion pasada a World-Space
	output.WorldPosition = mul(float4(skinPosition.xyz, 1.0), matWorld).xyz;
	
	//Vector desde el vertice hacia la luz
	output.LightVec = lightPosition.xyz - output.WorldPosition;

	return output;
}


//Input del Pixel Shader
struct PS_DIFFUSE_MAP
{
	float4 Color : COLOR0; 
	float2 Texcoord : TEXCOORD0;
};

//Input del Pixel Shader
struct PS_INPUT_SIMPLE 
{
	float4 Color : COLOR0; 
	float2 Texcoord : TEXCOORD0;
	float3 WorldPosition : TEXCOORD4;
	float3 WorldNormal	: TEXCOORD1;
	float3 LightVec	: TEXCOORD5;
};	

//Pixel Shader
float4 ps_DiffuseMap(PS_DIFFUSE_MAP input) : COLOR0
{      
	//Modular color de la textura por color del mesh
	return tex2D(diffuseMap, input.Texcoord) * input.Color;
}

//Pixel Shader
float4 ps_simpleEnv(PS_INPUT_SIMPLE input) : COLOR0
{      
	//Normalizar vectores
	float3 Nn = normalize(input.WorldNormal);
	float3 Ln = normalize(input.LightVec);
    
	//Obtener texel de la textura
	float4 texelColor = tex2D(diffuseMap, input.Texcoord) * input.Color;
	
	//Obtener texel de CubeMap
	float3 Vn = normalize(eyePosition.xyz - input.WorldPosition);
	float3 R = reflect(Vn, Nn);
    float3 reflectionColor = texCUBE(cubeMap, R).rgb;
	
	//Calcular intensidad de luz, con atenuacion por distancia
	//float distAtten = length(lightPosition.xyz - input.WorldPosition) * lightAttenuation;
	//float intensity = lightIntensity / distAtten;
	float intensity = lightIntensity;

	//Ambient
	float3 ambientLight = intensity * lightColor * materialAmbientColor;
	
	//Diffuse (N dot L, usando normal de NormalMap)
	float3 n_dot_l = dot(Nn, Ln);
	float3 diffuseLight = intensity * lightColor * materialDiffuseColor.rgb * max(0.0, n_dot_l);
	
	//Specular (como vector de R se usa el HalfAngleVec)
	float3 HalfAngleVec = normalize(Vn + Ln);
	float3 n_dot_h = dot(Nn, HalfAngleVec);
	float3 specularLight = n_dot_l <= 0.0
			? float3(0.0, 0.0, 0.0)
			: (
				intensity * lightColor * materialSpecularColor 
				* pow(max( 0.0, n_dot_h), materialSpecularExp)
			);
	
	//Color final: modular (Emissive + Ambient + Diffuse) por el color de la textura, y luego sumar Specular.
	//El color Alpha sale del diffuse material
	float4 finalColor = float4((materialEmissiveColor + ambientLight + diffuseLight) * texelColor + specularLight + (texelColor * reflectionColor * reflection), materialDiffuseColor.a);
	
	
	//float4 finalColor = float4(reflectionColor, materialDiffuseColor.a);
	
	return finalColor;
}

/*
* Technique DIFFUSE_MAP
*/
technique DIFFUSE_MAP
{
   pass Pass_0
   {
	  VertexShader = compile vs_2_0 vs_DiffuseMap();
	  PixelShader = compile ps_2_0 ps_simpleEnv();
   }
}


