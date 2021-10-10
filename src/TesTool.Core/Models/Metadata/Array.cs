﻿namespace TesTool.Core.Models.Metadata
{
    public class Array : TypeWrapper
    {
        public Array(TypeWrapper type) : base(nameof(Array)) 
        { 
            Type = type;    
        }

        public TypeWrapper Type { get; private set; }
    }
}
