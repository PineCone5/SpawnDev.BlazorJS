﻿using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.DependencyInjection;
using SpawnDev.BlazorJS.JSObjects;
using SpawnDev.BlazorJS.JsonConverters;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Reflection.Metadata.Ecma335;
using System.Runtime.CompilerServices;
using System.Text.Json;
using static SpawnDev.BlazorJS.IServiceCollectionExtensions;

namespace SpawnDev.BlazorJS
{
    public interface IBackgroundService
    {

    }
    public interface IAsyncBackgroundService : IBackgroundService
    {
        Task InitAsync();
    }
    public static class IServiceCollectionExtensions
    {
        /// <summary>
        /// WARNING: Modifying the JSRuntime.JsonSerializerOptions can have unexpected results.
        /// </summary>
        /// <param name="_this"></param>
        /// <param name="configure"></param>
        /// <returns></returns>
        public static IServiceCollection AddJSRuntimeJsonOptions(this IServiceCollection _this, Action<JsonSerializerOptions> configure)
        {
            if (BlazorJSRuntime.RuntimeJsonSerializerOptions != null) configure(BlazorJSRuntime.RuntimeJsonSerializerOptions);
            return _this;
        }
        static IServiceCollection? serviceCollection = null;
        static bool _AddBlazorJSRuntimeCalled = false;
        /// <summary>
        /// Adds the BlazorJSRuntime singleton service and initializes it.
        /// </summary>
        /// <param name="_this"></param>
        /// <returns></returns>
        public static IServiceCollection AddBlazorJSRuntime(this IServiceCollection _this)
        {
            if (_AddBlazorJSRuntimeCalled) return _this;
            _AddBlazorJSRuntimeCalled = true;
            serviceCollection = _this;
            // add IServiceCollection singleton
            _this.AddSingleton<IServiceCollection>(_this);
            // add BlazorJSRuntime and IBlazorJSRuntime singleton
            BlazorJSRuntime.JS = new BlazorJSRuntime();
            return _this.AddSingleton<BlazorJSRuntime>(BlazorJSRuntime.JS).AddSingleton<IBlazorJSRuntime>(BlazorJSRuntime.JS);
        }
        internal static Dictionary<Type, AutoStartedService> AutoStartedServices { get; private set; } = new Dictionary<Type, AutoStartedService>();
        internal enum StartupState
        {
            None,
            ShouldStart,
            Starting,
            Constructing,
            Started,
        }
        internal class AutoStartedService
        {
            public ServiceDescriptor ServiceDescriptor { get; set; }
            public GlobalScope GlobalScope { get; set; }
            public bool ImplementsIBackgroundService { get; set; }
            public bool ImplementsIAsyncBackgroundService { get; set; }
            public StartupState StartupState { get; set; }
            public List<Type> DependencyTypes { get; set; }
            public int DependencyOrder { get; set; } = -1;
            public Type? DependencyOfType { get; set; }
            public bool InitAsyncCalled { get; set; }
        }

        //static WebAssemblyHost? WebAssemblyHost { get; set; }

        //static IServiceProvider? Services { get; set; }

