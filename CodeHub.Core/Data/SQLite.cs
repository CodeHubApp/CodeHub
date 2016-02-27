//
// Copyright (c) 2009-2015 Krueger Systems, Inc.
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
//
#if WINDOWS_PHONE && !USE_WP8_NATIVE_SQLITE
#define USE_CSHARP_SQLITE
#endif

#if NETFX_CORE
#define USE_NEW_REFLECTION_API
#endif

using System;
using System.Diagnostics;
#if !USE_SQLITEPCL_RAW
using System.Runtime.InteropServices;
#endif
using System.Collections.Generic;
#if NO_CONCURRENT
using ConcurrentStringDictionary = System.Collections.Generic.Dictionary<string, object>;
using SQLite.Extensions;
#else
using ConcurrentStringDictionary = System.Collections.Concurrent.ConcurrentDictionary<string, object>;
#endif
using System.Reflection;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;

#if USE_CSHARP_SQLITE
using Sqlite3 = Community.CsharpSqlite.Sqlite3;
using Sqlite3DatabaseHandle = Community.CsharpSqlite.Sqlite3.sqlite3;
using Sqlite3Statement = Community.CsharpSqlite.Sqlite3.Vdbe;
#elif USE_WP8_NATIVE_SQLITE
using Sqlite3 = Sqlite.Sqlite3;
using Sqlite3DatabaseHandle = Sqlite.Database;
using Sqlite3Statement = Sqlite.Statement;
#elif USE_SQLITEPCL_RAW
using Sqlite3DatabaseHandle = SQLitePCL.sqlite3;
using Sqlite3Statement = SQLitePCL.sqlite3_stmt;
using Sqlite3 = SQLitePCL.raw;
#else
using Sqlite3DatabaseHandle = System.IntPtr;
using Sqlite3Statement = System.IntPtr;
#endif

namespace SQLite
{
public class SQLiteException : Exception
{
public SQLite3.Result Result { get; private set; }

protected SQLiteException (SQLite3.Result r,string message) : base(message)
{
Result = r;
}

public static SQLiteException New (SQLite3.Result r, string message)
{
return new SQLiteException (r, message);
}
}

public class NotNullConstraintViolationException : SQLiteException
{
public IEnumerable<TableMapping.Column> Columns { get; protected set; }

protected NotNullConstraintViolationException (SQLite3.Result r, string message)
: this (r, message, null, null)
{

}

protected NotNullConstraintViolationException (SQLite3.Result r, string message, TableMapping mapping, object obj)
: base (r, message)
{
if (mapping != null && obj != null) {
this.Columns = from c in mapping.Columns
where c.IsNullable == false && c.GetValue (obj) == null
select c;
}
}

public static new NotNullConstraintViolationException New (SQLite3.Result r, string message)
{
return new NotNullConstraintViolationException (r, message);
}

public static NotNullConstraintViolationException New (SQLite3.Result r, string message, TableMapping mapping, object obj)
{
return new NotNullConstraintViolationException (r, message, mapping, obj);
}

public static NotNullConstraintViolationException New (SQLiteException exception, TableMapping mapping, object obj)
{
return new NotNullConstraintViolationException (exception.Result, exception.Message, mapping, obj);
}
}

[Flags]
public enum SQLiteOpenFlags {
ReadOnly = 1, ReadWrite = 2, Create = 4,
NoMutex = 0x8000, FullMutex = 0x10000,
SharedCache = 0x20000, PrivateCache = 0x40000,
ProtectionComplete = 0x00100000,
ProtectionCompleteUnlessOpen = 0x00200000,
ProtectionCompleteUntilFirstUserAuthentication = 0x00300000,
ProtectionNone = 0x00400000
}

[Flags]
public enum CreateFlags
{
None                = 0x000,
ImplicitPK          = 0x001,    // create a primary key for field called 'Id' (Orm.ImplicitPkName)
ImplicitIndex       = 0x002,    // create an index for fields ending in 'Id' (Orm.ImplicitIndexSuffix)
AllImplicit         = 0x003,    // do both above
AutoIncPK           = 0x004,    // force PK field to be auto inc
FullTextSearch3     = 0x100,    // create virtual table using FTS3
FullTextSearch4     = 0x200     // create virtual table using FTS4
}

/// <summary>
/// Represents an open connection to a SQLite database.
/// </summary>
public partial class SQLiteConnection : IDisposable
{
private bool _open;
private TimeSpan _busyTimeout;
private Dictionary<string, TableMapping> _mappings = null;
private Dictionary<string, TableMapping> _tables = null;
private System.Diagnostics.Stopwatch _sw;
private long _elapsedMilliseconds = 0;

private int _transactionDepth = 0;
private Random _rand = new Random ();

public Sqlite3DatabaseHandle Handle { get; private set; }
internal static readonly Sqlite3DatabaseHandle NullHandle = default(Sqlite3DatabaseHandle);

public string DatabasePath { get; private set; }

public bool TimeExecution { get; set; }

public bool Trace { get; set; }

public bool StoreDateTimeAsTicks { get; private set; }

/// <summary>
/// Constructs a new SQLiteConnection and opens a SQLite database specified by databasePath.
/// </summary>
/// <param name="databasePath">
/// Specifies the path to the database file.
/// </param>
/// <param name="storeDateTimeAsTicks">
/// Specifies whether to store DateTime properties as ticks (true) or strings (false). You
/// absolutely do want to store them as Ticks in all new projects. The value of false is
/// only here for backwards compatibility. There is a *significant* speed advantage, with no
/// down sides, when setting storeDateTimeAsTicks = true.
/// If you use DateTimeOffset properties, it will be always stored as ticks regardingless
/// the storeDateTimeAsTicks parameter.
/// </param>
public SQLiteConnection (string databasePath, bool storeDateTimeAsTicks = true)
: this (databasePath, SQLiteOpenFlags.ReadWrite | SQLiteOpenFlags.Create, storeDateTimeAsTicks)
{
}

/// <summary>
/// Constructs a new SQLiteConnection and opens a SQLite database specified by databasePath.
/// </summary>
/// <param name="databasePath">
/// Specifies the path to the database file.
/// </param>
/// <param name="storeDateTimeAsTicks">
/// Specifies whether to store DateTime properties as ticks (true) or strings (false). You
/// absolutely do want to store them as Ticks in all new projects. The value of false is
/// only here for backwards compatibility. There is a *significant* speed advantage, with no
/// down sides, when setting storeDateTimeAsTicks = true.
/// If you use DateTimeOffset properties, it will be always stored as ticks regardingless
/// the storeDateTimeAsTicks parameter.
/// </param>
public SQLiteConnection (string databasePath, SQLiteOpenFlags openFlags, bool storeDateTimeAsTicks = true)
{
if (string.IsNullOrEmpty (databasePath))
throw new ArgumentException ("Must be specified", "databasePath");

DatabasePath = databasePath;

#if NETFX_CORE
SQLite3.SetDirectory(/*temp directory type*/2, Windows.Storage.ApplicationData.Current.TemporaryFolder.Path);
#endif

Sqlite3DatabaseHandle handle;

#if SILVERLIGHT || USE_CSHARP_SQLITE || USE_SQLITEPCL_RAW
var r = SQLite3.Open (databasePath, out handle, (int)openFlags, IntPtr.Zero);
#else
// open using the byte[]
// in the case where the path may include Unicode
// force open to using UTF-8 using sqlite3_open_v2
var databasePathAsBytes = GetNullTerminatedUtf8 (DatabasePath);
var r = SQLite3.Open (databasePathAsBytes, out handle, (int) openFlags, IntPtr.Zero);
#endif

Handle = handle;
if (r != SQLite3.Result.OK) {
throw SQLiteException.New (r, String.Format ("Could not open database file: {0} ({1})", DatabasePath, r));
}
_open = true;

StoreDateTimeAsTicks = storeDateTimeAsTicks;

BusyTimeout = TimeSpan.FromSeconds (0.1);
}

#if __IOS__
static SQLiteConnection ()
{
if (_preserveDuringLinkMagic) {
var ti = new ColumnInfo ();
ti.Name = "magic";
}
}

/// <summary>
/// Used to list some code that we want the MonoTouch linker
/// to see, but that we never want to actually execute.
/// </summary>
static bool _preserveDuringLinkMagic;
#endif

#if !USE_SQLITEPCL_RAW
public void EnableLoadExtension(int onoff)
{
SQLite3.Result r = SQLite3.EnableLoadExtension(Handle, onoff);
if (r != SQLite3.Result.OK) {
string msg = SQLite3.GetErrmsg (Handle);
throw SQLiteException.New (r, msg);
}
}
#endif

#if !USE_SQLITEPCL_RAW
static byte[] GetNullTerminatedUtf8 (string s)
{
    var utf8Length = System.Text.Encoding.UTF8.GetByteCount (s);
    var bytes = new byte [utf8Length + 1];
    utf8Length = System.Text.Encoding.UTF8.GetBytes(s, 0, s.Length, bytes, 0);
    return bytes;
}
#endif

/// <summary>
/// Sets a busy handler to sleep the specified amount of time when a table is locked.
/// The handler will sleep multiple times until a total time of <see cref="BusyTimeout"/> has accumulated.
/// </summary>
public TimeSpan BusyTimeout {
    get { return _busyTimeout; }
    set {
        _busyTimeout = value;
        if (Handle != NullHandle) {
            SQLite3.BusyTimeout (Handle, (int)_busyTimeout.TotalMilliseconds);
        }
    }
}

/// <summary>
/// Returns the mappings from types to tables that the connection
/// currently understands.
/// </summary>
public IEnumerable<TableMapping> TableMappings {
    get {
        return _tables != null ? _tables.Values : Enumerable.Empty<TableMapping> ();
    }
}

/// <summary>
/// Retrieves the mapping that is automatically generated for the given type.
/// </summary>
/// <param name="type">
/// The type whose mapping to the database is returned.
/// </param>         
/// <param name="createFlags">
/// Optional flags allowing implicit PK and indexes based on naming conventions
/// </param>     
/// <returns>
/// The mapping represents the schema of the columns of the database and contains 
/// methods to set and get properties of objects.
/// </returns>
public TableMapping GetMapping(Type type, CreateFlags createFlags = CreateFlags.None)
{
    if (_mappings == null) {
        _mappings = new Dictionary<string, TableMapping> ();
    }
    TableMapping map;
    if (!_mappings.TryGetValue (type.FullName, out map)) {
        map = new TableMapping (type, createFlags);
        _mappings [type.FullName] = map;
    }
    return map;
}

/// <summary>
/// Retrieves the mapping that is automatically generated for the given type.
/// </summary>
/// <returns>
/// The mapping represents the schema of the columns of the database and contains 
/// methods to set and get properties of objects.
/// </returns>
public TableMapping GetMapping<T> ()
{
    return GetMapping (typeof (T));
}

private struct IndexedColumn
{
    public int Order;
    public string ColumnName;
}

private struct IndexInfo
{
    public string IndexName;
    public string TableName;
    public bool Unique;
    public List<IndexedColumn> Columns;
}

/// <summary>
/// Executes a "drop table" on the database.  This is non-recoverable.
/// </summary>
public int DropTable<T>()
{
    var map = GetMapping (typeof (T));

    var query = string.Format("drop table if exists \"{0}\"", map.TableName);

    return Execute (query);
}

/// <summary>
/// Executes a "create table if not exists" on the database. It also
/// creates any specified indexes on the columns of the table. It uses
/// a schema automatically generated from the specified type. You can
/// later access this schema by calling GetMapping.
/// </summary>
/// <returns>
/// The number of entries added to the database schema.
/// </returns>
public int CreateTable<T>(CreateFlags createFlags = CreateFlags.None)
{
    return CreateTable(typeof (T), createFlags);
}

/// <summary>
/// Executes a "create table if not exists" on the database. It also
/// creates any specified indexes on the columns of the table. It uses
/// a schema automatically generated from the specified type. You can
/// later access this schema by calling GetMapping.
/// </summary>
/// <param name="ty">Type to reflect to a database table.</param>
/// <param name="createFlags">Optional flags allowing implicit PK and indexes based on naming conventions.</param>  
/// <returns>
/// The number of entries added to the database schema.
/// </returns>
public int CreateTable(Type ty, CreateFlags createFlags = CreateFlags.None)
{
    if (_tables == null) {
        _tables = new Dictionary<string, TableMapping> ();
    }
    TableMapping map;
    if (!_tables.TryGetValue (ty.FullName, out map)) {
        map = GetMapping (ty, createFlags);
        _tables.Add (ty.FullName, map);
    }

    // Present a nice error if no columns specified
    if (map.Columns.Length == 0) {
        throw new Exception (string.Format ("Cannot create a table with zero columns (does '{0}' have public properties?)", ty.FullName));
    }

    // Facilitate virtual tables a.k.a. full-text search.
    bool fts3 = (createFlags & CreateFlags.FullTextSearch3) != 0;
    bool fts4 = (createFlags & CreateFlags.FullTextSearch4) != 0;
    bool fts = fts3 || fts4;
    var @virtual = fts ? "virtual " : string.Empty;
    var @using = fts3 ? "using fts3 " : fts4 ? "using fts4 " : string.Empty;

    // Build query.
    var query = "create " + @virtual + "table if not exists \"" + map.TableName + "\" " + @using + "(\n";
        var decls = map.Columns.Select (p => Orm.SqlDecl (p, StoreDateTimeAsTicks));
    var decl = string.Join (",\n", decls.ToArray ());
    query += decl;
    query += ")";

    var count = Execute (query);

    if (count == 0) { //Possible bug: This always seems to return 0?
        // Table already exists, migrate it
        MigrateTable (map);
    }

    var indexes = new Dictionary<string, IndexInfo> ();
    foreach (var c in map.Columns) {
        foreach (var i in c.Indices) {
            var iname = i.Name ?? map.TableName + "_" + c.Name;
            IndexInfo iinfo;
            if (!indexes.TryGetValue (iname, out iinfo)) {
                iinfo = new IndexInfo {
                    IndexName = iname,
                    TableName = map.TableName,
                    Unique = i.Unique,
                    Columns = new List<IndexedColumn> ()
                };
                indexes.Add (iname, iinfo);
            }

            if (i.Unique != iinfo.Unique)
                throw new Exception ("All the columns in an index must have the same value for their Unique property");

            iinfo.Columns.Add (new IndexedColumn {
                Order = i.Order,
                ColumnName = c.Name
            });
        }
    }

    foreach (var indexName in indexes.Keys) {
        var index = indexes[indexName];
        var columns = index.Columns.OrderBy(i => i.Order).Select(i => i.ColumnName).ToArray();
        count += CreateIndex(indexName, index.TableName, columns, index.Unique);
    }

    return count;
}

/// <summary>
/// Creates an index for the specified table and columns.
/// </summary>
/// <param name="indexName">Name of the index to create</param>
/// <param name="tableName">Name of the database table</param>
/// <param name="columnNames">An array of column names to index</param>
/// <param name="unique">Whether the index should be unique</param>
public int CreateIndex(string indexName, string tableName, string[] columnNames, bool unique = false)
{
    const string sqlFormat = "create {2} index if not exists \"{3}\" on \"{0}\"(\"{1}\")";
    var sql = String.Format(sqlFormat, tableName, string.Join ("\", \"", columnNames), unique ? "unique" : "", indexName);
    return Execute(sql);
}

/// <summary>
/// Creates an index for the specified table and column.
/// </summary>
/// <param name="indexName">Name of the index to create</param>
/// <param name="tableName">Name of the database table</param>
/// <param name="columnName">Name of the column to index</param>
/// <param name="unique">Whether the index should be unique</param>
public int CreateIndex(string indexName, string tableName, string columnName, bool unique = false)
{
    return CreateIndex(indexName, tableName, new string[] { columnName }, unique);
}

/// <summary>
/// Creates an index for the specified table and column.
/// </summary>
/// <param name="tableName">Name of the database table</param>
/// <param name="columnName">Name of the column to index</param>
/// <param name="unique">Whether the index should be unique</param>
public int CreateIndex(string tableName, string columnName, bool unique = false)
{
    return CreateIndex(tableName + "_" + columnName, tableName, columnName, unique);
}

/// <summary>
/// Creates an index for the specified table and columns.
/// </summary>
/// <param name="tableName">Name of the database table</param>
/// <param name="columnNames">An array of column names to index</param>
/// <param name="unique">Whether the index should be unique</param>
public int CreateIndex(string tableName, string[] columnNames, bool unique = false)
{
    return CreateIndex(tableName + "_" + string.Join ("_", columnNames), tableName, columnNames, unique);
}

/// <summary>
/// Creates an index for the specified object property.
/// e.g. CreateIndex<Client>(c => c.Name);
/// </summary>
/// <typeparam name="T">Type to reflect to a database table.</typeparam>
/// <param name="property">Property to index</param>
/// <param name="unique">Whether the index should be unique</param>
public void CreateIndex<T>(Expression<Func<T, object>> property, bool unique = false)
{
    MemberExpression mx;
    if (property.Body.NodeType == ExpressionType.Convert)
    {
        mx = ((UnaryExpression)property.Body).Operand as MemberExpression;
    }
    else
    {
        mx= (property.Body as MemberExpression);
    }
    var propertyInfo = mx.Member as PropertyInfo;
    if (propertyInfo == null)
    {
        throw new ArgumentException("The lambda expression 'property' should point to a valid Property");
    }

    var propName = propertyInfo.Name;

    var map = GetMapping<T>();
    var colName = map.FindColumnWithPropertyName(propName).Name;

    CreateIndex(map.TableName, colName, unique);
}

public class ColumnInfo
{
    //          public int cid { get; set; }

