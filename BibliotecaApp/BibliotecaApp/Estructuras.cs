using System;
using System.Collections.Generic;

namespace BibliotecaApp
{
    // Nodo genérico del Árbol B+.
    // Un nodo puede ser:
    //   - Interno: solo guarda claves (códigos) e hijos, sirve de índice.
    //   - Hoja: guarda claves y los Libro reales, y apunta a la SIGUIENTE hoja (lista enlazada de hojas).
    public class NodoBMas
    {
        public bool EsHoja;
        public string[] Claves;       // Códigos de libro usados como llaves de ordenamiento
        public Libro[] Libros;        // Solo se usa si EsHoja == true (paralelo a Claves)
        public NodoBMas[] Hijos;      // Solo se usa si EsHoja == false
        public NodoBMas Siguiente;    // Solo se usa si EsHoja == true (enlaza hojas entre sí)
        public int NumClaves;         // Cuántas posiciones de Claves están ocupadas actualmente

        public NodoBMas(int orden, bool esHoja)
        {
            EsHoja = esHoja;
            // TODO (juntos): reservar tamaño de los arreglos según 'orden'.
            // Pista: un nodo puede tener como máximo (orden - 1) claves antes de dividirse,
            // pero conviene reservar espacio para 'orden' claves temporalmente durante el split.
        }
    }

    // Árbol B+ propio (sin colecciones nativas de .NET para su lógica interna).
    // Indexado por Libro.Codigo.
    public class ArbolBMas
    {
        private NodoBMas raiz;
        private int orden; // Orden 4 por defecto: máximo 3 claves por nodo antes de dividir

        public ArbolBMas(int orden = 4)
        {
            this.orden = orden;
            raiz = new NodoBMas(orden, esHoja: true);
        }

        // Inserta un libro nuevo usando su Codigo como clave.
        public void Insertar(Libro libro)
        {
            // TODO (juntos):
            // 1. Bajar desde la raíz hasta la hoja correcta comparando 'libro.Codigo'.
            // 2. Insertar en la hoja en la posición ordenada correcta.
            // 3. Si la hoja se llena (NumClaves == orden), dividirla (split) y
            //    propagar la clave promovida hacia el padre (posible split en cadena
            //    hasta la raíz, lo que aumenta la altura del árbol).
            throw new NotImplementedException("Pendiente: lo implementamos juntos paso a paso.");
        }

        // Busca un libro por código. Retorna null si no existe.
        public Libro Buscar(string codigo)
        {
            // TODO (juntos):
            // 1. Desde la raíz, en cada nodo interno decidir por qué hijo bajar
            //    comparando 'codigo' contra las Claves del nodo.
            // 2. Al llegar a la hoja, buscar 'codigo' en sus Claves.
            throw new NotImplementedException("Pendiente: lo implementamos juntos paso a paso.");
        }

        // Elimina un libro por código.
        public bool Eliminar(string codigo)
        {
            // TODO (juntos):
            // 1. Encontrar la hoja que contiene 'codigo' (igual que en Buscar).
            // 2. Quitarlo de la hoja.
            // 3. Si la hoja queda con menos claves de las permitidas (underflow),
            //    redistribuir con un hermano o fusionar nodos, actualizando el padre.
            throw new NotImplementedException("Pendiente: lo implementamos juntos paso a paso.");
        }

        // Recorre todas las hojas en orden (de izquierda a derecha) y devuelve
        // la lista completa de libros ordenados por Codigo.
        // Esta es la gran ventaja del B+: no hace falta bajar/subir por el árbol,
        // basta con seguir los punteros 'Siguiente' de hoja en hoja.
        public List<Libro> RecorrerOrdenadoPorCodigo()
        {
            // TODO (juntos):
            // 1. Bajar por el hijo más a la izquierda desde la raíz hasta llegar a la primera hoja.
            // 2. Recorrer hoja por hoja usando 'Siguiente' hasta que sea null,
            //    acumulando los libros de cada una.
            throw new NotImplementedException("Pendiente: lo implementamos juntos paso a paso.");
        }

        // Imprime la estructura del árbol nivel por nivel (para depurar / demostrar en la defensa).
        public void ImprimirEstructura()
        {
            // TODO (juntos): recorrido por niveles (BFS) mostrando claves de cada nodo.
            throw new NotImplementedException("Pendiente: lo implementamos juntos paso a paso.");
        }
    }

    // Max Heap propio, ordenado por Libro.VecesPrestado (mayor primero).
    // Uso: obtener rápidamente los libros MÁS prestados (top N).
    public class MaxHeap
    {
        private Libro[] datos;
        private int tamano;
        private int capacidad;

        public MaxHeap(int capacidadInicial = 16)
        {
            capacidad = capacidadInicial;
            datos = new Libro[capacidad];
            tamano = 0;
        }

        public void Insertar(Libro libro)
        {
            // TODO (juntos):
            // 1. Si el arreglo está lleno, duplicar su capacidad (redimensionar manualmente).
            // 2. Colocar 'libro' en la posición 'tamano' y aumentar 'tamano'.
            // 3. SiftUp: mientras el padre tenga MENOS VecesPrestado que el hijo, intercambiar.
            throw new NotImplementedException("Pendiente: lo implementamos juntos paso a paso.");
        }

        public Libro ExtraerMaximo()
        {
            // TODO (juntos):
            // 1. Guardar datos[0] (la raíz) para retornarlo al final.
            // 2. Mover el último elemento a la posición 0 y reducir 'tamano'.
            // 3. SiftDown: comparar con ambos hijos y bajar mientras alguno sea mayor.
            throw new NotImplementedException("Pendiente: lo implementamos juntos paso a paso.");
        }

        // Muestra el heap sin modificarlo (recorrido simple del arreglo interno).
        public void Imprimir()
        {
            // TODO (juntos)
            throw new NotImplementedException("Pendiente: lo implementamos juntos paso a paso.");
        }

        public int Tamano => tamano;
    }

    // Min Heap propio, ordenado por Libro.VecesPrestado (menor primero).
    // Uso: obtener rápidamente los libros MENOS prestados (poco populares / candidatos a promoción o baja).
    public class MinHeap
    {
        private Libro[] datos;
        private int tamano;
        private int capacidad;

        public MinHeap(int capacidadInicial = 16)
        {
            capacidad = capacidadInicial;
            datos = new Libro[capacidad];
            tamano = 0;
        }

        public void Insertar(Libro libro)
        {
            // TODO (juntos): misma lógica que MaxHeap.Insertar pero comparando al revés
            // (el padre debe tener MENOS o igual VecesPrestado que sus hijos).
            throw new NotImplementedException("Pendiente: lo implementamos juntos paso a paso.");
        }

        public Libro ExtraerMinimo()
        {
            // TODO (juntos): misma lógica que MaxHeap.ExtraerMaximo pero invertida.
            throw new NotImplementedException("Pendiente: lo implementamos juntos paso a paso.");
        }

        public void Imprimir()
        {
            // TODO (juntos)
            throw new NotImplementedException("Pendiente: lo implementamos juntos paso a paso.");
        }

        public int Tamano => tamano;
    }
}
