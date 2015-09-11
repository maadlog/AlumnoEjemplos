using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;
using Microsoft.DirectX.DirectInput;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using TgcViewer;
using TgcViewer.Utils._2D;
using TgcViewer.Utils.Input;
using TgcViewer.Utils.Sound;
using TgcViewer.Utils.TgcGeometry;
using TgcViewer.Utils.TgcSceneLoader;

namespace AlumnoEjemplos.Los_Borbotones
{
    class Player1:GameObject
    {
        float WEAPON_ORIENTATION_Y;
        Vector3 WEAPON_OFFSET;
        float HITSCAN_DELAY;
        float FIRE_DELAY = 0;
        float MAX_DELAY = 2;
        TgcStaticSound sound = new TgcStaticSound();
        string weaponSoundDir = GuiController.Instance.AlumnoEjemplosMediaDir + "Audio/Armas/Sniper.wav";
        float ZOOM_CONST = 0.8f; //TODO Hacer dependiente del arma
        bool zoomEnabled = false;

        public override void Init()
        {
            //Carga del mesh del arma
            TgcSceneLoader loader = new TgcSceneLoader();
            TgcScene scene = loader.loadSceneFromFile(GuiController.Instance.AlumnoEjemplosMediaDir + "Meshes\\svd\\svd-TgcScene.xml");
            mesh = scene.Meshes[0];

            ///////////////CONFIGURAR CAMARA PRIMERA PERSONA CUSTOM//////////////////
            //Camara en primera persona, tipo videojuego FPS
            //Solo puede haber una camara habilitada a la vez. Al habilitar la camara FPS se deshabilita la camara rotacional
            //Por default la camara FPS viene desactivada
            CustomFpsCamera.Instance.Enable = true;
            GuiController.Instance.CurrentCamera =  CustomFpsCamera.Instance;
            //Configurar posicion y hacia donde se mira
            CustomFpsCamera.Instance.setCamera(new Vector3(0, 20, 0), new Vector3(0, 0, -1000));

            //Permitir matrices custom
            mesh.AutoTransformEnable = false;

        }

        public override void Update(float elapsedTime)
        {
            WEAPON_OFFSET = (Vector3)GuiController.Instance.Modifiers["weaponOffset"];
            WEAPON_ORIENTATION_Y = (float)GuiController.Instance.Modifiers["weaponRotation"];

            //Procesamos input de teclado
            TgcD3dInput input = GuiController.Instance.D3dInput;
            if (input.keyDown(Key.E) && FIRE_DELAY <= 0)
            {
                FIRE_DELAY = MAX_DELAY;
                GameManager.Instance.fireWeapon();
                CustomFpsCamera.Instance.rotateSmoothly(-0.30f, -1.5f, 0);
                playSound(weaponSoundDir);
            }

            if (FIRE_DELAY > 0) { FIRE_DELAY -= elapsedTime; }

            if (input.keyPressed(Key.LeftShift))
            {
                zoomCamera();
            }

            mesh.Transform = getWeaponTransform(); 
        
        }

        public void zoomCamera()
        {
            if (zoomEnabled)
            {
                CustomFpsCamera.Instance.Zoom = 0;
                zoomEnabled = false;
            } else {
                CustomFpsCamera.Instance.Zoom = ZOOM_CONST;
                zoomEnabled = true;
            }

        }

        public override void Render(float elapsedTime)
        {
            mesh.render();

            GuiController.Instance.D3dDevice.Transform.World = Matrix.Identity;
        }

        public Matrix getWeaponTransform()
        {
            Matrix fpsMatrixInv = Matrix.Invert(CustomFpsCamera.Instance.ViewMatrix);

            float weaponRecoil = FIRE_DELAY/MAX_DELAY;

            Matrix weaponOffset = Matrix.Translation(WEAPON_OFFSET + new Vector3(0,0, 4* -weaponRecoil));
            Matrix weaponScale = Matrix.Scaling(0.5f, 0.5f, 0.5f);
            Matrix weaponRotationY = Matrix.RotationY(WEAPON_ORIENTATION_Y);

            return weaponScale * weaponRotationY * weaponOffset * fpsMatrixInv;
        }

        private void playSound(string dir)
        {
            sound.dispose();
            sound.loadSound(dir);
            sound.play();
        }
    }
}
