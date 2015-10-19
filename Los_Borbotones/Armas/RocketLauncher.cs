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
        public override void Init()
        {
            //Carga del mesh del arma
            TgcSceneLoader loader = new TgcSceneLoader();
            TgcScene scene = loader.loadSceneFromFile(GuiController.Instance.AlumnoEjemplosMediaDir + "Los_Borbotones\\Meshes\\launcher\\MissilePod-TgcScene.xml");
            mesh = scene.Meshes[0];

            base.Init();
        }

        public override void fireWeapon()
        {
            GameManager.Instance.fireLauncher();
        }
    }
}
