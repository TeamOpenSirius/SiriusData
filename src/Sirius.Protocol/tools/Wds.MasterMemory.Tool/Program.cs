using System.Collections;
using System.Globalization;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using MasterMemory.Meta;
using MessagePack;
using Sirius.Protocol.Shared;

internal static class Program
{
    private const string MetaDir = ".wdsmm";
    private const string ManifestFile = "manifest.json";
    private const string OriginalDbFile = "original.db";

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

    private static int Main(string[] args)
    {
        try
        {
            if (args.Length == 0)
            {
                PrintHelp();
                return 1;
            }

            return args[0].ToLowerInvariant() switch
            {
                "unpack" when args.Length == 3 => Unpack(args[1], args[2]),
                "pack" when args.Length == 3 => Pack(args[1], args[2]),
                "verify" when args.Length == 2 => Verify(args[1]),
                "roundtrip" when args.Length == 3 => Roundtrip(args[1], args[2]),
                "extract-text" when args.Length >= 3 => ExtractText(args[1], args[2], args.Skip(3).Contains("--all")),
                "apply-text" when args.Length == 3 => ApplyText(args[1], args[2]),
                _ => FailUsage()
            };
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"ERROR: {ex.Message}");
            if (Environment.GetEnvironmentVariable("WDSMM_TRACE") == "1")
                Console.Error.WriteLine(ex);
            return 2;
        }
    }

    private static int Unpack(string databasePath, string outputDirectory)
    {
        byte[] bytes = File.ReadAllBytes(databasePath);
        var db = new MemoryDatabase(bytes, maxDegreeOfParallelism: Environment.ProcessorCount);
        var meta = MemoryDatabase.GetMetaDatabase();

        Directory.CreateDirectory(outputDirectory);
        string tablesDir = Path.Combine(outputDirectory, "tables");
        string metaDir = Path.Combine(outputDirectory, MetaDir);
        Directory.CreateDirectory(tablesDir);
        Directory.CreateDirectory(metaDir);

        string originalCopy = Path.Combine(metaDir, OriginalDbFile);
        File.WriteAllBytes(originalCopy, bytes);

        var tableInfos = meta.GetTableInfos().ToArray();
        var entries = new List<TableEntry>(tableInfos.Length);
        long totalRows = 0;

        foreach (MetaTable tableInfo in tableInfos)
        {
            object table = MemoryDatabase.GetTable(db, tableInfo.TableName)
                ?? throw new InvalidOperationException($"Generated table not found: {tableInfo.TableName}");
            Array rows = GetRawRows(table);
            totalRows += rows.Length;

            var arr = new JsonArray();
            foreach (object? row in rows)
                arr.Add(ModelJson.ToNode(row, tableInfo.DataType));

            string json = arr.ToJsonString(JsonOptions) + Environment.NewLine;
            string relative = Path.Combine("tables", tableInfo.TableName + ".json").Replace('\\', '/');
            string path = Path.Combine(outputDirectory, relative.Replace('/', Path.DirectorySeparatorChar));
            File.WriteAllText(path, json, new UTF8Encoding(false));
            entries.Add(new TableEntry(tableInfo.TableName, relative, Sha256File(path), rows.Length));
        }

        var manifest = new DumpManifest(
            Version: 1,
            OriginalSha256: Sha256Bytes(bytes),
            OriginalLength: bytes.LongLength,
            Tables: entries);
        File.WriteAllText(Path.Combine(metaDir, ManifestFile), JsonSerializer.Serialize(manifest, JsonOptions) + Environment.NewLine, new UTF8Encoding(false));

        Console.WriteLine($"Unpacked {entries.Count} tables / {totalRows:N0} rows.");
        Console.WriteLine($"Original SHA-256: {manifest.OriginalSha256}");
        Console.WriteLine($"Edit JSON under: {tablesDir}");
        return 0;
    }

    private static int Pack(string inputDirectory, string outputDatabase)
    {
        DumpManifest manifest = LoadManifest(inputDirectory);
        string metaDir = Path.Combine(inputDirectory, MetaDir);
        string originalDb = Path.Combine(metaDir, OriginalDbFile);
        if (!File.Exists(originalDb))
            throw new FileNotFoundException("Missing original DB copy required by the dump manifest.", originalDb);

        var changed = manifest.Tables
            .Where(t => !string.Equals(Sha256File(Path.Combine(inputDirectory, t.JsonFile.Replace('/', Path.DirectorySeparatorChar))), t.JsonSha256, StringComparison.OrdinalIgnoreCase))
            .ToArray();

        Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(outputDatabase))!);

        // Strong no-op guarantee: no JSON changed => return the exact original byte stream.
        if (changed.Length == 0)
        {
            File.Copy(originalDb, outputDatabase, overwrite: true);
            string outputHash = Sha256File(outputDatabase);
            if (!string.Equals(outputHash, manifest.OriginalSha256, StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException("No-op pack failed byte-exact SHA-256 verification.");

            Console.WriteLine("No table changed; copied original DB byte-for-byte.");
            Console.WriteLine($"SHA-256: {outputHash}");
            return 0;
        }

        // Modified pack: rebuild through the official generated MasterMemory DatabaseBuilder.
        var meta = MemoryDatabase.GetMetaDatabase();
        var builder = new DatabaseBuilder();
        foreach (TableEntry entry in manifest.Tables)
        {
            MetaTable tableInfo = meta.GetTableInfo(entry.Name)
                ?? throw new InvalidOperationException($"Model does not contain table: {entry.Name}");
            string path = Path.Combine(inputDirectory, entry.JsonFile.Replace('/', Path.DirectorySeparatorChar));
            JsonArray jsonRows = JsonNode.Parse(File.ReadAllText(path)) as JsonArray
                ?? throw new InvalidDataException($"Table JSON root must be an array: {entry.Name}");

            Array typedRows = Array.CreateInstance(tableInfo.DataType, jsonRows.Count);
            for (int i = 0; i < jsonRows.Count; i++)
                typedRows.SetValue(ModelJson.FromNode(jsonRows[i], tableInfo.DataType), i);

            AppendTyped(builder, tableInfo.DataType, typedRows);
        }

        byte[] rebuilt = builder.Build();
        File.WriteAllBytes(outputDatabase, rebuilt);

        // Prove that the generated file can be consumed by the same generated model.
        _ = new MemoryDatabase(rebuilt, maxDegreeOfParallelism: Environment.ProcessorCount);
        Console.WriteLine($"Rebuilt with official MasterMemory DatabaseBuilder. Changed tables: {changed.Length}");
        foreach (var x in changed.Take(20)) Console.WriteLine($"  {x.Name}");
        if (changed.Length > 20) Console.WriteLine($"  ... and {changed.Length - 20} more");
        Console.WriteLine($"Output SHA-256: {Sha256Bytes(rebuilt)}");
        return 0;
    }

    private static int Verify(string databasePath)
    {
        byte[] bytes = File.ReadAllBytes(databasePath);
        var db = new MemoryDatabase(bytes, maxDegreeOfParallelism: Environment.ProcessorCount);
        var meta = MemoryDatabase.GetMetaDatabase();
        long rows = 0;
        foreach (var info in meta.GetTableInfos())
        {
            object table = MemoryDatabase.GetTable(db, info.TableName)
                ?? throw new InvalidOperationException($"Missing generated table: {info.TableName}");
            rows += GetRawRows(table).Length;
        }

        Console.WriteLine($"OK: {meta.Count} tables / {rows:N0} rows");
        Console.WriteLine($"SHA-256: {Sha256Bytes(bytes)}");
        return 0;
    }

    private static int Roundtrip(string databasePath, string workDirectory)
    {
        if (Directory.Exists(workDirectory)) Directory.Delete(workDirectory, recursive: true);
        Unpack(databasePath, workDirectory);
        string output = Path.Combine(workDirectory, "roundtrip.db");
        Pack(workDirectory, output);

        string a = Sha256File(databasePath);
        string b = Sha256File(output);
        bool same = string.Equals(a, b, StringComparison.OrdinalIgnoreCase)
                    && new FileInfo(databasePath).Length == new FileInfo(output).Length;
        Console.WriteLine(same ? "BYTE-EXACT ROUNDTRIP: PASS" : "BYTE-EXACT ROUNDTRIP: FAIL");
        Console.WriteLine($"input : {a}");
        Console.WriteLine($"output: {b}");
        return same ? 0 : 3;
    }

    private static int ExtractText(string dumpDirectory, string csvPath, bool allStrings)
    {
        DumpManifest manifest = LoadManifest(dumpDirectory);
        var records = new List<TextRecord>();

        foreach (TableEntry entry in manifest.Tables)
        {
            string path = Path.Combine(dumpDirectory, entry.JsonFile.Replace('/', Path.DirectorySeparatorChar));
            JsonNode? root = JsonNode.Parse(File.ReadAllText(path));
            WalkStrings(root, "", (pointer, value) =>
            {
                if (allStrings || LooksTranslatable(value))
                    records.Add(new TextRecord(entry.Name, pointer, value, ""));
            });
        }

        Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(csvPath))!);
        using var sw = new StreamWriter(csvPath, false, new UTF8Encoding(true));
        Csv.WriteRow(sw, "table", "path", "source", "translation");
        foreach (var r in records) Csv.WriteRow(sw, r.Table, r.Path, r.Source, r.Translation);
        Console.WriteLine($"Extracted {records.Count:N0} strings to {csvPath}");
        return 0;
    }

    private static int ApplyText(string dumpDirectory, string csvPath)
    {
        DumpManifest manifest = LoadManifest(dumpDirectory);
        var tableFiles = manifest.Tables.ToDictionary(x => x.Name, StringComparer.Ordinal);
        var rows = Csv.ReadAll(csvPath).ToArray();
        if (rows.Length == 0) return 0;
        int start = rows[0].Length >= 4 && string.Equals(rows[0][0], "table", StringComparison.OrdinalIgnoreCase) ? 1 : 0;

        var groups = rows.Skip(start)
            .Where(r => r.Length >= 4 && !string.IsNullOrEmpty(r[3]))
            .GroupBy(r => r[0], StringComparer.Ordinal);

        int applied = 0;
        foreach (var group in groups)
        {
            if (!tableFiles.TryGetValue(group.Key, out TableEntry? entry))
                throw new InvalidDataException($"CSV references unknown table: {group.Key}");
            string path = Path.Combine(dumpDirectory, entry.JsonFile.Replace('/', Path.DirectorySeparatorChar));
            JsonNode root = JsonNode.Parse(File.ReadAllText(path)) ?? throw new InvalidDataException($"Invalid JSON: {path}");
            foreach (var r in group)
            {
                SetJsonPointer(root, r[1], JsonValue.Create(r[3]));
                applied++;
            }
            File.WriteAllText(path, root.ToJsonString(JsonOptions) + Environment.NewLine, new UTF8Encoding(false));
        }

        Console.WriteLine($"Applied {applied:N0} translations.");
        return 0;
    }

    private static Array GetRawRows(object table)
    {
        MethodInfo method = table.GetType().GetMethod("GetRawDataUnsafe", BindingFlags.Instance | BindingFlags.Public)
            ?? throw new MissingMethodException(table.GetType().FullName, "GetRawDataUnsafe");
        return (Array)(method.Invoke(table, null) ?? throw new InvalidOperationException("GetRawDataUnsafe returned null."));
    }

    private static void AppendTyped(DatabaseBuilder builder, Type dataType, Array rows)
    {
        MethodInfo? append = typeof(DatabaseBuilder).GetMethods(BindingFlags.Instance | BindingFlags.Public)
            .Where(m => m.Name == "Append" && m.GetParameters().Length == 1)
            .FirstOrDefault(m =>
            {
                Type p = m.GetParameters()[0].ParameterType;
                return p.IsGenericType && p.GetGenericTypeDefinition() == typeof(IEnumerable<>) && p.GetGenericArguments()[0] == dataType;
            });
        if (append is null) throw new MissingMethodException($"DatabaseBuilder.Append(IEnumerable<{dataType.Name}>)");
        append.Invoke(builder, new object[] { rows });
    }

    private static DumpManifest LoadManifest(string dumpDirectory)
    {
        string path = Path.Combine(dumpDirectory, MetaDir, ManifestFile);
        if (!File.Exists(path)) throw new FileNotFoundException("Not a WDS MasterMemory dump (manifest missing).", path);
        return JsonSerializer.Deserialize<DumpManifest>(File.ReadAllText(path))
               ?? throw new InvalidDataException("Invalid manifest.");
    }

    private static string Sha256File(string path) => Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(path))).ToLowerInvariant();
    private static string Sha256Bytes(byte[] bytes) => Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant();

    private static readonly Regex JapaneseRegex = new(@"[\u3040-\u30ff\u3400-\u9fff]", RegexOptions.Compiled);
    private static bool LooksTranslatable(string s) => JapaneseRegex.IsMatch(s);

    private static void WalkStrings(JsonNode? node, string pointer, Action<string, string> onString)
    {
        switch (node)
        {
            case JsonObject obj:
                foreach (var kv in obj)
                    WalkStrings(kv.Value, pointer + "/" + EscapePointer(kv.Key), onString);
                break;
            case JsonArray arr:
                for (int i = 0; i < arr.Count; i++) WalkStrings(arr[i], pointer + "/" + i.ToString(CultureInfo.InvariantCulture), onString);
                break;
            case JsonValue val when val.TryGetValue<string>(out string? s) && s is not null:
                onString(pointer, s);
                break;
        }
    }

    private static void SetJsonPointer(JsonNode root, string pointer, JsonNode? value)
    {
        if (string.IsNullOrEmpty(pointer) || pointer[0] != '/') throw new InvalidDataException($"Invalid JSON pointer: {pointer}");
        string[] parts = pointer.Split('/').Skip(1).Select(UnescapePointer).ToArray();
        JsonNode current = root;
        for (int i = 0; i < parts.Length - 1; i++)
        {
            current = current switch
            {
                JsonObject o => o[parts[i]] ?? throw new KeyNotFoundException(pointer),
                JsonArray a when int.TryParse(parts[i], out int n) && n >= 0 && n < a.Count => a[n] ?? throw new KeyNotFoundException(pointer),
                _ => throw new KeyNotFoundException(pointer)
            };
        }
        string last = parts[^1];
        switch (current)
        {
            case JsonObject o: o[last] = value; break;
            case JsonArray a when int.TryParse(last, out int n) && n >= 0 && n < a.Count: a[n] = value; break;
            default: throw new KeyNotFoundException(pointer);
        }
    }

    private static string EscapePointer(string s) => s.Replace("~", "~0").Replace("/", "~1");
    private static string UnescapePointer(string s) => s.Replace("~1", "/").Replace("~0", "~");

    private static int FailUsage() { PrintHelp(); return 1; }
    private static void PrintHelp()
    {
        Console.WriteLine("WDS MasterMemory Tool (.NET / official MasterMemory toolchain)");
        Console.WriteLine("  unpack <mastermemory.db> <dump-dir>");
        Console.WriteLine("  pack <dump-dir> <output.db>");
        Console.WriteLine("  verify <mastermemory.db>");
        Console.WriteLine("  roundtrip <mastermemory.db> <work-dir>");
        Console.WriteLine("  extract-text <dump-dir> <translation.csv> [--all]");
        Console.WriteLine("  apply-text <dump-dir> <translation.csv>");
    }

    private sealed record TableEntry(string Name, string JsonFile, string JsonSha256, int RowCount);
    private sealed record DumpManifest(int Version, string OriginalSha256, long OriginalLength, List<TableEntry> Tables);
    private sealed record TextRecord(string Table, string Path, string Source, string Translation);
}

