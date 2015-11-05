using AlumnoEjemplo.Los_Borbotones;
using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using TgcViewer;
using TgcViewer.Utils.Sound;
using TgcViewer.Utils.TgcGeometry;
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
        Effect currentShader;

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
            
            currentShader = GuiController.Instance.Shaders.TgcMeshPointLightShader;
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
            if (muzzle.TIME_RENDER > 0) { 
                muzzle.renderFlash();
                mesh.Effect = currentShader;
                //El Technique depende del tipo RenderType del mesh
                mesh.Technique = GuiController.Instance.Shaders.getTgcMeshTechnique(mesh.RenderType);

                mesh.Effect.SetValue("lightColor", ColorValue.FromColor(Color.White));
                mesh.Effect.SetValue("lightPosition", TgcParserUtils.vector3ToFloat4Array(muzzle.getPosicion()));
                mesh.Effect.SetValue("eyePosition", TgcParserUtils.vector3ToFloat4Array(CustomFpsCamera.Instance.getPosition()));
                mesh.Effect.SetValue("lightIntensity", 2000);
                mesh.Effect.SetValue("lightAttenuation", 0.3f);

                //Cargar variables de shader de Material. El Material en realidad deberia ser propio de cada mesh. Pero en este ejemplo se simplifica con uno comun para todos
                mesh.Effect.SetValue("materialEmissiveColor", ColorValue.FromColor(Color.Black));
                mesh.Effect.SetValue("materialAmbientColor", ColorValue.FromColor(Color.White));
                mesh.Effect.SetValue("materialDiffuseColor", ColorValue.FromColor(Color.White));
                mesh.Effect.SetValue("materialSpecularColor", ColorValue.FromColor(Color.Khaki));
                mesh.Effect.SetValue("materialSpecularExp", 9);
            }
            mesh.render();
            mesh.Effect = GuiController.Instance.Shaders.TgcMeshShader;
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
