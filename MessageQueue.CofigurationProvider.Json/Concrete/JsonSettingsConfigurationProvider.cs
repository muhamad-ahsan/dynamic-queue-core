using System;
using System.IO;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using MessageQueue.CofigurationProvider.Core.Abstract;
using MessageQueue.CofigurationProvider.Json.Resources;

namespace MessageQueue.CofigurationProvider.Json.Concrete
{
    /// <summary>
    /// Implementation of IQueueConfigurationProvider provider which returns configuration from .json file (e.g. appsettings).
    /// </summary>
    public class JsonSettingsConfigurationProvider : IQueueConfigurationProvider
    {
        #region Public Data Members
        public string ConfigurationFileName { get; set; } = "appsettings";
        #endregion

        #region IQueueConfigurationProvider Implementation
        public Dictionary<string, string> GetConfiguration(string configurationIdentifier)
        {
            try
            {
                #region Business Description
                // 1- All the keys in xyz.json file should be under the provided configuration section.
                #endregion

                #region Validation
                if (string.IsNullOrEmpty(configurationIdentifier))
                {
                    throw new ArgumentNullException(nameof(configurationIdentifier));
                }
                #endregion

                #region Configuration Retrieval
                var configuration = new ConfigurationBuilder().AddJsonFile($"{ConfigurationFileName}.json").Build();


                return configuration.GetSection(configurationIdentifier).Get<Dictionary<string, string>>();
                #endregion
            }
            catch (FileNotFoundException)
            {
                throw new ApplicationException(string.Format(ErrorMessages.FailedToLoadConfigurationFile, ConfigurationFileName));
            }
            catch (Exception exception)
            {
                throw new ApplicationException(string.Format(ErrorMessages.FailedToReadConfiguration, configurationIdentifier), exception);
            }
        }
        #endregion
    }
}
