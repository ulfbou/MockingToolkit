// Copyright (c) Ulf Bourelius. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Linq.Expressions;
using System.Reflection;

namespace MockingToolkit.Extensions.Moq.PropertyTracking
{
    public static class IPropertyTrackingConfiguratorExtensions
    {
        public static IPropertyTrackingConfigurator IncludeProperties<T, TProperty>(this IPropertyTrackingConfigurator configurator, Expression<Func<T, TProperty>> propertyExpression) where T : class
        {
            if (propertyExpression.Body is MemberExpression memberExpression)
            {
                if (memberExpression.Member is PropertyInfo propertyInfo)
                {
                    return configurator.IncludeProperties(propertyInfo.Name);
                }
                else
                {
                    throw new ArgumentException($"The expression '{propertyExpression}' does not refer to a property.");
                }
            }
            else
            {
                throw new ArgumentException($"The expression '{propertyExpression}' is not a simple member access.");
            }
        }
    }
}
