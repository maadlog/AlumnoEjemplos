using Microsoft.DirectX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TgcViewer;
using TgcViewer.Utils.TgcSceneLoader;

namespace AlumnoEjemplos.Los_Borbotones
{
    public class RocketLauncher:Weapon
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
            TgcScene scene = loader.loadSceneFromFile(GuiController.Instance.AlumnoEjemplosMediaDir + "Los_Borbotones\\Meshes\\launcher\\RailGun-TgcScene.xml");
            mesh = scene.Meshes[0];
            weaponSoundDir = GuiController.Instance.AlumnoEjemplosMediaDir + "Los_Borbotones\\Audio/Armas/Launcher.wav";
            WEAPON_OFFSET = new Vector3(5f, -14.2f, -7f);
            WEAPON_ORIENTATION_Y = 0.05f;
            scaleMuzzle = new Vector3(0.5f, 0.5f, 0.5f);
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
                explosion.Explotar();
                fired = false;
            }
            base.Update(elapsedTime);
        }

        public override void Render(float elapsedTime)
        {
            if (explosion != null) { explosion.render(elapsedTime); }
            base.Render(elapsedTime);
        }
    }
}
