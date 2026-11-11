# Arboleda Verdescente — Hoja de ruta de desarrollo regional T23

**Estado actualizado en T25:** el usuario confirma **T01–T24 aprobadas**, incluida la duración adecuada de B2 para su función. Corte: 10 de noviembre de 2026, rama `main`, base `72e7961` (T24). T25 incorpora B3/P03 para revisión manual; no se declara aprobada ni completada toda la planta regional.

Esta es la hoja de ruta regional oficial aprobada en T23, con su seguimiento posterior. **T23 fue documental; T24 inicia la siguiente implementación de Fase 1** con un encargo propio. La numeración posterior sigue siendo provisional y no se prometen fechas de entrega.

## 1. Qué construimos y dónde consultar cada dato

Arboleda Verdescente será una región explorable, con orientación espacial, decisiones de ruta, aprendizaje de combate, descubrimientos y cierre hacia Abismo Lagrimal. Las seis escenas actuales son una base técnica. Se pueden ampliar, dividir, fusionar o adaptar si el diseño y las pruebas lo justifican; una puerta programada no obliga a conservar una conexión artificial.

| Fuente | Responsabilidad |
| --- | --- |
| [GDD](GDD.md), especialmente 5, 7 y 8.1 | Visión, historia, reglas de combate, Guardián Raíz y principios de exploración. No se reescribe aquí. |
| [Region1Plan](Region1Plan.md) | Funciones de las siete áreas y antecedentes T16–T22; sus propuestas antiguas no son inventario actual. |
| [Region1SpatialMap](Region1SpatialMap.md), secciones 4–6, 17–19; [SVG](Region1SpatialMap.svg) | IDs de las 16 salas, planta provisional, anclas/puertos y diferencias implementadas en T22/T24/T25. No se copian las tablas de coordenadas. |
| Este documento | Fases, dependencias, tareas, estado por sala, decisiones abiertas y criterios de cierre. |
| [DevelopmentLog](DevelopmentLog.md), T16–T25 | Evidencias de entregas y pruebas, limitaciones y contexto de cada aprobación. |

El objetivo previo es **15–18 salas, con base provisional de 16** agrupadas en siete áreas. No es un límite absoluto. La sala 17 para el jefe es una propuesta adicional de planificación, pendiente de conciliar con G2, que ya tiene función de arena en T21. Ninguna de esas reservas está construida. Un aumento, fusión o cambio de función debe registrar su efecto sobre alcance, mapa, dependencias y pruebas antes de ejecutarse.

El cronograma original del GDD corresponde a un alcance menor y ya advertía que debía revisarse. Esta hoja ordena trabajo por resultados, sin trasladar esas fechas automáticamente al nuevo alcance.

## 2. Base técnica y seguimiento actualizado en T25

**Área** = conjunto geográfico A–G. **Sala del mapa** = ID A1–G3 con una función y envolvente de diseño. **Escena Unity** = asset que carga el juego. No son unidades intercambiables: seis escenas no equivalen a seis áreas completas ni a las 16 salas terminadas.

| Existe realmente | Alcance y límite |
| --- | --- |
| Ocho escenas en `Assets/_Project/Scenes/Arboleda/TechnicalCircuit/` | Seis muestras A–F, B2 Pasarela Quebrada y **`Arboleda_B3_TroncoPartido.unity`**, con ocho RoomDefinitions. No hay escena G ni arena independiente. |
| Ocho enlaces bidireccionales | A↔B, B↔C, C↔D, D↔E, C↔F, F↔B, B↔B2/P02 y **B2↔B3/P03**: 16 salidas dirigidas. E↔B no existe. |
| Adaptaciones T22–T25 | A sirve de muestra A1. B1 conserva RoomId B/Llama; B2 y B3 son escenas independientes enlazadas consecutivamente. B3 respeta ancla (32,−4) y envolvente 12×20, con capas inferior/superior. C aún conserva extensión técnica hacia D/F, sin C2/F1 construidas. |
| P01/P04 y P02 | P01/P04 conservan su relación T22; **P04 usa x=11** frente al x=8 del SVG. T24 ajusta solo el lado este del pozo B a x=14. P02 conecta B1 este↔B2 oeste en global (16,0). Mapa §17 conserva el estado T22 y §18 da las posiciones vigentes. |
| P03 y reserva P12 | P03 une B2 este a B3 oeste en global (32,0), con suelo local 0/4. B3 tiene cuenca y ascenso desde local −4 hasta 4; **P12 (38,−8) sigue sellado y sin transición**. No se atribuye su reserva a la escena técnica F actual. |
| Sistemas aprobados | Movimiento, dash/doble salto sujetos a desbloqueos, ataques/pogo, parry, postura/vulnerabilidad del enemigo, daño/muerte, feedback T10/T20, HUD, Llamas, transición y estado de sesión. La aprobación funcional no implica balance regional final. |
| Colmillo y descanso | Recogida inicial en A; Llama única en B, ID `B.memory-flame`. No hay adquisición jugable de dash/doble salto ni enemigos colocados en A–F/B2/B3. |
| Pruebas y prototipos | `MovementTest` y `ArboledaPrototype` conservan sus objetivos y enemigo de prueba; existen prefab/datos de enemigo terrestre. No se da por realizada su distribución regional. |
| Arquitectura de salas | RoomSession carga Single y conserva vida/arma/desbloqueos/checkpoint durante la sesión; cada sala recrea RoomPlayerRig. RoomDefinition no guarda ancla global o límites de mapa. No hay minimapa ni guardado permanente. |

T22 aprobó un tramo parcial y T24 continúa su adaptación. F↔B sigue este↔este: su acceso B se recoloca sobre una terraza elevada, separado de P02, sin cambiar IDs ni escena F. C↔D/C↔F siguen técnicos y la extensión de C invade la reserva futura F1. Ninguno de esos pendientes se declara resuelto por añadir B2.

## 3. Principios de trabajo

