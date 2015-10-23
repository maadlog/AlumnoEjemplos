using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TgcViewer.Example;
using TgcViewer;
using Microsoft.DirectX.Direct3D;
using System.Drawing;
using Microsoft.DirectX;
using TgcViewer.Utils.Modifiers;

namespace AlumnoEjemplos.Los_Borbotones
{
    public class Gota : TgcExample
    {
        int width, height;
        GotaEmitter gotas;
        Random r = new Random();

        public override string getCategory()
        {
            return "Los Salieri de Mandelbrot";
        }

        public override string getName()
        {
            return "Gota de Agua";
        }

        public override string getDescription()
        {
            return "Ejemplo de gotas de agua";
        }

        public override void init()
        {
            SystemState_Particulas.Instance.SetState_Zero();

            //Relative Positioning
            width = GuiController.Instance.Panel3d.Width;
            height = GuiController.Instance.Panel3d.Height;

            //Variables del emitter
            int cantidad = 1;//Parametrizable
            Vector3 origen = new Vector3(width / 2, height / 2, 0);//Parametrizable
            float speed = 0f;//Parametrizable
            float divergence = 0f;//Parametrizable
            Vector3 velocidad = new Vector3(divergence,speed,divergence);
            Vector3 aceleracion = new Vector3(0, -9.8f, 0);
            float max=1f,aRRecorrer=4f;//Parametrizables
            int alpha = 255;
            float sizeSpeed = 2f;
            
            //Inicializo el emisor de burbujas
            gotas = new GotaEmitter(cantidad, origen, velocidad, aceleracion, max, aRRecorrer, Color.LightBlue, alpha, 0.5f, 0.01f, sizeSpeed,0.2f,0.5f);
            
            //INIT
            gotas.Init();
            gotas.AgregarModifiers(width,height);
            gotas.Splash.AgregarModifiers(width,height);

            //Camara
            GuiController.Instance.RotCamera.Enable = true;
            //GuiController.Instance.RotCamera.CameraDistance = 10;
            GuiController.Instance.BackgroundColor = Color.Black;
            GuiController.Instance.RotCamera.CameraCenter = new Vector3(width / 2, height / 2, 0);
        }

        public override void render(float elapsedTime)
        {
            //Seteo los valores del render state para los point sprites
            SystemState_Particulas.Instance.SetRenderState();
            
            //Actualizo los modifiers
            gotas.ActualizarModifiers();
            
            //Dibujo el emitter
            gotas.Render(elapsedTime);

            //Vuelvo los valores por defecto
            SystemState_Particulas.Instance.SetRenderState_Zero();
        }


        public override void close()
        {
            gotas.Dispose();
        }

    }
}
