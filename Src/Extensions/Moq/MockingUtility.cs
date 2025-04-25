// Copyright (c) Ulf Bourelius. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Reflection;

namespace MockingToolkit
{
    public static class MockingUtility
    {
        public static bool CanMock(Type type, bool useReflection = false)
        {
            if (type.IsValueType)
            {
                return false;
            }
            if (type.IsSealed && !type.IsAbstract) // Sealed concrete classes
            {
                return false;
            }

            if (type.IsAbstract && type.IsSealed) // Static classes
            {
                return false;
            }

            if (type.IsGenericType && !type.IsGenericTypeDefinition) // Generic types
            {
                return false;
            }

            if (type.IsGenericTypeDefinition) // Generic type definitions
            {
                return false;
            }

            if (useReflection)
            {
                // Check if the type has a default constructor
                var constructor = type.GetConstructor(Type.EmptyTypes);
                if (constructor == null)
                {
                    return false; // No default constructor
                }
            }

            return true;
        }
    }
}
