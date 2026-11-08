# Arboleda Verdescente — Plano estructural T16

## 1. Estado y decisiones

**Documento de propuesta, pendiente de aprobación del desarrollador.** T16 planifica la región; no construye salas ni modifica `ArboledaPrototype`. Los nombres de áreas y salas son de trabajo. Los identificadores A–G y L1–L8 permiten revisar las conexiones sin depender de esos nombres.

**Estado técnico posterior:** el circuito de muestra B–C–F de T17 está aprobado mediante pruebas automatizadas y manuales, según el contexto de T18. T18 añade una primera escena jugable de D y C ↔ D; su aprobación manual sigue pendiente. La sección 11 distingue esa implementación de la propuesta regional completa.

| Estado | Decisión |
| --- | --- |
| Confirmado por GDD e instrucciones | Región compuesta por áreas interconectadas, con exploración horizontal/vertical, circuitos, alternativas, descubrimientos, atajos, dificultad progresiva y descansos estratégicos. |
| Confirmado previamente | Alcance de Arboleda: 15–18 salas, objetivo inicial 16. Un **área** agrupa varias salas; una sala es un espacio jugable reconocible, no necesariamente un asset `.unity`. |
| Confirmado narrativo | Despertar de la salamandra herida, pasado desconocido, enseñanza opcional del maestro, criaturas huidizas/extrañamente calmadas, marcas de fuerza y cristales lila apagados como pistas del Velo. Guardián Raíz y salida hacia Abismo Lagrimal según GDD. |
| Confirmado funcional | Llamas del Recuerdo como descanso, curación y respawn; identidad blanco-turquesa sobre piedra. Se mantienen las reglas existentes de movimiento y combate. |
| Propuesta T16 | Siete áreas que agrupan las 16 salas anteriores; ocho conexiones base, dos circuitos independientes y posibles atajos adicionales. Distribución ambiental y de encuentros por validar. |
| Pendiente de aprobación | Ubicación/región de los desbloqueos, bloqueos por habilidades, nombres finales, geometría, densidad de encuentros y ubicación/cantidad final de Llamas. |

El plan inicial situaba dash en Raíces Resonantes y doble salto en Copa del Vigía; el GDD también los asignaba a región 1. **T16 deja esas asignaciones pendientes**, conforme a la instrucción actual. Se conservan sus nombres y posibilidades de exploración, pero no se reserva allí una adquisición obligatoria. Las asignaciones históricas no son autorización para implementar desbloqueos.

## 2. Visión y principios espaciales

Arboleda debe sentirse como un mismo bosque visto desde el suelo, sus raíces y sus copas. Un tronco partido reconocible organiza el claro central; las raíces conducen a la gruta y a los cimientos de las ruinas; desde las copas se ven el claro y accesos inferiores que después se pueden recorrer. Se conserva el tono misterioso y melancólico sin explicar el Velo ni inventar historia nueva.

1. **Superponer espacios:** copas sobre el claro, raíces bajo sus caminos y gruta bajo el acceso al refugio. Las áreas comparten huella horizontal; no se colocan siete bloques consecutivos de izquierda a derecha.
2. **Elegir antes de especializarse:** desde el claro se reconoce una subida y una bajada. Las salidas tienen siluetas distintas; los accesos se distribuyen entre sus salas para evitar cuatro puertas iguales en una pantalla.
3. **Volver por otro camino:** cerrar primero circuitos completos. Cada ascenso tiene terrazas de descanso y un retorno reconocible; no añadir ramales largos que solo obliguen a desandar.
4. **Separar ruta base y habilidades futuras:** toda la red base se recorre con dirección y salto normal. Los atajos candidatos pueden quedar inaccesibles en la primera visita sin bloquear el refugio ni el jefe.
5. **Recuperar las caídas:** aterrizar en apoyos o caminos inferiores desde los que se pueda salir con salto normal. Evitar huecos que atrapen al jugador, techos que impidan alcanzar el apoyo siguiente y saltos obligatorios de precisión extrema.
6. **Orientar con lugares:** tronco partido, perfil de ruina, raíces y luz ambiental distinguen los accesos. No se necesita mapa, brújula ni instrucciones textuales obligatorias para seguir el recorrido base.
7. **Alternar ritmo:** exploración, plataformas, descubrimiento, combate y reposo ocupan espacios distintos. Los aterrizajes de una conexión vertical deben permitir observar antes de entrar en combate.

