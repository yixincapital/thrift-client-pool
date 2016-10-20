using System.Text;
using CommonPool2.impl;

namespace CommonPool2.Test
{
    public class StringBufferFactory:BasePooledObjectFactory<StringBuilder>
    {
        public override StringBuilder Create()
        {
           return new StringBuilder();
        }

        public override IPooledObject<StringBuilder> Wrap(StringBuilder obj)
        {
            return new DefaultPooledObject<StringBuilder>(obj);
        }

        /// <summary>
        /// Clear the stringbuilder
        /// </summary>
        /// <param name="pooledObject"></param>
        public override void PassivateObject(IPooledObject<StringBuilder> pooledObject)
        {
            pooledObject.GetObject().Clear();
        }
    }
}