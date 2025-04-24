// Copyright (c) Ulf Bourelius. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace MockingToolkit
{
    public static class MockingUtility
    {
        public static bool CanMock(Type type)
        {
            if (type.IsValueType)
            {
                return false;
            }
            if (type.IsSealed && !type.IsAbstract) // Sealed concrete classes
            {
                return false;
            }
            // Add more checks as needed (e.g., constructor accessibility)
            return true;
        }
    }
}
