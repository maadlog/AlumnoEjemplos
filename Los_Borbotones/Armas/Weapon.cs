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
        internal float WEAPON_ORIENTATION_Y;
        internal Vector3 WEAPON_OFFSET;
        public float MAX_DELAY;
        public float FIRE_DELAY;
        float weaponOscilation;
        public string weaponSoundDir;
        TgcStaticSound weaponSound;
        public MuzzleFlash muzzle;
        public Vector3 scaleMuzzle;

        public override void Init()
        {
            weaponOscilation = 0;
            FIRE_DELAY = 0;
            MAX_DELAY = 2;
            weaponSound = new TgcStaticSound();
            muzzle = new MuzzleFlash();
            muzzle.crearMuzzle();

            //Permitir matrices custom
            mesh.AutoTransformEnable = false;
        }

        public override void Update(float elapsedTime)
        {
            mesh.Transform = getWeaponTransform();
            
            float length = ((CustomFpsCamera.Instance.eye - GameManager.Instance.player1.prevEye).Length());
            if (CustomFpsCamera.Instance.moveBackwardsPressed)
            {
                length *= -1;
            }
            weaponOscilation += length / 37;

            muzzle.actualizarFlash();
        }

        public override void Render(float elapsedTime)
        {
            if(muzzle.TIME_RENDER > 0) muzzle.renderFlash();
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
        }
    }
}