Las franjas siguientes expresan relaciones espaciales, **no coordenadas, alturas de salto ni tamaños de escena**. Cambiar de franja implica una secuencia de apoyos, nunca un único salto gigantesco.

## 3. Siete áreas propuestas

| ID y área | Posición e identidad ambiental | Propósito y progresión | Dificultad aproximada | Conexiones base |
| --- | --- | --- | --- | --- |
| A — Claro del Despertar | Franja media, al oeste del núcleo; vegetación abierta, raíces bajas y luz suave. | Inicio seguro, orientación y presentación de la salamandra herida. Regreso fácil desde el primer cruce. | Baja; movimiento y salto básico sin presión simultánea de combate. | B |
| B — Claro de las Sendas | Núcleo medio, debajo de E; tronco partido visible desde varias alturas, suelo abierto y pasarela rota lateral. | Referencia común, primeras elecciones y retorno de los circuitos. Enseña dónde se puede bajar o subir. Candidato a descanso central. | Baja–media; saltos accesibles y encuentro introductorio separado de las bifurcaciones. | A, C, E, F |
| C — Raíces Profundas | Debajo y algo al oeste de B; contrafuertes de raíces, helechos y terrazas con suelo seguro. | Descubrir la capa inferior y elegir entre ascenso a ruinas o travesía hacia gruta. Introduce cambios de dirección sin habilidades nuevas. | Baja–media; plataformas anchas y recuperación sencilla. | B, D, F |
| D — Ruinas del Colmillo | Franja media-alta occidental, sobre C; piedra cubierta de musgo y fragmentos de columnas. | Descubrimiento ambiental y primera memoria opcional del maestro; paso alternativo hacia las copas. La memoria no es una llave. | Media; exploración pausada y ascenso moderado. | C, E |
| E — Copas del Vigía | Franja alta, sobre B y parte de F; ramas escalonadas, claros entre hojas y mirador. | Perspectiva de lugares conocidos, plataformas opcionales más exigentes y retorno al claro. Se puede llegar desde B o D. | Media–alta opcional; salto normal con margen, descansos y caída recuperable hacia B/C. | D, B |
| F — Gruta del Retorno | Debajo del lado oriental de B y del acceso a G; piedra húmeda, raíces largas y luz tenue, sin objetos turquesas decorativos. | Travesía inferior propia, ascenso de retorno al claro y acceso al refugio. Reconecta exploración con preparación del combate final. | Media; alterna navegación tranquila y encuentro de preparación en suelo estable. | C, B, G |
| G — Umbral de Savia | Franja media oriental, cerca de F; refugio despejado, transición a arena y salida regional. | Descanso antes del Guardián Raíz, encuentro previsto por GDD y cierre hacia Abismo Lagrimal. Mantener legible el regreso al refugio. | Acceso bajo–medio; combate del jefe según GDD, sin redefinir sus reglas. | F; salida tras el Guardián |

### Reagrupación de las 16 salas anteriores

Se preservan los nombres y funciones compatibles del plan inicial. Cambian la topología y las asignaciones de desbloqueos. Esta distribución es una hipótesis de trabajo que suma 16 salas, no contenido ya construido.

