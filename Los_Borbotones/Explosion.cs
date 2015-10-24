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
                time = 0;

                int width = GuiController.Instance.Panel3d.Width;
                int height = GuiController.Instance.Panel3d.Height;

                int cantExplosion = 20;
                float particleTime = 1f, sizeMax = 2000f, expSpeed = 1f, expSizeSpeed = 20f; ;
                float expUpdateTime = 0;

                //Creo el emisor de explosion
                emisorExplosion = new ExplosionEmitter(cantExplosion, posicion, new Vector3(expSpeed, expSpeed, expSpeed), new Vector3(0.00f, 0.00f, 0.00f), 500f, sizeMax, particleTime, Color.White, 150, 0f, expUpdateTime, GameManager.Instance.random.Next(0, 1000), expSizeSpeed, 2);
                emisorExplosion.Init();
                
                
                int cantidad = 20;
                Vector3 origen1 = posicion;
                float speed = 5f;
                float divergence = 7f;
                Vector3 velocidad = new Vector3(divergence, speed, divergence);
                Vector3 aceleracion = new Vector3(0, 0, 0);
                float min = 500f, max = 1000f, tiempoVida_Particula = 1f;
                int alpha = 150;
                float spawn = 0.01f;
                float sizeSpeed = 100f;
                float updateTime = 0.02f;

                //Creo los emisores de humo
                emisorHumo = new SmokeEmitter(cantidad, origen1, velocidad, aceleracion, min, max, tiempoVida_Particula, Color.DarkGray, alpha, spawn, updateTime, sizeSpeed);
                emisorHumo.Init();
                
            }

            public void Explotar()
            {
                GameManager.Instance.playSound(sound, explosionSoundDir, false);
                GameManager.Instance.enemies.ForEach(enemy => chequearColision(sphere, enemy));
                GameManager.Instance.barriles.ForEach(barril => chequearColision(sphere, barril));
            }


        public void render(float elapsedTime){
            Microsoft.DirectX.Direct3D.Device device = GuiController.Instance.D3dDevice;
            SystemState_Particulas.Instance.SetRenderState();
            //device.RenderState.PointSize = 1000f;
            if (time == 0)
            {
                emisorExplosion.RevivirParticulas();
            }
            if (time > 1)
            {
                emisorExplosion.Dispose();
                //emisorParticulas.Playing = false;
                //emisorParticulas.dispose();
               
            }
            else
            {
                emisorExplosion.Render(elapsedTime);
                //emisorParticulas.render();
            }
            if (time > 0.25f && time <= 5.0f)
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
            time += elapsedTime;
        }
        private static void chequearColision(TgcBoundingSphere sphere, Enemy enemy)
        {
            if (TgcCollisionUtils.testSphereAABB(sphere, enemy.mesh.BoundingBox))
            {
                GameManager.Instance.sumarScore(enemy);
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