internal static class ModelJson
{
    private static readonly Dictionary<Type, PropertyInfo[]> KeyPropertyCache = new();

    public static JsonNode? ToNode(object? value, Type declaredType)
    {
        if (value is null) return null;
        Type type = Nullable.GetUnderlyingType(declaredType) ?? declaredType;

        if (type == typeof(string)) return JsonValue.Create((string)value);
        if (type == typeof(DateTime)) return JsonValue.Create(((DateTime)value).ToString("O", CultureInfo.InvariantCulture));
        if (type == typeof(TimeSpan)) return JsonValue.Create(((TimeSpan)value).ToString("c", CultureInfo.InvariantCulture));
        if (type == typeof(byte[])) return JsonValue.Create(Convert.ToBase64String((byte[])value));
        if (type.IsEnum) return JsonValue.Create(value.ToString());
        if (IsSimple(type)) return JsonSerializer.SerializeToNode(value, type);

        if (type.IsArray)
        {
            var arr = new JsonArray();
            Type elementType = type.GetElementType()!;
            foreach (object? item in (Array)value) arr.Add(ToNode(item, elementType));
            return arr;
        }

        var obj = new JsonObject();
        foreach (PropertyInfo p in KeyProperties(type))
            obj[p.Name] = ToNode(p.GetValue(value), p.PropertyType);
        return obj;
    }

