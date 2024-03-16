using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using UnityEngine;

namespace Adobe.Substance
{
    public class SubstanceNotInitializedException : Exception
    {
    }

    public class SubstanceEngineNotFoundException : Exception
    {
        public SubstanceEngineNotFoundException(string engine) : base($"Substance engine not found {engine}")
        {
        }
    }

    public class SubstanceException : Exception
    {
        internal SubstanceException(ErrorCode code) : base(code.GetMessage())
        {
        }
    }
}