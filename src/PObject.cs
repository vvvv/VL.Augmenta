using System.Numerics;
using System.Runtime.CompilerServices;

namespace Augmenta
{
    public class PObject
    {
        public int objectID;
        private Vector3[] pointsA = new Vector3[0];
        private int pointCount;
        public ReadOnlyMemory<Vector3> points => new ReadOnlyMemory<Vector3>(pointsA, 0, pointCount);

        public Matrix4x4 transform;
        internal Matrix4x4 parentTransform = Matrix4x4.Identity;

        //cluster
        public enum State { Enter = 0, Update = 1, Leave = 2, Ghost = 3 };
        public State state;

        public Vector3 centroid;
        public Vector3 velocity;
        public Vector3 minBounds;
        public Vector3 maxBounds;
        //[Range(0, 1)]
        public float weight;

        float lastUpdateTime;

        public float killDelayTime = 0;

        public float timeSinceGhost;
        public bool drawDebug;

        public enum PositionUpdateMode { None, Centroid, BoxCenter }
        public PositionUpdateMode posUpdateMode = PositionUpdateMode.Centroid;

        public enum CoordMode { Absolute, Relative }
        public CoordMode pointMode = CoordMode.Relative;

        public delegate void OnRemoveEvent(PObject obj);
        public event OnRemoveEvent onRemove;


        // Update is called once per frame
        internal void Update(float time)
        {
            if (time - lastUpdateTime > .5f) 
                timeSinceGhost = time;
            else 
                timeSinceGhost = -1;
        }

        public void updateData(float time, Span<byte> data, int offset)
        {
            var pos = offset + 1 + 2 * sizeof(int); //packet type (1) + packet size (4) + objectID (4)
            while (pos < data.Length)
            {
                var propertyID = Utils.ReadInt(data, pos);
                var propertySize = Utils.ReadInt(data, pos + sizeof(int));

                if (propertySize < 0)
                {
                    //Debug.LogWarning("Error : property size < 0");
                    break;
                }

                switch (propertyID)
                {
                    case 0: updatePointsData(data, pos + 2 * sizeof(int)); break;
                    case 1: updateClusterData(data, pos + 2 * sizeof(int)); break;
                }

                pos += propertySize;
            }

            lastUpdateTime = time;
        }

        void updatePointsData(Span<byte> data, int offset)
        {
            pointCount = Utils.ReadInt(data, offset);
            var vectors = Utils.ReadVectors(data, offset + sizeof(int), pointCount * Unsafe.SizeOf<Vector3>());

            if (pointsA.Length < pointCount)
                pointsA = new Vector3[(int)(pointCount * 1.5)];

            if (pointMode == CoordMode.Absolute)
                vectors.CopyTo(pointsA.AsSpan());
            else
            {
                for (int i = 0; i < vectors.Length; i++)
                    pointsA[i] = Vector3.Transform(vectors[i], parentTransform);
            }
        }

        void updateClusterData(Span<byte> data, int offset)
        {
            state = (State)Utils.ReadInt(data, offset);
            if (state == State.Leave) //Will leave
            {
                onRemove(this);
                return;
            }

            var clusterData = new Vector3[4];
            for (int i = 0; i < 4; i++)
            {
                var si = offset + sizeof(int) + i * Unsafe.SizeOf<Vector3>();

                var p = Utils.ReadVector(data, si);
                if (pointMode == CoordMode.Absolute) 
                    clusterData[i] = clusterData[i] = p; 
                else
                    clusterData[i] = Vector3.Transform(p, parentTransform);//  parentTransform.InverseTransformPoint(p);
            }

            centroid = clusterData[0];
            velocity = clusterData[1];
            minBounds = clusterData[2];
            maxBounds = clusterData[3];
            weight = Utils.ReadFloat(data, offset + 4 + 4 * Unsafe.SizeOf<Vector3>());

            switch (posUpdateMode)
            {
                case PositionUpdateMode.None:
                    break;
                case PositionUpdateMode.Centroid:
                    transform.Translation = centroid;
                    break;
                case PositionUpdateMode.BoxCenter:
                    transform.Translation = (minBounds + maxBounds) / 2;
                    break;
            }
        }

        public void kill()
        {
            if (killDelayTime == 0)
            {
                //Destroy(gameObject);
                return;
            }

            //points = new Vector3[0];
            //StartCoroutine(killForReal(killDelayTime));
        }

        //IEnumerator killForReal(float timeBeforeKill)
        //{
        //    yield return new WaitForSeconds(timeBeforeKill);
        //    Destroy(gameObject);
        //}

        //void OnDrawGizmos() 
        //{ 
        //    if (drawDebug)
        //    {
        //        Color c = Color.HSVToRGB((objectID * .1f) % 1, 1, 1); //Color.red;// getColor();
        //        if (state == State.Ghost) c = Color.gray / 2;

        //        Gizmos.color = c;
        //        foreach (var p in points) Gizmos.DrawLine(p, p + Vector3.forward * .01f);

        //        Gizmos.color = c + Color.white * .3f;
        //        Gizmos.DrawWireSphere(centroid, .03f);
        //        Gizmos.DrawWireCube((minBounds + maxBounds) / 2, maxBounds - minBounds);
        //    }
        //}
    }
}