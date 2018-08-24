using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using CogisoftConnector.Models.Cogisoft.CogisoftRequestModels;
using CogisoftConnector.Models.Cogisoft.CogisoftResponseModels;
using EmploApiSDK.ApiModels.Employees;
using EmploApiSDK.Logger;
using EmploApiSDK.Logic.EmployeeImport;

namespace CogisoftConnector.Logic
{
    public class EmployeeImportLogic
    {
        private readonly ILogger _logger;

        private readonly ImportLogic _importLogic;

        private readonly CogisoftEmployeeImportConfiguration _cogisoftEmployeeImportMappingConfiguration;

        public EmployeeImportLogic(ILogger logger)
        {
            _logger = logger;
            _importLogic = new ImportLogic(logger);
            try
            {
                _cogisoftEmployeeImportMappingConfiguration = new CogisoftEmployeeImportConfiguration(logger);
            }
            catch (EmploApiClientFatalException e)
            {
                _logger.WriteLine($"{ExceptionLoggingUtils.ExceptionAsString(e)}", LogLevelEnum.Error);
                throw;
            }
        }

        private async Task ImportEmployeeDataInternal(CogisoftServiceClient client,
            List<string> employeeIdsToImport = null)
        {
            if (employeeIdsToImport == null)
            {
                employeeIdsToImport = new List<string>();
                _logger.WriteLine($"Employee import started");
            }
            else
            {
                _logger.WriteLine($"Employee import started for employees {string.Join(",", employeeIdsToImport)}");
            }

            bool dryRun;
            if (bool.TryParse(ConfigurationManager.AppSettings["DryRun"], out dryRun) && dryRun)
            {
                _logger.WriteLine(
                    "Importer is in DryRun mode, data retrieved from Cogisoft will be printed to log, but it won't be sent to emplo.");
            }

            GetEmployeeDataRequestCogisoftModel cogisoftRequest =
                new GetEmployeeDataRequestCogisoftModel(employeeIdsToImport);

            var importMode = ConfigurationManager.AppSettings["ImportMode"];
            var requireRegistrationForNewEmployees =
                ConfigurationManager.AppSettings["RequireRegistrationForNewEmployees"];
            List<UserDataRowWithStatus> allRowsCollection = new List<UserDataRowWithStatus>();
            List<UserDataRowWithStatus> validRowsCollection = new List<UserDataRowWithStatus>();

            bool anyObjectsLeft;

            do
            {
                var cogisoftResponse =
                    client.PerformRequestReceiveResponse<GetEmployeeDataRequestCogisoftModel,
                        GetEmployeeDataResponseCogisoftModel>(cogisoftRequest);

                allRowsCollection.AddRange(
                    cogisoftResponse.GetUserDataRowCollection(_cogisoftEmployeeImportMappingConfiguration));

                anyObjectsLeft = cogisoftResponse.AnyRemainingObjectsLeft();

                cogisoftRequest.IncrementQueryIndex();
            } while (anyObjectsLeft);

            validRowsCollection = allRowsCollection.Where(row => row.buildStatus).ToList();

            var superiorMappingAttribute = _cogisoftEmployeeImportMappingConfiguration.PropertyMappings
                .FirstOrDefault(m =>
                    m.ExternalPropertyName.Equals(CogisoftEmployeeProperties.SuperiorLogin.ToString()));
            if (superiorMappingAttribute != null)
            {
                _logger.WriteLine(
                    "Superior/underling structure import requested, performing hierarchical import operation...");
                List<List<UserDataRowWithStatus>> hierarchicalRowStructure = new List<List<UserDataRowWithStatus>>();
                var loginMappingAttribute = _cogisoftEmployeeImportMappingConfiguration.PropertyMappings
                    .First(m => m.ExternalPropertyName.Equals(CogisoftEmployeeProperties.Email.ToString()));

                RemoveEmployeesWithNoSuperiors(validRowsCollection, loginMappingAttribute, superiorMappingAttribute);

                CycleDetector(validRowsCollection, loginMappingAttribute, superiorMappingAttribute);

                var highestLevelEmployees = validRowsCollection.Where(r =>
                    r.userDataRow[superiorMappingAttribute.EmploPropertyName]
                        .Equals(r.userDataRow[loginMappingAttribute.EmploPropertyName])).ToList();
                highestLevelEmployees.ForEach(r => r.userDataRow.Remove(superiorMappingAttribute.EmploPropertyName));
                highestLevelEmployees.ForEach(r => validRowsCollection.Remove(r));
                hierarchicalRowStructure.Add(highestLevelEmployees);

                while (validRowsCollection.Any())
                {
                    var nextEmployeeLevel = validRowsCollection
                        .Where(r => hierarchicalRowStructure.Last().Any(rs =>
                            rs.userDataRow[loginMappingAttribute.EmploPropertyName]
                                .Equals(r.userDataRow[superiorMappingAttribute.EmploPropertyName]))).ToList();
                    nextEmployeeLevel.ForEach(r => validRowsCollection.Remove(r));
                    hierarchicalRowStructure.Add(nextEmployeeLevel);
                }

                _logger.WriteLine(
                    "---------------------------------------------------------------------------------------------------------------");
                _logger.WriteLine(
                    "-----------------------------------------------IMPORT OPERATION------------------------------------------------");

                foreach (var hierarchyLevelToImport in hierarchicalRowStructure)
                {
                    _logger.WriteLine(
                        "---------------------------------------------------------------------------------------------------------------");

                    ImportUsersRequestModel importUsersRequestModel =
                        new ImportUsersRequestModel(importMode, requireRegistrationForNewEmployees)
                        {
                            Rows = hierarchyLevelToImport.Where(r => r.buildStatus).Select(r => r.userDataRow).ToList()
                        };

                    if (dryRun)
                    {
                        _logger.WriteLine($"#### Data that would be imported (hierarchy level {hierarchicalRowStructure.IndexOf(hierarchyLevelToImport) + 1}): ####");
                        importUsersRequestModel.Rows.ForEach(row =>
                            _logger.WriteLine(string.Join(" | ", row.Select(r => $"{r.Key}: {r.Value}"))));
                    }
                    else
                    {
                        var result = await _importLogic.ImportEmployees(importUsersRequestModel);
                        if (result == -1)
                        {
                            throw new Exception("An error has occurred during import");
                        }
                    }
                }
            }
            else
            {
                _logger.WriteLine(
                    "No superior/underling structure import requested, performing flat import operation...");

                ImportUsersRequestModel importUsersRequestModel =
                    new ImportUsersRequestModel(importMode, requireRegistrationForNewEmployees)
                    {
                        Rows = validRowsCollection.Select(r => r.userDataRow).ToList()
                    };

                if (dryRun)
                {
                    _logger.WriteLine("#### Data that would be imported: ####");
                    importUsersRequestModel.Rows.ForEach(row =>
                        _logger.WriteLine(string.Join(" | ", row.Select(r => $"{r.Key}: {r.Value}"))));
                }
                else
                {
                    var result = await _importLogic.ImportEmployees(importUsersRequestModel);
                    if (result == -1)
                    {
                        throw new Exception("An error has occurred during import");
                    }
                }
            }

            _logger.WriteLine(
                "---------------------------------------------------------------------------------------------------------------");
            _logger.WriteLine("#### Invalid data: ####");
            foreach (var reasonGroup in allRowsCollection.Where(row => !row.buildStatus).ToList().GroupBy(r => r.errorMessage).Select(g => new { reason = g.Key, rows = g.ToList()}))
            {
                _logger.WriteLine($"#### REASON: {reasonGroup.reason} ####");
                reasonGroup.rows.ForEach(row =>
                    _logger.WriteLine(
                        $"| {string.Join(" | ", row.userDataRow.Select(r => $"{r.Key}: {r.Value}"))}"));
            }
        }