    [Column ("name")]
    public string Name { get; set; }

    //          [Column ("type")]
    //          public string ColumnType { get; set; }

    public int notnull { get; set; }

    //          public string dflt_value { get; set; }

    //          public int pk { get; set; }

    public override string ToString ()
    {
        return Name;
    }
}

public List<ColumnInfo> GetTableInfo (string tableName)
{
    var query = "pragma table_info(\"" + tableName + "\")";         
    return Query<ColumnInfo> (query);
}

void MigrateTable (TableMapping map)
{
    var existingCols = GetTableInfo (map.TableName);

    var toBeAdded = new List<TableMapping.Column> ();

    foreach (var p in map.Columns) {
        var found = false;
        foreach (var c in existingCols) {
            found = (string.Compare (p.Name, c.Name, StringComparison.OrdinalIgnoreCase) == 0);
            if (found)
                break;
        }
        if (!found) {
            toBeAdded.Add (p);
        }
    }

    foreach (var p in toBeAdded) {
        var addCol = "alter table \"" + map.TableName + "\" add column " + Orm.SqlDecl (p, StoreDateTimeAsTicks);
        Execute (addCol);
    }
}

/// <summary>
/// Creates a new SQLiteCommand. Can be overridden to provide a sub-class.
/// </summary>
/// <seealso cref="SQLiteCommand.OnInstanceCreated"/>
protected virtual SQLiteCommand NewCommand ()
{
    return new SQLiteCommand (this);
}

/// <summary>
/// Creates a new SQLiteCommand given the command text with arguments. Place a '?'
/// in the command text for each of the arguments.
/// </summary>
/// <param name="cmdText">
/// The fully escaped SQL.
/// </param>
/// <param name="args">
/// Arguments to substitute for the occurences of '?' in the command text.
/// </param>
/// <returns>
/// A <see cref="SQLiteCommand"/>
/// </returns>
public SQLiteCommand CreateCommand (string cmdText, params object[] ps)
{
    if (!_open)
        throw SQLiteException.New (SQLite3.Result.Error, "Cannot create commands from unopened database");

    var cmd = NewCommand ();
    cmd.CommandText = cmdText;
    foreach (var o in ps) {
        cmd.Bind (o);
    }
    return cmd;
}

/// <summary>
/// Creates a SQLiteCommand given the command text (SQL) with arguments. Place a '?'
/// in the command text for each of the arguments and then executes that command.
/// Use this method instead of Query when you don't expect rows back. Such cases include
/// INSERTs, UPDATEs, and DELETEs.
/// You can set the Trace or TimeExecution properties of the connection
/// to profile execution.
/// </summary>
/// <param name="query">
/// The fully escaped SQL.
/// </param>
/// <param name="args">
/// Arguments to substitute for the occurences of '?' in the query.
/// </param>
/// <returns>
/// The number of rows modified in the database as a result of this execution.
/// </returns>
public int Execute (string query, params object[] args)
{
    var cmd = CreateCommand (query, args);

    if (TimeExecution) {
        if (_sw == null) {
            _sw = new Stopwatch ();
        }
        _sw.Reset ();
        _sw.Start ();
    }

    var r = cmd.ExecuteNonQuery ();

    if (TimeExecution) {
        _sw.Stop ();
        _elapsedMilliseconds += _sw.ElapsedMilliseconds;
        Debug.WriteLine (string.Format ("Finished in {0} ms ({1:0.0} s total)", _sw.ElapsedMilliseconds, _elapsedMilliseconds / 1000.0));
    }

    return r;
}

public T ExecuteScalar<T> (string query, params object[] args)
{
    var cmd = CreateCommand (query, args);

    if (TimeExecution) {
        if (_sw == null) {
            _sw = new Stopwatch ();
        }
        _sw.Reset ();
        _sw.Start ();
    }

    var r = cmd.ExecuteScalar<T> ();

    if (TimeExecution) {
        _sw.Stop ();
        _elapsedMilliseconds += _sw.ElapsedMilliseconds;
        Debug.WriteLine (string.Format ("Finished in {0} ms ({1:0.0} s total)", _sw.ElapsedMilliseconds, _elapsedMilliseconds / 1000.0));
    }

    return r;
}

/// <summary>
/// Creates a SQLiteCommand given the command text (SQL) with arguments. Place a '?'
/// in the command text for each of the arguments and then executes that command.
/// It returns each row of the result using the mapping automatically generated for
/// the given type.
/// </summary>
/// <param name="query">
/// The fully escaped SQL.
/// </param>
/// <param name="args">
/// Arguments to substitute for the occurences of '?' in the query.
/// </param>
/// <returns>
/// An enumerable with one result for each row returned by the query.
/// </returns>
public List<T> Query<T> (string query, params object[] args) where T : new()
{
    var cmd = CreateCommand (query, args);
    return cmd.ExecuteQuery<T> ();
}

/// <summary>
/// Creates a SQLiteCommand given the command text (SQL) with arguments. Place a '?'
/// in the command text for each of the arguments and then executes that command.
/// It returns each row of the result using the mapping automatically generated for
/// the given type.
/// </summary>
/// <param name="query">
/// The fully escaped SQL.
/// </param>
/// <param name="args">
/// Arguments to substitute for the occurences of '?' in the query.
/// </param>
/// <returns>
/// An enumerable with one result for each row returned by the query.
/// The enumerator will call sqlite3_step on each call to MoveNext, so the database
/// connection must remain open for the lifetime of the enumerator.
/// </returns>
public IEnumerable<T> DeferredQuery<T>(string query, params object[] args) where T : new()
{
    var cmd = CreateCommand(query, args);
    return cmd.ExecuteDeferredQuery<T>();
}

/// <summary>
/// Creates a SQLiteCommand given the command text (SQL) with arguments. Place a '?'
/// in the command text for each of the arguments and then executes that command.
/// It returns each row of the result using the specified mapping. This function is
/// only used by libraries in order to query the database via introspection. It is
/// normally not used.
/// </summary>
/// <param name="map">
/// A <see cref="TableMapping"/> to use to convert the resulting rows
/// into objects.
/// </param>
/// <param name="query">
/// The fully escaped SQL.
/// </param>
/// <param name="args">
/// Arguments to substitute for the occurences of '?' in the query.
/// </param>
/// <returns>
/// An enumerable with one result for each row returned by the query.
/// </returns>
public List<object> Query (TableMapping map, string query, params object[] args)
{
    var cmd = CreateCommand (query, args);
    return cmd.ExecuteQuery<object> (map);
}

/// <summary>
/// Creates a SQLiteCommand given the command text (SQL) with arguments. Place a '?'
/// in the command text for each of the arguments and then executes that command.
/// It returns each row of the result using the specified mapping. This function is
/// only used by libraries in order to query the database via introspection. It is
/// normally not used.
/// </summary>
/// <param name="map">
/// A <see cref="TableMapping"/> to use to convert the resulting rows
/// into objects.
/// </param>
/// <param name="query">
/// The fully escaped SQL.
/// </param>
/// <param name="args">
/// Arguments to substitute for the occurences of '?' in the query.
/// </param>
/// <returns>
/// An enumerable with one result for each row returned by the query.
/// The enumerator will call sqlite3_step on each call to MoveNext, so the database
/// connection must remain open for the lifetime of the enumerator.
/// </returns>
public IEnumerable<object> DeferredQuery(TableMapping map, string query, params object[] args)
{
    var cmd = CreateCommand(query, args);
    return cmd.ExecuteDeferredQuery<object>(map);
}

/// <summary>
/// Returns a queryable interface to the table represented by the given type.
/// </summary>
/// <returns>
/// A queryable object that is able to translate Where, OrderBy, and Take
/// queries into native SQL.
/// </returns>
public TableQuery<T> Table<T> () where T : new()
{
    return new TableQuery<T> (this);
}

/// <summary>
/// Attempts to retrieve an object with the given primary key from the table
/// associated with the specified type. Use of this method requires that
/// the given type have a designated PrimaryKey (using the PrimaryKeyAttribute).
/// </summary>
/// <param name="pk">
/// The primary key.
/// </param>
/// <returns>
/// The object with the given primary key. Throws a not found exception
/// if the object is not found.
/// </returns>
public T Get<T> (object pk) where T : new()
{
    var map = GetMapping (typeof(T));
    return Query<T> (map.GetByPrimaryKeySql, pk).First ();
}

/// <summary>
/// Attempts to retrieve the first object that matches the predicate from the table
/// associated with the specified type. 
/// </summary>
/// <param name="predicate">
/// A predicate for which object to find.
/// </param>
/// <returns>
/// The object that matches the given predicate. Throws a not found exception
/// if the object is not found.
/// </returns>
public T Get<T> (Expression<Func<T, bool>> predicate) where T : new()
{
    return Table<T> ().Where (predicate).First ();
}

/// <summary>
/// Attempts to retrieve an object with the given primary key from the table
/// associated with the specified type. Use of this method requires that
/// the given type have a designated PrimaryKey (using the PrimaryKeyAttribute).
/// </summary>
/// <param name="pk">
/// The primary key.
/// </param>
/// <returns>
/// The object with the given primary key or null
/// if the object is not found.
/// </returns>
public T Find<T> (object pk) where T : new ()
{
    var map = GetMapping (typeof (T));
    return Query<T> (map.GetByPrimaryKeySql, pk).FirstOrDefault ();
}

/// <summary>
/// Attempts to retrieve an object with the given primary key from the table
/// associated with the specified type. Use of this method requires that
/// the given type have a designated PrimaryKey (using the PrimaryKeyAttribute).
/// </summary>
/// <param name="pk">
/// The primary key.
/// </param>
/// <param name="map">
/// The TableMapping used to identify the object type.
/// </param>
/// <returns>
/// The object with the given primary key or null
/// if the object is not found.
/// </returns>
public object Find (object pk, TableMapping map)
{
    return Query (map, map.GetByPrimaryKeySql, pk).FirstOrDefault ();
}

/// <summary>
/// Attempts to retrieve the first object that matches the predicate from the table
/// associated with the specified type. 
/// </summary>
/// <param name="predicate">
/// A predicate for which object to find.
/// </param>
/// <returns>
/// The object that matches the given predicate or null
/// if the object is not found.
/// </returns>
public T Find<T> (Expression<Func<T, bool>> predicate) where T : new()
{
    return Table<T> ().Where (predicate).FirstOrDefault ();
}

/// <summary>
/// Attempts to retrieve the first object that matches the query from the table
/// associated with the specified type. 
/// </summary>
/// <param name="query">
/// The fully escaped SQL.
/// </param>
/// <param name="args">
/// Arguments to substitute for the occurences of '?' in the query.
/// </param>
/// <returns>
/// The object that matches the given predicate or null
/// if the object is not found.
/// </returns>
public T FindWithQuery<T> (string query, params object[] args) where T : new()
{
    return Query<T> (query, args).FirstOrDefault ();
}

/// <summary>
/// Whether <see cref="BeginTransaction"/> has been called and the database is waiting for a <see cref="Commit"/>.
/// </summary>
public bool IsInTransaction {
    get { return _transactionDepth > 0; }
}

/// <summary>
/// Begins a new transaction. Call <see cref="Commit"/> to end the transaction.
/// </summary>
/// <example cref="System.InvalidOperationException">Throws if a transaction has already begun.</example>
public void BeginTransaction ()
{
    // The BEGIN command only works if the transaction stack is empty, 
    //    or in other words if there are no pending transactions. 
    // If the transaction stack is not empty when the BEGIN command is invoked, 
    //    then the command fails with an error.
    // Rather than crash with an error, we will just ignore calls to BeginTransaction
    //    that would result in an error.
    if (Interlocked.CompareExchange (ref _transactionDepth, 1, 0) == 0) {
        try {
            Execute ("begin transaction");
        } catch (Exception ex) {
            var sqlExp = ex as SQLiteException;
            if (sqlExp != null) {
                // It is recommended that applications respond to the errors listed below 
                //    by explicitly issuing a ROLLBACK command.
                // TODO: This rollback failsafe should be localized to all throw sites.
                switch (sqlExp.Result) {
                    case SQLite3.Result.IOError:
                    case SQLite3.Result.Full:
                    case SQLite3.Result.Busy:
                    case SQLite3.Result.NoMem:
                    case SQLite3.Result.Interrupt:
                        RollbackTo (null, true);
                        break;
                }
            } else {
                // Call decrement and not VolatileWrite in case we've already 
                //    created a transaction point in SaveTransactionPoint since the catch.
                Interlocked.Decrement (ref _transactionDepth);
            }

            throw;
        }
    } else { 
        // Calling BeginTransaction on an already open transaction is invalid
        throw new InvalidOperationException ("Cannot begin a transaction while already in a transaction.");
    }
}

/// <summary>
/// Creates a savepoint in the database at the current point in the transaction timeline.
/// Begins a new transaction if one is not in progress.
/// 
/// Call <see cref="RollbackTo"/> to undo transactions since the returned savepoint.
/// Call <see cref="Release"/> to commit transactions after the savepoint returned here.
/// Call <see cref="Commit"/> to end the transaction, committing all changes.
/// </summary>
/// <returns>A string naming the savepoint.</returns>
public string SaveTransactionPoint ()
{
    int depth = Interlocked.Increment (ref _transactionDepth) - 1;
    string retVal = "S" + _rand.Next (short.MaxValue) + "D" + depth;

    try {
        Execute ("savepoint " + retVal);
    } catch (Exception ex) {
        var sqlExp = ex as SQLiteException;
        if (sqlExp != null) {
            // It is recommended that applications respond to the errors listed below 
            //    by explicitly issuing a ROLLBACK command.
            // TODO: This rollback failsafe should be localized to all throw sites.
            switch (sqlExp.Result) {
                case SQLite3.Result.IOError:
                case SQLite3.Result.Full:
                case SQLite3.Result.Busy:
                case SQLite3.Result.NoMem:
                case SQLite3.Result.Interrupt:
                    RollbackTo (null, true);
                    break;
            }
        } else {
            Interlocked.Decrement (ref _transactionDepth);
        }

        throw;
    }

    return retVal;
}

/// <summary>
/// Rolls back the transaction that was begun by <see cref="BeginTransaction"/> or <see cref="SaveTransactionPoint"/>.
/// </summary>
public void Rollback ()
{
    RollbackTo (null, false);
}

/// <summary>
/// Rolls back the savepoint created by <see cref="BeginTransaction"/> or SaveTransactionPoint.
/// </summary>
/// <param name="savepoint">The name of the savepoint to roll back to, as returned by <see cref="SaveTransactionPoint"/>.  If savepoint is null or empty, this method is equivalent to a call to <see cref="Rollback"/></param>
public void RollbackTo (string savepoint)
{
    RollbackTo (savepoint, false);
}

/// <summary>
/// Rolls back the transaction that was begun by <see cref="BeginTransaction"/>.
/// </summary>
/// <param name="noThrow">true to avoid throwing exceptions, false otherwise</param>
void RollbackTo (string savepoint, bool noThrow)
{
    // Rolling back without a TO clause rolls backs all transactions 
    //    and leaves the transaction stack empty.   
    try {
        if (String.IsNullOrEmpty (savepoint)) {
            if (Interlocked.Exchange (ref _transactionDepth, 0) > 0) {
                Execute ("rollback");
            }
        } else {
            DoSavePointExecute (savepoint, "rollback to ");
        }   
    } catch (SQLiteException) {
        if (!noThrow)
            throw;

    }
    // No need to rollback if there are no transactions open.
}

/// <summary>
/// Releases a savepoint returned from <see cref="SaveTransactionPoint"/>.  Releasing a savepoint 
///    makes changes since that savepoint permanent if the savepoint began the transaction,
///    or otherwise the changes are permanent pending a call to <see cref="Commit"/>.
/// 
/// The RELEASE command is like a COMMIT for a SAVEPOINT.
/// </summary>
/// <param name="savepoint">The name of the savepoint to release.  The string should be the result of a call to <see cref="SaveTransactionPoint"/></param>
public void Release (string savepoint)
{
    DoSavePointExecute (savepoint, "release ");
}

void DoSavePointExecute (string savepoint, string cmd)
{
    // Validate the savepoint
    int firstLen = savepoint.IndexOf ('D');
    if (firstLen >= 2 && savepoint.Length > firstLen + 1) {
        int depth;
        if (Int32.TryParse (savepoint.Substring (firstLen + 1), out depth)) {
            // TODO: Mild race here, but inescapable without locking almost everywhere.
            if (0 <= depth && depth < _transactionDepth) {
                #if NETFX_CORE || USE_SQLITEPCL_RAW
                Volatile.Write (ref _transactionDepth, depth);
                #elif SILVERLIGHT
                _transactionDepth = depth;
                #else
                Thread.VolatileWrite (ref _transactionDepth, depth);
                #endif
                Execute (cmd + savepoint);
                return;
            }
        }
    }

    throw new ArgumentException ("savePoint is not valid, and should be the result of a call to SaveTransactionPoint.", "savePoint");
}

/// <summary>
/// Commits the transaction that was begun by <see cref="BeginTransaction"/>.
/// </summary>
public void Commit ()
{
    if (Interlocked.Exchange (ref _transactionDepth, 0) != 0) {
        Execute ("commit");
    }
    // Do nothing on a commit with no open transaction
}

/// <summary>
/// Executes <param name="action"> within a (possibly nested) transaction by wrapping it in a SAVEPOINT. If an
/// exception occurs the whole transaction is rolled back, not just the current savepoint. The exception
/// is rethrown.
/// </summary>
/// <param name="action">
/// The <see cref="Action"/> to perform within a transaction. <param name="action"> can contain any number
/// of operations on the connection but should never call <see cref="BeginTransaction"/> or
/// <see cref="Commit"/>.
/// </param>
public void RunInTransaction (Action action)
{
    try {
        var savePoint = SaveTransactionPoint ();
        action ();
        Release (savePoint);
    } catch (Exception) {
        Rollback ();
        throw;
    }
}

/// <summary>
/// Inserts all specified objects.
/// </summary>
/// <param name="objects">
/// An <see cref="IEnumerable"/> of the objects to insert.
/// <param name="runInTransaction"/>
/// A boolean indicating if the inserts should be wrapped in a transaction.
/// </param>
/// <returns>
/// The number of rows added to the table.
/// </returns>
public int InsertAll (System.Collections.IEnumerable objects, bool runInTransaction=true)
{
    var c = 0;
    if (runInTransaction) {
        RunInTransaction(() => {
            foreach (var r in objects) {
                c += Insert (r);
            }
        });
    }
    else {
        foreach (var r in objects) {
            c += Insert (r);
        }
    }
    return c;
}

/// <summary>
/// Inserts all specified objects.
/// </summary>
/// <param name="objects">
/// An <see cref="IEnumerable"/> of the objects to insert.
/// </param>
/// <param name="extra">
/// Literal SQL code that gets placed into the command. INSERT {extra} INTO ...
/// </param>
/// <param name="runInTransaction"/>
/// A boolean indicating if the inserts should be wrapped in a transaction.
/// </param>
/// <returns>
/// The number of rows added to the table.
/// </returns>
public int InsertAll (System.Collections.IEnumerable objects, string extra, bool runInTransaction=true)
{
    var c = 0;
    if (runInTransaction) {
        RunInTransaction (() => {
            foreach (var r in objects) {
                c += Insert (r, extra);
            }
        });
    }
    else {
        foreach (var r in objects) {
            c+= Insert (r);
        }
    }
    return c;
}

/// <summary>
/// Inserts all specified objects.
/// </summary>
/// <param name="objects">
/// An <see cref="IEnumerable"/> of the objects to insert.
/// </param>
/// <param name="objType">
/// The type of object to insert.
/// </param>
/// <param name="runInTransaction"/>
/// A boolean indicating if the inserts should be wrapped in a transaction.
/// </param>
/// <returns>
/// The number of rows added to the table.
/// </returns>
public int InsertAll (System.Collections.IEnumerable objects, Type objType, bool runInTransaction=true)
{
    var c = 0;
    if (runInTransaction) {
        RunInTransaction (() => {
            foreach (var r in objects) {
                c += Insert (r, objType);
            }
        });
    }
    else {
        foreach (var r in objects) {
            c += Insert (r, objType);
        }
    }
    return c;
}

/// <summary>
/// Inserts the given object and retrieves its
/// auto incremented primary key if it has one.
/// </summary>
/// <param name="obj">
/// The object to insert.
/// </param>
/// <returns>
/// The number of rows added to the table.
/// </returns>
public int Insert (object obj)
{
    if (obj == null) {
        return 0;
    }
    return Insert (obj, "", obj.GetType ());
}

/// <summary>
/// Inserts the given object and retrieves its
/// auto incremented primary key if it has one.
/// If a UNIQUE constraint violation occurs with
/// some pre-existing object, this function deletes
/// the old object.
/// </summary>
/// <param name="obj">
/// The object to insert.
/// </param>
/// <returns>
/// The number of rows modified.
/// </returns>
public int InsertOrReplace (object obj)
{
    if (obj == null) {
        return 0;
    }
    return Insert (obj, "OR REPLACE", obj.GetType ());
}

/// <summary>
/// Inserts the given object and retrieves its
/// auto incremented primary key if it has one.
/// </summary>
/// <param name="obj">
/// The object to insert.
/// </param>
/// <param name="objType">
/// The type of object to insert.
/// </param>
/// <returns>
/// The number of rows added to the table.
/// </returns>
public int Insert (object obj, Type objType)
{
    return Insert (obj, "", objType);
}

/// <summary>
/// Inserts the given object and retrieves its
/// auto incremented primary key if it has one.
/// If a UNIQUE constraint violation occurs with
/// some pre-existing object, this function deletes
/// the old object.
/// </summary>
/// <param name="obj">
/// The object to insert.
/// </param>
/// <param name="objType">
/// The type of object to insert.
/// </param>
/// <returns>
/// The number of rows modified.
/// </returns>
public int InsertOrReplace (object obj, Type objType)
{
    return Insert (obj, "OR REPLACE", objType);
}

/// <summary>
/// Inserts the given object and retrieves its
/// auto incremented primary key if it has one.
/// </summary>
/// <param name="obj">
/// The object to insert.
/// </param>
/// <param name="extra">
/// Literal SQL code that gets placed into the command. INSERT {extra} INTO ...
/// </param>
/// <returns>
/// The number of rows added to the table.
/// </returns>
public int Insert (object obj, string extra)
{
    if (obj == null) {
        return 0;
    }
    return Insert (obj, extra, obj.GetType ());
}

/// <summary>
/// Inserts the given object and retrieves its
/// auto incremented primary key if it has one.
/// </summary>
/// <param name="obj">
/// The object to insert.
/// </param>
/// <param name="extra">
/// Literal SQL code that gets placed into the command. INSERT {extra} INTO ...
/// </param>
/// <param name="objType">
/// The type of object to insert.
/// </param>
/// <returns>
/// The number of rows added to the table.
/// </returns>
public int Insert (object obj, string extra, Type objType)
{
    if (obj == null || objType == null) {
        return 0;
    }


    var map = GetMapping (objType);

    #if USE_NEW_REFLECTION_API
    if (map.PK != null && map.PK.IsAutoGuid)
    {
        // no GetProperty so search our way up the inheritance chain till we find it
        PropertyInfo prop;
        while (objType != null)
        {
            var info = objType.GetTypeInfo();
            prop = info.GetDeclaredProperty(map.PK.PropertyName);
            if (prop != null) 
            {
                if (prop.GetValue(obj, null).Equals(Guid.Empty))
                {
                    prop.SetValue(obj, Guid.NewGuid(), null);
                }
                break; 
            }

            objType = info.BaseType;
        }
    }
    #else
    if (map.PK != null && map.PK.IsAutoGuid) {
    var prop = objType.GetProperty(map.PK.PropertyName);
    if (prop != null) {
    if (prop.GetValue(obj, null).Equals(Guid.Empty)) {
    prop.SetValue(obj, Guid.NewGuid(), null);
    }
    }
    }
    #endif


    var replacing = string.Compare (extra, "OR REPLACE", StringComparison.OrdinalIgnoreCase) == 0;

    var cols = replacing ? map.InsertOrReplaceColumns : map.InsertColumns;
    var vals = new object[cols.Length];
    for (var i = 0; i < vals.Length; i++) {
        vals [i] = cols [i].GetValue (obj);
    }

    var insertCmd = map.GetInsertCommand (this, extra);
    int count;

    lock (insertCmd) {
        // We lock here to protect the prepared statement returned via GetInsertCommand.
        // A SQLite prepared statement can be bound for only one operation at a time.
        try {
            count = insertCmd.ExecuteNonQuery (vals);
        } catch (SQLiteException ex) {
            if (SQLite3.ExtendedErrCode (this.Handle) == SQLite3.ExtendedResult.ConstraintNotNull) {
                throw NotNullConstraintViolationException.New (ex.Result, ex.Message, map, obj);
            }
            throw;
        }

        if (map.HasAutoIncPK) {
            var id = SQLite3.LastInsertRowid (Handle);
            map.SetAutoIncPK (obj, id);
        }
    }
    if (count > 0)
        OnTableChanged (map, NotifyTableChangedAction.Insert);

    return count;
}

/// <summary>
/// Updates all of the columns of a table using the specified object
/// except for its primary key.
/// The object is required to have a primary key.
/// </summary>
/// <param name="obj">
/// The object to update. It must have a primary key designated using the PrimaryKeyAttribute.
/// </param>
/// <returns>
/// The number of rows updated.
/// </returns>
public int Update (object obj)
{
    if (obj == null) {
        return 0;
    }
    return Update (obj, obj.GetType ());
}

/// <summary>
/// Updates all of the columns of a table using the specified object
/// except for its primary key.
/// The object is required to have a primary key.
/// </summary>
/// <param name="obj">
/// The object to update. It must have a primary key designated using the PrimaryKeyAttribute.
/// </param>
/// <param name="objType">
/// The type of object to insert.
/// </param>
/// <returns>
/// The number of rows updated.
/// </returns>
public int Update (object obj, Type objType)
{
    int rowsAffected = 0;
    if (obj == null || objType == null) {
        return 0;
    }

    var map = GetMapping (objType);

    var pk = map.PK;

    if (pk == null) {
        throw new NotSupportedException ("Cannot update " + map.TableName + ": it has no PK");
    }

    var cols = from p in map.Columns
            where p != pk
        select p;
    var vals = from c in cols
        select c.GetValue (obj);
    var ps = new List<object> (vals);
    ps.Add (pk.GetValue (obj));
    var q = string.Format ("update \"{0}\" set {1} where {2} = ? ", map.TableName, string.Join (",", (from c in cols
        select "\"" + c.Name + "\" = ? ").ToArray ()), pk.Name);

    try {
        rowsAffected = Execute (q, ps.ToArray ());
    }
    catch (SQLiteException ex) {

        if (ex.Result == SQLite3.Result.Constraint && SQLite3.ExtendedErrCode (this.Handle) == SQLite3.ExtendedResult.ConstraintNotNull) {
            throw NotNullConstraintViolationException.New (ex, map, obj);
        }

        throw ex;
    }

    if (rowsAffected > 0)
        OnTableChanged (map, NotifyTableChangedAction.Update);

    return rowsAffected;
}

/// <summary>
/// Updates all specified objects.
/// </summary>
/// <param name="objects">
/// An <see cref="IEnumerable"/> of the objects to insert.
/// </param>
/// <param name="runInTransaction"/>
/// A boolean indicating if the inserts should be wrapped in a transaction
/// </param>
/// <returns>
/// The number of rows modified.
/// </returns>
public int UpdateAll (System.Collections.IEnumerable objects, bool runInTransaction=true)
{
    var c = 0;
    if (runInTransaction) {
        RunInTransaction (() => {
            foreach (var r in objects) {
                c += Update (r);
            }
        });
    }
    else {
        foreach (var r in objects) {
            c += Update (r);
        }
    }
    return c;
}

/// <summary>
/// Deletes the given object from the database using its primary key.
/// </summary>
/// <param name="objectToDelete">
/// The object to delete. It must have a primary key designated using the PrimaryKeyAttribute.
/// </param>
/// <returns>
/// The number of rows deleted.
/// </returns>
public int Delete (object objectToDelete)
{
    var map = GetMapping (objectToDelete.GetType ());
    var pk = map.PK;
    if (pk == null) {
        throw new NotSupportedException ("Cannot delete " + map.TableName + ": it has no PK");
    }
    var q = string.Format ("delete from \"{0}\" where \"{1}\" = ?", map.TableName, pk.Name);
    var count = Execute (q, pk.GetValue (objectToDelete));
    if (count > 0)
        OnTableChanged (map, NotifyTableChangedAction.Delete);
    return count;
}

/// <summary>
/// Deletes the object with the specified primary key.
/// </summary>
/// <param name="primaryKey">
/// The primary key of the object to delete.
/// </param>
/// <returns>
/// The number of objects deleted.
/// </returns>
/// <typeparam name='T'>
/// The type of object.
/// </typeparam>
public int Delete<T> (object primaryKey)
{
    var map = GetMapping (typeof (T));
    var pk = map.PK;
    if (pk == null) {
        throw new NotSupportedException ("Cannot delete " + map.TableName + ": it has no PK");
    }
    var q = string.Format ("delete from \"{0}\" where \"{1}\" = ?", map.TableName, pk.Name);
    var count = Execute (q, primaryKey);
    if (count > 0)
        OnTableChanged (map, NotifyTableChangedAction.Delete);
    return count;
}

/// <summary>
/// Deletes all the objects from the specified table.
/// WARNING WARNING: Let me repeat. It deletes ALL the objects from the
/// specified table. Do you really want to do that?
/// </summary>
/// <returns>
/// The number of objects deleted.
/// </returns>
/// <typeparam name='T'>
/// The type of objects to delete.
/// </typeparam>
public int DeleteAll<T> ()
{
    var map = GetMapping (typeof (T));
    var query = string.Format("delete from \"{0}\"", map.TableName);
    var count = Execute (query);
    if (count > 0)
        OnTableChanged (map, NotifyTableChangedAction.Delete);
    return count;
}

~SQLiteConnection ()
{
    Dispose (false);
}

public void Dispose ()
{
    Dispose(true);
    GC.SuppressFinalize(this);
}

public void Close()
{
    Dispose(true);
}

protected virtual void Dispose(bool disposing)
{
    if (_open && Handle != NullHandle) {
        try {
            if (disposing) {
                if (_mappings != null) {
                    foreach (var sqlInsertCommand in _mappings.Values){
                        sqlInsertCommand.Dispose();
                    }
                }

                var r = SQLite3.Close(Handle);
                if (r != SQLite3.Result.OK)
                {
                    string msg = SQLite3.GetErrmsg(Handle);
                    throw SQLiteException.New(r, msg);
                }
            } else {
                SQLite3.Close2(Handle);
            }
        }
        finally {
            Handle = NullHandle;
            _open = false;
        }
    }
}

void OnTableChanged (TableMapping table, NotifyTableChangedAction action)
{
    var ev = TableChanged;
    if (ev != null)
        ev (this, new NotifyTableChangedEventArgs (table, action));
}

public event EventHandler<NotifyTableChangedEventArgs> TableChanged;
}

public class NotifyTableChangedEventArgs : EventArgs
{
    public TableMapping Table { get; private set; }
    public NotifyTableChangedAction Action { get; private set; }

