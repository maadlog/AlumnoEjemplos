using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TgcViewer.Example;
using TgcViewer;
using Microsoft.DirectX.Direct3D;
using System.Drawing;
using Microsoft.DirectX;

namespace AlumnoEjemplos.Los_Borbotones
{
    public class GotaEmitter : Emitter
    {
        static private float MIN_SIZE = 100f;

        //Cantidad de particulas para la salpicadura
        static private int SPLASH_CANTIDAD = 30;

        //Velocidad para agrandar de tamaño
        public float SizeSpeed { get; set; }

        //Emisor de la salpicadura
        public SplashEmitter Splash { get; set; }

        //Tamaño de la salpicadura
        public float SplashSize { get; set; }

        //Velocidad de la salpicadura
        public float SplashSpeed { get; set; }
    
        public GotaEmitter(int particlesAmount, Vector3 origin, Vector3 speed, Vector3 aceleracion, float max, float positionLife, Color initColor, int initAlpha, float time_createParticle, float updateTime, float sizeSpeed, float splashSize, float splashSpeed)
            : base(particlesAmount, origin, speed, aceleracion, MIN_SIZE, max, positionLife, initColor, initAlpha, time_createParticle,updateTime) 
        {
            this.SizeSpeed = sizeSpeed;
            this.SplashSize = splashSize;
            this.SplashSpeed = splashSpeed;
        }

        public override void Init()
        {
            base.Init();
            this.InicializarSplash();
        }

        public override void InicializarParticulas()
        {
            GotaPart p;
            int i;
            Vector3 dire = this.Speed;
            //this.Speed = new Vector3(5,0,5);
            float size = this.PointSizeMin;
            int tex;

            for (i = 0; i < (this.ParticlesAmount); i++)
            {
                tex = r.Next(0, this.Textures.Count);
                p = new GotaPart(this.InitCoords, new Vector3(5,0,5), this.Acceleration, this.Speed, this.InitialColor, this.InitialAlpha, size, this.TimeToLive_Particle, this.Textures[tex]);
                this.Particles.Add(p);
                p.Index_ParticleVertex = i;
                this.ParticlesVertex.Add(p.CV_PositionColored);
            }
        }

        public override void InicializarTexturas()
        {
            Device d3dDevice = GuiController.Instance.D3dDevice;

            string path0 = GuiController.Instance.AlumnoEjemplosMediaDir + "Los_Borbotones\\Particulas\\blood.JPG";
            this.Textures.Add(TextureLoader.FromFile(d3dDevice, path0));
        }

        //Se inicializa la salpicadura que va a ser la gota al llegar a la altura máxima
        public void InicializarSplash()
        {
            //El origen del splash es a la altura donde termina la gota
            Vector3 pos = new Vector3(this.InitCoords.X + 5 * Speed.X * 0.50f * TimeToLive_Particle, this.InitCoords.Y - TimeToLive_Particle, this.InitCoords.Z + 5 * Speed.Z * 0.50f * TimeToLive_Particle);
            Vector3 speed = new Vector3(this.SplashSpeed, this.SplashSpeed, this.SplashSpeed);
            this.Splash = new SplashEmitter(SPLASH_CANTIDAD, pos, speed, new Vector3(0, -9.8f, 0), this.SplashSize, this.SplashSize, this.InitialColor, 255, 0f, this.UpdateTime);
            this.Splash.Init();
        }

        public override void AgregarModifiers(float width, float height)
        {
            //Crear un modifier para un valor FLOAT
            GuiController.Instance.Modifiers.addInt("Cantidad", 0, 5, this.ParticlesAmount);

            GuiController.Instance.Modifiers.addFloat("Tamaño MIN", 0f, 10f, this.PointSizeMin);

            GuiController.Instance.Modifiers.addFloat("Tamaño MAX", 0.1f, 10f, this.PointSizeMax);

            //Crear un modifier para modificar un vértice
            GuiController.Instance.Modifiers.addVertex3f("Posicion", new Vector3(0, 0, 0), new Vector3(width, height, 10), this.InitCoords);

            //Crear un modifier para un valor FLOAT
            GuiController.Instance.Modifiers.addFloat("Duracion altura", 0.1f, height/2, this.TimeToLive_Particle);
        }

