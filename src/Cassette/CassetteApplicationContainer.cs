﻿using System;
using System.IO;

namespace Cassette
{
    class CassetteApplicationContainer : ICassetteApplicationContainer
    {
        public static ICassetteApplicationContainer Instance { get; set; }

        readonly Func<ICassetteApplication> createApplication;
        FileSystemWatcher watcher;
        Lazy<ICassetteApplication> application;
        bool creationFailed;

        public CassetteApplicationContainer(Func<ICassetteApplication> createApplication)
        {
            this.createApplication = createApplication;
            application = new Lazy<ICassetteApplication>(CreateApplication);
        }

        public CassetteApplicationContainer(Func<ICassetteApplication> createApplication, string rootDirectoryToWatch)
            : this(createApplication)
        {

            // In production mode we don't expect the asset files to change
            // while the application is running. Changes to assets will involve a 
            // re-deployment and restart of the app pool. So new assets are loaded then.

            // In development mode, asset files will likely change while application is
            // running. So watch the file system and recycle the application object 
            // when files are created/changed/deleted/etc.
            StartWatchingFileSystem(rootDirectoryToWatch);
        }

        public ICassetteApplication Application
        {
            get
            {
                return application.Value;
            }
        }

        void StartWatchingFileSystem(string rootDirectoryToWatch)
        {
            watcher = new FileSystemWatcher(rootDirectoryToWatch)
            {
                IncludeSubdirectories = true,
                EnableRaisingEvents = true
            };
            watcher.Created += HandleFileSystemChange;
            watcher.Changed += HandleFileSystemChange;
            watcher.Renamed += HandleFileSystemChange;
            watcher.Deleted += HandleFileSystemChange;
        }

        void HandleFileSystemChange(object sender, FileSystemEventArgs e)
        {
            RecycleApplication();
        }

        public void RecycleApplication()
        {
            if (IsPendingCreation) return; // Already recycled, awaiting first creation.

            lock (this)
            {
                if (IsPendingCreation) return;

                if (creationFailed)
                {
                    creationFailed = false;
                }
                else
                {
                    application.Value.Dispose();
                }
                // Re-create the lazy object. So the application isn't created until it's asked for.
                application = new Lazy<ICassetteApplication>(CreateApplication);
            }
        }

        public void ForceApplicationCreation()
        {
            var forceCreation = application.Value;
        }

        bool IsPendingCreation
        {
            get { return creationFailed == false && application.IsValueCreated == false; }
        }

        ICassetteApplication CreateApplication()
        {
            try
            {
                var app = createApplication();
                creationFailed = false;
                return app;
            }
            catch
            {
                creationFailed = true;
                throw;
            }
        }

        public void Dispose()
        {
            if (watcher != null)
            {
                watcher.Dispose();
            }
            if (application.IsValueCreated)
            {
                application.Value.Dispose();
            }
        }
    }
}