    public NotifyTableChangedEventArgs (TableMapping table, NotifyTableChangedAction action)
    {
        Table = table;
        Action = action;        
    }
}

public enum NotifyTableChangedAction
{
    Insert,
    Update,
    Delete,
}

/// <summary>
/// Represents a parsed connection string.
/// </summary>
class SQLiteConnectionString
{
    public string ConnectionString { get; private set; }
    public string DatabasePath { get; private set; }
    public bool StoreDateTimeAsTicks { get; private set; }

    #if NETFX_CORE
    static readonly string MetroStyleDataPath = Windows.Storage.ApplicationData.Current.LocalFolder.Path;
    #endif

    public SQLiteConnectionString (string databasePath, bool storeDateTimeAsTicks)
    {
        ConnectionString = databasePath;
        StoreDateTimeAsTicks = storeDateTimeAsTicks;

        #if NETFX_CORE
        DatabasePath = System.IO.Path.Combine (MetroStyleDataPath, databasePath);
        #else
        DatabasePath = databasePath;
        #endif
    }
}

[AttributeUsage (AttributeTargets.Class)]
public class TableAttribute : Attribute
{
    public string Name { get; set; }

    public TableAttribute (string name)
    {
        Name = name;
    }
}

[AttributeUsage (AttributeTargets.Property)]
public class ColumnAttribute : Attribute
{
    public string Name { get; set; }

