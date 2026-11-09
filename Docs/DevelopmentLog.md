# Registro de desarrollo

## Tarea 01: Preparación y prototipo inicial

### Estado inicial encontrado

- Unity Editor del proyecto: `6000.5.5f1`.
- URP está instalado (`com.unity.render-pipelines.universal` `17.6.0`) y el perfil de calidad activo referencia un asset URP. El Renderer configurado es `Renderer2D`.
- El Input System está instalado (`com.unity.inputsystem` `1.19.0`), `activeInputHandler` está en modo Input System y existe `Assets/Settings/InputSystem_Actions.inputactions` con un mapa `Player`.
- La gravedad 2D existente es `(0, -9.81)`; se mantienen los valores de iteraciones y demás ajustes del proyecto.
- Existe `Assets/Scenes/SampleScene.unity`, con cámara ortográfica predeterminada. Se conserva sin cambios.
- No se encontraron scripts C# propios ni escena de pruebas previa.
- La estructura propia existente usa carpetas numeradas vacías (`00_Scripts`, `01_Data`, `03_Prefabs`, `04_Scenes`, `05_Art`, `06_Audio`, `07_Settings`). Se reutiliza; no se reorganiza.
- Antes de esta tarea ya aparecían modificaciones locales en `Assets/Settings/UniversalRenderPipelineGlobalSettings.asset`, `ProjectSettings/EditorBuildSettings.asset`, `ProjectSettings/ProjectSettings.asset`, `ProjectSettings/ShaderGraphSettings.asset` y contenido sin seguimiento en `Docs/`. Esos cambios se conservaron. `Docs/GDD.md` se leyó y no se modificó.

### Archivos creados

- `AGENTS.md`: reglas persistentes para el trabajo futuro.
- `Assets/_Project/00_Scripts/Editor/MovementTestSceneGenerator.cs`: comando de Editor que genera la escena de prueba con objetos y componentes Unity.
- `Assets/_Project/05_Art/Placeholder/MovementTestSquare.asset`: sprite cuadrado blanco creado localmente por el generador para los placeholders.
- `Assets/_Project/Scenes/Test/MovementTest.unity`: escena de pruebas generada con Unity Editor.
- `Docs/DevelopmentLog.md`: este registro.
- `Docs/Region1Plan.md`: propuesta de conexiones para 16 salas; no son salas implementadas.
- Unity creó los `.meta` asociados a carpetas y assets, y `ProjectSettings/SceneTemplateSettings.json` durante la ejecución del Editor.

### Configuración realizada

- No se instalaron paquetes ni se modificó la configuración global de proyecto.
- El generador está preparado para crear cámara ortográfica, suelo dividido para dejar un hueco, tres plataformas a distintas alturas, pared, punto de aparición, salamandra provisional oscura con indicador de dirección y colisionadores 2D.
- La salamandra usa Rigidbody2D y CapsuleCollider2D, sin controlador ni lógica de movimiento.
- No se añadió MovementTest a Build Settings para evitar alterar la lista de escenas que ya tenía cambios locales.

### Problemas detectados y verificación

- La carpeta `Assets/_Project/Scenes/Test` no existía y se crea únicamente para la escena solicitada.
- La escena se genera mediante la API de Unity Editor, no serializando YAML a mano.
- Unity 6000.5.5f1 compiló el script en modo batch, ejecutó `Generate`, guardó la escena y confirmó los objetos requeridos.
- Se inspeccionó la serialización de la escena: cámara ortográfica, objetos requeridos, colliders de bloques de tamaño local 1×1, CapsuleCollider2D del jugador y Rigidbody2D. El sprite placeholder es un asset local, no externo.
- El diagnóstico de VS Code no detecta errores en el script.
- No se ejecutó Play Mode ni se tomó una captura de pantalla; la composición visual y el contacto real entre colliders requieren comprobación manual en el Editor.

### Pasos pendientes

- Abrir `Assets/_Project/Scenes/Test/MovementTest.unity` y comprobar cámara, encuadre, colores y colisiones en Play Mode.
- Revisar y aprobar el mapa propuesto en `Docs/Region1Plan.md` antes de construir salas.
- La siguiente tarea puede abordar el controlador de movimiento básico, de forma aislada y con pruebas específicas.

## Tarea 02: Auditoría y movimiento básico

### Auditoría de carpetas

- `Assets/_Project/00_Scripts/Editor` contiene la herramienta de generación/configuración de `MovementTest`; se conserva en su ubicación porque es código de Editor existente.
- `Assets/_Project/05_Art/Placeholder` contiene el sprite cuadrado gratuito usado por la escena.
- `Assets/_Project/Scenes/Test` contiene la escena de prueba solicitada en la Tarea 01. `04_Scenes`, `03_Prefabs`, `06_Audio` y `07_Settings` permanecen vacías; no se borraron.
- `01_Data` estaba vacía y ahora contiene `Player/PlayerMovementData.asset`.
- Se crearon los scripts en la ruta explícita solicitada `Scripts/Player`. La estructura numerada anterior (`00_Scripts`, `01_Data`, etc.) se conserva; no se movieron ni renombraron assets para resolver diferencias de convención.
- No se encontraron prefabs, ScriptableObjects ni scripts de juego preexistentes que duplicaran esta implementación. No se modificaron paquetes, ajustes globales, recursos internos ni el GDD.

### Implementación

- `Assets/_Project/Scripts/Player/PlayerController.cs`: lectura de Move/Jump/Sprint, aceleración, frenado, inversión rápida, orientación visual, salto variable, coyote time, jump buffer, comprobación de suelo por LayerMask y límite de caída.
- `Assets/_Project/Scripts/Player/PlayerMovementData.cs`: ScriptableObject con parámetros de movimiento.
- `Assets/_Project/01_Data/Player/PlayerMovementData.asset`: configuración inicial conectada al personaje.
- `Assets/_Project/00_Scripts/Editor/MovementTestSceneGenerator.cs`: se añadió un comando de Editor para enlazar controlador y asset sin reconstruir la escena; también agrega un techo de prueba solo si no existe.
- `Assets/_Project/Scenes/Test/MovementTest.unity`: mantiene las superficies previas, integra el controlador y añade el techo de prueba. Usa `Default` como capa de suelo.
- Se reutilizó `Assets/Settings/InputSystem_Actions.inputactions`: Move incluye stick, cruceta, A/D y flechas; Jump incluye Espacio y botón inferior; Sprint usa Shift y pulsación del stick izquierdo.

### Valores iniciales

- Caminar `4.5`, correr `7.5`, aceleración `50`, frenado `60`, giro `90`.
- Salto `13`, coyote time `0.12 s`, jump buffer `0.12 s`, corte al soltar `0.45`.
- Gravedad del Rigidbody2D `3`, caída máxima `18`, distancia de detección de suelo `0.14`, ancho relativo `0.85`.

### Verificación

- Unity `6000.5.5f1` compiló los ensamblados de juego y Editor; Tundra terminó correctamente sin errores C# en el log.
- El comando de Editor creó el asset y verificó las referencias serializadas, collider y Rigidbody2D del jugador. La escena guarda el `LayerMask` `Default` y conserva las plataformas.
- No se probó en Play Mode. Aceleración, frenado, inversión, salto variable, coyote time, jump buffer y colisiones deben comprobarse manualmente en Unity.

### Siguiente paso

- Revisar la escena en Play Mode con los pasos indicados al entregar esta tarea. Si la detección de suelo o el arco del salto requieren ajuste, cambiar los valores del ScriptableObject antes de modificar el controlador.
- La Tarea 03 puede comenzar tras validar estos fundamentos; dash, doble salto, pogo y combate permanecen fuera de esta entrega.

## Tarea 02.1: El personaje se suspende contra plataformas

### Diagnóstico

- El CapsuleCollider2D del jugador y los colliders rectangulares de las plataformas no tenían Physics Material 2D asignado (`m_Material` nulo); `Physics2DSettings` tampoco define un material predeterminado propio. Al empujar horizontalmente contra una cara vertical, la fricción de contacto predeterminada podía oponerse a la gravedad y dejar al jugador suspendido.
- `PlayerController.FixedUpdate` no anula la velocidad vertical al tocar una pared: conserva `body.linearVelocity.y`, salvo al iniciar un salto o limitar la caída máxima.
- La comprobación de suelo es un OverlapBox bajo los pies, con un ancho del 85% de los bounds del capsule. En el contacto lateral vertical de esta escena queda dentro del ancho del personaje, por lo que la pared no entra en esa consulta. No se modificó esa detección.

### Corrección

- Se creó `Assets/_Project/01_Data/Player/PlayerNoFriction.physicsMaterial2D` desde la API de Unity Editor.
- El material establece fricción y rebote en `0`, y combina ambos con `Minimum` para evitar reintroducir fricción/rebote al combinar con colliders sin material.
- Se asignó únicamente al CapsuleCollider2D de `Salamander Placeholder`. No se cambiaron código ni valores de aceleración, velocidad o salto.
- Se modificó el comando de Editor de `MovementTestSceneGenerator` para crear/reutilizar el material, asignarlo y verificar el estado guardado.

### Verificación

- Unity `6000.5.5f1` recompiló los ensamblados sin errores y ejecutó el comando de aplicación; el log terminó con código de salida `0`.
- Se verificó el asset serializado: fricción `0`, rebote `0`, combinación mínima y referencia asignada al CapsuleCollider2D en `MovementTest`.
- No se ejecutó Play Mode después de aplicar el cambio. La ausencia de suspensión y el contacto real con suelo, laterales y techo necesitan la prueba manual.

### Prueba manual pendiente

- En `MovementTest`, entrar en Play Mode y saltar hacia el lateral izquierdo de una plataforma manteniendo D; repetir contra el lateral derecho manteniendo A. En ambos casos debe seguir cayendo sin soltar la dirección.
- Aterrizar en la parte superior y caminar sobre suelo/plataforma; confirmar que permanece apoyado y que el frenado del controlador evita deslizamiento anormal.
- Saltar debajo de una plataforma y confirmar que su cara inferior detiene el ascenso.
- Revisar la consola de Unity para errores de compilación o ejecución.

## Tarea 02.2: Estructura sin numeración

### Estructura final

```text
Assets/_Project/
	Animations/
	Art/
		Placeholder/MovementTestSquare.asset
	Audio/
	Data/
		Player/PlayerMovementData.asset
		Player/PlayerNoFriction.physicsMaterial2D
	Materials/
	Prefabs/
	Scenes/
		Test/MovementTest.unity
	Scripts/
		Editor/MovementTestSceneGenerator.cs
		Player/PlayerController.cs
		Player/PlayerMovementData.cs
	Settings/
	Tests/
```

`Animations`, `Audio`, `Materials`, `Prefabs`, `Settings` y `Tests` están vacías, pero se conservaron/crearon como categorías previstas. No se eliminaron por estar vacías.

### Movimientos realizados

- `00_Scripts/Editor` se fusionó en `Scripts/Editor`; `MovementTestSceneGenerator.cs` y sus `.meta` se movieron mediante `AssetDatabase.MoveAsset`.
- `01_Data` se trasladó a `Data`.
- `03_Prefabs` se trasladó a `Prefabs`.
- `04_Scenes` estaba vacía y se fusionó con `Scenes`; su raíz se retiró después de verificar que quedó vacía.
- `05_Art` se trasladó a `Art`.
- `06_Audio` se trasladó a `Audio`.
- `07_Settings` se trasladó a `Settings`.
- `Animations`, `Materials` y `Tests` se crearon con la API de carpetas de Unity.
- `00_Scripts` quedó vacío al mover `Editor`; se retiró entonces mediante AssetDatabase. No quedaron carpetas numeradas.
- No hubo conflictos de nombres ni assets que debieran quedarse sin mover. El migrador temporal se retiró tras completar la tarea.

### Referencias y compilación

- La migración capturó los GUIDs del árbol antes de mover y comprobó que siguieran resolviendo después, exceptuando solo las raíces vacías fusionadas y retiradas.
- Se compararon además los `.meta` del generador de Editor, controlador, ScriptableObject, asset de datos, material físico, sprite placeholder y escena; todos conservan sus GUID originales.
- Unity volvió a abrir `Scenes/Test/MovementTest.unity` y verificó `PlayerController`, sus referencias de datos e Input Actions, el material del jugador y los objetos de suelo/plataformas/pared/techo.
- Unity 6000.5.5f1 compiló antes y después del traslado, y también después de retirar el migrador temporal. Tundra terminó con código 0 y sin errores C#.
- El generador actualiza las rutas de datos y arte a `Data` y `Art`. No se modificó el código de movimiento ni el GDD.

## Tarea 03: Dash y doble salto

### Implementación

- `Assets/_Project/Scripts/Player/PlayerAbilities.cs` mantiene dash y doble salto desactivados por defecto y permite configurar su disponibilidad desde el Inspector.
- `PlayerController` conserva el movimiento horizontal y la detección de suelo existentes; el dash suspende temporalmente la velocidad/gravedad normal, usa la orientación actual y restaura la gravedad al finalizar. Se permite un dash por periodo aéreo, con recuperación al aterrizar y cooldown.
- El doble salto reutiliza el buffer y la detección de suelo/coyote existentes. Solo se habilita tras un salto normal o de coyote; consume el salto aéreo adicional una sola vez y se reinicia al aterrizar.
- La velocidad horizontal ahora es única y configurable (`7.5`), equivalente al máximo anterior; Shift deja de ser sprint y pasa a Dash.
- `InputSystem_Actions.inputactions`: Sprint se sustituyó por Dash, asignado a `Left Shift` y `Gamepad/rightShoulder`. Jump conserva Espacio y botón inferior.
- `MovementTest` tiene ambos flags de `PlayerAbilities` activos para prueba. Una instancia nueva de `PlayerAbilities` mantiene ambas opciones apagadas.
- El hueco existente sirve para ensayar el dash; no se reconstruyó la escena ni se cambió su material sin fricción. La invulnerabilidad durante dash queda como parámetro configurable, sin sistema de daño.

### Parámetros iniciales

- Velocidad horizontal `7.5`; aceleración `50`, frenado `60`, giro `90`.
- Dash: distancia `3.2`, velocidad `18`, duración máxima `0.18 s`, cooldown `0.35 s`; invulnerabilidad configurable, activa como opción de datos, pero aún sin sistema que la aplique a daño.
- Doble salto: velocidad vertical `12`; salto normal conserva `13` y los valores existentes de corte, coyote y buffer.

### Verificación

- Unity 6000.5.5f1 compiló ensamblados de juego y Editor sin errores; Tundra terminó con código `0`.
- El configurador volvió a abrir/guardar la escena y verificó la configuración de datos/acciones; el JSON de Input Actions se parseó y se comprobó que Dash tiene únicamente Shift y el hombro derecho.
- La escena serializa `PlayerAbilities` con ambos flags activos y conserva la referencia al controlador, los datos de movimiento, el material sin fricción y las plataformas.
- No se ejecutó Play Mode. Dash en suelo/aire, límite y recuperación aérea, doble salto/límite del tercer salto, combinación de habilidades, colisión con paredes y caída sin adherencia quedan pendientes de la prueba manual.

### Pruebas manuales pendientes

- Con ambas opciones activas en `MovementTest`, probar dash en suelo y aire; repetir dash en el mismo periodo aéreo (debe bloquearse), aterrizar y probarlo de nuevo.
- Saltar y pulsar Espacio/botón inferior una segunda vez; intentar un tercer salto antes de aterrizar y confirmar que no ocurre. Repetir combinando dash y doble salto.
- Probar dash contra pared y junto a laterales de plataformas; comprobar que no atraviesa paredes ni suspende al personaje, y que la gravedad vuelve al terminar.
- Desactivar ambos flags desde `PlayerAbilities` y comprobar que el movimiento y salto normal siguen operativos sin dash ni salto extra.
- Revisar la consola de Unity durante estas pruebas.

## Tarea 04: Cámara de seguimiento 2D

### Decisión e implementación

- Cinemachine no está declarado en `Packages/manifest.json` ni instalado en `Library/PackageCache`; no se instaló ningún paquete.
- Se creó `Assets/_Project/Scripts/Camera/CameraFollow2D.cs`, implementado con `LateUpdate` y `Vector3.SmoothDamp`. Tiene objetivo, collider de límites, zona muerta X/Y y suavidad editables desde el Inspector.
- Los límites usan el bounds de un `BoxCollider2D` trigger; la cámara se clampa considerando el tamaño ortográfico y aspect ratio para no mostrar fuera de la sala. El trigger no participa en las colisiones del jugador.
- `MovementTest` conserva la cámara ortográfica, Renderer 2D, plataformas y material sin fricción. Se añadió `Camera Room Bounds` con tamaño `28 x 20`, centrado en `(0, 1.2)`, y se enlazó al seguimiento.
- Se configuró una zona muerta de `1.2 x 0.8`, suavidad `0.12 s`, y `Rigidbody2D.interpolation = Interpolate` solo para reducir jitter visual. No se modificaron scripts ni datos de movimiento.

### Verificación

- Unity 6000.5.5f1 compiló el componente y el código de Editor; Tundra terminó con código `0` y sin errores C#.
- El configurador abrió y guardó la escena existente; se verificaron la cámara ortográfica/tamaño, el objetivo, los límites trigger y la interpolación del Rigidbody2D.
- No se ejecutó Play Mode. El seguimiento al caminar, dash, saltos, caídas, límites y jitter deben probarse manualmente.

### Pruebas manuales pendientes

- Abrir `Scenes/Test/MovementTest.unity` y entrar en Play Mode; caminar a ambos lados y comprobar que la cámara espera dentro de la zona muerta y sigue con suavidad al salir de ella.
- Probar dash, salto normal, doble salto y caída desde plataformas; comprobar que el jugador permanece visible y que no hay vibración.
- Recorrer los cuatro bordes de `Camera Room Bounds` y verificar que el viewport no muestra fuera del rectángulo.
- En Inspector, variar `Dead Zone Size`, `Smooth Time` y el BoxCollider2D para comprobar que la configuración por sala es utilizable.
- Revisar la consola de Unity durante las pruebas.

## Tarea 05: Sistema de pogo

### Estado previo

- Se leyeron `AGENTS.md`, `Docs/GDD.md` y este registro. La copia local estaba limpia en `main` y contiene el controlador, dash, doble salto, cámara y `MovementTest` de las tareas anteriores. No había código de pogo ni objetivos compatibles.
- Se conservó `PlayerMovementData.asset` y no se modificaron los valores de movimiento existentes. El GDD no se modificó.

### Implementación

- `PlayerPogo` usa las acciones `Move` y `Attack` del Input System. Requiere estar en el aire, mantener abajo y pulsar ataque. Ejecuta un descenso y detecta objetivos solo durante la ventana configurada. Permite un intento por periodo aéreo; un impacto válido habilita otro y aterrizar también reinicia la disponibilidad.
- `PlayerPogoData` define ventana de ataque, velocidad de descenso, velocidad de rebote, desplazamiento y tamaño de la zona de impacto. El estado temporal permanece en `PlayerPogo`.
- `PogoTarget` marca colliders compatibles sin introducir vida, daño ni lógica de enemigos. El mismo objetivo no puede recibir impactos repetidos durante un ataque.
- `PlayerController` recibió solo dos métodos de física para iniciar el descenso y aplicar el rebote. El rebote conserva el movimiento horizontal y no restaura dash ni doble salto. También limpia el salto almacenado y el corte de salto pendiente para que no anulen el impulso vertical.
- Se añadió `J` a la acción `Attack`; se conserva el botón oeste del mando. La cruceta del mando se añadió a `Move` para permitir mantener abajo.
- `MovementTest` incluye `PlayerPogo`, su asset de datos y dos objetivos dorados estáticos con colliders trigger, para que el controlador no los confunda con suelo. `PogoTestSceneSetup` permite volver a aplicar esta configuración desde `Fangbound/Configure Movement Test Pogo` sin reconstruir la escena.

### Verificación realizada

- Unity `6000.5.5f1` compiló los scripts y ejecutó `PogoTestSceneSetup.Configure` en modo batch; el proceso terminó con código `0` y confirmó los dos objetivos en `MovementTest`.
- Se comprobó la serialización de `PlayerPogo`, sus referencias al asset de datos y las acciones, y los componentes `PogoTarget` de ambos objetivos.
- La primera compilación detectó un uso incorrecto de `ContactFilter2D.NoFilter`; se corrigió y la compilación final no mostró errores C#.
- No se ejecutó Play Mode. El tacto del descenso, el rebote y las combinaciones de habilidades aún requieren prueba manual.

### Prueba manual pendiente

1. Abrir `Assets/_Project/Scenes/Test/MovementTest.unity` y entrar en Play Mode. Saltar sobre el primer objetivo, mantener `S` o flecha abajo y pulsar `J`; comprobar descenso y rebote vertical.
2. Dirigir el rebote con `A/D` o flechas y hacer otro pogo sobre el segundo objetivo sin tocar el suelo. En mando: stick o cruceta abajo más botón oeste; mover con stick o cruceta.
3. Atacar hacia abajo lejos de los objetivos; debe haber descenso sin rebote y no se debe poder repetir el pogo hasta acertar o aterrizar.
4. Mantener abajo y pulsar ataque desde el suelo; no debe iniciar el descenso. Probar también un ataque en el aire sin mantener abajo.
5. Combinar pogo con doble salto y dash antes y después de un impacto. Confirmar que el pogo no devuelve usos consumidos y que el control horizontal vuelve tras el rebote.
6. Revisar la consola para errores de ejecución y ajustar únicamente `PlayerPogoData.asset` si la ventana o la zona de impacto requieren calibración.

### Limitaciones actuales

- Solo los colliders con `PogoTarget` producen rebote. El GDD contempla suelo, pinchos y enemigos; su integración queda para las tareas correspondientes.
- No hay animación ni efecto visual de ataque. La zona se puede ver como gizmo al seleccionar al jugador en Scene View.

## Tarea 06: Combate básico y ajuste del pogo

### Ajuste del pogo

- `PlayerController.TryStartPogoDive` ya no recibe velocidad de caída ni modifica `linearVelocity.y`; al iniciar el ataque solo limpia coyote time, jump buffer y corte pendiente. La caída queda a cargo de la gravedad normal.
- Se conserva la ventana/hitbox de `PlayerPogo`, la condición en aire, el rebote de `BounceFromPogo`, el límite/reinicio del pogo y los controles abajo + J/botón oeste.
- Se retiraron `DiveSpeed` y `diveSpeed` del componente/datos. Unity reserializó `PlayerPogoData.asset` sin el campo obsoleto; `attackWindow`, rebote y hitbox se conservaron.

### Combate y objetivos

- `Assets/_Project/Scripts/Player/PlayerCombat.cs` lee Move/Attack del Input System. J sin dirección vertical ataca horizontalmente hacia la orientación actual; izquierda/derecha + J fijan la dirección; arriba + J usa el hitbox ascendente. Abajo + J se reserva para pogo en aire y no produce ataque/impulso descendente en suelo.
- PlayerCombat mantiene los objetivos ya golpeados durante cada ventana, evitando impactos repetidos sobre el mismo objetivo. Las zonas horizontal/ascendente, daño y ventana están en `PlayerCombatData`.
- `Assets/_Project/Scripts/Combat/IDamageable.cs` separa la recepción de daño del ataque. `TrainingTarget` implementa vida configurable por componente, flash de color al recibir daño, degradado visual al perder vida y desactivación de collider al agotarse.
- `Assets/_Project/Scripts/Editor/CombatTestSceneSetup.cs` configura el jugador/asset y añade dos blancos reutilizables para probar los ataques. Los dos objetivos dorados `PogoTarget` existentes no se reemplazaron.
- `PlayerCombat` es dueño de habilitar/deshabilitar `Attack`; `PlayerController` sigue gestionando `Move`, que usan tanto combate como pogo.

### Configuración inicial

- PlayerCombatData: daño `1`, ventana `0.12 s`, hitbox horizontal offset `(0.65, 0.05)`, tamaño `(0.85, 0.55)`; ascendente offset `(0, 0.75)`, tamaño `(0.6, 0.8)`.
- Cada objetivo de entrenamiento empieza con `3` puntos de vida y flash visual `0.1 s`.
- Se conservan valores de movimiento, dash, salto, cámara y material sin fricción.

### Verificación

- Unity `6000.5.5f1` compiló el pogo corregido, el combate, los objetivos y el configurador; Tundra terminó con código `0` y sin errores C#.
- El configurador abrió y guardó `MovementTest`, verificó referencias de `PlayerCombat` y `PlayerPogo`, creó el asset de datos y comprobó los blancos de entrenamiento y los dos `PogoTarget` existentes.
- Se comprobó que la escena conserva el material sin fricción, y que `PlayerPogoData.asset` ya no serializa `diveSpeed`.
- No se ejecutó Play Mode; los casos de combate y la sensación de caída/rebote requieren prueba manual.

### Pruebas manuales pendientes

- En Play Mode, mantener abajo + J en el aire: confirmar caída natural sin aceleración añadida y rebote solo al golpear un objetivo dorado. Repetir lejos de objetivos y desde el suelo; no debe haber rebote ni impulso descendente artificial.
- Usar J quieto y derecha/izquierda + J para golpear horizontalmente; comprobar el lado al que mira y que el objetivo pierde vida una sola vez por ataque.
- Usar arriba + J en suelo y aire; confirmar el impacto en el objetivo superior sin impulso automático del jugador.
- Confirmar que abajo + J en aire prioriza pogo, abajo + J en suelo no ataca ni impulsa, y dash/doble salto conservan su comportamiento.
- Repetir ataques hasta agotar los 3 puntos de vida; observar flash/degradado y que el objetivo deja de recibir impactos.
- Revisar la consola de Unity durante las pruebas.

## Tarea 07: Parry y postura

### Implementación

- Se añadió la acción `Parry` al mapa `Player`: K y hombro izquierdo del mando. No comparte botón con Attack/J, Jump/A, ni Dash/Shift/hombro derecho; se activa por `WasPressedThisFrame`, no al mantenerlo.
- `PlayerParry` gestiona una ventana finita, recuperación completa posterior al éxito/fallo y feedback provisional de éxito/fallo/impacto. Puede usarse en suelo y aire.
- El impacto se detecta por la hitbox activa del ataque enemigo. `EnemyAttackInstance` se resuelve una sola vez; `PlayerImpactReceiver` devuelve `Parried`, `Hit`, `AvoidedByDash` o `Ignored`. Dash evita el impacto sin contarlo como parry y sin dar postura.
- `EnemyPosture` separa postura de vida. Cada parry exitoso reduce postura; a cero activa vulnerabilidad temporal. Al terminar, postura se recupera progresivamente; un nuevo parry durante recuperación parcial la vuelve a reducir. La vulnerabilidad activa no acumula postura.
- `TrainingAttackEmitter` ejecuta ciclos estáticos de espera → telegraph → hitbox activa → cooldown. No persigue ni navega; el telegraph visual representa la misma zona de impacto.
- Se añadieron `EnemyPostureData`, `TrainingAttackData` y `PlayerParryData` para los parámetros. El receptor del jugador solo comunica el impacto; no introduce vida, muerte ni respawn del jugador.
- `MovementTest` conserva combate y pogo previos, ambos objetivos de pogo, los blancos horizontal/ascendente y el material sin fricción. Se añadió el entrenador de parry con postura y telegraph.

### Parámetros iniciales

- Parry: ventana `0.15 s`, recuperación `0.4 s`, feedback visual `0.12 s`.
- Postura: máximo `3`, daño por parry `1`, vulnerabilidad `1.5 s`, recuperación `1 punto/s`.
- Entrenador: espera inicial `0.75 s`, telegraph `0.65 s`, ataque activo `0.14 s`, cooldown `1.3 s`. La zona se configura con offset `(-1.8, -0.05)` y tamaño `(3.6, 0.75)`.

### Verificación

- Unity 6000.5.5f1 compiló los componentes de parry/postura, receptor/emisor y el configurador; Tundra terminó con código `0` y sin errores C#.
- `CombatTestSceneSetup.Configure` abrió/guardó `MovementTest` y verificó referencias de PlayerCombat, PlayerParry, PlayerPogo, datos de parry/postura/ataque, receiver, emisor y telegraph.
- La escena contiene dos `PogoTarget`, dos blancos `IDamageable`, el entrenador `EnemyPosture`/`TrainingAttackEmitter` y el material sin fricción. No se cambió movimiento, dash, salto ni cámara.
- No se ejecutó Play Mode; timing de parry, postura, vulnerabilidad y la interacción efectiva con dash quedan pendientes de prueba manual.

