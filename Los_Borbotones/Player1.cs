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
using TgcViewer.Utils.Particles;
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
        TgcStaticSound weaponSound = new TgcStaticSound();
        TgcStaticSound playerSound = new TgcStaticSound();
        TgcStaticSound footstepSound = new TgcStaticSound();
        bool running;
        string weaponSoundDir = GuiController.Instance.AlumnoEjemplosMediaDir + "Audio/Armas/Sniper.wav";
        string breathingSoundDir = GuiController.Instance.AlumnoEjemplosMediaDir + "Audio/Player/Breathing.wav";
        string hitSoundDir = GuiController.Instance.AlumnoEjemplosMediaDir + "Audio/Player/Hit.wav";
        string walkSoundDir = GuiController.Instance.AlumnoEjemplosMediaDir + "Audio/Player/Walk.wav";
        string runSoundDir = GuiController.Instance.AlumnoEjemplosMediaDir + "Audio/Player/Run.wav";
        Vector3 prevEye;
        public int vida;
        double intensidadMaximaEscalable = Math.Pow(0.7, 2);
        float sprintTime;
        float tiredTime;
        float MAX_SPRINT_TIME = 5;
        float TIRED_TIME = 4.5f;
        float weaponOscilation = 0;
        
        float ZOOM_DELAY = 0;
        float MAX_ZOOM_DELAY = 0.2f;

        public TgcMesh meshAuxiliarParaSonido;

        public override void Init()
        {
            vida = 100;
            sprintTime = 0;
            tiredTime = 0;
            running = false;
            weaponOscilation = 0;


            //Carga del mesh del arma
            TgcSceneLoader loader = new TgcSceneLoader();
            TgcScene scene = loader.loadSceneFromFile(GuiController.Instance.AlumnoEjemplosMediaDir + "Meshes\\svd\\svd-TgcScene.xml");
            mesh = scene.Meshes[0];

            //Mesh auxiliar para el sonido
            TgcScene scene2 = loader.loadSceneFromFile(GuiController.Instance.AlumnoEjemplosMediaDir + "Meshes\\svd\\svd-TgcScene.xml");
            meshAuxiliarParaSonido = scene2.Meshes[0];

            //hago que los 3dSound sigan al arma
            GuiController.Instance.DirectSound.ListenerTracking = meshAuxiliarParaSonido;
            ///////////////CONFIGURAR CAMARA PRIMERA PERSONA CUSTOM//////////////////
            //Camara en primera persona, tipo videojuego FPS
            //Solo puede haber una camara habilitada a la vez. Al habilitar la camara FPS se deshabilita la camara rotacional
            //Por default la camara FPS viene desactivada
            GuiController.Instance.RotCamera.Enable = false;
            CustomFpsCamera.Instance.Enable = true;
            GuiController.Instance.CurrentCamera =  CustomFpsCamera.Instance;
            //Configurar posicion y hacia donde se mira
            CustomFpsCamera.Instance.setCamera(new Vector3(0, 930, 0), new Vector3(-1000, 930, 0));

            //Permitir matrices custom
            mesh.AutoTransformEnable = false;

            prevEye = CustomFpsCamera.Instance.eye;
            //cargar sonido
            playerSound.loadSound(breathingSoundDir);
            //reproducir sonido de respawn
            playSound(footstepSound, walkSoundDir, true);
        }

        public override void Update(float elapsedTime)
        {
            WEAPON_OFFSET = (Vector3)GuiController.Instance.Modifiers["weaponOffset"];
            WEAPON_ORIENTATION_Y = (float)GuiController.Instance.Modifiers["weaponRotation"];
            //update de la pos del mesh auxiliar
            meshAuxiliarParaSonido.Position = CustomFpsCamera.Instance.eye;
            //Procesamos input de teclado
            TgcD3dInput input = GuiController.Instance.D3dInput;
            //Seteamos las teclas
            if (GuiController.Instance.D3dInput.buttonDown(TgcD3dInput.MouseButtons.BUTTON_LEFT) && FIRE_DELAY <= 0)
            {
                FIRE_DELAY = MAX_DELAY;
                GameManager.Instance.fireWeapon();
                CustomFpsCamera.Instance.rotateSmoothly(-0.30f, -1.5f, 0);
                playSound(weaponSound,weaponSoundDir, false);
            }

            if (FIRE_DELAY > 0) { FIRE_DELAY -= elapsedTime; }


            if (GuiController.Instance.D3dInput.buttonPressed(TgcD3dInput.MouseButtons.BUTTON_RIGHT) && ZOOM_DELAY <= 0.5f)
            {
                ZOOM_DELAY = MAX_ZOOM_DELAY;
                GameManager.Instance.zoomCamera();
            }

            if (GuiController.Instance.D3dInput.keyDown(Key.LeftShift) && sprintTime < MAX_SPRINT_TIME)
            {
                CustomFpsCamera.Instance.MovementSpeed = 300f;
                sprintTime += elapsedTime;
                if (!running)
                {
                    playSound(footstepSound, runSoundDir, true);
                    running = true;
                }
                if (sprintTime > MAX_SPRINT_TIME) 
                {
                    playSound(playerSound, breathingSoundDir, false);
                    tiredTime = 0; 
                }
            }
            else { 
                CustomFpsCamera.Instance.MovementSpeed = CustomFpsCamera.DEFAULT_MOVEMENT_SPEED;
                tiredTime += elapsedTime;
                if (running)
                {
                    playSound(footstepSound, walkSoundDir, true);
                    running = false;
                }
                if (tiredTime > TIRED_TIME && sprintTime != 0) 
                {
                    playerSound.stop();
                    sprintTime = 0; 
                }
            }

            if (ZOOM_DELAY > 0) { ZOOM_DELAY -= elapsedTime; }

            //muzzleFlash.Position = WEAPON_OFFSET;
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

            if (prevEye != CustomFpsCamera.Instance.eye)
            { footstepSound.play(true); }
            else { footstepSound.stop(); }

            float length = ((CustomFpsCamera.Instance.eye - prevEye).Length());
            if (Vector3.Dot(CustomFpsCamera.Instance.LookAt, CustomFpsCamera.Instance.eye - prevEye) > 0)
            {
                length *= -1;
            }
            weaponOscilation += length/37;

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

            double z = Math.Cos((double)weaponOscilation)/2;
            double y = Math.Sin(2 * (double)weaponOscilation) / 16;

            Matrix weaponOffset = Matrix.Translation(WEAPON_OFFSET + new Vector3(0, 0, 4 * -weaponRecoil) + new Vector3(0, (float)y, (float)z));
            Matrix weaponScale = Matrix.Scaling(0.5f, 0.5f, 0.5f);
            Matrix weaponRotationY = Matrix.RotationY(WEAPON_ORIENTATION_Y);

            return weaponScale * weaponRotationY * weaponOffset * fpsMatrixInv;
        }

        public void recibirAtaque(int damage, float elapsedTime)
        {
            
            playSound(playerSound, hitSoundDir, false);
            FIRE_DELAY = 0.5f;
            vida -= damage;
            GameManager.Instance.healthText.Text = "HEALTH: " + vida;
            GameManager.Instance.ChangeColorHealth();
            if(vida <= 0)
            {
                GameManager.Instance.gameOver(elapsedTime);
            }
        }

        private void playSound(TgcStaticSound sound, string dir, bool loop)
        {
            //reproducir un sonido
            sound.dispose();
            sound.loadSound(dir, GameManager.Instance.PLAYER_VOLUME);
            sound.play(loop);
        }
    }
}