    public ColumnAttribute (string name)
    {
        Name = name;
    }
}

[AttributeUsage (AttributeTargets.Property)]
public class PrimaryKeyAttribute : Attribute
{
}

[AttributeUsage (AttributeTargets.Property)]
public class AutoIncrementAttribute : Attribute
{
}

[AttributeUsage (AttributeTargets.Property)]
public class IndexedAttribute : Attribute
{
    public string Name { get; set; }
    public int Order { get; set; }
    public virtual bool Unique { get; set; }

    public IndexedAttribute()
    {
    }

    public IndexedAttribute(string name, int order)
    {
        Name = name;
        Order = order;
    }
}

[AttributeUsage (AttributeTargets.Property)]
public class IgnoreAttribute : Attribute
{
}

[AttributeUsage (AttributeTargets.Property)]
public class UniqueAttribute : IndexedAttribute
{
    public override bool Unique {
        get { return true; }
        set { /* throw?  */ }
    }
}

[AttributeUsage (AttributeTargets.Property)]
public class MaxLengthAttribute : Attribute
{
    public int Value { get; private set; }

    public MaxLengthAttribute (int length)
    {
        Value = length;
    }
}

[AttributeUsage (AttributeTargets.Property)]
public class CollationAttribute: Attribute
{
    public string Value { get; private set; }

    public CollationAttribute (string collation)
    {
        Value = collation;
    }
}

[AttributeUsage (AttributeTargets.Property)]
public class NotNullAttribute : Attribute
{
}

public class TableMapping
{
    public Type MappedType { get; private set; }

    public string TableName { get; private set; }

    public Column[] Columns { get; private set; }

    public Column PK { get; private set; }

    public string GetByPrimaryKeySql { get; private set; }

    Column _autoPk;
    Column[] _insertColumns;
    Column[] _insertOrReplaceColumns;

    public TableMapping(Type type, CreateFlags createFlags = CreateFlags.None)
    {
        MappedType = type;

        #if USE_NEW_REFLECTION_API
        var tableAttr = (TableAttribute)System.Reflection.CustomAttributeExtensions
            .GetCustomAttribute(type.GetTypeInfo(), typeof(TableAttribute), true);
        #else
        var tableAttr = (TableAttribute)type.GetCustomAttributes (typeof (TableAttribute), true).FirstOrDefault ();
        #endif

        TableName = tableAttr != null ? tableAttr.Name : MappedType.Name;

        #if !USE_NEW_REFLECTION_API
        var props = MappedType.GetProperties (BindingFlags.Public | BindingFlags.Instance | BindingFlags.SetProperty);
        #else
        var props = from p in MappedType.GetRuntimeProperties()
                where ((p.GetMethod != null && p.GetMethod.IsPublic) || (p.SetMethod != null && p.SetMethod.IsPublic) || (p.GetMethod != null && p.GetMethod.IsStatic) || (p.SetMethod != null && p.SetMethod.IsStatic))
            select p;
        #endif
        var cols = new List<Column> ();
        foreach (var p in props) {
            #if !USE_NEW_REFLECTION_API
            var ignore = p.GetCustomAttributes (typeof(IgnoreAttribute), true).Length > 0;
            #else
            var ignore = p.GetCustomAttributes (typeof(IgnoreAttribute), true).Count() > 0;
            #endif
            if (p.CanWrite && !ignore) {
                cols.Add (new Column (p, createFlags));
            }
        }
        Columns = cols.ToArray ();
        foreach (var c in Columns) {
            if (c.IsAutoInc && c.IsPK) {
                _autoPk = c;
            }
            if (c.IsPK) {
                PK = c;
            }
        }

        HasAutoIncPK = _autoPk != null;

        if (PK != null) {
            GetByPrimaryKeySql = string.Format ("select * from \"{0}\" where \"{1}\" = ?", TableName, PK.Name);
        }
        else {
            // People should not be calling Get/Find without a PK
            GetByPrimaryKeySql = string.Format ("select * from \"{0}\" limit 1", TableName);
        }
        _insertCommandMap = new ConcurrentStringDictionary ();
    }

    public bool HasAutoIncPK { get; private set; }

    public void SetAutoIncPK (object obj, long id)
    {
        if (_autoPk != null) {
            _autoPk.SetValue (obj, Convert.ChangeType (id, _autoPk.ColumnType, null));
        }
    }

    public Column[] InsertColumns {
        get {
            if (_insertColumns == null) {
                _insertColumns = Columns.Where (c => !c.IsAutoInc).ToArray ();
            }
            return _insertColumns;
        }
    }

    public Column[] InsertOrReplaceColumns {
        get {
            if (_insertOrReplaceColumns == null) {
                _insertOrReplaceColumns = Columns.ToArray ();
            }
            return _insertOrReplaceColumns;
        }
    }

    public Column FindColumnWithPropertyName (string propertyName)
    {
        var exact = Columns.FirstOrDefault (c => c.PropertyName == propertyName);
        return exact;
    }

    public Column FindColumn (string columnName)
    {
        var exact = Columns.FirstOrDefault (c => c.Name == columnName);
        return exact;
    }

    ConcurrentStringDictionary _insertCommandMap;

    public PreparedSqlLiteInsertCommand GetInsertCommand(SQLiteConnection conn, string extra)
    {
        object prepCmdO;

        if (!_insertCommandMap.TryGetValue (extra, out prepCmdO)) {
            var prepCmd = CreateInsertCommand (conn, extra);
            prepCmdO = prepCmd;
            if (!_insertCommandMap.TryAdd (extra, prepCmd)) {
                // Concurrent add attempt beat us.
                prepCmd.Dispose ();
                _insertCommandMap.TryGetValue (extra, out prepCmdO);
            }
        }
        return (PreparedSqlLiteInsertCommand)prepCmdO;
    }

    PreparedSqlLiteInsertCommand CreateInsertCommand(SQLiteConnection conn, string extra)
    {
        var cols = InsertColumns;
        string insertSql;
        if (!cols.Any() && Columns.Count() == 1 && Columns[0].IsAutoInc)
        {
            insertSql = string.Format("insert {1} into \"{0}\" default values", TableName, extra);
        }
        else
        {
            var replacing = string.Compare (extra, "OR REPLACE", StringComparison.OrdinalIgnoreCase) == 0;

            if (replacing) {
                cols = InsertOrReplaceColumns;
            }

            insertSql = string.Format("insert {3} into \"{0}\"({1}) values ({2})", TableName,
                string.Join(",", (from c in cols
                    select "\"" + c.Name + "\"").ToArray()),
                string.Join(",", (from c in cols
                    select "?").ToArray()), extra);

        }

        var insertCommand = new PreparedSqlLiteInsertCommand(conn);
        insertCommand.CommandText = insertSql;
        return insertCommand;
    }

