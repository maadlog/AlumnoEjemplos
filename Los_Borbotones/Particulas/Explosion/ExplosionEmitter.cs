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
    public class ExplosionEmitter : Emitter
    {
        #region ATRIBUTOS
        //Para obtener los randmo
        public int Seed { get; set; }

        //Velocidad para aumentar de tamaño
        public float SizeSpeed { get; set; }

        //Lista de colores que van a tener las particulas creadas
        public List<Color> Colours { get; set; }

        //La cantidad de alpha que va disminuyendo en cada render
        public int AlphaDecr { get; set; }
        #endregion

        #region CONSTRUCTOR
        public ExplosionEmitter(int particlesAmount, Vector3 origin, Vector3 speed, Vector3 aceleracion, float min, float max, float particleTime, Color initColor, int initAlpha, float spawn,float updateTime, int seed, float sizeSpeed, int alphaDecr)
            : base(particlesAmount, origin, speed, aceleracion, min, max, particleTime, initColor, initAlpha, spawn,updateTime)
        {
            this.Seed = seed;
            this.SizeSpeed = sizeSpeed;
            this.AlphaDecr = alphaDecr;
            this.InicializarColores();
        }
        #endregion

        #region METHODS
        #region INIT
        public void InicializarColores()
        {
            this.Colours = new List<Color>();

            this.Colours.Add(Color.White);
            this.Colours.Add(Color.WhiteSmoke);
            this.Colours.Add(Color.LightGray);
            this.Colours.Add(Color.Gray);
            this.Colours.Add(Color.DarkGray);
            this.Colours.Add(Color.Orange);
        }

        public override void InicializarParticulas()
        {
            Random r = new Random(this.Seed);
            int i, posTex = 0, posCol = 0;
            Vector3 randomVect;
            ExplosionPart p;

            //Creo una primera particula sin direccion asi queda en el centro
            posCol = r.Next(0, this.Colours.Count);
            p = new ExplosionPart(this.InitCoords, this.Speed, this.Acceleration, new Vector3(0, 0, 0), this.Colours[posCol], this.InitialAlpha, this.PointSizeMin, this.TimeToLive_Particle, this.Textures[0], this.SizeSpeed);
            this.Particles.Add(p);
            this.ParticlesVertex.Add(p.CV_PositionColored);
            
            //Se crean las particulas restantes
            for (i = 0; i < (this.ParticlesAmount-1); i++)
            {
                randomVect = this.RandVec3();
                posTex = r.Next(0, this.Textures.Count);
                posCol = r.Next(0, this.Colours.Count);
                p = new ExplosionPart(this.InitCoords, this.Speed, this.Acceleration, randomVect, this.Colours[posCol], this.InitialAlpha, this.PointSizeMin, this.TimeToLive_Particle, this.Textures[0], this.SizeSpeed);
                this.Particles.Add(p);
                //Se le pone i+1 por la particula que ya se creo antes del ciclo
                p.Index_ParticleVertex = (i+1);
                this.ParticlesVertex.Add(p.CV_PositionColored);
            }
        }

        public override void InicializarTexturas()
        {
            Device d3dDevice = GuiController.Instance.D3dDevice;

            string path0 = GuiController.Instance.AlumnoEjemplosMediaDir + "Los_Borbotones\\Particulas\\explosion.jpg";
            this.Textures.Add(TextureLoader.FromFile(d3dDevice, path0));
        }

        public override void AgregarModifiers(float width, float height)
        {
            GuiController.Instance.Modifiers.addInt("Cantidad", 1, 20, this.ParticlesAmount);
            GuiController.Instance.Modifiers.addFloat("Tamaño", 0f, 40f, this.PointSizeMax);
            GuiController.Instance.Modifiers.addFloat("Velocidad", 0f, 0.1f, this.Speed.X);
            GuiController.Instance.Modifiers.addFloat("Velocidad Tamaño", 0f, 5f, this.SizeSpeed);
            GuiController.Instance.Modifiers.addVertex3f("Posicion", new Vector3(0, 0, 0), new Vector3(width, height, 10), this.InitCoords);
        }
        #endregion

        #region UPDATE
        public override void ActualizarModifiers()
        {

            float speed = (float)GuiController.Instance.Modifiers["Velocidad"];
            if (this.Speed.X != speed)
            {
                this.Speed = new Vector3(speed, speed, speed);
            }

            float sizeSpeed = (float)GuiController.Instance.Modifiers["Velocidad Tamaño"];
            if (this.SizeSpeed != sizeSpeed)
            {
                this.SizeSpeed = sizeSpeed;
            }

            float sizeMax = (float)GuiController.Instance.Modifiers["Tamaño"];
            if (this.PointSizeMax != sizeMax)
            {
                this.PointSizeMax = sizeMax;
            }

            Vector3 pos = (Vector3)GuiController.Instance.Modifiers["Posicion"];
            if (this.InitCoords != pos)
            {
                this.InitCoords = pos;
            }

            int cantidad = (int)GuiController.Instance.Modifiers["Cantidad"];
            if (this.ParticlesAmount != cantidad)
            {
                this.Upd_ParticlesAmount(cantidad - this.ParticlesAmount);
            }
        }

        public override void AgregarParticulas(int cantidad)
        {
            Vector3 randomVect;
            ExplosionPart p;
            int i, posTex = 0, posCol = 0;
            for (i = this.ParticlesAmount; i < (this.ParticlesAmount + cantidad); i++)
            {
                randomVect = this.RandVec3();
                posTex = r.Next(0, this.Textures.Count);
                posCol = r.Next(0, this.Colours.Count);
                p = new ExplosionPart(this.InitCoords, this.Speed, this.Acceleration, randomVect, this.Colours[posCol], this.InitialAlpha, this.PointSizeMin, this.TimeToLive_Particle, this.Textures[0], this.SizeSpeed);
                this.Particles.Add(p);
                //Se le pone i+1 por la particula que ya se creo antes del ciclo
                p.Index_ParticleVertex = (i + 1);
                this.ParticlesVertex.Add(p.CV_PositionColored);
            }
            this.ParticlesAmount += cantidad;
        }

        public override void Update(float elapsedTime)
        {
            Device d3dDevice = GuiController.Instance.D3dDevice;
            CustomVertex.PositionColored pc;

            foreach (ExplosionPart p in this.Particles)
            {
                if (p.Active)
                {
                    //Si llega al tiempo de vida
                    if (elapsedTime <= p.TimeToLive)
                    {
                        p.LifeTime += elapsedTime;
                        p.Spawn += elapsedTime;
                    }
                    //Si se pasa del tiempo de vida
                    else
                    {
                        p.LifeTime = p.TimeToLive;
                        p.Spawn = p.TimeToLive;
                    }

                    //Si la particula desaparece
                    if (p.Alpha >0f)
                    {
                        p.Render(this);
                    }
                    else
                    {
                        p.Matar(this);
                    }
                }
                pc = new CustomVertex.PositionColored(p.CurrentPos, Color.FromArgb(p.Alpha, p.Colour).ToArgb());
                this.ParticlesVertex[this.Particles.IndexOf(p)] = pc;
            }

            this.VertexBuffer_Emitter.SetData(this.ParticlesVertex.ToArray(), 0, LockFlags.Discard);
        }
        #endregion

        #region UTILS
        //Se obtiene un vector random de acuerdo a un radio (forma de esfera)
        public override Vector3 RandVec3()
        {
            Vector3 rVec;

            rVec.Z = this.GetRandomMinMax(-1.0f,1.0f);

            float radius = (float)Math.Sqrt(1 - rVec.Z * rVec.Z);

            float t = this.GetRandomMinMax(-(float)Math.PI,(float)Math.PI);

            rVec.X = (float)Math.Cos(t) * radius;
            rVec.Y = (float)Math.Sin(t) * radius;

            return rVec;
        }
        #endregion
        #endregion
    }
}
