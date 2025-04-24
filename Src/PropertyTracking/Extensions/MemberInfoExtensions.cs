//// Copyright (c) Ulf Bourelius. All rights reserved.
//// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace MockingToolkit.PropertyTracking.Extensions
{
    public static class MemberInfoExtensions
    {
        // IsPublic
        public static bool IsPublic(this MemberInfo memberInfo)
        {
            if (memberInfo == null)
            {
                throw new ArgumentNullException(nameof(memberInfo));
            }

            if (memberInfo.DeclaringType == null)
            {
                throw new InvalidOperationException("DeclaringType is null.");
            }

            return memberInfo.DeclaringType.IsPublic;
        }
    }
}
