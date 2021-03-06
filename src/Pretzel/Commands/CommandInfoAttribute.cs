﻿using System;
using System.ComponentModel.Composition;

namespace Pretzel.Commands
{
    [MetadataAttribute]
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public sealed class CommandInfoAttribute : ExportAttribute
    {
        public string CommandName { get; set; }

        public CommandInfoAttribute() : base(typeof(ICommand))
        {
        }
    }
}
