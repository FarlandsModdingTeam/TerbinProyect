using System;
using System.Collections.Generic;
using System.Text;

namespace TerbinLibrary.Communication
{
    public enum Action : byte
    {
        None = 0,
        PlaceHolder = 1,
    }


    public struct Header
    {
        private Guid _id; // pruebas y esas cosas.
        private Action _action;


    }
}
