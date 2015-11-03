// ---------------------------------------------------------
// Shaders para la HUD 
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

float screen_dx;
float screen_dy; 
float time;
float health;


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

VS_OUTPUT vs_main_weapon( VS_INPUT Input )
{
   VS_OUTPUT Output;
   Output.Position = Input.Position;
   Output.Texcoord = Input.Texcoord;
   Output.Color = Input.Color;
   return( Output );
}

VS_OUTPUT vs_sec_weapon( VS_INPUT Input )
{
   VS_OUTPUT Output;
   Output.Position = Input.Position;
   Output.Texcoord = Input.Texcoord;
   Output.Color = Input.Color;
   return( Output );
}

//Pixel Shader
float4 ps_main( float2 Texcoord: TEXCOORD0, float4 Color:COLOR0) : COLOR0
{      
	return tex2D( diffuseMap, Texcoord );
}

float4 ps_health( float2 Texcoord: TEXCOORD0, float4 Color:COLOR0) : COLOR0
{      
	return tex2D( diffuseMap, Texcoord );
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

technique MainWeapon
{
	pass Pass_0
	{
		VertexShader = compile vs_3_0 vs_main_weapon();
		PixelShader = compile ps_3_0 ps_main();
	}
}

technique SecWeapon
{
	pass Pass_0
	{
		VertexShader = compile vs_3_0 vs_sec_weapon();
		PixelShader = compile ps_3_0 ps_main();
	}
}

technique Health
{
	pass Pass_0
	{
		VertexShader = compile vs_3_0 vs_main();
		PixelShader = compile ps_3_0 ps_health();
	}
}