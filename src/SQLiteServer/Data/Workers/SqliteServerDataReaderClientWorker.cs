﻿//This file is part of SQLiteServer.
//
//    SQLiteServer is free software: you can redistribute it and/or modify
//    it under the terms of the GNU General Public License as published by
//    the Free Software Foundation, either version 3 of the License, or
//    (at your option) any later version.
//
//    SQLiteServer is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.See the
//    GNU General Public License for more details.
//
//    You should have received a copy of the GNU General Public License
//    along with SQLiteServer.  If not, see<https://www.gnu.org/licenses/gpl-3.0.en.html>.
using System;
using System.Collections.Generic;
using System.Text;
using SQLiteServer.Data.Connections;
using SQLiteServer.Data.Data;
using SQLiteServer.Data.Enums;
using SQLiteServer.Data.Exceptions;
using SQLiteServer.Fields;

namespace SQLiteServer.Data.Workers
{
  internal class SqliteServerDataReaderClientWorker : ISqliteServerDataReaderWorker
  {
    #region Private variables
    /// <summary>
    /// Save the ordinal values.
    /// </summary>
    private readonly Dictionary<string, int> _ordinals = new Dictionary<string, int>();

    /// <summary>
    /// Save the field types.
    /// </summary>
    private readonly Dictionary<int, Type> _fieldTypes = new Dictionary<int, Type>();

    /// <summary>
    /// The SQLite Connection
    /// </summary>
    private readonly ConnectionsController _controller;

    /// <summary>
    /// The command server guid.
    /// </summary>
    private readonly string _commandGuid;

    /// <summary>
    /// The max amount of time we will wait for a response from the server.
    /// </summary>
    private readonly int _queryTimeouts;
    #endregion

    public SqliteServerDataReaderClientWorker(ConnectionsController controller, string commandGuid, int queryTimeouts)
    {
      if (null == controller)
      {
        throw new ArgumentNullException( nameof(controller), "The controller cannot be null.");
      }
      _controller = controller;
      _commandGuid = commandGuid;
      _queryTimeouts = queryTimeouts;
    }

    public void ExecuteReader()
    {
      var response = _controller.SendAndWait(SQLiteMessage.ExecuteReaderRequest, Encoding.ASCII.GetBytes(_commandGuid), _queryTimeouts);
      if (null == response)
      {
        throw new TimeoutException("There was a timeout error executing the reader.");
      }

      switch (response.Type)
      {
        case SQLiteMessage.ExecuteReaderResponse:
          var result = response.Get<int>();
          if( 1 != result )
          {
            throw new SQLiteServerException($"Received an unexpected error {result} from the server.");
          }
          return;

        case SQLiteMessage.ExecuteReaderException:
          var error = response.Get<string>();
          throw new SQLiteServerException(error);

        default:
          throw new InvalidOperationException($"Unknown response {response.Type} from the server.");
      }
    }

    public bool Read()
    {
      var response = _controller.SendAndWait(SQLiteMessage.ExecuteReaderReadRequest, Encoding.ASCII.GetBytes(_commandGuid), _queryTimeouts);
      if (null == response)
      {
        throw new TimeoutException("There was a timeout error executing the read request from the reader.");
      }

      switch (response.Type)
      {
        case SQLiteMessage.ExecuteReaderResponse:
          return response.Get<int>() != 0;

        case SQLiteMessage.ExecuteReaderException:
          var error = response.Get<string>();
          throw new SQLiteServerException(error);

        default:
          throw new InvalidOperationException($"Unknown response {response.Type} from the server.");
      }
    }

    /// <inheritdoc />
    public object this[int ordinal] => GetValue(ordinal);

    /// <inheritdoc />
    public object this[string name] => GetValue(GetOrdinal(name));

    /// <inheritdoc />
    public int GetOrdinal(string name)
    {
      var lname = name.ToLower();
      if (_ordinals.ContainsKey(lname))
      {
        return _ordinals[lname];
      }
      // get the value
      var value = GetNamedValue<int>(SQLiteMessage.ExecuteReaderGetOrdinalRequest, lname);

      // save it.
      _ordinals[lname] = value;

      // return it.
      return value;
    }

