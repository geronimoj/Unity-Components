using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Xml.Linq;

namespace Helpers
{
    /// <summary>
    /// Tools for converting objects into XML
    /// </summary>
    public class XMLHelper
    {
        /// <summary>
        /// Read or Writes a float into or from XElement
        /// </summary>
        /// <param name="element">The XML to store/read it from</param>
        /// <param name="name">The name of the parameter</param>
        /// <param name="read">Should the value be read or written to</param>
        /// <param name="value">The value to write</param>
        public static void XMLFloat(XElement element, string name, bool read, ref float value)
        {
            if (read)
                float.TryParse(element.Attribute(name).Value, out value);
            else
                element.Add(new XAttribute(name, value));
        }
        /// <summary>
        /// Read or Write an int value from XElement
        /// </summary>
        /// <param name="element">The XML to store/read from</param>
        /// <param name="name">The name of the element</param>
        /// <param name="read">Should the value be read or written to</param>
        /// <param name="value">The value to write</param>
        public static void XMLInt(XElement element, string name, bool read, ref int value)
        {
            if (read)
                int.TryParse(element.Attribute(name).Value, out value);
            else
                element.Add(new XAttribute(name, value));
        }
        /// <summary>
        /// Read or Write a string value from XElement
        /// </summary>
        /// <param name="element">The XML to store/read from</param>
        /// <param name="name">The name of the element</param>
        /// <param name="read">Should the value be read or written to</param>
        /// <param name="value">The value to write</param>
        public static void XMLString(XElement element, string name, bool read, ref string value)
        {
            if (read)
                value = element.Attribute(name).Value;
            else
                element.Add(new XAttribute(name, value));
        }
    }
}