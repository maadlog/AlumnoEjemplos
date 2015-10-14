using AlumnoEjemplo.Los_Borbotones;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TgcViewer.Utils.TgcGeometry;
using TgcViewer.Utils.TgcSceneLoader;

namespace AlumnoEjemplos.Los_Borbotones
{
    class Activo : State
    {
        public override void Update(float elapsedTime, Barril barril)
        {

        }


        public override void Render(float elapsedTime, TgcMesh mesh)
        {

            mesh.render();

        }

        public override void explotar(Barril barril)
        {
            foreach(Enemy enemigo in GameManager.Instance.enemies)
            {
                if(TgcCollisionUtils.testSphereAABB(barril.explosion, enemigo.mesh.BoundingBox))
                {
                    GameManager.Instance.eliminarEnemigo(enemigo);
                }
            }
            barril.estado = new Inactivo();
        }
    }
}
