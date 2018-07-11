using System;

namespace Company.Common.Data
{
    [Serializable]
    public class UserDto
        : DtoBase<int>
    {
        public UserDto(int id, string name)
            : base(id)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
        }

        public string Name
        {
            get;
        }
    }
}
