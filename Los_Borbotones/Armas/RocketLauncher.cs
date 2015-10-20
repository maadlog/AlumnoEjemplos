using Microsoft.DirectX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TgcViewer;
using TgcViewer.Utils.TgcSceneLoader;

namespace AlumnoEjemplos.Los_Borbotones
{
    class RocketLauncher:Weapon
    {
        float hitDelay;
        float DELAY_FACTOR = 5000;
        Vector3 target;
        bool fired;
        Explosion explosion;

        public override void Init()
        {
            fired = false;

            //Carga del mesh del arma
            TgcSceneLoader loader = new TgcSceneLoader();
            TgcScene scene = loader.loadSceneFromFile(GuiController.Instance.AlumnoEjemplosMediaDir + "Los_Borbotones\\Meshes\\launcher\\MissilePod-TgcScene.xml");
            mesh = scene.Meshes[0];
            weaponSoundDir = GuiController.Instance.AlumnoEjemplosMediaDir + "Los_Borbotones\\Audio/Armas/Launcher.wav";
            base.Init();

            MAX_DELAY = 4;
        }

        public override void fireWeapon()
        {
            target = GameManager.Instance.fireLauncher();
            fired = true;
            base.fireWeapon();
            hitDelay = GameManager.Instance.distanciaACamara(target) / DELAY_FACTOR;
        }

        public override void Update(float elapsedTime)
        {
            hitDelay -= elapsedTime;
            if (hitDelay <= 0 && fired)
            {
                explosion = new Explosion();
                explosion.posicion = target;
                explosion.init();
                fired = false;
            }
            base.Update(elapsedTime);
        }
    }
}
