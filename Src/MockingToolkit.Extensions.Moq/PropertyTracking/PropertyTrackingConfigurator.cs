// Copyright (c) Ulf Bourelius. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Moq;

using System;
using System.Collections.Frozen;
using System.Collections.Immutable;
using System.Linq.Expressions;
using System.Reflection;

namespace MockingToolkit.Extensions.Moq.PropertyTracking
{
    /// <summary>
    /// Provides a fluent interface for configuring property tracking options.
    /// </summary>
    public class PropertyTrackingConfigurator : IPropertyTrackingConfigurator
    {
        private readonly HashSet<string> _includedProperties = new();
        private Func<PropertyInfo, bool>? _includePredicate;
        private readonly HashSet<string> _excludedProperties = new();
        private Func<PropertyInfo, bool>? _excludePredicate;
        private Func<object?, string>? _valueFormatter;
        private Func<PropertyInfo, bool>? _accessFilter;
        private PropertyAccessType? _accessType;
        private bool _disposed = false;
        private bool _trackAll;

        public PropertyTrackingConfigurator TrackAll()
        {
            _trackAll = true;
            _includedProperties.Clear();
            return this;
        }

        public IPropertyTrackingConfigurator IncludeProperties(params string[] propertyNames)
        {
            foreach (var propertyName in propertyNames)
                _includedProperties.Add(propertyName);
            return this;
        }

        public IPropertyTrackingConfigurator IncludeProperties(Func<PropertyInfo, bool> predicate)
        {
            _includePredicate = predicate;
            return this;
        }

        public IPropertyTrackingConfigurator ExcludeProperties(params string[] propertyNames)
        {
            foreach (var propertyName in propertyNames)
                _excludedProperties.Add(propertyName);
            return this;
        }

        public IPropertyTrackingConfigurator ExcludeProperties(Func<PropertyInfo, bool> predicate)
        {
            _accessFilter = predicate;
            return this;
        }

        public IPropertyTrackingConfigurator WithValueFormatter(Func<object?, string> formatter)
        {
            _valueFormatter = formatter;
            return this;
        }

        public IPropertyTrackingConfigurator WithAccessFilter(Func<PropertyInfo, bool> filter)
        {
            _accessFilter = filter;
            return this;
        }

        public IPropertyTrackingConfigurator WithAccess(PropertyAccessType accessType)
        {
            _accessType = accessType;
            return this;
        }

        public PropertyTrackingOptions Build()
        {
            return new PropertyTrackingOptions(
                _trackAll,
                _includedProperties.Count > 0 ? _includedProperties.ToFrozenSet() : null,
                _excludedProperties.Count > 0 ? _excludedProperties.ToFrozenSet() : null,
                p => GeneratePredicates().All(predicate => predicate(p)),
                _valueFormatter,
                _accessType
            );
        }

        private IEnumerable<Func<PropertyInfo, bool>> GeneratePredicates()
        {
            // Exclude properties whose names are in the _excludedProperties collection.
            if (_excludedProperties.Count > 0)
            {
                var excludedProperties = _excludedProperties.ToFrozenSet();
                yield return p => !_excludedProperties.Contains(p.Name);
            }

            // Apply custom exclusion predicate if one is defined.
            if (_excludePredicate != null)
            {
                yield return _excludePredicate;
            }

            // Handle included properties and custom inclusion logic if tracking all properties is disabled.
            if (!_trackAll)
            {
                // Include properties whose names are in the _includedProperties collection.
                if (_includedProperties.Count > 0)
                {
                    var includedProperties = _includedProperties.ToFrozenSet();
                    yield return p => includedProperties.Contains(p.Name);
                }

                // Apply custom inclusion predicate if one is defined.
                if (_includePredicate != null)
                {
                    yield return _includePredicate;
                }
            }

            // Filter properties based on access level if an access filter is defined.
            if (_accessFilter != null)
            {
                yield return _accessFilter;
            }
        }

        public IPropertyTrackingConfigurator Clear()
        {
            _includedProperties.Clear();
            _includePredicate = null;
            _excludedProperties.Clear();
            _excludePredicate = null;
            _valueFormatter = null;
            _accessFilter = null;
            _accessType = null;
            return this;
        }

        /// <inheritdoc />
        public void Dispose()
        {
            if (!_disposed)
            {
                _disposed = true;
                Dispose(true);
                GC.SuppressFinalize(this);
            }
        }

        private void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (!_disposed)
                {
                    _disposed = true;
                    _includedProperties.Clear();
                    _includePredicate = null;
                    _excludedProperties.Clear();
                    _excludePredicate = null;
                    _valueFormatter = null;
                    _accessFilter = null;
                    _accessType = null;
                }
            }
        }
    }
}
