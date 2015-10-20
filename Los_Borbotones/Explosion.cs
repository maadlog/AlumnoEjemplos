using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AlumnoEjemplos.Los_Borbotones;
using TgcViewer.Utils.Particles;
using Microsoft.DirectX.Direct3D;
using TgcViewer;
using Microsoft.DirectX;
using TgcViewer.Utils.TgcGeometry;
using TgcViewer.Utils.Sound;

namespace AlumnoEjemplos.Los_Borbotones
{
   public class Explosion
    {
            public Vector3 posicion;
            public TgcBoundingSphere sphere;
            float EXPLOSION_RADIUS = 300f;
            Device device = GuiController.Instance.D3dDevice;
            float time = 0;
            string explosionSoundDir = GuiController.Instance.AlumnoEjemplosMediaDir + "Los_Borbotones\\Audio/Barril/explosion.wav";
            TgcStaticSound sound;

            ParticleEmitter emisorParticulas;
            ParticleEmitter emisor;
            public void init() {
                
                Device device = GuiController.Instance.D3dDevice;
                sphere = new TgcBoundingSphere(posicion, EXPLOSION_RADIUS);

                sound = new TgcStaticSound();

                Explotar();
                emisorParticulas = new ParticleEmitter(GuiController.Instance.AlumnoEjemplosMediaDir + "Los_Borbotones\\Particulas\\explosion.png", 100);
                emisorParticulas.Position = posicion + new Vector3(0,50,0);
                emisorParticulas.Dispersion = 2000;
                emisorParticulas.ParticleTimeToLive = 1f;
               // emisorParticulas.CreationFrecuency = 0.04f;
                emisorParticulas.MinSizeParticle = 1.0f;
                emisorParticulas.MaxSizeParticle = 5.0f;
               // emisorParticulas.Speed = new Vector3(10f,10f,10f);
               
                time = GuiController.Instance.ElapsedTime;

                emisor = new ParticleEmitter(GuiController.Instance.AlumnoEjemplosMediaDir + "Los_Borbotones\\Particulas\\humo.png", 50);
                emisor.Position = posicion + new Vector3(0, 50, 0);
                emisor.Dispersion = 2500;
                emisor.ParticleTimeToLive = 1.0f;
               // emisor.CreationFrecuency = 0.01f;
                emisor.MinSizeParticle = 1.0f;
                emisor.MaxSizeParticle = 2.0f;
                //emisor.Speed = new Vector3(10f, 10f, 10f);
                
            }

            public void Explotar()
            {
                GameManager.Instance.playSound(sound, explosionSoundDir, false);
                GameManager.Instance.enemies.ForEach(enemy => chequearColision(sphere, enemy));
                GameManager.Instance.barriles.ForEach(barril => chequearColision(sphere, barril));
            }
            //emisor.GradoDeDispersion = 1000;
            //emisor.TiempoDeVidaParticula = 1.0f;
            //emisor.TiempoFrecuenciaDeCreacion = 0.01f;
            //emisor.MinTamañoParticula = 2.0f;
            //emisor.MaxTamañoParticula = 5.0f;
            //emisor.FactorDeVelocidad = 5.0f;
            //emisor.ColorInicialParticula = System.Drawing.Color.FromArgb(0xff, 0xff, 0xff, 0xff);
            //emisor.ColorFinalParticula   = System.Drawing.Color.FromArgb(0x00, 0xff, 0xff, 0xff);
            //time = GuiController.Instance.ElapsedTime;

        public void render(float elapsedTime){
            time += elapsedTime;
            if (time > 1)
            {
                emisorParticulas.Playing = false;
                emisorParticulas.dispose();
               
            }
            else
            {
                emisorParticulas.render();
            }
            if (time > 0.75f && time <= 2.0f)
            {
                emisor.render();
            }
            if (time > 2.0f)
            {
                emisor.Playing = false;
                emisor.dispose();
                
            }
        }
        private static void chequearColision(TgcBoundingSphere sphere, Enemy enemy)
        {
            if (TgcCollisionUtils.testSphereAABB(sphere, enemy.mesh.BoundingBox))
            {
                GameManager.Instance.eliminarEnemigo(enemy);
            }
        }
        private static void chequearColision(TgcBoundingSphere sphere, Barril barril)
        {
            if (TgcCollisionUtils.testSphereAABB(sphere, barril.mesh.BoundingBox))
            {
                barril.explotar();
            }
        }
    }
}