        static bool StartBackgroundServicesRan = false;
        /// <summary>
        /// Services implementing IBackgroundService or IAsyncBackgroundService will be started
        /// Services implementing IAsyncBackgroundService will have their InitAsync methods called in the order the were constructed
        /// Singletons registered with an auto start GlobalScope that matches the current scope will be started
        /// Background services must be careful to not take too long in their InitAsync methods as other services are waiting to init and the app is waiting to start
        /// </summary>
        /// <param name="_this"></param>
        /// <returns></returns>
        public static async Task<WebAssemblyHost> StartBackgroundServices(this WebAssemblyHost _this)
        {
            if (StartBackgroundServicesRan) return _this;
            StartBackgroundServicesRan = true;
            var JS = BlazorJSRuntime.JS;
            //WebAssemblyHost = _this;
            //Services = _this.Services;
            serviceCollection!.ToList().ForEach(o =>
            {
                var isIBackgroundService = typeof(IBackgroundService).IsAssignableFrom(o.ServiceType) || typeof(IBackgroundService).IsAssignableFrom(o.ImplementationType);
                var isIAsyncBackgroundService = typeof(IAsyncBackgroundService).IsAssignableFrom(o.ServiceType) || typeof(IAsyncBackgroundService).IsAssignableFrom(o.ImplementationType);
                var serviceAutoStartScopeSet = GetAutoStartMode(o.ServiceType);
                var serviceAutoStartScope = GlobalScope.None;
                if (serviceAutoStartScopeSet == null || serviceAutoStartScopeSet.Value == GlobalScope.Default)
                {
                    serviceAutoStartScope = isIBackgroundService ? GlobalScope.All : GlobalScope.None;
                }
                else
                {
                    serviceAutoStartScope = serviceAutoStartScopeSet.Value;
                }
                var implementationType = o.ImplementationType ?? o.ServiceType;
                var constructor = implementationType.GetConstructors().FirstOrDefault();
                var shouldStart = JS.IsScope(serviceAutoStartScope);
                var autoStartedService = new AutoStartedService
                {
                    ServiceDescriptor = o,
                    //Service = null,
                    GlobalScope = serviceAutoStartScope,
                    ImplementsIAsyncBackgroundService = isIAsyncBackgroundService,
                    ImplementsIBackgroundService = isIBackgroundService,
                    StartupState = shouldStart ? StartupState.ShouldStart : StartupState.None,
                    DependencyTypes = constructor != null ? constructor.GetParameters().Select(p => p.ParameterType).ToList() : new List<Type>(),
                };
                //Console.WriteLine($"{o.ServiceType.Name} shouldStart: {shouldStart} {autoStartedService.DependencyTypes.Count}");
                AutoStartedServices.Add(o.ServiceType, autoStartedService);
            });
            ////// start in order of registration. dependencies will be started first.
            ////foreach (var autoStartedService in AutoStartedServices)
            ////{
            ////    InitService(autoStartedService.ServiceDescriptor.ServiceType, null);
            ////}
            // call InitAsync on each IAsyncBackgroundService in order of creation
            //var startedServicesOrdered = AutoStartedServices.Where(o => o.DependencyOrder >= 0).OrderBy(o => o.DependencyOrder).ToList();
            foreach (var serviceInfo in AutoStartedServices.Values)
            {
                await InitServiceAsync(_this.Services, serviceInfo, null, false);
            }
            return _this;
        }

        public static async Task<object?> GetServiceAsync(this IServiceProvider _this, Type type)
        {
            if (!AutoStartedServices.TryGetValue(type, out var serviceInfo)) return null;
            await InitServiceAsync(_this, serviceInfo, null, true);
            return _this.GetService(type);
        }

        public static async Task<T?> GetServiceAsync<T>(this IServiceProvider _this) where T : class 
        {
            return (T?)await _this.GetServiceAsync(typeof(T));
        }

