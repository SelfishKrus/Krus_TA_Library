namespace Adobe.Substance
{
    //! @brief Enum type mapping from sbsario to C#
    internal enum ErrorCode : uint
    {
        SBSARIO_ERROR_OK = 0x00u, //!< No error has occurred
        SBSARIO_ERROR_STATE = 0x01u, //!< Call made with an invalid state
        SBSARIO_ERROR_INVALID = 0x02u, //!< An invalid argument was given to the api
        SBSARIO_ERROR_UNKNOWN = 0x03u, //!< An unspecified error has occurred
        SBSARIO_ERROR_FAILURE = 0x04u, //!< The operation failed to complete
    }
} // namespace Alg.Sbsario