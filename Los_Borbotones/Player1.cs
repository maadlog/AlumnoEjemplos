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
    public class Player1:GameObject
    {
        float WEAPON_ORIENTATION_Y;
        Vector3 WEAPON_OFFSET;
        float HITSCAN_DELAY;
        float FIRE_DELAY = 0;
        float MAX_DELAY = 2;
        TgcStaticSound sound = new TgcStaticSound();
        string weaponSoundDir = GuiController.Instance.AlumnoEjemplosMediaDir + "Audio/Armas/Sniper.wav";
        Vector3 prevEye;
        public int vida;
        double intensidadMaximaEscalable = Math.Pow(0.7, 2);
        float sprintTime;
        float tiredTime;
        float MAX_SPRINT_TIME = 10;
        float TIRED_TIME = 5;
        
        float ZOOM_DELAY = 0;
        float MAX_ZOOM_DELAY = 0.2f;

        public override void Init()
        {
            vida = 100;
            sprintTime = 0;
            tiredTime = 0;

            //Carga del mesh del arma
            TgcSceneLoader loader = new TgcSceneLoader();
            TgcScene scene = loader.loadSceneFromFile(GuiController.Instance.AlumnoEjemplosMediaDir + "Meshes\\svd\\svd-TgcScene.xml");
            mesh = scene.Meshes[0];

            //hago que los 3dSound sigan al arma
            GuiController.Instance.DirectSound.ListenerTracking = mesh;
            ///////////////CONFIGURAR CAMARA PRIMERA PERSONA CUSTOM//////////////////
            //Camara en primera persona, tipo videojuego FPS
            //Solo puede haber una camara habilitada a la vez. Al habilitar la camara FPS se deshabilita la camara rotacional
            //Por default la camara FPS viene desactivada
            CustomFpsCamera.Instance.Enable = true;
            GuiController.Instance.CurrentCamera =  CustomFpsCamera.Instance;
            //Configurar posicion y hacia donde se mira
            CustomFpsCamera.Instance.setCamera(new Vector3(-20, 930, -20), new Vector3(0, 930, -1000));

            //Permitir matrices custom
            mesh.AutoTransformEnable = false;

            prevEye = CustomFpsCamera.Instance.eye;
        }

        public override void Update(float elapsedTime)
        {
            WEAPON_OFFSET = (Vector3)GuiController.Instance.Modifiers["weaponOffset"];
            WEAPON_ORIENTATION_Y = (float)GuiController.Instance.Modifiers["weaponRotation"];

            //Procesamos input de teclado
            TgcD3dInput input = GuiController.Instance.D3dInput;
            if (GuiController.Instance.D3dInput.buttonDown(TgcD3dInput.MouseButtons.BUTTON_LEFT) && FIRE_DELAY <= 0)
            {
                FIRE_DELAY = MAX_DELAY;
                GameManager.Instance.fireWeapon();
                CustomFpsCamera.Instance.rotateSmoothly(-0.30f, -1.5f, 0);
                playSound(weaponSoundDir);
            }

            if (FIRE_DELAY > 0) { FIRE_DELAY -= elapsedTime; }


            if (GuiController.Instance.D3dInput.buttonDown(TgcD3dInput.MouseButtons.BUTTON_RIGHT) && ZOOM_DELAY <= 0)
            {
                ZOOM_DELAY = MAX_ZOOM_DELAY;
                GameManager.Instance.zoomCamera();
            }

            if (GuiController.Instance.D3dInput.keyDown(Key.LeftShift) && sprintTime < MAX_SPRINT_TIME)
            {
                CustomFpsCamera.Instance.MovementSpeed = 300f;
                sprintTime += elapsedTime;
                if (sprintTime > MAX_SPRINT_TIME) { tiredTime = 0; }
            }
            else { 
                CustomFpsCamera.Instance.MovementSpeed = CustomFpsCamera.DEFAULT_MOVEMENT_SPEED;
                tiredTime += elapsedTime;
                if (tiredTime > TIRED_TIME) { sprintTime = 0; }
            }

            if (ZOOM_DELAY > 0) { ZOOM_DELAY -= elapsedTime; }

            mesh.Transform = getWeaponTransform();

            //Maxima inclinacion sobre terreno
            float yActual;
            float yAnterior;
            float movspeed = CustomFpsCamera.Instance.MovementSpeed;
            GameManager.Instance.interpoledHeight(CustomFpsCamera.Instance.eye.X, CustomFpsCamera.Instance.eye.Z, out yActual);
            GameManager.Instance.interpoledHeight(prevEye.X, prevEye.Z, out yAnterior);
            double diferenciaPotenciada = Math.Pow((yActual - yAnterior) / (movspeed * elapsedTime), 2);

            if ( diferenciaPotenciada >= intensidadMaximaEscalable)
            {
                CustomFpsCamera.Instance.eye = prevEye;
                CustomFpsCamera.Instance.reconstructViewMatrix(false);
            }


            //Colision de la camara con sliding
            foreach (TgcMesh obstaculo in GameManager.Instance.Vegetation.Meshes)
            {
                Vector3 dirMov = CustomFpsCamera.Instance.eye - prevEye;

                TgcCollisionUtils.BoxBoxResult result = TgcCollisionUtils.classifyBoxBox(CustomFpsCamera.Instance.boundingBox, obstaculo.BoundingBox);
                if (result == TgcCollisionUtils.BoxBoxResult.Adentro || result == TgcCollisionUtils.BoxBoxResult.Atravesando)
                {
                    CustomFpsCamera.Instance.eye = prevEye + new Vector3(dirMov.X, 0, 0);
                    CustomFpsCamera.Instance.reconstructViewMatrix(false);
                    TgcCollisionUtils.BoxBoxResult resultX = TgcCollisionUtils.classifyBoxBox(CustomFpsCamera.Instance.boundingBox, obstaculo.BoundingBox);
                    if (resultX == TgcCollisionUtils.BoxBoxResult.Adentro || resultX == TgcCollisionUtils.BoxBoxResult.Atravesando)
                    {
                        CustomFpsCamera.Instance.eye = prevEye + new Vector3(0, 0, dirMov.Z);
                        CustomFpsCamera.Instance.reconstructViewMatrix(false);

                        TgcCollisionUtils.BoxBoxResult resultZ = TgcCollisionUtils.classifyBoxBox(CustomFpsCamera.Instance.boundingBox, obstaculo.BoundingBox);
                        if (resultZ == TgcCollisionUtils.BoxBoxResult.Adentro || resultZ == TgcCollisionUtils.BoxBoxResult.Atravesando)
                        {
                            CustomFpsCamera.Instance.eye = prevEye;
                        }
                    }
                    break;
                }
            }

            prevEye = CustomFpsCamera.Instance.eye;
        }

        public override void Render(float elapsedTime)
        {
            mesh.render();
            
           // GuiController.Instance.D3dDevice.Transform.World = Matrix.Identity;
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

        public void recibirAtaque(int damage, float elapsedTime)
        {
            vida -= damage;
            GameManager.Instance.healthText.Text = "HEALTH: " + vida;
            GameManager.Instance.ChangeColorHealth();
            if(vida <= 0)
            {
                GameManager.Instance.gameOver(elapsedTime);
            }
        }

        private void playSound(string dir)
        {
            sound.dispose();
            sound.loadSound(dir);
            sound.play();
        }
    }
}
