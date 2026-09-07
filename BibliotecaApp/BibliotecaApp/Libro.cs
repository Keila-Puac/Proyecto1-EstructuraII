using System;
using System.IO;

namespace BibliotecaApp
{
    // Clase de datos pura. No contiene lógica de estructuras (eso vive en Estructuras.cs).
    public class Libro
    {
        public string Codigo { get; set; }
        public string Titulo { get; set; }
        public string Autor { get; set; }
        public string Categoria { get; set; }
        public int CopiasDisponibles { get; set; }
        public int VecesPrestado { get; set; }

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

    // Esto SÍ puede usar clases nativas de .NET (File, string.Split, etc.)
    // porque es soporte auxiliar de lectura/parseo, no lógica de las estructuras solicitadas.
    public static class CargadorArchivos
    {
        // Formato esperado por línea: codigo;titulo;autor;categoria;copiasDisponibles;vecesPrestado
        public static Libro[] CargarLibrosDesdeTxt(string rutaArchivo, out int cantidad)
        {
            if (!File.Exists(rutaArchivo))
            {
                Console.WriteLine($"Advertencia: no se encontró el archivo '{rutaArchivo}'. Se continuará sin datos precargados.");
                cantidad = 0;
                return Array.Empty<Libro>();
            }

            string[] lineas = File.ReadAllLines(rutaArchivo);
            Libro[] libros = new Libro[lineas.Length];
            int contador = 0;

            foreach (string linea in lineas)
            {
                if (string.IsNullOrWhiteSpace(linea)) continue;

                string[] campos = linea.Split(';');
                if (campos.Length != 6)
                {
                    Console.WriteLine($"Línea inválida, se omite: {linea}");
                    continue;
                }

                try
                {
                    string codigo = campos[0].Trim();
                    string titulo = campos[1].Trim();
                    string autor = campos[2].Trim();
                    string categoria = campos[3].Trim();
                    int copias = int.Parse(campos[4].Trim());
                    int prestamos = int.Parse(campos[5].Trim());

                    libros[contador] = new Libro(codigo, titulo, autor, categoria, copias, prestamos);
                    contador++;
                }
                catch (FormatException)
                {
                    Console.WriteLine($"Error de formato en línea, se omite: {linea}");
                }
            }

            cantidad = contador;
            return libros;
        }
    }
}