| Área | Salas del plan anterior | Función conservada o ajuste |
| --- | --- | --- |
| A | 1 Claro del Despertar | Inicio y presentación ambiental. |
| B | 2 Sendero de Esporas; 3 Pasarela Quebrada; 7 Tronco Partido | Orientación, referencia central y vista de una conexión futura. La pasarela rota no bloquea el acceso normal a las copas. |
| C | 4 Hondonada de Helechos; 6 Raíces Resonantes | Exploración segura inferior y ascenso; se retira la adquisición obligatoria de dash. |
| D | 5 Santuario del Colmillo; 8 Galería de Musgo | Memoria opcional y pistas ambientales ya previstas; la galería se conecta al recorrido de ruinas, sin exigir un secreto obligatorio. |
| E | 9 Nido Suspendido; 10 Cámara del Polen Quieto; 11 Copa del Vigía | Plataformas, calma extraña y mirador; se retira la adquisición obligatoria de doble salto. |
| F | 12 Atajo de las Raíces; 13 Umbral del Guardián | Retorno al centro y preparación del tramo final. El escondite por habilidad se conserva solo como posibilidad pendiente. |
| G | 14 Refugio de Savia; 15 Cámara del Guardián Raíz; 16 Salida del Velo | Descanso, jefe y cierre regional. |

No se decide aquí cuántas escenas Unity alojarán estas salas ni cómo se cargarán. Las conexiones entre salas de una misma área se detallarán al construir cada bloque, respetando los puertos del plano regional.

## 4. Diagrama de conexiones base

Todos los enlaces de doble flecha son **bidireccionales con salto normal**, sin necesidad de adquirir una habilidad o activar un mecanismo. La flecha hacia Abismo representa el cierre posterior al Guardián del plan anterior; su implementación se decidirá en la tarea correspondiente. Los atajos por habilidades de la sección 7 no forman parte de este diagrama base.

```mermaid
flowchart TB
    subgraph Alta["Franja alta: ruinas y copas"]
        D["D · Ruinas del Colmillo"] <-->|"L4 · cornisas"| E["E · Copas del Vigía"]
    end
    subgraph Media["Franja media: entrada, claro y refugio"]
        A["A · Claro del Despertar"] <-->|"L1 · sendero"| B["B · Claro de las Sendas"]
        G["G · Umbral de Savia"]
    end
    subgraph Baja["Franja baja: raíces y gruta"]
        C["C · Raíces Profundas"] <-->|"L6 · galería inferior"| F["F · Gruta del Retorno"]
    end
    B <-->|"L2 · terrazas de raíces"| C
    C <-->|"L3 · ascenso a ruinas"| D
    E <-->|"L5 · tronco escalonado"| B
    F <-->|"L7 · retorno al claro"| B
    F <-->|"L8 · ascenso al refugio"| G
    G -->|"Tras el Guardián"| Salida["Abismo Lagrimal"]
```

Es un plano de relaciones, no una planta a escala. La tabla espacial y los puertos siguientes gobiernan la futura colocación; el renderizador de Mermaid puede distribuir los nodos de otra forma.

| Enlace | Puertos y dirección espacial | Qué permite reconocer |
| --- | --- | --- |
| L1 A ↔ B | Sendero bajo hacia la base del tronco; regreso sin caída irreversible. | El claro permanece visible antes de entrar y al regresar al inicio. |
| L2 B ↔ C | Abertura occidental del claro, con terrazas que bajan entre raíces. | Desde B se ve el primer aterrizaje; desde C se reconocen la raíz y la luz del claro. |
| L3 C ↔ D | Ascenso por cimientos y terrazas, con pequeños cambios de dirección. | Perfil de las columnas antes de alcanzar la ruina; raíces conocidas vistas desde sus cimientos. |
| L4 D ↔ E | Cornisas laterales que suben gradualmente hacia las ramas. | Mirador visible desde la ruina y columnas reconocibles desde las copas. |
| L5 E ↔ B | Zigzag normal por el tronco, separado de la futura pasarela directa. | Copa y base del mismo tronco como referencia común, con apoyos visibles al bajar. |
| L6 C ↔ F | Travesía inferior con desniveles y descansos, bajo el claro. | Cambio gradual de raíces a piedra húmeda; se anticipa el ascenso oriental. |
| L7 F ↔ B | Ascenso por la cara oriental del tronco hacia otra sala del claro. | Reaparición junto a un lugar conocido desde una altura distinta; conexión alternativa al camino por C. |
| L8 F ↔ G | Ascenso ancho y descansado hasta el refugio, antes de la arena. | Llama reconocible y suelo protegido antes de entrar al combate; retorno a F accesible desde el refugio. |