    protected internal void Dispose()
    {
        foreach (var pair in _insertCommandMap) {
            ((PreparedSqlLiteInsertCommand)pair.Value).Dispose ();
        }
        _insertCommandMap = null;
    }

    public class Column
    {
        PropertyInfo _prop;

        public string Name { get; private set; }

        public string PropertyName { get { return _prop.Name; } }

        public Type ColumnType { get; private set; }

        public string Collation { get; private set; }

        public bool IsAutoInc { get; private set; }
        public bool IsAutoGuid { get; private set; }

        public bool IsPK { get; private set; }

        public IEnumerable<IndexedAttribute> Indices { get; set; }

        public bool IsNullable { get; private set; }

        public int? MaxStringLength { get; private set; }

        public Column(PropertyInfo prop, CreateFlags createFlags = CreateFlags.None)
        {
            var colAttr = (ColumnAttribute)prop.GetCustomAttributes(typeof(ColumnAttribute), true).FirstOrDefault();

            _prop = prop;
            Name = colAttr == null ? prop.Name : colAttr.Name;
            //If this type is Nullable<T> then Nullable.GetUnderlyingType returns the T, otherwise it returns null, so get the actual type instead
            ColumnType = Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType;
            Collation = Orm.Collation(prop);

            IsPK = Orm.IsPK(prop) ||
                (((createFlags & CreateFlags.ImplicitPK) == CreateFlags.ImplicitPK) &&
                    string.Compare (prop.Name, Orm.ImplicitPkName, StringComparison.OrdinalIgnoreCase) == 0);

            var isAuto = Orm.IsAutoInc(prop) || (IsPK && ((createFlags & CreateFlags.AutoIncPK) == CreateFlags.AutoIncPK));
            IsAutoGuid = isAuto && ColumnType == typeof(Guid);
            IsAutoInc = isAuto && !IsAutoGuid;

            Indices = Orm.GetIndices(prop);
            if (!Indices.Any()
                && !IsPK
                && ((createFlags & CreateFlags.ImplicitIndex) == CreateFlags.ImplicitIndex)
                && Name.EndsWith (Orm.ImplicitIndexSuffix, StringComparison.OrdinalIgnoreCase)
            )
            {
                Indices = new IndexedAttribute[] { new IndexedAttribute() };
            }
            IsNullable = !(IsPK || Orm.IsMarkedNotNull(prop));
            MaxStringLength = Orm.MaxStringLength(prop);
        }

        public void SetValue (object obj, object val)
        {
            _prop.SetValue (obj, val, null);
        }

        public object GetValue (object obj)
        {
            return _prop.GetValue (obj, null);
        }
    }
}

public static class Orm
{
    public const int DefaultMaxStringLength = 140;
    public const string ImplicitPkName = "Id";
    public const string ImplicitIndexSuffix = "Id";

    public static string SqlDecl (TableMapping.Column p, bool storeDateTimeAsTicks)
    {
        string decl = "\"" + p.Name + "\" " + SqlType (p, storeDateTimeAsTicks) + " ";

        if (p.IsPK) {
            decl += "primary key ";
        }
        if (p.IsAutoInc) {
            decl += "autoincrement ";
        }
        if (!p.IsNullable) {
            decl += "not null ";
        }
        if (!string.IsNullOrEmpty (p.Collation)) {
            decl += "collate " + p.Collation + " ";
        }

        return decl;
    }

    public static string SqlType (TableMapping.Column p, bool storeDateTimeAsTicks)
    {
        var clrType = p.ColumnType;
        if (clrType == typeof(Boolean) || clrType == typeof(Byte) || clrType == typeof(UInt16) || clrType == typeof(SByte) || clrType == typeof(Int16) || clrType == typeof(Int32) || clrType == typeof(UInt32) || clrType == typeof(Int64))
        {
            return "integer";
        } else if (clrType == typeof(Single) || clrType == typeof(Double) || clrType == typeof(Decimal)) {
            return "float";
        } else if (clrType == typeof(String)) {
            int? len = p.MaxStringLength;

            if (len.HasValue)
                return "varchar(" + len.Value + ")";

            return "varchar";
        } else if (clrType == typeof(TimeSpan)) {
            return "bigint";
        } else if (clrType == typeof(DateTime)) {
            return storeDateTimeAsTicks ? "bigint" : "datetime";
        } else if (clrType == typeof(DateTimeOffset)) {
            return "bigint";
            #if !USE_NEW_REFLECTION_API
            } else if (clrType.IsEnum) {
            #else
        } else if (clrType.GetTypeInfo().IsEnum) {
            #endif
            return "integer";
        } else if (clrType == typeof(byte[])) {
            return "blob";
        } else if (clrType == typeof(Guid)) {
            return "varchar(36)";
        } else {
            throw new NotSupportedException ("Don't know about " + clrType);
        }
    }

    public static bool IsPK (MemberInfo p)
    {
        var attrs = p.GetCustomAttributes (typeof(PrimaryKeyAttribute), true);
        #if !USE_NEW_REFLECTION_API
        return attrs.Length > 0;
        #else
        return attrs.Count() > 0;
        #endif
    }

    public static string Collation (MemberInfo p)
    {
        var attrs = p.GetCustomAttributes (typeof(CollationAttribute), true);
        #if !USE_NEW_REFLECTION_API
        if (attrs.Length > 0) {
        return ((CollationAttribute)attrs [0]).Value;
        #else
        if (attrs.Count() > 0) {
            return ((CollationAttribute)attrs.First()).Value;
        #endif
        } else {
            return string.Empty;
        }
    }

    public static bool IsAutoInc (MemberInfo p)
    {
        var attrs = p.GetCustomAttributes (typeof(AutoIncrementAttribute), true);
        #if !USE_NEW_REFLECTION_API
        return attrs.Length > 0;
        #else
        return attrs.Count() > 0;
        #endif
    }

    public static IEnumerable<IndexedAttribute> GetIndices(MemberInfo p)
    {
        var attrs = p.GetCustomAttributes(typeof(IndexedAttribute), true);
        return attrs.Cast<IndexedAttribute>();
    }

    public static int? MaxStringLength(PropertyInfo p)
    {
        var attrs = p.GetCustomAttributes (typeof(MaxLengthAttribute), true);
        #if !USE_NEW_REFLECTION_API
        if (attrs.Length > 0)
        return ((MaxLengthAttribute)attrs [0]).Value;
        #else
        if (attrs.Count() > 0)
            return ((MaxLengthAttribute)attrs.First()).Value;
        #endif

        return null;
    }

    public static bool IsMarkedNotNull(MemberInfo p)
    {
        var attrs = p.GetCustomAttributes (typeof (NotNullAttribute), true);
        #if !USE_NEW_REFLECTION_API
        return attrs.Length > 0;
        #else
        return attrs.Count() > 0;
        #endif
    }
}

public partial class SQLiteCommand
{
    SQLiteConnection _conn;
    private List<Binding> _bindings;

    public string CommandText { get; set; }

    internal SQLiteCommand (SQLiteConnection conn)
    {
        _conn = conn;
        _bindings = new List<Binding> ();
        CommandText = "";
    }

    public int ExecuteNonQuery ()
    {
        if (_conn.Trace) {
            Debug.WriteLine ("Executing: " + this);
        }

        var r = SQLite3.Result.OK;
        var stmt = Prepare ();
        r = SQLite3.Step (stmt);
        Finalize (stmt);
        if (r == SQLite3.Result.Done) {
            int rowsAffected = SQLite3.Changes (_conn.Handle);
            return rowsAffected;
        } else if (r == SQLite3.Result.Error) {
            string msg = SQLite3.GetErrmsg (_conn.Handle);
            throw SQLiteException.New (r, msg);
        }
        else if (r == SQLite3.Result.Constraint) {
            if (SQLite3.ExtendedErrCode (_conn.Handle) == SQLite3.ExtendedResult.ConstraintNotNull) {
                throw NotNullConstraintViolationException.New (r, SQLite3.GetErrmsg (_conn.Handle));
            }
        }

        throw SQLiteException.New(r, r.ToString());
    }

    public IEnumerable<T> ExecuteDeferredQuery<T> ()
    {
        return ExecuteDeferredQuery<T>(_conn.GetMapping(typeof(T)));
    }

    public List<T> ExecuteQuery<T> ()
    {
        return ExecuteDeferredQuery<T>(_conn.GetMapping(typeof(T))).ToList();
    }

    public List<T> ExecuteQuery<T> (TableMapping map)
    {
        return ExecuteDeferredQuery<T>(map).ToList();
    }

    /// <summary>
    /// Invoked every time an instance is loaded from the database.
    /// </summary>
    /// <param name='obj'>
    /// The newly created object.
    /// </param>
    /// <remarks>
    /// This can be overridden in combination with the <see cref="SQLiteConnection.NewCommand"/>
    /// method to hook into the life-cycle of objects.
    ///
    /// Type safety is not possible because MonoTouch does not support virtual generic methods.
    /// </remarks>
    protected virtual void OnInstanceCreated (object obj)
    {
        // Can be overridden.
    }

    public IEnumerable<T> ExecuteDeferredQuery<T> (TableMapping map)
    {
        if (_conn.Trace) {
            Debug.WriteLine ("Executing Query: " + this);
        }

        var stmt = Prepare ();
        try
        {
            var cols = new TableMapping.Column[SQLite3.ColumnCount (stmt)];

            for (int i = 0; i < cols.Length; i++) {
                var name = SQLite3.ColumnName16 (stmt, i);
                cols [i] = map.FindColumn (name);
            }

            while (SQLite3.Step (stmt) == SQLite3.Result.Row) {
                var obj = Activator.CreateInstance(map.MappedType);
                for (int i = 0; i < cols.Length; i++) {
                    if (cols [i] == null)
                        continue;
                    var colType = SQLite3.ColumnType (stmt, i);
                    var val = ReadCol (stmt, i, colType, cols [i].ColumnType);
                    cols [i].SetValue (obj, val);
                }
                OnInstanceCreated (obj);
                yield return (T)obj;
            }
        }
        finally
        {
            SQLite3.Finalize(stmt);
        }
    }

    public T ExecuteScalar<T> ()
    {
        if (_conn.Trace) {
            Debug.WriteLine ("Executing Query: " + this);
        }

        T val = default(T);

        var stmt = Prepare ();

        try
        {
            var r = SQLite3.Step (stmt);
            if (r == SQLite3.Result.Row) {
                var colType = SQLite3.ColumnType (stmt, 0);
                val = (T)ReadCol (stmt, 0, colType, typeof(T));
            }
            else if (r == SQLite3.Result.Done) {
            }
            else
            {
                throw SQLiteException.New (r, SQLite3.GetErrmsg (_conn.Handle));
            }
        }
        finally
        {
            Finalize (stmt);
        }

        return val;
    }

    public void Bind (string name, object val)
    {
        _bindings.Add (new Binding {
            Name = name,
            Value = val
        });
    }

    public void Bind (object val)
    {
        Bind (null, val);
    }

    public override string ToString ()
    {
        var parts = new string[1 + _bindings.Count];
        parts [0] = CommandText;
        var i = 1;
        foreach (var b in _bindings) {
            parts [i] = string.Format ("  {0}: {1}", i - 1, b.Value);
            i++;
        }
        return string.Join (Environment.NewLine, parts);
    }

    Sqlite3Statement Prepare()
    {
        var stmt = SQLite3.Prepare2 (_conn.Handle, CommandText);
        BindAll (stmt);
        return stmt;
    }

    void Finalize (Sqlite3Statement stmt)
    {
        SQLite3.Finalize (stmt);
    }

    void BindAll (Sqlite3Statement stmt)
    {
        int nextIdx = 1;
        foreach (var b in _bindings) {
            if (b.Name != null) {
                b.Index = SQLite3.BindParameterIndex (stmt, b.Name);
            } else {
                b.Index = nextIdx++;
            }

            BindParameter (stmt, b.Index, b.Value, _conn.StoreDateTimeAsTicks);
        }
    }

    internal static IntPtr NegativePointer = new IntPtr (-1);

    internal static void BindParameter (Sqlite3Statement stmt, int index, object value, bool storeDateTimeAsTicks)
    {
        if (value == null) {
            SQLite3.BindNull (stmt, index);
        } else {
            if (value is Int32) {
                SQLite3.BindInt (stmt, index, (int)value);
            } else if (value is String) {
                SQLite3.BindText (stmt, index, (string)value, -1, NegativePointer);
            } else if (value is Byte || value is UInt16 || value is SByte || value is Int16) {
                SQLite3.BindInt (stmt, index, Convert.ToInt32 (value));
            } else if (value is Boolean) {
                SQLite3.BindInt (stmt, index, (bool)value ? 1 : 0);
            } else if (value is UInt32 || value is Int64) {
                SQLite3.BindInt64 (stmt, index, Convert.ToInt64 (value));
            } else if (value is Single || value is Double || value is Decimal) {
                SQLite3.BindDouble (stmt, index, Convert.ToDouble (value));
            } else if (value is TimeSpan) {
                SQLite3.BindInt64(stmt, index, ((TimeSpan)value).Ticks);
            } else if (value is DateTime) {
                if (storeDateTimeAsTicks) {
                    SQLite3.BindInt64 (stmt, index, ((DateTime)value).Ticks);
                }
                else {
                    SQLite3.BindText (stmt, index, ((DateTime)value).ToString ("yyyy-MM-dd HH:mm:ss"), -1, NegativePointer);
                }
            } else if (value is DateTimeOffset) {
                SQLite3.BindInt64 (stmt, index, ((DateTimeOffset)value).UtcTicks);
                #if !USE_NEW_REFLECTION_API
                } else if (value.GetType().IsEnum) {
                #else
            } else if (value.GetType().GetTypeInfo().IsEnum) {
                #endif
                SQLite3.BindInt (stmt, index, Convert.ToInt32 (value));
            } else if (value is byte[]){
                SQLite3.BindBlob(stmt, index, (byte[]) value, ((byte[]) value).Length, NegativePointer);
            } else if (value is Guid) {
                SQLite3.BindText(stmt, index, ((Guid)value).ToString(), 72, NegativePointer);
            } else {
                throw new NotSupportedException("Cannot store type: " + value.GetType());
            }
        }
    }

