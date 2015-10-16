using AlumnoEjemplo.Los_Borbotones;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TgcViewer.Utils.TgcGeometry;
using TgcViewer.Utils.TgcSceneLoader;

namespace AlumnoEjemplos.Los_Borbotones
{
    public class Activo : State
    {
        public override void Update(float elapsedTime, Barril barril)
        {

        }


        public override void Render(float elapsedTime, Barril barril)
        {

            barril.mesh.render();
            barril.explosion.render();
            

        }

        public override void explotar(Barril barril)
        {

           /* for (int i = GameManager.Instance.enemies.Count - 1; i >= 0; i--)
            {
                if(TgcCollisionUtils.testSphereAABB(barril.explosion, GameManager.Instance.enemies[i].mesh.BoundingBox))
                {
                    GameManager.Instance.eliminarEnemigo(GameManager.Instance.enemies[i]);
                }
            }*/
            GameManager.Instance.enemies.ForEach( enemy => chequearColision(barril, enemy));
            barril.estado = new Inactivo();

            GameManager.Instance.eliminarBarril(barril);
        }

        private static void chequearColision(Barril barril, Enemy enemy)
        {
            if (TgcCollisionUtils.testSphereAABB(barril.explosion, enemy.mesh.BoundingBox))
            {
                GameManager.Instance.eliminarEnemigo(enemy);
            }
        }
    }
}
