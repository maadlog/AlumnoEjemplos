// ---------------------------------------------------------
// Shader de Vision Nocturna 
// ---------------------------------------------------------

/**************************************************************************************/
/* Variables comunes */
/**************************************************************************************/

//Matrices de transformacion
float4x4 matWorld; //Matriz de transformacion World
float4x4 matWorldView; //Matriz World * View
float4x4 matWorldViewProj; //Matriz World * View * Projection
float4x4 matInverseTransposeWorld; //Matriz Transpose(Invert(World))


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

texture g_RenderTarget;
sampler2D RenderTarget = sampler_state
{
	Texture = (g_RenderTarget);
	ADDRESSU = CLAMP;
	ADDRESSV = CLAMP;
	MINFILTER = LINEAR;
	MAGFILTER = LINEAR;
	MIPFILTER = LINEAR;
};


texture g_GlowMap;
sampler GlowMap = sampler_state
{
    Texture = (g_GlowMap);
	ADDRESSU = CLAMP;
	ADDRESSV = CLAMP;
	MINFILTER = LINEAR;
	MAGFILTER = LINEAR;
	MIPFILTER = LINEAR;
};

float screen_dx;
float screen_dy; 

static const int kernel_r = 6;
static const int kernel_size = 13;
static const float Kernel[kernel_size] = 
{
    0.002216,    0.008764,   	0.026995,   	0.064759,
    0.120985,    0.176033,		0.199471,    	0.176033,
	0.120985,
    0.064759,    0.026995,    	0.008764,    	0.002216,
};


/**************************************************************************************/
/* RenderScene */
/**************************************************************************************/

//Input del Vertex Shader
struct VS_INPUT 
{
   float4 Position : POSITION0;
   float4 Color : COLOR0;
   float2 Texcoord : TEXCOORD0;
};

//Output del Vertex Shader
struct VS_OUTPUT 
{
   float4 Position :        POSITION0;
   float2 Texcoord :        TEXCOORD0;
   float4 Color :			COLOR0;
};



//Vertex Shader
VS_OUTPUT vs_main( VS_INPUT Input )
{
   VS_OUTPUT Output;
   Output.Position = Input.Position;
   Output.Texcoord = Input.Texcoord;
   Output.Color = Input.Color;
   return( Output );
}

void vs_copyFromTex( float4 vPos : POSITION, float2 vTex : TEXCOORD0,out float4 oPos : POSITION,out float2 oScreenPos: TEXCOORD0)
{
    oPos = vPos;
	oScreenPos = vTex;
	oPos.w = 1;
}


//Pixel Shader
float4 ps_main( float2 Texcoord: TEXCOORD0, float4 Color:COLOR0) : COLOR0
{      
	return tex2D( RenderTarget, Texcoord );
}

float4 ps_dull( float2 Texcoord: TEXCOORD0, float4 Color:COLOR0) : COLOR0
{      
	float4 fvBaseColor = tex2D( diffuseMap, Texcoord );
	return float4(0,0,0,fvBaseColor.a);
}

void ps_gaussBlur_H(float2 screen_pos  : TEXCOORD0,out float4 Color : COLOR)
{ 
    Color = 0;
	for(int i=0;i<kernel_size;++i)
		Color += tex2D(RenderTarget, screen_pos+float2((float)(i-kernel_r)/screen_dx,0)) * Kernel[i];
	Color.a = 1;
}

void ps_gaussBlur_V(float2 screen_pos  : TEXCOORD0,out float4 Color : COLOR)
{ 
    Color = 0;
	for(int i=0;i<kernel_size;++i)
		Color += tex2D(RenderTarget, screen_pos+float2(0,(float)(i-kernel_r)/screen_dy)) * Kernel[i];
	Color.a = 1;

}

float4 ps_downFilter4( in float2 Tex : TEXCOORD0 ) : COLOR0
{
    float4 Color = 0;
    for (int i = 0; i < 4; i++)
    for (int j = 0; j < 4; j++)
		Color += tex2D(RenderTarget, Tex+float2((float)i/screen_dx,(float)j/screen_dy));

	return Color / 16;
}

float4 ps_nightVision( in float2 Tex : TEXCOORD0 , in float2 vpos : VPOS) : COLOR0
{
	float4 ColorBase = tex2D(RenderTarget, Tex);
	float4 ColorBrillante = tex2D(GlowMap, Tex);
	
	 // Y = 0.2126 R + 0.7152 G + 0.0722 B
	float Yb = 0.2126*ColorBase.r + 0.7152*ColorBase.g + 0.0722*ColorBase.b;
	float Yk = 0.2126*ColorBrillante.r + 0.7152*ColorBrillante.g + 0.0722*ColorBrillante.b;
	if( round(vpos.y/2)%2 == 0)
	{
		Yb *= 0.85;
		Yk *= 0.85;
	}

	return float4(Yk*0.50,Yb*0.6+Yk*4,Yk*0.50,1);
}


// ------------------------------------------------------------------
technique Propagation
{
   pass Pass_0
   {
	  VertexShader = compile vs_3_0 vs_main();
	  PixelShader = compile ps_3_0 ps_main();
   }

}

technique DullObjects
{
   pass Pass_0
   {
	  VertexShader = compile vs_3_0 vs_main();
	  PixelShader = compile ps_3_0 ps_dull();
   }
}

technique SeparableGaussianBlur
{
   pass Pass_0
   {
	  VertexShader = compile vs_3_0 vs_copyFromTex();
	  PixelShader = compile ps_3_0 ps_gaussBlur_H();
   }
   pass Pass_1
   {
	  VertexShader = compile vs_3_0 vs_copyFromTex();
	  PixelShader = compile ps_3_0 ps_gaussBlur_V();
   }

}

technique DownFilter4
{
   pass Pass_0
   {
	  VertexShader = compile vs_3_0 vs_copyFromTex();
	  PixelShader = compile ps_3_0 ps_downFilter4();
   }

}

technique NightVision
{
   pass Pass_0
   {
	  VertexShader = compile vs_3_0 vs_copyFromTex();
	  PixelShader = compile ps_3_0 ps_nightVision();
   }

}