### Pruebas manuales pendientes

- Entrar en Play Mode y pulsar K o hombro izquierdo antes del golpe: el flash de éxito debe aparecer, no debe haber impacto y la postura del entrenador debe bajar una vez.
- Pulsar demasiado pronto: el flash de fallo debe aparecer al cerrar la ventana y el siguiente ataque debe impactar. Pulsar después del impacto tampoco debe marcar parry.
- Mantener K: debe producir un solo intento; probarlo en suelo y aire.
- Acertar tres parries: comprobar postura `3 → 0`, color vulnerable durante `1.5 s` y recuperación gradual hasta `3`.
- Recibir ataque durante dash: no debe recibir impacto ni reducir postura. Probar también el feedback de impacto fuera de dash/parry.
- Confirmar que Attack/J, pogo, dash, doble salto y movimiento siguen respondiendo; revisar la consola.

## Tarea 08: Vida, daño, invulnerabilidad y knockback

### Implementación

- `PlayerHealthData` separa parámetros de diseño y `PlayerHealth` mantiene vida/estados temporales. Vida máxima e inicial: `5`; API pública `TryHeal`; eventos `HealthChanged`, `DamageTaken`, `Healed` y `Died`.
- `PlayerImpactReceiver` sigue siendo el único receptor. Orden de resolución: ataque ya resuelto → muerte → dash invulnerable → i-frames posteriores al golpe → parry → daño real. Un impacto en dash no consume parry ni daña; uno durante i-frames tampoco puede registrarse como parry.
- Los impactos válidos aplican daño, `0.8 s` de invulnerabilidad visual y knockback con control bloqueado temporalmente. El emisor suministra posición/damage mediante `EnemyAttackInstance`; no se introduce un segundo sistema de recepción.
- Al llegar a cero, el jugador queda bloqueado, se detienen sus componentes de combate/pogo/parry, el cuerpo queda inmóvil y la HUD muestra estado de derrota. `R` reinicia vida, posición de `Player Spawn Point`, velocidad, gravedad y estados temporales; dash/doble salto siguen disponibles según las opciones actuales de `PlayerAbilities`.
- `PlayerHealthUI` usa IMGUI provisional para mostrar bloques de vida y el aviso de muerte, sin recursos gráficos adicionales.
- `MovementTest` se configuró con `PlayerHealth`, `PlayerHealthUI` y el asset de vida. No se modificaron movimiento, dash, salto, cámara, GDD ni estructura de carpetas.

### Parámetros iniciales

- Vida máxima: `5`.
- Invulnerabilidad tras daño: `0.8 s`; parpadeo visual: `0.08 s`.
- Knockback horizontal: `7`; vertical: `4.5`; bloqueo de control: `0.28 s`.
- Daño del ataque de entrenamiento: `1` por ataque activo.

### Verificación

- Unity `6000.5.5f1` compiló el núcleo de salud y ejecutó `CombatTestSceneSetup.Configure`; Tundra terminó con código `0`.
- Se verificaron las referencias serializadas de salud, Input Actions, spawn y HUD; la escena conserva `PlayerImpactReceiver`, parry/postura, enemigos de entrenamiento, objetivos de pogo y el material sin fricción.
- El Input System contiene una única acción `Restart` ligada a R. El receptor comprueba dash e invulnerabilidad antes de consumir la ventana de parry.
- No se ejecutó Play Mode; daño, i-frames, knockback, muerte y reinicio requieren prueba manual.

### Pruebas manuales pendientes

- Recibir un ataque sin parry: vida baja de `5` a `4`, aparece knockback/parpadeo y los golpes de los siguientes `0.8 s` se ignoran.
- Parryar otro ataque y comprobar vida sin cambio ni knockback; fallar el parry y comprobar daño normal.
- Activar dash durante la hitbox: no debe recibir daño ni registrar parry ni postura.
- Recibir daño hasta cero: comprobar bloqueo, aviso de derrota y HUD; pulsar R y verificar posición, vida, gravedad, velocidades, controles e invulnerabilidad restaurados.
- Tras reiniciar, probar ataque horizontal/ascendente, pogo y doble salto para comprobar que los componentes se reactivan.
- Revisar la consola de Unity durante las pruebas.

## Tarea 09: Enemigo terrestre (continuación del trabajo interrumpido)

### Auditoría inicial

- Se leyeron `AGENTS.md`, `GDD.md`, este registro y `Region1Plan.md`. El último commit era `891b90d` (Tarea 08); se respetó su condición de completada y probada indicada por el usuario.
- Git solo mostraba dos archivos modificados (`EnemyPosture.cs`, `TrainingAttackEmitter.cs`) y el nuevo `EnemyAttackHitbox.cs` con su `.meta`. No había controlador terrestre, datos propios, vida de enemigo, prefab ni integración de Tarea 09 en la escena.
- La extracción de la hitbox era una base válida, pero la migración estaba incompleta: el emisor requería una referencia todavía ausente en MovementTest; el configurador seguía buscando el campo retirado `posture`. Compilar esos scripts no garantizaba que el entrenador pudiera ejecutarse.
- La hitbox calculaba `SourcePosition` a partir de los bounds de un collider desactivado, que no ofrecen una geometría válida. Además, excluir todo `transform.root` podía excluir al jugador si ambos estaban bajo un grupo común. Resolver el golpe también adelantaba el cooldown del entrenador, alterando sus ciclos.
- El indicador visual opcional de `EnemyPosture` era válido y se conservó, con el renderer original como alternativa para el entrenador.

### Archivos y comportamiento

- `Assets/_Project/Scripts/Combat/EnemyAttackHitbox.cs`: se completó la extracción compartida. Usa la forma de un `BoxCollider2D`, calcula centro/tamaño mediante transforms y consulta física durante el ataque. El collider permanece desactivado: no añade una zona donde el enemigo pueda recibir golpes a distancia ni produce contactos adicionales. Cada ataque usa un único `EnemyAttackInstance`; comunica el resultado y aplica postura solo en `Parried`. Filtra el propietario, no todo el grupo de escena, y cancela el ataque al desactivarse.
- `Assets/_Project/Scripts/Combat/TrainingAttackEmitter.cs`: usa la hitbox común y conserva las fases/tiempos originales. Un impacto resuelto no acorta su fase activa. Al desactivarse limpia hitbox y aviso.
- `Assets/_Project/Scripts/Combat/EnemyPosture.cs`: se conservó el cambio previo del indicador opcional, sin reescribir las reglas de postura o recuperación.
- `Assets/_Project/Scripts/Enemies/GroundEnemyData.cs` y `Data/Enemies/GroundEnemyData.asset`: parámetros de vida, velocidades, detección, ataque, multiplicador vulnerable y feedback. Los estados temporales quedan en componentes, no en el asset.
- `Assets/_Project/Scripts/Enemies/EnemyHealth.cs`: implementa `IDamageable`, flash de daño y vida independiente de la postura. Durante vulnerabilidad recibe daño multiplicado. A cero cancela ataques, desactiva colliders/postura/simulación física y emite `Died` una sola vez. El cuerpo queda visible con color de muerte.
- `Assets/_Project/Scripts/Enemies/GroundEnemyController.cs`: estados `Patrol`, `Chase`, `Telegraph`, `Attack`, `Recovery`, `Vulnerable` y `Dead`. Detecta por proximidad horizontal y tolerancia vertical; pierde al jugador por distancia, altura, desactivación o muerte. Fija orientación al comenzar el aviso. La ruptura de postura cancela el ataque y detiene la IA; al terminar la vulnerabilidad pasa por recuperación y retoma patrulla/persecución. Los límites horizontales restringen también la persecución; detecta suelo y obstáculos para no caminar fuera de una plataforma.
- `Assets/_Project/Prefabs/Enemies/GroundEnemy.prefab`: prefab reutilizable con cuerpo, vida, postura, controlador, indicador de orientación, indicador de postura y zona de aviso/ataque. Usa el sprite provisional y el material sin fricción existentes. En otras escenas puede enlazarse `target`; si está vacío busca un `PlayerHealth` al iniciar.
- `Assets/_Project/Scripts/Editor/CombatTestSceneSetup.cs`: se actualizó únicamente la creación/verificación del entrenador para enlazar la hitbox compartida. El aviso ahora coincide con la zona física original en unidades de mundo, compensando la escala del entrenador.
- `Assets/_Project/Scripts/Editor/GroundEnemySceneSetup.cs`: menú `Fangbound/Configure Movement Test Ground Enemy`. Migra el entrenador, crea los assets nuevos si faltan e incorpora una instancia del prefab sin ejecutar el configurador general del jugador. Comprueba contratos, prefab, objetivos anteriores y ausencia de scripts faltantes.
- `Assets/_Project/Scripts/Editor/GroundEnemyValidation.cs`: menú `Fangbound/Validate Ground Enemy in Play Mode`; regresión automatizada con la escena real. Se ejecuta desde Edit Mode, abre MovementTest y termina saliendo de Play Mode; en batch cierra el Editor con código de resultado. Las modificaciones de prueba solo ocurren en Play Mode y no se guardan en la escena.
- `Assets/_Project/Scenes/Test/MovementTest.unity`: entrenador migrado y enemigo morado en el suelo derecho, inicialmente `(4.5, -1.58)`. Patrulla aproximadamente entre `x=1.4` y `x=6.8`. Conserva jugador, spawn, cámara, geometría, HUD, dos blancos de combate y dos objetivos de pogo.
- Se añadieron los `.meta` de los scripts, carpeta `Enemies`, datos y prefab nuevos. Este registro documenta la entrega. No se modificaron scripts/datos/Input Actions del jugador, paquetes, GDD ni el plan de región. No se realizaron commits ni push.

### Parámetros iniciales

| Parámetro | Valor |
| --- | --- |
| Vida del enemigo | 4 |
| Daño recibido durante vulnerabilidad | x2 |
| Velocidad de patrulla / persecución | 1.3 / 2.6 unidades/s |
| Detección / pérdida horizontal | 3.5 / 5.5 unidades |
| Tolerancia vertical | 1.2 unidades |
| Alcance para iniciar ataque | 1.3 unidades |
| Daño / aviso / activo / recuperación | 1 / 0.65 s / 0.14 s / 1.3 s |
| Offset / tamaño de ataque | (0.85, 0) / (1.2, 0.8), reflejados según orientación |
| Postura compartida | 3 puntos; 1 por parry; vulnerable 1.5 s; recupera 1 punto/s |

### Migración del entrenador

- Se conserva la zona física anterior: centro `(-5.6, -1.4)` en MovementTest, tamaño `(3.6, 0.75)`, offset `(-1.8, -0.05)` respecto al entrenador. El aviso anterior se veía más pequeño por la escala `0.7` del padre; se corrigió su representación para mostrar la zona que ya dañaba.
- Se conservan espera inicial `0.75 s`, aviso `0.65 s`, fase activa completa `0.14 s`, cooldown `1.3 s`, daño `1` y la resolución única por ataque.
- La recepción sigue pasando por `PlayerImpactReceiver`: muerte, dash e i-frames mantienen prioridad sobre parry/daño. No hay segundo receptor ni cambios al sistema del jugador.

### Validación realizada

- Unity `6000.5.5f1` compiló los ensamblados de juego y Editor. El configurador generó el prefab y guardó MovementTest mediante la API de Unity, con `TASK09_SETUP_PASS` y salida `0` (`Logs/Task09Setup.log`).
- La prueba automatizada en Play Mode terminó con `TASK09_PLAYMODE_PASS` (`Logs/Task09PlayMode.log`): patrulla en ambos sentidos/límites, detección y persecución, lado fijo del aviso, daño único, retorno al perder al jugador, regresión del entrenador, tres parries del enemigo, vulnerabilidad x2, recuperación de postura, muerte durante aviso, evento único de muerte, colliders cancelados y cuerpo detenido. También se probó una plataforma aislada con bordes y una pared.
- La ejecución final está en `Logs/Task09Validation.log`, con `TASK09_PLAYMODE_PASS`, sin errores/avisos C# ni excepciones. Amplía la regresión a tres parries del propio entrenador, color vulnerable del renderer original y prioridad de i-frames frente a una ventana de parry abierta. Una pasada intermedia no observó ambos sentidos al medir el transform interpolado; se cambió la medición a `Rigidbody2D.position` y la ejecución completa posterior pasó. No se alteró la lógica de patrulla para obtener ese resultado.
- `git diff --check` pasó. La comparación con HEAD de los objetos/componentes serializados del jugador confirmó que no cambiaron; tampoco hay diferencias en sus scripts, datos, Input Actions ni ajustes de proyecto.
- La regresión usa física y componentes reales. Para aislar los casos inmoviliza/reposiciona al jugador, abre ventanas de parry mediante reflexión y llama a la consulta existente de `PlayerCombat`. La patrulla se mide con la posición física de `Rigidbody2D`, evitando depender del transform visual interpolado en modo sin gráficos. No reproduce pulsaciones humanas ni certifica el ajuste visual o la sensación de combate.
- El primer lanzamiento dentro del sandbox falló por conexión IPC con Package Manager; se repitió fuera del sandbox. No fue un error C# del proyecto. Se actualizaron las búsquedas de objetos de los archivos nuevos a las APIs vigentes para evitar los avisos de obsolescencia de Unity 6.5.

### Cómo probar manualmente en Unity

1. Abrir `Assets/_Project/Scenes/Test/MovementTest.unity` con Unity `6000.5.5f1`, limpiar la consola y entrar en Play Mode. La escena ya está configurada; no hace falta regenerarla. Seleccionar `Environment/Ground Enemy` y activar Gizmos en Scene View para ver límites y zona de detección. Los valores de vida/postura/estado son propiedades públicas comprobadas por la validación automática; no hay HUD de enemigo en esta entrega.
2. Sin acercarse, observar al enemigo morado en el suelo derecho: debe recorrer ambos sentidos, girar en sus límites y permanecer sobre el suelo. Acercarse usando el movimiento/dash/doble salto existentes: debe perseguir; alejarse más de `5.5` unidades o salir de su tolerancia vertical: debe volver a patrullar.
3. Entrar en su alcance. Observar el aviso rojizo de `0.65 s` y el ataque amarillo de `0.14 s`. Cambiar de lado durante el aviso: la zona no debe girar para seguir al jugador. Sin parry/dash, el impacto debe quitar una vida al jugador, con el knockback y feedback existentes, una sola vez por ataque.
4. Pulsar K o el hombro izquierdo justo antes del impacto. Cada parry correcto debe conservar la vida del jugador y reducir postura una vez (`3 → 2 → 1 → 0`). Al romperla, la franja superior debe mostrarse turquesa y el enemigo debe dejar de moverse/atacar durante `1.5 s`.
5. Golpear con J antes de romper postura: debe restar `1` de vida. Golpear durante la vulnerabilidad: debe restar `2`. Si se deja vivir, observar el fin de la vulnerabilidad, recuperación progresiva de postura y reanudación del comportamiento. La vida no se regenera por recuperar postura.
6. Agotar su vida con ataques: debe quedar gris/transparente, sin colisiones, movimiento, avisos ni nuevos impactos. Probar un golpe letal durante el aviso para comprobar su cancelación. Salir y volver a entrar en Play Mode para restaurar el enemigo; R conserva el reinicio existente del jugador y no resucita enemigos.
7. Repetir con el entrenador a la izquierda: comprobar el aviso ampliado hasta la zona física real, ciclos anteriores, daño `1`, dash sin daño ni postura y tres parries hasta vulnerabilidad con su color original. Probar también ataque horizontal/ascendente, pogo, salto, doble salto, dash, muerte/R y HUD como regresión manual; revisar la consola.
8. Para repetir la validación automática, salir de Play Mode y usar `Fangbound/Validate Ground Enemy in Play Mode`. La consola debe terminar con `TASK09_PLAYMODE_PASS`. Si se necesitan reconstruir los assets de Tarea 09, usar su menú de configuración específico; evitar el generador completo de MovementTest porque reconstruye la escena base.

### Limitaciones y pendientes

- Enemigo de prueba con sprites/colores provisionales, sin animaciones finales, audio, navegación compleja, saltos ni rutas entre plataformas. La detección es por proximidad, sin oclusión visual por paredes. Los límites pertenecen a cada instancia y los parámetros generales al asset.
- Solo usa ataques parryables y la reducción de postura por parry del contrato existente. No incorpora ataques rojos no parryables, ejecución, daño de contacto ni daño de pogo: esos sistemas no forman parte de esta entrega.
- Pendientes la revisión humana de visibilidad del aviso, sensación de combate y regresión completa con teclado/mando. No se generó ni ejecutó una build standalone.
- **Se conserva pendiente la verificación manual específica de los `0.8 s` de i-frames de `PlayerHealth`.** Se verificó que un impacto inmediato durante invulnerabilidad se ignora; no se midió la frontera temporal de `0.8 s`. Para cerrarla, usar ataques independientes antes y después de ese límite y confirmar vida/parry/feedback. No se modificó `PlayerHealth` ni su asset.

## Tarea 09.1: Corrección de empuje físico y recuperación de patrulla

### Estado y causa identificada

- El usuario confirmó la prueba manual satisfactoria de la Tarea 09: patrulla, detección, persecución, ataques, parry, postura, vulnerabilidad, vida y muerte. Esta corrección conserva esos sistemas y no inicia la Tarea 10.
- Jugador y enemigo tenían `Rigidbody2D` dinámicos, masa `1`, rotación congelada y colliders sólidos en `Default`. La matriz de colisiones 2D permitía todos los contactos. Asignar `linearVelocity.x` desde la IA no impide que el solver de contactos aplique impulsos/desplace el enemigo durante el paso físico posterior; el material sin fricción tampoco evita el impulso normal del contacto.
- `TryMove` rechazaba cualquier paso que terminase fuera de los límites de patrulla, incluso si acercaba al enemigo al intervalo válido. Tras ser empujado suficientemente lejos, ambos sentidos quedaban bloqueados y Patrol invertía orientación sin poder avanzar.
- En Chase, un movimiento bloqueado simplemente dejaba velocidad horizontal a cero y volvía a intentarse. No había una retirada temporal ante paredes/bordes/límites. Telegraph, Attack y Recovery podían seguir funcionando, explicando que un enemigo inmóvil continuase atacando.

### Solución y archivos de esta corrección

- `Assets/_Project/Scripts/Enemies/GroundEnemyController.cs`: excluye con `Physics2D.IgnoreCollision` únicamente los pares entre su collider corporal y los colliders sólidos del jugador objetivo. Se aplica al iniciar y al reactivar el componente. Ambos cuerpos siguen siendo dinámicos y sólidos contra el terreno; no se cambian masas, gravedad ni restricciones físicas. Los cuerpos del jugador/enemigo ahora pueden atravesarse, conservando las consultas independientes de ataques y daño.
- `ProjectSettings/TagManager.asset`: añade `EnemyBody` en el índice libre `8`. No modifica las capas existentes ni la Layer Collision Matrix. Esta separación es necesaria porque las consultas de suelo de `PlayerController` usan `OverlapBox` con máscara `Default`: ignorar contactos por pareja no impediría por sí solo que el enemigo fuese detectado como suelo al atravesarlo.
- `Assets/_Project/Prefabs/Enemies/GroundEnemy.prefab`: solo cambia la capa del objeto raíz que contiene el collider sólido a `EnemyBody`. Sus hijos visuales/hitbox y todas las referencias/parámetros se conservan. MovementTest hereda el cambio del prefab; no necesita regenerarse.
- `Assets/_Project/Scripts/Editor/GroundEnemySceneSetup.cs`: los prefabs nuevos usan `EnemyBody`; la verificación exige cuerpo sólido en esa capa y comprueba que no esté incluida en el suelo del jugador.
- La patrulla prioriza regresar al intervalo si el cuerpo está fuera, orientándose hacia él. `TryMove` permite pasos que se acerquen desde fuera y mantiene las comprobaciones de paredes y suelo. No teletransporta ni atraviesa paredes/bordes para volver.
- Una persecución bloqueada vuelve a Patrol, orientada para retirarse. Durante el tiempo de recuperación ya configurado (`1.3 s` inicialmente) pospone la nueva detección, evitando la alternancia inmediata Patrol/Chase frente al mismo obstáculo. Después puede volver a intentar perseguir. No se añaden estados de navegación ni pathfinding.
- Se conservan orientación y tiempos de los ataques comprometidos. Si el desplazamiento ocurre durante Telegraph/Attack/Recovery, completa el ciclo y después recupera su posición mediante Patrol. La vulnerabilidad mantiene su parada temporal y el daño recibido x2; no hay instakill ni ejecuciones del enemigo básico. Las ejecuciones que eliminan una barra completa se reservan para futuros jefes según la decisión del usuario.
- `Assets/_Project/Scripts/Editor/GroundEnemyValidation.cs`: amplía la regresión existente con acciones reales del Input System de caminar y dash en ambos sentidos, y desplazamientos forzados independientes de la prevención del empuje. La prueba dirige temporalmente la entrada al juego aun sin foco de ventana y restaura esas opciones en `finally`; no guarda cambios en Input Settings.
- `Docs/DevelopmentLog.md`: documenta esta corrección. No se modifican scripts/datos del jugador, Input Actions, salud/postura/daño de enemigo, parámetros de dash ni sistemas del entrenador. Se preservan todos los cambios locales de Tarea 09 anteriores a esta petición. No hay commits ni push.

### Validación automatizada

- Unity `6000.5.5f1` compiló y ejecutó la batería completa en el proyecto principal. `Logs/Task09CollisionValidation.log` terminó con `TASK09_COLLISION_PASS`, `TASK09_RECOVERY_PASS` y `TASK09_PLAYMODE_PASS`, sin errores C#, avisos C# ni excepciones en esa ejecución. `git diff --check` pasó.
- La regresión de colisiones comprueba contacto excluido solo entre cuerpos; terreno conservado; caminar/dash mediante A/D y Shift con el controlador real, en ambos sentidos; desplazamiento horizontal del enemigo menor de `0.03` unidades; cruce efectivo del jugador y velocidad de dash conservada; ausencia de daño de contacto; enemigo excluido del suelo consultado por el jugador; y exclusión de contacto restaurada al reactivar el enemigo.
- La regresión de recuperación fuerza al enemigo fuera de cada límite y comprueba que regrese, coloca el límite detrás de una pared para verificar una persecución realmente bloqueada por terreno y observa la retirada en Patrol. También fuerza el desplazamiento durante Telegraph y comprueba Attack → Recovery → Patrol y regreso al intervalo.
- Se conserva la batería anterior: patrulla normal, detección/persecución/ataque, pérdida del jugador, daño único, tres parries, aturdimiento y vulnerabilidad x2, recuperación de postura, muerte, bordes, paredes y regresión del entrenador con sus colores y resultados de impacto.
- El primer intento con teclado sintético no produjo el cruce esperado en modo sin ventana. Se ajustó únicamente el foco de entrada de la prueba, de forma temporal y reversible. No se alteró el movimiento para conseguir el resultado.
- Se preparó una copia ignorada por Git bajo `.utmp/Task09CollisionValidation` mientras el Editor principal estaba abierto. Su primer arranque registró una excepción interna del índice de búsqueda de Unity. Una vez cerrado el Editor principal, la validación de la corrección se ejecutó directamente en el proyecto principal; no se cerró ni terminó desde herramientas la sesión interactiva del usuario.

### Pruebas manuales recomendadas

1. Abrir MovementTest, comprobar que el objeto raíz del prefab GroundEnemy usa `EnemyBody` y entrar en Play Mode. Caminar y hacer dash a través del enemigo desde izquierda y derecha. Debe poder atravesarse sin empujarlo; el enemigo sigue sujeto a su IA y ambos cuerpos conservan el suelo.
2. Repetir cuando el enemigo esté avisando un ataque, recuperándose o vulnerable: el contacto corporal no debe desplazarlo. Saltar cerca y encima de él; su cuerpo atravesable no debe habilitar un falso salto desde el aire ni actuar como plataforma.
3. Alejarse para que patrulle. Durante Play Mode, mover temporalmente el cuerpo a `x=7.35`, `y≈-1.6` (entre límite derecho y pared), y luego a `x=0.95`, `y≈-1.6` (fuera del izquierdo pero todavía sobre suelo). Con el jugador lejos debe regresar a sus límites y continuar, sin teletransportarse. No guardar esos movimientos en Edit Mode.
4. Probar una pared o borde delante de una persecución: debe detener el avance peligroso y retirarse brevemente patrullando, con posibilidad de reintentar si el jugador sigue cerca. No puede alcanzar una zona separada por paredes/saltos que requieran navegación; no debe atravesar obstáculos para hacerlo.
5. Volver a acercarse para comprobar persecución, aviso y ataque. Acertar tres parries: postura `3 → 0`, aturdimiento de `1.5 s`, daño x2 durante vulnerabilidad y posterior recuperación. Agotar vida con ataques para comprobar muerte/cancelación; romper postura por sí solo no mata al enemigo.
6. Repetir el entrenador anterior y la regresión del jugador: daño, parry, dash invulnerable, salto/doble salto, ataque horizontal/ascendente, pogo, muerte/R y HUD. R continúa reiniciando solo al jugador. Revisar la consola.
7. Para repetir la batería automática, usar desde Edit Mode `Fangbound/Validate Ground Enemy in Play Mode`; debe terminar con `TASK09_COLLISION_PASS`, `TASK09_RECOVERY_PASS` y `TASK09_PLAYMODE_PASS`.

### Pendiente conservado

- La verificación manual de la frontera exacta de `0.8 s` de i-frames de `PlayerHealth` sigue abierta. La corrección no cambia su código ni sus datos. La regresión de impactos durante invulnerabilidad no sustituye esa medición temporal.

## Tarea 09.2: Integración del pogo con el enemigo terrestre

### Causa y alcance

- El usuario confirmó que la Tarea 09 y la corrección de colisiones/patrulla funcionan en sus pruebas manuales. El problema pendiente era que el ataque descendente atravesaba al enemigo sin rebotar.
- `PlayerPogo` ya detectaba su ventana descendente mediante `OverlapBox`, pero solo aceptaba colliders con `PogoTarget` en su jerarquía. El prefab terrestre tenía `EnemyHealth`/`IDamageable`, pero no esa marca. Además, el pogo de Tarea 05 solo producía rebote: todavía no enviaba daño a `IDamageable`.
- Las colisiones corporales sin empuje y la capa `EnemyBody` no causan el fallo: el pogo usa una consulta independiente con `ContactFilter2D.noFilter`. No es necesario restaurar contactos físicos para que el ataque acierte.

### Archivos de esta entrega

- `Assets/_Project/Scripts/Player/PlayerPogo.cs`: conserva el sistema existente de entrada, ventana, consulta, disponibilidad y rebote. Tras encontrar `PogoTarget`, busca opcionalmente `IDamageable`, descarta los receptores que ya no pueden recibir daño y resuelve un único impacto. Usa el mismo `BounceFromPogo` y luego aplica `TakeDamage`; el rebote ya queda resuelto aunque el daño desactive o destruya al enemigo. Los blancos sin `IDamageable` conservan su rebote sin daño.
- `Assets/_Project/Scripts/Player/PlayerPogoData.cs` y `Assets/_Project/Data/Player/PlayerPogoData.asset`: añaden `Damage`, inicialmente `1`, como parámetro de diseño. Conservan `BounceSpeed=13`, ventana `0.2 s`, offset `(0,-0.18)` y tamaño `(0.65,0.35)`.
- `Assets/_Project/Prefabs/Enemies/GroundEnemy.prefab`: añade el componente existente `PogoTarget` al objeto raíz. No cambia colliders, capas, Rigidbody2D, referencias ni parámetros anteriores. MovementTest hereda la integración del prefab sin modificar la escena.
- `Assets/_Project/Scripts/Editor/GroundEnemySceneSetup.cs`: añade la marca al crear futuros prefabs y verifica que el enemigo la tenga. La comprobación de los blancos antiguos usa sus dos nombres explícitos en lugar de exigir que solo existan dos `PogoTarget` en toda la escena.
- `Assets/_Project/Scripts/Editor/GroundEnemyValidation.cs`: incorpora la nueva regresión a la batería de Play Mode existente, después de las pruebas de colisiones/recuperación.
- `Assets/_Project/Scripts/Editor/GroundEnemyPogoValidation.cs` y su `.meta`: pruebas de integración con un teclado sintético del Input System y los componentes/consultas reales. Los fixtures se crean y eliminan únicamente en Play Mode. Las opciones temporales de foco de entrada se restauran al terminar.
- `Docs/DevelopmentLog.md`: registro de la causa, solución y pruebas. Se preservan todos los cambios previos sin commit. No hay commits ni push y no se implementa la Tarea 10.

