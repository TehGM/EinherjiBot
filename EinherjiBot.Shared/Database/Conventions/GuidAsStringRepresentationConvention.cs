using System;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Conventions;

namespace TehGM.EinherjiBot.Database.Conventions
{
    //http://www.codewrecks.com/blog/index.php/2016/04/15/change-how-mongodb-c-driver-serialize-guid-in-new-driver-version/

    /// <summary>
    /// A convention that allows you to set the serialization representation of guid to a simple string
    /// </summary>
    public class GuidAsStringRepresentationConvention : ConventionBase, IMemberMapConvention
    {
        /// <summary>
        /// Applies a modification to the member map.
        /// </summary>
        /// The member map.
        public void Apply(BsonMemberMap memberMap)
        {
            if (memberMap.MemberType == typeof(Guid))
            {
                var serializer = memberMap.GetSerializer();
                var representationConfigurableSerializer = serializer as IRepresentationConfigurable;
                if (representationConfigurableSerializer != null)
                {
                    BsonType representation = BsonType.String;
                    var reconfiguredSerializer = representationConfigurableSerializer.WithRepresentation(representation);
                    memberMap.SetSerializer(reconfiguredSerializer);
                }
            }
        }
    }
}
