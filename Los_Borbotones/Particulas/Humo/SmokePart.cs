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
    public class SmokePart:Particle
    {
        public float SizeSpeed { get; set; }

        public SmokePart(Vector3 pos, Vector3 velocidad, Vector3 aceleracion, Vector3 direccion, Color color, int alpha, float size, float timeToLive, Texture tex,float sizeSpeed)
            : base(pos, velocidad, aceleracion, direccion, color, alpha, size, timeToLive, tex) 
        {
            this.SizeSpeed = sizeSpeed;
        }

        public override void Render(Emitter emisor)
        {
            base.Render(emisor);
            if (this.Spawn > 0.0005f)
            {
                if (Alpha > 0)
                    this.Alpha--;
                this.Spawn = 0f;
            }
            this.Agrandar(emisor);
        }

        public override void Update_Modifiers(Emitter emisor)
        {
            SmokeEmitter e = (SmokeEmitter)emisor;
            if (e.SizeSpeed != this.SizeSpeed)
            {
                this.SizeSpeed = e.SizeSpeed;
            }
            base.Update_Modifiers(emisor);
        }

        //Se agranda la particula de acuerdo a la velocidad de tamaño
        public void Agrandar(Emitter emisor)
        {
            if((this.Size + this.SizeSpeed * this.LifeTime) <= emisor.PointSizeMax)
                this.Size += this.SizeSpeed * this.LifeTime;
        }

        
        public override void Matar(Emitter emisor)
        {
            base.Matar(emisor);
            //Vuelve al tamaño inicial
            this.Size = emisor.PointSizeMin;
        }
    }
}