### Comportamiento conservado

- La salamandra cae por gravedad natural. No se modifica `PlayerController`, no se añade velocidad descendente ni se cambia la fuerza de rebote, dash, doble salto, parry o cámara.
- El ataque descendente acertado causa `1` de daño inicial. `EnemyHealth` conserva su multiplicador x2 durante vulnerabilidad, por lo que ese mismo golpe causa `2` en dicho estado. Romper postura sigue produciendo aturdimiento/vulnerabilidad temporal, sin ejecuciones ni instakill.
- La ventana se cierra tras el primer impacto válido y recupera la disponibilidad del pogo como antes. Varios colliders del mismo enemigo no multiplican daño ni rebote. Los golpes fallidos no rebotan ni restituyen el uso antes de aterrizar/acertar, y los ataques horizontal/ascendente no activan la ventana descendente.
- Los dos objetivos dorados anteriores siguen aceptando el pogo. Se mantienen `EnemyBody`, exclusión de contacto entre cuerpos, colisiones con suelo y corrección de patrulla fuera de límites.

### Validación y preparación de pruebas

- Unity `6000.5.5f1` compiló y ejecutó la batería completa. La ejecución final de `Logs/Task09PogoValidation.log` terminó con `TASK09_POGO_PASS`, `TASK09_COLLISION_PASS`, `TASK09_RECOVERY_PASS` y `TASK09_PLAYMODE_PASS`, sin errores C#, avisos C# ni excepciones. Los tres impactos verificaron vida `4 → 3`, `4 → 2` vulnerable y `1 → 0` letal, conservando el rebote en todos los casos. `git diff --check` pasó.
- Se compararon por hash 12 archivos protegidos con su estado al iniciar esta integración: movimiento, salud, combate y parry del jugador; controlador/vida del enemigo; datos de movimiento/salud; Input Actions; ajustes físicos/capas y MovementTest. Todos se conservaron. La única modificación de datos del jugador corresponde al nuevo daño de pogo; sus parámetros de rebote y hitbox permanecen iguales.
- La regresión nueva usa S + J para pogo, D + J para ataque horizontal y W + J para ascendente mediante Input System. Comprueba daño y velocidad vertical reales, sin llamar directamente al rebote ni a la recepción de daño del jugador para simular un éxito.
- Cubre enemigo vivo, vulnerable y golpe letal normal; impacto único con varios colliders; fallo sin daño/rebote ni aceleración añadida; horizontal/ascendente sin rebote; y ambos blancos dorados. La vulnerabilidad y la vida previa al golpe letal se preparan mediante los contratos existentes, para aislar cada caso; la batería anterior comprueba también los parries y la muerte por combate.
- Se ejecuta la batería completa, incluidas las regresiones de colisiones sin empuje, recuperación de patrulla, combate, parry, postura, muerte y entrenador. Al comenzar, el Editor del usuario estaba abierto; por ello se utilizó una copia sincronizada del proyecto bajo `.utmp/Task09CollisionValidation`, ignorada por Git. No se controló ni cerró la sesión interactiva.
- En el primer intento se detectó una espera insuficiente en la preparación del caso fallido: tras reposicionar al personaje, la prueba enviaba el ataque antes de actualizar su estado de suelo. Se añadió esa espera solo en el fixture. La copia también carecía de `UserSettings/Search.index`, provocando una excepción interna del buscador de Unity; se copiaron sus archivos de configuración existentes sin alterar el proyecto principal ni suprimir errores.
- La preparación de los ataques normal/ascendente espera también a que termine `PlayerCombatData.AttackWindow` entre pulsaciones. No se redujo ni modificó la ventana del combate para encadenar artificialmente las pruebas.

### Pruebas manuales recomendadas

1. Abrir MovementTest y comprobar que el objeto raíz de GroundEnemy tiene `PogoTarget`, conserva su collider sólido en `EnemyBody` y mantiene sus datos anteriores. Entrar en Play Mode; no hace falta regenerar la escena.
2. Saltar sobre el enemigo vivo y pulsar abajo + J durante la caída. Debe rebotar hacia arriba y aplicar un punto de daño por golpe, usando la misma fuerza de los blancos dorados. Dejar caer al personaje sin atacar para comprobar que el contacto corporal por sí solo no produce rebote.
3. Acertar tres parries para romper postura y repetir el pogo durante el aturdimiento: debe rebotar y causar el daño vulnerable x2. Si el golpe agota la vida, debe completar la muerte y conservar el rebote.
4. En otra sesión de Play Mode, dejar al enemigo con una vida mediante tres ataques normales y terminar con pogo: debe morir mientras la salamandra sale hacia arriba. R sigue reiniciando solo al jugador; para restaurar al enemigo, salir y volver a entrar en Play Mode.
5. Usar abajo + J en el aire lejos de objetivos: caída natural, sin rebote, impulso descendente añadido ni daño remoto. Repetir ataques horizontal y ascendente contra el enemigo: deben causar su daño habitual sin rebote.
6. Repetir pogo sobre los dos blancos dorados. Comprobar también salto/doble salto, dash a través del enemigo sin empujarlo, patrulla/persecución, parry, vulnerabilidad, muerte y entrenador anterior; revisar la consola.
7. Desde Edit Mode se puede repetir la batería con `Fangbound/Validate Ground Enemy in Play Mode`. Los casos de pogo deben registrar `TASK09_POGO_CASE_PASS` y `TASK09_POGO_PASS`, además de las marcas de colisiones/recuperación y el resultado final general.

### Pendiente conservado

- Continúa pendiente la verificación manual específica de los `0.8 s` de i-frames de `PlayerHealth`. Esta integración no cambia vida, invulnerabilidad ni knockback del jugador.

## Tarea 10: Feedback de combate provisional

### Auditoría y puntos de confirmación

- Se leyeron `AGENTS.md`, `GDD.md`, este registro y `Region1Plan.md`, y se inspeccionaron combate, pogo, salud del jugador/enemigo, postura, parry, hitbox compartida y entrenador. El árbol de trabajo estaba limpio y HEAD era `aa44d6b` (Tarea 09 con pogo y correcciones); el usuario confirmó las tareas 01–09 probadas manualmente y con commit.
- No existía hitstop ni otro propietario de `Time.timeScale`. Sí existían destellos de `EnemyHealth`/`TrainingTarget`, feedback de éxito/fallo de `PlayerParry` y color vulnerable/de recuperación de `EnemyPosture`. Se reutilizaron sin reemplazar el combate.
- Un impacto normal se confirma dentro de `PlayerCombat.ApplyHitbox`, tras validar `IDamageable.CanReceiveDamage` y aplicar `TakeDamage`; su conjunto de objetivos ya evita daño repetido por varios colliders. El pogo confirma su impacto en `PlayerPogo`, después de reconocer `PogoTarget`, rebotar y aplicar daño. El éxito de parry se confirma exclusivamente en `PlayerParry.TryConsumeWindow`; `PlayerImpactReceiver` conserva su orden muerte → dash → i-frames → parry → daño. La ruptura se confirma cuando `EnemyPosture` pasa a cero.

### Arquitectura y archivos creados

- `Assets/_Project/Scripts/Combat/CombatFeedbackData.cs`: ScriptableObject con opciones de hitstop y feedback visual. `Assets/_Project/Data/Combat/CombatFeedbackData.asset` contiene los valores iniciales; no almacena estados de ejecución.
- `Assets/_Project/Scripts/Combat/CombatHitstop.cs`: servicio único por escena, con solicitudes normal/pogo/parry, reloj no escalado, combinación acotada de solicitudes y restauración del tiempo. Expone `IsActive`, `PauseCount` y `Cancel` para diagnóstico/validación.
- `Assets/_Project/Scripts/Combat/CombatVisualFeedback.cs`: coordinación pequeña de apariencia y señales. En enemigos gestiona el color del cuerpo y del indicador de postura; en el jugador gestiona únicamente un sprite hijo de éxito, manteniendo la propiedad del cuerpo en los sistemas anteriores. Se suscribe a daño confirmado, muerte, parry correcto y ruptura. Sus contadores sirven para comprobar efectos únicos.
- `Assets/_Project/Scripts/Editor/CombatFeedbackSceneSetup.cs`: menú `Fangbound/Configure Movement Test Combat Feedback`. Crea datos si faltan, actualiza el prefab mediante la API del Editor y enlaza jugador, entrenador y servicio en MovementTest, sin ejecutar los configuradores generales de movimiento/combate.
- `Assets/_Project/Scripts/Editor/CombatFeedbackValidation.cs`: menú `Fangbound/Validate Combat Feedback in Play Mode`, que ejecuta la batería anterior ampliada con regresiones específicas de feedback e interrupciones.
- Unity generó los `.meta` de scripts, datos y carpeta nueva. No se añadieron materiales, shaders, recursos externos, paquetes ni dependencias de pago; las señales reutilizan el sprite cuadrado y material existentes.

### Archivos existentes modificados

- `PlayerCombat.cs`: solicita un hitstop normal por ataque confirmado; un indicador temporal por ventana impide una segunda solicitud dentro del mismo ataque, además del filtrado de colliders existente.
- `PlayerPogo.cs`: solicita hitstop de pogo solo si el impacto válido tiene `IDamageable`. Mantiene rebote previo al daño, fuerza `13`, daño actual y caída natural; los blancos dorados sin daño no activan pausa.
- `PlayerParry.cs`: emite `Succeeded` y solicita hitstop tras consumir correctamente la ventana. Conserva lectura de inputs, colores anteriores, ventana `0.15 s` y recuperación `0.4 s`.
- `EnemyHealth.cs`: emite `DamageTaken` tras descontar vida y expone su color de muerte. Delega el color al coordinador cuando está instalado/activo, y conserva el destello anterior como alternativa si no lo está. Daño, multiplicador vulnerable, colliders, Rigidbody2D y muerte mantienen sus reglas.
- `EnemyPosture.cs`: emite `Broken` en la transición a cero y expone renderer/color derivados del estado existente. Delega únicamente la escritura visual cuando hay coordinador; conserva su alternativa anterior. No se modificaron la pérdida de postura, los `1.5 s` de vulnerabilidad ni la recuperación.
- `GroundEnemyValidation.cs` y `GroundEnemyPogoValidation.cs`: incorporan la batería nueva y verifican hitstop único en pogo vivo, vulnerable y letal, y ausencia de pausa al fallar.
- `GroundEnemy.prefab`: añade coordinador y señal visual hija. `MovementTest.unity`: añade esas referencias al jugador/entrenador y el objeto `Combat Hitstop`; la instancia terrestre hereda el feedback del prefab. Se conservaron los IDs/referencias de los componentes existentes.
- `Docs/DevelopmentLog.md`: documenta esta tarea. No se modificaron `PlayerController`, `PlayerHealth`, cámara, datos de movimiento/combate/parry/pogo/postura/vida, Input Actions, IA terrestre, hitbox compartida, física/capas, paquetes, GDD ni plan de región. No se realizaron commits, push ni cambios de rama.

### Hitstop y restauración

- Se usa `Time.timeScale=0` durante una micro-pausa porque permite detener juntos movimiento, gravedad, hitboxes y temporizadores de combate, sin alternar `Rigidbody2D.simulated`, cambiar colliders, reescribir motores ni guardar/restaurar velocidades. El rebote del pogo queda almacenado antes de la pausa y se conserva al reanudar.
- El servicio guarda la escala previa, utiliza `Time.realtimeSinceStartupAsDouble` para el vencimiento y no cambia `Time.fixedDeltaTime`. No depende de una coroutine escalada que pudiera quedar detenida con su propia pausa.
- Las solicitudes simultáneas comparten una única pausa: puede prevalecer la duración mayor, calculada desde el comienzo original, pero nunca se suman duraciones ni se crea una cola. Las duraciones se limitan a un máximo configurable en Inspector de `0.2 s`.
- Restaura al vencer, al desactivar/destruir el servicio, al perder foco o al recibir pausa de aplicación. También limpia la referencia estática al iniciar Play Mode sin recarga de dominio. No inicia hitstop si otro sistema ya tiene la escala a cero; si detecta un cambio externo a una escala distinta de cero, lo respeta.
- Los `Update` de entrada siguen ejecutándose. Los controles existentes almacenan salto/dash/pogo y las ventanas de ataque/parry permanecen en tiempo de simulación. Se verificaron pulsaciones breves de salto/dash durante la pausa, liberadas antes de reanudar.
- Todos los temporizadores escalados se detienen coherentemente con el mundo, incluidos parry, recuperación, vulnerabilidad e i-frames. No se cambiaron sus valores ni el código de salud. Una pausa añade tiempo real de presentación: `0.8 s` de simulación no equivale a `0.8 s` de reloj si interviene hitstop. La medición manual exacta pendiente de PlayerHealth debe hacerse también con hitstop desactivado para aislar el sistema original.
- Se desactiva globalmente desmarcando `Hitstop Enabled` en el asset; desactivar el componente `CombatHitstop` cancela inmediatamente una pausa activa. En escenas sin el servicio, las solicitudes son inocuas. No hay cambios globales persistentes al salir de Play Mode.

### Prioridades y efectos visuales

- Muerte tiene prioridad: cancela destello/señal y conserva el color final de cuerpo e indicador; el jugador conserva su manejo de muerte y el coordinador oculta su señal de éxito.
- Ruptura emite una señal dorada más grande y persistente que el parry. El indicador existente mantiene su turquesa durante toda la vulnerabilidad; el cuerpo terrestre recibe además un tinte derivado de ese estado.
- Un daño durante vulnerabilidad mezcla brevemente blanco sobre el tinte del cuerpo, mientras la franja turquesa conserva su identidad. Al terminar el destello vuelve al color derivado del estado actual, no a un color normal que borre la postura.
- El parry exitoso mantiene el feedback anterior del jugador y añade una señal cian breve detrás de su sprite. El fallo no emite `Succeeded`, no genera esa señal y no solicita hitstop.
- Las señales son sprites cuadrados provisionales, sin colliders, que crecen ligeramente y se desvanecen. Usan tiempo no escalado para seguir siendo visibles durante la micro-pausa. El color del cuerpo enemigo y el indicador tienen un único coordinador activo; los scripts anteriores solo escriben su alternativa cuando ese coordinador no está activo.
- Telegraph sigue en su renderer independiente y no se sobrescribe. El controlador existente lo cancela al morir o entrar en vulnerabilidad. El entrenador conserva sus ciclos y capacidades originales.

### Valores provisionales

| Parámetro | Valor inicial |
| --- | --- |
| Hitstop normal / pogo / parry | 0.05 / 0.04 / 0.07 s reales |
| Destello de daño | 0.10 s reales, blanco |
| Señal de parry | 0.16 s reales, cian, escala 1.35 |
| Señal inicial de ruptura | 0.25 s reales, dorado, escala 1.7 |
| Tinte vulnerable del cuerpo | Mezcla de 0.6 con el color de postura |
| Destello durante vulnerabilidad | Mezcla de 0.4 con blanco |
| Ventana de vulnerabilidad existente | 1.5 s de simulación, sin cambio |

### Validación realizada

- Unity `6000.5.5f1` compiló y ejecutó `CombatFeedbackSceneSetup.Configure`: `TASK10_SETUP_PASS` y salida `0` en `Logs/Task10Setup.log`.
- La batería completa se ejecutó en Play Mode del proyecto principal y terminó con `TASK10_VISUAL_PASS`, `TASK10_PARRY_PASS`, `TASK10_HITSTOP_PASS`, `TASK10_FEEDBACK_PASS` y `TASK09_PLAYMODE_PASS` en `Logs/Task10Validation.log`, sin errores C#, avisos C# ni excepciones.
- Se probaron ataque fallido sin pausa; horizontal y ascendente con pausa; un único impacto/destello con colliders duplicados; pogo vivo/vulnerable/letal con rebote conservado y pausa única; ruptura única; daño sin perder la franja vulnerable; apariencia tras recuperación; y muerte que no vuelve a colorearse por un efecto tardío.
- Se comprobó parry fallido sin éxito ni pausa, tres parries exitosos del entrenador sin duplicación ni daño, ruptura del entrenador y recuperación. La batería anterior sigue comprobando además parry/postura del enemigo terrestre, ataques, patrulla/persecución, bordes, muerte, colisiones sin empuje y los dos blancos antiguos de pogo.
- Se probaron veinte solicitudes solapadas sin acumular pausas, restauración automática a escala previa `0.5`, cancelación al desactivar/perder foco, respeto de escala externa `0.75`, opción desactivada y rechazo de una pausa externa a cero. Se verificaron inputs reales de salto/dash durante la pausa, recuperación posterior y congelación coherente de relojes de parry/i-frames. `fixedDeltaTime` permaneció igual.
- Las pruebas usan física/componentes reales y teclado sintético. Se preparan por reflexión algunas ventanas de parry/estados y una copia temporal de los datos de feedback para disponer de pausas largas en los casos de input; esos datos no se guardan. No se hizo una prueba humana ni se generó una build standalone.

### Pruebas manuales recomendadas

1. Abrir MovementTest en Unity `6000.5.5f1` y entrar en Play Mode. La escena y el prefab ya están configurados; no hace falta ejecutar los generadores anteriores.
2. Golpear con J horizontal y arriba + J: observar micro-pausa y destello del enemigo; fallar lejos de objetivos y comprobar que no hay pausa. Repetir con el enemigo moviéndose y revisar que sigue patrullando/persiguiendo/avisando al recuperarse.
3. Hacer abajo + J en el aire sobre el enemigo: comprobar daño, micro-pausa y rebote normal de fuerza 13, también en un golpe letal. Fallar y repetir sobre los blancos dorados: deben conservar su comportamiento anterior.
4. Usar K/hombro izquierdo justo antes del impacto del entrenador y del enemigo. El éxito debe producir la señal cian y pausa más marcada; el fallo conserva su color anterior y no genera señal de éxito. Acertar tres parries para observar la señal dorada de ruptura y el indicador turquesa durante la vulnerabilidad.
5. Golpear durante vulnerabilidad: el destello debe percibirse sin perder la franja turquesa; al terminar debe recuperar su apariencia actual. Matar durante el destello o ruptura: debe quedar con su color final, sin señal residual ni recoloreado posterior.
6. Probar movimiento, salto, doble salto, dash sin empuje, parry, muerte/R y controles de teclado/mando. Pulsar salto/dash durante una micro-pausa y comprobar la respuesta al reanudar. Revisar la consola y comprobar que nunca queda congelado.
7. Comparar con `Hitstop Enabled` desmarcado en `Assets/_Project/Data/Combat/CombatFeedbackData.asset`. Reponerlo al terminar; ajustar únicamente los parámetros de ese asset si la intensidad o duración requieren calibración. No cambiar todavía valores de combate/movimiento.
8. Para repetir las pruebas automáticas desde Edit Mode: `Fangbound/Validate Combat Feedback in Play Mode`. También el menú anterior de validación terrestre ejecuta ahora la batería ampliada.

### Limitaciones y pendientes

- Feedback provisional sin sonidos, partículas complejas, materiales/shaders definitivos, animaciones finales ni camera shake. La percepción visual y el balance deben revisarse manualmente; esta entrega no los certifica.
- El hitstop es global y detiene también el resto de la simulación de la escena. Si se añade posteriormente un gestor de menús/pausa que escriba simultáneamente el mismo valor cero, deberá coordinar la propiedad del tiempo mediante `Cancel`; dos escritores de cero no pueden distinguirse por leer `Time.timeScale`. Actualmente no existe ese segundo gestor.
- La señal inicial usa tiempo real y la vulnerabilidad usa tiempo de simulación. Las señales visuales no prolongan ni acortan la ventana de vulnerabilidad.
- Sigue pendiente la medición manual exacta de los `0.8 s` de i-frames de `PlayerHealth`. No se cerró ese pendiente ni se modificó su sistema para incorporar feedback.
- No se realizaron commits, push, cambios de rama ni trabajo de Tarea 11.

## Tarea 11: Llamas del Recuerdo — checkpoints y respawn

### Inspección y decisiones

- Se leyeron `AGENTS.md`, `GDD.md`, este registro y `Region1Plan.md`, y se revisaron salud/muerte/R, Input System, controlador, cámara, HUD, combate, hitstop, MovementTest y prefabs. HEAD era `2482378` (T10); el usuario confirmó T01–T10 terminadas, probadas manualmente y con commit.
- Existía un cambio local en `CombatFeedbackData.asset`: `normalHitstop=0.10` y `hitstopEnabled=0`. Se conserva exactamente; no se reactivó hitstop ni se modificaron otros parámetros de feedback en el asset.
- R ya llamaba a `PlayerHealth.RestartForTesting`, que restablece al jugador en la misma escena y no la recarga. No es necesario añadir persistencia estática del checkpoint, archivos, PlayerPrefs ni objetos persistentes entre escenas.
- La acción `Player/Interact` ya existía, con E y `<Gamepad>/buttonNorth`, y ningún componente la utilizaba. Se reutiliza esa acción sin editar Input Actions ni desplazar controles existentes. La lectura es por `WasPressedThisFrame`, sin repetición al mantener pulsado.
- La decisión narrativa/visual quedó registrada oficialmente en GDD, sección 5, subsección **Llamas del Recuerdo — descanso y checkpoints (decisión oficial)**. Se reemplazó la referencia genérica de calidad de vida por su nombre oficial, sin duplicar el diseño ni definir la causa narrativa de la reaparición.

### Arquitectura y archivos creados

- `Assets/_Project/Scripts/World/MemoryFlameData.cs` y `Assets/_Project/Data/World/MemoryFlameData.asset`: parámetros de interacción, colores, brillo provisional, señal de activación y comprobación del punto de reaparición. Los estados temporales permanecen en componentes.
- `Assets/_Project/Scripts/World/MemoryFlame.cs`: componente de la llama con rango, referencia a su hijo `Respawn Point`, renderers reemplazables y estado visual. Un registro de instancias activas permite buscar la llama más cercana dentro de su propio rango; se limpia al iniciar Play Mode sin recarga de dominio.
- `Assets/_Project/Scripts/Player/PlayerCheckpoint.cs`: lee Interact, muestra la indicación provisional de descanso, mantiene el checkpoint actual y emite `CheckpointChanged`/`Rested`. Rechaza descanso muerto, en dash, con control bloqueado, fuera de rango o con destino inseguro. Una interacción válida usa `TryHeal(MaximumHealth)`; no codifica el máximo 5 por separado. Reutilizar la llama actual cura/señala sin crear otro checkpoint ni emitir otro cambio.
- `Assets/_Project/Prefabs/World/MemoryFlame.prefab`: pedestal de piedra con tres sprites, llama flotante blanco-turquesa y brillo provisional. No tiene colliders, scripts de movimiento ni luces/partículas complejas; no cambia las colisiones del nivel. Los renderers y el punto de reaparición pueden reemplazarse/recolocarse sin reescribir la lógica.
- `Assets/_Project/Scripts/Editor/MemoryFlameSceneSetup.cs`: menú `Fangbound/Configure Movement Test Memory Flames`; crea datos/prefab si faltan, enlaza el componente del jugador e incorpora dos instancias sin regenerar la escena ni reconfigurar sistemas anteriores.
- `Assets/_Project/Scripts/Editor/MemoryFlameValidation.cs`: menú `Fangbound/Validate Memory Flames in Play Mode`; incorpora pruebas de E/R reales a la batería existente. Se añadieron sus `.meta` y los de carpetas/assets nuevos.

### Archivos existentes modificados

- `PlayerHealth.cs`: consulta opcionalmente el destino del checkpoint en el reinicio existente. Si no hay uno válido, conserva `spawnPosition`/`spawnRotation` anteriores. Desactiva brevemente combate/pogo/parry/controlador para limpiar sus ventanas mediante los `OnDisable` existentes, después restaura vida máxima, velocidad, gravedad, controles y estados como antes. Añade `Respawned` al terminar. No cambia recepción de daño, duración de i-frames, knockback, muerte ni HUD.
- `CameraFollow2D.cs`: añade `SnapToTarget`, que conserva el eje Z, respeta los límites actuales y limpia la velocidad de suavizado al teletransportar. `PlayerCheckpoint` lo llama al recibir `Respawned`, también para el spawn alternativo. No se modifican el seguimiento habitual, dead zone ni smooth time.
- `GroundEnemyValidation.cs`: integra la regresión de checkpoints. Para comprobar T10 aunque el diseñador haya desactivado hitstop, la batería usa una copia de sus datos en memoria con hitstop activo; mantiene las duraciones configuradas y restaura la referencia original al terminar o fallar. No guarda esa copia ni cambia el asset local.
- `MovementTest.unity`: añade `PlayerCheckpoint`, el grupo `Memory Flames` y dos instancias del prefab; conserva geometría, cámara, spawn inicial, jugador, objetivos, entrenador, enemigo y feedback anteriores.
- `Docs/GDD.md` y `Docs/DevelopmentLog.md`: decisión oficial y documentación de esta entrega. No se modifican Input Actions, parámetros de salud/movimiento/combate, datos de feedback, IA, física/capas ni el prefab enemigo. No hay commits, push, cambios de rama ni tareas posteriores.

### Descanso, estado y seguridad

- El checkpoint actual pertenece al componente del jugador durante la sesión. Solo la llama actual mantiene el brillo activo; al elegir otra, la anterior vuelve a tenue y puede volver a activarse. Cada descanso válido emite un destello blanco breve, también al reutilizar la misma llama.
- El descanso restaura vida hasta el máximo existente (`5` en MovementTest), sin superarlo. No teletransporta, reinicia enemigos, abre menús ni desbloquea habilidades. Tampoco cambia los temporizadores de daño/invulnerabilidad al curar.
- El punto configurado se valida al descansar y nuevamente antes de reaparecer. La comprobación usa el tamaño/offset real del collider del jugador, una holgura y la máscara de terreno: rechaza sólidos ocupando el cuerpo y exige soporte bajo los pies. Excluye el propio jugador y triggers; `EnemyBody` conserva su exclusión de contacto físico y no se convierte en suelo.
- Si se desactiva/destruye la llama, se ocupa su destino o deja de haber suelo, el reinicio usa el spawn original. No se busca una posición arbitraria ni se atraviesan paredes para corregir un destino mal configurado.
- No se recarga la escena al pulsar R, por lo que el checkpoint sobrevive a la muerte/reinicio. Salir de Play Mode o cargar otra escena/recrear al jugador inicia otra sesión; la persistencia entre niveles y el guardado permanente quedan fuera de esta entrega.

### Configuración y posiciones

| Parámetro | Valor inicial |
| --- | --- |
| Interacción | E / botón norte del mando, acción Interact existente |
| Rango | 1.3 unidades |
| Destello de activación | 0.30 s reales |
| Llama tenue | Turquesa apagado, alfa 0.4 |
| Llama actual | Blanco-turquesa, alfa 1 |
| Brillo tenue / activo | Alfa 0.08 / 0.30 |
| Holgura de respawn / sonda de suelo | 0.02 / 0.20 unidades |
| Desplazamiento local del Respawn Point | (-0.6, 0.58) |

- `Memory Flame - Start`: altar en `(-7.9, -2.1)`, destino aproximado `(-8.5, -1.52)` sobre el suelo inicial, a la izquierda de la zona de ataque del entrenador.
- `Memory Flame - Upper`: altar en `(5.8, 2.725)`, destino aproximado `(5.2, 3.305)` sobre la plataforma alta existente, fuera del alcance vertical del enemigo terrestre. Se llega con las habilidades de movimiento ya disponibles.

### Validación

