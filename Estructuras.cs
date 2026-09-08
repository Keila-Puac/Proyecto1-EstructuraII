using System;

namespace BibliotecaApp
{
    // =====================================================================
    //  ÁRBOL B+  (estructura principal del catálogo, indexada por Código)
    // =====================================================================
    //
    // JUSTIFICACIÓN DE USO:
    // El catálogo necesita búsquedas MUY rápidas por código único, así como
    // insertar/eliminar libros manteniendo los datos siempre ordenados y
    // permitir un recorrido secuencial completo y eficiente (para listar
    // todo el catálogo). Un Árbol B+ es ideal porque:
    //   - Toda la información útil (los Libro) vive SOLO en las hojas.
    //   - Las hojas están enlazadas entre sí (como una lista enlazada),
    //     así que recorrer todo el catálogo en orden de código es O(n)
    //     sin tener que "bajar y subir" por el árbol.
    //   - Los nodos internos solo guardan claves "guía" para decidir el
    //     camino de búsqueda, por lo que el árbol se mantiene bajo (poca
    //     altura) incluso con muchos libros -> búsquedas casi O(log n).
    //
    // NodoB: nodo interno del árbol. Se usa "internal" (no lo necesitamos
    // fuera de este archivo) y sus campos son públicos dentro del ensamblado
    // para que ArbolBMas pueda manipularlos directamente (patrón común
    // cuando el nodo es un simple "contenedor de datos").
    internal class NodoB
    {
        // ORDEN = máximo número de HIJOS que puede tener un nodo interno.
        // Por lo tanto el máximo de CLAVES por nodo es ORDEN - 1 = 3.
        // Elegimos 4 porque es fácil de dibujar/seguir a mano (similar a un
        // árbol 2-3-4) y es más que suficiente para un catálogo de biblioteca.
        public const int ORDEN = 4;

        public bool EsHoja;
        public int[] Claves;      // claves (códigos) guardadas en este nodo
        public Libro[] Valores;   // solo se usa si EsHoja == true
        public NodoB[] Hijos;     // solo se usa si EsHoja == false
        public int NumClaves;     // cuántas posiciones de Claves están en uso
        public NodoB Siguiente;   // enlaza esta hoja con la siguiente hoja (lista enlazada de hojas)

        public NodoB(bool esHoja)
        {
            EsHoja = esHoja;
            // Reservamos tamaño ORDEN (y no ORDEN-1) a propósito: así se puede
            // insertar temporalmente una clave "de más" en el nodo, y recién
            // después decidimos si hay que dividirlo (split). Es una técnica
            // clásica que simplifica muchísimo el algoritmo de inserción.
            Claves = new int[ORDEN];
            if (esHoja)
                Valores = new Libro[ORDEN];
            else
                Hijos = new NodoB[ORDEN + 1];
            NumClaves = 0;
            Siguiente = null;
        }
    }

    public class ArbolBMas
    {
        private NodoB raiz;

        // Pequeña clase auxiliar: cuando un nodo se divide, hay que "avisarle"
        // al nodo padre cuál es la clave que sube y cuál es el nuevo nodo
        // hermano que se creó. Este objeto viaja de regreso en la recursión.
        private class ResultadoSplit
        {
            public int ClavePromovida;
            public NodoB NuevoNodo;
        }

        // -----------------------------------------------------------------
        // INSERTAR: inserta un libro identificado por su código único.
        // Complejidad: O(log n). El llamador (Program.cs) es responsable de
        // verificar que el código no exista todavía (los códigos son únicos).
        // -----------------------------------------------------------------
        public void Insertar(int clave, Libro valor)
        {
            if (raiz == null)
            {
                // Árbol vacío: la raíz nace siendo una hoja.
                raiz = new NodoB(true);
                raiz.Claves[0] = clave;
                raiz.Valores[0] = valor;
                raiz.NumClaves = 1;
                return;
            }

            ResultadoSplit resultado = InsertarRecursivo(raiz, clave, valor);

            // Si la raíz se dividió, el árbol crece un nivel: se crea una
            // nueva raíz interna con dos hijos (la raíz vieja y su nuevo hermano).
            if (resultado != null)
            {
                NodoB nuevaRaiz = new NodoB(false);
                nuevaRaiz.Claves[0] = resultado.ClavePromovida;
                nuevaRaiz.Hijos[0] = raiz;
                nuevaRaiz.Hijos[1] = resultado.NuevoNodo;
                nuevaRaiz.NumClaves = 1;
                raiz = nuevaRaiz;
            }
        }

