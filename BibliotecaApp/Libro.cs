using System;
using System.IO;
using System.Text.Json;

// Clase de datos pura. No contiene lógica de estructuras (eso vive en Estructuras.cs).
public class Libro
{
    public string Codigo { get; set; }
    public string Titulo { get; set; }
    public string Autor { get; set; }
    public string Categoria { get; set; }
    public int CopiasDisponibles { get; set; }
    public int VecesPrestado { get; set; }

    // Constructor vacío requerido por el deserializador de JSON.
    public Libro() { }

    public Libro(string codigo, string titulo, string autor, string categoria, int copiasDisponibles, int vecesPrestado)
    {
        Codigo = codigo;
        Titulo = titulo;
        Autor = autor;
        Categoria = categoria;
        CopiasDisponibles = copiasDisponibles;
        VecesPrestado = vecesPrestado;
    }

    public override string ToString()
    {
        return $"[{Codigo}] {Titulo} - {Autor} ({Categoria}) | Copias: {CopiasDisponibles} | Prestamos: {VecesPrestado}";
    }
}

// Esto SÍ puede usar clases nativas de .NET (File, System.Text.Json, etc.)
// porque es soporte auxiliar de lectura/parseo, no lógica de las estructuras solicitadas.
// System.Text.Json solo convierte el archivo en un arreglo de objetos Libro;
// la inserción de esos libros en el Árbol B+ y los heaps se hace manualmente, uno por uno.
public static class CargadorArchivos
{
    public static Libro[] CargarLibrosDesdeJson(string rutaArchivo, out int cantidad)
    {
        if (!File.Exists(rutaArchivo))
        {
            Console.WriteLine($"Advertencia: no se encontró el archivo '{rutaArchivo}'. Se continuará sin datos precargados.");
            cantidad = 0;
            return Array.Empty<Libro>();
        }

        try
        {
            string contenidoJson = File.ReadAllText(rutaArchivo);
            var opciones = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            Libro[] libros = JsonSerializer.Deserialize<Libro[]>(contenidoJson, opciones);

            if (libros == null)
            {
                cantidad = 0;
                return Array.Empty<Libro>();
            }

            cantidad = libros.Length;
            return libros;
        }
        catch (JsonException ex)
        {
            Console.WriteLine($"Error al parsear el archivo JSON: {ex.Message}");
            cantidad = 0;
            return Array.Empty<Libro>();
        }
    }
}