## 5. Progresión inicial y circuitos

La red completa A–G debe poder navegarse sin dash, doble salto, pogo o parry. Esto se refiere a **acceso y plataformas**; no sustituye las reglas de combate del jefe por un requisito de salto normal.

- **Primer recorrido sugerido, sin imponer orden:** A → B → C → F → G. Presenta el claro desde arriba, explora sus raíces y vuelve a subir cerca del refugio. El jugador puede investigar D/E antes o después.
- **Alternativa superior:** A → B → E → D → C → F → G. El ascenso por L5 ofrece mayor exigencia de plataformas; la ruta por C permite evitarlo. Visitar D no obliga a activar ni obtener la memoria.
- **Regreso corto por conocimiento:** A → B → F → G también es válido si se descubre L7 desde B. No se introduce una cerradura para impedirlo: la libertad de ruta es deliberada. Si en prueba humana vacía de interés el resto de la región, revisar disposición y ritmo antes de proponer un bloqueo nuevo.
- **Circuito superior, cuatro áreas:** B → C → D → E → B. Une descenso, ascenso por ruinas, mirador y regreso por el tronco; también se recorre al revés.
- **Circuito inferior, tres áreas:** B → C → F → B. Permite bajar por un acceso, avanzar bajo el claro y volver por el otro. L7 evita repetir la exploración de C cuando se vuelve desde F.

Los dos circuitos comparten B–C, pero sus retornos son diferentes: D–E–B y F–B. Son dos ciclos independientes de una red de siete áreas y ocho enlaces base. A es una entrada segura y G un cierre; no hace falta convertir inicio o arena en otro bucle.

### Atajos, observación y regreso

- **L7 es un atajo de recorrido siempre abierto**, no un desbloqueo. Encontrarlo cambia el conocimiento del espacio, no el estado de una puerta. Sus dos entradas deben ser visibles al aproximarse; no esconder deliberadamente la salida de la gruta.
- Desde B se observa parte de E antes de llegar; L5 da un camino normal más largo, mientras una separación lateral puede anticipar una futura conexión directa. No insinuar que un salto imposible forma parte del camino base.
- Desde F se ven raíces y una abertura superior hacia E; el camino normal de regreso pasa por B. En D, una ventana hacia F puede anticipar un paso lateral futuro sin exigir recordarlo para continuar.
- Desde el refugio se reconoce el retorno a F y luego al claro. Al regresar, el tronco partido permite orientarse sin memorizar una lista de puertas.

## 6. Capacidades y márgenes para el futuro graybox

Inspección del prototipo T15: `PlayerMovementData` usa velocidad 7.5, salto 13, gravedad 3, coyote/buffer 0.12; las travesías anteriores midieron aproximadamente 2.74 de altura normal. Dash tiene distancia 3.2, velocidad 18 y duración 0.18; doble salto usa velocidad 12. Pogo y combate existen, pero no se usan como llaves de navegación de esta propuesta.

`PlayerAbilities` dispone de banderas de dash/doble salto. Que esas funciones existan o estén habilitadas para pruebas **no establece un lugar de adquisición en la historia**. La ubicación de todos los desbloqueos que se propongan para la región requiere aprobación antes de diseñar dependencias.

