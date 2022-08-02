using System;
using System.Collections.Generic;
using System.Text;
using System.Globalization;
using NLog;
using System.Linq;
using ImportData.IntegrationServicesClient.Models;

namespace ImportData
{
    internal class City : Entity
    {
        public int PropertiesCount = 3;

        /// <summary>
        /// Получить наименование число запрашиваемых параметров.
        /// </summary>
        /// <returns>Число запрашиваемых параметров.</returns>
        public override int GetPropertiesCount()
        {
            return PropertiesCount;
        }

        /// <summary>
        /// Сохранение сущности в RX.
        /// </summary>
        /// <param name="shift">Сдвиг по горизонтали в XLSX документе. Необходим для обработки документов, составленных из элементов разных сущностей.</param>
        /// <param name="logger">Логировщик.</param>
        /// <returns>Число запрашиваемых параметров.</returns>
        public override IEnumerable<Structures.ExceptionsStruct> SaveToRX(Logger logger, bool supplementEntity, string ignoreDuplicates, int shift = 0)
        {
            var exceptionList = new List<Structures.ExceptionsStruct>();

            var name = this.Parameters[shift + 0].Trim();
            var variableForParameters = this.Parameters[shift + 0].Trim();

            if (string.IsNullOrEmpty(name))
            {
                var message = string.Format("Не заполнено поле \"Наименование\".");
                exceptionList.Add(new Structures.ExceptionsStruct { ErrorType = "Error", Message = message });
                logger.Error(message);

                return exceptionList;
            }
            //регион
            variableForParameters = this.Parameters[shift + 1].Trim();
            var region = BusinessLogic.GetEntityWithFilter<IRegions>(r => r.Name == variableForParameters, exceptionList, logger);
            if (!string.IsNullOrEmpty(this.Parameters[shift + 1].Trim()) && region == null)
            {
                var message = string.Format("Не найдена регион \"{1}\". Наименование организации: \"{0}\". ", name, this.Parameters[shift + 1].Trim());
                exceptionList.Add(new Structures.ExceptionsStruct { ErrorType = Constants.ErrorTypes.Warn, Message = message });
                logger.Warn(message);
            }

            //страна
            variableForParameters = this.Parameters[shift + 2].Trim();
            var country = BusinessLogic.GetEntityWithFilter<ICountries>(r => r.Name == variableForParameters, exceptionList, logger);
            if (!string.IsNullOrEmpty(this.Parameters[shift + 2].Trim()) && country == null)
            {
                var message = string.Format("Не найдена Страна \"{1}\". Наименование организации: \"{0}\". ", name, this.Parameters[shift + 2].Trim());
                exceptionList.Add(new Structures.ExceptionsStruct { ErrorType = Constants.ErrorTypes.Warn, Message = message });
                logger.Warn(message);
            }
            try
            {
                if (ignoreDuplicates.ToLower() != Constants.ignoreDuplicates.ToLower())
                {
                    var cities = BusinessLogic.GetEntityWithFilter<ICities>(x => x.Name == name, exceptionList, logger);
                    // Обновление сущности при условии, что найдено одно совпадение.
                    if (cities != null)
                    {
                        cities.Name = name;
                        cities.Country = country;
                        cities.Region = region;
                        cities.Status = "Active";

                        var updatedEntity = BusinessLogic.UpdateEntity<ICities>(cities, exceptionList, logger);
                        return exceptionList;
                    }
                }

                var city = new ICities();

                city.Name = name;
                city.Country = country;
                city.Region = region;
                city.Status = "Active";

                BusinessLogic.CreateEntity<ICities>(city, exceptionList, logger);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                exceptionList.Add(new Structures.ExceptionsStruct { ErrorType = Constants.ErrorTypes.Error, Message = ex.Message });

                return exceptionList;
            }
            return exceptionList;
        }
    }
}
