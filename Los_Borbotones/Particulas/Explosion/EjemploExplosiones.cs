using System;
using System.Collections.Generic;
using System.Text;
using TgcViewer.Example;
using TgcViewer;
using Microsoft.DirectX.Direct3D;
using System.Drawing;
using Microsoft.DirectX;
using TgcViewer.Utils.Modifiers;
using TgcViewer.Utils.TgcSceneLoader;
using TgcViewer.Utils.Interpolation;
using TgcViewer.Utils.TgcGeometry;
using TgcViewer.Utils._2D;
using System.IO;


namespace AlumnoEjemplos.Los_Borbotones
{
    public class EjemploExplosiones : TgcExample
    {
        int width, height;
        ExplosionEmitter explosion;
        Random r = new Random();
        float time = 0f;

        public override string getCategory()
        {
            return "Los Salieri de Mandelbrot";
        }

        public override string getName()
        {
            return "Explosion";
        }

        public override string getDescription()
        {
            return "Ejemplo de Explosiones";
        }

        public override void init()
        {
            Device d3dDevice = GuiController.Instance.D3dDevice;

            SystemState_Particulas.Instance.SetState_Zero();

            //Relative Positioning
            width = GuiController.Instance.Panel3d.Width;
            height = GuiController.Instance.Panel3d.Height;
            
            //Variables
            int cantExplosion = 10;
            float particleTime = 20f, sizeMax = 30f, speed = 0.05f, sizeSpeed = 1.5f;
            float updateTime = 0.01f;
            
            explosion = new ExplosionEmitter(cantExplosion, new Vector3(width / 2, height / 2, 0), new Vector3(speed, speed, speed), new Vector3(0.00f, 0.00f, 0.00f), 0f, sizeMax, particleTime, Color.White, 150, 0f, updateTime,r.Next(0, 1000), sizeSpeed,5);

            explosion.Init();
            explosion.AgregarModifiers(width, height);
 
            //Camara
            GuiController.Instance.RotCamera.Enable = true;
            GuiController.Instance.RotCamera.CameraDistance = 20;
            GuiController.Instance.BackgroundColor = Color.Black;
            GuiController.Instance.RotCamera.CameraCenter = new Vector3(width / 2, height / 2, 0);
        }

        public override void render(float elapsedTime)
        {
            SystemState_Particulas.Instance.SetRenderState();

            time += elapsedTime;

            explosion.ActualizarModifiers();

            //Cada 5 segundos emite la explosion
            if (!explosion.Active() && time > 5f)
            {
                explosion.RevivirParticulas();
                time = 0f;
            }

            explosion.Render(elapsedTime);

            //Vuelvo los valores por defecto
            SystemState_Particulas.Instance.SetRenderState_Zero();
        }

        public override void close()
        {
            explosion.Dispose();
            //flash.Dispose();
        }
    }
}