El graybox debe validar subidas con margen respecto al máximo normal, apoyos amplios, techos despejados y salida desde los aterrizajes inferiores. No usar 2.74 como altura obligatoria de un escalón ni 3.2 como ancho exacto de un hueco de dash: trayectoria, collider y espacio de despegue necesitan prueba real. No se cambian parámetros para hacer encajar el plano.

## 7. Posibles conexiones por habilidades — pendientes de aprobación

Estas propuestas no fijan dónde ni en qué región se obtiene una habilidad. Pueden servir para una visita posterior si el desarrollador decide situar su adquisición fuera de Arboleda. **Quitarlas todas deja intacta la progresión base y sus dos circuitos.**

| Candidato | Idea y lectura previa | Recorrido normal alternativo | Decisión pendiente |
| --- | --- | --- | --- |
| P1 D ↔ F — posible dash | Paso horizontal entre un borde bajo de ruina y una terraza alta de gruta, a una altura común. Se ve el otro lado; una caída termina en el recorrido inferior seguro. | D → C → F. | Aprobar vínculo y habilidad antes de medir el hueco; la posición final debe permitir dash horizontal, no exigir un impulso vertical inexistente. |
| P2 F ↔ E — posible doble salto | Pozo vertical con destino visible hacia las copas; apoyos inferiores permiten salir aunque no se alcance la rama alta. | F → B → E. | Aprobar si merece un atajo directo y si requiere doble salto; no reservar aquí su desbloqueo. |
| P3 B ↔ E — posible pasarela directa | Reutilizar el concepto de Pasarela Quebrada para una conexión más rápida entre alturas compatibles, junto a la ruta normal L5 por el tronco. | L5 B ↔ E, siempre disponible con salto normal. | Aprobar su utilidad frente a P1/P2 y la habilidad apropiada; puede descartarse para reducir redundancia. |

No se añaden interruptores, puertas, elevadores, cuerdas, nuevas habilidades ni sistemas de desbloqueo en T16. Si en una tarea posterior se desea un atajo que se abra desde un solo lado, deberá especificarse y aprobarse su mecanismo; no se asume como parte de L1–L8.

## 8. Ritmo, encuentros y Llamas del Recuerdo

| Tramo | Ritmo y encuentro conceptual | Descanso y retorno |
| --- | --- | --- |
| A → B | Inicio tranquilo; espacio para observar antes de un encuentro introductorio en suelo estable. | Candidato de Llama central en una zona protegida de B, desde la que se entiendan ambas capas del bosque. |
| B → C → D | Descenso amable, exploración de raíces, pausa de descubrimiento y memoria opcional. | Descansos geométricos; regreso a B por L2 o por D/E. No convertir cada terraza en checkpoint. |
| D → E → B | Plataformas opcionales con vista del entorno; no colocar un enemigo obligatorio sobre un aterrizaje estrecho. | Recuperaciones hacia apoyos inferiores; retorno reconocible al claro. |
| C → F | Exploración horizontal y vertical; encuentro de preparación solo después de un descanso, fuera de las bocas de ascenso. | L7 proporciona regreso al claro por otra ruta. Una Llama inferior adicional solo se estudiará si las pruebas muestran una repetición excesiva tras morir. |
| F → G | Disminuir presión antes del refugio; después, arena del Guardián según GDD y cierre regional. | Llama antes del jefe, visible y protegida, para repetir la pelea sin rehacer plataformas o encuentros previos. |

Se proponen **dos funciones de descanso** —central y previa al jefe—, no una cifra final de Llamas. La primera sostiene exploración y retornos; la segunda preserva el reinicio rápido de combate aprobado. No se fijan cantidades de enemigos ni nuevas especies: se ubican funciones de encuentro que luego se probarán con contratos existentes y presupuesto de cada bloque.