- Unity `6000.5.5f1` compiló y ejecutó el configurador mediante la API del Editor: `TASK11_SETUP_PASS`, salida `0`, en `Logs/Task11Setup.log`.
- La ejecución final completa terminó con `TASK11_CHECKPOINT_PASS` y `TASK09_PLAYMODE_PASS` en `Logs/Task11Validation.log`, junto a las marcas de T10, sin errores C#, avisos C# ni excepciones. Incluye R con el cuerpo dinámico libre y desplazamiento horizontal real después de reaparecer, no solo la medición del destino con el fixture inmovilizado.
- La regresión usa teclado sintético para Interact/E y Restart/R. Comprueba ausencia de activación fuera de rango/muerto; primer descanso con curación; tecla mantenida sin repetición; reactivación sin duplicar checkpoint; sustitución por la segunda llama; persistencia con muerte/R; vuelta a la primera; colores/activación; curación limitada al máximo; referencias del HUD/cámara; y limpieza de ventanas de combate, bloqueo y knockback.
- También prueba que un destino ocupado se rechace y provoque fallback al spawn original, y que vuelva a ser válido al retirar el obstáculo. La batería anterior comprueba movimiento, dash sin empuje, pogo normal/vulnerable/letal, parry, postura, IA, muerte, hitstop y feedback.
- Las pruebas disponen temporalmente de cuerpo inmovilizado en los casos de interacción y preparan algunos estados por los contratos existentes/reflexión. No se modifican ni guardan parámetros del juego para simular un éxito. La revisión humana del aspecto y del recorrido sigue siendo necesaria; no se generó una build standalone.

### Pruebas manuales exactas en Unity

1. Abrir `Assets/_Project/Scenes/Test/MovementTest.unity` en Unity `6000.5.5f1`. No regenerar la escena completa. Entrar en Play Mode: ambas llamas comienzan tenues y no hay checkpoint actual.
2. Para comprobar la alternativa original, morir sin usar E y pulsar R: debe volver a `Player Spawn Point`, con vida completa y controles operativos.
3. Acercarse a la llama del suelo inicial, alrededor de `x=-7.9`. Debe aparecer la indicación de descanso con E y el binding del mando disponible. Pulsar E: señal blanca, brillo activo y vida completa. Para probar curación, recibir daño del entrenador, alejarse a la izquierda y descansar cuando termine el bloqueo de knockback.
4. Mantener E: no debe producir varios descansos. Soltar y volver a pulsar permite curarse otra vez. Alejarse más de `1.3` unidades: desaparece el aviso y E no activa la llama.
5. Usar salto/doble salto y las plataformas existentes para llegar a la llama de la plataforma alta, cerca de `(5.8, 2.725)`. Pulsar E: esta se ilumina y la primera vuelve a tenue.
6. Volver al combate, dejar que el enemigo o entrenador agote la vida y pulsar R. Debe reaparecer sobre la plataforma alta, aproximadamente en `(5.2, 3.305)`, con 5/5 de vida, sin knockback ni bloqueos. La cámara debe mostrarlo inmediatamente respetando sus límites; comprobar HUD, caminar, salto, ataque, parry, dash, doble salto y pogo.
7. Volver a la primera llama y pulsar E: al morir/R debe reaparecer junto a ella. Los enemigos muertos no resucitan y los vivos conservan su estado; no hay respawn global del mundo.
8. Para configurar otro destino, mover el hijo `Respawn Point` del prefab/instancia, manteniendo al menos la holgura sobre suelo libre. Activar Gizmos al seleccionar la llama muestra rango y referencia del cuerpo. Un punto dentro de un sólido o sin suelo será rechazado.
9. Desde Edit Mode, usar `Fangbound/Validate Memory Flames in Play Mode` para repetir la batería completa. Los menús anteriores de validación incluyen también el nuevo caso. Los ajustes locales de hitstop permanecen intactos al finalizar.

### Límites y pendiente conservado

- Representación de sprites provisionales, sin arte final, partículas, audio, menús, viaje rápido, recuerdos/cinemáticas ni explicación narrativa definitiva de la reaparición. Las variantes regionales pueden sustituir los hijos visuales conservando el componente.
- Comprobación conservadora mediante caja del cuerpo y máscara de terreno; para nuevos niveles se deben configurar correctamente las capas y el punto de spawn. No implementa búsqueda de rutas ni reparación automática de destinos.
- Sigue pendiente la medición manual exacta de los `0.8 s` de i-frames de PlayerHealth, fuera del alcance de T11. Sus valores no se alteraron.

## Ajuste final de T11: contraste del HUD de vida

- El usuario confirmó T11 aprobada en sus pruebas manuales. Se ajustó únicamente la presentación de `PlayerHealthUI.cs`: lleno `#59D9D0`, vacío `#34343D`, borde `#91A9AC` de 2 px, configurables mediante campos serializados del componente. Se conservan los cinco indicadores actuales, tamaño de 26 px, separación de 5 px y posición original.
- Los bloques usan `Texture2D.whiteTexture` con rellenos opacos, evitando que el fondo del skin de `GUI.Box` oscurezca los colores. El HUD sigue leyendo `CurrentHealth`/`MaximumHealth` en cada dibujo; no se modificaron salud, daño, invulnerabilidad, curación, muerte, respawn ni checkpoints. Se restaura `GUI.color` al terminar para no afectar otras indicaciones.
- En `CombatFeedbackData.asset` se activó `Hitstop Enabled`. Se conserva la duración normal ya configurada en `0.10 s`, pogo `0.04 s` y parry `0.07 s`, sin cambios en otros parámetros.
- Unity `6000.5.5f1` compiló y ejecutó la batería completa en una copia aislada para no cerrar el Editor del usuario. `Logs/Task11HudValidation.log` terminó con `TASK11_CHECKPOINT_PASS` y `TASK09_PLAYMODE_PASS`, incluidas las regresiones de T10, sin errores C#, avisos C# ni excepciones. Se cubren daño, curación/reactivación/cambio de llama, muerte y R; `git diff --check` pasó.
- El contraste calculado entre lleno/vacío es aproximadamente `7.2:1`; lleno/fondo de cámara `9.6:1` y borde/fondo `6.6:1`. El borde mantiene distinguibles los huecos oscuros sobre el fondo de la escena.
- Se revisaron capturas del Game View real y sus píxeles: `5/5`, `4/5`, `2/5` y `1/5` tienen exactamente los rellenos `#59D9D0`/`#34343D`; al curarse o cambiar checkpoint se conservan cinco llenos, al morir cinco vacíos y tras R cinco llenos. La comprobación gráfica aislada terminó con `HUD_RENDER_TRANSITIONS_PASS` en `Logs/Task11HudRender.log`; las imágenes están en `Logs/HudPreview-*.png`. La herramienta temporal de captura queda fuera de los archivos del producto, en la carpeta ignorada `.utmp`.
- Prueba manual: en MovementTest recibir daño para comparar `5/5`, `4/5`, `2/5` y `1/5`; descansar con E para recuperar los cinco bloques turquesa; cambiar de llama; morir para ver los cinco huecos y reaparecer con R para recuperar `5/5`. Comprobar también el hitstop activo en impactos normales. No se hicieron commits, push ni cambios de rama. La medición manual exacta de los `0.8 s` de i-frames continúa pendiente.

## T12 — Primer recorrido jugable de Arboleda Verdescente

### Entrega y reutilización

- Nueva escena independiente: `Assets/_Project/Scenes/Test/ArboledaPrototype.unity`. Puede abrirse y ejecutarse directamente; no necesita arrancar MovementTest ni ejecutar configuradores.
- Se copió la configuración de jugador, spawn, cámara, límites, HUD y servicio de hitstop de MovementTest mediante la API del Editor, y se sustituyó únicamente la geometría de la nueva copia. Se instanciaron los prefabs oficiales `GroundEnemy.prefab` y `MemoryFlame.prefab`, enlazando el objetivo del enemigo al jugador de la escena.
- No se modificaron scripts de ejecución, prefabs, estadísticas, datos de movimiento/combate, Input Actions, matriz física, GDD, Region1Plan ni Build Settings. MovementTest conserva el mismo hash SHA-256 anterior al trabajo. Los parámetros y sistemas existentes siguen compartidos.
- Se crearon dos utilidades bajo `Scripts/Editor`: `ArboledaPrototypeSceneSetup.cs`, configuración puntual que rechaza sobrescribir una escena existente, y `ArboledaPrototypeValidation.cs`, prueba de integración del recorrido. No generan geometría en ejecución ni son dependencias de la escena guardada. Se incluyen los `.meta` de escena y scripts.

### Recorrido provisional

| Sección | Distribución |
| --- | --- |
| A — Inicio | Spawn en `(3, 0.6)`, espacio llano inicial, señal de dirección y dos escalones amplios de 0.9/1.8 m de altura. |
| B — Bifurcación | Meseta de 10 m a altura 2.4; camino principal con plataformas bajas y escalón de retorno. Cuatro plataformas superiores conducen al descubrimiento ambiental. |
| Ruta superior | Superficies a alturas 3.9, 5.4, 6.9 y 8.4; anchuras 3.4/3.8/4/6 m. Ruina de columnas incompletas, piedra caída, musgo y cristal turquesa tenue, sin componentes interactivos. Se baja por la derecha a la arena antes de la llama. |
| C — Descanso y combate | Arena plana de 19 m entre `x=44..63`, altura 2.4. Llama en `(47, 2.4)`, respawn en `(46.4, 2.98)`. Único enemigo en `(57.5, 2.92)`, patrulla original aproximadamente `54.4..59.8`, alejado del checkpoint y los extremos. |
| D — Final | Escalón y plataforma amplia con arco de piedra, vegetación y rótulo «FIN DEL RECORRIDO», cerca de `x=75`. Sin transición, puerta, victoria ni nueva interacción. |

- Anchura transitable: 80 m. Suelo sólido continuo a altura 0 con espesor de 4 m y límites sólidos laterales; caer de las plataformas permite volver a saltar desde suelo seguro. No hay daño ambiental, precipicios abiertos, triggers de muerte ni recuperación de caída.
- Altura de plataforma máxima: 8.4 m; envolvente vertical del personaje con salto normal completo: aproximadamente 12.2 m sobre el suelo. Los límites de cámara cubren `x=0..80`, `y=-4..16`, incluyendo margen sobre saltos y bajo el suelo. Se conservan dead zone, suavizado y tamaño ortográfico existentes.
- Jerarquía: `Level Geometry` contiene suelo/límites, `Main Route` con A–D y `Optional Upper Route - Four Platforms`; grupos separados para decoración provisional, enemigos, checkpoint e hitos de inicio/final.

### Capacidades y márgenes

- Datos existentes inspeccionados: velocidad horizontal 7.5 m/s, velocidad inicial de salto 13 m/s, gravedad 3 × 9.81 m/s² y paso físico 0.02 s. Cálculo continuo de salto sostenido: altura 2.87 m y alcance aproximado en plano 6.63 m a velocidad constante; no incluye aceleración ni discretización.
- Medición con la física real: altura libre de unos **2.74 m**. La subida máxima diseñada es **1.5 m** en la rama superior; los huecos de esa rama son como máximo **1.4 m**. El camino principal tiene huecos de hasta **2 m** y subida máxima de **1.3 m** entre apoyos previstos. Se usa salto normal sostenido, sin cambiar fuerza, gravedad ni capacidades aprobadas.
- Las plataformas superiores son sólidas también por debajo. Bajo la segunda plataforma, el salto del camino inferior encuentra su techo y alcanza aproximadamente 1.66 m; el aterrizaje en el siguiente bloque se comprobó físicamente. No se añadieron plataformas de un solo sentido ni capas nuevas.

### Validaciones ejecutadas

- Unity **6000.5.5f1** creó la escena con `TASK12_SETUP_PASS`, salida 0. Se utilizó una copia aislada bajo `.utmp/Task09CollisionValidation` para mantener abierto el Editor del usuario; los logs quedan en `Logs/` y no se versionan.
- `Logs/Task12Validation.log`: `TASK12_SCENE_PASS`, todos los `TASK12_JUMP_PASS`, dos `TASK12_ROUTE_PASS`, `TASK12_ENCOUNTER_PASS` y `TASK12_PLAYMODE_PASS`, salida 0. Se verificaron referencias, prefabs, ausencia de scripts perdidos, plataformas sólidas en Default, HUD/servicio de hitstop, límites de patrulla y exclusión de colisión corporal jugador/enemigo.
- El bot usa eventos del New Input System sobre el cuerpo dinámico real: A/D y Espacio durante ambas rutas, E en el checkpoint y J/R en el encuentro. Dash y doble salto se deshabilitan **solo temporalmente en la prueba** y se restauran al terminar; en la escena permanecen disponibles. No se teletransporta al personaje durante cada recorrido ni se añaden fuerzas o velocidades para superar saltos.
- Para aislar las plataformas, la IA se deshabilita temporalmente durante las dos travesías; después se reactiva y se comprueban patrulla, detección/persecución, telegrafiado y daño real al jugador. Se retrocede a la llama y se cura con E; se prepara una muerte mediante el contrato de daño existente, se pulsa R y se verifica vida completa, posición segura y cámara. Finalmente, ataques normales con J matan al enemigo sin exigir habilidades especiales. Los reinicios entre pruebas utilizan el respawn existente.
- La validación observa el cuerpo dentro del viewport y del volumen seguro durante los desplazamientos/saltos. Ambas rutas llegan al final y activan la misma llama después de reconectar. No es una prueba de todas las relaciones de aspecto ni sustituye la valoración humana del seguimiento de cámara.
- `Logs/Task12Regression.log`: batería anterior completa con `TASK09_COLLISION_PASS`, `TASK09_RECOVERY_PASS`, `TASK09_POGO_PASS`, `TASK10_FEEDBACK_PASS`, `TASK11_CHECKPOINT_PASS` y `TASK09_PLAYMODE_PASS`, salida 0. Incluye caminar/dash sin empuje, retorno desde límites/paredes, pogo vivo/vulnerable/letal, entrenador, parry, ruptura de postura, daño vulnerable, muerte, feedback, hitstop, curación y R. No se observaron errores C#, avisos C# ni excepciones en estas ejecuciones finales.
- Hitstop permanece habilitado: normal **0.10 s**, pogo **0.04 s**, parry **0.07 s**. HUD conserva los cinco bloques turquesa `#59D9D0`, huecos `#34343D` y bordes actuales. Se compararon hashes de los archivos protegidos para comprobar su conservación.
- Se revisaron seis capturas del Game View real: inicio, bifurcación, ruina, arena, final y vista general. Se redujeron/reubicaron rótulos decorativos que invadían la zona del personaje y se repitió T12 con éxito. `Logs/Task12Render.log` terminó con `TASK12_RENDER_PASS`, salida 0; vistas en `Logs/ArboledaPreview-*.png`. La herramienta temporal de captura permanece fuera del producto, bajo `.utmp`; posicionar al personaje para estas vistas no cuenta como prueba de navegación. La escena y los dos scripts entregados se compararon por hash con los archivos finales validados.

### Prueba manual y límites pendientes

1. Abrir `ArboledaPrototype.unity` directamente en Unity 6000.5.5f1 y pulsar Play. Comprobar inicio, cinco bloques de vida y cámara. Usar A/D o flechas y Espacio mantenido para saltos completos; avanzar por el camino inferior hasta el arco final sin dash, doble salto, pogo ni parry.
2. Reiniciar Play Mode y tomar la rama superior desde la meseta de B. Subir las cuatro plataformas, observar la ruina/cristal y bajar por su extremo derecho hacia la arena, antes de la llama. Probar caídas voluntarias entre plataformas: debe haber suelo seguro y salida mediante saltos normales.
3. Descansar con E junto a la llama antes de acercarse al enemigo. Comprobar destello y estado activo. Recibir daño, retirarse y volver a descansar para curarse. Morir en el encuentro y pulsar R: reaparecer junto a la llama con 5/5, controles operativos y cámara en el destino.
4. Probar combate normal, parry, vulnerabilidad temporal por postura rota, pogo —incluido letal—, dash sin empuje y muerte enemiga. Confirmar feedback/HUD/hitstop. En la escena los enemigos muertos no reaparecen al usar R, según el contrato existente.
5. Alcanzar el arco y comprobar que solo señala el final. Revisar legibilidad de señales, ruta opcional, siluetas, cristal tenue y cámara en la relación de aspecto habitual. Reabrir MovementTest para una comprobación manual breve de compatibilidad.
6. Desde Edit Mode: `Fangbound/Validate Arboleda Prototype in Play Mode` repite T12; `Fangbound/Validate Ground Enemy in Play Mode` repite la batería anterior. No es necesario regenerar escenas.

- Es un bloqueado geométrico provisional de Arboleda, sin arte final ni mapa/narrativa definitivos. No se generó una build standalone.
- **Duración humana pendiente:** los recorridos automáticos aislados duraron 16.7 s (principal) y 17.9 s (superior), con trayectorias ya conocidas, sin exploración ni combate durante la travesía. No se ha demostrado el objetivo de 3–5 minutos de primera exploración; la velocidad aprobada y el tamaño compacto permiten atravesarlo rápidamente. Cronometrar una primera partida y decidir el ritmo tras esa prueba, sin presentar los tiempos del bot como una sesión humana.
- Sigue pendiente la medición manual exacta de los **0.8 s** de i-frames de PlayerHealth. No se alteró ese sistema.
- No se hicieron commits, push ni cambios de rama.

## T13 — Ampliación del recorrido de Arboleda Verdescente

### Alcance y conservación

- El desarrollador confirmó las diez pruebas manuales de T12, con una primera exploración de aproximadamente 1–2 minutos. T13 amplía la misma `ArboledaPrototype.unity` para añadir variedad y dificultad progresiva; todavía requiere una nueva prueba humana de ritmo y duración.
- Se conservaron inicio, saltos A/B, las cuatro plataformas de la ruina, columnas, vegetación, cristal, jugador, HUD, cámara, llama, enemigo y arco. La arena y el final se desplazaron juntos **70 m a la derecha**, manteniendo dimensiones, alturas y distancias relativas del encuentro.
- La ampliación es aditiva: los **109 GameObjects originales** mantuvieron sus identificadores. La comparación del archivo serializado conservó los **319 identificadores originales**, con 641 objetos/componentes serializados en total tras ampliar. No se regeneró la escena desde MovementTest ni se borró geometría anterior.
- Solo se adaptaron las dos herramientas de Editor de T12. `ArboledaPrototypeSceneSetup.Expand` abre la escena existente, rechaza Play Mode, escenas abiertas con cambios sin guardar, posiciones inesperadas de los elementos que desplaza y una ampliación ya aplicada. `Generate` sigue rechazando un destino existente. No hay generador de nivel en ejecución ni herramienta genérica nueva.

### Distribución y dificultad

| Tramo | Diseño final |
| --- | --- |
| Inicio y ruina | Geometría de T12 conservada hasta `x≈44`. Nuevo apoyo bajo un hueco de la ruina y retorno escalonado a alturas 6.5/4.7/2.4. |
| C — Ascenso entre árboles | Terraza de entrada `44..52` a altura 2.4. Subida a 3.9, salto al este a 5.5, cambio hacia la izquierda a 7.2 y regreso a la derecha a un descanso amplio a 8.8. Se continúa por una terraza desde `64.5..81`. |
| Mirador opcional | Plataformas a 10.7, 12.7 y 14.5, anchuras 3.5/3.5/5 m. Postes incompletos, roca, vegetación y siluetas rocosas de fondo distinguen la composición de la ruina. Regreso por apoyos a 12.5/10.6/8.7 y descenso al descanso principal a 6.8. |
| D — Pasarelas | Descanso inferior `81..92` a 6.8; subidas a 8.6, 10.4 y 11.9, con anchuras de 4/3.5/4 m y huecos de hasta 2 m. Descenso en cuatro escalones de 9.9/7.9/5.9/4.1 antes de la llama. |
| E — Checkpoint y arena | Arena original de 19 m en `114..133`, altura 2.4. Llama en `(117, 2.4)` y destino `(116.4, 2.98)`. Enemigo en `(127.5, 2.92)` con patrulla aproximadamente `124.4..129.8`, sin cambios de datos o IA. |
| F — Final | Escalón y plataforma originales desplazados; arco y rótulo de final alrededor de `x=145`. Sin nuevas interacciones. |

- Anchura total: **150 m**. Plataforma máxima: **14.5 m**; envolvente del personaje con salto normal desde ella: aproximadamente **18.3 m**. Cámara existente con límites `x=0..150`, `y=-4..22`; tamaño ortográfico, dead zone y suavizado conservados.
- Datos actuales comprobados: velocidad 7.5 m/s, salto 13 m/s, gravedad 3 y paso físico 0.02 s. Las pruebas volvieron a medir unos **2.74 m** de altura libre. Nuevas subidas principales de hasta **1.8 m**, mirador hasta **2 m** y hueco principal máximo de **2.5 m** en el cambio de dirección. No se cambió el control aéreo, coyote time, jump buffer ni otro parámetro para facilitar saltos.
- Jerarquía nueva `Level Geometry/T13 Expansion`: retorno de la ruina, `C - Switchback Grove`, `Optional Lookout Route` y `D - Stepped Passages`. El encuentro anterior queda agrupado en E y el final en F; decoración nueva separada de la ruina conservada.

### Fallos recuperables

- Suelo continuo ampliado a 150 m y paredes laterales hasta altura 22. Terrazas inferiores y bloques escalonados proporcionan apoyos físicos; no hay daño ambiental, teletransporte ni sistema de recuperación por caída.
- `Ruin Catch`, a 4.6, recoge una caída desde la segunda plataforma de la ruina y permite volver a ella. Los nuevos escalones del regreso evitan bajar directamente desde 8.4 hasta el camino de 2.4.
- En el ascenso, fallar junto al cambio de dirección permite aterrizar en la entrada a 3.9 y repetir los dos saltos, en vez de reiniciar desde el suelo. La recuperación baja junto al lado este permite volver por los apoyos anteriores.
- La terraza a 8.8 recoge fallos iniciales del mirador; `Lookout Catch`, a 11.3, permite recuperar el salto entre la segunda plataforma y el mirador. El retorno oriental baja mediante escalones de alrededor de 2 m.
- Las pasarelas tienen terraza inferior a 6.8 y `Passage Catch` a 8.7, bajo el hueco final. Los apoyos se colocaron fuera de los vuelos necesarios; la travesía completa comprueba que los saltos previstos no queden bloqueados.

### Validaciones de T13

- Unity **6000.5.5f1**, en copia aislada para conservar abierto el Editor del desarrollador. `Logs/Task13Setup.log`: `TASK13_SETUP_PASS`, salida 0, sin errores/avisos C# en la ejecución final.
- `ArboledaPrototypeValidation` conserva las comprobaciones de referencias, prefabs, scripts ausentes, colisiones, HUD, hitstop y encuentro de T12, adaptando coordenadas a los objetos reales. Añade saltos hacia la izquierda, descenso natural, frenado de la entrada del bot según la desaceleración existente y tres itinerarios independientes.
- `Logs/Task13Validation.log`: `TASK12_PLAYMODE_PASS` y `TASK13_PLAYMODE_PASS`, salida 0. Recorridos principal, ruina/retorno y mirador/retorno con teclado sintético A/D y Espacio sobre Rigidbody2D real; E para la llama. Dash y doble salto se bloquean solo temporalmente durante la validación, sin guardar esos cambios. La IA se aísla durante las travesías y se reactiva para el encuentro.
- Cuatro recuperaciones físicas verificadas sin teletransporte ni curación: ruina (caída de 0.8 m y salto de retorno), ascenso (3.3 m y vuelta por dos saltos), mirador (1.4 m y retorno) y pasarelas (1.7 m y retorno). Se verifican vida completa, aterrizaje en el collider previsto y cuerpo dentro del viewport durante desplazamientos y caídas.
- Encuentro separado aprobado: patrulla dentro de límites, persecución, telegrafiado, golpe real, retirada y curación E, muerte preparada mediante el contrato existente, R a la llama con vida completa/cámara y muerte enemiga mediante J, sin habilidades especiales. Las dos rutas opcionales vuelven al recorrido antes del checkpoint.
- Tiempos del bot: **37.7 s** principal, **42.6 s** ruina con recuperación y **50.2 s** mirador con recuperaciones. Son trayectorias conocidas con IA aislada durante la travesía; no representan una partida humana ni demuestran los **3–4 minutos** objetivo.
- `Logs/Task13Regression.log`: batería anterior completa, salida 0, con `TASK09_COLLISION_PASS`, `TASK09_RECOVERY_PASS`, `TASK09_POGO_PASS`, `TASK10_FEEDBACK_PASS`, `TASK11_CHECKPOINT_PASS` y `TASK09_PLAYMODE_PASS`. Conserva dash/caminar sin empuje, recuperación de IA, pogo vivo/vulnerable/letal, entrenador, parry/postura, daño vulnerable, muerte, feedback, hitstop y checkpoints. No se observaron errores ni avisos C# o excepciones en las ejecuciones finales de configuración, T13 y regresión.
- Se revisaron nueve capturas del Game View real: inicio, bifurcación, ruina, ascenso, mirador, pasarelas, arena, final y vista general. `Logs/Task13Render.log`: `TASK13_RENDER_PASS`, salida 0; imágenes en `Logs/Task13Preview-*.png`. Son vistas técnicas con posiciones preparadas, independientes de la prueba de navegación. La herramienta de captura queda en `.utmp`, sin añadir scripts al producto.
- Escena y scripts entregados desde la copia validada, con hashes coincidentes entre ambas copias. Al finalizar se eliminaron espacios finales del YAML emitido por Unity, sin alterar valores ni identificadores; `git diff --check` pasó. MovementTest, scripts de ejecución, prefabs, datos, Input Actions, Physics Settings, HUD, GDD y Region1Plan conservan los hashes iniciales. Hitstop sigue habilitado en **0.10/0.04/0.07 s** para normal/pogo/parry; colores del HUD conservados. Archivos de producto modificados: escena, los dos scripts de Editor existentes y este registro; sin nuevos archivos de producto.

### Pruebas manuales pendientes

1. Abrir directamente `ArboledaPrototype.unity` y entrar en Play. Completar el camino principal solo con A/D o flechas y Espacio; en el ascenso intermedio, saltar hacia la izquierda y luego volver a la derecha. Observar el aumento gradual de dificultad y los descansos.
2. Explorar la ruina original y su cristal; bajar por los dos nuevos escalones hacia la terraza de entrada, antes de continuar el ascenso.
3. Desde la terraza alta a 8.8, subir al mirador con salto normal, observar su composición y regresar por los tres escalones orientales hasta el descanso a 6.8. La rama es opcional: también se puede caminar por debajo.
4. Fallar deliberadamente saltos de ambos ramales y de las pasarelas. Confirmar aterrizajes sin daño y regreso por los apoyos inferiores. Las pruebas automáticas cubren casos representativos; revisar otros puntos y direcciones de caída, especialmente junto a bordes.
5. Completar las pasarelas, bajar al checkpoint y descansar con E o mando. Recibir daño, retirarse para curarse, morir y pulsar R. Comprobar cámara, 5/5, parry, postura/vulnerabilidad temporal, pogo, dash sin empuje y muerte enemiga; llegar al arco final.
6. Revisar cámara y señales en la relación de aspecto habitual. Cronometrar una primera exploración humana, incluyendo ambas rutas, sin confundirla con los tiempos del bot.
7. Repetir `Fangbound/Validate Arboleda Prototype in Play Mode` para T12/T13 adaptada, y `Fangbound/Validate Ground Enemy in Play Mode` para la batería anterior de MovementTest. No ejecutar configuradores para jugar la escena guardada.

- Continúa siendo geometría y decoración provisional, sin cambios narrativos o mapa definitivo de la región. No se generó build standalone. El ajuste de ritmo de primera exploración y la evaluación humana de cámara/legibilidad quedan pendientes.
- Se conserva pendiente la medición manual exacta de los **0.8 s** de i-frames de PlayerHealth.
- Sin commits, push ni cambios de rama.

## T13 — Corrección 01: interacción de checkpoint y ubicación del enemigo

