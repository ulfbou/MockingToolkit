//// Copyright (c) Ulf Bourelius. All rights reserved.
//// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Reflection;

namespace MockingToolkit.PropertyTracking.Extensions
{
    public static class PropertyInfoExtensions
    {
        // IsPublic
        public static bool IsPublic(this PropertyInfo propertyInfo)
        {
            if (propertyInfo == null)
            {
                throw new ArgumentNullException(nameof(propertyInfo));
            }

            return propertyInfo.GetMethod?.IsPublic == true || propertyInfo.SetMethod?.IsPublic == true;
        }
    }
}
