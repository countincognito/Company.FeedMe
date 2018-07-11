using System;

namespace Company.Common.Data
{
    public class AddedEvent<TKey, T>
        where TKey : struct
        where T : DtoBase<TKey>
    {

        public AddedEvent(T dto)
        {
            Dto = dto ?? throw new ArgumentNullException(nameof(dto));
        }

        public T Dto
        {
            get;
        }
    }
}