- **Causa confirmada por el desarrollador:** el objeto señalado era el cristal de la ruina superior. `Provisional Decoration/Ancient Ruin - Decoration Only/Faint Crystal` está en `(41.4, 9.3)` y solo tiene Transform/SpriteRenderer. Es el cristal ambiental de T12, sin `MemoryFlame`, collider ni interacción; E no debe activarlo. Su comportamiento es el previsto y no requiere corrección funcional. El altar oficial sí respondió en las pruebas automatizadas.
- Hay **una sola Llama interactiva**, `Checkpoint/Memory Flame - Arboleda`, con raíz global `(117, 2.4)`, visual flotante `(117, 3.05)` y respawn `(116.4, 2.98)`. Está activa/habilitada, conserva datos/renderers/destino y usa el mismo prefab/sistema que las llamas funcionales de MovementTest. Rango **1.3**, sin cambios. No hay duplicados ni separación entre visual, componente y destino: el padre `Checkpoint` aporta `+70` en X a toda la instancia.
- Hay **un solo enemigo terrestre**, `Enemies/Ground Enemy - Arena`, en `(127.5, 2.92)`, con referencias válidas al jugador, datos y hitbox. En Play está activo, con 4 puntos de vida y cuerpo simulado; patrulla prevista `124.4..129.8`. Su traslado desde `(57.5, 2.92)` fue intencional: el padre `Enemies` aporta `+70` en X. Se encuentra siguiendo el camino principal, después de la llama y antes del arco; no corresponde restaurarlo ni duplicarlo en la ubicación anterior.
- **Corrección funcional:** no fue necesaria en la escena guardada. No se modificaron escena, herramientas T13, prefabs, entrada, rango, sistemas de ejecución ni parámetros. Único archivo de producto modificado en este diagnóstico: este registro; los cambios sin commit que ya existían de T13 se conservaron. El auxiliar de diagnóstico queda en `.utmp/CheckpointDiagnosis01.cs`, fuera del producto.
- **Prueba real de entrada y física:** Unity 6000.5.5f1 ejecutó `Logs/Task13Diagnosis01.log`, con `DIAG01_PLAYMODE_PASS`, salida 0. Se comprobó E sin efecto en el cristal, rechazo fuera del radio de la llama (distancia 1.590), E dentro (1.000), curación 3→5, checkpoint único, destello blanco y brillo activo persistente. Botón norte sintético del mando reactivó/curó 4→5 sin duplicar checkpoint; muerte y R conservaron el destino seguro, restauraron 5/5 y permitieron caminar con el cuerpo dinámico y cámara correctos. Se posicionaron fixtures entre comprobaciones; esto no se presenta como recorrido humano ni como travesía del nivel.
- **Regresiones repetidas:** `Logs/Task13Diagnosis01Routes.log` terminó con `TASK13_PLAYMODE_PASS`, incluidos los tres recorridos, recuperaciones y encuentro con patrulla/persecución/ataque; `Logs/Task13Diagnosis01Regression.log` terminó con `TASK09_PLAYMODE_PASS`, `TASK10_FEEDBACK_PASS` y `TASK11_CHECKPOINT_PASS`, incluyendo las dos llamas de MovementTest y su sustitución de checkpoint. Ambas ejecuciones salieron con 0. Sin errores/avisos C# ni excepciones en las tres ejecuciones finales; `git diff --check` correcto. Los 201 archivos protegidos del proyecto —todos los inspeccionados salvo este registro— conservaron sus hashes iniciales, incluida la escena T13 sin commit.
- **Pendiente manual:** abrir ArboledaPrototype guardada y distinguir el cristal sin pedestal del altar posterior a las pasarelas. En el altar, con Game View enfocado y dentro de rango, soltar/pulsar E o el botón norte, comprobar la indicación de descanso, curación y cambio visual; recibir daño del enemigo situado más a la derecha, morir y usar R. Si el fallo ocurre en ese altar concreto, contrastar también el estado no guardado del Editor, que no forma parte de la copia probada. No se ha atribuido un defecto al rango ni al sistema global de interacción. La medición manual exacta de los **0.8 s** de i-frames permanece pendiente. Sin commits, push ni cambio de rama.

## T13 — Ajuste final: retirada del primer elemento turquesa

- Se volvió a identificar el objeto mediante inventario de sprites y observación del frustum de la cámara durante la ruta principal con entrada/física reales. El primer elemento turquesa era `Crystal Glow`, seguido por `Faint Crystal`, ambos en `(41.4, 9.3)`, al saltar cerca de `x=36.4`. `Logs/Task13TurquoiseAudit.log`: `TURQUOISE_AUDIT_PASS`. No se asumió su identidad por el diagnóstico anterior.
- Se retiraron únicamente `Ancient Ruin - Decoration Only/Faint Crystal` y su propio `Crystal Glow`, ambos decorativos sin MemoryFlame, lógica ni colliders. Se conservaron los siete objetos restantes de la ruina —piedras, columnas, vegetación— y todas las plataformas. La comparación del YAML verificó que solo desaparecieron seis objetos/componentes serializados y los dos enlaces de sus Transform al padre; todos los demás datos quedaron idénticos.
- La única llama funcional continúa en `(117, 2.4)`, con respawn `(116.4, 2.98)` y rango 1.3; el único enemigo sigue en `(127.5, 2.92)`. Rutas, jugador, movimiento, combate, cámara, HUD, hitstop y MovementTest se conservan. Archivos modificados por este ajuste: `ArboledaPrototype.unity` y este registro; los scripts de Editor y cambios previos sin commit de T13 se mantuvieron intactos.
- Unity 6000.5.5f1, copia aislada: `Logs/Task13TurquoiseCheckpoint.log` terminó con `DIAG01_PLAYMODE_PASS`. Confirmó ausencia del cristal/brillo, siete objetos de ruina conservados, E y botón norte, curación completa, destello/brillo activo, checkpoint único y muerte/R con vida, suelo, cámara y movimiento restaurados. Las posiciones de interacción se prepararon como fixtures; no fue una prueba humana.
- `Logs/Task13TurquoiseRoutes.log`: `TASK13_PLAYMODE_PASS`, salida 0; camino principal y ambas rutas opcionales recorridos con teclado sintético y física real, sin dash/doble salto, incluidos retorno, recuperaciones y encuentro. `Logs/Task13TurquoiseRender.log`: `TURQUOISE_RENDER_PASS`, salida 0; se revisaron capturas reales del Game View de la ruina sin cristal y del altar conservado (`Logs/TurquoiseFinalPreview-Ruin.png` y `Logs/TurquoiseFinalPreview-Arena.png`). No se observaron errores/avisos C# ni excepciones en las ejecuciones finales. La escena entregada coincide por hash con la validada; 200 archivos protegidos del proyecto conservaron sus hashes iniciales, incluidos scripts, datos y MovementTest. `git diff --check` correcto.
- Prueba manual pendiente: abrir la escena directamente, visitar la ruina y comprobar que desaparecieron cristal y brillo conservando el resto; completar el recorrido y probar E/mando, curación y R en el altar final. La medición manual exacta de los **0.8 s** de i-frames permanece pendiente. Sin commits, push ni cambio de rama.

## T13 — Persistencia del objeto turquesa y recarga del Editor

- **Evidencia en el Editor del desarrollador:** una captura de solo lectura de su ventana (`Logs/UnityLiveTurquoise.png`) mostró Unity en Play Mode, en el proyecto correcto y con ArboledaPrototype abierta, mientras el cristal seguía visible entre las columnas. El archivo guardado de esa misma escena ya carecía de `Faint Crystal` y `Crystal Glow` y coincidía con la copia validada del ajuste anterior. Hay una diferencia entre el estado cargado en esa sesión y el asset guardado; no se identificó un segundo elemento decorativo guardado para eliminar.
- La captura posterior enviada por el desarrollador confirmó los nombres en Hierarchy: `Crystal Glow` seleccionado y `Faint Crystal` inmediatamente debajo, bajo `Provisional Decoration/Ancient Ruin - Decoration Only`. Corresponden a los dos objetos ya retirados del asset, no a la llama oficial ni a un tercer objeto. La nueva revisión del archivo y del constructor confirmó que ambos nombres siguen ausentes; no se realizó otra eliminación especulativa.
- Se revisaron todos los scripts y prefabs del proyecto y las instancias visuales. El constructor manual `ArboledaPrototypeSceneSetup.Generate` aún contenía dos llamadas que creaban el cristal/brillo originales. Ese método solo se invoca desde su menú, rechaza una escena destino existente y no se llama automáticamente al abrir la escena ni durante Play. **No se encontró regeneración automática.** Se retiraron exclusivamente esas dos llamadas para que tampoco una futura creación manual vuelva a incluir el elemento retirado; se conserva el resto del constructor y sus protecciones.
- No se volvió a modificar la escena ni se eliminaron objetos adicionales. La ruina mantiene siete objetos, sus plataformas y vegetación; la única llama real continúa en `(117, 2.4)` y el enemigo en su arena. Archivos de producto modificados en esta revisión: `ArboledaPrototypeSceneSetup.cs` y este registro. No se cerró ni reinició la sesión activa del desarrollador.
- **Persistencia nativa verificada en copia aislada:** Unity 6000.5.5f1 guardó la escena, la cerró abriendo una escena vacía, volvió a abrir el asset, entró en Play y comprobó durante tres segundos que no aparecieran los objetos retirados, salió de Play, guardó y repitió cierre/reapertura. `Logs/Task13PersistentReopen.log`: `PERSISTENCE_PASS`, salida 0. Los inventarios completos de SpriteRenderer, posición, componentes, jerarquía y prefab están en `Logs/TurquoiseInventory-EDIT.json` y `Logs/TurquoiseInventory-PLAY.json`; los únicos sprites turquesa activos pertenecen a `Checkpoint/Memory Flame - Arboleda/{Floating Flame, Flame Glow}`, en `(117, 3.05)`, no hay decoración turquesa anterior.
- **Checkpoint:** `Logs/Task13PersistentCheckpoint.log`: `DIAG01_PLAYMODE_PASS`, salida 0; E, botón norte, curación completa, estado visual activo y muerte/R con destino seguro, cámara y movimiento restaurados. Las posiciones de interacción se prepararon como fixtures; no se presentan como pruebas humanas.
- **Rutas y presentación tras reabrir:** `Logs/Task13PersistentRoutes.log`: `TASK13_PLAYMODE_PASS`, salida 0, con las tres rutas, recuperaciones y encuentro de enemigo/cámara aprobados. `Logs/Task13PersistentRender.log`: `TURQUOISE_RENDER_PASS`, salida 0; se revisaron capturas de un Game View recién abierto sin cristal (`Logs/PersistentPreview-Ruin.png`) y con el altar real conservado (`Logs/PersistentPreview-Arena.png`). No hubo errores/avisos C# ni excepciones en las ejecuciones finales. Los 635 objetos/componentes serializados de la escena coinciden en datos antes y después del guardado nativo, salvo espacios de formato; no se alteraron ruina, plataformas, cámara, enemigo ni checkpoint. En el proyecto principal solo cambió el constructor manual además de este registro: los otros 200 archivos protegidos, incluida la escena guardada y MovementTest, conservaron sus hashes. `git diff --check` correcto.
- **Recarga necesaria en el Editor abierto:** salir de Play, cambiar temporalmente a otra escena y abrir de nuevo `Assets/_Project/Scenes/Test/ArboledaPrototype.unity` desde Project antes de volver a jugar. No sobrescribir el asset corregido guardando la sesión antigua que todavía contiene el cristal; conservar aparte cualquier cambio manual pendiente antes de recargar. Si el objeto persiste después de esa recarga completa, se necesita su nombre/ruta exacta en Hierarchy, posición e imagen con el objeto seleccionado para distinguir el estado local; no se han eliminado elementos ambiguos.
- Siguen pendientes la comprobación humana tras recargar y la medición manual exacta de los **0.8 s** de i-frames. Sin commits, push ni cambios de rama.

## T14 — Exploración no lineal y rutas interconectadas

### Objetivo y sección modificada

- T13 fue aprobada manualmente por el desarrollador. T14 transforma el entorno del mirador en una red local de tres alternativas, en vez de extender el nivel. Anchura **150 m**, plataforma máxima **14.5 m**, límites/código de cámara y encuentro final conservados.
- Bifurcación principal en el extremo derecho de `Grove Upper Rest`, alrededor de `(63.6, 8.8)` —altura de los pies—. Desde allí se ven las entradas superior, central e inferior con la cámara existente. Las tres alternativas se reúnen en `Passage Low Rest`, ahora `x=83..92`, altura **6.8**, antes de las pasarelas y de la llama `(117, 2.4)`.

| Ruta | Recorrido y dificultad |
| --- | --- |
| Superior | Desde el descanso, salto hacia atrás/arriba a `Lookout 1`, en `x=60..63.25`, altura 11. Después, `Lookout 2` a 13, nuevo puente a 13.9 y mirador original a 14.5. Subidas de 2.2/2.0/0.9/0.6, con plataformas de al menos 3.25 m. Retorno por los tres escalones originales hasta la reconexión. |
| Central | Salto hacia delante a `Grove Crossing`, ahora una plataforma sólida elevada de `66.5..70.5`, altura 8; después apoyos `74..77` a 7 y `78.5..81.5` a 8.8. El hueco de 3.5 m coincide con un descenso de 1 m; el siguiente, de 1.5 m, sube 1.8. Descenso natural al descanso común. |
| Inferior | Bajar sin saltar del descanso alto al apoyo de `64.5..72`, altura 6; continuar a `72..80`, altura 4.6; subir al escalón amplio `80..83` a 5.9 y al descanso común a 6.8. Menos precisión horizontal, con apoyos amplios y cambios de altura. Tiene además una entrada temprana desde `Grove East`, bajando a `Grove Low Recovery` y saltando a la terraza de 6; permite evitar el ascenso al descanso alto. |

### Conexiones y recuperación

- Conexión intermedia en torno a `x=71`: desde el borde derecho de la primera plataforma central se puede bajar a la terraza inferior a 6; desde su extremo derecho, un salto normal hacia la izquierda vuelve a la plataforma central a 8. No requiere regresar al inicio de la bifurcación.
- Desde la central también se puede saltar de vuelta al descanso a 8.8 y elegir otra salida. La validación recorre este bucle completo. Las alternativas mantienen tramos separados hasta el descanso común; no obligan a atravesar el mirador.
- Se conservó el suelo continuo y se añadieron apoyos inferiores sólidos que también recogen fallos de la ruta central. La superior cuenta con recuperación a 12.2 entre el segundo apoyo y el puente y con el apoyo anterior recolocado a 12 bajo el último hueco; permiten volver a intentar el salto. Fallar la primera subida permite regresar al descanso. No hay daño ambiental, triggers nuevos, teletransporte ni mecánicas nuevas.
- Valores inspeccionados: velocidad 7.5, salto 13, gravedad 3, coyote/jump buffer 0.12, paso físico 0.02; no se alteraron. La física volvió a medir aproximadamente **2.74–2.75 m** de altura normal. La subida superior mayor deja unos 0.54 m de margen vertical y dispone de una superficie amplia de aterrizaje.

### Conservación, jerarquía y herramientas

- Se preservaron los **205 GameObjects anteriores y los 635 identificadores serializados**. La escena pasó a 690 objetos/componentes serializados. Solo cambiaron datos de **15 Transform** anteriores: superficies locales, sus cubiertas de musgo, el rótulo opcional del mirador y los enlaces de jerarquía correspondientes. Los demás registros anteriores permanecieron iguales salvo formato de serialización.
- `Grove Crossing` pasó de bloque macizo a plataforma elevada para abrir el paso inferior; conserva BoxCollider2D sólido, Default y su identidad. Se recolocaron dos apoyos superiores y una recuperación; se ajustó el borde izquierdo del descanso común. El mirador, ruina, plataformas iniciales, arena, checkpoint, enemigo y arco conservan su posición y función. Los cristales retirados siguen ausentes.
- Nueva agrupación `Level Geometry/T14 Interconnected Routes`, con `Central Route`, `Lower Route` y el grupo superior existente `Optional Lookout Route`. No se añadieron rótulos obligatorios, mapa, brújula ni marcadores de objetivos; se recolocó la indicación opcional del mirador para corresponder con su nueva entrada.
- Se adaptaron `ArboledaPrototypeSceneSetup.cs` y `ArboledaPrototypeValidation.cs`, sin scripts nuevos de producto. El menú puntual `Connect Arboleda Routes for T14 (once)` comprueba geometría/posiciones y escenas sin cambios pendientes antes de editar; rechaza repetición y detiene la operación si detecta configuración inesperada. No llama a Generate/Expand ni reconstruye la escena. Las protecciones anteriores y la retirada del cristal se conservan.
- No se modificaron scripts de ejecución, datos, Input Actions, prefabs, colisiones globales, salud, combate, habilidades, dificultad del enemigo, HUD, hitstop, GDD, Region1Plan ni MovementTest. El único enemigo sigue en `(127.5, 2.92)` y la única llama en `(117, 2.4)`, con el mismo destino `(116.4, 2.98)`.

### Validaciones ejecutadas

- Unity **6000.5.5f1** en copia aislada. `Logs/Task14Setup.log`: `TASK14_SETUP_PASS`, salida 0; las comprobaciones previas de superficies existentes pasaron sin detectar cambios manuales incompatibles.
- `Logs/Task14Validation.log`: `TASK14_PLAYMODE_PASS`, salida 0. Tres travesías independientes desde el spawn hasta el arco final, usando teclado sintético y cuerpo físico real, con dash/doble salto bloqueados solo temporalmente durante el test. No se teletransporta al personaje durante los recorridos. Se aisló la IA para las travesías y se reactivó después para comprobar el encuentro.
- `TASK14_CONNECTION_PASS` y `TASK14_LOOP_PASS`: conexión central→inferior→central y regreso al descanso→entrada inferior→central comprobados con controles reales. `TASK14_FORK_VIEW_PASS` confirma las tres entradas dentro del viewport tras caminar al punto de decisión; no exige que estén visibles desde el punto de aterrizaje izquierdo anterior.
- Se conservaron las pruebas de ruina, regreso, caída al apoyo inferior y pasarelas de T13; se adaptaron las recuperaciones superiores a la nueva geometría. Cada aterrizaje exige el collider/altura previstos y vida completa. Cámara observada durante saltos, caídas y cambios de ruta. Encuentro separado aprobado: patrulla, persecución, telegrafiado, ataque, curación, muerte/R seguro y muerte enemiga por ataques normales.
- `Logs/Task14Checkpoint.log`: `DIAG01_PLAYMODE_PASS`, salida 0; E y botón norte de mando, curación, checkpoint único, destello y brillo activo, muerte/R con vida, suelo y cámara correctos, movimiento posterior y ausencia de cristales. Se prepararon posiciones de interacción como fixtures, independientes de las travesías.
- Tiempos del bot con recuperaciones/bucles: central 46.8 s, inferior/ruina 43.1 s, superior/mirador 56.0 s. Son pruebas de física con trayectorias conocidas, no exploración humana ni medición de satisfacción o duración de una primera partida.
- `Logs/Task14Regression.log`: `TASK09_PLAYMODE_PASS`, `TASK10_FEEDBACK_PASS` y `TASK11_CHECKPOINT_PASS`, salida 0, ejecutados en MovementTest sin modificarla. Incluye caminar/dash sin empuje, recuperación de IA, pogo vivo/vulnerable/letal, entrenador, parry/postura, daño/muerte, hitstop y cambio entre checkpoints con curación/reaparición.
- `Logs/Task14Render.log`: `TASK14_RENDER_PASS`, salida 0. Se generaron capturas reales del Game View y se revisaron bifurcación sin textos (`Logs/Task14Preview-ForkNoText.png`), conexión inferior-central, centro, reconexión, subida/mirador, ruina sin cristal y arena conservada. Los fixtures de captura se posicionaron para observar la presentación; las pruebas de navegación anteriores usan entrada y física reales.
- Sin errores/avisos C# ni excepciones en las ejecuciones finales. La escena entregada y ambos scripts coinciden por hash con la copia validada; se retiraron únicamente espacios finales del YAML emitido por Unity. Los **198 archivos protegidos** conservaron sus hashes iniciales, incluidos MovementTest, scripts de ejecución, datos, prefabs, ajustes, GDD y Region1Plan. `git diff --check` pasó. Archivos de producto modificados: escena, ambos scripts de Editor indicados y este registro; sin nuevos archivos de producto.

### Pruebas manuales pendientes y límites

1. Abrir la escena guardada directamente, sin regenerarla. Subir por el tramo del bosque al descanso a 8.8 y caminar hasta su extremo derecho: identificar las tres alturas de salida sin depender de textos.
2. **Central:** saltar hacia la plataforma de enfrente a 8, continuar a la de 7 y a la de 8.8 y bajar al descanso de reconexión. Probar el salto horizontal más largo con Espacio sostenido y sin dash.
3. **Superior:** desde el descanso, saltar hacia la plataforma superior situada algo a la izquierda; continuar por los dos apoyos y el mirador y bajar por el retorno original. Fallar saltos y comprobar los apoyos de recuperación.
4. **Inferior:** bajar del borde del descanso sin saltar y seguir las terrazas inferiores hasta los escalones de salida. En otra pasada, desde `Grove East` tomar la recuperación baja hacia la derecha para entrar sin completar el ascenso alto.
5. **Cambio de ruta:** en la primera plataforma central caminar por su borde derecho y caer a la terraza de 6. Desde aproximadamente `x=71.4`, saltar hacia la izquierda para volver a la central. Probar también el regreso al descanso compartido y elegir otra alternativa.
6. Desde la reconexión completar las pasarelas, descansar con E/mando en la llama, comprobar curación, morir/R y combatir con el enemigo existente. Alcanzar el arco. Visitar la ruina y comprobar que no reaparecieron los cristales.
7. Revisar legibilidad de entradas, cambios de altura, aterrizajes fuera de los ejemplos del bot y otras relaciones de aspecto. Las pruebas automáticas prueban transitabilidad y visibilidad de apoyos, no que la elección resulte intuitiva para un jugador nuevo. Repetir `Fangbound/Validate Arboleda Prototype in Play Mode` para T14 y el menú de enemigo para la regresión de MovementTest.

- El nivel sigue siendo un prototipo; no se modificó el mapa definitivo ni se generó build standalone. Sigue pendiente la medición manual exacta de los **0.8 s** de i-frames de PlayerHealth. Sin commits, push ni cambios de rama.

## T15 — Exploración vertical y conexiones múltiples

### Diseño y conservación

- T01–T14 aprobadas manualmente por el desarrollador. Se aprovechó el volumen bajo las pasarelas, aproximadamente **x=92..108**, para crear una hondonada compacta de raíces. No se amplió la anchura de 150 unidades ni fue necesario excavar bajo el suelo existente: la nueva zona está entre el suelo a **0** y la terraza a **6.8**, por debajo del recorrido conocido.
- Se conservan completas las tres rutas y la conexión intermedia de T14. Desde su descanso común se puede seguir por las pasarelas anteriores o bajar a la nueva zona y volver a aparecer en `Passage 2`, con la ruina y el mirador todavía accesibles por sus rutas anteriores. Inicio, llama `(117, 2.4)`, enemigo `(127.5, 2.92)`, arena y arco no se desplazaron. No reaparecen `Faint Crystal` ni `Crystal Glow`.
- **Acceso occidental, x=96..98:** salir por la derecha de `Passage 1`, bajar al apoyo de **4.8**, girar a la izquierda al de **2.8** y después a la derecha hasta la base de **1.0**. La abertura queda entre los tramos del techo; también se puede entrar desde la terraza de recuperación a 6.8.
- **Tramo inferior:** suelo seguro continuo y piedra central de **0.9**, con superficies amplias, cambios de altura y un cambio de dirección en el acceso occidental. Es una zona transitable propia, no un hueco sin salida ni una segunda colección de tres rutas independientes.
- **Acceso oriental, x=103..106:** piedra central → apoyo a **2.8** → apoyo a **4.8** → terraza oriental a **6.8**. Desde allí se salta hacia la izquierda al `Passage Catch` existente, a **8.7**, y se vuelve a `Passage 2`, a **10.4**, continuando por el recorrido conocido. La misma secuencia permite bajar por el este y volver a subir por el oeste.
- La mayor subida nueva es **2.0**, frente a los aproximadamente **2.74** del salto normal. No se requieren dash, doble salto, pogo, parry ni coyote deliberado. Los apoyos inferiores y el suelo permiten recuperarse sin daño ni teletransporte. Algunos saltos bajo techos tienen menor altura efectiva; se comprueba que aun así alcanzan su destino con entrada normal.
- Identidad provisional: piedra/musgo en tonos tierra apagados, fondo cálido oscuro y raíces colgantes sin collider. Sin cristales, luces turquesas, textos obligatorios, enemigos, peligros, interacciones, habilidades ni arte definitivo nuevos.

### Cambios locales y herramientas

- `Passage Recovery Terrace` era un bloque macizo de 16 × 6.8 que ocupaba todo el espacio inferior. Se conserva su objeto/collider como tramo occidental de techo de 4 × 0.3 y se añaden tramos central y oriental, dejando dos aberturas. `Passage 1` también era macizo: se conserva exactamente su superficie superior y se reduce el volumen inferior para liberar la escalera occidental. Ambos mantienen collider sólido, sin triggers.
- `Passage Catch` conserva su altura y área anterior de aterrizaje; se ensancha 0.9 hacia la derecha para servir de conexión con la salida oriental. Las demás plataformas aprobadas, incluido el suelo, permanecen sin cambios.
- Nueva jerarquía `Level Geometry/T15 Root Underpass`, con techo/aberturas y ambos ascensos; decoración en `Provisional Decoration/T15 Root Hollow Decoration`. Se preservan los **222 GameObjects y 690 identificadores serializados** previos. La escena pasa a **771 registros**: solo cambian ocho Transform anteriores y los dos colores del tramo de techo reutilizado; no cambian datos de componentes funcionales originales.
- `ArboledaPrototypeSceneSetup.AddUnderpass`, menú `Fangbound/Add Arboleda Underpass for T15 (once)`, valida T14, posiciones y dimensiones antes de editar, rechaza repetición y escenas con cambios pendientes. No reconstruye ni ejecuta configuradores previos. La escena guardada no depende del helper para jugar.
- `ArboledaPrototypeValidation` conserva los tres recorridos T14 y añade un circuito continuo oeste→inferior→este→pasarelas→este→inferior→oeste, incluida caída al suelo y recuperación. Se amplía el descenso de pruebas para aceptar dirección izquierda; no se modifican sistemas de ejecución.
- Archivos de producto de esta entrega: `ArboledaPrototype.unity`, ambos scripts de Editor anteriores y este registro. Movimiento, salto, gravedad, dash, doble salto, combate, colisiones sin empuje, cámara, HUD, hitstop, checkpoints, prefabs y datos compartidos se conservan. Tampoco cambian MovementTest, GDD o Region1Plan.

### Validación

- Unity **6000.5.5f1** en copia aislada: `Logs/Task15Setup.log`, `TASK15_SETUP_PASS`, salida 0. Compilación del cambio y guardado nativo aprobados; ningún identificador original eliminado.
- `Logs/Task15Validation.log`: `TASK15_PLAYMODE_PASS`, salida 0, conservando también los marcadores T12/T13/T14. `TASK15_FORWARD_PASS`, `TASK15_REVERSE_PASS` y `TASK15_FLOOR_RECOVERY_PASS` confirman el circuito completo en ambos sentidos y caída/recuperación desde el suelo. Se usa teclado sintético con física real y controles normales, sin teletransporte dentro del circuito ni cambios de gravedad o fuerza de salto.
- Los tres recorridos independientes desde el spawn hasta el arco pasaron: central con circuito T15 70.4 s, inferior/ruina 43.1 s, superior/mirador 56.1 s. Son tiempos del bot con trayectorias conocidas, no una medida de exploración humana. La IA se aísla durante navegación y se reactiva para comprobar el encuentro real, curación E, muerte/R, cámara y muerte enemiga. Las recuperaciones y conexiones de T14 siguen pasando.
- `Logs/Task15Checkpoint.log`: `DIAG01_PLAYMODE_PASS`, salida 0; E y botón norte de mando, curación 3→5 y 4→5, destello/brillo activo, rango conservado, muerte/R con destino seguro, movimiento/cámara/vida restaurados y ausencia de cristales. Se posicionan fixtures entre muestras de interacción; no se presentan como navegación continua.
- `Logs/Task15Regression.log`: `TASK09_PLAYMODE_PASS`, `TASK10_FEEDBACK_PASS`, `TASK11_CHECKPOINT_PASS`, salida 0. Batería existente de MovementTest sin modificarla: caminar/dash sin empuje, IA y recuperación, pogo vivo/vulnerable/letal y objetivos anteriores, entrenador, parry/postura, daño/muerte, feedback/hitstop y cambio de checkpoints.
- `Logs/Task15Render.log`: `TASK15_RENDER_PASS`, salida 0. Capturas reales del Game View; se revisaron `Logs/Task15Preview-WestEntrance.png`, `Logs/Task15Preview-LowerCenter.png` y `Logs/Task15Preview-EastRoof.png`: abertura occidental y primer apoyo visibles, identidad inferior diferenciada y plataformas del retorno oriental visibles. La captura posiciona fixtures y no sustituye las pruebas de física ni una evaluación humana de orientación.
- Sin errores/avisos C# ni excepciones en las cinco ejecuciones finales. La escena guardada se abrió en procesos independientes para navegación, interacción y capturas. Escena entregada y ambos scripts coinciden por hash con la copia validada; solo se retiraron espacios finales del YAML. Los **198 archivos protegidos** conservan sus hashes iniciales. `git diff --check` correcto; únicamente cambian los cuatro archivos de producto indicados. Auxiliares temporales y registros/capturas quedan fuera del producto, en `.utmp` y `Logs`.

