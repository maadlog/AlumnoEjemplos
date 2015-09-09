using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TgcViewer;


namespace AlumnoEjemplos.Los_Borbotones
{
    abstract class Enemy : GameObject
    {
        public float MOVEMENT_SPEED = 10f;
        public float SPAWN_RADIUS;
        public float ANGLE;
        public Matrix posicionActual;
        public float Angulo = 0f;
        public Vector3 vectorDireccionAnterior = new Vector3(1, 0, 0);
        public Vector3 Normal;
        public override void Init()
            
        {
            throw new NotImplementedException();
        }

        public override void Update(float elapsedTime)
        {
       Device d3dDevice = GuiController.Instance.D3dDevice;
            Vector3 vectorDireccion;
            
            
            vectorDireccion = ( CustomFpsCamera.Instance.Position - new Vector3 (posicionActual.M41, posicionActual.M42, posicionActual.M43) );
            Vector3 vectorDireccionRotacion = new Vector3(vectorDireccion.X, 0, vectorDireccion.Z);
            vectorDireccionRotacion.Normalize();
            
            
            vectorDireccion.Normalize();
            
            //Matrix MatOrientarObjeto = Matrix.RotationAxis(Vector3.Cross(normal, vectorDireccion), (float)Math.Acos(Vector3.Dot(normal, vectorDireccion)));
            Angulo = (float)Math.Acos(Vector3.Dot(Normal, vectorDireccionRotacion));
            
            Matrix MatOrientarObjeto = Matrix.RotationY(Angulo);
            
            Matrix Traslacion = Matrix.Translation(vectorDireccion * MOVEMENT_SPEED * elapsedTime);
           
            this.mesh.Transform =  MatOrientarObjeto * posicionActual * Traslacion;
            this.mesh.BoundingBox.transform(MatOrientarObjeto * posicionActual * Traslacion);
            posicionActual = posicionActual * Traslacion;
            
        }

        public override void Render(float elapsedTime)
        {
            this.mesh.render();
            this.mesh.BoundingBox.render();
        }
    }
}