    class Binding
    {
        public string Name { get; set; }

        public object Value { get; set; }

        public int Index { get; set; }
    }

    object ReadCol (Sqlite3Statement stmt, int index, SQLite3.ColType type, Type clrType)
    {
        if (type == SQLite3.ColType.Null) {
            return null;
        } else {
            if (clrType == typeof(String)) {
                return SQLite3.ColumnString (stmt, index);
            } else if (clrType == typeof(Int32)) {
                return (int)SQLite3.ColumnInt (stmt, index);
            } else if (clrType == typeof(Boolean)) {
                return SQLite3.ColumnInt (stmt, index) == 1;
            } else if (clrType == typeof(double)) {
                return SQLite3.ColumnDouble (stmt, index);
            } else if (clrType == typeof(float)) {
                return (float)SQLite3.ColumnDouble (stmt, index);
            } else if (clrType == typeof(TimeSpan)) {
                return new TimeSpan(SQLite3.ColumnInt64(stmt, index));
            } else if (clrType == typeof(DateTime)) {
                if (_conn.StoreDateTimeAsTicks) {
                    return new DateTime (SQLite3.ColumnInt64 (stmt, index));
                }
                else {
                    var text = SQLite3.ColumnString (stmt, index);
                    return DateTime.Parse (text);
                }
            } else if (clrType == typeof(DateTimeOffset)) {
                return new DateTimeOffset(SQLite3.ColumnInt64 (stmt, index),TimeSpan.Zero);
                #if !USE_NEW_REFLECTION_API
                } else if (clrType.IsEnum) {
                #else
            } else if (clrType.GetTypeInfo().IsEnum) {
                #endif
                return SQLite3.ColumnInt (stmt, index);
            } else if (clrType == typeof(Int64)) {
                return SQLite3.ColumnInt64 (stmt, index);
            } else if (clrType == typeof(UInt32)) {
                return (uint)SQLite3.ColumnInt64 (stmt, index);
            } else if (clrType == typeof(decimal)) {
                return (decimal)SQLite3.ColumnDouble (stmt, index);
            } else if (clrType == typeof(Byte)) {
                return (byte)SQLite3.ColumnInt (stmt, index);
            } else if (clrType == typeof(UInt16)) {
                return (ushort)SQLite3.ColumnInt (stmt, index);
            } else if (clrType == typeof(Int16)) {
                return (short)SQLite3.ColumnInt (stmt, index);
            } else if (clrType == typeof(sbyte)) {
                return (sbyte)SQLite3.ColumnInt (stmt, index);
            } else if (clrType == typeof(byte[])) {
                return SQLite3.ColumnByteArray (stmt, index);
            } else if (clrType == typeof(Guid)) {
                var text = SQLite3.ColumnString(stmt, index);
                return new Guid(text);
            } else{
                throw new NotSupportedException ("Don't know how to read " + clrType);
            }
        }
    }
}

/// <summary>
/// Since the insert never changed, we only need to prepare once.
/// </summary>
public class PreparedSqlLiteInsertCommand : IDisposable
{
    public bool Initialized { get; set; }

    protected SQLiteConnection Connection { get; set; }

    public string CommandText { get; set; }

    protected Sqlite3Statement Statement { get; set; }
    internal static readonly Sqlite3Statement NullStatement = default(Sqlite3Statement);

    internal PreparedSqlLiteInsertCommand (SQLiteConnection conn)
    {
        Connection = conn;
    }

    public int ExecuteNonQuery (object[] source)
    {
        if (Connection.Trace) {
            Debug.WriteLine ("Executing: " + CommandText);
        }

        var r = SQLite3.Result.OK;

        if (!Initialized) {
            Statement = Prepare ();
            Initialized = true;
        }

        //bind the values.
        if (source != null) {
            for (int i = 0; i < source.Length; i++) {
                SQLiteCommand.BindParameter (Statement, i + 1, source [i], Connection.StoreDateTimeAsTicks);
            }
        }
        r = SQLite3.Step (Statement);

        if (r == SQLite3.Result.Done) {
            int rowsAffected = SQLite3.Changes (Connection.Handle);
            SQLite3.Reset (Statement);
            return rowsAffected;
        } else if (r == SQLite3.Result.Error) {
            string msg = SQLite3.GetErrmsg (Connection.Handle);
            SQLite3.Reset (Statement);
            throw SQLiteException.New (r, msg);
        } else if (r == SQLite3.Result.Constraint && SQLite3.ExtendedErrCode (Connection.Handle) == SQLite3.ExtendedResult.ConstraintNotNull) {
            SQLite3.Reset (Statement);
            throw NotNullConstraintViolationException.New (r, SQLite3.GetErrmsg (Connection.Handle));
        } else {
            SQLite3.Reset (Statement);
            throw SQLiteException.New (r, r.ToString ());
        }
    }

    protected virtual Sqlite3Statement Prepare ()
    {
        var stmt = SQLite3.Prepare2 (Connection.Handle, CommandText);
        return stmt;
    }

    public void Dispose ()
    {
        Dispose (true);
        GC.SuppressFinalize (this);
    }

    private void Dispose (bool disposing)
    {
        if (Statement != NullStatement) {
            try {
                SQLite3.Finalize (Statement);
            } finally {
                Statement = NullStatement;
                Connection = null;
            }
        }
    }

    ~PreparedSqlLiteInsertCommand ()
    {
        Dispose (false);
    }
}

public abstract class BaseTableQuery
{
    protected class Ordering
    {
        public string ColumnName { get; set; }
        public bool Ascending { get; set; }
    }
}

public class TableQuery<T> : BaseTableQuery, IEnumerable<T>
{
    public SQLiteConnection Connection { get; private set; }

    public TableMapping Table { get; private set; }

    Expression _where;
    List<Ordering> _orderBys;
    int? _limit;
    int? _offset;

    BaseTableQuery _joinInner;
    Expression _joinInnerKeySelector;
    BaseTableQuery _joinOuter;
    Expression _joinOuterKeySelector;
    Expression _joinSelector;

    Expression _selector;

    TableQuery (SQLiteConnection conn, TableMapping table)
    {
        Connection = conn;
        Table = table;
    }

    public TableQuery (SQLiteConnection conn)
    {
        Connection = conn;
        Table = Connection.GetMapping (typeof(T));
    }

    public TableQuery<U> Clone<U> ()
    {
        var q = new TableQuery<U> (Connection, Table);
        q._where = _where;
        q._deferred = _deferred;
        if (_orderBys != null) {
            q._orderBys = new List<Ordering> (_orderBys);
        }
        q._limit = _limit;
        q._offset = _offset;
        q._joinInner = _joinInner;
        q._joinInnerKeySelector = _joinInnerKeySelector;
        q._joinOuter = _joinOuter;
        q._joinOuterKeySelector = _joinOuterKeySelector;
        q._joinSelector = _joinSelector;
        q._selector = _selector;
        return q;
    }

    public TableQuery<T> Where (Expression<Func<T, bool>> predExpr)
    {
        if (predExpr.NodeType == ExpressionType.Lambda) {
            var lambda = (LambdaExpression)predExpr;
            var pred = lambda.Body;
            var q = Clone<T> ();
            q.AddWhere (pred);
            return q;
        } else {
            throw new NotSupportedException ("Must be a predicate");
        }
    }

    public int Delete(Expression<Func<T, bool>> predExpr)
    {
        if (predExpr.NodeType == ExpressionType.Lambda) {
            var lambda = (LambdaExpression)predExpr;
            var pred = lambda.Body;
            var args = new List<object> ();
            var w = CompileExpr (pred, args);
            var cmdText = "delete from \"" + Table.TableName + "\"";
            cmdText += " where " + w.CommandText;
            var command = Connection.CreateCommand (cmdText, args.ToArray ());

            int result = command.ExecuteNonQuery();
            return result;
        } else {
            throw new NotSupportedException ("Must be a predicate");
        }
    }

    public TableQuery<T> Take (int n)
    {
        var q = Clone<T> ();
        q._limit = n;
        return q;
    }

    public TableQuery<T> Skip (int n)
    {
        var q = Clone<T> ();
        q._offset = n;
        return q;
    }

    public T ElementAt (int index)
    {
        return Skip (index).Take (1).First ();
    }

    bool _deferred;
    public TableQuery<T> Deferred ()
    {
        var q = Clone<T> ();
        q._deferred = true;
        return q;
    }

    public TableQuery<T> OrderBy<U> (Expression<Func<T, U>> orderExpr)
    {
        return AddOrderBy<U> (orderExpr, true);
    }

    public TableQuery<T> OrderByDescending<U> (Expression<Func<T, U>> orderExpr)
    {
        return AddOrderBy<U> (orderExpr, false);
    }

    public TableQuery<T> ThenBy<U>(Expression<Func<T, U>> orderExpr)
    {
        return AddOrderBy<U>(orderExpr, true);
    }

    public TableQuery<T> ThenByDescending<U>(Expression<Func<T, U>> orderExpr)
    {
        return AddOrderBy<U>(orderExpr, false);
    }

    private TableQuery<T> AddOrderBy<U> (Expression<Func<T, U>> orderExpr, bool asc)
    {
        if (orderExpr.NodeType == ExpressionType.Lambda) {
            var lambda = (LambdaExpression)orderExpr;

            MemberExpression mem = null;

            var unary = lambda.Body as UnaryExpression;
            if (unary != null && unary.NodeType == ExpressionType.Convert) {
                mem = unary.Operand as MemberExpression;
            }
            else {
                mem = lambda.Body as MemberExpression;
            }

            if (mem != null && (mem.Expression.NodeType == ExpressionType.Parameter)) {
                var q = Clone<T> ();
                if (q._orderBys == null) {
                    q._orderBys = new List<Ordering> ();
                }
                q._orderBys.Add (new Ordering {
                    ColumnName = Table.FindColumnWithPropertyName(mem.Member.Name).Name,
                    Ascending = asc
                });
                return q;
            } else {
                throw new NotSupportedException ("Order By does not support: " + orderExpr);
            }
        } else {
            throw new NotSupportedException ("Must be a predicate");
        }
    }

    private void AddWhere (Expression pred)
    {
        if (_where == null) {
            _where = pred;
        } else {
            _where = Expression.AndAlso (_where, pred);
        }
    }

    public TableQuery<TResult> Join<TInner, TKey, TResult> (
        TableQuery<TInner> inner,
        Expression<Func<T, TKey>> outerKeySelector,
        Expression<Func<TInner, TKey>> innerKeySelector,
        Expression<Func<T, TInner, TResult>> resultSelector)
    {
        var q = new TableQuery<TResult> (Connection, Connection.GetMapping (typeof (TResult))) {
            _joinOuter = this,
            _joinOuterKeySelector = outerKeySelector,
            _joinInner = inner,
            _joinInnerKeySelector = innerKeySelector,
            _joinSelector = resultSelector,
        };
        return q;
    }

    public TableQuery<TResult> Select<TResult> (Expression<Func<T, TResult>> selector)
    {
        var q = Clone<TResult> ();
        q._selector = selector;
        return q;
    }

    private SQLiteCommand GenerateCommand (string selectionList)
    {
        if (_joinInner != null && _joinOuter != null) {
            throw new NotSupportedException ("Joins are not supported.");
        }
        else {
            var cmdText = "select " + selectionList + " from \"" + Table.TableName + "\"";
            var args = new List<object> ();
            if (_where != null) {
                var w = CompileExpr (_where, args);
                cmdText += " where " + w.CommandText;
            }
            if ((_orderBys != null) && (_orderBys.Count > 0)) {
                var t = string.Join (", ", _orderBys.Select (o => "\"" + o.ColumnName + "\"" + (o.Ascending ? "" : " desc")).ToArray ());
                cmdText += " order by " + t;
            }
            if (_limit.HasValue) {
                cmdText += " limit " + _limit.Value;
            }
            if (_offset.HasValue) {
                if (!_limit.HasValue) {
                    cmdText += " limit -1 ";
                }
                cmdText += " offset " + _offset.Value;
            }
            return Connection.CreateCommand (cmdText, args.ToArray ());
        }
    }

    class CompileResult
    {
        public string CommandText { get; set; }

        public object Value { get; set; }
    }

