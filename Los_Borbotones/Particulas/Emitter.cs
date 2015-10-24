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
    public abstract class Emitter
    {
        #region ATRIBUTOS
        //Random global para uso de todos los sistemas
        public Random r = new Random();
        
        public int ParticlesAmount { get; set; }
        
        public int CurrentParticlesAmount { get; set; }
        
        public Vector3 InitCoords { get; set; }
        
        public Vector3 Speed { get; set; }
        
        public Vector3 Acceleration { get; set; }
        
        public float PointSizeMin { get; set; }
        
        public float PointSizeMax { get; set; }
        
        public float TimeToLive_Particle { get; set; }
        //Tiempo para crear la proxima particula
        public float TimeToCreateParticle { get; set; }
        //Tiempo transcurrido desde el ultimo render
        public float ElapsedTime { get; set; }
        //Tiempo para hacer cada update
        public float UpdateTime { get; set; }
        //Tiempo
        public float Spawn { get; set; }

        public Color InitialColor { get; set; }

        public int InitialAlpha { get; set; }

        public List<Texture> Textures { get; set; }

        public List<Particle> Particles { get; set; }
        //Lista de vertices(point sprites) con posicion y color
        public List<CustomVertex.PositionColored> ParticlesVertex { get; set; }
        //VertexBuffer que va a dibujar
        public VertexBuffer VertexBuffer_Emitter { get; set; }
        #endregion

        #region CONSTRUCTOR
        public Emitter()
        {
        }
        public Emitter(int particlesAmount, Vector3 origin, Vector3 speed, Vector3 aceleracion, float min, float max, float particleTime, Color initColor, int initAlpha, float time_createParticle,float updateTime)
        {
            Device d3dDevice = GuiController.Instance.D3dDevice;
            this.ElapsedTime = 0f;
            this.Spawn = 0f;
            this.CurrentParticlesAmount = 0;
            this.TimeToCreateParticle = time_createParticle;
            this.InitCoords = origin;
            this.Speed = speed;
            this.Acceleration = aceleracion;
            this.InitialColor = initColor;
            this.InitialAlpha = initAlpha;
            this.PointSizeMin = min;
            this.PointSizeMax = max;
            this.TimeToLive_Particle = particleTime;
            this.ParticlesAmount = particlesAmount;
            this.UpdateTime = updateTime;
            this.Textures = new List<Texture>();
            this.Particles = new List<Particle>();
            this.ParticlesVertex = new List<CustomVertex.PositionColored>();
            //Inicializo un vertexBuffer renderizalbe
            //EL tipo customVertex.PositionColored es un pointSprite que se le puede asignar posicion y color
            this.VertexBuffer_Emitter = new VertexBuffer(typeof(CustomVertex.PositionColored), this.ParticlesAmount, d3dDevice, Usage.Dynamic | Usage.WriteOnly | Usage.Points, CustomVertex.PositionColored.Format, Pool.Default);
        }
        #endregion

        #region METODOS
        #region INIT
        public virtual void Init()
        {
            this.InicializarTexturas();
            this.InicializarParticulas();
        }

        //Metodo que se va a sobreescribir en cada sistema
        public virtual void InicializarParticulas()
        {
            Particle p;
            Vector3 dire;
            int tex;
            for (int i = 0; i < (this.ParticlesAmount); i++)
            {
                //Obtengo una direccion random
                dire = this.RandVec3();
                //Obtengo la posicion de alguna de las texturas ya cargadas
                tex = r.Next(0, this.Textures.Count);
                p = new Particle(this.InitCoords, this.Speed, this.Acceleration, dire, this.InitialColor, this.InitialAlpha, this.PointSizeMin, this.TimeToLive_Particle, this.Textures[tex]);
                this.Particles.Add(p);
                //Indice correspondiente al ParticlesVertex
                p.Index_ParticleVertex = i;
                this.ParticlesVertex.Add(p.CV_PositionColored);
            }
        }

        //Se inicializan y cargan todas las texturas que se van a utilizar
        public abstract void InicializarTexturas();

        //Modifiers por defecto
        public virtual void AgregarModifiers(float width,float height)
        {
            GuiController.Instance.Modifiers.addInt("Cantidad", 1, 10000, this.ParticlesAmount);
            GuiController.Instance.Modifiers.addFloat("Tamaño MIN",0f,10f,this.PointSizeMin);
            GuiController.Instance.Modifiers.addFloat("Tamaño MAX", 1f, 10f, this.PointSizeMax);
            GuiController.Instance.Modifiers.addFloat("Velocidad", -10f, 10f, this.Speed.Y);
            GuiController.Instance.Modifiers.addFloat("Divergencia", 0f, 2f, (this.Speed.X + this.Speed.Z)/2f);
            GuiController.Instance.Modifiers.addFloat("LifeTime", 0.1f, 5f, this.TimeToLive_Particle);
            GuiController.Instance.Modifiers.addVertex3f("Posicion", new Vector3(0, 0, 0), new Vector3(width, height, 10),this.InitCoords);
        }
        #endregion

        #region UPDATE
        public virtual void ActualizarModifiers()
        {
            //Obtener valores de Modifiers
            float speed = (float)GuiController.Instance.Modifiers["Velocidad"];
            if (this.Speed.Y != speed)
            {
                this.Speed = new Vector3(this.Speed.X, speed, this.Speed.Z);
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

            float divergencia = (float)GuiController.Instance.Modifiers["Divergencia"];
            if (this.Speed.X != divergencia)
            {
                this.Speed = new Vector3(divergencia, speed, divergencia);
            }

            float lifeTime = (float)GuiController.Instance.Modifiers["LifeTime"];
            if (this.TimeToLive_Particle != lifeTime)
            {
                this.TimeToLive_Particle = lifeTime;
            }

            /*
            Vector3 pos = (Vector3)GuiController.Instance.Modifiers["Posicion"];
            if (this.InitCoords != pos)
            {
                this.InitCoords = pos;
            }
             */

            int cantidad = (int)GuiController.Instance.Modifiers["Cantidad"];
            //Agrega o saca la diferencia de la cantidad de particulas actuales con las del modifier
            if (this.ParticlesAmount != cantidad)
            {
                this.Upd_ParticlesAmount(cantidad - this.ParticlesAmount);
            }
        }

        //Actualizo la cantidad de particulas
        public void Upd_ParticlesAmount(int cantidad)
        {
            //Cambio el tamaño del vertexBuffer
            this.ChangeSize_VertexBuffer(cantidad);
            if (cantidad > 0)
            {
                this.AgregarParticulas(cantidad);
            }
            else
            {
                this.SacarParticulas(-cantidad);
            }
        }

        //Hago un dispose del vertexBuffer para crear otro con un nuevo tmaño
        private void ChangeSize_VertexBuffer(int cantidad)
        {
            Device d3dDevice = GuiController.Instance.D3dDevice;

            
            this.VertexBuffer_Emitter.Dispose();
            this.VertexBuffer_Emitter = new VertexBuffer(typeof(CustomVertex.PositionColored), (this.ParticlesAmount + cantidad), d3dDevice, Usage.Dynamic | Usage.WriteOnly | Usage.Points, CustomVertex.PositionColored.Format, Pool.Default);
        }

        //Se inicializan nuevas particulas(identico al init)
        public virtual void AgregarParticulas(int cantidad)
        {
            Particle p;
            int i;
            Vector3 dire;
            int tex;
            for (i = this.ParticlesAmount; i < (this.ParticlesAmount + cantidad); i++)
            {
                //Obtengo una direccion random
                dire = this.RandVec3();
                //Obtengo la posicion de alguna de las texturas ya cargadas
                tex = r.Next(0, this.Textures.Count);
                p = new Particle(this.InitCoords, this.Speed, this.Acceleration, dire, this.InitialColor, this.InitialAlpha, this.PointSizeMin, this.TimeToLive_Particle, this.Textures[tex]);
                this.Particles.Add(p);
                //Indice correspondiente al ParticlesVertex
                p.Index_ParticleVertex = i;
                this.ParticlesVertex.Add(p.CV_PositionColored);
            }
            this.ParticlesAmount += cantidad;
        }

        //Se matan n cantidad de particulas
        public virtual void SacarParticulas(int cantidad)
        {
            int cantVivas = this.ParticlesAmount - cantidad;
            for (int i = 0; i < cantidad; i++)
            {
                //Se elimina la ultima particula y se la saca del vertex
                this.Particles.RemoveAt(cantVivas);
                this.ParticlesVertex.RemoveAt(cantVivas);
            }
            this.ParticlesAmount -= cantidad;
        }

        //Render basico para todos los sistemas(Cada uno lo sobreescribe)
        public virtual void Render(float elapsedTime)
        {
            Device d3dDevice = GuiController.Instance.D3dDevice;
            //Aumento el tiempo transcurrido
            this.ElapsedTime += elapsedTime;
            //Si es mayor el tiempo de update preestablecido, realiza el update
            if (this.ElapsedTime > this.UpdateTime)
            {
                this.Update(elapsedTime);
                this.ElapsedTime = 0f;
            }

            //Meto en el buffer en vertexBuffer para que lo dibuje
            d3dDevice.SetStreamSource(0, this.VertexBuffer_Emitter, 0);

            foreach(Particle p in this.Particles)
            {
                if (p.Active)
                {
                    d3dDevice.SetTexture(0, p.Tex);
                    d3dDevice.RenderState.PointSize = p.Size;
                    d3dDevice.DrawPrimitives(PrimitiveType.PointList, p.Index_ParticleVertex, 1);
                }
            }
        }

        public virtual void Update(float elapsedTime)
        {
            Device d3dDevice = GuiController.Instance.D3dDevice;
            
            this.Spawn += elapsedTime;

            //Paso el tiempo de crear otra particula
            if (this.Spawn > this.TimeToCreateParticle)
            {
                this.RevivirParticula();
                this.Spawn = 0f;
            }

            foreach (Particle p in this.Particles)
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

                    //Se renderiza si todavia no murio o si se sigue dibujando(por alpha)
                    if (p.LifeTime < p.TimeToLive && p.Alpha > 0f)
                    {
                        p.Render(this);
                    }
                    else
                    {
                        //p.Matar(this);
                    }
                    //Actualiza el point sprite de la particula
                    p.CV_PositionColored = new CustomVertex.PositionColored(p.CurrentPos, Color.FromArgb(p.Alpha, p.Colour).ToArgb());
                    //Se mete en el vertices de point sprites
                    this.ParticlesVertex[p.Index_ParticleVertex] = p.CV_PositionColored;
                }
            }

            //Se mete en el vertex buffer todos los point sprites vivos
            this.VertexBuffer_Emitter.SetData(this.ParticlesVertex.ToArray(), 0, LockFlags.Discard);
        }

        #endregion

        #region UTILS
        //Revivo la primer particula que encuentro muerta
        public virtual void RevivirParticula()
        {
            foreach (Particle p in this.Particles)
            {
                if (!p.Active)
                {
                    p.Active = true;
                    return;
                }
            }
        }

        //Se reviven todas las particulas(esten muertas o no)
        public virtual void RevivirParticulas()
        {
            foreach (Particle p in this.Particles)
            {
                if (!p.Active)
                    p.Active = true;
            }
        }

        //Devuelve si anunque sea una particula esta viva
        public virtual bool Active()
        {
            foreach (Particle p in this.Particles)
            {
                if (p.Active)
                    return true;
            }
            return false;
        }

        //Obtengo un valor random entre dos valores float
        public float GetRandomMinMax(float fMin, float fMax)
        {
            float fRand = (float)r.NextDouble();
            return fMin + (fMax - fMin) * fRand;
        }

        public virtual Vector3 RandVec3()
        {
            float x = 1f, y = 1f, z = 1f;
            int r1 = r.Next(-1000, 1001);
            int r2 = r.Next(-1000, 1001);

            if (r1 != 0)
                x = r1 / Math.Abs(r1);
            if (r2 != 0)
                z = r2 / Math.Abs(r2);

            x *= (float)r.NextDouble();
            y = (float)r.NextDouble();
            z *= (float)r.NextDouble();
            return new Vector3(x, y, z);
        }

        //Obtengo el origen de coordenadas
        public virtual Vector3 DameCoordenadasEmision()
        {
            return this.InitCoords;
        }
        #endregion

        #region DISPOSE
        public virtual void Dispose()
        {
            this.VertexBuffer_Emitter.Dispose();
            for (int i = 0; i < this.Textures.Count; i++)
            {
                this.Textures[i].Dispose();
            }
        }
        
        #endregion
        #endregion
    }
}
