using System;
using System.IO;

public class Menu
{
    private ArbolBMas arbol;
    private MaxHeap heapMasPrestados;
    private MinHeap heapMenosPrestados;

    public Menu()
    {
        arbol = new ArbolBMas(orden: 4);
        heapMasPrestados = new MaxHeap();
        heapMenosPrestados = new MinHeap();
    }

    public void Iniciar()
    {
        CargarDatosIniciales();

        bool salir = false;
        while (!salir)
        {
            MostrarOpciones();
            string opcion = Console.ReadLine();

            switch (opcion)
            {
                case "1":
                    // TODO: registrar libro nuevo (manual) -> arbol.Insertar(...) + heaps
                    break;
                case "2":
                    // TODO: buscar libro por código -> arbol.Buscar(codigo)
                    break;
                case "3":
                    // TODO: listado ordenado por título (recorrer B+ + ordenar por título)
                    break;
                case "4":
                    // TODO: registrar préstamo (baja copias, sube VecesPrestado, actualiza heaps)
                    break;
                case "5":
                    // TODO: registrar devolución (sube copias disponibles)
                    break;
                case "6":
                    // TODO: mostrar libros más prestados -> heapMasPrestados.ExtraerMaximo() repetido
                    break;
                case "7":
                    // TODO: mostrar libros menos prestados -> heapMenosPrestados.ExtraerMinimo() repetido
                    break;
                case "8":
                    // TODO: eliminar libro por código -> arbol.Eliminar(codigo)
                    break;
                case "9":
                    // TODO: imprimir estructura del árbol B+ (para depurar/demostrar)
                    break;
                case "0":
                    salir = true;
                    break;
                default:
                    Console.WriteLine("Opción inválida.");
                    break;
            }
        }
    }

    private void CargarDatosIniciales()
    {
        string ruta = Path.Combine("Datos", "libros.json");
        Libro[] libros = CargadorArchivos.CargarLibrosDesdeJson(ruta, out int cantidad);

        // TODO: recorrer 'libros' (0..cantidad-1) e insertarlos en arbol, heapMasPrestados y heapMenosPrestados
    }

    private void MostrarOpciones()
    {
        Console.WriteLine("\n===== SISTEMA DE BIBLIOTECA =====");
        Console.WriteLine("1. Registrar libro nuevo");
        Console.WriteLine("2. Buscar libro por código");
        Console.WriteLine("3. Listar catálogo ordenado por título");
        Console.WriteLine("4. Registrar préstamo");
        Console.WriteLine("5. Registrar devolución");
        Console.WriteLine("6. Ver libros más prestados");
        Console.WriteLine("7. Ver libros menos prestados");
        Console.WriteLine("8. Eliminar libro");
        Console.WriteLine("9. Imprimir estructura del Árbol B+ (debug)");
        Console.WriteLine("0. Salir");
        Console.Write("Seleccione una opción: ");
    }
}

class Program
{
    static void Main(string[] args)
    {
        Menu menu = new Menu();
        menu.Iniciar();
    }
}
