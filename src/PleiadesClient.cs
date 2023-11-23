using System.Numerics;

namespace Augmenta
{
    public class PleiadesClient
    {
        public Dictionary<int, PObject> objects = new Dictionary<int, PObject>();
        Matrix4x4 transform;

        //Call once per frame
        public void Update(float time, Matrix4x4 transform)
        {
            this.transform = transform;

            var objectsToRemove = new List<int>();
            foreach (var o in objects.Values)
            {
                o.Update(time);
                if (o.timeSinceGhost > 1) 
                    objectsToRemove.Add(o.objectID);
            }

            foreach (var oid in objectsToRemove)
            {
                var o = objects[oid];
                objects.Remove(oid);
                o.kill();
            }
        }

        public void processData(float time, Span<byte> data, int offset = 0)
        {
            var type = data[offset];

            if (type == 255) //bundle
            {
                var pos = offset + 1; //offset + sizeof(packettype)
                while (pos < data.Length - 5) //-sizeof(packettype) - sizeof(packetsize)
                {
                    var packetSize = Utils.ReadInt(data, pos + 1);  //pos + sizeof(packettype)
                    processData(time, data, pos);
                    pos += packetSize;
                }
            }

            //Debug.Log("Packet type : " + type);

            switch (type)
            {
                case 0: //Object
                    {
                        processObject(time, data, offset);
                    }
                    break;

                case 1: //Zone
                    {
                        processZone(time, data, offset);
                    }
                    break;
            }
        }

        private void processObject(float time, Span<byte> data, int offset)
        {
            var objectID = Utils.ReadInt(data, offset + 1 + sizeof(int)); //offset + sizeof(packettype) + sizeof(packetsize)

            PObject o = null;
            if (objects.ContainsKey(objectID)) o = objects[objectID];
            if (o == null)
            {
                o = new PObject();
                o.objectID = objectID;
                o.onRemove += onObjectRemove;
                objects.Add(objectID, o);
            }

            o.parentTransform = transform;
            o.updateData(time, data, offset);
        }

        void processZone(float time, Span<byte> data, int offset)
        {

        }

        //events
        void onObjectRemove(PObject o)
        {
            objects.Remove(o.objectID);
            o.kill();
        }
    }
}