﻿namespace TesTool.Core.Models
{
    public class Setting
    {
        public Setting(string key)
        {
            Key = key;
        }

        public string Key { get; private set; }
    }
}