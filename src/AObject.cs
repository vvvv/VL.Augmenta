using Stride.Core.Mathematics;
using System.Runtime.CompilerServices;

namespace Augmenta
{
    public class AObject: GenericObject<Vector3>
    {
        public AObject()
            : base() { }


        protected override void UpdateTransform()
        {
            //throw new NotImplementedException();
        }
    }
}