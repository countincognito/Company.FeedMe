using System;

namespace Company.Api.Rest.Data
{
    [Serializable]
    public class UserRequestDto
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public bool Silent { get; set; }
    }
}
