using AlumnoEjemplos.Los_Borbotones;
using Microsoft.DirectX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TgcViewer;
using TgcViewer.Utils.TgcGeometry;
using TgcViewer.Utils.TgcSceneLoader;

namespace AlumnoEjemplos.Los_Borbotones
{
    public class Proyectil:GameObject
    {
        public Matrix shooterMatrix;
        float MOVEMENT_SPEED = 4000f;
        public Vector3 vectorDireccion;
        int damage = 25;
        float MAX_TIME = 10;
        float time;
        Matrix posActual;

        public Proyectil(Matrix shooterMatrix, Vector3 vectorDireccion)
        {
            this.shooterMatrix = shooterMatrix;
            this.vectorDireccion = vectorDireccion;
        }

        public override void Init()
        {
            this.mesh = GameManager.Instance.ModeloProyectil.clone("proyectil");
            time = 0;

            mesh.AutoTransformEnable = false;
            //Matrix offY = Matrix.Translation(15, 30, 20);
            //Matrix offXZ = Matrix.Translation(vectorDireccion * 0.15f);
            Matrix scale = Matrix.Scaling(new Vector3(0.07f, 0.07f, 0.07f));
            Matrix pos = Matrix.Translation(CustomFpsCamera.Instance.Position);
            posActual = scale * shooterMatrix;
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
            mesh.BoundingBox.transform(mesh.Transform);

            //Colision de enemigos con vegetacion, hecho para que no se queden trabados con o sin "ayuda" del player
            List<TgcMesh> obstaculos = new List<TgcMesh>();
            obstaculos = GameManager.Instance.quadTree.findMeshesToCollide(mesh.BoundingBox);

            foreach (TgcMesh obstaculo in obstaculos)
            {
                TgcCollisionUtils.BoxBoxResult result = TgcCollisionUtils.classifyBoxBox(mesh.BoundingBox, obstaculo.BoundingBox);
                if (result == TgcCollisionUtils.BoxBoxResult.Adentro || result == TgcCollisionUtils.BoxBoxResult.Atravesando)
                {
                    GameManager.Instance.eliminarProyectil(this);
                    return;
                }
            }

            //Colision con Player1
            TgcCollisionUtils.BoxBoxResult resultPlayer = TgcCollisionUtils.classifyBoxBox(mesh.BoundingBox, CustomFpsCamera.Instance.boundingBox);
            if (resultPlayer == TgcCollisionUtils.BoxBoxResult.Adentro || resultPlayer == TgcCollisionUtils.BoxBoxResult.Atravesando)
            {
                GameManager.Instance.player1.recibirAtaque(damage);
                GameManager.Instance.eliminarProyectil(this);
                return;
            }

            if (time > MAX_TIME)
            {
                GameManager.Instance.eliminarProyectil(this);
                return;
            }

            time += elapsedTime;
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