    public static object? FromNode(JsonNode? node, Type declaredType)
    {
        Type? nullable = Nullable.GetUnderlyingType(declaredType);
        Type type = nullable ?? declaredType;
        if (node is null) return null;

        if (type == typeof(string)) return node.GetValue<string>();
        if (type == typeof(DateTime)) return DateTime.Parse(node.GetValue<string>(), CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind);
        if (type == typeof(TimeSpan)) return TimeSpan.Parse(node.GetValue<string>(), CultureInfo.InvariantCulture);
        if (type == typeof(byte[])) return Convert.FromBase64String(node.GetValue<string>());
        if (type.IsEnum)
        {
            if (node is JsonValue ev && ev.TryGetValue<string>(out string? enumName))
                return Enum.Parse(type, enumName!, ignoreCase: false);
            object raw = JsonSerializer.Deserialize(node.ToJsonString(), Enum.GetUnderlyingType(type))!;
            return Enum.ToObject(type, raw);
        }
        if (IsSimple(type)) return JsonSerializer.Deserialize(node.ToJsonString(), type);

        if (type.IsArray)
        {
            JsonArray src = node as JsonArray ?? throw new InvalidDataException($"Expected JSON array for {type.Name}");
            Type elementType = type.GetElementType()!;
            Array result = Array.CreateInstance(elementType, src.Count);
            for (int i = 0; i < src.Count; i++) result.SetValue(FromNode(src[i], elementType), i);
            return result;
        }

