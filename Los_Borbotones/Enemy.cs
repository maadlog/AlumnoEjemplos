using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AlumnoEjemplos.Los_Borbotones
{
    abstract class Enemy : GameObject
    {
        public float MOVEMENT_SPEED;
        public float SPAWN_RADIUS;
        public float ANGLE;

        public override void Init()
        {
            throw new NotImplementedException();
        }

        public override void Update(float elapsedTime)
        {
            throw new NotImplementedException();
        }

        public override void Render(float elapsedTime)
        {
            this.mesh.render();
            this.mesh.BoundingBox.render();
        }
    }
}
