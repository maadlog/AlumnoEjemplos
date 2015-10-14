using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TgcViewer.Utils.TgcSceneLoader;

namespace AlumnoEjemplos.Los_Borbotones
{
    class Inactivo : State
    {
        float SpawnTime = 20f;
        float Time = 0;
        public override void Update(float elapsedTime, Barril barril)
        {
            Time += elapsedTime;
            if (Time >= SpawnTime)
            {
                barril.estado = new Activo();
            }
        }


        public override void Render(float elapsedTime, TgcMesh mesh)
        {



        }

        public override void explotar(Barril barril)
        {

        }
    }
}