1. Alinear accesos y alturas en un plano global. Un cambio considerable de altura debe tener un ascenso/descenso visible y recorrible.
2. Alternar exploración horizontal y vertical, tamaños y funciones de sala; medir orientación y deseo de explorar además de transitabilidad.
3. Justificar atajos mediante geografía y utilidad. Mantener el retorno inferior reconocible; no cerrar círculos solo por simetría.
4. Preparar IDs, anclas, límites y puertos compatibles con un mapa futuro, sin trasladar físicamente los rigs a coordenadas globales ni rehacer RoomSession por anticipado.
5. Mantener rutas obligatorias accesibles con movimiento y salto normal mientras no exista adquisición aprobada de habilidades. Las reservas opcionales deben indicar habilidad requerida, método de adquisición pendiente, alternativa y recuperación de caída.
6. Colocar enemigos con intención: suelo apropiado, lectura previa, descanso y separación de aterrizajes/checkpoints; no rellenar cada plataforma.
7. Introducir ataques, parry y pogo progresivamente, conservando alternativas. Las opciones elementales pertenecen a la visión del GDD; no están implementadas ni se convierten aquí en requisitos de Arboleda.
8. Dar motivos para explorar mediante lugares, descubrimientos y narrativa sencilla aprobada. No inventar recompensas o lore para justificar una sala.
9. Mantener identidad 2D dibujada a mano, siluetas claras y legibilidad. El arte de prueba puede entrar antes del acabado final.
10. Entregar un sistema o bloque comprobable por tarea, con pruebas manuales concretas. Conservar regresiones y contenido válido; no regenerar destructivamente las escenas aprobadas.

## 4. Cuatro fases y criterios de finalización

El orden de cierre es **Fase 1 → Fase 2 → Fase 3 → Fase 4**. Se permiten bocetos, pruebas de sprites y preparación de diseños en paralelo al trabajo técnico; no se exige tener todo el arte dibujado para avanzar. Cerrar una fase requiere resultados y aprobación manual, no terminar un número fijo de tareas.

### Fase 1 — Geografía y exploración

**Objetivo:** convertir el plano provisional en estructura espacial jugable, con salas suficientemente amplias y distintas, conexiones horizontales/verticales coherentes y retornos reconocibles. Parte de T22 está hecha; la fase completa sigue pendiente.

Adaptar/ampliar las muestras A–F, construir los recintos necesarios y corregir F↔B mediante F1→B3→B2→B1. Separar la extensión técnica C/F y revisar los accesos occidentales hacia las ruinas. Las dimensiones pueden revisarse con justificación; conservar relaciones artificiales no tiene prioridad sobre la geografía. Registrar posiciones globales, límites físicos/visuales y correspondencia local/global de accesos en el documento espacial. El minimapa funcional queda para Fase 3.

La reserva G2/G3 se trata como graybox sin jefe ni salida regional operativa. Antes de fijarla, decidir cómo afecta la propuesta de sala 17 (D03). Si se difiere alguna geometría a Fase 3, hace falta una **revisión explícita aprobada del alcance de Fase 1**; no se considera construida por estar dibujada.

**Cierre de Fase 1, todos exigibles:**

- Las salas previstas están construidas, o una revisión aprobada identifica exactamente cuáles cambian o se difieren y por qué.
- Cada enlace final tiene posición y dirección compatibles con un mapa global sin solapes de rutas ni teletransportes inexplicados. Los enlaces técnicos temporales se han retirado o sustituido en el alcance cerrado.
- Recorridos previstos, caídas y regreso son físicamente accesibles sin exigir habilidades todavía sin adquisición; se prueban ambos sentidos, aperturas y recuperación.
- F↔B tiene ascenso físico reconocible. El segundo circuito entre áreas exigido por GDD se resuelve con P17 aprobado o una alternativa útil aprobada; no se presupone E↔B.
- Se conservan vida reducida entre salas, Colmillo, desbloqueos, Llama, muerte/R, cámara y ausencia de rigs/HUD duplicados; siguen funcionando los prototipos técnicos.
- El usuario aprueba navegación, orientación, tamaños y sensación de exploración. Un recorrido automático no sustituye esta evaluación.

### Fase 2 — Enemigos, combate y contenido

**Objetivo:** convertir la geografía en desafíos y descubrimientos con dificultad progresiva. Depende del cierre de Fase 1, o de un sector estable autorizado expresamente para un ensayo acotado; ese ensayo no cierra toda la fase.

Empezar por un encuentro después de recoger el Colmillo y extender la distribución por sectores. Ofrecer práctica real de horizontal/ascendente, parry y pogo, sin exigir pogo como llave de navegación base. Usar espacios abiertos, verticales y cerrados con superficies compatibles con la IA terrestre; no colocarla donde necesite navegación que no posee.

Medir antes de ajustar: intentos/intervalos de ataque, tiempo de telegrafía y recuperación, oportunidades de parry, reposicionamientos, frecuencia ofensiva y resultados al esquivar con dash. Cambiar una variable o configuración acotada, comparar con la base aprobada y pedir validación específica. No acelerar todos los enemigos ni reducir parámetros del jugador para compensar encuentros mal colocados. El ritmo insuficiente y la eficacia del dash son reportes del encargo, no mediciones realizadas en T23.

Añadir secretos, recompensas y momentos narrativos sencillos solo con contenido aprobado. Evaluar checkpoints adicionales por repetición real tras morir; conservar la Llama central. La función de Llama previa al jefe es necesaria según GDD, pero su ubicación exacta depende de la arena y se integra en Fase 3.

**Cierre de Fase 2, todos exigibles:**

- Encuentros progresivos y suficientemente activos, con avisos legibles, oportunidades reales de parry y alternativas justas; balance provisional aprobado manualmente.
- Primer combate después de la recogida, sin posibilidad de quedar desarmado y atrapado en un encuentro obligatorio si se omitió el Colmillo.
- Ataques/pogo, parry/postura/vulnerabilidad, daño/muerte y colisiones sin empuje se conservan; no se añaden ejecuciones a enemigos normales.
- Las rutas secundarias ofrecen motivos aprobados para explorarlas; ni caídas, enemigos ni secretos crean bloqueos injustos.
- Los descansos incorporados tienen justificación y pruebas de curación, persistencia y retorno tras morir. Se documenta también la decisión de no añadir uno si no hace falta.
- La medición manual exacta de los 0.8 s de i-frames está registrada antes de cerrar esta fase; hasta entonces sigue pendiente.

### Fase 3 — Progresión, minimapa y jefe

**Objetivo:** completar el recorrido regional desde A hasta una salida posterior al primer jefe. Requiere geografía estable y balance base aprobado.

