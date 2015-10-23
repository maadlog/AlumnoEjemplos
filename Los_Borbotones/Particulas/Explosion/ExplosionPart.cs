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
    public class ExplosionPart : Particle
    {
        #region ATRIBUTOS
        public float SizeSpeed { get; set; }
        #endregion

        #region CONSTRUCTOR
        public ExplosionPart(Vector3 pos, Vector3 velocidad, Vector3 aceleracion, Vector3 direccion, Color color, int alpha, float size, float timeToLive, Texture tex,float sizeSpeed)
            : base(pos,velocidad,aceleracion,direccion,color,alpha,size,timeToLive,tex)
        {
            this.SizeSpeed = sizeSpeed;
        }
        #endregion

        #region METHODS
        #region UPDATE
        public override void Update_Modifiers(Emitter emisor)
        {
            ExplosionEmitter e = (ExplosionEmitter)emisor;
            if (e.SizeSpeed != this.SizeSpeed)
            {
                this.SizeSpeed = e.SizeSpeed;
            }
            base.Update_Modifiers(emisor);
        }

        //Se agranda la particula de acuerdo al SizeSpeed
        private void Agrandar(Emitter emisor)
        {
            if ((this.Size + this.SizeSpeed * this.LifeTime) <= emisor.PointSizeMax)
                this.Size += this.SizeSpeed * this.LifeTime;
        }

        public override void Render(Emitter emisor)
        {
            this.Update_Modifiers(emisor);
            this.Agrandar(emisor);
            base.Render(emisor);
            ExplosionEmitter e = (ExplosionEmitter) emisor;
            if (this.Spawn > 0.01f)
            {
                if (this.Alpha > e.AlphaDecr)
                {
                    this.Alpha -= e.AlphaDecr;
                }
                else
                {
                    if (Alpha > 0)
                        this.Alpha--;
                }

                this.Spawn = 0f;
            }

        }
        #endregion

        #region KILL
        public override void Matar(Emitter emisor)
        {
            base.Matar(emisor);
            //Vuelve al tamaño inicial
            this.Size = emisor.PointSizeMin;
        }
        #endregion
        #endregion
    }
}
