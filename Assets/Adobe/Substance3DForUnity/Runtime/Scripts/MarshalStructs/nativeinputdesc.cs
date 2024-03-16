using System;
using System.Runtime.InteropServices;

namespace Adobe.Substance
{
    //! @brief Managed representation of the native sbsario input desc type
    [StructLayout(LayoutKind.Sequential)]
    internal struct NativeInputDesc
    {
        //! @brief Unique string identifier of the input
        public IntPtr mIdentifier;

        //! @brief Display label of the input
        public IntPtr mLabel;

        //! @brief Gui group of the input.  
        public IntPtr GuiGroup;

        //! @brief Description of the input.
        public IntPtr GuiDescription;

        //! @brief GUI visibility condition.
        public IntPtr GuiVisibleIf;

        //! @brief Index of the input
        public IntPtr mIndex;

        //! @brief Type of widget used for the input
        public IntPtr inputWidgetType;

        //! @brief Type of the input
        public IntPtr mValueType;
    }
} // namespace Alg.Sbsario