Implementar el minimapa y/o mapa regional según diseño elegido: habitaciones en posiciones globales coherentes, revelación de salas exploradas y ubicación del jugador. Reutilizar los IDs y anclas verificados; decidir el alcance de persistencia de exploración por separado, sin asumir guardado permanente.

Revisar atajos y bloqueos por habilidades. Si se aprueba exigir una habilidad, implementar/probar primero su adquisición y asegurar el acceso hasta ella; si no se aprueba, conservar la ruta obligatoria sin esa dependencia. No asignar automáticamente habilidades o recompensas a las salas históricas.

Preparar la sala 17 propuesta tras resolver D03; diseñar e implementar el **Guardián Raíz** conforme al GDD (al menos dos fases, ataques parryables/no parryables y postura). Integrar la ejecución para jefes en una tarea propia; concretar sus reglas antes de implementarla y no extenderla al enemigo básico. Añadir derrota, desbloqueo de salida y conexión geográfica lógica hacia Región 2. Una sala receptora mínima necesitará alcance aprobado; esto no autoriza construir toda la segunda región.

**Cierre de Fase 3, todos exigibles:**

- Partida desde A: recoger el Colmillo, explorar/progresar, descansar, superar el jefe y alcanzar la salida regional sin bloqueos involuntarios.
- Mapa fiel a las salas construidas, revelación y posición correctas tras transiciones, retornos y respawn; ningún secreto requiere una marca de mapa no diseñada.
- Atajos y requisitos de habilidad comprensibles, con adquisiciones previas donde corresponda.
- Jefe funcional con sus fases, avisos, postura y ejecución aprobados; derrota y reintentos consistentes.
- Llama previa al jefe visible y protegida, retorno breve tras morir y salida habilitada por la derrota según la regla aprobada.
- Prueba integral manual superada; no se declara completa por un fixture aislado del jefe o del mapa.

### Fase 4 — Arte, animación y ambientación

**Objetivo:** sustituir recursos provisionales por una presentación propia y consistente. Dirección confirmada por el usuario: **2D dibujado a mano, sin pixel art como estilo principal**. El usuario desarrolla personalmente la salamandra, priorizando simplicidad, silueta reconocible y facilidad de animación; la referencia a metroidvanias ilustrados no autoriza copiar personajes o escenarios.

El usuario, que está aprendiendo dibujo, producirá recursos progresivamente. Codex ayudará a importarlos, integrarlos, configurar animación y presentación cuando corresponda. No se fija un número final de tareas ni se condiciona el avance técnico a disponer de todos los sprites.

Los lotes comprenden protagonista y animaciones de movimiento/combate; enemigos y jefe; tilesets de suelo/roca/raíces/ramas/ruinas; árboles, vegetación y decoración; fondos por capas y parallax; iluminación ambiental compatible con URP 2D; VFX; legibilidad de plataformas/interactivos, rendimiento y revisión visual integral. Son funciones de producción, no una selección obligatoria de herramientas o arquitectura de animación.

**Cierre de Fase 4, todos exigibles:**

- Recursos principales con identidad consistente y animaciones necesarias funcionando, con aprobación visual del usuario.
- Escenarios reconocibles y legibles: plataformas distinguibles, Llamas identificables y decoración sin confusiones ni obstrucción de rutas.
- VFX/animaciones comunican dirección, impacto, telegrafía y ventanas reales de combate; el acabado no cambia hitboxes o tiempos sin tarea específica.
- Iluminación, fondos y parallax conservan lectura del jugador y de los accesos.
- Rendimiento aceptable conforme al equipo/resolución/objetivo de fluidez que se acuerden; medición en build, no solo impresión en Editor.
- Revisión visual y regresión funcional del recorrido completo aprobadas.

## 5. Seguimiento de tareas

Estados: **Aprobada** = resultado y aprobación manual comunicados; **En revisión** = entrega preparada sin aprobación; **Propuesta** = trabajo sin iniciar; **Condicionada** = depende de decisión explícita. Registrar evidencias en DevelopmentLog y fecha/referencia de aprobación al recibirla. La aprobación del plan no aprueba automáticamente cada tarea futura.

### Base completada

| Tarea | Fase / ámbito | Resultado acotado | Dependencias históricas | Estado al corte | Criterio de aprobación / evidencia |
| --- | --- | --- | --- | --- | --- |
| T01–T15 | Base técnica | Proyecto, jugador, combate/feedback/checkpoints y prototipos de navegación. | Según historial de cada tarea. | Aprobadas por contexto T23. | Pruebas manuales confirmadas; registros específicos en DevelopmentLog. No equivalen a región construida. |
| T16 | Planificación | Áreas y presupuesto regional inicial. | Prototipo T15 y GDD. | Aprobada por contexto T23. | Plan revisado; detalles espaciales posteriores en T21. |
| T17 | Base de salas | Transición, sesión y checkpoints B/C/F; herramientas técnicas. | Sistemas del jugador y Llamas. | Aprobada por contexto T23. | Circuitos y estado entre salas aprobados; no toda su geografía. |
| T18 | Muestra geográfica | Primera escena técnica D y C↔D. | T17. | Aprobada por contexto T23. | Recorrido, conexión y recuperación de D. |
| T19 | Muestra geográfica | Primera escena técnica E y D↔E. | T18. | Aprobada por contexto T23. | Ambas rutas de E y retorno; E↔B no implementada. |
| T20 | Inicio y combate | A, Colmillo inicial y feedback provisional de ataques/recogida. | T17–T19 y contratos de combate. | Aprobada por contexto T23. | Confirmación previa de 8/8 funcionales y 6/6 visuales. |
| T21 | Planificación espacial | Plano provisional de 16 salas, anclas, puertos y recorridos. | T16–T20. | Aprobada como base provisional. | Revisión del plano; no aprobación individual de P17 ni contenido futuro. |
| T22 | Fase 1, primer tramo | Adaptación parcial A↔B↔C/P01/P04. | T21. | Aprobada manualmente por contexto T23. | Recorrido y estado aprobados; diferencias y límites en mapa §17 y registro T22. |

### Entrega actual y trabajo propuesto

**T23–T25 tienen su número de encargo establecido. T26–T51 siguen como secuencia provisional**, sin fechas ni compromiso de cantidad. Se pueden insertar, dividir, fusionar o renumerar tareas no iniciadas, actualizando referencias. Los prefijos Txx no son IDs de sala. Todas las aprobaciones futuras requieren una prueba manual del objetivo indicado, además de los controles comunes de la sección 9.

