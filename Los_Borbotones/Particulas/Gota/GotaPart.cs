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
    public class GotaPart : Particle
    {
        public GotaPart(Vector3 pos, Vector3 velocidad, Vector3 aceleracion, Vector3 direccion, Color color, int alpha, float size, float posToLive, Texture tex)
            : base(pos, velocidad, aceleracion, direccion, color, alpha, size, posToLive, tex) 
        {
            //Altura que vive la particula
            this.TimeToLive = pos.Y - posToLive;
        }

        public override void Render(Emitter emisor)
        {
            this.Agrandar(emisor);
            base.Render(emisor);
        }

        //Se agranda la particula de acuerdo a la velocidad de tamaño
        private void Agrandar(Emitter emisor)
        {
            GotaEmitter e = (GotaEmitter)emisor;
            if ((this.Size + e.SizeSpeed * this.LifeTime) <= emisor.PointSizeMax)
                this.Size += e.SizeSpeed * this.LifeTime;
        }

        public override void Update_Modifiers(Emitter emisor)
        {
            if (this.InitialPos != emisor.InitCoords)
            {
                this.CurrentPos += (emisor.InitCoords - this.InitialPos);
                this.InitialPos = emisor.InitCoords;
            }

            if (this.TimeToLive != (emisor.InitCoords.Y - emisor.TimeToLive_Particle))
            {
                this.TimeToLive = (emisor.InitCoords.Y - emisor.TimeToLive_Particle);
            }
        }

        public override void Matar(Emitter emisor)
        {
            base.Matar(emisor);
            this.Size = emisor.PointSizeMin;
        }
    }
}
