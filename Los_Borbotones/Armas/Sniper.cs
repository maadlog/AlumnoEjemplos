using AlumnoEjemplo.Los_Borbotones;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TgcViewer;
using TgcViewer.Utils.TgcSceneLoader;

namespace AlumnoEjemplos.Los_Borbotones
{
    class Sniper:Weapon
    {
        public override void Init()
        {
            //Carga del mesh del arma
            TgcSceneLoader loader = new TgcSceneLoader();
            TgcScene scene = loader.loadSceneFromFile(GuiController.Instance.AlumnoEjemplosMediaDir + "Los_Borbotones\\Meshes\\svd\\svd-TgcScene.xml");
            mesh = scene.Meshes[0];
            weaponSoundDir = GuiController.Instance.AlumnoEjemplosMediaDir + "Los_Borbotones\\Audio/Armas/Sniper.wav";
            base.Init();
        }

        public override void fireWeapon()
        {
            GameManager.Instance.fireSniper();
            base.fireWeapon();
        }
    }
}
