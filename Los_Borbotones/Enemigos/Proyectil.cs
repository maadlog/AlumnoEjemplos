using AlumnoEjemplo.Los_Borbotones;
using Microsoft.DirectX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TgcViewer;
using TgcViewer.Utils.TgcSceneLoader;

namespace AlumnoEjemplos.Los_Borbotones
{
    public class Proyectil:GameObject
    {
        public Matrix shooterMatrix;
        float MOVEMENT_SPEED = 1000f;
        public Vector3 vectorDireccion;
        Matrix posActual;

        public Proyectil(Matrix shooterMatrix, Vector3 vectorDireccion)
        {
            this.shooterMatrix = shooterMatrix;
            this.vectorDireccion = vectorDireccion;
        }

        public override void Init()
        {
            TgcSceneLoader loader = new TgcSceneLoader();
            TgcScene scene = loader.loadSceneFromFile(GuiController.Instance.AlumnoEjemplosMediaDir + "Los_Borbotones\\Meshes\\proyectiles\\EnergyBall-TgcScene.xml");
            mesh = scene.Meshes[0];

            mesh.AutoTransformEnable = false;
            Matrix offY = Matrix.Translation(15, 30, 20);
            Matrix offXZ = Matrix.Translation(vectorDireccion * 0.15f);
            Matrix scale = Matrix.Scaling(new Vector3(0.07f, 0.07f, 0.07f));
            Matrix pos = Matrix.Translation(CustomFpsCamera.Instance.Position);
            posActual = scale *offY * offXZ * shooterMatrix;
            mesh.Transform = posActual;
            Vector3 vectorPosActual = new Vector3(posActual.M41, posActual.M42, posActual.M43);
            vectorDireccion = (vectorPosActual -CustomFpsCamera.Instance.Position);
            vectorDireccion.Normalize();
        }

        public override void Update(float elapsedTime)
        {
            Matrix translate = Matrix.Translation(-vectorDireccion * MOVEMENT_SPEED * elapsedTime);
            //Matrix translate = Matrix.Translation(new Vector3(1,0,1) * MOVEMENT_SPEED * elapsedTime);
            posActual = translate * posActual;
            mesh.Transform = posActual;
        }

        public override void Render(float elapsedTime)
        {
            mesh.render();
            if (GameManager.Instance.drawBoundingBoxes)
            {
                mesh.BoundingBox.render();
            }
        }

        public override void dispose()
        {
            base.dispose();
        }
    }
}
