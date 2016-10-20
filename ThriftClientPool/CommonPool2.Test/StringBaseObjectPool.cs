using System.Collections.Concurrent;
using System.Text;

namespace CommonPool2.Test
{
    public class StringBaseObjectPool:BaseObjectPool<StringBuilder>
    {
        private ConcurrentQueue<StringBuilder> _builderDictionary=new ConcurrentQueue<StringBuilder>(); 
        public override StringBuilder BorrowObject()
        {
            throw new System.NotImplementedException();
        }

        public override void ReturnObject(StringBuilder obj)
        {
            throw new System.NotImplementedException();
        }

        public override void InvalidateObject(StringBuilder obj)
        {
            throw new System.NotImplementedException();
        }

        public override void AddObject()
        {
            throw new System.NotImplementedException();
        }

        public override void Clear()
        {
            throw new System.NotImplementedException();
        }
    }
}