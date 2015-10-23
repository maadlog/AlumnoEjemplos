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
    public class Particle
    {
        #region ATRIBUTOS
        public Vector3 InitialPos { get; set; }

        public Vector3 CurrentPos { get; set; }

        public Vector3 Speed { get; set; }

        public Vector3 Acceleration { get; set; }

        public Vector3 Direction { get; set; }
        
        public float Size { get; set; }

        public Color Colour { get; set; }

        public int Alpha { get; set; }

        public Texture Tex { get; set; }

        public float TimeToLive { get; set; }

        public float LifeTime { get; set; }

        public float Spawn { get; set; }

        public bool Active { get; set; }

        //Indice para buscar la particula en el ParticleVertex
        public int Index_ParticleVertex { get; set; }

        //Point sprite de la particula
        public CustomVertex.PositionColored CV_PositionColored { get; set; }

        static public Color DEFAULT_COLOR = Color.FromArgb(255, 255, 255);
        #endregion

        #region CONSTRUCTORES
        public Particle(Vector3 pos, Vector3 velocidad, Vector3 aceleracion, Vector3 direccion, Color color, int alpha, float size, float timeToLive)
        {
            this.InitialPos = pos;
            this.CurrentPos = this.InitialPos;
            this.Speed = velocidad;
            this.Acceleration = aceleracion;
            this.Direction = direccion;
            this.Colour = color;
            this.Alpha = alpha;
            this.Size = size;
            this.LifeTime = 0f;
            this.TimeToLive = timeToLive;
            this.Spawn = 0f;
            this.Active = false;
            this.Index_ParticleVertex = 0;
            this.CV_PositionColored = new CustomVertex.PositionColored(this.CurrentPos, Color.FromArgb(this.Alpha, this.Colour).ToArgb());
        }

        public Particle(Vector3 pos, Vector3 velocidad, Vector3 aceleracion, Vector3 direccion, Color color, int alpha, float size, float timeToLive,Texture tex)
        {
            this.InitialPos = pos;
            this.CurrentPos = pos;
            this.Speed = velocidad;
            this.Acceleration = aceleracion;
            this.Direction = direccion;
            this.Colour = color;
            this.Alpha = alpha;
            this.Size = size;
            this.LifeTime = 0f;
            this.TimeToLive = timeToLive;
            this.Spawn = 0f;
            this.Active = false;
            this.Index_ParticleVertex = 0;
            this.Tex = tex;
        }
        #endregion

        #region METODOS
        #region UPDATE
        public virtual void Render(Emitter emisor)
        {
            this.Active = true;
            this.Update_Modifiers(emisor);
            float x = this.Mover(this.CurrentPos.X,this.Direction.X,this.Speed.X,this.Acceleration.X);
            float y = this.Mover(this.CurrentPos.Y, this.Direction.Y, this.Speed.Y, this.Acceleration.Y);
            float z = this.Mover(this.CurrentPos.Z, this.Direction.Z, this.Speed.Z, this.Acceleration.Z);

            this.CurrentPos = new Vector3(x,y,z);
        }

        public virtual float Mover(float pos, float dire, float speed, float acce)
        {
            return pos + (dire * ((speed * this.LifeTime) + (0.5f * acce * this.LifeTime * this.LifeTime)));
        }

        //Actualizo velocidad, divergencia, posicion y tiempo de vida
        public virtual void Update_Modifiers(Emitter emisor)
        {
            if (emisor.Speed != this.Speed)
            {
                this.Speed = emisor.Speed;
            }

            if (emisor.TimeToLive_Particle != this.TimeToLive)
            {
                this.TimeToLive = emisor.TimeToLive_Particle;
            }

            if (this.InitialPos != emisor.InitCoords)
            {
                this.CurrentPos += (emisor.InitCoords - this.InitialPos);
                this.InitialPos = emisor.InitCoords;
            }
        }
        #endregion

        #region KILL
        public virtual void Matar(Emitter emisor)
        {
            this.CurrentPos = emisor.DameCoordenadasEmision();
            this.Alpha = emisor.InitialAlpha;
            this.LifeTime = 0f;
            this.Spawn = 0f;
            this.Active = false;
        }
        #endregion
        #endregion
    }
}
