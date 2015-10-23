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
using System.Drawing;

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

            ExplosionEmitter emisorExplosion;
            SmokeEmitter emisorHumo;
            public void init() {
                
                Device device = GuiController.Instance.D3dDevice;
                sphere = new TgcBoundingSphere(posicion, EXPLOSION_RADIUS);

                sound = new TgcStaticSound();
                time = GuiController.Instance.ElapsedTime;

                Explotar();

                int width = GuiController.Instance.Panel3d.Width;
                int height = GuiController.Instance.Panel3d.Height;

                int cantExplosion = 10;
                float particleTime = 1f, sizeMax = 5000f, expSpeed = 5f, expSizeSpeed = 50f; ;
                float expUpdateTime = 0;

                //Creo el emisor de explosion
                emisorExplosion = new ExplosionEmitter(cantExplosion, posicion, new Vector3(expSpeed, expSpeed, expSpeed), new Vector3(0.00f, 0.00f, 0.00f), 500f, sizeMax, particleTime, Color.White, 150, 0f, expUpdateTime, GameManager.Instance.random.Next(0, 1000), expSizeSpeed, 2);
                emisorExplosion.Init();
                
                
                int cantidad = 100;
                Vector3 origen1 = posicion;
                float speed = 5f;
                float divergence = 5f;
                Vector3 velocidad = new Vector3(divergence, speed, divergence);
                Vector3 aceleracion = new Vector3(0, 0, 0);
                float min = 500f, max = 5000f, tiempoVida_Particula = 2f;
                int alpha = 150;
                float spawn = 0.02f;
                float sizeSpeed = 100f;
                float updateTime = 0.03f;

                //Creo los emisores de humo
                emisorHumo = new SmokeEmitter(cantidad, origen1, velocidad, aceleracion, min, max, tiempoVida_Particula, Color.DarkGray, alpha, spawn, updateTime, sizeSpeed);
                emisorHumo.Init();
                /*
                emisorParticulas = new ParticleEmitter(GuiController.Instance.AlumnoEjemplosMediaDir + "Los_Borbotones\\Particulas\\explosion.png", 100);
                emisorParticulas.Position = posicion + new Vector3(0,50,0);
                emisorParticulas.Dispersion = 2000;
                emisorParticulas.ParticleTimeToLive = 1f;
               // emisorParticulas.CreationFrecuency = 0.04f;
                emisorParticulas.MinSizeParticle = 1.0f;
                emisorParticulas.MaxSizeParticle = 5.0f;
               // emisorParticulas.Speed = new Vector3(10f,10f,10f);

                emisor = new ParticleEmitter(GuiController.Instance.AlumnoEjemplosMediaDir + "Los_Borbotones\\Particulas\\humo.png", 50);
                emisor.Position = posicion + new Vector3(0, 50, 0);
                emisor.Dispersion = 2500;
                emisor.ParticleTimeToLive = 1.0f;
               // emisor.CreationFrecuency = 0.01f;
                emisor.MinSizeParticle = 1.0f;
                emisor.MaxSizeParticle = 2.0f;
                //emisor.Speed = new Vector3(10f, 10f, 10f);
                 */
                
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
            Microsoft.DirectX.Direct3D.Device device = GuiController.Instance.D3dDevice;
            time += elapsedTime;
            SystemState_Particulas.Instance.SetRenderState();
            //device.RenderState.PointSize = 1000f;
            if (time > 1)
            {
                emisorExplosion.Dispose();
                //emisorParticulas.Playing = false;
                //emisorParticulas.dispose();
               
            }
            else
            {
                emisorExplosion.RevivirParticulas();
                emisorExplosion.Render(elapsedTime);
                //emisorParticulas.render();
            }
            if (time > 0.75f && time <= 5.0f)
            {
                emisorHumo.Render(elapsedTime);
                //emisor.render();
            }
            if (time > 5.0f)
            {
                emisorHumo.Dispose();
                //emisor.Playing = false;
                //emisor.dispose();
                
            }
            SystemState_Particulas.Instance.SetRenderState_Zero();
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