### Comprobación de la continuación oficial de T15

- Recibida la continuación de los apartados 10–18, se contrastó con la implementación entregada, sin reiniciarla ni modificar de nuevo la escena. Escena y ambos scripts mantienen los mismos hashes que las ejecuciones anteriores; los 198 archivos protegidos siguen intactos. Esta revisión adicional solo actualiza este registro.
- `CameraFollow2D` conserva su comportamiento, tamaño ortográfico 5.5, zona muerta 1.2 × 0.8 y suavizado 0.12. Sus límites siguen centrados en `(75, 9)`, tamaño `150 × 26`: cubren `x=0..150`, `y=-4..22`, incluida la nueva zona inferior a partir de 0. No se necesita ampliación de límites. El runner comprueba al jugador dentro del viewport durante descensos, travesía, ascensos, cambios de ruta y después de R.
- Las aberturas y apoyos visibles, el cambio de tono y las raíces cubren la orientación provisional sin textos nuevos, mapa, minimapa, brújula ni objetos turquesas. No se establece lore nuevo. La facilidad de descubrimiento y reconocimiento por una persona sigue pendiente de prueba manual; no se deduce de los tiempos ni de los marcadores del bot.

| Validaciones oficiales | Evidencia vigente y alcance |
| --- | --- |
| 1 y 16: apertura y compilación | Cinco procesos de Unity completados con salida 0, sin errores/avisos C# ni excepciones; escena entregada idéntica a la probada. |
| 2: tres rutas de T14 | Tres travesías independientes hasta el arco; conexión intermedia y bucle anterior conservados. |
| 3–7: acceso, avance inferior, dos conexiones y regreso bidireccional | `TASK15_FORWARD_PASS` y `TASK15_REVERSE_PASS`: circuito continuo por ambos accesos usando dirección y salto normal, sin teletransporte dentro del circuito. |
| 8–9: bloqueos y caídas | Recuperaciones de T14 y `TASK15_FLOOR_RECOVERY_PASS`, con salud completa y salida alcanzable. Sin bloqueos en los recorridos y muestras probados; no es una comprobación exhaustiva de cada posición posible. |
| 10: cámara | Comprobación de viewport durante los recorridos y tras respawn, límites adecuados y parámetros/código conservados; capturas de entrada, interior y retorno revisadas. |
| 11: ruina y mirador | Incluidos en las travesías inferior/ruina y superior/mirador. Cristal y brillo ausentes; siete objetos de ruina conservados. |
| 12–14: llama, enemigo y arco | E/mando, curación, muerte/R; encuentro con IA real y llegada al arco desde las tres alternativas. Una sola llama y un solo enemigo; prefabs y datos intactos. |
| 15: MovementTest | `TASK09_PLAYMODE_PASS`, `TASK10_FEEDBACK_PASS` y `TASK11_CHECKPOINT_PASS`; asset y sistemas anteriores sin modificaciones. |

- Protecciones de editor conservadas: cambios pendientes y configuración inesperada detienen el helper antes de editar; T15 aplicada impide repetición. Se edita el prototipo existente. La copia bajo `.utmp` solo aísla las pruebas, no es una escena paralela de producto. Las restricciones finales se mantienen: sin cambios de movimiento/combate, enemigos/checkpoints nuevos, habilidades, interacciones, daño ambiental, cristales restaurados, arte definitivo, ampliación horizontal, commits, push o cambios de rama.

### Pruebas manuales pendientes

1. Abrir directamente `ArboledaPrototype.unity`, sin regenerarla. Recorrer las tres rutas de T14 y su conexión central/inferior, llegar al descanso común y continuar hasta `Passage 1`.
2. Bajar por la abertura occidental, reconocer el cambio de tono y las raíces, recorrer la zona inferior y subir por el este hasta `Passage Catch` y `Passage 2`, solo con dirección y salto normal.
3. Volver a bajar desde las pasarelas por el este y recorrer el circuito en sentido contrario. Subir por el oeste a la terraza a 6.8, caminar hacia el descanso común y volver al camino de `Passage 1`. Comprobar que se reconocen lugares ya visitados.
4. Fallar saltos de ambos ascensos, aterrizar en apoyos inferiores/suelo y volver a intentarlos. Revisar especialmente techos, bordes y la terraza oriental junto al bloque de descenso, sin dash ni doble salto.
5. Evaluar si ambas aberturas se descubren sin instrucciones, cómo sigue la cámara al bajar/subir y la legibilidad en la relación de aspecto habitual. Visitar ruina y mirador, probar la llama con E/mando, curación, muerte/R, combate y acceso al arco final.
6. Repetir `Fangbound/Validate Arboleda Prototype in Play Mode` y la validación de enemigo en MovementTest si se editan superficies después de esta entrega.

- Las pruebas sintéticas no sustituyen una evaluación humana de exploración, orientación y dificultad. No se generó build standalone. Sigue pendiente la medición manual exacta de los **0.8 s** de i-frames. Sin commits, push ni cambios de rama.

## T16 — Plano regional de Arboleda Verdescente

### Inspección y alcance

- Tarea de planificación. Se leyeron AGENTS, GDD, Region1Plan y el registro; se revisaron la escena guardada, herramientas T14/T15, capacidades del personaje y la captura general existente `Logs/Task15Preview-Overview.png`. El prototipo conserva sus alternativas dentro de un eje común de 150 unidades. Se mantiene como laboratorio técnico; no se construyó una región ni se crearon escenas.
- Git tenía cambios pendientes de T15 en la escena, ambos scripts de Editor y este registro. Se conservaron. Para T16 solo se modifican **Docs/Region1Plan.md, Docs/GDD.md y Docs/DevelopmentLog.md**, sin archivos nuevos. Se tomó una referencia de los otros 203 archivos rastreados para comprobar conservación, incluidos los archivos técnicos ya modificados antes de empezar.
- Capacidades inspeccionadas: movimiento 7.5, salto 13, gravedad 3, dash 3.2/18/0.18 y doble salto 12. La altura normal de aproximadamente 2.74 procede de las pruebas anteriores, no de una medición nueva. Las banderas existentes de PlayerAbilities no deciden dónde se adquieren las habilidades.

### Propuesta y contradicciones resueltas

- `Region1Plan.md` contiene siete áreas propuestas: A Claro del Despertar, B Claro de las Sendas, C Raíces Profundas, D Ruinas del Colmillo, E Copas del Vigía, F Gruta del Retorno y G Umbral de Savia. Reagrupa las 16 salas anteriores conservando todos sus nombres; siete áreas no sustituyen el alcance de 15–18 salas por siete salas. Área, sala y escena son unidades distintas.
- Plano con ocho enlaces base bidireccionales y dos circuitos independientes: **B–C–D–E–B** y **B–C–F–B**. Se superponen copas, claro y raíces/gruta en altura. Progresión sugerida A–B–C–F–G; también son válidas una alternativa superior y la ruta corta A–B–F–G. L7 F–B es un camino siempre abierto que se descubre, no un mecanismo nuevo ni una cerradura implícita.
- La red base usa movimiento y salto normal. P1 D–F por posible dash, P2 F–E por posible doble salto y P3 B–E como posible pasarela directa quedan como propuestas opcionales pendientes. Quitarlas no bloquea ninguna área base. No se decide lugar ni región de adquisición y no se implementan desbloqueos.
- El GDD previo asignaba dash/doble salto a región 1 y el plan a salas 6/11; T16 exige dejar su ubicación pendiente. Se marca la asignación anterior como referencia histórica y se corrige la afirmación categórica del apartado de movimiento. Se aclaran las referencias antiguas a 4–6 salas y un checkpoint, conservando el alcance vigente de Arboleda y dejando el número final de Llamas a pruebas de recorrido/retorno. Las tablas de historia, nombres regionales y reglas de combate se conservan.
- GDD sección 8.1 incorpora áreas interconectadas, exploración horizontal/vertical, circuitos, atajos, descubrimientos, progresión por habilidades, dificultad y checkpoints estratégicos. En Region1Plan se distinguen principios confirmados de topología, nombres, descansos y bloqueos propuestos. Memoria opcional, pistas del Velo, Guardián Raíz y salida a Abismo se mantienen sin lore nuevo.
- Descansos conceptuales central y previo al jefe, con justificación de retorno tras morir; una posible Llama inferior depende de pruebas. No se fijan conteos definitivos de enemigos o checkpoints ni se implementa ninguno. Se alternan navegación tranquila, plataformas, descubrimientos, encuentros y descanso.

### Comprobaciones y siguientes pasos

- Revisión programática del Mermaid: **7 áreas, 8 enlaces, 2 ciclos independientes**, conectividad completa desde A, tres recorridos iniciales válidos y ambos circuitos con conexiones bidireccionales. A–B–C–F–G sigue disponible al excluir las áreas opcionales D/E; P1–P3 no forman parte de la red base. Los 16 identificadores y nombres de salas originales aparecen exactamente una vez en la tabla de reagrupación.
- Historia principal, reglas de combate, sección de bosses y filas narrativas regionales conservadas mediante comparación de texto; los apartados anteriores de este registro se mantienen. Se comprueban los hashes de los **203 archivos protegidos** y `git diff --check`. Estas verificaciones documentales no prueban geometría, orientación humana, duración o transitabilidad de una región aún no construida. No se ejecutó Unity ni se atribuyen pruebas nuevas de compilación/física a T16.
- Próxima tarea pequeña propuesta, después de aprobar el plano: graybox de **tres salas de muestra**, una de B/C/F, cerrando el circuito inferior. Reutilizar jugador/cámara, probar ambos sentidos y caídas con salto normal y evaluar reconocimiento humano del retorno antes de completar áreas o añadir encuentros. La escena de muestra se autorizará y creará en esa tarea futura; ArboledaPrototype permanece intacta.
- Pendiente del desarrollador: aprobar o ajustar agrupación/topología y ruta corta, nombres, reparto de salas, adquisiciones y atajos, descansos, ritmo de encuentros y alcance de producción. La planificación no cambia fechas del cronograma ni resuelve carga/transiciones de escenas. Sigue pendiente la medición manual exacta de los **0.8 s** de i-frames. Sin commits, push ni cambios de rama.

## T17 — Salas independientes, transiciones y checkpoints entre escenas

### Auditoría al continuar la implementación

- Se conservaron todos los cambios anteriores de T15/T16. Al retomar T17 ya existían CheckpointLocation, RoomDefinition, RoomEntrance, RoomExit, RoomScene, RoomSession, SpawnSafety2D y un generador, junto con modificaciones iniciales de PlayerHealth, PlayerCheckpoint, PlayerController y MemoryFlame. El código compilaba y había generado tres escenas, datos y prefab en la copia aislada; esos assets aún no estaban entregados al proyecto principal ni validados en Play. No se reconstruyeron los sistemas ni la geometría ya creada.
- La auditoría y la validación de assets detectaron definiciones de sala y destinos nulos tras guardar/reabrir. Se corrigió el generador para resolver de nuevo los ScriptableObjects después de abrir cada escena, registrar overrides de prefab y comprobar referencias tras el guardado. Se repararon únicamente las referencias de las escenas existentes y los overrides de inicio/límites de C. El auxiliar puntual de reparación quedó en `.utmp`, fuera del producto.
- Se completó el uso de la posición/rotación registradas al activar la Llama; no se depende de una referencia a su instancia descargada. Se añadieron las validaciones de física/entrada, sesiones nuevas, regresiones y herramientas de prueba del Editor. Las escenas y assets validados se entregaron conservando GUID y referencias.

### Arquitectura y persistencia

- Tres escenas de carga **Single**, sin carga aditiva. Cada una contiene exactamente una instancia del nuevo prefab `RoomPlayerRig`: jugador con sus componentes existentes, CameraFollow2D, AudioListener, HUD y CombatHitstop. Solo el pequeño **RoomSession** permanece mediante DontDestroyOnLoad; no persisten ni se duplican jugadores, cámaras o HUD.
- RoomSession toma una copia de transferencia desde los componentes al cargar: vida actual y máxima configurada, banderas de dash/doble salto y CheckpointLocation. La vida y las habilidades siguen siendo responsabilidad de PlayerHealth/PlayerAbilities, y la selección del checkpoint sigue en PlayerCheckpoint. La copia se vuelve a tomar antes de cada carga; no hay inventario, guardado en disco ni otra lógica de daño/curación.
- RoomDefinition es un ScriptableObject por sala con ID, ruta de escena, entrada inicial y referencia a la sala de recuperación B. RoomEntrance identifica un punto y su orientación; RoomExit inicia una carga automática al entrar su trigger con un jugador vivo. El bloqueo IsLoading rechaza cargas simultáneas y una protección de 0.25 s tras aparecer complementa la separación física entre entrada y salida. Se validaron las seis entradas fuera de todos los triggers de retorno.
- PlayerHealth reutiliza su limpieza de respawn para la llegada: detiene velocidad/dash, ataques, pogo, parry y estados de daño/knockback; restaura simulación, gravedad y control. En viaje normal recibe la vida capturada, **sin TryHeal ni curación automática**. Solo muerte/R utiliza vida máxima. El valor máximo procede del mismo PlayerHealthData; se exige configuración coherente entre rigs.
- CheckpointLocation guarda escena, ID de Llama, posición/rotación y la definición de sala como asset; no guarda objetos de una escena descargada. PlayerCheckpoint restaura el registro y, cuando corresponde a la escena actual, localiza la nueva instancia de MemoryFlame por escena/ID y marca su apariencia activa sin curar. Fuera de B, CurrentCheckpoint es nulo y el registro estable sigue válido.
- La Llama única del circuito está en B, raíz `(6.6, 0)`, respawn `(6, 0.58)`, ID **B.memory-flame**. Reutiliza el prefab y datos oficiales sin modificarlos. E y norte del mando activan/curan mediante T11. La posición guardada se valida con la misma comprobación de suelo/despeje compartida por Llamas y entradas.
- Al morir en C/F, PlayerHealth ejecuta su muerte normal. R delega la carga al destino de PlayerCheckpoint; al entrar en B se restaura vida máxima, pose segura, control y cámara. Sin checkpoint se usa la entrada `start` de B, `(4, 0.58)`, conservando desbloqueos de la sesión. La instancia recreada de la Llama recupera su estado activo cuando existe checkpoint.
- Cada escena se puede iniciar directamente. B y F empiezan en `(4, 0.58)`; C en `(4, 8.58)`. Una partida iniciada desde C/F no hereda checkpoint ni desbloqueos de otro Play y recupera en B si muere sin activar una Llama. Los datos temporales se reinician en SubsystemRegistration; dos entradas en Play dentro del mismo proceso comprobaron el aislamiento con las opciones actuales del proyecto (Domain Reload desactivado y Scene Reload activo). Entrar explícitamente en una escena antigua termina el ámbito de RoomSession.

### Escenas, geometría y conexiones

Carpeta de escenas: `Assets/_Project/Scenes/Arboleda/TechnicalCircuit/`.

| Escena | Función y recorrido |
| --- | --- |
| Arboleda_B_ClaroSendas.unity | Claro central verde, inicio y Llama. Salida a C en el suelo occidental y a F sobre la terraza oriental a 4.6; apoyos intermedios a 1.5 y 3. |
| Arboleda_C_RaicesProfundas.unity | Raíces ocres, conexión B arriba y F abajo. Descenso/ascenso en zigzag por alturas 8, 6, 4 y 2, con suelo a 0. |
| Arboleda_F_GrutaRetorno.unity | Gruta gris/lila apagada, C por el suelo occidental y B por el extremo oriental elevado. Apoyos a 0.8, 2 y 3.8. |

Las salas tienen 24 unidades de anchura, suelo seguro continuo, plataformas sólidas y marcos de paso provisionales. No contienen enemigos, daño ambiental, nuevas interacciones ni objetos turquesas decorativos. Los ascensos nuevos no exceden 2 unidades; se recorrieron sin necesitar habilidades adicionales. CameraFollow2D conserva código y parámetros; B/F usan límites `x=0..24, y=-4..12`, C `x=0..24, y=-4..17`, con tamaño ortográfico 5.5 y sincronización al aparecer.

| Salida estable | Destino / entrada | Posición de entrada (centro del jugador) | Orientación |
| --- | --- | --- | --- |
| B-to-C | C / from-B | (4, 8.58) | Derecha |
| C-to-B | B / from-C | (4, 0.58) | Derecha |
| C-to-F | F / from-C | (4, 0.58) | Derecha |
| F-to-C | C / from-F | (20, 0.58) | Izquierda |
| F-to-B | B / from-F | (19.5, 5.18) | Izquierda |
| B-to-F | F / from-B | (20, 4.38) | Izquierda |

### Archivos de producto

- **Runtime nuevos:** `Scripts/Player/CheckpointLocation.cs`; `Scripts/World/RoomDefinition.cs`, `RoomEntrance.cs`, `RoomExit.cs`, `RoomScene.cs`, `RoomSession.cs`, `SpawnSafety2D.cs`, todos bajo `Assets/_Project/`.
- **Editor nuevos:** `Scripts/Editor/RoomCircuitSceneSetup.cs` y `RoomCircuitValidation.cs`. El configurador rechaza assets existentes y escenas con cambios pendientes; no se ejecuta para jugar las escenas guardadas. El validador incluye fixtures del Editor para daño, muerte, desbloqueos temporales y borrado del checkpoint, sin añadir controles de pruebas al juego compilado.
- **Assets nuevos:** las tres escenas anteriores; `Assets/_Project/Prefabs/Player/RoomPlayerRig.prefab`; tres RoomDefinition en `Assets/_Project/Data/World/Rooms/`, con los mismos nombres que las escenas. Se incluyen los `.meta` de assets, scripts y cuatro carpetas nuevas para conservar sus GUID.
- **PlayerHealth.cs:** delegación opcional de R a la sesión y reutilización de la limpieza existente con vida explícita al llegar. Las escenas antiguas conservan respawn local.
- **PlayerCheckpoint.cs:** registro estable, restauración de instancia/visual y pose registrada, además de su comportamiento local anterior.
- **PlayerController.cs:** se expone la orientación existente para las entradas; se reutiliza desde el movimiento sin modificar aceleración, velocidad, salto, gravedad ni dash.
- **MemoryFlame.cs:** ID estable, búsqueda por escena/ID y extracción de su comprobación de aparición a SpawnSafety2D, manteniendo sus parámetros oficiales.
- **ProjectSettings/EditorBuildSettings.asset:** únicamente se añaden las tres escenas habilitadas; escena previa y configuración de Input System conservadas. **Docs/DevelopmentLog.md:** este registro. GDD y Region1Plan no recibieron cambios T17.

### Validaciones ejecutadas

- Unity **6000.5.5f1**, copia aislada. `Logs/Task17Validation.log`: **TASK17_PLAYMODE_PASS**. Tres escenas reabiertas, dependencias/prefab y seis enlaces comprobados; todas las entradas seguras y fuera de triggers. Un jugador, cámara, HUD, hitstop y sesión por sala; SceneManager.sceneCount igual a 1.
- `TASK17_FORWARD_PASS` y `TASK17_REVERSE_PASS`: circuitos **B→C→F→B** y **B→F→C→B** con entrada sintética y física real. Vida **3/5** conservada en el primer circuito, sin curación al cargar; orientación, desbloqueos, bloqueo de carga simultánea y reposo tras cada llegada sin bucles. Retorno con dash/doble salto bloqueados y recuperación de una caída al suelo de C. No se teletransporta al jugador durante esos circuitos.
- `TASK17_FLAME_INPUT_PASS`, `TASK17_DEATH_C_PASS`, `TASK17_DEATH_F_PASS`, `TASK17_NO_CHECKPOINT_PASS`: E/norte, curación, registro de escena/ID/posición, muertes por el contrato existente y R hacia B con vida máxima, control y cámara. Se comprobaron además color del renderer y alfa del brillo activo de la Llama recreada.
- `TASK17_ATTACK_TRANSFER_PASS` y `TASK17_KNOCKBACK_TRANSFER_PASS`: fixture separado con ataque/parry/hitstop y llamada a la API real de transición; hitstop restaurado y registro del checkpoint sin referencia a la Llama descargada. Otro fixture sitúa al actor junto al trigger C→F y lo cruza con knockback real; daño conservado y control recuperado. El circuito principal ya comprueba dash físico a través de B→C. Estos fixtures se distinguen de las travesías completas.
- `TASK17_NEW_SESSION_PASS`: salir de Play con daño, checkpoint y ambas habilidades activos, volver a entrar directamente en F y comprobar vida inicial, ausencia de checkpoint/desbloqueos heredados y recuperación en B. `Logs/Task17DirectC.log` y `Task17DirectF.log`: inicios independientes y muerte/R sin checkpoint aprobados, salida 0.
- `Logs/Task17Regression.log`: `TASK09_PLAYMODE_PASS`, `TASK10_FEEDBACK_PASS` y `TASK11_CHECKPOINT_PASS`, salida 0. MovementTest conserva caminar/dash sin empuje, IA/recuperación, pogo vivo/vulnerable/letal, entrenador, parry/postura, daño/muerte, hitstop, HUD y sus checkpoints.
- `Logs/Task17Arboleda.log`: `TASK12_PLAYMODE_PASS`, `TASK13_PLAYMODE_PASS`, `TASK14_PLAYMODE_PASS` y `TASK15_PLAYMODE_PASS`, salida 0. Las tres rutas, circuito inferior, recuperaciones, cámara, curación/respawn, combate y arco del prototipo anterior siguen pasando.
- `Logs/Task17Render.log`: `TASK17_RENDER_PASS`, salida 0. Capturas reales del Game View; se revisaron `Task17Preview-COverview.png`, `Task17Preview-FOverview.png` y `Task17Preview-BFlameActive.png` en Logs. Las vistas Overview cambian solo el encuadre del fixture para inspeccionar geometría completa; no cambian la cámara entregada ni sustituyen pruebas humanas.
- `Logs/Task17FinalCheck.log`: compilación final e inicio directo de C aprobados después de añadir los comandos de pruebas del Editor, con `TASK17_PLAYMODE_PASS`. Sin errores/avisos C# ni excepciones en las ejecuciones finales anteriores. Las primeras comprobaciones fallidas de referencias y una ruta de descenso del bot se corrigieron; el descenso de C usa gravedad natural por los bordes, sin saltar de vuelta a un apoyo superior.
- Entrega: 41 archivos de código/assets/configuración coinciden por hash con la copia validada; solo se normalizaron espacios finales del YAML. Los 199 archivos protegidos conservan sus hashes, incluidos escenas antiguas con cambios previos, datos y prefabs oficiales, Input Actions, cámara, HUD, herramientas anteriores y GDD. `git diff --check` correcto. Los auxiliares, manifiesto y registros/capturas permanecen en `.utmp`/Logs, fuera del producto.

### Pruebas manuales en Unity y límites

1. Abrir B directamente desde TechnicalCircuit, sin ejecutar configuradores, y entrar en Play. Caminar hasta la Llama y pulsar E; repetir con botón norte del mando. Revisar brillo, cinco indicadores de vida y control. El inicio está en `(4, 0.58)` y el respawn de Llama en `(6, 0.58)`.
2. **B→C→F→B:** en B tomar el paso occidental del suelo. En C bajar por los bordes alternando derecha/izquierda por 6, 4 y 2; caminar al paso oriental inferior. En F subir por sus apoyos hasta el paso oriental elevado. Al regresar a B, aparecer en la terraza oriental, mirando hacia dentro de la sala.
3. **B→F→C→B:** desde el suelo de B subir por 1.5, 3 y 4.6 hacia el paso oriental. En F bajar hacia el paso occidental. En C subir por 2, 4, 6 y 8 y salir por el paso occidental superior. Llegar al suelo occidental de B. Probar ambas rutas solo con dirección y salto normal.
4. Para probar vida sin añadir enemigos, usar en Play `Fangbound/Room Circuit/Damage 1 HP (Play Mode)`; esperar a que termine el parpadeo antes de repetir. Verificar que cambiar de sala no cura y descansar sí. `Unlock Dash and Double Jump (session)` permite probar sus estados entre cargas sin crear un desbloqueo de juego. Volver a enfocar Game View para usar controles después de un menú.
5. Activar la Llama, viajar a C y usar `Defeat Player (Play Mode)`, después R con Game View enfocado. Repetir desde F. Si la invulnerabilidad de un daño anterior impide la petición, esperar a que termine el parpadeo y repetir el comando. Comprobar muerte normal, B junto a la Llama, vida completa, apariencia activa, control y cámara.
6. Usar `Clear Checkpoint (session)`, viajar a C/F, morir y pulsar R: volver al inicio de B, sin activar automáticamente la Llama. Salir de Play y abrir directamente C y luego F en sesiones distintas; probar movimiento, cámara y recuperación B sin depender de una visita previa.
7. Fallar saltos, caminar bajo plataformas, regresar por las otras entradas y observar techos, alturas, orientación y encuadre. Los recorridos automáticos cubren muestras y circuitos completos, no todas las posiciones posibles ni la comprensión humana del espacio.
8. Repetir `Fangbound/Validate Three Room Circuit in Play Mode` para la batería completa; los validadores anteriores siguen disponibles para MovementTest y ArboledaPrototype. Las tres salas deben permanecer registradas en Build Settings; al mover/renombrar una escena, actualizar su RoomDefinition y validar de nuevo IDs/destinos.

- Progreso solo durante la sesión: salir de Play inicia una partida nueva; no hay guardado permanente. Es un circuito técnico de tres salas, no las áreas completas ni la región final. No se generó build standalone ni se atribuyen pruebas humanas a las entradas sintéticas. Sigue pendiente la medición manual exacta de los **0.8 s** de i-frames de PlayerHealth. Sin commits, push ni cambios de rama.

## T17 — Diagnóstico de habilidades y herramientas de pruebas

**Estado: pendiente de aprobación manual del desarrollador.** El usuario confirmó el funcionamiento manual de las transiciones; todavía faltan sus comprobaciones de habilidades, curación, vida entre salas y muerte/reaparición. Los resultados automatizados siguientes no sustituyen esa aprobación.

### Causas comprobadas

- `MovementTest` serializa `dashUnlocked: 1` y `doubleJumpUnlocked: 1`. El prefab `RoomPlayerRig` serializa ambas banderas a **0**, sin overrides de desbloqueo en B/C/F. El configurador las inicializa así intencionalmente para comprobar que el circuito es transitable con salto normal. No existe todavía una adquisición de habilidades implementada en estas salas; T16 deja su ubicación pendiente.
- Ambos jugadores referencian el mismo PlayerMovementData y el mismo asset de Input Actions. PlayerController impide el dash con su bandera bloqueada y solo reserva el salto aéreo cuando la bandera de doble salto está activa al despegar del suelo. No se encontró un defecto de referencias, controles o restauración de RoomSession que requiriera cambiar runtime.
- Iniciar B/C/F directamente crea una sesión con los valores del prefab. Llegar mediante una transición restaura los desbloqueos de la sesión; no vuelve a bloquearlos. El menú existente `Fangbound/Room Circuit/Unlock Dash and Double Jump (session)` habilita las dos banderas del jugador vivo de esa sala y se conservan al viajar. Salir de Play empieza otra sesión con los valores originales del prefab.
- Para probar doble salto se debe desbloquear antes del despegue, aterrizar si se habilitó en el aire, pulsar/sostener Espacio, soltarlo y pulsarlo otra vez durante el salto. Dash usa Shift y respeta cooldown, disponibilidad aérea, colisiones y bloqueo temporal de control por daño. Las pruebas requieren Game View enfocado y espacio libre sobre/delante del personaje.
- `Damage 1 HP` y `Defeat Player` ya delegaban en PlayerHealth.TryTakeDamage: daño, knockback, muerte e i-frames eran los oficiales. Una petición durante invulnerabilidad o con el jugador muerto se rechazaba sin informar al usuario. Se conserva ese comportamiento de combate y se mejora la respuesta de las herramientas del Editor.