Si se muere explorando con la Llama de B activa, se puede elegir L2, L5 o L7 para volver, sin perder conexiones ni repetir adquisiciones. Si se muere ante el Guardián con la Llama de G activa, el retorno debe ser local, despejado y sin otro combate obligatorio. Un descanso no exige visitar una memoria ni justifica nuevo lore de resurrección.

Identidad ambiental según GDD: fauna huidiza o extrañamente calmada, piedra intervenida por vegetación y vetas/cristales lila apagados como pistas discretas. No se añaden explicaciones, nombres de civilizaciones o rituales. Blanco-turquesa y pedestal reconocible se reservan para las Llamas; las propuestas ambientales futuras no autorizan restaurar `Faint Crystal` ni `Crystal Glow` en el prototipo.

## 9. Relación con ArboledaPrototype y orden de implementación

`ArboledaPrototype` sigue siendo un laboratorio técnico de 150 unidades, con rutas T14, circuito T15, una Llama, un enemigo y arco final. Su captura general y geometría muestran que las opciones siguen dentro de un eje horizontal común. El plano regional **no traslada esa tira a siete bloques contiguos ni convierte sus coordenadas en ubicaciones definitivas**.

Se reutilizarán aprendizajes —márgenes del salto, descenso recuperable, pasos bajo plataformas, reconocimiento del retorno y cámara— y los sistemas existentes. La ruina y el mirador aportan referencias de composición; no se copian automáticamente las dimensiones. En T16 no cambian escena, prefabs, scripts, herramientas, estadísticas o parámetros de cámara. No se crean escenas jugables.

Orden recomendado para futuras tareas, después de aprobar el plano:

1. **Tarea siguiente pequeña:** un graybox de tres salas de muestra, una de B, una de C y una de F, que cierre B ↔ C ↔ F ↔ B. No son tres áreas completas. Usar personaje y cámara existentes, movimiento/salto normal, suelo seguro y referencias visuales provisionales. En esa tarea se autorizará una escena de prueba independiente; T16 no la crea ni altera el prototipo técnico.
2. Probar ese circuito en ambos sentidos, caídas y retorno con física real. Evaluar con una persona si reconoce el claro desde abajo, descubre las dos salidas y entiende que volvió por otro acceso. Detener la expansión si la estructura no se entiende.
3. Completar B/C/F dentro del presupuesto de salas y añadir A. Revalidar regreso temprano y distribución de accesos antes de decorar o colocar encuentros.
4. Construir D/E para cerrar el circuito superior, manteniendo el inferior como alternativa accesible. Comprobar vistas entre ruina, copas y claro, y que fallar un salto no atrape al jugador.
5. Situar descansos y encuentros de muestra, comprobar curación, muerte/respawn y tiempo de repetición; ajustar ubicación antes de densidad o estadísticas. No convertir el número de muestras en conteo final.
6. Construir el acceso/refugio G y reservar el espacio del jefe. La implementación del Guardián y de la transición regional tendrá tareas propias; este plano no las adelanta.
7. Solo tras aprobar progresión y ubicación de habilidades, probar los atajos elegidos P1–P3. Descartar los redundantes y validar sus fallos seguros antes del arte final.

No se asignan fechas nuevas al cronograma antiguo. Número de escenas, carga/transiciones, dimensiones, cámaras por espacio y estimación de producción se resolverán después de validar el primer circuito; no se presupone un sistema nuevo para ellos.

## 10. Revisión del plano y decisiones pendientes

La revisión documental comprueba que hay siete áreas, ocho enlaces base y dos ciclos independientes; todas las áreas son alcanzables desde A sin P1–P3, y D/E no bloquean la llegada a G. Mantener los 16 nombres anteriores permite rastrear qué se conserva y qué cambia. Esto no prueba la transitabilidad de una geometría que todavía no existe.

Antes de construir la región completa, el desarrollador debe aprobar o ajustar:

