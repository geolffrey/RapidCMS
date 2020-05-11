﻿using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using Blazor.FileReader;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Http;
using RapidCMS.Core.Abstractions.Config;
using RapidCMS.Core.Abstractions.Dispatchers;
using RapidCMS.Core.Abstractions.Factories;
using RapidCMS.Core.Abstractions.Interactions;
using RapidCMS.Core.Abstractions.Resolvers;
using RapidCMS.Core.Abstractions.Services;
using RapidCMS.Core.Abstractions.Setup;
using RapidCMS.Core.Abstractions.State;
using RapidCMS.Core.Authorization;
using RapidCMS.Core.Dispatchers;
using RapidCMS.Core.Factories;
using RapidCMS.Core.Forms;
using RapidCMS.Core.Handlers;
using RapidCMS.Core.Interactions;
using RapidCMS.Core.Models.Config;
using RapidCMS.Core.Models.Request;
using RapidCMS.Core.Models.Setup;
using RapidCMS.Core.Providers;
using RapidCMS.Core.Resolvers.Buttons;
using RapidCMS.Core.Resolvers.Convention;
using RapidCMS.Core.Resolvers.Data;
using RapidCMS.Core.Resolvers.Repositories;
using RapidCMS.Core.Resolvers.Setup;
using RapidCMS.Core.Services.Auth;
using RapidCMS.Core.Services.Concurrency;
using RapidCMS.Core.Services.Exceptions;
using RapidCMS.Core.Services.Messages;
using RapidCMS.Core.Services.Parent;
using RapidCMS.Core.Services.Persistence;
using RapidCMS.Core.Services.Presentation;
using RapidCMS.Core.Services.SidePane;
using RapidCMS.Core.Services.State;
using RapidCMS.Core.Services.Tree;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class RapidCMSMiddleware
    {
        public static IServiceCollection AddRapidCMS(this IServiceCollection services, Action<ICmsConfig>? config = null)
        {
            var rootConfig = new CmsConfig();
            config?.Invoke(rootConfig);

            services.AddSingleton(rootConfig);
            services.AddSingleton<ICmsConfig>(rootConfig);

            // var cmsSetup = new CmsSetup(rootConfig);

            services.AddSingleton<ICms, CmsSetup>();
            services.AddSingleton(x => (ILogin)x.GetService(typeof(ICms)));

            services.AddSingleton<ISetupResolver<IPageSetup>, PageSetupResolver>();
            services.AddSingleton<ISetupResolver<ICollectionSetup>, CollectionSetupResolver>();
            services.AddSingleton<ISetupResolver<IEnumerable<ITreeElementSetup>>, TreeElementsSetupResolver>();
            services.AddSingleton<ISetupResolver<IEnumerable<ITreeElementSetup>, IEnumerable<ITreeElementConfig>>, TreeElementSetupResolver>();

            // TODO: fix not having corresponding interface for either class
            services.AddSingleton<ISetupResolver<ITypeRegistration, CustomTypeRegistrationConfig>, TypeRegistrationSetupResolver>();
            services.AddSingleton<ISetupResolver<IEntityVariantSetup, EntityVariantConfig>, EntityVariantSetupResolver>();
            services.AddSingleton<ISetupResolver<TreeViewSetup, TreeViewConfig>, TreeViewSetupResolver>();

            services.AddSingleton<ISetupResolver<PaneSetup, PaneConfig>, PaneSetupResolver>();
            services.AddSingleton<ISetupResolver<ListSetup, ListConfig>, ListSetupResolver>();
            services.AddSingleton<ISetupResolver<NodeSetup, NodeConfig>, NodeSetupResolver>();
            services.AddSingleton<ISetupResolver<FieldSetup, FieldConfig>, FieldSetupResolver>();
            services.AddSingleton<ISetupResolver<IButtonSetup, ButtonConfig>, ButtonSetupResolver>();
            services.AddSingleton<ISetupResolver<SubCollectionListSetup, CollectionListConfig>, SubCollectionListSetupResolver>();
            services.AddSingleton<ISetupResolver<RelatedCollectionListSetup, CollectionListConfig>, RelatedCollectionListSetupResolver>();

            services.AddSingleton<IConventionBasedResolver<ListConfig>, ConventionBasedListConfigResolver>();
            services.AddSingleton<IConventionBasedResolver<NodeConfig>, ConventionBasedNodeConfigResolver>();
            services.AddSingleton<IFieldConfigResolver, FieldConfigResolver>();

            if (rootConfig.AllowAnonymousUsage)
            {
                services.AddSingleton<IAuthorizationHandler, AllowAllAuthorizationHandler>();
                services.AddSingleton<AuthenticationStateProvider, AnonymousAuthenticationStateProvider>();
            }

            services.AddTransient<IUIResolverFactory, UIResolverFactory>();

            services.AddTransient<IButtonActionHandlerResolver, ButtonActionHandlerResolver>();
            services.AddTransient<IDataProviderResolver, DataProviderResolver>();
            services.AddTransient<IDataViewResolver, DataViewResolver>();
            services.AddTransient<IRepositoryResolver, RepositoryResolver>();

            services.AddTransient<IPresenationDispatcher<GetEntityRequestModel, EditContext>, GetEntityDispatcher>();
            services.AddTransient<IPresenationDispatcher<GetEntitiesRequestModel, ListContext>, GetEntitiesDispatcher>();
            services.AddTransient<IPresenationDispatcher<string, IEnumerable<ITypeRegistration>>, GetPageDispatcher>();
            services.AddTransient<IPresentationService, PresentationService>();

            services.AddTransient<IInteractionDispatcher, EntityInteractionDispatcher>();
            services.AddTransient<IInteractionDispatcher, EntitiesInteractionDispatcher>();
            services.AddTransient<IButtonInteraction, ButtonInteraction>();
            services.AddTransient<IDragInteraction, DragInteraction>();
            services.AddTransient<IInteractionService, InteractionService>();

            services.AddTransient<IAuthService, AuthService>();
            services.AddTransient<IConcurrencyService, ConcurrencyService>();
            services.AddSingleton<IExceptionService, ExceptionService>();
            services.AddScoped<IMessageService, MessageService>();
            services.AddScoped<INavigationState, NavigationState>();
            services.AddTransient<IPageState, PageState>();
            services.AddTransient<IParentService, ParentService>();
            services.AddScoped<ISidePaneService, SidePaneService>();
            services.AddTransient<ITreeService, TreeService>();

            services.AddScoped<DefaultButtonActionHandler>();
            services.AddScoped(typeof(OpenPaneButtonActionHandler<>));

            services.AddScoped(typeof(EnumDataProvider<>), typeof(EnumDataProvider<>));

            // UI requirements
            services.AddHttpContextAccessor();
            services.AddScoped<HttpContextAccessor>();

            services.AddHttpClient();
            services.AddScoped<HttpClient>();

            // Semaphore for repositories
            services.AddSingleton(serviceProvider => new SemaphoreSlim(rootConfig.Advanced.SemaphoreCount, rootConfig.Advanced.SemaphoreCount));

            services.AddFileReaderService();

            services.AddMemoryCache();

            return services;
        }

        public static IApplicationBuilder UseRapidCMS(this IApplicationBuilder app, bool isDevelopment = false)
        {
            app.ApplicationServices.GetService<ICms>().IsDevelopment = isDevelopment;

            return app;
        }
    }
}
