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
    public class SmokeEmitter:Emitter
    {
        Random s = new Random();

        public float SizeSpeed { get; set; }

        public SmokeEmitter(int particlesAmount, Vector3 origin, Vector3 speed, Vector3 aceleracion, float min, float max, float particleTime, Color initColor, int initAlpha, float spawn,float updateTime,float sizeSpeed)
            : base(particlesAmount, origin, speed, aceleracion, min, max, particleTime, initColor, initAlpha, spawn, updateTime)
        {
            this.SizeSpeed = sizeSpeed;
        }

        public override void InicializarParticulas()
        {
            SmokePart p;
            int i;
            Vector3 dire;
            float size = this.PointSizeMin;
            int tex;

            for (i = 0; i < (this.ParticlesAmount); i++)
            {
                dire = this.RandVec3();
                tex = r.Next(0, this.Textures.Count);
                p = new SmokePart(this.InitCoords, this.Speed, this.Acceleration, dire, this.InitialColor, this.InitialAlpha, size, this.TimeToLive_Particle, this.Textures[tex],this.SizeSpeed);
                this.Particles.Add(p);
                p.Index_ParticleVertex = i;
                this.ParticlesVertex.Add(p.CV_PositionColored);
            }
        }

        public override void InicializarTexturas()
        {
            Device d3dDevice = GuiController.Instance.D3dDevice;

            string path0 = GuiController.Instance.AlumnoEjemplosMediaDir + "Los_Borbotones\\Particulas\\humo.jpg";
             
            this.Textures.Add(TextureLoader.FromFile(d3dDevice, path0));
        }

        public override void ActualizarModifiers()
        {
            float SizeSpeed = (float)GuiController.Instance.Modifiers["Velocidad Tamaño"];
            if (this.SizeSpeed != SizeSpeed)
            {
                this.SizeSpeed = SizeSpeed;
            }
            base.ActualizarModifiers();
        }

        public override void AgregarModifiers(float width, float height)
        {
            //Crear un modifier para un valor INT
            GuiController.Instance.Modifiers.addInt("Cantidad", 1, 500, this.ParticlesAmount);

            //Crear un modifier para un valor FLOAT
            GuiController.Instance.Modifiers.addFloat("Tamaño MIN", 0f, 1f, this.PointSizeMin);

            //Crear un modifier para un valor FLOAT
            GuiController.Instance.Modifiers.addFloat("Tamaño MAX", 1f, 50f, this.PointSizeMax);

            //Crear un modifier para un valor FLOAT
            GuiController.Instance.Modifiers.addFloat("Velocidad", 0f, 1f, this.Speed.Y);

            //Crear un modifier para un valor FLOAT
            GuiController.Instance.Modifiers.addFloat("Velocidad Tamaño", 0f, 1f, this.SizeSpeed);

            //Crear un modifier para un valor FLOAT
            GuiController.Instance.Modifiers.addFloat("Divergencia", 0f, 0.5f, (this.Speed.X + this.Speed.Z) / 2f);

            //Crear un modifier para un valor FLOAT
            GuiController.Instance.Modifiers.addFloat("LifeTime", 0.1f, 20f, this.TimeToLive_Particle);

            //Crear un modifier para modificar un vértice
            //GuiController.Instance.Modifiers.addVertex3f("Posicion", new Vector3(0, 0, 0), new Vector3(width, height, 10), this.InitCoords);

        }

        public override void AgregarParticulas(int cantidad)
        {
            SmokePart p;
            int i;
            Vector3 dire;
            int tex;
            for (i = this.ParticlesAmount; i < (this.ParticlesAmount + cantidad); i++)
            {
                dire = this.RandVec3();
                tex = r.Next(0, this.Textures.Count);
                p = new SmokePart(this.InitCoords, this.Speed, this.Acceleration, dire, this.InitialColor, this.InitialAlpha, this.PointSizeMin, this.TimeToLive_Particle, this.Textures[tex],this.SizeSpeed);
                this.Particles.Add(p);
                p.Index_ParticleVertex = i;
                this.ParticlesVertex.Add(p.CV_PositionColored);
            }
            this.ParticlesAmount += cantidad;
        }

        //Vector random siempre para arriba (y = 1)
        public override Vector3 RandVec3()
        {
            Vector3 v3 = base.RandVec3();
            return new Vector3(v3.X,1,v3.Z);
        }
    }
}
