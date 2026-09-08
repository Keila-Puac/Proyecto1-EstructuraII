using System;
using System.IO;

namespace BibliotecaApp
{
    //Información que se guarda en las hojas del arbol B+
    public class Libro
    {
        public int Codigo { get; set; }
        public string Titulo { get; set; }
        public string Autor { get; set; }
        public string Categoria { get; set; }
        public int CopiasDisponibles { get; set; }
        public int VecesPrestado { get; set; }

        // Cabeza de la lista enlazada con el historial de préstamos/devoluciones.
        public NodoPrestamo HistorialPrestamos { get; set; }

        public Libro(int codigo, string titulo, string autor, string categoria, int copiasDisponibles, int vecesPrestado = 0)
        {
            Codigo = codigo;
            Titulo = titulo;
            Autor = autor;
            Categoria = categoria;
            CopiasDisponibles = copiasDisponibles;
            VecesPrestado = vecesPrestado;
            HistorialPrestamos = null;
        }

        // Inserta un movimiento nuevo AL INICIO de la lista enlazada, así el
        // historial queda ordenado del más reciente al más antiguo sin tener
        // que recorrer nada
        public void RegistrarMovimiento(string tipo)
        {
            NodoPrestamo nuevo = new NodoPrestamo(tipo, DateTime.Now);
            nuevo.Siguiente = HistorialPrestamos;
            HistorialPrestamos = nuevo;
        }

        public void ImprimirHistorial()
        {
            if (HistorialPrestamos == null)
            {
                Console.WriteLine("  (Sin movimientos registrados)");
                return;
            }
            NodoPrestamo actual = HistorialPrestamos;
            while (actual != null)
            {
                Console.WriteLine($"  - {actual.Tipo} el {actual.Fecha:dd/MM/yyyy HH:mm}");
                actual = actual.Siguiente;
            }
        }

        public override string ToString()
        {
            return $"Código: {Codigo} | Título: {Titulo} | Autor: {Autor} | Categoría: {Categoria} | " +
                   $"Copias disponibles: {CopiasDisponibles} | Veces prestado: {VecesPrestado}";
        }
    }

    //  LISTA ENLAZADA DE PRÉSTAMOS (historial de un libro)
    public class NodoPrestamo
    {
        public string Tipo;         // "Préstamo" o "Devolución"
        public DateTime Fecha;
        public NodoPrestamo Siguiente;

        public NodoPrestamo(string tipo, DateTime fecha)
        {
            Tipo = tipo;
            Fecha = fecha;
            Siguiente = null;
        }
    }

    // =========================================================================
    //  PROGRAMA PRINCIPAL
    // =========================================================================
    class Program
    {
        static void Main(string[] args)
        {
            // El catálogo completo vive en un único Árbol B+, indexado por
            // Código (la llave única de cada libro).
            ArbolBMas catalogo = new ArbolBMas();
            bool salir = false;

            Console.WriteLine("=====================================================");
            Console.WriteLine("   SISTEMA DE GESTIÓN DE CATÁLOGO DE BIBLIOTECA");
            Console.WriteLine("=====================================================");

            while (!salir)
            {
                Console.WriteLine();
                Console.WriteLine("----------------- MENÚ PRINCIPAL -----------------");
                Console.WriteLine(" 1. Cargar libros desde archivo .csv");
                Console.WriteLine(" 2. Registrar nuevo libro");
                Console.WriteLine(" 3. Buscar libro por código");
                Console.WriteLine(" 4. Registrar préstamo");
                Console.WriteLine(" 5. Registrar devolución");
                Console.WriteLine(" 6. Listar catálogo completo (orden por código)");
                Console.WriteLine(" 7. Listar catálogo ordenado por título (Min Heap)");
                Console.WriteLine(" 8. Top 5 libros más prestados (Max Heap)");
                Console.WriteLine(" 9. Eliminar libro");
                Console.WriteLine("10. Ver estructura interna del Árbol B+");
                Console.WriteLine(" 0. Salir");
                Console.Write("Selecciona una opción: ");

                string opcion = Console.ReadLine();
                Console.WriteLine();

                switch (opcion)
                {
                    case "1":
                        Console.WriteLine("---Carga libros desde un archivo .csv---");
                        Console.Write("Ruta del archivo .csv: ");
                        string ruta = Console.ReadLine();
                        CargarDesdeArchivo(catalogo, ruta);
                        break;
                    case "2":
                        Console.WriteLine("---Registrar un nuevo libro---");
                        RegistrarLibroManual(catalogo);
                        break;
                    case "3":
                        Console.WriteLine("---Buscar libro por código---");
                        BuscarLibro(catalogo);
                        break;
                    case "4":
                        Console.WriteLine("---Registrar Préstamo---");
                        RegistrarPrestamo(catalogo);
                        break;
                    case "5":
                        Console.WriteLine("---Registrar Devolución---");
                        RegistrarDevolucion(catalogo);
                        break;
                    case "6":
                        Console.WriteLine("---Listado de catalogo (Por Código)---");
                        ListarPorCodigo(catalogo);
                        break;
                    case "7":
                        Console.WriteLine("---Lista de catalogo (Por Titulo)---");
                        ListarPorTitulo(catalogo);
                        break;
                    case "8":
                        Console.WriteLine("---Top 5 Libros Mas Prestados---");
                        MostrarTopPrestados(catalogo);
                        break;
                    case "9":
                        Console.WriteLine("---Eliminar Libro---");
                        EliminarLibro(catalogo);
                        break;
                    case "10":
                        Console.WriteLine("---Estructura del Árbol B+---");
                        catalogo.Imprimir();
                        break;
                    case "0":
                        salir = true;
                        Console.WriteLine("Saliendo...");
                        break;
                    default:
                        Console.WriteLine("Opción no válida, intenta de nuevo.");
                        break;
                }
            }
        }