| Tarea | Fase | Objetivo acotado | Dependencias | Estado | Criterio específico de aprobación |
| --- | --- | --- | --- | --- | --- |
| T23 | Transversal | Consolidar esta hoja de ruta y referencias. | Lectura de T16–T22 y encargo actual. | Aprobada por contexto T24. | Hoja de ruta con commit y aprobación comunicada por el usuario. |
| T24 | 1 | Construir graybox B2 y adaptar B1↔B2/P02, con límites/anclas coherentes. | T23 aprobada; T22; migración de borde oriental/F documentada en mapa §18. | Aprobada por contexto T25. | Pruebas manuales y duración adecuada de B2 confirmadas; enlazar resultados históricos T24. |
| T25 | 1 | Construir B3 y B2↔B3/P03, preparando la base del tronco. | T24 aprobada; puertos T21. | Implementada para revisión manual, no aprobada. | Probar entrada, ascenso/descenso, ruta secundaria, recuperación, ciclo desde A y estado de sesión. P12 sigue reservado; resultados en DevelopmentLog T25 y mapa §19. |
| T26 | 1 | Separar C1 de la extensión técnica y adaptar F como F1/P11. | T22; revisar ramas técnicas C↔D y F↔B antes de moverlas. | Propuesta. | C1↔F1 a cotas alineadas, sin solape global; conexión B↔C y rutas técnicas todavía necesarias permanecen accesibles. Dividir si la migración supera una entrega. |
| T27 | 1 | Construir ascenso F1↔B3/P12 y sustituir el retorno este↔este. | T25 y T26. | Propuesta prioritaria. | B1→C1→F1→B3→B2→B1 y recorrido inverso con física real; retorno reconocible sin habilidades y sin puerta técnica residual incoherente. |
| T28 | 1 | Construir C2 y su bifurcación desde C1/P05. | T26; posición del futuro acceso occidental D. | Propuesta. | Descubrir C2, explorar y regresar; mantener ruta a D hasta su sustitución, sin activar puertas a salas ausentes. |
| T29 | 1 | Adaptar D en D1/D2 y acceso C2↔D1/P06, ascenso P07. | T28; envolventes/descansos revisados. | Propuesta; dividir en D1 y D2 si hace falta. | Subir y bajar las ruinas con pausas, caídas recuperables y salto normal; sustituir el acceso oriental técnico de C y conservar llegada a la muestra E hasta migrarla. |
| T30 | 1 | Adaptar llegada E1 desde D2/P08. | T29. | Propuesta. | Entrada a copas a cota compatible, descenso inverso y orientación legibles; sin pérdida de rutas E que sigan en uso. |
| T31 | 1 | Construir E2 y travesía E1↔E2/P09. | T30. | Propuesta. | Travesía superior y retorno con apoyos/descansos; comprobar cámara y caídas. |
| T32 | 1 | Adaptar mirador E3 y E2↔E3/P10. | T31. | Propuesta. | Alcanzar y abandonar el mirador con ambas direcciones claras; vistas y recuperación sin requerir doble salto. |
| T33 | 1 | Resolver segundo circuito regional y probar el retorno elegido. | T25, T32 y decisión D02. | Condicionada: P17 o alternativa aprobada. | Segundo circuito útil y geográficamente representable, probado en ambos sentidos; no se exige construir P17 si se rechaza. |
| T34 | 1 | Construir F2 y acceso/refugio G1/P13/P14. | T26; criterios de ascenso y descanso. | Propuesta; separar por sala si es necesario. | F1↔F2↔G1 accesible y recuperable con salto normal; descanso prejefe reservado, sin prometer jefe/puerta regional funcional. |
| T35 | 1 | Preparar graybox/reservas G2/G3, sin implementar jefe ni salida a Región 2. | T34; D03 resuelta en cuanto a alcance/funciones. | Condicionada. | Volúmenes y accesos de prueba coherentes, o revisión explícita aprobada que difiere su geometría; ninguna arena doble asumida. |
| T36 | 1 | Cerrar navegación regional y registro global. | T24–T35, incluida resolución de alcance de T35. | Propuesta. | Todos los criterios de Fase 1, revisión humana de orientación y regresiones; no basta conectar todos los nodos. |
| T37 | 2 | Integrar un primer encuentro tras obtener el Colmillo. | T36; emplazamiento aprobado en B2 o C1, sin fijarlo aquí. | Propuesta. | Encuentro legible, daño/muerte/parry/pogo válidos y salida segura si se omitió el arma; sin empuje al caminar/dash. |
| T38 | 2 | Medir ritmo del enemigo/dash e i-frames; ajustar una configuración si lo justifican los datos. | T37; base de parámetros registrada. | Propuesta; medición pendiente. | Comparación reproducible de ataques/parries/esquivas y 0.8 s medidos; cualquier ajuste separado y aprobado, sin aceleración general automática. |
| T39 | 2 | Distribuir encuentros progresivos por sectores estables. | T38 y geografía aprobada. | Propuesta; entregar un sector por lote. | Retos en espacios abiertos/verticales/cerrados, sin spawns injustos ni IA fuera de sus capacidades; balance manual por lote. |
| T40 | 2 | Integrar un descubrimiento o momento narrativo sencillo aprobado. | Sector estable y contenido elegido, p. ej. reserva D2. | Condicionada al contenido. | Motivo real para explorar, abandono/retorno seguros y sin bloquear la historia principal; no inventar recompensa obligatoria. Repetir por lote si se aprueban otros. |
| T41 | 2 | Evaluar repetición tras morir y descansos adicionales. | T39; rutas y tiempos humanos medidos. | Propuesta; cantidad abierta. | Justificación de añadir o no una Llama; si se añade, validar curación/respawn/selección entre checkpoints. El prejefe se concreta en T49. |
| T42 | 2 | Cerrar equilibrio provisional y contenido regional. | T37–T41. | Propuesta. | Criterios de Fase 2 completos y aprobación manual; diferencias respecto a estadísticas anteriores trazables. |
| T43 | 3 | Revisar progresión, habilidades y atajos; implementar una adquisición solo si se aprueba exigirla. | T42; D04. | Condicionada a decisiones de progresión. | Ninguna ruta obligatoria exige una habilidad no obtenible antes; si no hay adquisición aprobada, documentar ruta base sin requisito. Un desbloqueo por entrega si se autorizan varios. |
| T44 | 3 | Implementar mapa/minimapa funcional mínimo. | T36, T42, diseño D05 y IDs/anclas estables. | Propuesta. | Salas ubicadas correctamente, revelación y marcador del jugador correctos al viajar/reaparecer; sin guardar permanentemente por suposición. |
| T45 | 3 | Diseñar y preparar arena del jefe/sala 17 según decisión D03. | T42; T35 o revisión explícita de su alcance; D03; G1 accesible. | Condicionada. | Arena y accesos comprobados, relación con G2/G3 resuelta en mapa y presupuesto; sin construir un segundo jefe. |
| T46 | 3 | Implementar base del Guardián Raíz y primer patrón. | T45; diseño de patrón aprobado. | Propuesta. | Un ciclo de ataque, recepción de daño y recuperación legible y comprobable; aún no equivale a jefe completo. |
| T47 | 3 | Completar segunda fase y patrones parryables/no parryables. | T46; diseño del jefe según GDD. | Propuesta; separar patrones si hace falta. | Cambio de fase y señales justas, combate completo sin dependencia elemental no aprobada; ajustes con pruebas específicas. |
| T48 | 3 | Integrar postura y ejecución exclusiva para jefes. | T47; reglas de ejecución concretadas según GDD. | Condicionada al diseño de reglas. | Ruptura/ventana/ejecución y daño coherentes; enemigo normal conserva aturdimiento/vulnerabilidad y no recibe instakill. |
| T49 | 3 | Integrar Llama previa al jefe y reintento. | T45; ubicación final respecto a la arena aprobada. | Propuesta. | Activar/curar, morir y regresar con vida completa; retorno breve y despejado, selección B/prejefe persistente durante la sesión. |
| T50 | 3 | Integrar derrota, salida y enlace mínimo hacia Región 2. | T47–T49; diseño de desbloqueo y recepción aprobado. | Condicionada a alcance de conexión. | Victoria habilita la salida, no derrota/respawn; no existe salida prematura ni punto de llegada inválido. No autoriza desarrollar Región 2 completa. |
| T51 | 3 | Cerrar recorrido integral de Región 1. | T43, T44 y T50. | Propuesta. | Todos los criterios de Fase 3 probados desde una sesión nueva en A; aprobación manual integral. |

