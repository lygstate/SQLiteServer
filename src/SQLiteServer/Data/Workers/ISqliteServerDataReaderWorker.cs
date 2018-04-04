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

namespace SQLiteServer.Data.Workers
{
  internal interface ISqliteServerDataReaderWorker
  {
    /// <summary>
    /// Get a value at a given colum name.
    /// </summary>
    /// <param name="i"></param>
    /// <returns></returns>
    object this[int i ] { get; }

    /// <summary>
    /// Get a value at a given name
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    object this[string name] { get; }

    /// <summary>
    /// Execute a 'read' operation
    /// </summary>
    /// <returns></returns>
    void ExecuteReader();

    /// <summary>
    /// Prepare to read the next value.
    /// </summary>
    /// <returns></returns>
    bool Read();

    /// <summary>
    /// Retrieves the column index given its name.
    /// </summary>
    /// <param name="name">The mae of the column.</param>
    /// <returns>int the index</returns>
    int GetOrdinal(string name);

    /// <summary>
    /// Retrieves the column as a string
    /// </summary>
    /// <param name="i">The index of the column.</param>
    /// <returns>string</returns>
    string GetString(int i);

    /// <summary>
    /// Retrieves the column as a short
    /// </summary>
    /// <param name="i">The index of the column.</param>
    /// <returns>short</returns>
    short GetInt16(int i);

    /// <summary>
    /// Retrieves the column as an int
    /// </summary>
    /// <param name="i">The index of the column.</param>
    /// <returns>int</returns>
    int GetInt32(int i);

    /// <summary>
    /// Retrieves the column as a long
    /// </summary>
    /// <param name="i">The index of the column.</param>
    /// <returns>long</returns>
    long GetInt64(int i);

    /// <summary>
    /// Retrieves the column as a double
    /// </summary>
    /// <param name="i">The index of the column.</param>
    /// <returns>double</returns>
    double GetDouble(int i );

    /// <summary>
    /// Get the field type for a given colum
    /// </summary>
    /// <param name="i">The index of the column.</param>
    /// <returns></returns>
    Type GetFieldType(int i);

    /// <summary>
    /// Get a raw value 
    /// </summary>
    /// <param name="i">The index of the column.</param>
    /// <returns></returns>
    object GetValue(int i);
  }
}