        // ---------------------------------------------------------------
        // Carga masiva desde un archivo .csv con formato:
        // codigo,titulo,autor,categoria,copiasDisponibles[,vecesPrestado]
        // File.ReadAllLines es una función NATIVA usada solo como soporte
        // auxiliar de lectura de archivo
        // ---------------------------------------------------------------
        static void CargarDesdeArchivo(ArbolBMas arbol, string ruta)
        {
            if (!File.Exists(ruta))
            {
                Console.WriteLine("El archivo no existe: " + ruta);
                return;
            }

            string[] lineas = File.ReadAllLines(ruta);
            int cargados = 0;

            foreach (string linea in lineas)
            {
                if (string.IsNullOrWhiteSpace(linea)) continue;

                string[] campos = linea.Split(',');
                if (campos.Length < 5)
                {
                    Console.WriteLine("Línea con formato inválido, se omite: " + linea);
                    continue;
                }

                try
                {
                    int codigo = int.Parse(campos[0].Trim());
                    string titulo = campos[1].Trim();
                    string autor = campos[2].Trim();
                    string categoria = campos[3].Trim();
                    int copias = int.Parse(campos[4].Trim());
                    int veces = campos.Length >= 6 ? int.Parse(campos[5].Trim()) : 0;

                    if (arbol.Buscar(codigo) != null)
                    {
                        Console.WriteLine($"Código duplicado {codigo}, se omite esa línea.");
                        continue;
                    }

                    Libro nuevo = new Libro(codigo, titulo, autor, categoria, copias, veces);
                    arbol.Insertar(codigo, nuevo);
                    cargados++;
                }
                catch
                {
                    Console.WriteLine("Línea con formato inválido, se omite: " + linea);
                }
            }

            Console.WriteLine($"Se cargaron {cargados} libro(s) desde el archivo.");
        }

        static void RegistrarLibroManual(ArbolBMas arbol)
        {
            Console.Write("Código único: ");
            int codigo = LeerEntero();
            if (arbol.Buscar(codigo) != null)
            {
                Console.WriteLine("Ya existe un libro con ese código.");
                return;
            }

            Console.Write("Título: ");
            string titulo = Console.ReadLine();
            Console.Write("Autor: ");
            string autor = Console.ReadLine();
            Console.Write("Categoría: ");
            string categoria = Console.ReadLine();
            Console.Write("Copias disponibles: ");
            int copias = LeerEntero();

            Libro nuevo = new Libro(codigo, titulo, autor, categoria, copias);
            arbol.Insertar(codigo, nuevo);
            Console.WriteLine("Libro registrado correctamente.");
        }

        static void BuscarLibro(ArbolBMas arbol)
        {
            Console.Write("Código a buscar: ");
            int codigo = LeerEntero();
            Libro libro = arbol.Buscar(codigo);

            if (libro == null)
            {
                Console.WriteLine("No se encontró ningún libro con ese código.");
                return;
            }

            Console.WriteLine(libro.ToString());
            Console.Write("¿Ver historial de préstamos? (s/n): ");
            string resp = Console.ReadLine();
            if (resp != null && resp.Trim().ToLower() == "s")
                libro.ImprimirHistorial();
        }

