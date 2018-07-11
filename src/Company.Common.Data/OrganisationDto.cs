using System;

namespace Company.Common.Data
{
    [Serializable]
    public class OrganisationDto
        : DtoBase<int>
    {
        public OrganisationDto(int id, string name)
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
