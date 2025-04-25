// Copyright (c) Ulf Bourelius. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Reflection;

namespace MockingToolkit.Extensions.Moq.PropertyTracking
{
    public sealed record PropertyTrackingOptions(
        bool TrackAll = true,
        IReadOnlySet<string>? IncludedProperties = null,
        IReadOnlySet<string>? ExcludedProperties = null,
        Func<PropertyInfo, bool>? Predicate = null,
        Func<object?, string>? ValueFormatter = null,
        PropertyAccessType? AccessType = null,
        Func<PropertyInfo, bool>? AccessFilter = null)
    {
        internal bool ShouldTrackProperty<T>(string propertyName)
        {
            return ShouldTrackProperty(propertyName, typeof(T));
        }

        internal bool ShouldTrackProperty<T>(PropertyInfo property)
        {
            return property.DeclaringType == typeof(T) && ShouldTrackProperty(property.Name, typeof(T));
        }

        internal bool ShouldTrackProperty(PropertyInfo p)
        {
            if (p == null)
                throw new ArgumentNullException(nameof(p));

            if (p.DeclaringType == null)
                throw new ArgumentException("Property does not have a declaring type.", nameof(p));

            if (TrackAll)
                return true;

            if (Predicate != null)
                return Predicate(p);

            if (AccessFilter != null && AccessFilter(p))
                return true;

            return false;
        }

        internal bool ShouldTrackProperty(string propertyName, Type type)
        {
            if (string.IsNullOrWhiteSpace(propertyName))
                throw new ArgumentNullException(nameof(propertyName));
            if (type == null)
                throw new ArgumentNullException(nameof(type));
            if (TrackAll)
                return true;
            if (Predicate != null)
            {
                var property = type.GetProperty(propertyName) ?? throw new ArgumentException("Property not found.", nameof(propertyName));
                return Predicate(property);
            }
            if (AccessFilter != null)
            {
                var property = type.GetProperty(propertyName);
                if (property != null && AccessFilter(property))
                    return true;
            }
            return false;
        }

        internal bool CanTrackForRead(PropertyInfo prop)
        {
            if (prop == null)
                throw new ArgumentNullException(nameof(prop));

            return AccessType == null || AccessType == PropertyAccessType.Get;
        }

        internal bool CanTrackForWrite(PropertyInfo prop)
        {
            if (prop == null)
                throw new ArgumentNullException(nameof(prop));

            return AccessType == null || AccessType == PropertyAccessType.Set;
        }
    }
}
