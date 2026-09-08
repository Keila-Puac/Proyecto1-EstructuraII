using System;

namespace BibliotecaApp
{
    // =====================================================================
    //  ÁRBOL B+  (estructura principal del catálogo, indexada por Código)
    // =====================================================================
    internal class NodoB
    {
        // Orden = 4
        // Claves = 3.
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

        // Divide una HOJA llena en dos.
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

        // Divide un nodo INTERNO lleno en dos.
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

        // ----------------
        // BUSCAR
        // ----------------
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
        // ELIMINAR
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
        // RECORRER
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
        // MOSTRAR
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
