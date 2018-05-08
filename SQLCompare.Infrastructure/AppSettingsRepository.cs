﻿using SQLCompare.Core;
using SQLCompare.Core.Entities;
using SQLCompare.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml.Serialization;

namespace SQLCompare.Infrastructure
{
    /// <summary>
    /// Defines the application settings repository
    /// </summary>
    public class AppSettingsRepository : IAppSettingsRepository
    {
        private IAppGlobals appGlobals;

        /// <summary>
        /// Initializes a new instance of the <see cref="AppSettingsRepository"/> class.
        /// </summary>
        /// <param name="appGlobals">Injected application global constants</param>
        public AppSettingsRepository(IAppGlobals appGlobals)
        {
            this.appGlobals = appGlobals;
        }

        /// <inheritdoc/>
        public void Write(AppSettings appSettings)
        {
            if (!Directory.Exists(Path.GetDirectoryName(this.appGlobals.AppSettingsFullFilename)))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(this.appGlobals.AppSettingsFullFilename));
            }

            XmlSerializer xml = new XmlSerializer(typeof(AppSettings));
            using (var f = File.OpenWrite(this.appGlobals.AppSettingsFullFilename))
            {
                xml.Serialize(f, appSettings);
            }
        }

        /// <inheritdoc/>
        public AppSettings Read()
        {
            XmlSerializer xml = new XmlSerializer(typeof(AppSettings));
            using (var f = File.OpenRead(this.appGlobals.AppSettingsFullFilename))
            {
                return xml.Deserialize(f) as AppSettings;
            }
        }
    }
}