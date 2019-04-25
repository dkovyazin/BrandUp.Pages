﻿using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;

namespace BrandUp.Pages.MongoDb.Documents
{
    public abstract class Document
    {
        [BsonId, BsonRepresentation(BsonType.String)]
        public Guid Id { get; set; }
        [BsonDateTimeOptions(Representation = BsonType.DateTime)]
        public DateTime CreatedDate { get; set; }
    }
}