using AlumnoEjemplo.Los_Borbotones;
using Microsoft.DirectX;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using TgcViewer;
using TgcViewer.Utils.Sound;
using TgcViewer.Utils.TgcSceneLoader;

namespace AlumnoEjemplos.Los_Borbotones
{
    public class Weapon:GameObject
    {
        float WEAPON_ORIENTATION_Y;
        Vector3 WEAPON_OFFSET;
        public float MAX_DELAY;
        public float FIRE_DELAY;
        float weaponOscilation;
        public string weaponSoundDir;
        TgcStaticSound weaponSound;
        ExplosionEmitter muzzle;

        public override void Init()
        {
            weaponOscilation = 0;
            FIRE_DELAY = 0;
            MAX_DELAY = 2;
            weaponSound = new TgcStaticSound();

            //Permitir matrices custom
            mesh.AutoTransformEnable = false;
        }

        public override void Update(float elapsedTime)
        {
            WEAPON_OFFSET = (Vector3)GuiController.Instance.Modifiers["weaponOffset"];
            WEAPON_ORIENTATION_Y = (float)GuiController.Instance.Modifiers["weaponRotation"];
            mesh.Transform = getWeaponTransform();
            
            float length = ((CustomFpsCamera.Instance.eye - GameManager.Instance.player1.prevEye).Length());
            if (CustomFpsCamera.Instance.moveBackwardsPressed)
            {
                length *= -1;
            }
            weaponOscilation += length / 37;
        }

        public override void Render(float elapsedTime)
        {
            if (FIRE_DELAY > 0)
            {
                SystemState_Particulas.Instance.SetRenderState();
                muzzle.Render(elapsedTime);
                SystemState_Particulas.Instance.SetRenderState_Zero();
            }
            mesh.render();
        }

        public Matrix getWeaponTransform()
        {
            Matrix fpsMatrixInv = Matrix.Invert(CustomFpsCamera.Instance.ViewMatrix);

            float weaponRecoil = FIRE_DELAY / MAX_DELAY;

            double z = Math.Cos((double)weaponOscilation) / 2;
            double y = Math.Sin(2 * (double)weaponOscilation) / 16;

            Matrix weaponOffset = Matrix.Translation(WEAPON_OFFSET + new Vector3(0, 0, 4 * -weaponRecoil) + new Vector3(0, (float)y, (float)z));
            Matrix weaponScale = Matrix.Scaling(0.5f, 0.5f, 0.5f);
            Matrix weaponRotationY = Matrix.RotationY(WEAPON_ORIENTATION_Y);

            return weaponScale * weaponRotationY * weaponOffset * fpsMatrixInv;
        }

        public virtual void fireWeapon() 
        {
            GameManager.Instance.player1.playSound(weaponSound, weaponSoundDir, false);

            //int cantExplosion = 10;
            //float particleTime = 4f, sizeMax = 100f, expSpeed = 1f, expSizeSpeed = 2f; ;
            //float expUpdateTime = 0.01f;

            int cantExplosion = 20;
            float particleTime = 1f, sizeMax = 2000f, expSpeed = 1f, expSizeSpeed = 20f; ;
            float expUpdateTime = 0;
            Matrix matrix = mesh.Transform;
            Matrix offset = Matrix.Translation(0, 0, 50);
            matrix = offset * matrix;
            Vector3 posicion = new Vector3(matrix.M41, matrix.M42, matrix.M43);

            //Creo el emisor de explosion
            //muzzle = new ExplosionEmitter(cantExplosion, posicion, new Vector3(expSpeed, expSpeed, expSpeed), new Vector3(0.00f, 0.00f, 0.00f), 50f, sizeMax, particleTime, Color.White, 150, 0f, expUpdateTime, GameManager.Instance.random.Next(0, 1000), expSizeSpeed, 2);
            muzzle = new ExplosionEmitter(cantExplosion, posicion, new Vector3(expSpeed, expSpeed, expSpeed), new Vector3(0.00f, 0.00f, 0.00f), 500f, sizeMax, particleTime, Color.White, 150, 0f, expUpdateTime, GameManager.Instance.random.Next(0, 1000), expSizeSpeed, 2);
            muzzle.Init();
        }
    }
}