        private void RemoveEmployeesWithNoSuperiors(List<UserDataRowWithStatus> rowsCollection, PropertyMapping loginMappingAttribute, PropertyMapping superiorMappingAttribute)
        {
            foreach (var rowToCheckForParent in rowsCollection)
            {
                if (!rowsCollection.Any(row =>
                    row.userDataRow[loginMappingAttribute.EmploPropertyName]
                        .Equals(rowToCheckForParent.userDataRow[superiorMappingAttribute.EmploPropertyName])))
                {
                    rowToCheckForParent.buildStatus = false;
                    rowToCheckForParent.AppendErrorMessage("Nie odnaleziono przełożonego");
                }
            }

            if (rowsCollection.RemoveAll(r => !r.buildStatus) > 0)
            {
                RemoveEmployeesWithNoSuperiors(rowsCollection, loginMappingAttribute, superiorMappingAttribute);
            }
        }

        private void CycleDetector(List<UserDataRowWithStatus> rowsCollection, PropertyMapping loginMappingAttribute,
            PropertyMapping superiorMappingAttribute)
        {
            foreach (var row in rowsCollection)
            {
                if (row.userDataRow[superiorMappingAttribute.EmploPropertyName]
                    .Equals(row.userDataRow[loginMappingAttribute.EmploPropertyName]))
                {
                    //Pracownicy nawyższego szczebla mają wpisanych samych siebie jako przełożonych, to jest poprawne.
                    continue;
                }
                
                List<UserDataRowWithStatus> employeeStructureGraph = new List<UserDataRowWithStatus>();
                employeeStructureGraph.Add(row);

                UserDataRowWithStatus employee = row;
                UserDataRowWithStatus superior;


                do
                {
                    //Wyciągamy następnego przełożonego dla pracownika. Powinien zawsze istnieć, bo ludzi bez przełożonych usuwaliśmy już wcześniej inną metodą.
                    superior = rowsCollection.First(r =>
                        r.userDataRow[loginMappingAttribute.EmploPropertyName]
                            .Equals(employee.userDataRow[superiorMappingAttribute.EmploPropertyName]));

                    if (employeeStructureGraph.Any(r =>
                        r.userDataRow[loginMappingAttribute.EmploPropertyName]
                            .Equals(superior.userDataRow[loginMappingAttribute.EmploPropertyName])))
                    {
                        var cycleErrorMessage =
                            $"Wykryto cykliczne przypisane przełożonych: {string.Join(" ==> ", employeeStructureGraph.Select(e => $"Login: {e.userDataRow[loginMappingAttribute.EmploPropertyName]}, Przełożony: {e.userDataRow[superiorMappingAttribute.EmploPropertyName]}"))}";

                        foreach (var cycleElement in employeeStructureGraph)
                        {
                            cycleElement.buildStatus = false;
                            cycleElement.AppendErrorMessage(cycleErrorMessage);
                        }

                        break;
                        //Jeśli jakikolwiek pracownik w grafie ma taki login, jak obecnie badany przełożony - to znaczy, że mamy cykl.
                    }

                    employeeStructureGraph.Add(superior);
                    employee = superior;
                } while (!superior.userDataRow[superiorMappingAttribute.EmploPropertyName]
                    .Equals(superior.userDataRow[loginMappingAttribute.EmploPropertyName]));

            }

            if (rowsCollection.Any(r => !r.buildStatus))
            {
                CycleDetector(rowsCollection.Where(r => r.buildStatus).ToList(), loginMappingAttribute, superiorMappingAttribute);
                rowsCollection.RemoveAll(r => !r.buildStatus);
            }
        }

        public async Task ImportEmployeeData(List<string> employeeIdsToImport = null)
        {
            try
            {
                using (var client = new CogisoftServiceClient(_logger))
                {
                    await ImportEmployeeDataInternal(client, employeeIdsToImport);
                }
            }
            catch (Exception e)
            {
                _logger.WriteLine($"An unexpected error has occurred, exception: {ExceptionLoggingUtils.ExceptionAsString(e)}", LogLevelEnum.Error);
            }
        }
    }
}