    /// <summary>
    /// Get a value from the server by index.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="requestType"></param>
    /// <param name="index"></param>
    /// <returns></returns>
    private T GetIndexedValue<T>(SQLiteMessage requestType, int index)
    {
      var getValue = new IndexRequest()
      {
        Guid = _commandGuid,
        Index = index
      };
      var fields = Fields.Fields.SerializeObject(getValue);

      var response = _controller.SendAndWait(requestType, fields.Pack(), _queryTimeouts);
      if (null == response)
      {
        throw new TimeoutException("There was a timeout error executing the read request from the reader.");
      }

      switch (response.Type)
      {
        case SQLiteMessage.ExecuteReaderResponse:
          return response.Get<T>();

        case SQLiteMessage.ExecuteReaderException:
          var error = response.Get<string>();
          throw new SQLiteServerException(error);

        default:
          throw new InvalidOperationException($"Unknown response {response.Type} from the server.");
      }
    }

    /// <summary>
    /// Get a value from the server by name.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="requestType"></param>
    /// <param name="name"></param>
    /// <returns></returns>
    private T GetNamedValue<T>(SQLiteMessage requestType, string name )
    {
      var getValue = new NameRequest()
      {
        Guid = _commandGuid,
        Name = name
      };
      var fields = Fields.Fields.SerializeObject(getValue);

      var response = _controller.SendAndWait(requestType, fields.Pack(), _queryTimeouts);
      if (null == response)
      {
        throw new TimeoutException("There was a timeout error executing the read request from the reader.");
      }

      switch (response.Type)
      {
        case SQLiteMessage.ExecuteReaderResponse:
          return response.Get<T>();

        case SQLiteMessage.ExecuteReaderException:
          var error = response.Get<string>();
          throw new SQLiteServerException(error);

        default:
          throw new InvalidOperationException($"Unknown response {response.Type} from the server.");
      }
    }

    /// <inheritdoc />
    public short GetInt16(int i)
    {
      return GetIndexedValue<short>(SQLiteMessage.ExecuteReaderGetInt16Request, i);
    }

    /// <inheritdoc />
    public int GetInt32(int i)
    {
      return GetIndexedValue<int>( SQLiteMessage.ExecuteReaderGetInt32Request, i);
    }

    /// <inheritdoc />
    public long GetInt64(int i)
    {
      return GetIndexedValue<long>(SQLiteMessage.ExecuteReaderGetInt64Request, i);
    }

    /// <inheritdoc />
    public double GetDouble(int i)
    {
      return GetIndexedValue<double>(SQLiteMessage.ExecuteReaderGetDoubleRequest, i);
    }

    /// <inheritdoc />
    public string GetString(int i)
    {
      return GetIndexedValue<string>(SQLiteMessage.ExecuteReaderGetStringRequest, i);
    }

    /// <inheritdoc />
    public Type GetFieldType(int i)
    {
      if (_fieldTypes.ContainsKey(i))
      {
        return _fieldTypes[i];
      }

      // get the value
      var fieldType = (FieldType)GetIndexedValue<int>(SQLiteMessage.ExecuteReaderGetFieldTypeRequest, i);
      var systemType = Field.FieldTypeToType(fieldType);

      // save it
      _fieldTypes[i] = systemType;

      // return it.
      return systemType;
    }

    /// <inheritdoc />
    public object GetValue(int i)
    {
      // get the data type
      var type = GetFieldType(i);

      if (type == typeof(short))
      {
        return GetInt16(i);
      }

      if (type == typeof(int))
      {
        return GetInt32(i);
      }

      if (type == typeof(long))
      {
        return GetInt64(i);
      }

      if (type == typeof(double))
      {
        return GetDouble(i);
      }
      
      if (type == typeof(string))
      {
        return GetString(i);
      }

      // not yet supported
      // we need byte[], short as well as double
      throw new NotImplementedException();
    }
  }
}