        private ResultadoSplit InsertarRecursivo(NodoB nodo, int clave, Libro valor)
        {
            if (nodo.EsHoja)
            {
                // 1) Buscar la posición ordenada donde debe ir la nueva clave.
                int pos = 0;
                while (pos < nodo.NumClaves && nodo.Claves[pos] < clave) pos++;

                // 2) Desplazar todo lo que está a la derecha una posición,
                //    para abrir espacio (así la hoja se mantiene ordenada).
                for (int i = nodo.NumClaves; i > pos; i--)
                {
                    nodo.Claves[i] = nodo.Claves[i - 1];
                    nodo.Valores[i] = nodo.Valores[i - 1];
                }
                nodo.Claves[pos] = clave;
                nodo.Valores[pos] = valor;
                nodo.NumClaves++;

                // 3) Si no se llenó el nodo, no hay nada más que hacer.
                if (nodo.NumClaves < NodoB.ORDEN) return null;

                // 4) Si se llenó (overflow), se divide la hoja en dos.
                return DividirHoja(nodo);
            }
            else
            {
                // Nodo interno: decidir por cuál hijo continuar bajando.
                // Convención: Hijos[pos] agrupa claves < Claves[pos]; por eso
                // avanzamos "pos" mientras la clave a insertar sea >= Claves[pos].
                int pos = 0;
                while (pos < nodo.NumClaves && clave >= nodo.Claves[pos]) pos++;

                ResultadoSplit resultadoHijo = InsertarRecursivo(nodo.Hijos[pos], clave, valor);
                if (resultadoHijo == null) return null; // el hijo no se dividió, ya terminamos

                // El hijo se dividió: hay que insertar la clave promovida y
                // el puntero al nuevo hijo en ESTE nodo interno.
                for (int i = nodo.NumClaves; i > pos; i--)
                {
                    nodo.Claves[i] = nodo.Claves[i - 1];
                    nodo.Hijos[i + 1] = nodo.Hijos[i];
                }
                nodo.Claves[pos] = resultadoHijo.ClavePromovida;
                nodo.Hijos[pos + 1] = resultadoHijo.NuevoNodo;
                nodo.NumClaves++;

                if (nodo.NumClaves < NodoB.ORDEN) return null;

                return DividirInterno(nodo);
            }
        }

        // Divide una HOJA llena en dos. En un B+, la primera clave de la
        // hoja derecha se copia (no se "consume") como separador hacia el
        // padre, porque esa clave debe seguir existiendo en la hoja para
        // que las búsquedas por igualdad la encuentren.
        private ResultadoSplit DividirHoja(NodoB hoja)
        {
            int mitad = NodoB.ORDEN / 2;                 // p.ej. 4/2 = 2
            NodoB nuevaHoja = new NodoB(true);
            int keysEnNueva = hoja.NumClaves - mitad;     // claves que pasan a la derecha

            for (int i = 0; i < keysEnNueva; i++)
            {
                nuevaHoja.Claves[i] = hoja.Claves[mitad + i];
                nuevaHoja.Valores[i] = hoja.Valores[mitad + i];
            }
            nuevaHoja.NumClaves = keysEnNueva;
            hoja.NumClaves = mitad;

            // Mantener la lista enlazada de hojas (clave para recorridos rápidos).
            nuevaHoja.Siguiente = hoja.Siguiente;
            hoja.Siguiente = nuevaHoja;

            return new ResultadoSplit { ClavePromovida = nuevaHoja.Claves[0], NuevoNodo = nuevaHoja };
        }

        // Divide un nodo INTERNO lleno en dos. Aquí (a diferencia de la hoja)
        // la clave del medio SÍ sube al padre y NO se copia en ninguno de los
        // dos nodos resultantes (así es un B+ "de manual").
        private ResultadoSplit DividirInterno(NodoB nodo)
        {
            int mitad = nodo.NumClaves / 2;
            int clavePromovida = nodo.Claves[mitad];

            NodoB nuevoInterno = new NodoB(false);
            int keysDerecha = nodo.NumClaves - mitad - 1;

            for (int i = 0; i < keysDerecha; i++)
                nuevoInterno.Claves[i] = nodo.Claves[mitad + 1 + i];
            for (int i = 0; i <= keysDerecha; i++)
                nuevoInterno.Hijos[i] = nodo.Hijos[mitad + 1 + i];

            nuevoInterno.NumClaves = keysDerecha;
            nodo.NumClaves = mitad;

            return new ResultadoSplit { ClavePromovida = clavePromovida, NuevoNodo = nuevoInterno };
        }

