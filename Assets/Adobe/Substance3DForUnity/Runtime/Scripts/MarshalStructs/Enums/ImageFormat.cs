namespace Adobe.Substance
{
    //! @brief Image format
    internal enum ImageFormat : int
    {
        // 2 bits reserved for the bytes per channel
        SBSARIO_IMAGE_FORMAT_8B = 0x00,

        SBSARIO_IMAGE_FORMAT_16B = 0x01,
        SBSARIO_IMAGE_FORMAT_32B = 0x02,
        /* Unused - 0x03u */
        SBSARIO_IMAGE_FORMAT_BITDEPTH_MASK = 0x03,

        // 2 bits reserved for the number of channels
        SBSARIO_IMAGE_FORMAT_RGBA = 0x00,

        SBSARIO_IMAGE_FORMAT_RGBX = 0x04,
        SBSARIO_IMAGE_FORMAT_RGB = 0x08,
        SBSARIO_IMAGE_FORMAT_L = 0x0c,
        SBSARIO_IMAGE_FORMAT_CHANNELS_MASK = 0x0c,

        // 1 bit to determine integer or floating point
        SBSARIO_IMAGE_FORMAT_INT = 0x00,

        SBSARIO_IMAGE_FORMAT_FLOAT = 0x10,

        /* Format (2 bits) */
        SBSARIO_IMAGE_FORMAT_PF_RAW = 0x0,        /**< Non-compressed flag */
        SBSARIO_IMAGE_FORMAT_PF_BC = 0x1 << 6,    /**< DXT compression flag */
        SBSARIO_IMAGE_FORMAT_PF_PVRTC = 0x3 << 6, /**< PVRTC compression flag */
        SBSARIO_IMAGE_FORMAT_PF_ETC = 0x3 << 6,   /**< ETC compression flag */
        SBSARIO_IMAGE_FORMAT_PF_Misc = 0x2 << 6,  /**< Other compression flag */
        SBSARIO_IMAGE_FORMAT_PF_MASK_RAWFormat = 0x3 << 6,

        // Combine integer and float bitfields to create more complex image types
        SBSARIO_IMAGE_FORMAT_8I = SBSARIO_IMAGE_FORMAT_8B | SBSARIO_IMAGE_FORMAT_INT,

        SBSARIO_IMAGE_FORMAT_16I = SBSARIO_IMAGE_FORMAT_16B | SBSARIO_IMAGE_FORMAT_INT,
        SBSARIO_IMAGE_FORMAT_16F = SBSARIO_IMAGE_FORMAT_16B | SBSARIO_IMAGE_FORMAT_FLOAT,
        SBSARIO_IMAGE_FORMAT_32F = SBSARIO_IMAGE_FORMAT_32B | SBSARIO_IMAGE_FORMAT_FLOAT,
        SBSARIO_IMAGE_FORMAT_PRECISION_MASK = SBSARIO_IMAGE_FORMAT_BITDEPTH_MASK | 0x10
    }

    //! @brief Enum representing the order of the output channels
    internal enum ChannelOrder : uint
    {
        SBSARIO_CHANNEL_ORDER_INVALID = 0x00u,

        SBSARIO_CHANNEL_ORDER_RGBA = 0xe4u,
        SBSARIO_CHANNEL_ORDER_BGRA = 0xc6u,
        SBSARIO_CHANNEL_ORDER_ABGR = 0x1bu,
        SBSARIO_CHANNEL_ORDER_ARGB = 0x39u,

        SBSARIO_CHANNEL_RED_MASK = 0x03u,
        SBSARIO_CHANNEL_GREEN_MASK = 0x0cu,
        SBSARIO_CHANNEL_BLUE_MASK = 0x30u,
        SBSARIO_CHANNEL_ALPHA_MASK = 0xc0u,

        SBSARIO_CHANNEL_RED_RSHIFT = 0x00u,
        SBSARIO_CHANNEL_GREEN_RSHIFT = 0x02u,
        SBSARIO_CHANNEL_BLUE_RSHIFT = 0x04u,
        SBSARIO_CHANNEL_ALPHA_RSHIFT = 0x06u,
    }
} // namespace Alg.Sbsario