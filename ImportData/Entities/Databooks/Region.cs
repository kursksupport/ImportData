using System;
using System.Collections.Generic;
using System.Text;
using System.Globalization;
using NLog;
using System.Linq;
using ImportData.IntegrationServicesClient.Models;

namespace ImportData
{
    internal class Region : Entity
    {
        public int PropertiesCount = 2;

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
            //страна
            variableForParameters = this.Parameters[shift + 1].Trim();
            var country = BusinessLogic.GetEntityWithFilter<ICountries>(r => r.Name == variableForParameters, exceptionList, logger);
            if (!string.IsNullOrEmpty(this.Parameters[shift + 1].Trim()) && country == null)
            {
                var message = string.Format("Не найдена Страна \"{1}\". Наименование организации: \"{0}\". ", name, this.Parameters[shift + 1].Trim());
                exceptionList.Add(new Structures.ExceptionsStruct { ErrorType = Constants.ErrorTypes.Warn, Message = message });
                logger.Warn(message);
            }
            try
            {
                if (ignoreDuplicates.ToLower() != Constants.ignoreDuplicates.ToLower())
                {
                    var regions = BusinessLogic.GetEntityWithFilter<IRegions>(x => x.Name == name, exceptionList, logger);
                    // Обновление сущности при условии, что найдено одно совпадение.
                    if (regions != null)
                    {
                        regions.Name = name;
                        regions.Country = country;
                        regions.Status = "Active";

                        var updatedEntity = BusinessLogic.UpdateEntity<IRegions>(regions, exceptionList, logger);
                        return exceptionList;
                    }
                }

                var region = new IRegions();

                region.Name = name;
                region.Country = country;
                region.Status = "Active";

                BusinessLogic.CreateEntity<IRegions>(region, exceptionList, logger);
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
