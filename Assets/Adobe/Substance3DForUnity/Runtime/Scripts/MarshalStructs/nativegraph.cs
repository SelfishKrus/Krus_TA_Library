using System;
using System.Runtime.InteropServices;

namespace Adobe.Substance
{
    //! @brief Managed representation of the native sbsario graph descriptor structure
    [StructLayout(LayoutKind.Sequential)]
    internal struct NativeGraphDesc
    {
        //! @brief Unique string label of the graph
        public IntPtr mLabel;

        //! @brief Description set for the graph
        public IntPtr mDescription;

        //! @brief Category of the graph
        public IntPtr mCategory;

        //! @brief Semicolon separated list of keywords
        public IntPtr mKeywords;

        //! @brief Graph author
        public IntPtr mAuthor;

        //! @brief Graph author website url
        public IntPtr mAuthorUrl;

        //! @brief Graph user data
        public IntPtr mUserTag;
    }
} // namespace Adobe.Substance