using System.Data;
using System.Data.Common;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace ApiTransporteLweb.Helpers
{
    // Ejecuta un procedimiento almacenado con parámetros POSICIONALES y devuelve
    // el primer result set como lista de filas (columna -> valor).
    // No requiere conocer los nombres de los parámetros ni de las columnas del SP.
    public static class ReporteSpHelper
    {
        public static async Task<(List<string> columnas, List<Dictionary<string, object?>> filas)> EjecutarAsync(
            DbContext context,
            string nombreSp,
            string[] parametros,
            CancellationToken ct = default)
        {
            var conexion = context.Database.GetDbConnection();
            var abrirYCerrar = conexion.State != ConnectionState.Open;
            if (abrirYCerrar)
                await conexion.OpenAsync(ct);

            try
            {
                await using var cmd = conexion.CreateCommand();
                cmd.CommandType = CommandType.Text;
                cmd.CommandTimeout = 170;

                // EXEC [dbo].[Sp_x] @p0, @p1, @p2  -> los valores se pasan por posición
                var nombresParam = new List<string>();
                for (int i = 0; i < parametros.Length; i++)
                {
                    var p = new SqlParameter($"@p{i}", SqlDbType.VarChar, 20) { Value = parametros[i] };
                    cmd.Parameters.Add(p);
                    nombresParam.Add(p.ParameterName);
                }
                // ARITHABORT ON replica la configuración de SSMS: ADO.NET lo trae en OFF y eso puede
                // generar otro plan de ejecución (mucho más lento) para el mismo SP.
                cmd.CommandText = $"SET ARITHABORT ON; EXEC [dbo].[{nombreSp}] {string.Join(", ", nombresParam)}";

                await using DbDataReader reader = await cmd.ExecuteReaderAsync(ct);

                var columnas = new List<string>();
                var usados = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                for (int i = 0; i < reader.FieldCount; i++)
                {
                    var nombre = reader.GetName(i);
                    if (string.IsNullOrWhiteSpace(nombre)) nombre = $"columna{i + 1}";
                    var unico = nombre;
                    int n = 2;
                    while (!usados.Add(unico)) unico = $"{nombre}_{n++}";
                    columnas.Add(unico);
                }

                var filas = new List<Dictionary<string, object?>>();
                while (await reader.ReadAsync(ct))
                {
                    var fila = new Dictionary<string, object?>(columnas.Count);
                    for (int i = 0; i < columnas.Count; i++)
                    {
                        var valor = reader.GetValue(i);
                        fila[columnas[i]] = valor is DBNull
                            ? null
                            : valor is string s ? s.TrimEnd() : valor;
                    }
                    filas.Add(fila);
                }

                return (columnas, filas);
            }
            finally
            {
                if (abrirYCerrar)
                    await conexion.CloseAsync();
            }
        }
    }
}