### Cambios limitados a Editor y documentación

- **Assets/_Project/Scripts/Editor/RoomCircuitValidation.cs:** los dos comandos de daño comparten una pequeña función que informa la vida antes/después, avisa si la petición fue rechazada por invulnerabilidad o si ya se está muerto y recuerda enfocar Game View para R. El desbloqueo informa que es temporal y cómo probarlo; borrar checkpoint confirma el cambio. No se fuerza daño durante i-frames ni se curan actores para facilitar una prueba.
- Se amplió el validador existente: invoca los menús reales mediante ExecuteMenuItem, prueba físicamente Shift y pulsaciones separadas de Espacio con habilidades bloqueadas/desbloqueadas, verifica que no haya tercer salto y comprueba configuraciones mixtas y persistencia entre salas. Los fixtures de habilidades usan posiciones despejadas y restituyen pose, orientación y vida; se distinguen de los circuitos completos de navegación.
- Los escenarios de vida usan ahora los comandos reales de daño y muerte del Editor. Se comprueba expresamente que una petición de muerte inmediatamente después de daño sea rechazada por i-frames sin alterar el estado. No se añadieron enemigos, peligros, controles de pruebas al juego compilado ni nuevas mecánicas.
- **Docs/DevelopmentLog.md:** este diagnóstico y guía manual. Ningún otro archivo se modifica por este ajuste: se conservan las escenas B/C/F, RoomPlayerRig, RoomSession, controlador, salud/checkpoint, cámaras, HUD, Input Actions, datos, GDD, Region1Plan y prototipos antiguos, incluidos los cambios previos sin commit.

### Validaciones del diagnóstico

- Unity **6000.5.5f1**, copia aislada del estado actual, sin regenerar escenas. `Logs/Task17DiagnosisValidation.log`: `TASK17_PLAYMODE_PASS`, con `TASK17_ABILITY_INPUT_PASS` en B/C/F. Se comprobaron B bloqueada/desbloqueada; C con dash activo/doble bloqueado y después ambos activos; F con ambos activos heredados de C. Una segunda sesión iniciada directamente en F comprobó bloqueo inicial y funcionamiento después del menú.
- `TASK17_DEBUG_IFRAMES_PASS`: el menú Defeat rechazó el daño durante invulnerabilidad, conservando 4/5 y sin muerte. Su advertencia de Console es esperada. Dos usos válidos de Damage dejaron 3/5; `TASK17_FORWARD_PASS` confirmó que esa vida y los desbloqueos se conservan por B→C→F→B sin curación automática.
- `TASK17_FLAME_INPUT_PASS`: E y norte del mando curan, registran escena/ID/posición y restauran aspecto activo. `TASK17_DEATH_C_PASS`, `TASK17_DEATH_F_PASS` y `TASK17_NO_CHECKPOINT_PASS` usan Defeat y R reales: regreso a la Llama de B o al inicio seguro según el registro, 5/5, control y cámara restaurados. Se conservan las pruebas de estados temporales, carga simultánea rechazada y ausencia de duplicados/bucles al aparecer.
- `TASK17_REVERSE_PASS`: B→F→C→B con dash/doble salto bloqueados y recuperación de caída en C. `TASK17_NEW_SESSION_PASS` confirma que dos Play no comparten checkpoint/desbloqueos de la sesión anterior. Son entradas sintéticas con física real, no pruebas humanas.
- `Logs/Task17DiagnosisDirectC.log` y `Task17DiagnosisDirectF.log`: `TASK17_PLAYMODE_PASS`, salida 0. Ambas escenas se iniciaron directamente; habilidades bloqueadas/desbloqueadas comprobadas físicamente y recuperación en B sin checkpoint, conservando los desbloqueos aplicados por el menú.
- `Logs/Task17DiagnosisRegression.log`: `TASK09_PLAYMODE_PASS`, `TASK10_FEEDBACK_PASS`, `TASK11_CHECKPOINT_PASS`, salida 0; se conservan colisiones sin empuje, enemigo/entrenador, pogo, parry/postura, daño/muerte, hitstop, HUD y checkpoints de MovementTest.
- `Logs/Task17DiagnosisArboleda.log`: `TASK12_PLAYMODE_PASS`, `TASK13_PLAYMODE_PASS`, `TASK14_PLAYMODE_PASS`, `TASK15_PLAYMODE_PASS`, salida 0. Tres recorridos anteriores, conexiones/recuperaciones, cámara, combate, curación, muerte/R y arco conservados. Las ejecuciones finales no presentan errores ni avisos C# ni excepciones; la advertencia del menú al rechazar daño durante i-frames es intencional y está cubierta por el test.
- Los **236 archivos protegidos** conservan sus hashes iniciales, incluidos escenas, prefab del jugador, componentes de ejecución, datos y configuración. El validador entregado coincide por hash con la copia probada y `git diff --check` pasó. Esta corrección solo cambia el validador de Editor y este registro; no hay archivos nuevos de producto. T17 permanece pendiente de aprobación manual.

### Comprobaciones manuales pendientes del usuario

1. Abrir B y entrar en Play sin pausa. En la instancia Salamander de Hierarchy, PlayerAbilities empieza con ambas casillas desmarcadas. Ejecutar `Fangbound/Room Circuit/Unlock Dash and Double Jump (session)` y comprobar el mensaje de Console y ambas casillas activas.
2. Enfocar Game View y aterrizar. En el tramo despejado inicial de B, orientar con A/D y pulsar Shift. Para doble salto, pulsar Espacio, soltarlo y pulsarlo otra vez en el aire. Probar luego en C y F con espacio libre; al viajar, las casillas deben seguir activas. Al iniciar otro Play se espera que vuelvan a estar bloqueadas.
3. Ejecutar `Damage 1 HP (Play Mode)`: comprobar mensaje 5/5→4/5, HUD y knockback. Esperar a que termine el parpadeo, volver junto al altar de B y pulsar E: comprobar 5/5 y activación de la Llama. Repetir con norte del mando si se desea contrastar la entrada.
4. Recibir daño de nuevo con el menú y viajar a C/F sin descansar: comprobar que se mantiene la vida reducida. La proximidad a una Llama por sí sola no cura.
5. Con la Llama de B activada, en F ejecutar `Defeat Player (Play Mode)`. Si avisa de invulnerabilidad, esperar al fin del parpadeo y repetir. Enfocar Game View, pulsar R y comprobar aparición junto al altar de B en `(6, 0.58)`, 5/5, Llama activa y control/cámara normales. Repetir desde C.
6. Con `Clear Checkpoint (session)`, repetir muerte/R desde C/F: aparecer en el inicio de B `(4, 0.58)`, 5/5 y sin activar automáticamente la Llama. Se conservan los desbloqueos de esa sesión.
7. Repetir ambos circuitos y fallar algunos saltos; revisar orientación y HUD. El validador automático sigue disponible en `Fangbound/Validate Three Room Circuit in Play Mode`.

- El bloqueo inicial es intencional y permanece. No se encontró un fallo de habilidades que justificara alterar el prefab, progresión o parámetros del jugador. La validación humana sigue pendiente y T17 no se declara aprobada. Se mantiene pendiente la medición manual exacta de los **0.8 s** de i-frames. Sin commits, push ni cambios de rama.

## T18 — Primera sala técnica de Ruinas del Colmillo

**Estado:** implementada para evaluación; T18 pendiente de aprobación manual del desarrollador. El contexto recibido confirma que T17 ya pasó sus pruebas automatizadas y manuales; las anotaciones anteriores que la dejan pendiente describen su estado histórico.

### Inspección y alcance

Se revisaron AGENTS, GDD, Region1Plan, este registro, B/C/F, sus definiciones, `RoomPlayerRig`, `RoomSession`, `RoomScene`, `RoomEntrance`, `RoomExit`, aparición segura y validadores. Git partía limpio en `3cdb3b7`. Se reutiliza la carga Single y el snapshot de vida, desbloqueos y checkpoint de T17; no se modifica ningún componente de ejecución, prefab del jugador, parámetro de movimiento/combate o conexión aprobada. Los nombres y funciones de D concuerdan con T16; la memoria opcional, E y las otras salas conceptuales del área quedan fuera de esta muestra autorizada.

### Sala, recorrido y conexiones

- **D nueva:** `Assets/_Project/Scenes/Arboleda/TechnicalCircuit/Arboleda_D_RuinasColmillo.unity`, definición homónima en `Assets/_Project/Data/World/Rooms/`. Recuperación sin checkpoint en B; entrada directa y `from-C` en `(4, 0.58)`, mirando a la derecha. Anchura 22, suelo continuo a 0 y paredes laterales. Sin enemigos ni nuevas Llamas.
- **Ascenso principal:** suelo → `D Base Step` a 1.8 → `D West Landing` a 3.6 → `D Broken Crossing` a 5.4 → `D East Rest` a 7.2 → `D Inner Ledge` a 9 → `D Upper Chamber` a 10.8. Cambios de dirección y saltos normales de 1.8 de subida; descansos de 4–7.5 de anchura. Dash y doble salto siguen bloqueados inicialmente y no son necesarios.
- **Desvío:** desde el cruce a 5.4, mirador occidental `D West Lookout` a 7.2, con reconexión al apoyo interior a 9. Sin recompensa nueva. Un dintel sólido `D Entry Roof`, `x=0..1.9`, altura 2.8, recoge caídas por su lado izquierdo por encima del activador D→C y permite volver al apoyo de 3.6 con salto normal. Las otras caídas previstas aterrizan en apoyos inferiores o en el suelo seguro; se validó regresar desde el suelo al ascenso.
- **Regreso:** descenso por los apoyos alternos hasta el suelo y salida inferior izquierda `D-to-C`, trigger centrado en `(1.2, 1)`, hacia C/`from-D`. Aparece en C `(20.5, 6.58)`, mirando a la izquierda, sobre el nuevo apoyo oriental.
- **Rama aditiva de C:** grupo `Level Geometry/T18 Ruins Connection` con `C Ruins Approach`, `x=15..18`, y `C Ruins Gate`, `x=20..24`, ambos a altura 6. Se accede desde `C Terrace 6` con dos saltos normales. `C-to-D`, trigger `(22.8, 7)`, lleva a D/`from-C`. No se movieron los apoyos, entradas o salidas B/F anteriores.
- **Cámara y decoración:** mismo CameraFollow2D y tamaño ortográfico 5.5; únicamente D usa límites `x=0..22, y=-4..21`. Piedra gris verdosa, capiteles, columnas y dintel rotos, musgo y raíces mediante geometría provisional. La cámara superior tiene un hueco oriental visual para la futura conexión E, sin trigger, interacción, entrada, salida ni escena E. No hay decoración turquesa que simule una Llama.

### Archivos de producto

- **Nuevos:** escena D y RoomDefinition D, con sus `.meta`; `Assets/_Project/Scripts/Editor/RuinsRoomSceneSetup.cs` y `.meta`.
- **Modificados:** escena C; `Assets/_Project/Scripts/Editor/RoomCircuitValidation.cs`; `ProjectSettings/EditorBuildSettings.asset` (solo registro habilitado de D); `Docs/Region1Plan.md` y este documento. GDD conserva sus decisiones sin cambios.
- **Autoría protegida:** menú `Fangbound/Add Ruins Room D for T18 (once)`. Construcción aditiva para una C de T17, conservación de IDs originales, rechazo de Play, escenas sucias, D ya existente o rama ya aplicada; reabre C y D para comprobar referencias. Las escenas entregadas ya están guardadas: no ejecutar el configurador para jugar ni regenerar contenido aprobado.
- **Validador reutilizado:** nuevo menú `Fangbound/Validate Ruins D Circuit in Play Mode`, entrada pública `RunD` para inicio directo, comprobaciones de assets extendidas a las cuatro salas y fixtures de habilidades en una zona despejada de D. Se conservan el menú del circuito T17 y los comandos existentes de daño, muerte, desbloqueos y borrado de checkpoint. No se incorporan controles de pruebas al juego compilado.

### Validaciones automatizadas

Unity **6000.5.5f1**, copia aislada del proyecto; entrada sintética del Input System y física real. Las travesías completas utilizan movimiento/salto/caída, sin teletransportar al jugador. Las comprobaciones separadas de habilidades usan el fixture existente, restauran su pose y no se presentan como recorrido humano.

- `Logs/Task18Setup.log`: `TASK18_SETUP_PASS`, escena C guardada con sus **33 GameObjects originales** retenidos y D reabierta con referencias válidas. La comparación YAML no encuentra documentos originales eliminados: solo cambia la lista de hijos de Level Geometry y las adiciones de objetos al prefab local de C.
- `Logs/Task18Validation.log`: `TASK18_PLAYMODE_PASS`. C→D y D→C; circuito **B→C→D→C→F→B con 3/5**, checkpoint B conservado y ambas habilidades bloqueadas; ascenso hasta 10.8, mirador y reconexión; descenso, caída al suelo y recuperación; caída sobre dintel y regreso sin activar accidentalmente la salida. Durante los saltos y viajes se comprueba cámara, entradas seguras, orientación y un solo jugador/cámara/HUD/hitstop/sesión; se rechazan cargas simultáneas y se espera tras cada llegada para detectar bucles.
- En la misma validación: Llama B activada con E, curación, entrada a D con **4/5 y ambos desbloqueos**, Shift/doble salto físicos y rechazo de tercer salto; ida y vuelta adicional **D→C→D con 4/5 y desbloqueos conservados** (`TASK18_UNLOCKED_RETURN_PASS`); muerte en D por el contrato existente y R hacia la Llama B activa con **5/5**, desbloqueos conservados y control restaurado. Ejecución final: salida 0.
- `Logs/Task18DirectD.log`: `TASK17_DIRECT_START_PASS: D`, habilidades bloqueadas/desbloqueadas físicamente, muerte/R sin checkpoint → inicio seguro B con vida máxima y desbloqueos conservados; `TASK17_PLAYMODE_PASS`, salida 0.
- `Logs/Task18CircuitRegression.log`: `TASK17_PLAYMODE_PASS`, salida 0. Circuitos anteriores en ambos sentidos, vida, habilidades, Llama, muertes desde C/F, recuperación sin checkpoint, transferencias durante dash/ataque/parry/hitstop/knockback y nueva sesión sin estado heredado.
- `Logs/Task18MovementRegression.log`: `TASK09_PLAYMODE_PASS`, `TASK10_FEEDBACK_PASS`, `TASK11_CHECKPOINT_PASS`, salida 0. MovementTest conserva caminar/dash sin empuje, pogo vivo/vulnerable/letal y objetivos previos, entrenador, parry/postura, daño/muerte, hitstop, HUD, curación y respawn.
- `Logs/Task18ArboledaRegression.log`: `TASK12_PLAYMODE_PASS`, `TASK13_PLAYMODE_PASS`, `TASK14_PLAYMODE_PASS`, `TASK15_PLAYMODE_PASS`, salida 0. Se conservan rutas del prototipo, ruina/mirador, conexiones verticales y recuperación, cámara, combate, Llama y arco final.
- `Logs/Task18Render.log`: `TASK18_RENDER_PASS`, salida 0. Cinco capturas nativas del Game View inspeccionadas: rama C, vista general D, cámara superior, mirador y recuperación sobre el dintel. Archivos `Logs/Task18Preview-*.png`; las dos vistas generales usan una cámara de fixture más abierta, y las otras tres el seguimiento/tamaño aprobado. El fixture de captura solo existe en la copia aislada y no modifica escenas del producto. Se corrigió la orientación vertical de la lectura de RenderTexture para visualizar las capturas.
- Compilación Unity sin errores ni avisos C#, sin excepciones ni marcadores FAIL en los ocho logs T18. Los **237 archivos protegidos** conservan sus hashes iniciales, incluidos B/F, MovementTest, ArboledaPrototype, prefab, runtime, datos de movimiento/combate, GDD y herramientas anteriores ajenas al cambio. Los dos scripts de Editor entregados coinciden por hash con los compilados; escena/datos/metadatos/configuración coinciden con los probados salvo limpieza de espacios finales. Build Settings conserva exactamente la lista y configuración anteriores más D habilitada. `git diff --check` pasó; rama `main` y HEAD `3cdb3b7` conservados.

### Pruebas manuales pendientes

1. Abrir B y entrar en Play sin desbloqueos. Activar la Llama con **E** junto al altar. Usar `Fangbound/Room Circuit/Damage 1 HP (Play Mode)` dos veces, esperando al fin de la invulnerabilidad entre usos: debe quedar 3/5.
2. Salir por la izquierda hacia C; bajar desde el acceso superior a la terraza de altura 6. Saltar hacia la derecha por los dos apoyos nuevos y atravesar el marco oriental para llegar a D. Comprobar HUD 3/5, orientación y ausencia de transición de regreso automática.
3. Recorrer D con **A/D y Espacio**: primer apoyo derecho, izquierdo, cruce derecho, descanso derecho, apoyo interior izquierdo y cámara superior derecha. Explorar desde el cruce el mirador de la izquierda y volver por el apoyo interior. Revisar si el camino y los descansos se reconocen sin instrucciones textuales; no hay salida funcional E.
4. Probar caídas hacia apoyos/suelo y recuperación. Desde el lado izquierdo del mirador, comprobar aterrizaje sobre el dintel y salto al apoyo izquierdo de 3.6; bajar en zigzag para regresar a C por el marco inferior izquierdo. Completar C→F→B y confirmar que la vida reducida se conserva.
5. En otro recorrido, usar `Unlock Dash and Double Jump (session)`, enfocar Game View, aterrizar y probar **Shift** y dos pulsaciones separadas de **Espacio**. Viajar C↔D y revisar que las casillas y vida se conservan; el recorrido no exige esas habilidades.
6. Con la Llama B registrada, en D usar `Defeat Player (Play Mode)`, esperar/repetir si está invulnerable, enfocar Game View y pulsar **R**: regreso al altar B, 5/5, Llama activa y control/cámara normales. Abrir D en un Play nuevo, sin checkpoint, y repetir: regreso al inicio B `(4, 0.58)` sin activar la Llama automáticamente.
7. Revisar composición provisional, contraste y seguimiento de cámara al subir, caer, volver y reaparecer, además del circuito original y los dos prototipos. El validador de Ruins D puede ejecutarse desde Edit Mode con las escenas guardadas.

Las pruebas sintéticas no aprueban orientación, ritmo ni sensación de exploración: quedan a evaluación humana. Las caídas ensayadas cubren el recorrido previsto y dos recuperaciones concretas, sin afirmar cobertura exhaustiva de cualquier maniobra. Sigue pendiente la medición manual exacta de los **0.8 s de i-frames de PlayerHealth**. Sin commits, push ni cambios de rama.

## T19 — Primera sala técnica de Copas del Vigía

**Estado:** implementación para evaluación; T19 pendiente de aprobación manual. El encargo confirma T18 aprobada en sus seis pruebas manuales y con commit `ee8cac8`; las anotaciones históricas de T18 anteriores a esa aprobación se conservan. Git partía limpio en ese commit, rama `main`.

### Inspección e integración

Se revisaron AGENTS, GDD, Region1Plan y este registro, las salas y definiciones B/C/D/F, rig compartido, convenciones de capas/cámara, contratos de transición/sesión y configuradores/validadores T17–T18. No se reconstruyen salas ni se altera runtime. Se conserva carga Single, snapshot de vida/desbloqueos/checkpoint, orientación, guardia de llegada y recuperación en B. El prefab mantiene dash y doble salto bloqueados inicialmente; el recorrido base de E no requiere habilidades.

T19 implementa L4 D ↔ E desde el hueco oriental de la cámara superior de D, reservado en T18. L5 E ↔ B queda aplazada por instrucción explícita, aunque aparece como propuesta en el plano regional. No se modifica el GDD ni se fija adquisición de habilidades; tampoco se construyen las tres salas conceptuales completas de E.

### Geometría y recorrido

- **E:** `Assets/_Project/Scenes/Arboleda/TechnicalCircuit/Arboleda_E_CopasVigia.unity`, con RoomDefinition homónima en `Assets/_Project/Data/World/Rooms/`. Inicio y `from-D` en `(4, 0.58)`, mirando a la derecha; sala de recuperación B. Anchura 32, suelo seguro continuo a 0 y límites sólidos laterales.
- **Bifurcación:** `E Fork Bough`, `x=7..15.5`, altura 1.4, ofrece un descanso amplio y dos direcciones visibles. La ruta principal vuelve a la izquierda, sube hacia el centro y regresa otra vez al oeste antes de reconectar arriba.
- **Ruta principal:** `E Main West Bough` (`2.8..7.5`, 3.2) → `E Main Crossing` (`9..14`, 5.1) → `E Main Return` (`3..7`, 6.5) → `E Crown Junction` (`9.5..17`, 8.5) → `E Watch Approach` (`19..24`, 9.8) → `E Watch Lookout` (`25.5..32`, 11.4). Ascensos variables de 1.3–2 tras el primer salto, descansos y huecos distintos. El cruce central se desplazó una unidad hacia el oeste tras detectar en física real un salto que aterrizaba abajo; así se libera el despegue bajo la copa y se conserva la altura aprobada del salto.
- **Ruta secundaria:** desde la bifurcación, `E Side Sprig` (`18..22.5`, 2.5) → `E Side Outer Bough` (`25..30`, 4.5) → giro hacia `E Side Inner Bough` (`19..23.5`, 6.6) → copa central a 8.5 → mirador. Es una travesía hacia el borde oriental y un retorno hacia dentro, en contraste con el zigzag occidental. Su subida máxima es 2.1; no exige dash ni doble salto.
- **Circuito bajo y recuperación:** `E Lower Catch`, `x=22.8..26`, altura 0.9, recibe el descenso desde la rama exterior y permite subir a la rama de 2.5. Se puede bajar desde el mirador por los apoyos orientales, caer al suelo derecho y regresar por ese apoyo. La caída al oeste de la rama de 2.5 acaba en el suelo entre las rutas; desde ahí se salta a la bifurcación y se vuelve a la entrada. La prueba valida esa recuperación real, sin exigir que la gravedad atraviese horizontalmente el hueco hasta la bifurcación.
- **Mirador e identidad:** plataforma final de 6.5 de anchura, ramas y hojas por encima, troncos altos y siluetas de bosque inferior; geometría provisional marrón/verde, distinta de las ruinas grises. Decoración detrás del jugador y sin colliders. Plataformas y límites en Default, cubiertas verdes visibles; no se modifica la matriz física ni se crean objetos turquesas que parezcan Llamas.
- **Cámara:** CameraFollow2D y tamaño 5.5 sin cambios; límites de E `x=0..32, y=-4..23`. El extremo oriental del mirador es un candidato para estudiar después un descenso hacia B; ahora termina en el límite sólido, sin marco, trigger ni entrada que simule una conexión funcional.

### Transiciones y archivos

| Salida nueva | Destino / entrada | Centro del jugador al aparecer | Orientación |
| --- | --- | --- | --- |
| D-to-E, `(20, 11.8)`, suelo 10.8 | E / from-D | `(4, 0.58)` | Derecha |
| E-to-D, `(1.2, 1)`, suelo 0 | D / from-E | `(16.5, 11.38)` | Izquierda |

La escena D conserva su geometría, decoración y enlace C. Solo se añaden entrada `from-E`, salida `D-to-E` y su marco sobre el hueco reservado. La topología es B ↔ C ↔ F ↔ B, con C ↔ D ↔ E; no hay conexión E ↔ B.

- **Nuevos:** escena E y RoomDefinition E, ambas con `.meta`; `Assets/_Project/Scripts/Editor/CanopyRoomSceneSetup.cs` y `.meta`.
- **Modificados:** escena D, `Assets/_Project/Scripts/Editor/RoomCircuitValidation.cs`, `ProjectSettings/EditorBuildSettings.asset` (solo E habilitada), `Docs/Region1Plan.md` y este documento.
- **Configurador:** `Fangbound/Add Canopy Room E for T19 (once)`, aditivo y rechazado en Play, con escenas sucias, E existente, conexión aplicada o cámara superior D incompatible. Conserva IDs de los objetos originales y reabre D/E para validar referencias. Las escenas guardadas son la entrega; no regenerarlas para jugar.
- **Validación reutilizada:** `Fangbound/Validate Canopy E Circuit in Play Mode` y entrada pública `RunE`; se amplía la revisión de assets a cinco salas. Se conservan los recorridos anteriores y comandos de daño/muerte/desbloqueos. La aserción T18 sobre la única salida de D se adapta a su nuevo enlace E autorizado, manteniendo el enlace C. No hay controles de pruebas en runtime.

### Validaciones automatizadas de T19

Unity **6000.5.5f1**, copia aislada sincronizada con el commit aprobado de T18 y los cambios T19. Las travesías usan entrada sintética del Input System y física real, sin teletransportar al jugador. El fixture separado de habilidades reutiliza la restauración de pose existente en un tramo despejado de E (`x=16.5`); no se presenta como recorrido manual.

- `Logs/Task19Setup.log`: `TASK19_SETUP_PASS`, salida 0. Se guardan D/E y se reabren con referencias válidas; se conservan los **48 GameObjects originales de D**. Comparación YAML: ningún documento original eliminado o modificado salvo la lista de objetos añadidos a la instancia de RoomPlayerRig. Los colliders, plataformas y decoración originales permanecen idénticos.
- `Logs/Task19Validation.log`: `TASK19_PLAYMODE_PASS`, salida 0. **B→C→D→E→D→C→F→B con 3/5** y ambas habilidades bloqueadas; ascenso principal hasta 11.4, alternativa oriental y reconexión, pequeño circuito bajo, descensos y recuperación desde el suelo. Marcadores `TASK19_MAIN_ROUTE_PASS`, `LOWER_LOOP_PASS`, `SECONDARY_ROUTE_PASS`, `RECOVERY_PASS` y `CIRCUIT_PASS`.
- La misma ejecución comprueba Llama B activada con E, vida reducida sin curación al cargar, registro de checkpoint sin referencia a la Llama descargada, orientación y entradas seguras, ausencia de jugadores/cámaras/HUD/hitstop/sesiones duplicadas, una escena cargada y guardia contra cargas simultáneas/bucles. `TASK19_UNLOCKED_RETURN_PASS`: ida y vuelta **E→D→E con 4/5**, ambos desbloqueos conservados, dash/doble salto físicos y rechazo de tercer salto. `TASK19_CHECKPOINT_PASS`: muerte por el contrato existente en E, R hacia Llama B activa con **5/5**, desbloqueos conservados y control/cámara restaurados.
- `Logs/Task19DirectE.log`: `TASK17_DIRECT_START_PASS: E`, habilidades bloqueadas/desbloqueadas comprobadas con entrada, muerte/R sin checkpoint → inicio seguro B con vida máxima y desbloqueos conservados; `TASK17_PLAYMODE_PASS`, salida 0.
- `Logs/Task19RuinsRegression.log`: `TASK18_PLAYMODE_PASS`, salida 0. Conserva ascenso de D, mirador, dintel y recuperaciones, B→C→D→C→F→B a 3/5, D↔C con desbloqueos y muerte en D con retorno a la Llama B.
- `Logs/Task19CircuitRegression.log`: `TASK17_PLAYMODE_PASS`, salida 0. B→C→F→B y B→F→C→B, vida/desbloqueos, E/norte para la Llama, muertes desde C/F, recuperación sin checkpoint, transferencias durante dash/ataque/parry/hitstop/knockback y segundo Play sin estado heredado.
- `Logs/Task19MovementRegression.log`: `TASK09_PLAYMODE_PASS`, `TASK10_FEEDBACK_PASS`, `TASK11_CHECKPOINT_PASS`, salida 0. MovementTest conserva caminar/dash sin empuje, patrulla/persecución, ataques del enemigo y entrenador, parry/postura, pogo vivo/vulnerable/letal y objetivos anteriores, daño/muerte, hitstop, HUD, curación y respawn. `CombatFeedbackData.asset` permanece con hitstop habilitado y normal 0.10; no se modificó ninguna duración.
- `Logs/Task19ArboledaRegression.log`: `TASK12_PLAYMODE_PASS`, `TASK13_PLAYMODE_PASS`, `TASK14_PLAYMODE_PASS`, `TASK15_PLAYMODE_PASS`, salida 0. Se conservan los tres recorridos anteriores, ruina/mirador, circuito inferior, conexiones y recuperaciones, cámara, combate, curación/muerte/R y arco final.
- `Logs/Task19Render.log`: `TASK19_RENDER_PASS`, salida 0. Se inspeccionaron seis capturas nativas del Game View: nuevo paso superior D, conjunto de E, ruta principal, travesía exterior, mirador y apoyo bajo. `Logs/Task19Preview-*.png` corrige solo la orientación de lectura de RenderTexture. La vista general usa un fixture más abierto (tamaño 10); las otras cinco conservan CameraFollow2D/tamaño 5.5. El capturador solo existe en la copia aislada.
- Las primeras iteraciones detectaron un salto principal que terminaba en el apoyo inferior y una expectativa excesiva de alcance horizontal de caída en el test. Se corrigieron la posición del cruce y la secuencia de recuperación de la prueba, respectivamente, y se repitió T19 completa con éxito. Los logs `Task19Validation-InitialClearance.log` y `Task19Validation-InitialDescent.log` conservan esos fallos históricos; no son las ejecuciones finales.
- Compilación Unity sin errores ni avisos C#; las ejecuciones finales no presentan excepciones ni marcadores FAIL. Los **243 archivos protegidos** conservan sus hashes iniciales, incluidos B/C/F, MovementTest, ArboledaPrototype, RoomPlayerRig, scripts runtime, datos de movimiento/combate, Input System, matriz física y GDD. Los dos scripts de Editor entregados coinciden por hash con los compilados; escenas/datos/metadatos/configuración coinciden con la copia probada salvo eliminación de espacios finales. Build Settings contiene exactamente la configuración anterior más E habilitada. `git diff --check` y revisión de espacios de los seis archivos nuevos pasan. Rama `main` y HEAD `ee8cac8` conservados.