        //internal static void InitServiceNow(Type type)
        //{
        //    var serviceInfo = AutoStartedServices.Find(o => o.ServiceDescriptor.ServiceType == type);
        //    if (serviceInfo.ServiceDescriptor.ImplementationInstance)
        //    InitService(serviceInfo, null);
        //}
        //internal static void InitService(Type type, bool isRequired)
        //{
        //    var serviceInfo = AutoStartedServices.Find(o => o.ServiceDescriptor.ServiceType == type);
        //    InitService(serviceInfo, dependencyOfType);
        //}
        //        internal static object? InitService(AutoStartedService? serviceInfo, Type? dependencyOfType = null)
        //        {
        //            if (serviceInfo == null)
        //            {
        //                return null;
        //            }
        //            if (serviceInfo.StartupState == StartupState.None && dependencyOfType != null)
        //            {
        //                serviceInfo.StartupState = StartupState.ShouldStart;
        //                //serviceInfo.DependencyOfType = dependencyOfType;
        //            }
        //            if (serviceInfo.StartupState != StartupState.ShouldStart)
        //            {
        //                return null;
        //            }
        //            serviceInfo.StartupState = StartupState.Starting;
        //            foreach (var dependencyType in serviceInfo.DependencyTypes)
        //            {
        //                InitService(dependencyType, serviceInfo.ServiceDescriptor.ServiceType);
        //            }
        //            // this actual creates the instance
        //            var service = Services.GetRequiredService(serviceInfo.ServiceDescriptor.ServiceType);
        //            serviceInfo.DependencyOrder = AutoStartedServices.Where(o => o.StartupState == StartupState.Started).Count();
        //            serviceInfo.StartupState = StartupState.Started;
        //#if DEBUG && false
        //            if (serviceInfo.DependencyOfType != null)
        //            {
        //                Console.WriteLine($"Started background service: {serviceInfo.ServiceDescriptor.ServiceType.Name} dependency of {serviceInfo.DependencyOfType.Name}");
        //            }
        //            else
        //            {
        //                Console.WriteLine($"Started background service: {serviceInfo.ServiceDescriptor.ServiceType.Name}");
        //            }
        //#endif
        //            return service;
        //        }
        internal static async Task InitServiceAsync(this IServiceProvider _this, AutoStartedService? serviceInfo, Type? dependencyOfType, bool isRequired = false)
        {
            if (serviceInfo == null)
            {
                return;
            }
            if (serviceInfo.StartupState == StartupState.None && (isRequired || dependencyOfType != null))
            {
                serviceInfo.StartupState = StartupState.ShouldStart;
                serviceInfo.DependencyOfType = dependencyOfType;
            }
            if (serviceInfo.StartupState != StartupState.ShouldStart)
            {
                return;
            }
            serviceInfo.StartupState = StartupState.Starting;
            foreach (var dependencyType in serviceInfo.DependencyTypes)
            {
                await InitServiceAsync(_this, serviceInfo, serviceInfo.ServiceDescriptor.ServiceType);
            }
            var instanceExists = serviceInfo.ServiceDescriptor.ImplementationInstance != null;            
            // this actual creates the instance
            var service = _this.GetRequiredService(serviceInfo.ServiceDescriptor.ServiceType);
            serviceInfo.DependencyOrder = AutoStartedServices.Values.Where(o => o.StartupState == StartupState.Started).Count();
            serviceInfo.StartupState = StartupState.Started;
            if (!instanceExists)
            {
#if DEBUG && false
                if (serviceInfo.DependencyOfType != null)
                {
                    Console.WriteLine($"Started background service: {serviceInfo.ServiceDescriptor.ServiceType.Name} dependency of {serviceInfo.DependencyOfType.Name}");
                }
                else
                {
                    Console.WriteLine($"Started background service: {serviceInfo.ServiceDescriptor.ServiceType.Name}");
                }
#endif
            }
            if (!serviceInfo.InitAsyncCalled && service is IAsyncBackgroundService asyncBG)
            {
#if DEBUG && false
                if (serviceInfo.DependencyOfType != null)
                {
                    Console.WriteLine($"InitAsync background service: {serviceInfo.ServiceDescriptor.ServiceType.Name} dependency of {serviceInfo.DependencyOfType.Name}");
                }
                else
                {
                    Console.WriteLine($"InitAsync background service: {serviceInfo.ServiceDescriptor.ServiceType.Name}");
                }
#endif
                serviceInfo.InitAsyncCalled = true;
                await asyncBG.InitAsync();
            }
        }



        public static async Task<object?> FindServiceAsync( this IServiceProvider _this, Type? type)
        {
            if (type == null) return null;
            var service = await _this.GetServiceAsync(type);
            if (service == null)
            {
                //Console.WriteLine($"serviceType not found: {type.Name}");
                foreach (var serviceDescriptor in serviceCollection)
                {
                    var serviceType = serviceDescriptor.ServiceType;
                    var implementationType = serviceDescriptor.ImplementationType != null ? serviceDescriptor.ImplementationType :
                        (serviceDescriptor.ImplementationInstance != null ? serviceDescriptor.ImplementationInstance.GetType() : null);
                    //var implementationTypeName = implementationType == null ? "[UNNAMED]" : implementationType.Name;
                    var matchesImplementationType = type == implementationType;
                    //Console.WriteLine($">>> {serviceType.Name} {implementationTypeName}");
                    if (matchesImplementationType)
                    {
                        //Console.WriteLine($"+++ serviceType found using implementation: {serviceType.Name} {implementationTypeName}");
                        service = await _this.GetServiceAsync(serviceType);
                        break;
                    }
                    else if (serviceType.IsAssignableFrom(type))
                    {
                        //Console.WriteLine($"+++ serviceType found using implementation: {serviceType.Name} {implementationTypeName}");
                        service = await _this.GetServiceAsync(serviceType);
                        break;
                    }
                }
            }
            return service;
        }