        static void RegistrarPrestamo(ArbolBMas arbol)
        {
            Console.Write("Código del libro a prestar: ");
            int codigo = LeerEntero();
            Libro libro = arbol.Buscar(codigo);

            if (libro == null)
            {
                Console.WriteLine("No existe un libro con ese código.");
                return;
            }
            if (libro.CopiasDisponibles <= 0)
            {
                Console.WriteLine("No hay copias disponibles para préstamo.");
                return;
            }

            libro.CopiasDisponibles--;
            libro.VecesPrestado++;
            libro.RegistrarMovimiento("Préstamo");
            Console.WriteLine("Préstamo registrado. Copias disponibles ahora: " + libro.CopiasDisponibles);
        }

        static void RegistrarDevolucion(ArbolBMas arbol)
        {
            Console.Write("Código del libro a devolver: ");
            int codigo = LeerEntero();
            Libro libro = arbol.Buscar(codigo);

            if (libro == null)
            {
                Console.WriteLine("No existe un libro con ese código.");
                return;
            }

            libro.CopiasDisponibles++;
            libro.RegistrarMovimiento("Devolución");
            Console.WriteLine("Devolución registrada. Copias disponibles ahora: " + libro.CopiasDisponibles);
        }

        // Recorrido directo del Árbol B+ (usa la lista enlazada de hojas):
        // da el catálogo en orden ascendente de CÓDIGO, sin estructuras extra.
        static void ListarPorCodigo(ArbolBMas arbol)
        {
            Console.WriteLine("--- Catálogo ordenado por código (recorrido del Árbol B+) ---");
            bool hayDatos = false;
            arbol.Recorrer(libro => { Console.WriteLine(libro.ToString()); hayDatos = true; });
            if (!hayDatos) Console.WriteLine("El catálogo está vacío.");
        }

        // Usa el MIN HEAP: se insertan todos los libros (O(n)) y luego se
        // extrae el mínimo repetidamente (Heap Sort) para obtener el orden
        // alfabético por título.
        static void ListarPorTitulo(ArbolBMas arbol)
        {
            MinHeapLibros heap = new MinHeapLibros();
            arbol.Recorrer(libro => heap.Insertar(libro));

            if (heap.EstaVacio())
            {
                Console.WriteLine("El catálogo está vacío.");
                return;
            }

            Console.WriteLine("--- Catálogo ordenado por título (Min Heap) ---");
            while (!heap.EstaVacio())
            {
                Libro libro = heap.ExtraerMinimo();
                Console.WriteLine(libro.ToString());
            }
        }

        // Usa el MAX HEAP: se insertan todos los libros y se extrae el
        // máximo (mayor VecesPrestado) 5 veces, sin ordenar todo el catálogo.
        static void MostrarTopPrestados(ArbolBMas arbol, int top = 5)
        {
            MaxHeapLibros heap = new MaxHeapLibros();
            arbol.Recorrer(libro => heap.Insertar(libro));

            if (heap.EstaVacio())
            {
                Console.WriteLine("No hay libros registrados.");
                return;
            }

            Console.WriteLine($"--- Top {top} libros más prestados (Max Heap) ---");
            int mostrados = 0;
            while (!heap.EstaVacio() && mostrados < top)
            {
                Libro libro = heap.ExtraerMaximo();
                Console.WriteLine($"{mostrados + 1}. {libro.Titulo} - {libro.VecesPrestado} préstamos (código {libro.Codigo})");
                mostrados++;
            }
        }

        static void EliminarLibro(ArbolBMas arbol)
        {
            Console.Write("Código del libro a eliminar: ");
            int codigo = LeerEntero();

            if (arbol.Buscar(codigo) == null)
            {
                Console.WriteLine("No existe un libro con ese código.");
                return;
            }

            arbol.Eliminar(codigo);
            Console.WriteLine("Libro eliminado correctamente.");
        }

        // Lee un entero validando la entrada (evita que el programa truene
        // si el usuario escribe texto donde se esperaba un número).
        static int LeerEntero()
        {
            int valor;
            while (!int.TryParse(Console.ReadLine(), out valor))
            {
                Console.Write("Por favor ingresa un número válido: ");
            }
            return valor;
        }
    }
}