Para Fase 4 se usarán **lotes sin numeración T definitiva**; no representan seis tareas obligatorias ni una cantidad final:

| Lote orientativo | Fase | Objetivo | Dependencias | Estado | Criterio de aprobación |
| --- | --- | --- | --- | --- | --- |
| Protagonista | 4 | Importar sprites y animaciones de movimiento/combate progresivamente. | Diseño del usuario y escala validada; integración por estado. | Propuesto. | Silueta, pivotes, orientación y lectura correctos; ventanas/controles conservados. |
| Criaturas | 4 | Sustituir recursos de enemigos y jefe. | Diseños y patrones estables; geometría de colisión revisada. | Propuesto. | Avisos, fases y vulnerabilidad legibles sin modificar estadísticas por presentación. |
| Escenarios | 4 | Tilesets, raíces/ramas/ruinas, vegetación y decoración. | Geografía estable por sector. | Propuesto. | Rutas e interactivos reconocibles; sin falsas Llamas ni colisiones accidentales. |
| Ambientación | 4 | Fondos, parallax e iluminación URP 2D. | Paleta/recursos aprobados y cámara existente. | Propuesto. | Identidad propia, profundidad y jugador legible en las distintas alturas. |
| Efectos | 4 | Presentación definitiva de impactos, parry, ataques e interacciones. | Contratos de feedback y arte aprobado. | Propuesto. | Señales claras con tiempos reales y sin efectos duplicados al viajar. |
| Rendimiento y cierre | 4 | Medir build y revisar presentación integral. | Integración de los lotes acordados y criterios de rendimiento definidos. | Propuesto. | Criterios de Fase 4 y aprobación visual/funcional, con equipo y métricas registrados. |

## 6. Seguimiento de las 16 salas de T21

IDs y nombres proceden de la sección 4 del mapa. Las conexiones son objetivo propuesto salvo P01/P04, **P02 y P03 implementados en sus tramos descritos arriba**. El resto requiere implementación o revisión. Los contenidos son candidatos, salvo Colmillo/Llama existentes; no implican habilidades o encuentros aprobados.

