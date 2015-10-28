﻿using Microsoft.DirectX;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using TgcViewer;
using TgcViewer.Utils._2D;
using TgcViewer.Utils.TgcGeometry;
using TgcViewer.Utils.TgcSceneLoader;
using TgcViewer.Utils.Terrain;
using TgcViewer.Utils.Sound;
using TgcViewer.Utils.Fog;
using Microsoft.DirectX.Direct3D;
using System.Collections;
using System.Drawing.Imaging;
using System.Windows.Forms;
using TgcViewer.Utils.TgcSkeletalAnimation;
using TgcViewer.Utils.Shaders;
using TgcViewer.Utils;
using AlumnoEjemplos.Los_Borbotones;

namespace AlumnoEjemplo.Los_Borbotones
{
    public class PostProcessManager
    {
        #region Singleton
        private static volatile PostProcessManager instance;
        private static object syncRoot = new Object();

        /// <summary>
        /// Permite acceder a una instancia de la clase PostProcessManager desde cualquier parte del codigo.
        /// </summary>
        public static PostProcessManager Instance
        {
            get
            {
                lock (syncRoot)
                {
                    if (instance == null)
                        instance = new PostProcessManager();
                }
                return instance;
            }
        }
        #endregion

        //PostProcess Manager Instance Variables
        public string MediaDir;
        public string ShaderDir;
        public Effect theShader;
        string renderFlux;
        Device d3dDevice;

        //Render Targets
        public Surface depthStencil;
        public Texture firstRenderTarget;

        //
        public VertexBuffer quadVertexBuffer;

        internal void Init()
        {
            GuiController.Instance.CustomRenderEnabled = true;
            renderFlux = (string)GuiController.Instance.Modifiers.getValue("RenderFlux"); //1.21 GigaWatts

            d3dDevice = GuiController.Instance.D3dDevice;
            MediaDir = GuiController.Instance.AlumnoEjemplosMediaDir + "Los_Borbotones\\";
            ShaderDir = GuiController.Instance.AlumnoEjemplosMediaDir + "Los_Borbotones\\Shaders\\";

            //Cargar Shader personalizado
            string compilationErrors;
            theShader = Effect.FromFile(d3dDevice,
                GuiController.Instance.AlumnoEjemplosMediaDir + "Los_Borbotones\\Shaders/postProcess.fx",
                null, null, ShaderFlags.PreferFlowControl, null, out compilationErrors);
            if (theShader == null)
            {
                throw new Exception("Error al cargar shader. Errores: " + compilationErrors);
            }
            //Configurar Technique dentro del shader
            theShader.Technique = (string)GuiController.Instance.Modifiers.getValue("PostProcessTechnique");

            depthStencil = d3dDevice.CreateDepthStencilSurface(d3dDevice.PresentationParameters.BackBufferWidth,
                                                                         d3dDevice.PresentationParameters.BackBufferHeight,
                                                                         DepthFormat.D24S8,
                                                                         MultiSampleType.None,
                                                                         0,
                                                                         true);

            // Inicializar el/los render target

            firstRenderTarget = new Texture(d3dDevice, d3dDevice.PresentationParameters.BackBufferWidth
                    , d3dDevice.PresentationParameters.BackBufferHeight, 1, Usage.RenderTarget,
                        Format.X8R8G8B8, Pool.Default);

            // inicializar valores en el Shader
            theShader.SetValue("g_RenderTarget", firstRenderTarget);

            //       Resolucion de pantalla
            theShader.SetValue("screen_dx", d3dDevice.PresentationParameters.BackBufferWidth);
            theShader.SetValue("screen_dy", d3dDevice.PresentationParameters.BackBufferHeight);
            float desf_x = (float)(1 / (d3dDevice.PresentationParameters.BackBufferWidth * 0.5));
            float desf_y = (float)(1 / (d3dDevice.PresentationParameters.BackBufferHeight * 0.5));

            //Crea los 2 Triangulos
            CustomVertex.PositionTextured[] Vertices = new CustomVertex.PositionTextured[]
            {
                new CustomVertex.PositionTextured( -1-desf_x, 1+desf_y, 1, 0,0),
                new CustomVertex.PositionTextured(1,  1+desf_y, 1, 1,0),
                new CustomVertex.PositionTextured(-1-desf_x, -1, 1, 0,1),
                new CustomVertex.PositionTextured(1,-1, 1, 1,1)
            };
            //vertex buffer de los triangulos
            quadVertexBuffer = new VertexBuffer(typeof(CustomVertex.PositionTextured),
                    4, d3dDevice, Usage.Dynamic | Usage.WriteOnly,
                        CustomVertex.PositionTextured.Format, Pool.Default);
            quadVertexBuffer.SetData(Vertices, 0, LockFlags.None);

        }

        internal void Update(float elapsedTime)
        {
            theShader.Technique = (string)GuiController.Instance.Modifiers.getValue("PostProcessTechnique");
            renderFlux = (string)GuiController.Instance.Modifiers.getValue("RenderFlux");
            GameManager.Instance.Update(elapsedTime);
        }

        internal void Render(float elapsedTime)
        {
            //1 -- Cambiar Render Target:
            //guardo el Render target anterior y seteo la textura como render target
            Surface OldRenderTarget = d3dDevice.GetRenderTarget(0);
            Surface pSurf = firstRenderTarget.GetSurfaceLevel(0);

            d3dDevice.SetRenderTarget(0, pSurf);

            //hago lo mismo con el depthbuffer, necesito el que no tiene multisampling
             Surface OldDepthStencil = d3dDevice.DepthStencilSurface;
            //Probar de comentar esta linea, para ver como se produce el fallo en el ztest
             //por no soportar usualmente el multisampling en el render to texture.

            d3dDevice.DepthStencilSurface = depthStencil;

            d3dDevice.Clear(ClearFlags.Target | ClearFlags.ZBuffer, Color.Black, 1.0f, 0);

            //2 -- Renderizar Normal
           
            GameManager.Instance.RenderAll(elapsedTime);
            
            pSurf.Dispose();

            //3 -- Renderizar X (Normales, Iluminacion, GlowMap, Etc.)


            //4 -- Restuaro el render target y el stencil
            d3dDevice.SetRenderTarget(0, OldRenderTarget);
            d3dDevice.DepthStencilSurface = OldDepthStencil;
            
            
            //5 -- Renderizar Quad

            d3dDevice.BeginScene();
                        
            d3dDevice.VertexFormat = CustomVertex.PositionTextured.Format;
            d3dDevice.SetStreamSource(0, quadVertexBuffer, 0);
            theShader.SetValue("g_RenderTarget", firstRenderTarget);

            d3dDevice.Clear(ClearFlags.Target | ClearFlags.ZBuffer, Color.Red, 1.0f, 0);

            theShader.Begin(FX.None);
            theShader.BeginPass(0);

            d3dDevice.DrawPrimitives(PrimitiveType.TriangleStrip, 0, 2); //Renderiza 2 triangulos empezando del vertice 0 que pase en el stream

            theShader.EndPass();
            theShader.End();

            GuiController.Instance.Text3d.drawText("FPS: " + HighResolutionTimer.Instance.FramesPerSecond, 0, 0, Color.Yellow);
            d3dDevice.EndScene();
          
        }

        internal void close()
        {
            firstRenderTarget.Dispose();
            quadVertexBuffer.Dispose();
        }

    }
}
