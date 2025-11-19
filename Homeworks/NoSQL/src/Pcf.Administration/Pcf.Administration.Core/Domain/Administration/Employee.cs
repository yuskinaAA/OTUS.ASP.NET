using System;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Pcf.Administration.Core.Domain.Administration
{
    public class Employee
        : BaseEntity
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }

        [BsonIgnore]
        public string FullName => $"{FirstName} {LastName}";

        public string Email { get; set; }

        [BsonRepresentation(BsonType.String)]
        public Guid RoleId { get; set; }

        [BsonIgnore]
        public virtual Role Role { get; set; }

        public int AppliedPromocodesCount { get; set; }
    }
}