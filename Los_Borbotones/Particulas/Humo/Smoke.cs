using System;
using System.Collections.Generic;
using System.Text;
using TgcViewer.Example;
using TgcViewer;
using Microsoft.DirectX.Direct3D;
using System.Drawing;
using Microsoft.DirectX;
using TgcViewer.Utils.Modifiers;
using TgcViewer.Utils.Interpolation;
using TgcViewer.Utils.TgcGeometry;
using System.IO;

namespace AlumnoEjemplos.Los_Borbotones
{
    public class Smoke:TgcExample
    {
        int width, height;
        SmokeEmitter humo;
        Random r = new Random();

        public override string getCategory()
        {
            return "Los Salieri de Mandelbrot";
        }

        public override string getName()
        {
            return "Humo";
        }

        public override string getDescription()
        {
            return "Ejemplo de Humo";
        }

        public override void init()
        {
            Device d3dDevice = GuiController.Instance.D3dDevice;

            SystemState_Particulas.Instance.SetState_Zero();

            //Relative Positioning
            width = GuiController.Instance.Panel3d.Width;
            height = GuiController.Instance.Panel3d.Height;

            //Variables del humo
            int cantidad = 100;//Parametrizable
            Vector3 origen = new Vector3(width / 2, height / 2, 0);//Parametrizable
            float speed = 0.03f;//Parametrizable
            float divergence = 0.005f;//Parametrizable
            Vector3 velocidad = new Vector3(divergence, speed, divergence);
            Vector3 aceleracion = new Vector3(0.00f, 0f, 0.00f);
            float min = 0.1f, max = 20f, tiempoVida_Particula = 10f;//Parametrizables
            int alpha = 255;
            float spawn = 0.03f;
            float sizeSpeed = 0.14f;
            float updateTime = 0.03f;

            //Inicializo el emisor de humo
            humo = new SmokeEmitter(cantidad, origen, velocidad, aceleracion, min, max, tiempoVida_Particula, Color.DarkGray, alpha, spawn,updateTime,sizeSpeed);
            
            //INIT
            humo.Init();
            humo.AgregarModifiers(width, height);

            //Camara
            GuiController.Instance.RotCamera.Enable = true;
            //GuiController.Instance.RotCamera.CameraDistance = 10;
            GuiController.Instance.BackgroundColor = Color.Black;
            GuiController.Instance.RotCamera.CameraCenter = new Vector3(width / 2, height / 2, 0);
        }

        public override void render(float elapsedTime)
        {
            Device d3dDevice = GuiController.Instance.D3dDevice;

            //Sete el estado del Render para la visualización de point sprites
            SystemState_Particulas.Instance.SetRenderState();

            //Actualizo los modifiers
            humo.ActualizarModifiers();

            //Dibujo el emitter
            humo.Render(elapsedTime);

            //Vuelvo los valores por defecto
            SystemState_Particulas.Instance.SetRenderState_Zero();
        }

        public override void close()
        {
            humo.Dispose();
        }
    }
}
