using AlumnoEjemplo.Los_Borbotones;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TgcViewer.Utils.TgcSceneLoader;

namespace AlumnoEjemplos.Los_Borbotones
{
    public class Inactivo : State
    {
        float SpawnTime = 20f;
        float Time = 0;
        public override void Update(float elapsedTime, Barril barril)
        {
            Time += elapsedTime;
            if (Time >= SpawnTime)
            {
                Respawn(barril);
            }
        }

        private static void Respawn(Barril barril)
        {
            barril.estado = new Activo();
            GameManager.Instance.agregarBarril(barril);
        }


        public override void Render(float elapsedTime, Barril barril)
        {



        }

        public override void explotar(Barril barril)
        {

        }
    }
}