    private CompileResult CompileExpr (Expression expr, List<object> queryArgs)
    {
        if (expr == null) {
            throw new NotSupportedException ("Expression is NULL");
        } else if (expr is BinaryExpression) {
            var bin = (BinaryExpression)expr;

            // VB turns 'x=="foo"' into 'CompareString(x,"foo",true/false)==0', so we need to unwrap it
            // http://blogs.msdn.com/b/vbteam/archive/2007/09/18/vb-expression-trees-string-comparisons.aspx
            if (bin.Left.NodeType == ExpressionType.Call) { 
                var call = (MethodCallExpression)bin.Left;
                if (call.Method.DeclaringType.FullName == "Microsoft.VisualBasic.CompilerServices.Operators"
                    && call.Method.Name == "CompareString")
                    bin = Expression.MakeBinary(bin.NodeType, call.Arguments[0], call.Arguments[1]);
            }


            var leftr = CompileExpr (bin.Left, queryArgs);
            var rightr = CompileExpr (bin.Right, queryArgs);

            //If either side is a parameter and is null, then handle the other side specially (for "is null"/"is not null")
            string text;
            if (leftr.CommandText == "?" && leftr.Value == null)
                text = CompileNullBinaryExpression(bin, rightr);
            else if (rightr.CommandText == "?" && rightr.Value == null)
                text = CompileNullBinaryExpression(bin, leftr);
            else
                text = "(" + leftr.CommandText + " " + GetSqlName(bin) + " " + rightr.CommandText + ")";
            return new CompileResult { CommandText = text };
        } else if (expr.NodeType == ExpressionType.Not) {
            var operandExpr = ((UnaryExpression)expr).Operand;
            var opr = CompileExpr(operandExpr, queryArgs);
            object val = opr.Value;
            if (val is bool)
                val = !((bool) val);
            return new CompileResult
            {
                CommandText = "NOT(" + opr.CommandText + ")",
                Value = val
            };
        } else if (expr.NodeType == ExpressionType.Call) {

            var call = (MethodCallExpression)expr;
            var args = new CompileResult[call.Arguments.Count];
            var obj = call.Object != null ? CompileExpr (call.Object, queryArgs) : null;

            for (var i = 0; i < args.Length; i++) {
                args [i] = CompileExpr (call.Arguments [i], queryArgs);
            }

            var sqlCall = "";

            if (call.Method.Name == "Like" && args.Length == 2) {
                sqlCall = "(" + args [0].CommandText + " like " + args [1].CommandText + ")";
            }
            else if (call.Method.Name == "Contains" && args.Length == 2) {
                sqlCall = "(" + args [1].CommandText + " in " + args [0].CommandText + ")";
            }
            else if (call.Method.Name == "Contains" && args.Length == 1) {
                if (call.Object != null && call.Object.Type == typeof(string)) {
                    sqlCall = "(" + obj.CommandText + " like ('%' || " + args [0].CommandText + " || '%'))";
                }
                else {
                    sqlCall = "(" + args [0].CommandText + " in " + obj.CommandText + ")";
                }
            }
            else if (call.Method.Name == "StartsWith" && args.Length == 1) {
                sqlCall = "(" + obj.CommandText + " like (" + args [0].CommandText + " || '%'))";
            }
            else if (call.Method.Name == "EndsWith" && args.Length == 1) {
                sqlCall = "(" + obj.CommandText + " like ('%' || " + args [0].CommandText + "))";
            }
            else if (call.Method.Name == "Equals" && args.Length == 1) {
                sqlCall = "(" + obj.CommandText + " = (" + args[0].CommandText + "))";
            } else if (call.Method.Name == "ToLower") {
                sqlCall = "(lower(" + obj.CommandText + "))"; 
            } else if (call.Method.Name == "ToUpper") {
                sqlCall = "(upper(" + obj.CommandText + "))"; 
            } else {
                sqlCall = call.Method.Name.ToLower () + "(" + string.Join (",", args.Select (a => a.CommandText).ToArray ()) + ")";
            }
            return new CompileResult { CommandText = sqlCall };

        } else if (expr.NodeType == ExpressionType.Constant) {
            var c = (ConstantExpression)expr;
            queryArgs.Add (c.Value);
            return new CompileResult {
                CommandText = "?",
                Value = c.Value
            };
        } else if (expr.NodeType == ExpressionType.Convert) {
            var u = (UnaryExpression)expr;
            var ty = u.Type;
            var valr = CompileExpr (u.Operand, queryArgs);
            return new CompileResult {
                CommandText = valr.CommandText,
                Value = valr.Value != null ? ConvertTo (valr.Value, ty) : null
            };
        } else if (expr.NodeType == ExpressionType.MemberAccess) {
            var mem = (MemberExpression)expr;

            if (mem.Expression!=null && mem.Expression.NodeType == ExpressionType.Parameter) {
                //
                // This is a column of our table, output just the column name
                // Need to translate it if that column name is mapped
                //
                var columnName = Table.FindColumnWithPropertyName (mem.Member.Name).Name;
                return new CompileResult { CommandText = "\"" + columnName + "\"" };
            } else {
                object obj = null;
                if (mem.Expression != null) {
                    var r = CompileExpr (mem.Expression, queryArgs);
                    if (r.Value == null) {
                        throw new NotSupportedException ("Member access failed to compile expression");
                    }
                    if (r.CommandText == "?") {
                        queryArgs.RemoveAt (queryArgs.Count - 1);
                    }
                    obj = r.Value;
                }

                //
                // Get the member value
                //
                object val = null;

                #if !USE_NEW_REFLECTION_API
                if (mem.Member.MemberType == MemberTypes.Property) {
                #else
                if (mem.Member is PropertyInfo) {
                #endif
                    var m = (PropertyInfo)mem.Member;
                    val = m.GetValue (obj, null);
                    #if !USE_NEW_REFLECTION_API
                    } else if (mem.Member.MemberType == MemberTypes.Field) {
                    #else
                } else if (mem.Member is FieldInfo) {
                    #endif
                    #if SILVERLIGHT
                    val = Expression.Lambda (expr).Compile ().DynamicInvoke ();
                    #else
                    var m = (FieldInfo)mem.Member;
                    val = m.GetValue (obj);
                    #endif
                } else {
                    #if !USE_NEW_REFLECTION_API
                    throw new NotSupportedException ("MemberExpr: " + mem.Member.MemberType);
                    #else
                    throw new NotSupportedException ("MemberExpr: " + mem.Member.DeclaringType);
                    #endif
                }

                //
                // Work special magic for enumerables
                //
                if (val != null && val is System.Collections.IEnumerable && !(val is string) && !(val is System.Collections.Generic.IEnumerable<byte>)) {
                    var sb = new System.Text.StringBuilder();
                    sb.Append("(");
                    var head = "";
                    foreach (var a in (System.Collections.IEnumerable)val) {
                        queryArgs.Add(a);
                        sb.Append(head);
                        sb.Append("?");
                        head = ",";
                    }
                    sb.Append(")");
                    return new CompileResult {
                        CommandText = sb.ToString(),
                        Value = val
                    };
                }
                else {
                    queryArgs.Add (val);
                    return new CompileResult {
                        CommandText = "?",
                        Value = val
                    };
                }
            }
        }
        throw new NotSupportedException ("Cannot compile: " + expr.NodeType.ToString ());
    }

    static object ConvertTo (object obj, Type t)
    {
        Type nut = Nullable.GetUnderlyingType(t);

        if (nut != null) {
            if (obj == null) return null;               
            return Convert.ChangeType (obj, nut);
        } else {
            return Convert.ChangeType (obj, t);
        }
    }

    /// <summary>
    /// Compiles a BinaryExpression where one of the parameters is null.
    /// </summary>
    /// <param name="parameter">The non-null parameter</param>
    private string CompileNullBinaryExpression(BinaryExpression expression, CompileResult parameter)
    {
        if (expression.NodeType == ExpressionType.Equal)
            return "(" + parameter.CommandText + " is ?)";
        else if (expression.NodeType == ExpressionType.NotEqual)
            return "(" + parameter.CommandText + " is not ?)";
        else
            throw new NotSupportedException("Cannot compile Null-BinaryExpression with type " + expression.NodeType.ToString());
    }

    string GetSqlName (Expression expr)
    {
        var n = expr.NodeType;
        if (n == ExpressionType.GreaterThan)
        return ">"; else if (n == ExpressionType.GreaterThanOrEqual) {
            return ">=";
        } else if (n == ExpressionType.LessThan) {
            return "<";
        } else if (n == ExpressionType.LessThanOrEqual) {
            return "<=";
        } else if (n == ExpressionType.And) {
            return "&";
        } else if (n == ExpressionType.AndAlso) {
            return "and";
        } else if (n == ExpressionType.Or) {
            return "|";
        } else if (n == ExpressionType.OrElse) {
            return "or";
        } else if (n == ExpressionType.Equal) {
            return "=";
        } else if (n == ExpressionType.NotEqual) {
            return "!=";
        } else {
            throw new NotSupportedException ("Cannot get SQL for: " + n);
        }
    }

    public int Count ()
    {
        return GenerateCommand("count(*)").ExecuteScalar<int> ();           
    }

    public int Count (Expression<Func<T, bool>> predExpr)
    {
        return Where (predExpr).Count ();
    }

    public IEnumerator<T> GetEnumerator ()
    {
        if (!_deferred)
            return GenerateCommand("*").ExecuteQuery<T>().GetEnumerator();

        return GenerateCommand("*").ExecuteDeferredQuery<T>().GetEnumerator();
    }

    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator ()
    {
        return GetEnumerator ();
    }

    public T First ()
    {
        var query = Take (1);
        return query.ToList<T>().First ();
    }

    public T FirstOrDefault ()
    {
        var query = Take (1);
        return query.ToList<T>().FirstOrDefault ();
    }
}

public static class SQLite3
{
    public enum Result : int
    {
        OK = 0,
        Error = 1,
        Internal = 2,
        Perm = 3,
        Abort = 4,
        Busy = 5,
        Locked = 6,
        NoMem = 7,
        ReadOnly = 8,
        Interrupt = 9,
        IOError = 10,
        Corrupt = 11,
        NotFound = 12,
        Full = 13,
        CannotOpen = 14,
        LockErr = 15,
        Empty = 16,
        SchemaChngd = 17,
        TooBig = 18,
        Constraint = 19,
        Mismatch = 20,
        Misuse = 21,
        NotImplementedLFS = 22,
        AccessDenied = 23,
        Format = 24,
        Range = 25,
        NonDBFile = 26,
        Notice = 27,
        Warning = 28,
        Row = 100,
        Done = 101
    }

    public enum ExtendedResult : int
    {
        IOErrorRead = (Result.IOError | (1 << 8)),
        IOErrorShortRead = (Result.IOError | (2 << 8)),
        IOErrorWrite = (Result.IOError | (3 << 8)),
        IOErrorFsync = (Result.IOError | (4 << 8)),
        IOErrorDirFSync = (Result.IOError | (5 << 8)),
        IOErrorTruncate = (Result.IOError | (6 << 8)),
        IOErrorFStat = (Result.IOError | (7 << 8)),
        IOErrorUnlock = (Result.IOError | (8 << 8)),
        IOErrorRdlock = (Result.IOError | (9 << 8)),
        IOErrorDelete = (Result.IOError | (10 << 8)),
        IOErrorBlocked = (Result.IOError | (11 << 8)),
        IOErrorNoMem = (Result.IOError | (12 << 8)),
        IOErrorAccess = (Result.IOError | (13 << 8)),
        IOErrorCheckReservedLock = (Result.IOError | (14 << 8)),
        IOErrorLock = (Result.IOError | (15 << 8)),
        IOErrorClose = (Result.IOError | (16 << 8)),
        IOErrorDirClose = (Result.IOError | (17 << 8)),
        IOErrorSHMOpen = (Result.IOError | (18 << 8)),
        IOErrorSHMSize = (Result.IOError | (19 << 8)),
        IOErrorSHMLock = (Result.IOError | (20 << 8)),
        IOErrorSHMMap = (Result.IOError | (21 << 8)),
        IOErrorSeek = (Result.IOError | (22 << 8)),
        IOErrorDeleteNoEnt = (Result.IOError | (23 << 8)),
        IOErrorMMap = (Result.IOError | (24 << 8)),
        LockedSharedcache = (Result.Locked | (1 << 8)),
        BusyRecovery = (Result.Busy | (1 << 8)),
        CannottOpenNoTempDir = (Result.CannotOpen | (1 << 8)),
        CannotOpenIsDir = (Result.CannotOpen | (2 << 8)),
        CannotOpenFullPath = (Result.CannotOpen | (3 << 8)),
        CorruptVTab = (Result.Corrupt | (1 << 8)),
        ReadonlyRecovery = (Result.ReadOnly | (1 << 8)),
        ReadonlyCannotLock = (Result.ReadOnly | (2 << 8)),
        ReadonlyRollback = (Result.ReadOnly | (3 << 8)),
        AbortRollback = (Result.Abort | (2 << 8)),
        ConstraintCheck = (Result.Constraint | (1 << 8)),
        ConstraintCommitHook = (Result.Constraint | (2 << 8)),
        ConstraintForeignKey = (Result.Constraint | (3 << 8)),
        ConstraintFunction = (Result.Constraint | (4 << 8)),
        ConstraintNotNull = (Result.Constraint | (5 << 8)),
        ConstraintPrimaryKey = (Result.Constraint | (6 << 8)),
        ConstraintTrigger = (Result.Constraint | (7 << 8)),
        ConstraintUnique = (Result.Constraint | (8 << 8)),
        ConstraintVTab = (Result.Constraint | (9 << 8)),
        NoticeRecoverWAL = (Result.Notice | (1 << 8)),
        NoticeRecoverRollback = (Result.Notice | (2 << 8))
    }


    public enum ConfigOption : int
    {
        SingleThread = 1,
        MultiThread = 2,
        Serialized = 3
    }

    const string LibraryPath = "sqlite3";

    #if !USE_CSHARP_SQLITE && !USE_WP8_NATIVE_SQLITE && !USE_SQLITEPCL_RAW
    [DllImport(LibraryPath, EntryPoint = "sqlite3_threadsafe", CallingConvention=CallingConvention.Cdecl)]
    public static extern int Threadsafe ();

    [DllImport(LibraryPath, EntryPoint = "sqlite3_open", CallingConvention=CallingConvention.Cdecl)]
    public static extern Result Open ([MarshalAs(UnmanagedType.LPStr)] string filename, out IntPtr db);

    [DllImport(LibraryPath, EntryPoint = "sqlite3_open_v2", CallingConvention=CallingConvention.Cdecl)]
    public static extern Result Open ([MarshalAs(UnmanagedType.LPStr)] string filename, out IntPtr db, int flags, IntPtr zvfs);

    [DllImport(LibraryPath, EntryPoint = "sqlite3_open_v2", CallingConvention = CallingConvention.Cdecl)]
    public static extern Result Open(byte[] filename, out IntPtr db, int flags, IntPtr zvfs);

    [DllImport(LibraryPath, EntryPoint = "sqlite3_open16", CallingConvention = CallingConvention.Cdecl)]
    public static extern Result Open16([MarshalAs(UnmanagedType.LPWStr)] string filename, out IntPtr db);

    [DllImport(LibraryPath, EntryPoint = "sqlite3_enable_load_extension", CallingConvention=CallingConvention.Cdecl)]
    public static extern Result EnableLoadExtension (IntPtr db, int onoff);

    [DllImport(LibraryPath, EntryPoint = "sqlite3_close", CallingConvention=CallingConvention.Cdecl)]
    public static extern Result Close (IntPtr db);

    [DllImport(LibraryPath, EntryPoint = "sqlite3_close_v2", CallingConvention = CallingConvention.Cdecl)]
    public static extern Result Close2(IntPtr db);

    [DllImport(LibraryPath, EntryPoint = "sqlite3_initialize", CallingConvention=CallingConvention.Cdecl)]
    public static extern Result Initialize();

    [DllImport(LibraryPath, EntryPoint = "sqlite3_shutdown", CallingConvention=CallingConvention.Cdecl)]
    public static extern Result Shutdown();

    [DllImport(LibraryPath, EntryPoint = "sqlite3_config", CallingConvention=CallingConvention.Cdecl)]
    public static extern Result Config (ConfigOption option);