        /// <summary>
        /// BlazorJSRunAsync() is a scope aware replacement for RunAsync().<br />
        /// RunAsync() will be called internally but only when running in a Window global scope to prevent components from loading in Worker scopes.<br />
        /// BlazorJSRunAsync() also automatically starts IBackgroundService services, IAsyncBackgroundService services<br />
        /// and singletons services registered with a GlobalScope enum value that is compatible the current GlobalScope.<br />
        /// </summary>
        /// <param name="_this"></param>
        /// <returns></returns>
        public static async Task BlazorJSRunAsync(this WebAssemblyHost _this)
        {
            await _this.StartBackgroundServices();
            if (BlazorJSRuntime.JS.IsWindow)
            {
#if DEBUG && false
                Console.WriteLine($"BlazorJSRunAsync mode: Default");
#endif
                // run as normal where Blazor has the window global context it expects
                await _this.RunAsync();
            }
            else
            {
#if DEBUG && false
                Console.WriteLine($"BlazorJSRunAsync mode: Worker");
#endif
                // This is a worker so we are going to use this to allow services in workers without the html renderer trying to load pages
                var tcs = new TaskCompletionSource<object>();
                await tcs.Task;
            }
        }

        private static GlobalScope? GetAutoStartMode(Type type)
        {
            return AutoStartModes.TryGetValue(type, out var mode) ? mode : null;
        }


        // AddSingleton overloads that also take GlobalScope
        private static Dictionary<Type, GlobalScope> AutoStartModes = new Dictionary<Type, GlobalScope>();

        /// <summary>
        /// Adds a singleton service of the type specified in <typeparamref name="TService"/> with an
        /// implementation type specified in <typeparamref name="TImplementation"/> to the
        /// specified <see cref="IServiceCollection"/>.
        /// </summary>
        /// <typeparam name="TService">The type of the service to add.</typeparam>
        /// <typeparam name="TImplementation">The type of the implementation to use.</typeparam>
        /// <param name="services">The <see cref="IServiceCollection"/> to add the service to.</param>
        /// <returns>A reference to this instance after the operation has completed.</returns>
        /// <seealso cref="ServiceLifetime.Singleton"/>
        public static IServiceCollection AddSingleton<TService, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TImplementation>(this IServiceCollection services, GlobalScope autoStartMode)
            where TService : class
            where TImplementation : class, TService
        {
            ThrowIfNull(services);
            AutoStartModes.Add(typeof(TService), autoStartMode);

            return services.AddSingleton(typeof(TService), typeof(TImplementation));
        }

        /// <summary>
        /// Adds a singleton service of the type specified in <paramref name="serviceType"/> to the
        /// specified <see cref="IServiceCollection"/>.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/> to add the service to.</param>
        /// <param name="serviceType">The type of the service to register and the implementation to use.</param>
        /// <returns>A reference to this instance after the operation has completed.</returns>
        /// <seealso cref="ServiceLifetime.Singleton"/>
        public static IServiceCollection AddSingleton(
            this IServiceCollection services,
            [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] Type serviceType, GlobalScope autoStartMode)
        {
            ThrowIfNull(services);
            ThrowIfNull(serviceType);
            AutoStartModes.Add(serviceType, autoStartMode);

            return services.AddSingleton(serviceType, serviceType);
        }

        /// <summary>
        /// Adds a singleton service of the type specified in <typeparamref name="TService"/> to the
        /// specified <see cref="IServiceCollection"/>.
        /// </summary>
        /// <typeparam name="TService">The type of the service to add.</typeparam>
        /// <param name="services">The <see cref="IServiceCollection"/> to add the service to.</param>
        /// <returns>A reference to this instance after the operation has completed.</returns>
        /// <seealso cref="ServiceLifetime.Singleton"/>
        public static IServiceCollection AddSingleton<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TService>(this IServiceCollection services, GlobalScope autoStartMode)
            where TService : class
        {
            ThrowIfNull(services);
            AutoStartModes.Add(typeof(TService), autoStartMode);

            return services.AddSingleton(typeof(TService));
        }