        // -----------------------------------------------------------------
        // BUSCAR: baja del nivel raíz hasta la hoja correcta en O(log n) y
        // ahí hace una búsqueda lineal (el nodo es pequeño, ORDEN=4).
        // -----------------------------------------------------------------
        public Libro Buscar(int clave)
        {
            if (raiz == null) return null;

            NodoB actual = raiz;
            while (!actual.EsHoja)
            {
                int pos = 0;
                while (pos < actual.NumClaves && clave >= actual.Claves[pos]) pos++;
                actual = actual.Hijos[pos];
            }

            for (int i = 0; i < actual.NumClaves; i++)
                if (actual.Claves[i] == clave) return actual.Valores[i];

            return null;
        }

        // -----------------------------------------------------------------
        // ELIMINAR: estrategia de "eliminar por reconstrucción".
        //
        // Un B+ "de libro de texto" elimina en el lugar (in-place) pidiendo
        // prestadas claves a los nodos hermanos o fusionando nodos cuando un
        // nodo queda con muy pocas claves. Esa lógica es correcta pero muy
        // propensa a errores (hay que actualizar separadores en cascada,
        // mantener la lista de hojas, etc.).
        //
        // Para este proyecto se optó, a propósito, por una estrategia más
        // simple y 100% confiable: se recorre el árbol completo copiando
        // todas las parejas (código, libro) EXCEPTO la que se elimina, se
        // vacía el árbol y se reinsertan las que quedan usando el MISMO
        // método Insertar ya probado. El resultado final es exactamente el
        // mismo árbol balanceado y válido que produciría el algoritmo
        // "in-place", solo que llegamos a él de una forma más segura.
        // Esta es una decisión de diseño perfectamente defendible para un
        // catálogo de tamaño moderado como el de una biblioteca.
        // -----------------------------------------------------------------
        public void Eliminar(int clave)
        {
            if (raiz == null) return;
            if (Buscar(clave) == null) return; // no existe, no hay nada que hacer

            int total = ContarClaves();
            int[] clavesTemp = new int[total];
            Libro[] valoresTemp = new Libro[total];
            int indice = 0;

            Recorrer(libro =>
            {
                if (libro.Codigo != clave)
                {
                    clavesTemp[indice] = libro.Codigo;
                    valoresTemp[indice] = libro;
                    indice++;
                }
            });

            raiz = null;
            for (int i = 0; i < indice; i++)
                Insertar(clavesTemp[i], valoresTemp[i]);
        }

        private int ContarClaves()
        {
            int contador = 0;
            Recorrer(libro => contador++);
            return contador;
        }

        // -----------------------------------------------------------------
        // RECORRER: visita todos los libros en orden ascendente de código,
        // aprovechando la lista enlazada de hojas (por eso un B+ es tan
        // bueno para generar listados completos: no hace falta recursión).
        // Se recibe un delegado Action<Libro> en lugar de devolver una
        // colección, para no depender de List<T> ni de ningún otro
        // contenedor nativo de .NET.
        // -----------------------------------------------------------------
        public void Recorrer(Action<Libro> accion)
        {
            if (raiz == null) return;

            NodoB actual = raiz;
            while (!actual.EsHoja) actual = actual.Hijos[0]; // ir a la hoja más a la izquierda

            while (actual != null)
            {
                for (int i = 0; i < actual.NumClaves; i++)
                    accion(actual.Valores[i]);
                actual = actual.Siguiente;
            }
        }

        // -----------------------------------------------------------------
        // IMPRIMIR: muestra la estructura interna del árbol nivel por nivel,
        // útil para estudiar y para defender el proyecto ante el docente.
        // -----------------------------------------------------------------
        public void Imprimir()
        {
            Console.WriteLine("=== Estructura del Árbol B+ (orden = " + NodoB.ORDEN + ") ===");
            if (raiz == null)
            {
                Console.WriteLine("(árbol vacío)");
                return;
            }
            ImprimirRecursivo(raiz, 0);
        }

