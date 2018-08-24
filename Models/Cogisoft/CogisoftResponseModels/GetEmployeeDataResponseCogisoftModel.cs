using System;
using System.Collections.Generic;
using System.Linq;
using CogisoftConnector.Logic;
using EmploApiSDK.ApiModels.Employees;

namespace CogisoftConnector.Models.Cogisoft.CogisoftResponseModels
{
    public enum CogisoftEmployeeProperties
    {
        NameId,
        FirstName,
        LastName,
        Email,
        Position,
        Unit,
        SuperiorLogin
    }

    public class UserDataRowWithStatus
    {
        public UserDataRow userDataRow;
        public bool buildStatus;
        public string errorMessage;

        public UserDataRowWithStatus()
        {
            userDataRow = new UserDataRow();
            buildStatus = true;
            errorMessage = string.Empty;
        }

        public void AppendErrorMessage(string message)
        {
            errorMessage = errorMessage.Equals(string.Empty) ? message : $"{errorMessage}, {message}";
        }
    }

    public class GetEmployeeDataResponseCogisoftModel
    {
        public class H
        {
            public List<string> c { get; set; }
        }

        public class R
        {
            public string bg { get; set; }
            public List<object> sc { get; set; }
        }

        public class P
        {
            public int i { get; set; }
            public int left { get; set; }
            public int off { get; set; }
            public List<R> r { get; set; }
        }

        public class Qr
        {
            public int trc { get; set; }
            public int v { get; set; }
            public H h { get; set; }
            public List<P> p { get; set; }
            public string qid { get; set; }
        }

        public string f { get; set; }
        public List<Qr> qr { get; set; }

        public List<UserDataRowWithStatus> GetUserDataRowCollection(CogisoftEmployeeImportConfiguration configuration)
        {
            return this.qr[0].p[0].r.Select(r => BuildUserDataRow(configuration, r))//.Where(r => !r.errorMessage.Contains("Pracownik zwolniony"))
                .ToList();
        }

        public bool AnyRemainingObjectsLeft()
        {
            return this.qr[0].p[0].left > 0;
        }

        private UserDataRowWithStatus BuildUserDataRow(CogisoftEmployeeImportConfiguration configuration, R row)
        {
            var userDataRowWithStatus = new UserDataRowWithStatus();

            var unit = GetOrganizationalUnit(row);

            //if (unit != null && unit.Contains("zwolnieni"))
            //{
            //    userDataRowWithStatus.buildStatus = false;
            //    userDataRowWithStatus.AppendErrorMessage("Pracownik zwolniony");
            //}

            var login = GetValueDefaultOperation(row, 4);
            if (login == null)
            {
                userDataRowWithStatus.buildStatus = false;
                userDataRowWithStatus.AppendErrorMessage("Brak loginu");
            }

            var superiorLogin = GetValueDefaultOperation(row, 8);
            if (superiorLogin == null)
            {
                userDataRowWithStatus.buildStatus = false;
                userDataRowWithStatus.AppendErrorMessage("Brak informacji o przełożonym");
            }

            foreach (var mapping in configuration.PropertyMappings)
            {
                
                switch ((CogisoftEmployeeProperties)Enum.Parse(typeof(CogisoftEmployeeProperties),
                    mapping.ExternalPropertyName))
                {
                    case CogisoftEmployeeProperties.NameId:
                        userDataRowWithStatus.userDataRow.Add(mapping.EmploPropertyName, GetValueDefaultOperation(row, 0));
                        break;
                    case CogisoftEmployeeProperties.FirstName:
                        userDataRowWithStatus.userDataRow.Add(mapping.EmploPropertyName, GetValueDefaultOperation(row, 2));
                        break;
                    case CogisoftEmployeeProperties.LastName:
                        userDataRowWithStatus.userDataRow.Add(mapping.EmploPropertyName, GetValueDefaultOperation(row, 3));
                        break;
                    case CogisoftEmployeeProperties.Email:
                        if (login != null)
                        {
                            userDataRowWithStatus.userDataRow.Add(mapping.EmploPropertyName,
                                GetValueDefaultOperation(row, 4));
                        }
                        break;
                    case CogisoftEmployeeProperties.Position:
                        userDataRowWithStatus.userDataRow.Add(mapping.EmploPropertyName, GetValueDefaultOperation(row, 5));
                        break;
                    case CogisoftEmployeeProperties.Unit:
                        if (unit != null)
                        {
                            userDataRowWithStatus.userDataRow.Add(mapping.EmploPropertyName, GetOrganizationalUnit(row));
                        }
                        break;
                    case CogisoftEmployeeProperties.SuperiorLogin:
                        userDataRowWithStatus.userDataRow.Add(mapping.EmploPropertyName, GetValueDefaultOperation(row, 8));
                        break;
                }
            }
            
            return userDataRowWithStatus;
        }

        private string NormalizeString(string @string)
        {
            return @string.Trim('\t', ' ');
        }

        private string GetValueDefaultOperation(GetEmployeeDataResponseCogisoftModel.R row, int index)
        {
            var value = row.sc[index].ToString();

            if (value.Trim().Replace("\r\n", "").Replace(" ", "").Equals(@"{""n"":""1""}"))
            {
                return null;
            }
            else
            {
                return NormalizeString(value);
            }
        }

        private string GetOrganizationalUnit(GetEmployeeDataResponseCogisoftModel.R row)
        {
            var symbol = row.sc[6].ToString();
            var name = row.sc[7].ToString();

            if (symbol.Trim().Replace("\r\n", "").Replace(" ", "").Equals(@"{""n"":""1""}"))
            {
                return null;
            }
            else if (name.Trim().Replace("\r\n", "").Replace(" ", "").Equals(@"{""n"":""1""}"))
            {
                return null;
            }
            else
            {
                return $"{NormalizeString(symbol)}, {NormalizeString(name)}";
            }
        }
    }
}