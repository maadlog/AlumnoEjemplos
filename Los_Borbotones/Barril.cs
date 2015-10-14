using AlumnoEjemplo.Los_Borbotones;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TgcViewer.Utils.TgcGeometry;

namespace AlumnoEjemplos.Los_Borbotones
{
    class Barril : GameObject


    {
        public State estado = new Activo();

        public TgcBoundingSphere explosion;
        float EXPLOSION_RADIUS = 25f;

        public override void Init()
        {
            explosion = new TgcBoundingSphere(mesh.Position, EXPLOSION_RADIUS); 
        }

        public override void Update(float elapsedTime)
        {
            estado.Update(elapsedTime, this);
        }

       
         public override void Render(float elapsedTime)
         {

             estado.Render(elapsedTime, mesh);
                 
         }

         public void explotar()
         {
             estado.explotar(this);
         }
    }
}