        public override void AgregarParticulas(int cantidad)
        {
            GotaPart p;
            int i;
            Vector3 dire = new Vector3(0, 1, 0);
            int tex;
            for (i = this.ParticlesAmount; i < (this.ParticlesAmount + cantidad); i++)
            {
                tex = r.Next(0, this.Textures.Count);
                p = new GotaPart(this.InitCoords, this.Speed, this.Acceleration, dire, this.InitialColor, this.InitialAlpha, this.PointSizeMin, this.TimeToLive_Particle, this.Textures[tex]);
                this.Particles.Add(p);
                p.Index_ParticleVertex = i;
                this.ParticlesVertex.Add(p.CV_PositionColored);
            }
            this.ParticlesAmount += cantidad;
        }

        public override void Update(float elapsedTime)
        {
            Device d3dDevice = GuiController.Instance.D3dDevice;
            this.Spawn += elapsedTime;

            if (this.Spawn > this.TimeToCreateParticle)
            {
                this.RevivirParticula();
                this.Spawn = 0f;
                this.TimeToCreateParticle = 1000;
            }
            
            foreach (GotaPart p in this.Particles)
            {
                if (p.Active)
                {
                    p.LifeTime += elapsedTime;
                    p.Spawn += elapsedTime;
                    if (p.CurrentPos.Y > p.TimeToLive)
                    {
                        p.Render(this);
                    }
                    else
                    {
                        p.Matar(this);
                        //Si muere ya se hace el render del splash
                        this.Splash.RevivirParticulas();
                    }
                    p.CV_PositionColored = new CustomVertex.PositionColored(p.CurrentPos, Color.FromArgb(p.Alpha, p.Colour).ToArgb());
                    this.ParticlesVertex[p.Index_ParticleVertex] = p.CV_PositionColored;
                }
            }

            this.VertexBuffer_Emitter.SetData(this.ParticlesVertex.ToArray(), 0, LockFlags.Discard);
        }

        public override void Render(float elapsedTime)
        {
            base.Render(elapsedTime);
            if (this.Splash.Active())
            {
                this.Splash.Render(elapsedTime);
            }
        }

        public override void ActualizarModifiers()
        {
            Vector3 pos = (Vector3)GuiController.Instance.Modifiers["Posicion"];
            if (this.InitCoords != pos)
            {
                this.InitCoords = pos;
            }
            Vector3 pos_splash = new Vector3(pos.X, pos.Y - this.TimeToLive_Particle, pos.Z);
            if (this.Splash.InitCoords != pos_splash)
            {
                this.Splash.InitCoords = pos_splash;
            }

            int cantidad = (int)GuiController.Instance.Modifiers["Cantidad"];
            if (this.ParticlesAmount != cantidad)
            {
                this.Upd_ParticlesAmount(cantidad - this.ParticlesAmount);
            }

            float sizeMin = (float)GuiController.Instance.Modifiers["Tamaño MIN"];
            if (this.PointSizeMin != sizeMin)
            {
                this.PointSizeMin = sizeMin;
            }

            float sizeMax = (float)GuiController.Instance.Modifiers["Tamaño MAX"];
            if (this.PointSizeMax != sizeMax)
            {
                this.PointSizeMax = sizeMax;
            }

            float altura = (float)GuiController.Instance.Modifiers["Duracion altura"];
            if (this.TimeToLive_Particle != altura)
            {
                this.TimeToLive_Particle = altura;
            }

            //Se actualizan los modifiers del splash
            this.Splash.ActualizarModifiers();
        }
    }
}