### Pruebas manuales pendientes

1. Salir de Play y abrir de nuevo las escenas guardadas para cargar la conexión D–E. Desde B activar la Llama con E, aplicar dos veces `Fangbound/Room Circuit/Damage 1 HP (Play Mode)` esperando el fin de la invulnerabilidad entre usos y comprobar 3/5.
2. Recorrer B→C→D como en T18; subir a la cámara superior y caminar hacia el hueco oriental `(20, 11.8)`. Aparecer en E cerca del extremo inferior izquierdo, a 3/5, mirando a la derecha y sin regreso automático.
3. Con A/D y Espacio, saltar a la bifurcación de altura 1.4; tomar la rama occidental a 3.2, volver al cruce 5.1, ir al descanso occidental 6.5 y subir a la copa central 8.5. Desde ahí avanzar a la derecha por 9.8 hasta el mirador 11.4. Evaluar márgenes, ritmo, lectura de plataformas y sensación de estar sobre el bosque.
4. Bajar de nuevo y probar la alternativa desde la bifurcación: rama baja oriental 2.5, rama exterior 4.5, giro hacia la interior 6.6, reconexión en 8.5 y mirador. Descender también desde la rama exterior al apoyo 0.9 y regresar a 2.5. Comparar interés y exigencia con la ruta principal.
5. Probar las caídas previstas: borde derecho de la rama exterior → suelo → apoyo 0.9 → rama 2.5; desde esta, bajar por la izquierda al suelo entre las rutas y saltar a la bifurcación. Volver a D por el marco inferior izquierdo; comprobar llegada en su cámara superior, orientación izquierda y ausencia de bucles. Completar D→C→F→B a 3/5.
6. Con `Unlock Dash and Double Jump (session)`, probar Shift y dos pulsaciones separadas de Espacio en espacio libre; viajar D↔E y revisar desbloqueos/vida. Con la Llama B activa, usar `Defeat Player (Play Mode)` en E y pulsar R: regreso a B con 5/5. En un Play nuevo iniciado directamente en E, repetir sin checkpoint: inicio seguro B sin activación automática de Llama.
7. Revisar composición, contraste, cámara al subir/caer/regresar y conservación de B/C/D/F, MovementTest y ArboledaPrototype. La diversión, orientación y dificultad siguen pendientes de evaluación humana; la prueba sintética no las aprueba.

No se añaden enemigos, Llamas, habilidades, adquisiciones, recompensas, cinemáticas ni guardado permanente. Se mantiene pendiente la medición manual exacta de los **0.8 s de i-frames de PlayerHealth**. T19 no se declara aprobada. Sin commits, push ni cambios de rama.

## T20 — Claro del Despertar y adquisición inicial del Colmillo

**Estado:** implementada para evaluación, pendiente de aprobación manual. El encargo confirma T01–T19 aprobadas y con commits; se partió de Git limpio en `9fb9650`, rama `main`. Las anotaciones anteriores de aprobación pendiente son históricas.

### Inspección y decisiones

Se revisaron AGENTS, GDD, Region1Plan, este registro, salas/definiciones B–F, RoomSession/RoomScene, RoomPlayerRig, controles, PlayerAbilities, PlayerCombat, PlayerPogo, PlayerParry, PlayerCheckpoint/MemoryFlame y herramientas de autoría/prueba. Interact ya contiene E y botón norte, con metadata Hold; las interacciones existentes usan WasPressedThisFrame. Se conserva ese contrato de pulsación, sin editar Input Actions. La nueva indicación omite la palabra Hold para describir correctamente la recogida.

Los ataques horizontal/ascendente y el descendente/pogo consumen Attack y aplican los contratos existentes de daño. Parry tiene su acción/ventana independiente. Por tanto es viable añadir una condición pequeña de posesión, sin desactivar componentes ni refactorizar movimiento: se bloquea el comienzo de ataques con el Colmillo mientras no se posea; se mantienen las mismas ventanas, hitboxes, daño, hitstop y rebote una vez recogido. Parry permanece disponible antes y después. PlayerController y PlayerHealth no reciben cambios.

El resumen del GDD decía que despertaba con la espada; el encargo confirma que despierta desarmada y la encuentra cerca. Se actualizan solo ese resumen y el punto del despertar en 3.3, sin revelar intenciones del mentor en el juego ni inventar por qué el arma está separada, nuevos diálogos o lore.

### Sala y conexiones

- **A nueva:** `Assets/_Project/Scenes/Arboleda/TechnicalCircuit/Arboleda_A_ClaroDespertar.unity`, con definición homónima en `Assets/_Project/Data/World/Rooms/`. Inicio `(4, 0.58)` mirando a la derecha, Colmillo en `(8.2, 0.55)`, salida oriental hacia B. Anchura 20, suelo seguro continuo a 0, raíz baja en `x=12..14.5` a 0.4; no hay plataformas complejas, enemigos, peligros ni Llamas.
- **Presentación provisional:** árboles altos, vegetación de fondo y hoja de hueso clara con guarda/empuñadura marrón, realizada con recursos geométricos existentes. El avatar sigue siendo el placeholder actual; no se produce arte final de capa o cicatriz. El objeto no usa turquesa ni pedestal de checkpoint. La indicación aparece solo cerca, en estado vivo/controlable; al recoger se oculta la presentación, sin destruir el componente ni deshabilitar Interact compartido.
- **Acceso B:** el oeste inferior existente ya lleva a C. Se añaden `B Awakening Approach`, `x=5..7.5`, altura 3.6, y `B Awakening Landing`, `x=0..3.2`, altura 5.3, sobre esa salida sin moverla. El retorno se realiza desde B Step 1 a 1.5 mediante salto normal; se puede bajar por los mismos apoyos. El corredor vertical de la fixture inicial B en x=4 queda libre.

| Conexión | Destino / entrada | Centro del jugador al aparecer | Orientación |
| --- | --- | --- | --- |
| A-to-B, `(18.8, 1)`, suelo 0 | B / from-A | `(2.65, 5.88)` | Derecha |
| B-to-A, `(1.2, 6.3)`, suelo 5.3 | A / from-B | `(16.5, 0.58)` | Izquierda |

Se conservan B↔C, C↔D, D↔E, C↔F y F↔B. No hay G ni enlace E↔B. CameraFollow2D conserva comportamiento/tamaño 5.5; solo A define sus límites locales `x=0..20, y=-4..12`. B conserva cámara y objetos originales.

### Posesión, sesiones y compatibilidad

- `PlayerAbilities.FangOwned` es la única bandera nueva de posesión; no hay inventario, registro estático adicional ni PlayerPrefs. Su valor predeterminado true mantiene las escenas antiguas. `RoomDefinition.StartWithoutFang` vale false por defecto; solo A lo activa y solo se aplica al comenzar una sesión, sin sobrescribir el progreso al regresar.
- RoomSession incorpora esa bandera al snapshot ya existente para viajes y muerte/R. La recuperación sin checkpoint de una sesión iniciada en A vuelve a A, conservando el estado armado/desarmado; al activar la Llama B se vuelve allí con 5/5 según las reglas aprobadas. Una sesión nueva no hereda arma, checkpoint ni desbloqueos de la anterior.
- Abrir B/C/D/E/F directamente sigue siendo un inicio técnico con Colmillo disponible, habilidades inicialmente bloqueadas y recuperación sin checkpoint en B. MovementTest y ArboledaPrototype no necesitan una recogida nueva. RoomPlayerRig y los datos aprobados no se modifican.
- A se inserta como primera escena habilitada de Build Settings, conservando en su orden todas las escenas/configuraciones anteriores. En el Editor se abre A para probar la apertura; no se añade un bootstrap ni se fuerza el inicio de Play desde otra escena. No se genera un ejecutable en T20.
- La recogida no cura, no desbloquea dash/doble salto y no condiciona salidas: se puede omitir, viajar desarmado y volver a recogerla. Dash/doble salto conservan sus reglas y herramientas de prueba. En A no existen blancos de pogo; el rebote requiere un objetivo válido como antes y se comprueba con fixtures o en MovementTest.

### Archivos

- **Nuevos, con `.meta`:** escena A, RoomDefinition A, `Assets/_Project/Scripts/World/FangPickup.cs`, `Assets/_Project/Scripts/Editor/AwakeningRoomSceneSetup.cs`.
- **Runtime modificado:** `PlayerAbilities.cs` (bandera), `PlayerCombat.cs` y `PlayerPogo.cs` (condición de posesión), `RoomDefinition.cs` (política inicial) y `RoomSession.cs` (transferencia de bandera). No cambian estadísticas, física, acción de parry ni estados internos de ataque/rebote al poseer el arma.
- **Otros modificados:** escena B, `RoomCircuitValidation.cs`, `ProjectSettings/EditorBuildSettings.asset`, `Docs/GDD.md`, `Docs/Region1Plan.md` y este documento.
- **Autoría:** `Fangbound/Add Awakening Room A for T20 (once)`, que rechaza Play, escenas sucias, A existente, conexión ya aplicada o B Step 1 incompatible; añade contenido y conserva IDs, sin regenerar B–F. No ejecutar para jugar las escenas guardadas.
- **Prueba:** `Fangbound/Validate Awakening A and Fang in Play Mode`. Se amplía la validación de assets a seis salas y se comprueba la posesión en cada viaje/muerte. Los menús T17–T19 mantienen sus entradas y parámetros por defecto. El salto de prueba al acceso de B frena antes del trigger para comprobar aterrizaje y cruce por separado.

### Validaciones T20

Unity **6000.5.5f1**, copia aislada sincronizada con T19. Entrada sintética y física real, diferenciadas de la evaluación humana. Las travesías completas no teletransportan al jugador; los fixtures separados de ataques se colocan y restauran explícitamente y no se guardan en la escena.

- `Logs/Task20Setup.log`: `TASK20_SETUP_PASS`, salida 0. A/B guardadas y reabiertas con referencias válidas; **38 GameObjects originales de B** conservados. Comparación YAML sin documentos originales eliminados: solo cambian la lista de hijos de Level Geometry y los objetos añadidos a la instancia de RoomPlayerRig. Colliders, plataformas, Llama, entradas y salidas originales permanecen iguales.
- `Logs/Task20Validation.log`: `TASK20_PLAYMODE_PASS`, salida 0. Inicio A seguro, muerte desarmado/R hacia A sin concesión de arma, A→B→A desarmado sin habilidades y sin adquisición automática; E fuera de rango y mantenida desde fuera no recogen, una pulsación nueva dentro de rango recoge una sola vez y oculta la presentación.
- `TASK20_FANG_ACTIONS_PASS` en ambos estados: ataques horizontal/ascendente y pogo contra TrainingTarget/PogoTarget temporales, daño/rebote real al poseer el arma y ausencia de daño/rebote/impulso descendente antes de recogerla; parry abre su ventana en ambos casos. No se añaden enemigos al nivel para esas comprobaciones.
- `TASK20_SESSION_PASS`: A↔B conserva arma y 4/5 sin curación; recogida repetida no cambia vida; E sigue activando/curando en Llama B después de recoger; daño y B→C, muerte/R hacia B con 5/5 y desbloqueos conservados; retorno normal al acceso A y presentación todavía oculta. En cada viaje se comprueban orientación, aparición, control, cámara, una sola instancia y ausencia de bucles/cargas simultáneas.
- `TASK20_NEW_SESSION_GAMEPAD_PASS`: segundo Play en A sin arma, checkpoint o desbloqueos heredados; botón norte recoge, muerte/R sin checkpoint conserva el Colmillo y vuelve a A sin regenerarlo. El primer log `Task20Validation-InitialLanding.log` conserva una expectativa de aterrizaje fallida porque el bot cruzaba ya el portal; la ejecución final separa ambos pasos y pasa.
- `Logs/Task20CircuitRegression.log`: `TASK17_PLAYMODE_PASS`, salida 0. Circuitos previos en ambos sentidos, vida/desbloqueos, Llama, muertes C/F, recuperación sin checkpoint, transferencias con estados temporales y nueva sesión sin estado heredado.
- `Logs/Task20RuinsRegression.log` y `Logs/Task20CanopyRegression.log`: `TASK18_PLAYMODE_PASS` y `TASK19_PLAYMODE_PASS`, salidas 0. Se conservan recorridos, miradores, alternativas y recuperaciones, conexiones C↔D↔E, vida reducida, desbloqueos y muertes con retorno a la Llama B.
- `Logs/Task20DirectC.log`, `Task20DirectD.log` y `Task20DirectE.log`: `TASK17_PLAYMODE_PASS`, salidas 0. Inicio técnico con Colmillo, habilidades bloqueadas/desbloqueadas físicamente y muerte/R sin checkpoint hacia B. Los inicios B y F también se comprueban dentro de la regresión T17, incluido el segundo Play.
- `Logs/Task20MovementRegression.log`: `TASK09_PLAYMODE_PASS`, `TASK10_FEEDBACK_PASS`, `TASK11_CHECKPOINT_PASS`, salida 0. Las escenas antiguas conservan ataques, pogo vivo/vulnerable/letal y objetivos anteriores, colisiones sin empuje, entrenador, parry/postura, daño/muerte, hitstop, HUD, Llama y respawn; no requieren recoger el Colmillo.
- `Logs/Task20ArboledaRegression.log`: `TASK12_PLAYMODE_PASS`, `TASK13_PLAYMODE_PASS`, `TASK14_PLAYMODE_PASS`, `TASK15_PLAYMODE_PASS`, salida 0. Se conservan recorridos del prototipo, ruina/mirador, circuito inferior, conexiones, recuperaciones, combate, curación/muerte/R, cámara y arco final.
- `Logs/Task20Render.log`: `TASK20_RENDER_PASS`, salida 0. Cinco capturas nativas inspeccionadas: inicio desarmado, arma e indicación cercana, objeto recogido, llegada a B y conjunto del acceso occidental. Archivos `Logs/Task20Preview-*.png`; se corrige únicamente la orientación de lectura de RenderTexture. Solo BOverview utiliza un fixture de cámara más abierto; las otras cuatro conservan el seguimiento/tamaño 5.5. El capturador es temporal y no se entrega dentro de Assets.
- La revisión visual detectó la palabra Hold heredada en la etiqueta; se omiten únicamente las descripciones de interacción al mostrar los bindings del Colmillo. También se conserva el último estado visual mientras la sala se descarga, en lugar de mostrar de nuevo el arma al quedar temporalmente sin jugador consultable. Tras esos cambios exclusivos del nuevo pickup se recompila y repite T20 completa y las capturas, ambas con salida 0; la indicación final muestra E/Y, sin exigir mantener pulsado.
- Compilación sin errores ni avisos C#; las once ejecuciones finales no presentan excepciones ni marcadores FAIL. Los **243 archivos protegidos** conservan sus hashes, incluidos C/D/E/F, MovementTest, ArboledaPrototype, RoomPlayerRig, PlayerController, PlayerHealth, PlayerParry, cámara, Input Actions, física y datos aprobados. Los ocho scripts entregados coinciden por hash con la copia compilada; escenas/datos/metadatos/configuración coinciden con los assets probados salvo limpieza de espacios finales. Build Settings solo añade A como primera escena y conserva exactamente el resto. `git diff --check` y revisión de espacios de los ocho archivos nuevos pasan. Rama `main` y HEAD `9fb9650` conservados.

### Coherencia regional y límites

Region1Plan distingue la red técnica real de una propuesta pendiente: A como inicio/primer objetivo, B como referencia/Llama, A–B–C–F–G como camino principal candidato, C–D–E como exploración opcional y F–B como regreso útil. E–B solo debe construirse si aporta un retorno legible y útil, no para completar un círculo por simetría; G sigue siendo el destino regional futuro. Se propone revisar planta, alturas y vistas comunes antes de dar por definitivas las conexiones. No se elimina ninguna ni se añaden objetivos/enemigos futuros como si estuvieran implementados.

La adquisición y el mapa son provisionales y T20 requiere pruebas manuales. Las representaciones son técnicas; no se evalúan como arte definitivo. La medición manual exacta de los **0.8 s de i-frames de PlayerHealth** sigue pendiente. Sin commits, push ni cambios de rama.

### Pruebas manuales pendientes

1. Abrir A y entrar en un Play nuevo. Comprobar 5/5, Colmillo visible cerca de `(8.2, 0.55)`, dash/doble salto bloqueados y Fang Owned desmarcado en PlayerAbilities. Caminar/saltar; J, arriba+J y abajo+J en el aire no deben atacar, mientras K conserva parry.
2. Acercarse al Colmillo y pulsar E: desaparece el objeto/indicación y Fang Owned queda marcado, sin curación. Probar ataques horizontal/ascendente; el descendente conserva gravedad natural y solo rebota sobre objetivos válidos. Para comprobar rebote en objetivos anteriores, abrir MovementTest por separado.
3. Aplicar `Fangbound/Room Circuit/Damage 1 HP (Play Mode)`, esperar al fin de la invulnerabilidad, saltar la raíz de 0.4 y salir por la derecha. En B debe aparecer en el acceso occidental alto con 4/5 y arma conservada. Volver por su marco izquierdo a A; el Colmillo no reaparece. Probar también omitir la recogida en un Play nuevo y hacer A↔B: se sigue desarmado.
4. Desde la llegada alta a B, bajar al apoyo 3.6, al escalón 1.5 y al suelo; junto a la Llama pulsar E para activar/curar. Viajar, usar Defeat Player y R: regresar al altar B con 5/5 y posesión conservada. Para volver a A, saltar desde el suelo al escalón 1.5, girar a la izquierda al apoyo 3.6 y al acceso 5.3; cruzar su marco occidental. La salida inferior C sigue separada.
5. En otro Play nuevo de A, recoger con el botón norte del mando. Usar Defeat Player antes de activar una Llama y pulsar R: volver al inicio A con 5/5 y objeto recogido. Repetir muerte sin haberlo recogido: debe continuar visible y el jugador desarmado.
6. Usar el menú Unlock Dash and Double Jump (session) y comprobar que salud, arma y desbloqueos persisten al viajar/morir. Salir de Play y comenzar A de nuevo: todo el progreso temporal se reinicia. Abrir B/C/D/E/F directamente conserva su combate técnico anterior.
7. Evaluar claridad del primer objetivo, indicación E/norte, tamaño/identidad del objeto, brevedad del recorrido y lectura del acceso a B. Repetir los circuitos anteriores y los prototipos. No ejecutar configuradores para regenerar contenido aprobado.

## T20 — Ajuste provisional de feedback del Colmillo

**Estado:** el usuario confirma ocho pruebas funcionales manuales superadas; T20 sigue pendiente de aprobar el feedback visual y no está autorizada para commit. Se conserva todo el trabajo pendiente de T20.

Se revisaron AGENTS, PlayerCombat, PlayerPogo, FangPickup y el feedback T10 existente. Se amplía **CombatVisualFeedback**, usando el mismo sprite provisional de su señal, sin crear un sistema de animación ni modificar escenas/prefabs. Cada jugador crea una sola raíz visual hija: dos arcos de cuatro segmentos, para permitir ataques normal/pogo simultáneos si las ventanas existentes coinciden, y un pulso independiente de recogida. Ninguno contiene colliders o rigidbodies, ni sobrevive al jugador o a la carga Single.

- Ataque horizontal: arco claro delante del personaje, según la dirección fijada por el ataque real. Ascendente: arco sobre el personaje. Pogo: arco dirigido hacia abajo, durante su ventana real. Antes de recoger no se ejecutan ni muestran los ataques bloqueados; K/parry conserva su comportamiento.
- PlayerCombat y PlayerPogo exponen consultas de presentación de solo lectura, con los mismos centros/tamaños usados para sus hitboxes. Se extrae ese cálculo común sin cambiar offsets, consultas, daño o temporizadores. El pogo que impacta y consume su ventana en el primer tick conserva únicamente una muestra visual de ese fotograma; no se prolonga la ventana física. Los efectos se ocultan al terminar/cancelarse, morir o desactivar el feedback.
- Una recogida válida llama a CombatVisualFeedback y produce un pulso dorado breve detrás del cuerpo. Es independiente de la señal de parry y no cambia el color del jugador. Repetir E, regresar a A o reaparecer no vuelve a emitirlo. La indicación y recogida E/norte siguen iguales; no hay equipo visible permanente.
- CombatFeedbackData recibe solo campos nuevos de color/grosor y color/duración/escala del pulso. La señal dura 0.24 s y su escala final es 1, reducida tras inspeccionar la primera captura. No cambia ningún parámetro anterior; hitstop sigue habilitado, normal **0.10**, pogo **0.04** y parry **0.07**.

**Archivos de este ajuste:** `Scripts/Combat/CombatVisualFeedback.cs`, `CombatFeedbackData.cs`; `Data/Combat/CombatFeedbackData.asset`; `Scripts/Player/PlayerCombat.cs`, `PlayerPogo.cs`; `Scripts/World/FangPickup.cs`; `Scripts/Editor/RoomCircuitValidation.cs`, todos bajo `Assets/_Project/`, y este documento. No se crean archivos de producto. Se conservan escenas, RoomPlayerRig, movimiento, salud, desbloqueos, arquitectura regional y los demás cambios previos de T20.

### Validaciones del ajuste

Unity **6000.5.5f1**, copia aislada. Las entradas sintéticas y fixtures se distinguen de las pruebas manuales del usuario.

- `Logs/Task20FeedbackValidation.log`: `TASK20_PLAYMODE_PASS`, salida 0. Antes/después de recoger se verifican ausencia/presencia y dirección de arcos a derecha, izquierda, arriba y abajo; cierre de ventana; daños reales y rebote existentes; pogo fallido con gravedad natural y sin hitstop; parry independiente. E y norte producen una única señal de recogida. Se conservan posesión, vida, desbloqueos, muerte/R y reinicio de sesión de T20.
- El validador comprueba una sola raíz con nueve SpriteRenderers, cero colliders/rigidbodies, sin efectos ni pulso heredados al entrar en cada sala. Los nuevos controles están integrados en los validadores de salas existentes.
- `Logs/Task20FeedbackCircuitRegression.log`: `TASK17_PLAYMODE_PASS`, salida 0. Circuito original en ambos sentidos, transiciones durante ataque/parry/hitstop/knockback, Llama, salud/desbloqueos, muerte y nueva sesión conservados.
- `Logs/Task20FeedbackCanopyRegression.log`: `TASK19_PLAYMODE_PASS`, salida 0. B→C→D→E→D→C→F→B a 3/5, ambas rutas de E, recuperaciones, desbloqueos y respawn; cada sala comprueba que no recibe señales anteriores ni raíces visuales duplicadas.
- `Logs/Task20FeedbackMovementRegression.log`: `TASK09_PLAYMODE_PASS`, `TASK10_FEEDBACK_PASS`, `TASK11_CHECKPOINT_PASS`, salida 0. Colisiones sin empuje, enemigo/entrenador, parry/postura, pogo y daño/muerte, feedback de impacto/hitstop y Llamas/HUD/respawn conservados en MovementTest.
- `Logs/Task20FeedbackArboledaRegression.log`: `TASK12_PLAYMODE_PASS`, `TASK13_PLAYMODE_PASS`, `TASK14_PLAYMODE_PASS`, `TASK15_PLAYMODE_PASS`, salida 0. Rutas, recuperaciones, cámara, combate, Llama/respawn y arco final de ArboledaPrototype conservados.
- `Logs/Task20FeedbackRender.log`: `TASK20_FEEDBACK_RENDER_PASS`, salida 0. Se inspeccionaron seis capturas nativas: J desarmado, recogida, horizontal derecha/izquierda, ascendente y pogo. `Logs/Task20FeedbackPreview-*.png` corrige solo la orientación de lectura de RenderTexture. El fixture pausa una ventana realmente aceptada para inspeccionar el fotograma, sin modificar escenas ni parámetros del producto; las pruebas funcionales anteriores corren con su física normal. Tras reducir únicamente la escala del pulso se recompiló y recapturó la presentación final. El capturador solo existe en la copia aislada.
- Compilación sin errores ni avisos C#, sin excepciones/FAIL en las seis ejecuciones finales. Los **254 archivos protegidos del estado previo al ajuste**, incluidos los cambios pendientes de T20 fuera de estos ocho archivos, conservan sus hashes. No cambian escenas, prefabs, GDD, Region1Plan, movimiento, salud, parry, posesión/desbloqueos ni los datos anteriores del feedback. Los seis scripts y el asset entregados coinciden por hash con la copia compilada/capturada; al quitar las cinco claves visuales nuevas del asset queda exactamente su contenido anterior, normalizando saltos de línea. `git diff --check` pasa; rama `main` y HEAD `9fb9650` conservados.

### Pruebas manuales del feedback pendientes

1. Abrir A en un Play nuevo. Probar J, arriba+J y abajo+J en el aire antes de recoger: no debe aparecer ningún arco. K mantiene su parry.
2. Recoger con E: comprobar desaparición del arma y pulso dorado breve. Repetir en otro Play nuevo con norte del mando. No debe quedar un efecto permanente al acabar el pulso.
3. Después, pulsar J mirando a cada lado y arriba+J: comprobar un arco claro en la dirección correspondiente. Saltar y pulsar abajo+J: comprobar arco inferior, sin impulso nuevo hacia abajo. En A no hay blancos de pogo; para evaluar el impacto/rebote se mantienen los objetivos de MovementTest.
4. Cambiar de sala, morir/reaparecer y volver a A: ningún efecto debe quedarse activo, duplicarse o simular otra recogida. Revisar también ataque durante dash/knockback y el parry existente.
5. Probar los mismos ataques en MovementTest y ArboledaPrototype y evaluar legibilidad a velocidad normal. Las capturas de un fotograma no aprueban la duración percibida. El aspecto es provisional; no hay animación ni equipamiento definitivo.

T20 continúa pendiente de aprobación visual del usuario. Se conserva pendiente la medición manual exacta de los **0.8 s de i-frames**. Sin commits, push ni cambios de rama.