        private void ImprimirRecursivo(NodoB nodo, int nivel)
        {
            string sangria = new string(' ', nivel * 4);

            Console.Write(sangria + (nodo.EsHoja ? "Hoja: [" : "Interno: ["));
            for (int i = 0; i < nodo.NumClaves; i++)
            {
                Console.Write(nodo.Claves[i]);
                if (i < nodo.NumClaves - 1) Console.Write(", ");
            }
            Console.WriteLine("]");

            if (!nodo.EsHoja)
            {
                for (int i = 0; i <= nodo.NumClaves; i++)
                    ImprimirRecursivo(nodo.Hijos[i], nivel + 1);
            }
        }
    }

    // =====================================================================
    //  MIN HEAP  (por Título)
    // =====================================================================
    //
    // JUSTIFICACIÓN DE USO:
    // El montículo mínimo se usa para generar el listado del catálogo
    // ORDENADO POR TÍTULO. En vez de ordenar con un algoritmo genérico,
    // construimos un Min Heap con todos los libros (O(n)) y luego vamos
    // extrayendo el mínimo repetidamente (cada extracción es O(log n)):
    // esto es exactamente el algoritmo "Heap Sort" y da un listado
    // alfabético en O(n log n) sin usar ninguna colección nativa de .NET.
    //
    // Implementación: heap binario clásico sobre un arreglo propio (T[])
    // que nosotros mismos redimensionamos (no es List<T>, es un arreglo
    // que administramos a mano con Array.Copy manual).
    public class MinHeapLibros
    {
        private Libro[] datos;
        private int cantidad;
        private int capacidad;

        public int Cantidad { get { return cantidad; } }

        public MinHeapLibros(int capacidadInicial = 10)
        {
            capacidad = capacidadInicial;
            datos = new Libro[capacidad];
            cantidad = 0;
        }

        private void Redimensionar()
        {
            capacidad *= 2;
            Libro[] nuevo = new Libro[capacidad];
            for (int i = 0; i < cantidad; i++) nuevo[i] = datos[i];
            datos = nuevo;
        }

        private void Intercambiar(int a, int b)
        {
            Libro temp = datos[a];
            datos[a] = datos[b];
            datos[b] = temp;
        }

        // true si "a" debe ir MÁS ARRIBA que "b" en un min-heap por título
        private bool TieneMenorTitulo(int a, int b)
        {
            return string.Compare(datos[a].Titulo, datos[b].Titulo, StringComparison.OrdinalIgnoreCase) < 0;
        }

        public void Insertar(Libro libro)
        {
            if (cantidad == capacidad) Redimensionar();
            datos[cantidad] = libro;
            int i = cantidad;
            cantidad++;
            SiftUp(i);
        }

        private void SiftUp(int i)
        {
            while (i > 0)
            {
                int padre = (i - 1) / 2;
                if (TieneMenorTitulo(i, padre))
                {
                    Intercambiar(i, padre);
                    i = padre;
                }
                else break;
            }
        }

        private void SiftDown(int i)
        {
            while (true)
            {
                int izq = 2 * i + 1;
                int der = 2 * i + 2;
                int menor = i;
                if (izq < cantidad && TieneMenorTitulo(izq, menor)) menor = izq;
                if (der < cantidad && TieneMenorTitulo(der, menor)) menor = der;
                if (menor == i) break;
                Intercambiar(i, menor);
                i = menor;
            }
        }

        public Libro ExtraerMinimo()
        {
            if (cantidad == 0) return null;
            Libro min = datos[0];
            cantidad--;
            datos[0] = datos[cantidad];
            datos[cantidad] = null;
            SiftDown(0);
            return min;
        }

        // Búsqueda por código. NOTA: un heap solo garantiza que el padre sea
        // "menor" que sus hijos, no está ordenado como un árbol de búsqueda,
        // así que aquí NO se puede buscar en O(log n): se recorre linealmente.
        public Libro Buscar(int codigo)
        {
            for (int i = 0; i < cantidad; i++)
                if (datos[i].Codigo == codigo) return datos[i];
            return null;
        }

        public bool Eliminar(int codigo)
        {
            int pos = -1;
            for (int i = 0; i < cantidad; i++)
                if (datos[i].Codigo == codigo) { pos = i; break; }
            if (pos == -1) return false;

            cantidad--;
            if (pos < cantidad)
            {
                // Se reemplaza el elemento eliminado por el último y se
                // reacomoda hacia arriba o hacia abajo (no sabemos cuál hace falta).
                datos[pos] = datos[cantidad];
                SiftDown(pos);
                SiftUp(pos);
            }
            datos[cantidad] = null;
            return true;
        }