| ID / nombre | Área | Función prevista | Estado real de implementación | Conexiones objetivo | Ampliación / adaptación necesaria | Contenido documentado | Dependencias / tarea propuesta |
| --- | --- | --- | --- | --- | --- | --- | --- |
| A1 · Claro del Despertar | A | Inicio seguro. | Muestra jugable en escena A, aprobada T20/T22; no acabado final. | B1/P01. | Conservar inicio y escala; revisar con migraciones vecinas. | Colmillo inicial implementado, sin enemigos previstos en el inicio. | T22 conservada; regresión en T24/T36. |
| B1 · Sendero de Esporas | B | Orientación, descanso y bifurcación. | Graybox B de 16 unidades y P02 aprobados en T24; sin cambios en T25. | A1/P01, B2/P02, C1/P04; F técnico elevado provisional. | Conservar pozo x=11/Llama; retirar acceso técnico F al construir retorno B3. | Llama B implementada; lectura de rutas. | Regresión T25; T27 para retorno final. |
| B2 · Pasarela Quebrada | B | Travesía del claro y aproximación al tronco. | Aprobada T24; apertura P03 y llegada oriental añadidas en T25. | B1/P02 y B3/P03 operativos. | Terreno/16×16/cámara conservados; solo se abre el borde reservado. | Raíz amplia para encuentro futuro y mirador para descubrimiento, sin objetos nuevos. | Revisión integración T25; contenido T37. |
| B3 · Tronco Partido | B | Ascenso de retorno y referencia común. | Escena/RoomDefinition 12×20 implementados en T25, pendientes de aprobación manual. | B2/P03 operativo; F1/P12 reservado/sellado; E3/P17 candidato. | Validar lectura/ritmo; abrir P12 solo al construir salida superior compatible en F1. | Claro oriental para encuentro/parry y bolsillo para descubrimiento, sin objetos nuevos. | Prueba manual T25; sector inferior T26/T27; retorno superior T33/D02. |
| C1 · Hondonada de Helechos | C | Descenso, elección inferior y travesía. | Arranque parcial en escena C; retorno superior T22. | B1/P04, C2/P05, F1/P11. | Separar extensión oriental solapada con F1; migrar enlaces técnicos D/F. | Alternativa de primer encuentro; apoyos de recepción seguros. | T26/T28; preservar P04; T37. |
| C2 · Raíces Resonantes | C | Explorar bajo A y llegar a cimientos. | No construida. | C1/P05, D1/P06. | Crear recorrido occidental recuperable. | Reserva de descubrimientos/secretos; sin adquisición de dash. | T28/T29; contenido aprobado T40. |
| D1 · Galería de Musgo | D | Ascenso por cimientos de ruinas. | No construida como subdivisión; escena D es muestra del área. | C2/P06, D2/P07. | Adaptar/split de muestra D; ampliar y medir ascenso/descansos. | Ruinas provisionales como referencia, sin nuevo lore. | T29 tras C2. |
| D2 · Santuario del Colmillo | D | Destino de ruinas y acceso elevado a copas. | No construida como subdivisión; no memoria implementada. | D1/P07, E1/P08. | Distribuir ascenso y cámara; preparar llegada a E a cota común. | Memoria opcional del maestro candidata. | T29/T30; contenido T40 con aprobación. |
| E1 · Nido Suspendido | E | Recepción desde ruinas y comienzo de copas. | No construida; escena E es muestra del área. | D2/P08, E2/P09. | Adaptar la llegada y recuperar caídas. | Identidad de ramas/copa; sin recompensa fijada. | T30/T31. |
| E2 · Cámara del Polen Quieto | E | Travesía superior y pausa ambiental. | No construida. | E1/P09, E3/P10. | Crear espacio propio, revisar anchura y descansos. | Repisas opcionales para revisitas candidatas, habilidades pendientes. | T31/T32; D04 si se propone bloqueo opcional. |
| E3 · Copa del Vigía | E | Mirador y destino de excursión. | No construida como subdivisión; mirador técnico en E. | E2/P10; B3/P17 candidato sin aprobar. | Reutilizar aprendizajes de T19, adaptar envolvente y descenso elegido. | Mirador; no doble salto asignado. | T32/T33; D02. |
| F1 · Atajo de las Raíces | F | Travesía inferior y retorno al claro. | No construida como sala del mapa; escena F conserva gruta técnica. | C1/P11, B3/P12, F2/P13. | Ampliar/adaptar F, separar solape con C y retirar puerta este↔este al sustituirla. | Encuentro de preparación candidato. | T26/T27/T34; contenido T39. |
| F2 · Umbral del Guardián | F | Ascenso hacia el refugio oriental. | No construida. | F1/P13, G1/P14. | Crear ascenso con pausas, caída recuperable y regreso. | Reserva lateral de descubrimiento, sin llave/habilidad asignada. | T34; contenido T40 si se aprueba. |
| G1 · Refugio de Savia | G | Descanso previo a arena. | No construida; no escena G. | F2/P14, G2/P15 según revisión D03. | Crear acceso/refugio y revisar distancia al jefe definitivo. | Llama prejefe: función del GDD, ubicación candidata. | T34/T35; checkpoint T49 tras D03. |
| G2 · Cámara del Guardián Raíz | G | Arena reservada en T21. | No construida; ni jefe ni ejecución de jefe implementados. | G1/P15, G3/P16 tras jefe, sujetos a D03. | Conciliar con sala 17 antes de fijar función, geometría o accesos finales. | Guardián Raíz según GDD, sin diseño final de patrones. | D03; T35/T45–T48. |
| G3 · Salida hacia Abismo | G | Cierre y conexión regional. | No construida; nombre funcional de la antigua Salida del Velo. | G2/P16 y salida PX a receptor aún sin diseñar, sujetos a D03. | Definir destino válido y regla posterior a la victoria. | Salida a Región 2, sin acontecimiento narrativo nuevo. | D03; T35/T50. |

**Balance T25:** muestra A1, graybox B1 adaptado, B2 aprobado, B3 nuevo para revisión y arranque parcial C1; once subdivisiones aún no construidas, aunque D/E/F tengan muestras. T25 no está aprobada ni el retorno geográfico B/F completo. Ninguna sala tiene arte final. No sumar escenas técnicas y salas objetivo como dos inventarios completos.

### Sala 17 del jefe — registro separado

| Referencia de planificación | Área | Función | Estado | Conexiones / adaptación | Dependencias |
| --- | --- | --- | --- | --- | --- |
| Sala 17, **sin ID espacial ni RoomId asignado** | Por decidir; no se inventa área H. | Arena dedicada al primer jefe y vínculo con el tramo de salida hacia Región 2. | Propuesta pendiente; sin escena, posición, dimensiones ni diseño final. | Resolver si amplía/reemplaza G2 o si es adicional; revisar G1/G3, presupuesto y puertos antes de dibujar una ruta. | D03; alcance resuelto antes de cerrar Fase 1; diseño/graybox concreto T45 y jefe en Fase 3. |

**G2 ya reserva una arena:** T23 no aprueba una segunda ni eleva automáticamente el inventario a 17 salas distintas. Si se añade una sala real, actualizar el plano y el total; si se redistribuye G2, registrar la revisión sin contarla dos veces. **P17 es un puerto E3↔B3; no es la sala 17.**

## 7. Problemas conocidos y responsable de resolución

Estos estados registran inspección documental o reportes del usuario. Ningún ajuste se ejecuta en T23. Cerrar un problema requiere evidencia y, cuando proceda, aprobación manual específica.

