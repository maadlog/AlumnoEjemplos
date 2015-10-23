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
    public class SplashPart : Particle
    {
        public SplashPart(Vector3 pos, Vector3 velocidad, Vector3 aceleracion, Vector3 direccion, Color color, int alpha, float size, float posToLive, Texture tex)
            : base(pos, velocidad, aceleracion, direccion, color, alpha, size, posToLive, tex) 
        {}

        public override void Render(Emitter emisor)
        {
            base.Render(emisor);
            //Se disminuye el alpha para simular efecto de dispersion
            if (this.Alpha > 5)
                this.Alpha -= 5;
            else if (this.Alpha > 0) 
                this.Alpha--;
        }

        public override void Update_Modifiers(Emitter emisor)
        {
            if (this.InitialPos != emisor.InitCoords)
            {
                this.CurrentPos += (emisor.InitCoords - this.InitialPos);
                this.InitialPos = emisor.InitCoords;
            }

            if (emisor.Speed != this.Speed)
            {
                this.Speed = emisor.Speed;
            }
        }
    }
}