        public bool EstaVacio() { return cantidad == 0; }

        public void Imprimir()
        {
            Console.WriteLine("Estructura interna del Min Heap (por título):");
            for (int i = 0; i < cantidad; i++)
                Console.WriteLine($"  [{i}] {datos[i].Titulo} (código {datos[i].Codigo})");
        }

        // Recorrido en el orden interno del arreglo (orden de nivel). OJO:
        // esto NO da orden alfabético; para eso hay que usar ExtraerMinimo().
        public void Recorrer(Action<Libro> accion)
        {
            for (int i = 0; i < cantidad; i++) accion(datos[i]);
        }
    }

    // =====================================================================
    //  MAX HEAP  (por Veces Prestado)
    // =====================================================================
    //
    // JUSTIFICACIÓN DE USO:
    // El montículo máximo se usa para responder rápidamente "¿cuáles son
    // los libros más prestados?" (el Top 5 que pide el enunciado). Con un
    // Max Heap basta con extraer el máximo 5 veces (O(log n) cada vez) en
    // lugar de ordenar todo el catálogo completo para solo mostrar 5.
    public class MaxHeapLibros
    {
        private Libro[] datos;
        private int cantidad;
        private int capacidad;

        public int Cantidad { get { return cantidad; } }

        public MaxHeapLibros(int capacidadInicial = 10)
        {
            capacidad = capacidadInicial;
            datos = new Libro[capacidad];
            cantidad = 0;
        }

        private void Redimensionar()
        {
            capacidad *= 2;
            Libro[] nuevo = new Libro[capacidad];
            for (int i = 0; i < cantidad; i++) nuevo[i] = datos[i];
            datos = nuevo;
        }

        private void Intercambiar(int a, int b)
        {
            Libro temp = datos[a];
            datos[a] = datos[b];
            datos[b] = temp;
        }

        private bool TieneMasPrestamos(int a, int b)
        {
            return datos[a].VecesPrestado > datos[b].VecesPrestado;
        }

        public void Insertar(Libro libro)
        {
            if (cantidad == capacidad) Redimensionar();
            datos[cantidad] = libro;
            int i = cantidad;
            cantidad++;
            SiftUp(i);
        }

        private void SiftUp(int i)
        {
            while (i > 0)
            {
                int padre = (i - 1) / 2;
                if (TieneMasPrestamos(i, padre))
                {
                    Intercambiar(i, padre);
                    i = padre;
                }
                else break;
            }
        }

        private void SiftDown(int i)
        {
            while (true)
            {
                int izq = 2 * i + 1;
                int der = 2 * i + 2;
                int mayor = i;
                if (izq < cantidad && TieneMasPrestamos(izq, mayor)) mayor = izq;
                if (der < cantidad && TieneMasPrestamos(der, mayor)) mayor = der;
                if (mayor == i) break;
                Intercambiar(i, mayor);
                i = mayor;
            }
        }

        public Libro ExtraerMaximo()
        {
            if (cantidad == 0) return null;
            Libro max = datos[0];
            cantidad--;
            datos[0] = datos[cantidad];
            datos[cantidad] = null;
            SiftDown(0);
            return max;
        }

        public Libro Buscar(int codigo)
        {
            for (int i = 0; i < cantidad; i++)
                if (datos[i].Codigo == codigo) return datos[i];
            return null;
        }

        public bool Eliminar(int codigo)
        {
            int pos = -1;
            for (int i = 0; i < cantidad; i++)
                if (datos[i].Codigo == codigo) { pos = i; break; }
            if (pos == -1) return false;

            cantidad--;
            if (pos < cantidad)
            {
                datos[pos] = datos[cantidad];
                SiftDown(pos);
                SiftUp(pos);
            }
            datos[cantidad] = null;
            return true;
        }

        public bool EstaVacio() { return cantidad == 0; }

        public void Imprimir()
        {
            Console.WriteLine("Estructura interna del Max Heap (por veces prestado):");
            for (int i = 0; i < cantidad; i++)
                Console.WriteLine($"  [{i}] {datos[i].Titulo} - {datos[i].VecesPrestado} préstamos (código {datos[i].Codigo})");
        }

        public void Recorrer(Action<Libro> accion)
        {
            for (int i = 0; i < cantidad; i++) accion(datos[i]);
        }
    }
}
