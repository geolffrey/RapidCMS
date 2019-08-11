﻿using System;

namespace RapidCMS.Common.Enums
{
    [Flags]
    public enum ValidationState
    {
        None = 0,

        NotValidated = 1,
        Valid = 2,
        Invalid = 4,

        Modified = 128
    }
}
