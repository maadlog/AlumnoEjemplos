using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.DirectX.Direct3D;
using TgcViewer;
using System.Drawing;

namespace AlumnoEjemplos.Los_Borbotones
{
    public sealed class SystemState_Particulas
    {
        static SystemState_Particulas _instance = null;
        Blend blend;

        public static SystemState_Particulas Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new SystemState_Particulas();
                }
            return _instance;
            }
        }
        
        public SystemState_Particulas() { }

        public void SetState_Zero()
        {
            Device d3dDevice = GuiController.Instance.D3dDevice;

            //Seteo valores iniciales del renderState
            d3dDevice.SetTexture(0, null);
            d3dDevice.SamplerState[0].MinFilter = TextureFilter.Linear;
            d3dDevice.SamplerState[0].MagFilter = TextureFilter.Linear;
            d3dDevice.RenderState.Lighting = false;
        }

        public void SetRenderState()
        {
            Device d3dDevice = GuiController.Instance.D3dDevice;

            //Customizo el renderState
            d3dDevice.RenderState.ZBufferWriteEnable = false;
            //AlphaBlend
            d3dDevice.RenderState.AlphaBlendEnable = true;
            d3dDevice.RenderState.SourceBlend = Blend.SourceAlpha;
            blend = d3dDevice.RenderState.DestinationBlend;
            d3dDevice.RenderState.DestinationBlend = Blend.One;
            d3dDevice.SetTextureStageState(0, TextureStageStates.AlphaOperation, true);
            d3dDevice.RenderState.BlendFactor = Color.Black;

            //Seteo el formato a mostar en pantalla
            d3dDevice.VertexFormat = CustomVertex.PositionColored.Format;

            //Atributos del pointSprite
            d3dDevice.RenderState.PointSpriteEnable = true;
            d3dDevice.RenderState.PointScaleEnable = true;
            d3dDevice.RenderState.PointSizeMin = 0f;
            d3dDevice.RenderState.PointScaleA = 0f;
            d3dDevice.RenderState.PointScaleB = 0f;
            d3dDevice.RenderState.PointScaleC = 10f;
            d3dDevice.RenderState.PointSize = 100f;
        }

        public void SetRenderState_Zero()
        {
            Device d3dDevice = GuiController.Instance.D3dDevice;

            //Vuelvo los valores por defecto
            d3dDevice.RenderState.ZBufferWriteEnable = true;
            d3dDevice.RenderState.AlphaBlendEnable = false;
            d3dDevice.RenderState.DestinationBlend = blend;
            d3dDevice.SetTexture(0, null);
            d3dDevice.RenderState.PointSpriteEnable = false;
            d3dDevice.RenderState.PointScaleEnable = false;
        }
    }
}
