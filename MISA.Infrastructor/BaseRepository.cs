﻿using Dapper;
using Microsoft.Extensions.Configuration;
using MISA.ApplicationCore.Interfaces;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace MISA.Infrastructor
{
    public class BaseRepository<T> : IBaseRepository<T>
    {
        #region Declare
        IConfiguration _configuration;
        string _connectionString = string.Empty;
        protected IDbConnection _dbConnection = null;
        protected string _tableName;
        #endregion

        #region Constructor
        public BaseRepository(IConfiguration configuration)
        {
            _configuration = configuration;
            _connectionString = _configuration.GetConnectionString("MISACukCukConnectionString");
            _dbConnection = new MySqlConnection(_connectionString);
            _tableName = typeof(T).Name;
        }
        #endregion

        #region Method

        public int Delete(Guid entityId)
        {
            // Thực thi truy vấn:
            var rowAffects = _dbConnection.Execute($"Proc_Delete{_tableName}ById", new { EmployeeId = entityId.ToString() }, commandType: CommandType.StoredProcedure);
            // Trả lại số dòng ảnh hưởng
            return rowAffects;
        }

        public IEnumerable<T> Get()
        {
            var entities = _dbConnection.Query<T>($"Proc_Get{_tableName}s", commandType: CommandType.StoredProcedure);
            return entities;
        }

        public T GetById(Guid entityId)
        {
            var obj = _dbConnection.Query<T>($"Proc_Get{_tableName}ById", param: entityId.ToString(), commandType: CommandType.StoredProcedure).FirstOrDefault();
            return obj;
        }

        public int Insert(T entity)
        {
            //Lấy param đã được Mapping dự liệu với DB
            var dynamicParameters = MappingDataType(entity);
            // Thực thi truy vấn: 
            var rowAffects = _dbConnection.Execute($"Proc_Insert{_tableName}", commandType: CommandType.StoredProcedure, param: dynamicParameters);
            // Trả về số dòng bị ảnh hưởng:
            return rowAffects;
        }

        public int Update(T entityId)
        {
            //Lấy param đã được Mapping dự liệu với DB
            var dynamicParameters = MappingDataType(entityId);
            // Thực thi truy vấn: 
            var rowAffects = _dbConnection.Execute($"Proc_Update{_tableName}", commandType: CommandType.StoredProcedure, param: dynamicParameters);
            // Trả về số dòng bị ảnh hưởng:
            return rowAffects;
        }

        private DynamicParameters MappingDataType(T obj)
        {
            var properties = obj.GetType().GetProperties();
            DynamicParameters dynamicParameters = new DynamicParameters();
            foreach (var property in properties)
            {
                var propertyName = property.Name;
                var propertyValue = property.GetValue(obj);

                if (property.PropertyType == typeof(Guid) || property.PropertyType == typeof(Guid?))
                {
                    propertyValue = property.GetValue(obj, null).ToString();
                }
                dynamicParameters.Add($"@{propertyName}", propertyValue);
            }
            return dynamicParameters;
        }

        public T GetEntityByProperty(string propertyName, object propertyValue)
        {
            var query = $"SELECT * FROM {_tableName} WHERE {propertyName} = '{propertyValue}'";
            var enity = _dbConnection.Query<T>(query, commandType:CommandType.Text).FirstOrDefault();
            return enity;
        }
        #endregion
    }
}