        JsonObject srcObj = node as JsonObject ?? throw new InvalidDataException($"Expected JSON object for {type.FullName}");
        object resultObj = Activator.CreateInstance(type) ?? throw new InvalidOperationException($"Can not construct {type.FullName}");
        foreach (PropertyInfo p in KeyProperties(type))
        {
            if (srcObj.TryGetPropertyValue(p.Name, out JsonNode? child))
                p.SetValue(resultObj, FromNode(child, p.PropertyType));
        }
        return resultObj;
    }

    private static bool IsSimple(Type t) => t.IsPrimitive || t == typeof(decimal) || t == typeof(Guid);

    private static PropertyInfo[] KeyProperties(Type type)
    {
        lock (KeyPropertyCache)
        {
            if (KeyPropertyCache.TryGetValue(type, out var cached)) return cached;
            var props = type.GetProperties(BindingFlags.Instance | BindingFlags.Public)
                .Select(p => (Property: p, Key: NumericKey(p)))
                .Where(x => x.Key >= 0 && x.Property.CanRead && x.Property.CanWrite)
                .OrderBy(x => x.Key)
                .Select(x => x.Property)
                .ToArray();
            KeyPropertyCache[type] = props;
            return props;
        }
    }

    private static int NumericKey(PropertyInfo p)
    {
        CustomAttributeData? attr = p.CustomAttributes.FirstOrDefault(a => a.AttributeType.FullName == typeof(KeyAttribute).FullName);
        if (attr is null || attr.ConstructorArguments.Count == 0) return -1;
        object? value = attr.ConstructorArguments[0].Value;
        return value is int n ? n : -1;
    }
}

