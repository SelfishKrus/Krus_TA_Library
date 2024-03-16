namespace Adobe.Substance
{
    //! @brief Enum describing whether the data is an input or output
    internal enum DataType : uint
    {
        SBSARIO_DATA_INVALID = 0x00u, //!< Invalid data
        SBSARIO_DATA_INPUT = 0x01u, //!< Input data
        SBSARIO_DATA_OUTPUT = 0x02u, //!< Output data
    }

    //! @brief Enum describing the value type of an input or output
    internal enum ValueType : uint
    {
        SBSARIO_VALUE_FLOAT = 0x00u, //!< Float type
        SBSARIO_VALUE_FLOAT2 = 0x01u, //!< Float vector with two elements
        SBSARIO_VALUE_FLOAT3 = 0x02u, //!< Float vector with three elements
        SBSARIO_VALUE_FLOAT4 = 0x03u, //!< Float vector with four elements
        SBSARIO_VALUE_INT = 0x04u, //!< Integer type
        SBSARIO_VALUE_INT2 = 0x05u, //!< Integer vector with two elements
        SBSARIO_VALUE_INT3 = 0x06u, //!< Integer vector with three elements
        SBSARIO_VALUE_INT4 = 0x07u, //!< Integer vector with four elements
        SBSARIO_VALUE_IMAGE = 0x08u, //!< Image type
        SBSARIO_VALUE_STRING = 0x09u, //!< String type, input only
        SBSARIO_VALUE_FONT = 0x0Au, //!< Font type, input only
    }

    internal enum WidgetType : uint
    {
        SBSARIO_WIDGET_NOWIDGET = 0x00u,
        SBSARIO_WIDGET_SLIDER = 0x01u,
        SBSARIO_WIDGET_ANGLE = 0x02u,
        SBSARIO_WIDGET_COLOR = 0x03u,
        SBSARIO_WIDGET_TOGGLEBUTTON = 0x04u,
        SBSARIO_WIDGET_ENUMBUTTONS = 0x05u,
        SBSARIO_WIDGET_COMBOBOX = 0x06u,
        SBSARIO_WIDGET_IMAGE = 0x07u,
        SBSARIO_WIDGET_POSITION = 0x08u,
        SBSARIO_WIDGET_INTERNAL_COUNT
    }
} // namespace Alg.Sbsario