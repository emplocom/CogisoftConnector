using CogisoftConnector.Models.Cogisoft.CogisoftResponseModels;
using EmploApiSDK.ApiModels.Employees;
using EmploApiSDK.Logger;

namespace CogisoftConnector.Logic
{
    public class ApiRequestModelBuilder
    {
        private readonly CogisoftEmployeeImportConfiguration _cogisoftEmployeeImportMappingConfiguration;

        public ApiRequestModelBuilder(CogisoftEmployeeImportConfiguration configuration)
        {
            _cogisoftEmployeeImportMappingConfiguration = configuration;
        }

        public UserDataRow BuildUserDataRow(GetEmployeeDataResponseCogisoftModel.R row)
        {
            var importedEmployeeRow = new UserDataRow();

            foreach (var mapping in _cogisoftEmployeeImportMappingConfiguration.PropertyMappings)
            {
                var value = row.sc[_cogisoftEmployeeImportMappingConfiguration.PropertyMappings.IndexOf(mapping)].ToString();

                if (value.Trim().Replace("\r\n", "").Replace(" ", "").Equals(@"{""n"":""1""}"))
                {
                    importedEmployeeRow.Add(mapping.EmploPropertyName, null);
                }
                else
                {
                    importedEmployeeRow.Add(mapping.EmploPropertyName, NormalizeString(value));
                }
            }

            return importedEmployeeRow;
        }

        private string NormalizeString(string @string)
        {
            return @string.Trim('\t', ' ');
        }
    }
}