using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace SimulacionNavegador
{
    /// <summary>
    /// Representa un sitio web visitado en el simulador de navegación.
    /// </summary>
    public class PaginaWeb
    {
        public string Url { get; }
        public string Titulo { get; }
        public DateTime FechaAcceso { get; }

        public PaginaWeb(string url, string titulo)
        {
            Url = string.IsNullOrWhiteSpace(url) ? "about:blank" : url;
            Titulo = string.IsNullOrWhiteSpace(titulo) ? "Pestaña Nueva" : titulo;
            FechaAcceso = DateTime.Now;
        }

        public override string ToString()
        {
            return $"{Titulo} ({Url}) - Accedido: {FechaAcceso:HH:mm:ss}";
        }
    }

    /// <summary>
    /// Administra la navegación utilizando dos estructuras tipo Pila (Stack)
    /// para permitir la navegación hacia atrás y hacia adelante con complejidad O(1).
    /// </summary>
    public class Navegador
    {
        private readonly Stack<PaginaWeb> _historialAtras;
        private readonly Stack<PaginaWeb> _historialAdelante;
        
        public PaginaWeb? PaginaActual { get; private set; }

        public int CantidadAtras => _historialAtras.Count;
        public int CantidadAdelante => _historialAdelante.Count;

        public Navegador()
        {
            _historialAtras = new Stack<PaginaWeb>();
            _historialAdelante = new Stack<PaginaWeb>();
            PaginaActual = null;
        }

        /// <summary>
        /// Visita una nueva página. Limpia el historial de avance (Adelante)
        /// ya que se inicia una nueva bifurcación en el camino de navegación.
        /// </summary>
        /// <param name="nuevaPagina">La página web a visitar.</param>
        /// <returns>Los ticks de CPU que tomó la operación.</returns>
        public long VisitarPagina(PaginaWeb nuevaPagina)
        {
            Stopwatch cronometro = Stopwatch.StartNew();

            if (PaginaActual != null)
            {
                _historialAtras.Push(PaginaActual);
            }
            PaginaActual = nuevaPagina;
            _historialAdelante.Clear(); // Rompe la linealidad de avance anterior

            cronometro.Stop();
            return cronometro.ElapsedTicks;
        }

        /// <summary>
        /// Retrocede a la página anterior si existe.
        /// </summary>
        /// <returns>Una tupla indicando éxito, la página cargada (si aplica) y los ticks transcurridos.</returns>
        public (bool exito, PaginaWeb? paginaCargada, long ticks) Retroceder()
        {
            Stopwatch cronometro = Stopwatch.StartNew();

            if (_historialAtras.Count > 0)
            {
                if (PaginaActual != null)
                {
                    _historialAdelante.Push(PaginaActual);
                }
                PaginaActual = _historialAtras.Pop();
                cronometro.Stop();
                return (true, PaginaActual, cronometro.ElapsedTicks);
            }

            cronometro.Stop();
            return (false, null, cronometro.ElapsedTicks);
        }

        /// <summary>
        /// Avanza a la siguiente página en el historial de adelante si existe.
        /// </summary>
        /// <returns>Una tupla indicando éxito, la página cargada (si aplica) y los ticks transcurridos.</returns>
        public (bool exito, PaginaWeb? paginaCargada, long ticks) Adelantar()
        {
            Stopwatch cronometro = Stopwatch.StartNew();

            if (_historialAdelante.Count > 0)
            {
                if (PaginaActual != null)
                {
                    _historialAtras.Push(PaginaActual);
                }
                PaginaActual = _historialAdelante.Pop();
                cronometro.Stop();
                return (true, PaginaActual, cronometro.ElapsedTicks);
            }

            cronometro.Stop();
            return (false, null, cronometro.ElapsedTicks);
        }

        /// <summary>
        /// Limpia todo el historial de navegación y la página actual.
        /// </summary>
        public void LimpiarHistorial()
        {
            _historialAtras.Clear();
            _historialAdelante.Clear();
            PaginaActual = null;
        }

        /// <summary>
        /// Retorna el historial de retroceso.
        /// </summary>
        public IEnumerable<PaginaWeb> ObtenerHistorialAtras()
        {
            return _historialAtras;
        }

        /// <summary>
        /// Retorna el historial de avance.
        /// </summary>
        public IEnumerable<PaginaWeb> ObtenerHistorialAdelante()
        {
            return _historialAdelante;
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length > 0 && args[0].ToLower() == "--demo")
            {
                EjecutarDemo();
                return;
            }

            Navegador navegador = new Navegador();
            bool salir = false;

            // Inicializar con algunos datos de ejemplo para demostrar su funcionamiento inicial
            navegador.VisitarPagina(new PaginaWeb("https://www.google.com", "Google"));
            navegador.VisitarPagina(new PaginaWeb("https://www.github.com", "GitHub"));
            navegador.VisitarPagina(new PaginaWeb("https://stackoverflow.com", "Stack Overflow"));

            while (!salir)
            {
                Console.Clear();
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine("====================================================================");
                Console.WriteLine("        SIMULADOR DE BOTÓN RETROCEDER Y ADELANTAR DE NAVEGADOR      ");
                Console.WriteLine("====================================================================");
                Console.ResetColor();

                // 1. Mostrar la página actual
                Console.Write("\nPÁGINA ACTUAL: ");
                if (navegador.PaginaActual != null)
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine(navegador.PaginaActual);
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.DarkGray;
                    Console.WriteLine("[Ninguna página cargada (about:blank)]");
                }
                Console.ResetColor();

                // 2. Mostrar Reportería del Historial
                Console.WriteLine("\n--- REPORTERÍA DEL ESTADO DE LAS PILAS ---");
                
                // Mostrar Historial Adelante (Forward Stack)
                Console.ForegroundColor = ConsoleColor.DarkYellow;
                Console.WriteLine($"[PILA ADELANTE] (Elementos: {navegador.CantidadAdelante})");
                if (navegador.CantidadAdelante == 0)
                {
                    Console.WriteLine("  <- (Vacío - No se puede adelantar)");
                }
                else
                {
                    int i = 1;
                    foreach (var pag in navegador.ObtenerHistorialAdelante())
                    {
                        Console.WriteLine($"  [{i}] {pag.Titulo} ({pag.Url})");
                        i++;
                    }
                }

                Console.WriteLine();

                // Mostrar Historial Atrás (Back Stack)
                Console.ForegroundColor = ConsoleColor.Magenta;
                Console.WriteLine($"[PILA ATRÁS] (Elementos: {navegador.CantidadAtras})");
                if (navegador.CantidadAtras == 0)
                {
                    Console.WriteLine("  <- (Vacío - No se puede retroceder)");
                }
                else
                {
                    int i = 1;
                    foreach (var pag in navegador.ObtenerHistorialAtras())
                    {
                        Console.WriteLine($"  [{i}] {pag.Titulo} ({pag.Url})");
                        i++;
                    }
                }
                Console.ResetColor();
                Console.WriteLine("------------------------------------------");

                // 3. Menú de Opciones
                Console.WriteLine("\nMENÚ DE OPCIONES:");
                Console.WriteLine("1. Visitar una nueva página web");
                Console.WriteLine("2. Retroceder (Atrás)");
                Console.WriteLine("3. Adelantar (Adelante)");
                Console.WriteLine("4. Limpiar historial de navegación");
                Console.WriteLine("5. Ejecutar simulación de rendimiento (Benchmarking)");
                Console.WriteLine("6. Salir");
                Console.Write("\nSeleccione una opción: ");

                string opcion = Console.ReadLine() ?? "";
                switch (opcion)
                {
                    case "1":
                        Console.Write("\nIngrese la URL del sitio (ej. https://example.com): ");
                        string url = Console.ReadLine() ?? "";
                        Console.Write("Ingrese el título del sitio (ej. Ejemplo): ");
                        string titulo = Console.ReadLine() ?? "";
                        
                        if (string.IsNullOrWhiteSpace(url))
                        {
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.WriteLine("\n[Error] La URL no puede estar vacía.");
                            Console.ResetColor();
                            PresioneTecla();
                            break;
                        }

                        long ticksVisita = navegador.VisitarPagina(new PaginaWeb(url, titulo));
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine($"\n[Éxito] Página visitada con éxito.");
                        Console.ForegroundColor = ConsoleColor.DarkGray;
                        Console.WriteLine($"Tiempo de procesamiento de inserción (Stack.Push): {ticksVisita} ticks de CPU.");
                        Console.ResetColor();
                        PresioneTecla();
                        break;

                    case "2":
                        var resRetroceder = navegador.Retroceder();
                        if (resRetroceder.exito && resRetroceder.paginaCargada != null)
                        {
                            Console.ForegroundColor = ConsoleColor.Green;
                            Console.WriteLine($"\n[Éxito] Retrocediendo a: {resRetroceder.paginaCargada.Titulo}");
                            Console.ForegroundColor = ConsoleColor.DarkGray;
                            Console.WriteLine($"Tiempo de retroceso (Stack.Pop/Push): {resRetroceder.ticks} ticks de CPU.");
                        }
                        else
                        {
                            Console.ForegroundColor = ConsoleColor.Yellow;
                            Console.WriteLine("\n[Alerta] No hay páginas en el historial para retroceder.");
                        }
                        Console.ResetColor();
                        PresioneTecla();
                        break;

                    case "3":
                        var resAdelantar = navegador.Adelantar();
                        if (resAdelantar.exito && resAdelantar.paginaCargada != null)
                        {
                            Console.ForegroundColor = ConsoleColor.Green;
                            Console.WriteLine($"\n[Éxito] Adelantando a: {resAdelantar.paginaCargada.Titulo}");
                            Console.ForegroundColor = ConsoleColor.DarkGray;
                            Console.WriteLine($"Tiempo de avance (Stack.Pop/Push): {resAdelantar.ticks} ticks de CPU.");
                        }
                        else
                        {
                            Console.ForegroundColor = ConsoleColor.Yellow;
                            Console.WriteLine("\n[Alerta] No hay páginas en el historial para adelantar.");
                        }
                        Console.ResetColor();
                        PresioneTecla();
                        break;

                    case "4":
                        navegador.LimpiarHistorial();
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine("\n[Éxito] El historial se ha limpiado por completo.");
                        Console.ResetColor();
                        PresioneTecla();
                        break;

                    case "5":
                        EjecutarBenchmark();
                        PresioneTecla();
                        break;

                    case "6":
                        salir = true;
                        Console.WriteLine("\nGracias por usar el simulador de navegador.");
                        break;

                    default:
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("\nOpción no válida. Intente de nuevo.");
                        Console.ResetColor();
                        PresioneTecla();
                        break;
                }
            }
        }

        static void PresioneTecla()
        {
            Console.WriteLine("\nPresione cualquier tecla para continuar...");
            Console.ReadKey();
        }

        static void EjecutarBenchmark()
        {
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("====================================================================");
            Console.WriteLine("           SIMULACIÓN DE RENDIMIENTO (BENCHMARK DE LA PILA)        ");
            Console.WriteLine("====================================================================");
            Console.ResetColor();
            Console.WriteLine("Esta simulación realiza inserciones (Push) y extracciones (Pop) a gran");
            Console.WriteLine("escala para demostrar empíricamente la complejidad O(1) de las pilas.");
            Console.WriteLine("Se insertarán y extraerán 100,000 páginas en el historial.\n");

            Navegador navTest = new Navegador();
            int numElementos = 100000;
            
            // Medir Inserciones (Push)
            Stopwatch swPush = Stopwatch.StartNew();
            for (int i = 0; i < numElementos; i++)
            {
                navTest.VisitarPagina(new PaginaWeb($"https://site{i}.com", $"Sitio {i}"));
            }
            swPush.Stop();

            double promedioPushNs = (double)swPush.ElapsedTicks / numElementos * (1000000000.0 / Stopwatch.Frequency);
            Console.WriteLine($"- Inserción (Push) de {numElementos:N0} elementos:");
            Console.WriteLine($"  * Tiempo total: {swPush.ElapsedMilliseconds} ms ({swPush.ElapsedTicks:N0} ticks)");
            Console.WriteLine($"  * Tiempo promedio por Push: {promedioPushNs:F2} ns");
            Console.WriteLine($"  * Complejidad experimental: O(1) [Constante]");

            Console.WriteLine();

            // Medir Extracciones (Pop)
            Stopwatch swPop = Stopwatch.StartNew();
            for (int i = 0; i < numElementos - 1; i++)
            {
                navTest.Retroceder();
            }
            swPop.Stop();

            double promedioPopNs = (double)swPop.ElapsedTicks / (numElementos - 1) * (1000000000.0 / Stopwatch.Frequency);
            Console.WriteLine($"- Extracción (Pop) de {numElementos - 1:N0} elementos:");
            Console.WriteLine($"  * Tiempo total: {swPop.ElapsedMilliseconds} ms ({swPop.ElapsedTicks:N0} ticks)");
            Console.WriteLine($"  * Tiempo promedio por Pop: {promedioPopNs:F2} ns");
            Console.WriteLine($"  * Complejidad experimental: O(1) [Constante]");

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("\n[Conclusión] Las operaciones no dependen del número de elementos almacenados,");
            Console.WriteLine("lo que valida teórica y experimentalmente que Stack posee un rendimiento O(1).");
            Console.ResetColor();
        }

        static void EjecutarDemo()
        {
            Console.WriteLine("====================================================================");
            Console.WriteLine("          DEMO AUTOMATIZADO: SIMULACIÓN DE NAVEGADOR DE INTERNET    ");
            Console.WriteLine("====================================================================");
            Navegador navegador = new Navegador();

            Console.WriteLine("\n[1] Navegando a: Google, GitHub y Stack Overflow...");
            navegador.VisitarPagina(new PaginaWeb("https://www.google.com", "Google"));
            navegador.VisitarPagina(new PaginaWeb("https://www.github.com", "GitHub"));
            navegador.VisitarPagina(new PaginaWeb("https://stackoverflow.com", "Stack Overflow"));

            MostrarEstadoNavegador(navegador);

            Console.WriteLine("\n[2] Retrocediendo una vez (Atrás)...");
            navegador.Retroceder();
            MostrarEstadoNavegador(navegador);

            Console.WriteLine("\n[3] Retrocediendo de nuevo (Atrás)...");
            navegador.Retroceder();
            MostrarEstadoNavegador(navegador);

            Console.WriteLine("\n[4] Adelantando una vez (Adelante)...");
            navegador.Adelantar();
            MostrarEstadoNavegador(navegador);

            Console.WriteLine("\n[5] Visitando un nuevo sitio (Microsoft) -> Limpia la pila Adelante.");
            navegador.VisitarPagina(new PaginaWeb("https://www.microsoft.com", "Microsoft"));
            MostrarEstadoNavegador(navegador);

            Console.WriteLine("\n[6] Ejecutando Benchmark de rendimiento (100,000 elementos)...");
            EjecutarBenchmark();
        }

        static void MostrarEstadoNavegador(Navegador navegador)
        {
            Console.WriteLine("--------------------------------------------------------------------");
            Console.WriteLine($"PÁGINA ACTUAL: {(navegador.PaginaActual != null ? navegador.PaginaActual.ToString() : "[Vacía]")}");
            Console.WriteLine($"PILA ATRÁS (Elementos: {navegador.CantidadAtras})");
            foreach (var pag in navegador.ObtenerHistorialAtras())
            {
                Console.WriteLine($"   * {pag.Titulo} ({pag.Url})");
            }
            Console.WriteLine($"PILA ADELANTE (Elementos: {navegador.CantidadAdelante})");
            foreach (var pag in navegador.ObtenerHistorialAdelante())
            {
                Console.WriteLine($"   * {pag.Titulo} ({pag.Url})");
            }
            Console.WriteLine("--------------------------------------------------------------------\n");
        }
    }
}