| ID / problema | Impacto | Fase responsable | Posible tarea | Estado al corte |
| --- | --- | --- | --- | --- |
| R01 · F↔B geográficamente incoherente | Retorno este↔este sin ascenso regional representado. | 1 | T25–T27. | Abierto; funcional como enlace técnico, no corregido por T22. |
| R02 · Algunas muestras demasiado cortas | Poca variación/espacio para explorar o diseñar encuentros. | 1 | T25, T26 y bloques D/E. | B2 confirmada adecuada en T25; evaluar el resto según función, sin duración uniforme obligatoria. |
| R03 · Exploración todavía limitada | El circuito puede sentirse artificial aunque sus transiciones funcionen. | 1, después 2 | T27/T33/T36; T40. | Abierto; requiere prueba humana de elección, orientación e interés. |
| R04 · Sin enemigos en circuito A–F | No hay progresión regional de combate. | 2 | T37/T39. | Confirmado en escenas guardadas; enemigo existe en prototipos. |
| R05 · Ritmo ofensivo insuficiente del enemigo de prueba | Esperas largas y menor oportunidad de práctica activa. | 2 | T38/T42. | Reportado en encargo; pendiente de medir y ajustar con control. |
| R06 · Dash demasiado eficaz ante ciertos ataques | Puede reducir lectura del rival e incentivo de parry. | 2 | T38/T42. | Reportado; no justifica por sí solo cambiar dash o acelerar todo. |
| R07 · Sin minimapa | Falta lectura global de salas exploradas. | 3; preparación de datos en 1 | T36/T44. | No implementado; arquitectura de datos por decidir. |
| R08 · Área G pendiente | No hay refugio ni tramo final construido. | 1 para geografía; 3 para integración | T34/T35/T49/T50. | Sin escena G; no confundir tres reservas con contenido existente. |
| R09 · Sala/jefe pendientes y duplicidad G2/sala 17 | Riesgo de alcance doble y mapa contradictorio. | Decisión en 1; ejecución en 3 | D03; T35/T45–T50. | Abierto; no arena ni boss implementados. |
| R10 · Arte/animaciones definitivos pendientes | Identidad y lectura visual aún provisionales. | 4; ensayos permitidos antes | Lotes F4 por concretar. | Abierto; el usuario produce recursos progresivamente. |
| R11 · Medición exacta de i-frames 0.8 s pendiente | Falta confirmación humana de duración real; no invalida pruebas funcionales anteriores. | 2 / control transversal | T38, antes de cierre T42. | Pendiente explícito; no alterar PlayerHealth para darlo por probado. |
| R12 · Extensión técnica C invade reserva F1 | Aplicar anclas literalmente solaparía recorridos distintos. | 1 | T26. | Confirmado en mapa §17; requiere separación geométrica. |
| R13 · P17 sin aprobar y segundo circuito sin cerrar | La base propuesta sin P17 solo tiene un ciclo independiente. | 1 | D02/T33/T36. | Pendiente; no declarar cumplidos los dos circuitos del GDD. |
| R14 · Cámaras y ascensos frente a nuevas dimensiones | Salas estrechas o ascensos largos pueden resultar incómodos. | 1 | Cada graybox y T36. | Riesgo del mapa T21; requiere pruebas físicas/visuales, no cambio global automático de cámara. |
| R15 · Validadores históricos fijan coordenadas anteriores | Resultados falsos de regresión tras T22 si se ejecutan sin revisión. | 1 / validación transversal | T24 y cada migración. | Documentado en T22; conservar cobertura y actualizar expectativas justificadas. |
| R16 · Errores Search/Burst/paquetes de la copia T22 | Pueden contaminar criterios globales o bloquear una ejecución. | Preparación de pruebas | Previo a validaciones de T24. | Incidencias históricas sin resolución afirmada en T23; registrar entorno y distinguir trazas de producto sin ocultar errores. |

## 8. Gestión de decisiones

### Confirmadas

- Mundo con continuidad espacial, exploración horizontal/vertical, circuitos, alternativas y retornos útiles; geografía compatible con un futuro mapa.
- Estilo dibujado a mano, protagonista diseñado por el usuario, silueta simple y animable; no pixel art principal ni copia de obras existentes.
- Inicio desarmado y Colmillo cercano en A; la posesión no desbloquea dash/doble salto. No retirar parry antes de recoger.
- Llamas del Recuerdo como activación, curación y checkpoint, con función de descanso previa al jefe. La cantidad/ubicaciones finales no se deducen de esa función.
- Ataques, parry, postura y alternativas elementales como dirección de combate del GDD; las alternativas elementales no están implementadas por estar en la visión.
- Enemigo básico: ruptura de postura causa aturdimiento/vulnerabilidad temporal; no ejecución/instakill. Ejecución para jefes se aborda en Fase 3 con sus reglas concretas.
- T01–T24 aprobadas manualmente según encargo T25. T21 es base espacial provisional y las adaptaciones siguen parciales; T25 está pendiente de revisión. La región completa no está aprobada ni terminada.

### Propuestas provisionales

Planta completa de 16 salas y nombres de trabajo, con los tramos construidos distinguidos arriba; eje A–B–C–F–G con D/E opcionales y alternativa B–F; P17 E3↔B3; sala 17; candidatos de encuentros, secretos, recompensas, recuerdos, Llamas y adquisiciones; tareas T26–T51 y lotes F4. Una propuesta de mapa no es autorización automática para implementar su contenido.

### Decisiones pendientes y momento límite

| ID | Cuestión sin resolver | Resolver antes de | Registro/evidencia requerida |
| --- | --- | --- | --- |
| D01 | Límites/IDs/escenas para B1/B2/B3 y C1/F1; ampliaciones, divisiones o fusiones justificadas. | Cada migración T24–T29. | Actualizar correspondencia sala↔escena, mapa/puertos, checkpoint y alcance de pruebas. |
| D02 | Aprobar P17, sustituirlo por otro retorno útil o revisar la planta para el segundo circuito. | T33 y cierre de Fase 1. | Decisión y prueba de utilidad, continuidad y ambos sentidos; no contar P17 antes. |
| D03 | Sala 17 frente a G2: ampliación, sustitución o sala adicional; relación con G1/G3 y total regional. | Fijar geometría G2/G3 en T35 o aprobar su diferimiento antes de T36. | Revisión explícita de alcance/mapa. La geometría exacta y diseño del jefe se concretan en T45, sin ciclo de dependencias entre fases. |
| D04 | Habilidades: si habrá adquisiciones en Arboleda, cuáles, dónde y qué accesos/secretos dependen de ellas. | Construir un bloqueo dependiente; revisión T43. | Método de adquisición comprobado antes del requisito, alternativa segura y ubicación aprobada. |
| D05 | Minimapa, mapa regional o ambos; presentación, revelación y duración del registro explorado. | T44. | Diseño aprobado y pruebas de correspondencia; no asumir guardado permanente. |
| D06 | Ubicación/densidad de enemigos, valores de balance y necesidad de variantes. | Cada lote T37–T39. | Medición base, cambio acotado, comparación y aprobación específica. |
| D07 | Contenido y recompensa de secretos/recuerdo opcional; cantidad/ubicación de Llamas. | T40/T41/T49. | Función, contenido aprobado y prueba de retorno; no nuevo lore por inferencia. |
| D08 | Arena/patrones/reglas de postura-ejecución del jefe, desbloqueo de salida y receptor mínimo de Región 2. | T45–T50, según cada sistema. | Diseño según GDD, alcance explícito y pruebas independientes; sin conceder habilidades/recompensas automáticamente. |
| D09 | Recursos, técnica de animación, audio asociado y objetivos de rendimiento/equipo. | Cada integración artística y cierre de Fase 4. | Recursos aprobados por usuario, escala/legibilidad y métricas registradas; no exigir todo el arte antes de avanzar. |

