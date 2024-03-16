using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Adobe.Substance.Input.Description;

namespace Adobe.Substance.Input
{
    public interface ISubstanceInput
    {
        int Index { get; }

        SubstanceValueType ValueType { get; }

        bool IsNumeric { get; }

        bool IsValid { get; }
        SubstanceInputDescription Description { get; }

        void UpdateNativeHandle(SubstanceNativeGraph handler);

        internal void SetNumericDescription(NativeNumericInputDesc desc);

        /// <summary>
        /// Assigns the native enum data from the substance engine to the numerical input description (Only valid if input is numeric)
        /// </summary>
        /// <param name="options"></param>
        internal void SetEnumOptions(NativeEnumInputDesc[] options);

        bool TryGetNumericalDescription(out ISubstanceInputDescNumerical description);
    }

    /// <summary>
    /// Interface for representing all the different types of substance graph inputs.
    /// </summary>
    public abstract class SubstanceInputBase : ISubstanceInput
    {
        /// <summary>
        /// Input index inside the Substance Graph.
        /// </summary>
        public int Index;

        /// <summary>
        /// Input type.
        /// </summary>
        public abstract SubstanceValueType ValueType { get; }

        /// <summary>
        /// True if this input is numeric.
        /// </summary>
        public abstract bool IsNumeric { get; }

        /// <summary>
        /// True if this input is supported by the Unity plugin.
        /// </summary>
        public abstract bool IsValid { get; }

        /// <summary>
        /// Description with aditional information about the input.
        /// </summary>
        public SubstanceInputDescription Description;

        /// <summary>
        /// Updates the native side of the substance engine with the current value for this input.
        /// </summary>
        /// <param name="handler"></param>
        public virtual void UpdateNativeHandle(SubstanceNativeGraph handler)
        {
        }

        public virtual bool TryGetNumericalDescription(out ISubstanceInputDescNumerical description)
        {
            description = null;
            return false;
        }

        /// <summary>
        /// Assigns the native data from the substance engine to the numerical input description (Only valid if input is numeric)
        /// </summary>
        /// <param name="desc"></param>
        internal virtual void SetNumericDescription(NativeNumericInputDesc desc)
        {
        }

        /// <summary>
        /// Assigns the native enum data from the substance engine to the numerical input description (Only valid if input is numeric)
        /// </summary>
        /// <param name="options"></param>
        internal virtual void SetEnumOptions(NativeEnumInputDesc[] options)
        {
        }

        #region ISubstanceInput

        int ISubstanceInput.Index => Index;

        SubstanceInputDescription ISubstanceInput.Description => Description;

        SubstanceValueType ISubstanceInput.ValueType => ValueType;

        bool ISubstanceInput.IsNumeric => IsNumeric;

        bool ISubstanceInput.IsValid => IsValid;

        void ISubstanceInput.SetNumericDescription(NativeNumericInputDesc desc)
        {
            SetNumericDescription(desc);
        }

        void ISubstanceInput.SetEnumOptions(NativeEnumInputDesc[] options)
        {
            SetEnumOptions(options);
        }

        #endregion ISubstanceInput
    }
}