        /// <summary>
        /// Adds a singleton service of the type specified in <typeparamref name="TService"/> with a
        /// factory specified in <paramref name="implementationFactory"/> to the
        /// specified <see cref="IServiceCollection"/>.
        /// </summary>
        /// <typeparam name="TService">The type of the service to add.</typeparam>
        /// <param name="services">The <see cref="IServiceCollection"/> to add the service to.</param>
        /// <param name="implementationFactory">The factory that creates the service.</param>
        /// <returns>A reference to this instance after the operation has completed.</returns>
        /// <seealso cref="ServiceLifetime.Singleton"/>
        public static IServiceCollection AddSingleton<TService>(
            this IServiceCollection services,
            Func<IServiceProvider, TService> implementationFactory, GlobalScope autoStartMode)
            where TService : class
        {
            ThrowIfNull(services);
            ThrowIfNull(implementationFactory);
            AutoStartModes.Add(typeof(TService), autoStartMode);
            return services.AddSingleton(typeof(TService), implementationFactory);
        }

        /// <summary>
        /// Adds a singleton service of the type specified in <typeparamref name="TService"/> with an
        /// implementation type specified in <typeparamref name="TImplementation" /> using the
        /// factory specified in <paramref name="implementationFactory"/> to the
        /// specified <see cref="IServiceCollection"/>.
        /// </summary>
        /// <typeparam name="TService">The type of the service to add.</typeparam>
        /// <typeparam name="TImplementation">The type of the implementation to use.</typeparam>
        /// <param name="services">The <see cref="IServiceCollection"/> to add the service to.</param>
        /// <param name="implementationFactory">The factory that creates the service.</param>
        /// <returns>A reference to this instance after the operation has completed.</returns>
        /// <seealso cref="ServiceLifetime.Singleton"/>
        public static IServiceCollection AddSingleton<TService, TImplementation>(
            this IServiceCollection services,
            Func<IServiceProvider, TImplementation> implementationFactory, GlobalScope autoStartMode)
            where TService : class
            where TImplementation : class, TService
        {
            ThrowIfNull(services);
            ThrowIfNull(implementationFactory);
            AutoStartModes.Add(typeof(TService), autoStartMode);

            return services.AddSingleton(typeof(TService), implementationFactory);
        }

        /// <summary>
        /// Adds a singleton service of the type specified in <paramref name="serviceType"/> with an
        /// instance specified in <paramref name="implementationInstance"/> to the
        /// specified <see cref="IServiceCollection"/>.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/> to add the service to.</param>
        /// <param name="serviceType">The type of the service to register.</param>
        /// <param name="implementationInstance">The instance of the service.</param>
        /// <returns>A reference to this instance after the operation has completed.</returns>
        /// <seealso cref="ServiceLifetime.Singleton"/>
        public static IServiceCollection AddSingleton(
            this IServiceCollection services,
            Type serviceType,
            object implementationInstance, GlobalScope autoStartMode)
        {
            ThrowIfNull(services);
            ThrowIfNull(serviceType);
            ThrowIfNull(implementationInstance);
            AutoStartModes.Add(serviceType, autoStartMode);
            var serviceDescriptor = new ServiceDescriptor(serviceType, implementationInstance);
            services.Add(serviceDescriptor);
            return services;
        }

        /// <summary>
        /// Adds a singleton service of the type specified in <typeparamref name="TService" /> with an
        /// instance specified in <paramref name="implementationInstance"/> to the
        /// specified <see cref="IServiceCollection"/>.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/> to add the service to.</param>
        /// <param name="implementationInstance">The instance of the service.</param>
        /// <returns>A reference to this instance after the operation has completed.</returns>
        /// <seealso cref="ServiceLifetime.Singleton"/>
        public static IServiceCollection AddSingleton<TService>(
            this IServiceCollection services,
            TService implementationInstance, GlobalScope autoStartMode)
            where TService : class
        {
            ThrowIfNull(services);
            ThrowIfNull(implementationInstance);
            AutoStartModes.Add(typeof(TService), autoStartMode);
            return services.AddSingleton(typeof(TService), implementationInstance);
        }

        static void ThrowIfNull<T>(T? value)
        {
            if (value == null) throw new ArgumentNullException($"{typeof(T).Name} cannot be null");
        }
    }
}
