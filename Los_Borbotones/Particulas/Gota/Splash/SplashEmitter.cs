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
    public class SplashEmitter : Emitter
    {
        public SplashEmitter(int particlesAmount, Vector3 origin, Vector3 speed, Vector3 aceleracion, float min, float positionLife, Color initColor, int initAlpha, float time_createParticle, float updateTime)
            : base(particlesAmount, origin, speed, aceleracion, min, min, positionLife, initColor, initAlpha, time_createParticle, updateTime)
        { }

        public override void InicializarParticulas()
        {
            SplashPart p;
            int i;
            Vector3 dire;
            int tex;
            for (i = 0; i < (this.ParticlesAmount); i++)
            {
                dire = this.RandVec3();
                tex = r.Next(0, this.Textures.Count);
                p = new SplashPart(this.InitCoords, this.Speed, this.Acceleration, dire, this.InitialColor, this.InitialAlpha, this.PointSizeMin, this.TimeToLive_Particle, this.Textures[tex]);
                this.Particles.Add(p);
                p.Index_ParticleVertex = i;
                this.ParticlesVertex.Add(p.CV_PositionColored);
            }
        }

        public override void AgregarParticulas(int cantidad)
        {
            SplashPart p;
            int i;
            Vector3 dire;
            int tex;
            for (i = this.ParticlesAmount; i < (this.ParticlesAmount + cantidad); i++)
            {
                dire = this.RandVec3();
                tex = r.Next(0, this.Textures.Count);
                p = new SplashPart(this.InitCoords, this.Speed, this.Acceleration, dire, this.InitialColor, this.InitialAlpha, this.PointSizeMin, this.TimeToLive_Particle, this.Textures[tex]);
                this.Particles.Add(p);
                p.Index_ParticleVertex = i;
                this.ParticlesVertex.Add(p.CV_PositionColored);
            }
            this.ParticlesAmount += cantidad;
        }

        public override void InicializarTexturas()
        {
            Device d3dDevice = GuiController.Instance.D3dDevice;

            string path0 = GuiController.Instance.AlumnoEjemplosMediaDir + "Los_Borbotones\\Particulas\\blood.JPG";
            this.Textures.Add(TextureLoader.FromFile(d3dDevice, path0));
        }

        public override void AgregarModifiers(float width, float height)
        {
            //Crear un modifier para un valor FLOAT
            GuiController.Instance.Modifiers.addFloat("Velocidad Splash", 0, 1f, this.Speed.Y);

            //Crear un modifier para un valor FLOAT
            GuiController.Instance.Modifiers.addFloat("Divergencia Splash", 0f, 1f, (this.Speed.X + this.Speed.Z) / 2f);
        }

        public override void Update(float elapsedTime)
        {
            Device d3dDevice = GuiController.Instance.D3dDevice;
           
            this.Spawn += elapsedTime;

            foreach (SplashPart p in this.Particles)
            {
                if (p.Active)
                {
                    p.LifeTime += elapsedTime;
                    p.Spawn += elapsedTime;
                    //Render de la particula si no llego a la altura máxima
                    if (p.CurrentPos.Y >= this.InitCoords.Y)
                    {
                        p.Render(this);
                    }
                    else
                    {
                        p.Matar(this);
                    }
                    p.CV_PositionColored = new CustomVertex.PositionColored(p.CurrentPos, Color.FromArgb(p.Alpha, p.Colour).ToArgb());
                    this.ParticlesVertex[p.Index_ParticleVertex] = p.CV_PositionColored;
                }
            }

            this.VertexBuffer_Emitter.SetData(this.ParticlesVertex.ToArray(), 0, LockFlags.Discard);
        }

        public override void ActualizarModifiers()
        {
            float speed = (float)GuiController.Instance.Modifiers["Velocidad Splash"];
            if (this.Speed.Y != speed)
            {
                this.Speed = new Vector3(this.Speed.X, speed, this.Speed.Z);
            }

            float divergencia = (float)GuiController.Instance.Modifiers["Divergencia Splash"];
            if (this.Speed.X != divergencia)
            {
                this.Speed = new Vector3(divergencia, this.Speed.Y, divergencia);
            }
        }
    }
}
