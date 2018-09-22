using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Domain.Base.Services
{
    public static class AddressProcessor
    {
        /// <summary>
        /// Build a string containing the city, state/province, and postal code
        /// </summary>
        /// <param name="city">City</param>
        /// <param name="state">State or Province</param>
        /// <param name="postalCode">Postal Code</param>
        /// <returns>Formatted string with all 3 components</returns>
        public static string GetCityStatePostalCode(string city, string state, string postalCode)
        {
            StringBuilder line = new StringBuilder();

            if (!String.IsNullOrEmpty(city))
            {
                line.Append(city);
            }
            if (!String.IsNullOrEmpty(state))
            {
                if (line.Length > 0)
                {
                    line.Append(", ");
                }
                line.Append(state);
            }
            if (!String.IsNullOrEmpty(postalCode))
            {
                if (line.Length > 0)
                {
                    line.Append(" ");
                }
                line.Append(postalCode);
            }
            return line.ToString();
        }

        public static List<string> BuildAddressLabel(string addressModifier, List<string> addressLines, string city, string state, string zip, string countryCode, string countryDesc)
        {
            var label = new List<string>();
            // Build address label
            if (!String.IsNullOrEmpty(addressModifier))
            {
                label.Add(addressModifier);
            }
            if (addressLines.Count > 0)
            {
                label.AddRange(addressLines);
            }
            string cityStatePostalCode = AddressProcessor.GetCityStatePostalCode(city, state, zip);
            if (!String.IsNullOrEmpty(cityStatePostalCode))
            {
                label.Add(cityStatePostalCode);
            }
            if (!string.IsNullOrEmpty(countryCode))
            {
                if (!String.IsNullOrEmpty(countryDesc))
                {
                    // Country name gets included in all caps
                    label.Add(countryDesc.ToUpper());
                }
                else
                {
                    // if country description not available, use country code.
                    label.Add(countryCode.ToUpper());
                }
            }
            return label;
        }
    }
}