internal static class Csv
{
    public static void WriteRow(TextWriter writer, params string[] values)
    {
        writer.WriteLine(string.Join(',', values.Select(Escape)));
    }

    private static string Escape(string value)
    {
        if (value.IndexOfAny(new[] { ',', '"', '\r', '\n' }) < 0) return value;
        return '"' + value.Replace("\"", "\"\"") + '"';
    }

    public static IEnumerable<string[]> ReadAll(string path)
    {
        using var reader = new StreamReader(path, Encoding.UTF8, detectEncodingFromByteOrderMarks: true);
        var row = new List<string>();
        var field = new StringBuilder();
        bool quoted = false;
        while (true)
        {
            int ci = reader.Read();
            if (ci < 0)
            {
                if (quoted) throw new InvalidDataException("Unterminated CSV quote.");
                if (field.Length > 0 || row.Count > 0) { row.Add(field.ToString()); yield return row.ToArray(); }
                yield break;
            }
            char c = (char)ci;
            if (quoted)
            {
                if (c == '"')
                {
                    if (reader.Peek() == '"') { reader.Read(); field.Append('"'); }
                    else quoted = false;
                }
                else field.Append(c);
            }
            else
            {
                if (c == '"' && field.Length == 0) quoted = true;
                else if (c == ',') { row.Add(field.ToString()); field.Clear(); }
                else if (c == '\n') { row.Add(field.ToString().TrimEnd('\r')); field.Clear(); yield return row.ToArray(); row.Clear(); }
                else field.Append(c);
            }
        }
    }
}
