using System;

namespace Company.Common.Data
{
    public class FeedMeEvent<TKey, T>
        where TKey : struct
        where T : DtoBase<TKey>
    {
        public FeedMeEvent(TKey id)
        {
            Type = typeof(T);
            Id = id;
        }

        public Type Type
        {
            get;
        }

        public TKey Id
        {
            get;
        }
    }
}
