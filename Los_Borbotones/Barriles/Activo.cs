using AlumnoEjemplos.Los_Borbotones;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TgcViewer;
using TgcViewer.Utils.Sound;
using TgcViewer.Utils.TgcGeometry;
using TgcViewer.Utils.TgcSceneLoader;
using TgcViewer.Utils.Particles;

namespace AlumnoEjemplos.Los_Borbotones
{
    public class Activo : State
    {
        
        string explosionSoundDir = GuiController.Instance.AlumnoEjemplosMediaDir + "Los_Borbotones\\Audio/Barril/explosion.wav";
        public override void Update(float elapsedTime, Barril barril)
        {

        }


        public override void Render(float elapsedTime, Barril barril)
        {

            barril.mesh.render();

            if (GameManager.Instance.drawBoundingBoxes)
            {
               // barril.explosion.render();
            }

            
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

            barril.estado = new Inactivo();

            GameManager.Instance.eliminarBarril(barril);
            GameManager.Instance.playSound(explosionSoundDir);
            barril.explosionParticle = new Explosion();
             barril.explosionParticle.posicion = barril.mesh.Position;
             barril.explosionParticle.init();
        }

       
    }
}
