// Copyright (c) Ulf Bourelius. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Castle.Core.Logging;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

using MockingToolkit.Extensions.Moq;

using Moq;

using System.Collections.Concurrent;

namespace MockingToolkit
{
	/// <summary>
	/// Centralized provider for creating and managing mocks of arbitrary interfaces.
	/// Supports recursive mock generation, default behaviors, diagnostics, and DI integration.
	/// </summary>
	public sealed class MockProvider
	{
		private readonly ConcurrentDictionary<Type, Mock> _mocks = new();
		private readonly ConcurrentDictionary<Type, DefaultValueProvider> _customDefaultValueProviders = new();
		private readonly ConcurrentDictionary<Type, int> _usageCounts = new();
		private readonly ILogger<MockProvider>? _logger;

		public MockProvider(ILogger<MockProvider>? logger = null)
		{
			_logger = logger;
		}

		/// <summary>
		/// Retrieves the <see cref="Mock{T}"/> instance for the specified interface type T."/>
		/// Mocks are cached per-type, and DefaultValue.Mock is enabled for automatic recursive mocking.
		/// </summary>
		public Mock<T> GetMock<T>() where T : class
		{
			return (Mock<T>)GetOrCreateMock(typeof(T));
		}

		/// <summary>
		/// Retrieves the mock object (the .Object property) for T, useful for injections.
		/// </summary>
		public T Get<T>() where T : class
		{
			return GetMock<T>().Object;
		}

		/// <summary>
		/// Applies a configuration action to the Mock&lt;T&gt; of the specified interface.
		/// </summary>
		/// <typeparam name="T">The interface type for which to configure the mock.</typeparam>
		public void ConfigureMock<T>(Action<Mock<T>> configure) where T : class
		{
			if (configure == null)
			{
				throw new ArgumentNullException(nameof(configure));
			}

			if (_mocks.TryGetValue(typeof(T), out var mock))
			{
				if (mock is Mock<T> typedMock)
				{
					try
					{
						configure(typedMock);
					}
					catch (Exception ex)
					{
						if (_logger?.IsEnabled(LogLevel.Error) == true)
						{
							using (_logger.BeginScope(new Dictionary<string, object>
							{
								["TypeName"] = typeof(T).FullName ?? typeof(T).Name,
								["Type"] = typeof(T),
								["Exception"] = ex
							}))
							{
								_logger.LogError(ex, "Error configuring mock for type: {TypeName}", typeof(T).FullName);
							}
						}
					}
				}
				else
				{
					throw new InvalidOperationException($"Mock for {typeof(T).FullName} is not of the expected type.");
				}
			}
			else
			{
				throw new InvalidOperationException($"No mock found for {typeof(T).FullName}. Please create it first.");
			}
		}

		/// <summary>
		/// Diagnostic utility to inspect how many times each mock was retrieved or created.
		/// </summary>
		public IReadOnlyDictionary<Type, int> UsageCounts => _usageCounts;

		/// <summary>
		/// Registers a custom default value provider for a specific interface type.
		/// </summary>
		/// <typeparam name="T">The interface type for which to register the provider.</typeparam>
		/// <param name="provider">The custom default value provider.</param>
		public void RegisterDefaultValueProvider<T>(DefaultValueProvider provider) where T : class
		{
			_customDefaultValueProviders.TryAdd(typeof(T), provider);
		}

		private Mock GetOrCreateMock(Type type)
		{
			_usageCounts.AddOrUpdate(type, 1, (_, count) => count + 1);
			return _mocks.GetOrAdd(type, t =>
			{
				var mockType = typeof(Mock<>).MakeGenericType(t);
				try
				{
					var mock = (Mock)Activator.CreateInstance(mockType)!;
					mock.DefaultValue = DefaultValue.Mock;
					if (_customDefaultValueProviders.TryGetValue(type, out var customProvider))
					{
						mock.DefaultValueProvider = customProvider;
					}
					else
					{
						mock.DefaultValueProvider = new AutoMockDefaultValueProvider(this);
					}
					return mock;
				}
				catch (Exception ex)
				{
					if (_logger?.IsEnabled(LogLevel.Error) == true)
					{
						using (_logger.BeginScope(new Dictionary<string, object>
						{
							["TypeName"] = t.FullName ?? t.Name,
							["Type"] = t,
							["Exception"] = ex
						}))
						{
							_logger.LogError(ex, "Error creating mock for type: {TypeName}", t.FullName);

							if (ex.InnerException != null)
							{
								_logger.LogError(ex.InnerException, "Inner exception while creating mock for type: {TypeName}", t.FullName);
							}
						}
					}

					return CreateFallbackMock(t);
				}
			});
		}

		private readonly ConcurrentDictionary<Type, Func<object>> _fallbackFactories = new();

		public void RegisterFallbackFactory<T>(Func<T> factory) where T : class
		{
			_fallbackFactories.TryAdd(typeof(T), factory);
		}

		private Mock CreateFallbackMock(Type type)
		{
			var mockType = typeof(Mock<>).MakeGenericType(type);
			var mockInstance = (Mock)Activator.CreateInstance(mockType)!;

			if (_fallbackFactories.TryGetValue(type, out var factory))
			{
				LogWarning("Using fallback factory", type);

				// Get the generic Mock<T> type
				var genericMockType = typeof(Mock<>).MakeGenericType(type);

				// Get the TrackAllProperties extension method using reflection
				var TrackAllPropertiesMethod = typeof(GenericMockExtensions).GetMethod("TrackAllProperties");
				var genericTrackAllProperties = TrackAllPropertiesMethod?.MakeGenericMethod(type);
				genericTrackAllProperties?.Invoke(null, new[] { Convert.ChangeType(mockInstance, genericMockType) });

				// Get the ReturnsDefaultValue extension method using reflection
				var returnsDefaultValueMethod = typeof(GenericMockExtensions).GetMethod("ReturnsDefaultValue");
				var genericReturnsDefaultValue = returnsDefaultValueMethod?.MakeGenericMethod(type);
				genericReturnsDefaultValue?.Invoke(null, new[] { Convert.ChangeType(mockInstance, genericMockType) });

				return mockInstance;
			}
			else
			{
				LogWarning("No fallback registered. Returning basic mock", type);
				return mockInstance;
			}
		}

		private void LogWarning(string message, Type type)
		{
			if (_logger?.IsEnabled(LogLevel.Warning) == true)
			{
				using (_logger.BeginScope(new Dictionary<string, object>
				{
					["TypeName"] = type.FullName ?? type.Name,
					["Type"] = type
				}))
				{
					_logger.LogWarning("{Message} for type: {TypeName}", message, type.FullName);
				}
			}
		}

		internal class AutoMockDefaultValueProvider : DefaultValueProvider
		{
			private readonly MockProvider _provider;
			public AutoMockDefaultValueProvider(MockProvider provider)
			{
				_provider = provider;
			}

			protected override object GetDefaultValue(Type type, Mock mock)
			{
				if (!type.IsInterface && !type.IsAbstract)
				{
					return type.IsValueType ? Activator.CreateInstance(type)! : null!;
				}

				return _provider.Get(type);
			}
		}

		/// <summary>
		/// Internal helper to retrieve a mock object of unknown closed interface type.
		/// </summary>
		private object Get(Type type)
		{
			return GetOrCreateMock(type).Object;
		}
	}
}