- Agrupación en siete áreas y dos circuitos, incluyendo la ruta corta B–F y el papel opcional de ruinas/copas.
- Nombres de trabajo, reparto de las 16 salas y tamaño relativo de cada bloque.
- Ubicación/región de adquisición de habilidades y cuáles de P1–P3 aportan suficiente exploración para implementarse.
- Emplazamiento de descansos central/prejefe y necesidad real de otro inferior, según pruebas de retorno tras morir.
- Ritmo y densidad de encuentros, duración de exploración y alcance de producción, sin fijarlos a partir del prototipo lineal ni de tiempos de un bot.

Para la primera prueba humana: llegar al claro, elegir una bajada, reconocer el punto de regreso por otro acceso, repetir en sentido contrario y señalar qué lugares altos despiertan curiosidad. Revisar orientación y deseo de explorar por separado de la precisión de salto y del combate. Los sistemas técnicos T01–T15 permanecen fuera de los cambios de esta tarea; la medición manual exacta de los 0.8 s de i-frames sigue pendiente.

## 11. Estado técnico T18 — primera sala de Ruinas del Colmillo

`Assets/_Project/Scenes/Arboleda/TechnicalCircuit/Arboleda_D_RuinasColmillo.unity` es una muestra técnica compacta de D, pendiente de pruebas manuales. No representa todavía las dos salas conceptuales asignadas a D en el reparto de 16 salas, ni implementa la memoria opcional. Comparte `RoomPlayerRig`, contratos de transición y sesión de T17; no contiene enemigos, Llamas, adquisiciones ni coleccionables.

**Conectividad real:** B ↔ C ↔ F ↔ B permanece; se añade C ↔ D, correspondiente al ascenso L3. En C se conservan todas las plataformas y puertos anteriores y se incorpora una rama desde `C Terrace 6` hacia dos apoyos orientales a altura 6, en `x=15..18` y `x=20..24`. Las coordenadas son locales a cada escena, no un mapa continuo regional.

| Salida nueva | Destino / entrada | Centro del jugador al aparecer | Orientación |
| --- | --- | --- | --- |
| C-to-D: `(22.8, 7)`, suelo a 6 | D / from-C | `(4, 0.58)` | Derecha |
| D-to-C: `(1.2, 1)`, suelo a 0 | C / from-D | `(20.5, 6.58)` | Izquierda |

D ocupa 22 unidades de anchura y tiene suelo seguro continuo a 0. Su inicio directo coincide con la llegada desde C. El ascenso principal alterna derecha/izquierda entre apoyos a **1.8, 3.6, 5.4, 7.2, 9 y 10.8**; todos los saltos ascendentes principales suben 1.8, por debajo del salto normal medido de 2.74. El mirador occidental a 7.2 sale del cruce a 5.4 y reconecta con el apoyo interior a 9. El dintel de entrada a 2.8 recibe la caída lateral del mirador y permite regresar al apoyo de 3.6 sin caer sobre el trigger de salida. Otras caídas terminan en plataformas inferiores o en el suelo, desde donde se puede repetir el ascenso. El regreso previsto baja en zigzag y sale por el extremo inferior izquierdo.

La cámara superior tiene columnas deterioradas, dintel roto, musgo, raíces y un hueco visual oriental. Ese hueco prepara la lectura de la futura conexión L4, pero **no hay salida, entrada ni escena E**. El circuito superior B–C–D–E–B permanece incompleto hasta tareas posteriores. Tampoco se implementan P1–P3 ni adquisición de habilidades.

CameraFollow2D mantiene su comportamiento y tamaño ortográfico 5.5; solo D amplía sus límites locales a `x=0..22, y=-4..21`. Dash/doble salto empiezan bloqueados según el prefab aprobado y no son necesarios para el recorrido, mirador o retorno. La única Llama del circuito sigue en B; la sesión conserva ese checkpoint, vida y desbloqueos al viajar a D. El detalle de validaciones y las comprobaciones humanas pendientes se registra en `DevelopmentLog.md` bajo T18.