    [DllImport(LibraryPath, EntryPoint = "sqlite3_win32_set_directory", CallingConvention=CallingConvention.Cdecl, CharSet=CharSet.Unicode)]
    public static extern int SetDirectory (uint directoryType, string directoryPath);

    [DllImport(LibraryPath, EntryPoint = "sqlite3_busy_timeout", CallingConvention=CallingConvention.Cdecl)]
    public static extern Result BusyTimeout (IntPtr db, int milliseconds);

    [DllImport(LibraryPath, EntryPoint = "sqlite3_changes", CallingConvention=CallingConvention.Cdecl)]
    public static extern int Changes (IntPtr db);

    [DllImport(LibraryPath, EntryPoint = "sqlite3_prepare_v2", CallingConvention=CallingConvention.Cdecl)]
    public static extern Result Prepare2 (IntPtr db, [MarshalAs(UnmanagedType.LPStr)] string sql, int numBytes, out IntPtr stmt, IntPtr pzTail);

    #if NETFX_CORE
    [DllImport (LibraryPath, EntryPoint = "sqlite3_prepare_v2", CallingConvention = CallingConvention.Cdecl)]
    public static extern Result Prepare2 (IntPtr db, byte[] queryBytes, int numBytes, out IntPtr stmt, IntPtr pzTail);
    #endif

    public static IntPtr Prepare2 (IntPtr db, string query)
    {
    IntPtr stmt;
    #if NETFX_CORE
    byte[] queryBytes = System.Text.UTF8Encoding.UTF8.GetBytes (query);
    var r = Prepare2 (db, queryBytes, queryBytes.Length, out stmt, IntPtr.Zero);
    #else
    var r = Prepare2 (db, query, System.Text.UTF8Encoding.UTF8.GetByteCount (query), out stmt, IntPtr.Zero);
    #endif
    if (r != Result.OK) {
        throw SQLiteException.New (r, GetErrmsg (db));
    }
    return stmt;
}

[DllImport(LibraryPath, EntryPoint = "sqlite3_step", CallingConvention=CallingConvention.Cdecl)]
public static extern Result Step (IntPtr stmt);

[DllImport(LibraryPath, EntryPoint = "sqlite3_reset", CallingConvention=CallingConvention.Cdecl)]
public static extern Result Reset (IntPtr stmt);

[DllImport(LibraryPath, EntryPoint = "sqlite3_finalize", CallingConvention=CallingConvention.Cdecl)]
public static extern Result Finalize (IntPtr stmt);

[DllImport(LibraryPath, EntryPoint = "sqlite3_last_insert_rowid", CallingConvention=CallingConvention.Cdecl)]
public static extern long LastInsertRowid (IntPtr db);

[DllImport(LibraryPath, EntryPoint = "sqlite3_errmsg16", CallingConvention=CallingConvention.Cdecl)]
public static extern IntPtr Errmsg (IntPtr db);

public static string GetErrmsg (IntPtr db)
{
    return Marshal.PtrToStringUni (Errmsg (db));
}

[DllImport(LibraryPath, EntryPoint = "sqlite3_bind_parameter_index", CallingConvention=CallingConvention.Cdecl)]
public static extern int BindParameterIndex (IntPtr stmt, [MarshalAs(UnmanagedType.LPStr)] string name);

[DllImport(LibraryPath, EntryPoint = "sqlite3_bind_null", CallingConvention=CallingConvention.Cdecl)]
public static extern int BindNull (IntPtr stmt, int index);

[DllImport(LibraryPath, EntryPoint = "sqlite3_bind_int", CallingConvention=CallingConvention.Cdecl)]
public static extern int BindInt (IntPtr stmt, int index, int val);

[DllImport(LibraryPath, EntryPoint = "sqlite3_bind_int64", CallingConvention=CallingConvention.Cdecl)]
public static extern int BindInt64 (IntPtr stmt, int index, long val);

[DllImport(LibraryPath, EntryPoint = "sqlite3_bind_double", CallingConvention=CallingConvention.Cdecl)]
public static extern int BindDouble (IntPtr stmt, int index, double val);

[DllImport(LibraryPath, EntryPoint = "sqlite3_bind_text16", CallingConvention=CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
public static extern int BindText (IntPtr stmt, int index, [MarshalAs(UnmanagedType.LPWStr)] string val, int n, IntPtr free);

[DllImport(LibraryPath, EntryPoint = "sqlite3_bind_blob", CallingConvention=CallingConvention.Cdecl)]
public static extern int BindBlob (IntPtr stmt, int index, byte[] val, int n, IntPtr free);

[DllImport(LibraryPath, EntryPoint = "sqlite3_column_count", CallingConvention=CallingConvention.Cdecl)]
public static extern int ColumnCount (IntPtr stmt);

[DllImport(LibraryPath, EntryPoint = "sqlite3_column_name", CallingConvention=CallingConvention.Cdecl)]
public static extern IntPtr ColumnName (IntPtr stmt, int index);

[DllImport(LibraryPath, EntryPoint = "sqlite3_column_name16", CallingConvention=CallingConvention.Cdecl)]
static extern IntPtr ColumnName16Internal (IntPtr stmt, int index);
public static string ColumnName16(IntPtr stmt, int index)
{
    return Marshal.PtrToStringUni(ColumnName16Internal(stmt, index));
}

[DllImport(LibraryPath, EntryPoint = "sqlite3_column_type", CallingConvention=CallingConvention.Cdecl)]
public static extern ColType ColumnType (IntPtr stmt, int index);

[DllImport(LibraryPath, EntryPoint = "sqlite3_column_int", CallingConvention=CallingConvention.Cdecl)]
public static extern int ColumnInt (IntPtr stmt, int index);

[DllImport(LibraryPath, EntryPoint = "sqlite3_column_int64", CallingConvention=CallingConvention.Cdecl)]
public static extern long ColumnInt64 (IntPtr stmt, int index);

[DllImport(LibraryPath, EntryPoint = "sqlite3_column_double", CallingConvention=CallingConvention.Cdecl)]
public static extern double ColumnDouble (IntPtr stmt, int index);

[DllImport(LibraryPath, EntryPoint = "sqlite3_column_text", CallingConvention=CallingConvention.Cdecl)]
public static extern IntPtr ColumnText (IntPtr stmt, int index);

[DllImport(LibraryPath, EntryPoint = "sqlite3_column_text16", CallingConvention=CallingConvention.Cdecl)]
public static extern IntPtr ColumnText16 (IntPtr stmt, int index);

[DllImport(LibraryPath, EntryPoint = "sqlite3_column_blob", CallingConvention=CallingConvention.Cdecl)]
public static extern IntPtr ColumnBlob (IntPtr stmt, int index);

[DllImport(LibraryPath, EntryPoint = "sqlite3_column_bytes", CallingConvention=CallingConvention.Cdecl)]
public static extern int ColumnBytes (IntPtr stmt, int index);

public static string ColumnString (IntPtr stmt, int index)
{
    return Marshal.PtrToStringUni (SQLite3.ColumnText16 (stmt, index));
}

public static byte[] ColumnByteArray (IntPtr stmt, int index)
{
    int length = ColumnBytes (stmt, index);
    var result = new byte[length];
    if (length > 0)
        Marshal.Copy (ColumnBlob (stmt, index), result, 0, length);
    return result;
}

[DllImport (LibraryPath, EntryPoint = "sqlite3_extended_errcode", CallingConvention = CallingConvention.Cdecl)]
public static extern ExtendedResult ExtendedErrCode (IntPtr db);

[DllImport (LibraryPath, EntryPoint = "sqlite3_libversion_number", CallingConvention = CallingConvention.Cdecl)]
public static extern int LibVersionNumber ();
    #else
public static Result Open(string filename, out Sqlite3DatabaseHandle db)
{
    return (Result) Sqlite3.sqlite3_open(filename, out db);
}

public static Result Open(string filename, out Sqlite3DatabaseHandle db, int flags, IntPtr zVfs)
{
    #if USE_WP8_NATIVE_SQLITE
    return (Result)Sqlite3.sqlite3_open_v2(filename, out db, flags, "");
    #else
    return (Result)Sqlite3.sqlite3_open_v2(filename, out db, flags, null);
    #endif
}

public static Result Close(Sqlite3DatabaseHandle db)
{
    return (Result)Sqlite3.sqlite3_close(db);
}

public static Result Close2(Sqlite3DatabaseHandle db)
{
    return (Result)Sqlite3.sqlite3_close_v2(db);
}

public static Result BusyTimeout(Sqlite3DatabaseHandle db, int milliseconds)
{
    return (Result)Sqlite3.sqlite3_busy_timeout(db, milliseconds);
}

public static int Changes(Sqlite3DatabaseHandle db)
{
    return Sqlite3.sqlite3_changes(db);
}

public static Sqlite3Statement Prepare2(Sqlite3DatabaseHandle db, string query)
{
    Sqlite3Statement stmt = default(Sqlite3Statement);
    #if USE_WP8_NATIVE_SQLITE || USE_SQLITEPCL_RAW
    var r = Sqlite3.sqlite3_prepare_v2(db, query, out stmt);
    #else
    stmt = new Sqlite3Statement();
    var r = Sqlite3.sqlite3_prepare_v2(db, query, -1, ref stmt, 0);
    #endif
    if (r != 0)
    {
        throw SQLiteException.New((Result)r, GetErrmsg(db));
    }
    return stmt;
}

public static Result Step(Sqlite3Statement stmt)
{
    return (Result)Sqlite3.sqlite3_step(stmt);
}

public static Result Reset(Sqlite3Statement stmt)
{
    return (Result)Sqlite3.sqlite3_reset(stmt);
}

public static Result Finalize(Sqlite3Statement stmt)
{
    return (Result)Sqlite3.sqlite3_finalize(stmt);
}

public static long LastInsertRowid(Sqlite3DatabaseHandle db)
{
    return Sqlite3.sqlite3_last_insert_rowid(db);
}

public static string GetErrmsg(Sqlite3DatabaseHandle db)
{
    return Sqlite3.sqlite3_errmsg(db);
}

public static int BindParameterIndex(Sqlite3Statement stmt, string name)
{
    return Sqlite3.sqlite3_bind_parameter_index(stmt, name);
}

public static int BindNull(Sqlite3Statement stmt, int index)
{
    return Sqlite3.sqlite3_bind_null(stmt, index);
}

public static int BindInt(Sqlite3Statement stmt, int index, int val)
{
    return Sqlite3.sqlite3_bind_int(stmt, index, val);
}

public static int BindInt64(Sqlite3Statement stmt, int index, long val)
{
    return Sqlite3.sqlite3_bind_int64(stmt, index, val);
}

public static int BindDouble(Sqlite3Statement stmt, int index, double val)
{
    return Sqlite3.sqlite3_bind_double(stmt, index, val);
}

public static int BindText(Sqlite3Statement stmt, int index, string val, int n, IntPtr free)
{
    #if USE_WP8_NATIVE_SQLITE
    return Sqlite3.sqlite3_bind_text(stmt, index, val, n);
    #elif USE_SQLITEPCL_RAW
    return Sqlite3.sqlite3_bind_text(stmt, index, val);
    #else
    return Sqlite3.sqlite3_bind_text(stmt, index, val, n, null);
    #endif
}

public static int BindBlob(Sqlite3Statement stmt, int index, byte[] val, int n, IntPtr free)
{
    #if USE_WP8_NATIVE_SQLITE
    return Sqlite3.sqlite3_bind_blob(stmt, index, val, n);
    #elif USE_SQLITEPCL_RAW
    return Sqlite3.sqlite3_bind_blob(stmt, index, val);
    #else
    return Sqlite3.sqlite3_bind_blob(stmt, index, val, n, null);
    #endif
}

public static int ColumnCount(Sqlite3Statement stmt)
{
    return Sqlite3.sqlite3_column_count(stmt);
}

public static string ColumnName(Sqlite3Statement stmt, int index)
{
    return Sqlite3.sqlite3_column_name(stmt, index);
}

public static string ColumnName16(Sqlite3Statement stmt, int index)
{
    return Sqlite3.sqlite3_column_name(stmt, index);
}

public static ColType ColumnType(Sqlite3Statement stmt, int index)
{
    return (ColType)Sqlite3.sqlite3_column_type(stmt, index);
}

public static int ColumnInt(Sqlite3Statement stmt, int index)
{
    return Sqlite3.sqlite3_column_int(stmt, index);
}

public static long ColumnInt64(Sqlite3Statement stmt, int index)
{
    return Sqlite3.sqlite3_column_int64(stmt, index);
}

public static double ColumnDouble(Sqlite3Statement stmt, int index)
{
    return Sqlite3.sqlite3_column_double(stmt, index);
}

public static string ColumnText(Sqlite3Statement stmt, int index)
{
    return Sqlite3.sqlite3_column_text(stmt, index);
}

public static string ColumnText16(Sqlite3Statement stmt, int index)
{
    return Sqlite3.sqlite3_column_text(stmt, index);
}

public static byte[] ColumnBlob(Sqlite3Statement stmt, int index)
{
    return Sqlite3.sqlite3_column_blob(stmt, index);
}

public static int ColumnBytes(Sqlite3Statement stmt, int index)
{
    return Sqlite3.sqlite3_column_bytes(stmt, index);
}

public static string ColumnString(Sqlite3Statement stmt, int index)
{
    return Sqlite3.sqlite3_column_text(stmt, index);
}

public static byte[] ColumnByteArray(Sqlite3Statement stmt, int index)
{
    return ColumnBlob(stmt, index);
}

    #if !USE_SQLITEPCL_RAW
public static Result EnableLoadExtension(Sqlite3DatabaseHandle db, int onoff)
{
    return (Result)Sqlite3.sqlite3_enable_load_extension(db, onoff);
}
    #endif

public static ExtendedResult ExtendedErrCode(Sqlite3DatabaseHandle db)
{
    return (ExtendedResult)Sqlite3.sqlite3_extended_errcode(db);
}
    #endif

public enum ColType : int
{
    Integer = 1,
    Float = 2,
    Text = 3,
    Blob = 4,
    Null = 5
}
}
}

#if NO_CONCURRENT
namespace SQLite.Extensions
{
public static class ListEx
{
public static bool TryAdd<TKey, TValue> (this IDictionary<TKey, TValue> dict, TKey key, TValue value)
{
try {
dict.Add (key, value);
return true;
}
catch (ArgumentException) {
return false;
}
}
}
}
#endif