## 9. Validación, aprobación y mantenimiento

Para cada entrega regional registrar qué se cambió, qué se probó, resultado, límites y prueba manual propuesta. Las mediciones usan el estado real del producto, no solo los candidatos de esta tabla.

- **Geografía:** guardar/reabrir escenas, comprobar referencias y anclas/puertos, recorrer ambos sentidos con entrada y física reales cuando sea viable, fallar saltos y regresar, observar cámara y orientación. Pruebas con fixtures/teletransporte se identifican y no cuentan como navegación física.
- **Estado entre salas:** vida reducida sin curación al viajar; E/norte activan y curan; posesión/desbloqueos conservados; muerte/R con/sin checkpoint, selección entre descansos cuando existan, sesión nueva sin herencia y ningún jugador/cámara/HUD duplicado.
- **Combate:** antes/después de Colmillo, ataques reales y feedback, pogo vivo/vulnerable/letal, fallos sin rebote, parry/postura/daño/muerte y colisiones sin empuje; comparar estadísticas ante cambios de balance. No cambiar reglas para facilitar un test.
- **Pruebas actuales:** `InitialRegionGeographyValidation` / menú `Fangbound/Validate Initial Region Geography T22 in Play Mode` cubre el tramo T22. Sus pruebas de enlaces heredados usan fixtures y no demuestran recorrerlos. Revisar antes de reutilizar `RoomCircuitValidation`: parte de sus expectativas T17–T20 fija posiciones antiguas. Mantener la cobertura al actualizarla, sin borrar aserciones solo para conseguir PASS.
- **Regresiones:** conservar MovementTest (`GroundEnemyValidation`, feedback/checkpoints) y ArboledaPrototype (`ArboledaPrototypeValidation`). Ejecutar compilación y validadores relevantes al cambio de implementación, reportando errores de Editor/paquetes aparte de los del producto. T23 no ejecuta Unity ni reclama regresiones nuevas.

Reglas de mantenimiento:

1. Actualizar estados solo con resultados verificables; distinguir automatización, medición y evaluación humana.
2. No marcar una tarea/fase completada sin aprobación manual. Anotar su fecha o referencia y evidencia; una aprobación no autoriza commits automáticamente.
3. Mantener decisiones confirmadas, propuestas y pendientes diferenciadas. Resolver dependencias antes de iniciar implementaciones extensas.
4. Registrar cambios importantes de alcance (salas, puertas, jefe, habilidades, recursos) en esta hoja y DevelopmentLog. Actualizar mapa/puertos antes de considerarlos nueva base.
5. Mantener IDs y correspondencia con Region1SpatialMap; las diferencias T22 son vigentes hasta una adaptación aprobada. No copiar coordenadas antiguas del SVG a ciegas.
6. Al dividir una tarea, conservar trazabilidad mediante subtareas, dependencias, prueba manual propia y criterio de cierre; no ampliar silenciosamente una entrega. Actualizar numeración provisional y referencias afectadas.
7. Revisar todos los criterios de cierre de fase. Si una sala o criterio se difiere, exigir revisión explícita de alcance; no convertir pendiente en aprobado por omisión.
8. Referenciar GDD/mapa en vez de duplicarlos. Conservar registros históricos y añadir la nueva aprobación sin reescribir qué se probó entonces.

La región se considerará terminada cuando las cuatro fases cumplan sus criterios, el usuario apruebe el recorrido completo, combate/jefe/salida, mapa, presentación y rendimiento, y no queden bloqueos o pendientes críticos sin una decisión explícita. Tener las 16 salas en una tabla, una compilación correcta o el jefe aislado no basta.

## 10. Alcance recomendado en T23 y ejecutado en T24

**Primer bloque de implementación: B1↔B2/P02**, reutilizando lo aprobado en T22. Crear una Pasarela Quebrada con espacio para explorar y regresar; dejar B3/P12 para las siguientes entregas. Antes de editar, concretar la correspondencia de escenas/IDs y cómo migrar el borde oriental provisional de B sin romper el enlace técnico a F todavía en uso. No crear una puerta a B3 si B3 no existe. Si mantener operativa esa ruta exige ampliar demasiado el alcance, subdividir expresamente el bloque antes de ejecutarlo.

Conservar A/Colmillo, pozo B↔C (x=11), Llama B e integridad de sesión. Registrar ancla, límites y puertos de B1/B2; no imponer que su rectángulo sea igual al encuadre de cámara. No añadir enemigos, habilidades o arte definitivo en ese bloque.

Prueba manual de aceptación propuesta: iniciar A en una sesión nueva, recoger, viajar a B, activar Llama, explorar B2 y volver; bajar y regresar por B↔C solo con salto normal; repetir con vida reducida y verificar muerte/R; probar la ruta técnica F que deba seguir disponible y revisar cámara, suelo y orientación. Ejecutar validación geográfica actualizada y regresiones pertinentes, sin regenerar escenas aprobadas. Cerrar T24 únicamente tras la revisión manual de ese tramo.

Ese fue el alcance aprobado de T24, ya confirmado manualmente. T25 añade B3/P03 según mapa §19, sin adelantar P12. Tras aprobar T25, el siguiente bloque propuesto es separar C1/F1 (T26 provisional) y posteriormente construir/validar el retorno F1↔B3/P12 (T27), antes de retirar F↔B1 técnico. No se declara cerrado el retorno por haber construido sus raíces interiores.
